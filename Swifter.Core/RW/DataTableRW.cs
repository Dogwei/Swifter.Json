using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// System.Data.DataTable Reader impl.
    /// </summary>
    internal sealed class DataTableRW<T> : ITableRW, IInitialize<T>, IDirectContent where T : DataTable
    {
        private int readIndex;
        private int writeIndex;

        public DataTableRW()
        {
        }

        public ValueCopyer<string> this[string key] => new ValueCopyer<string>(this, key);

        public ValueCopyer<int> this[int key] => new ValueCopyer<int>(this, key);

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueReader IDataReader<string>.this[string key] => this[key];

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueWriter IDataWriter<string>.this[string key] => this[key];

        IValueRW IDataRW<int>.this[int key] => this[key];

        IValueRW IDataRW<string>.this[string key] => this[key];

        public IEnumerable<string> Keys => ArrayHelper.CreateNamesIterator(Content);

        public int Count => Content.Columns.Count;

        IEnumerable<int> IDataRW<int>.Keys => ArrayHelper.CreateLengthIterator(Count);

        IEnumerable<int> IDataReader<int>.Keys => ArrayHelper.CreateLengthIterator(Count);

        IEnumerable<int> IDataWriter<int>.Keys => ArrayHelper.CreateLengthIterator(Count);

        public T Content { get; private set; }

        public object ReferenceToken => null;

        object IDirectContent.DirectContent
        {
            get => Content;
            set => Initialize((T)value);
        }

        public void Initialize(T dataTable)
        {
            Content = dataTable;

            readIndex = -1;
            writeIndex = -1;
        }

        public void Initialize()
        {
            if (Content == null)
            {
                Initialize(Activator.CreateInstance<T>());
            }
        }

        public void Initialize(int capacity)
        {
            Initialize();
        }

        public void Next()
        {
            ++writeIndex;

            Content.Rows.Add(Content.NewRow());
        }

        public void OnReadAll(IDataWriter<string> dataWriter)
        {
            foreach (DataColumn item in Content.Columns)
            {
                ValueInterface.GetInterface(item.DataType).Write(dataWriter[item.ColumnName], Content.Rows[writeIndex][item.Ordinal]);
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            foreach (DataColumn item in Content.Columns)
            {
                ValueInterface.GetInterface(item.DataType).Write(dataWriter[item.Ordinal], Content.Rows[writeIndex][item.Ordinal]);
            }
        }

        public void OnReadAll(IDataWriter<string> dataWriter, IValueFilter<string> valueFilter)
        {
            var valueInfo = new ValueFilterInfo<string>();

            foreach (DataColumn item in Content.Columns)
            {
                ValueInterface.GetInterface(item.DataType).Write(valueInfo.ValueCopyer, Content.Rows[writeIndex][item.Ordinal]);

                valueInfo.Key = item.ColumnName;
                valueInfo.Type = item.DataType;

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            var valueInfo = new ValueFilterInfo<int>();

            foreach (DataColumn item in Content.Columns)
            {
                ValueInterface.GetInterface(item.DataType).Write(valueInfo.ValueCopyer, Content.Rows[writeIndex][item.Ordinal]);

                valueInfo.Key = item.Ordinal;
                valueInfo.Type = item.DataType;

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        public void OnReadValue(string key, IValueWriter valueWriter)
        {
            var dataColumn = Content.Columns[key];

            ValueInterface.GetInterface(dataColumn.DataType).Write(valueWriter, Content.Rows[readIndex][dataColumn.Ordinal]);
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            var dataColumn = Content.Columns[key];

            ValueInterface.GetInterface(dataColumn.DataType).Write(valueWriter, Content.Rows[readIndex][dataColumn.Ordinal]);
        }

        public void OnWriteValue(string key, IValueReader valueReader)
        {
            var dataColumn = Content.Columns[key];

            if (dataColumn == null)
            {
                var type = typeof(object);

                var value = valueReader.DirectRead();

                if (value != null)
                {
                    type = value.GetType();
                }

                dataColumn = Content.Columns.Add(key, type);

                Content.Rows[writeIndex][dataColumn.Ordinal] = value;

                return;
            }

            Content.Rows[writeIndex][dataColumn.Ordinal] = ValueInterface.GetInterface(dataColumn.DataType).Read(valueReader);
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            var dataColumn = Content.Columns[key];

            Content.Rows[writeIndex][dataColumn.Ordinal] = ValueInterface.GetInterface(dataColumn.DataType).Read(valueReader);
        }

        public bool Read()
        {
            ++readIndex;

            return readIndex < Content.Rows.Count;
        }

        public void OnWriteAll(IDataReader<string> dataReader)
        {
            foreach (DataColumn item in Content.Columns)
            {
                Content.Rows[writeIndex][item.Ordinal] = ValueInterface.GetInterface(item.DataType).Read(dataReader[item.ColumnName]);
            }
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
            foreach (DataColumn item in Content.Columns)
            {
                Content.Rows[writeIndex][item.Ordinal] = ValueInterface.GetInterface(item.DataType).Read(dataReader[item.Ordinal]);
            }
        }
    }

    internal sealed class DataTableInterface<T> : IValueInterface<T> where T : DataTable
    {
        public T ReadValue(IValueReader valueReader)
        {
            var dataWriter = new DataTableRW<T>();

            var toArrayWriter = new TableToArrayWriter(dataWriter);

            valueReader.ReadArray(toArrayWriter);

            return dataWriter.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            TableToArrayReader tableToArrayReader;

            if ((value.Rows.Count * value.Columns.Count) >= 100)
            {
                var overrideDbDataReader = new OverrideDbDataReader(value.CreateDataReader());

                tableToArrayReader = new TableToArrayReader(overrideDbDataReader);
            }
            else
            {
                var dataTableRW = new DataTableRW<T>();

                dataTableRW.Initialize(value);

                tableToArrayReader = new TableToArrayReader(dataTableRW);
            }

            valueWriter.WriteArray(tableToArrayReader);
        }
    }
}