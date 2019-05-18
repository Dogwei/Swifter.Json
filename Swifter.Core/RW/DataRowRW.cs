using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Data;

namespace Swifter.RW
{
    /// <summary>
    /// System.Data.DataRow Reader impl.
    /// </summary>
    internal sealed class DataRowRW<T> : IDataRW<string>, IInitialize<T>, IDirectContent where T : DataRow
    {
        public DataRowRW()
        {
        }

        IValueReader IDataReader<string>.this[string key] => this[key];

        IValueWriter IDataWriter<string>.this[string key] => this[key];

        public IValueRW this[string key] => new ValueCopyer<string>(this, key);

        public IEnumerable<string> Keys => ArrayHelper.CreateNamesIterator(Content.Table);

        public int Count => Content.Table.Columns.Count;

        public T Content { get; private set; }

        public object ReferenceToken => Content;

        object IDirectContent.DirectContent
        {
            get
            {
                return Content;
            }
            set
            {
                Content = (T)value;
            }
        }

        public void Initialize(T dataRow)
        {
            Content = dataRow;
        }

        public void Initialize()
        {
            if (Content == null)
            {
                if (typeof(T) == typeof(DataRow))
                {
                    var table = new DataTable();

                    Content = (T)table.NewRow();
                }
                else
                {
                    Content = Activator.CreateInstance<T>();
                }
            }
        }

        public void Initialize(int capacity)
        {
            Initialize();
        }

        public void OnReadAll(IDataWriter<string> dataWriter)
        {
            foreach (DataColumn item in Content.Table.Columns)
            {
                ValueInterface.GetInterface(item.DataType).Write(dataWriter[item.ColumnName], Content[item.Ordinal]);
            }
        }

        public void OnReadAll(IDataWriter<string> dataWriter, IValueFilter<string> valueFilter)
        {
            var valueInfo = new ValueFilterInfo<string>();

            foreach (DataColumn item in Content.Table.Columns)
            {
                ValueInterface.GetInterface(item.DataType).Write(valueInfo.ValueCopyer, Content[item.Ordinal]);

                valueInfo.Key = item.ColumnName;
                valueInfo.Type = item.DataType;

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        public void OnReadValue(string key, IValueWriter valueWriter)
        {
            var dataColumn = Content.Table.Columns[key];

            ValueInterface.GetInterface(dataColumn.DataType).Write(valueWriter, Content[dataColumn.Ordinal]);
        }

        public void OnWriteValue(string key, IValueReader valueReader)
        {
            var dataColumn = Content.Table.Columns[key];

            if (dataColumn == null)
            {
                var type = typeof(object);

                var value = valueReader.DirectRead();

                if (value != null)
                {
                    type = value.GetType();
                }

                dataColumn = Content.Table.Columns.Add(key, type);

                Content[dataColumn.Ordinal] = value;

                return;
            }

            Content[dataColumn.Ordinal] = ValueInterface.GetInterface(dataColumn.DataType).Read(valueReader);
        }

        public void OnWriteAll(IDataReader<string> dataReader)
        {
            foreach (DataColumn item in Content.Table.Columns)
            {
                Content[item.Ordinal] = ValueInterface.GetInterface(item.DataType).Read(dataReader[item.ColumnName]);
            }
        }
    }

    internal sealed class DataRowInterface<T> : IValueInterface<T> where T : DataRow
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }

            var dataWriter = new DataRowRW<T>();

            valueReader.ReadObject(dataWriter);

            return dataWriter.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            if (valueWriter is IValueWriter<T> tWriter)
            {
                tWriter.WriteValue(value);

                return;
            }

            var dataReader = new DataRowRW<T>();

            dataReader.Initialize(value);

            valueWriter.WriteObject(dataReader);
        }
    }
}