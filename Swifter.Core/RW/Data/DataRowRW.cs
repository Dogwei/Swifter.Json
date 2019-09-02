
using Swifter.Tools;

using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// System.Data.DataRow Reader impl.
    /// </summary>
    internal sealed class DataRowRW<T> : IDataRW<string>, IDataRW<int>, IInitialize<T>, IDirectContent where T : DataRow
    {
        internal readonly List<KeyValuePair<string, object>> Items;
        internal readonly bool SetTypeOfValues;

        internal T DataRow;

        bool Initialized;


        public DataRowRW(bool setTypeOfValues = false)
        {
            Items = new List<KeyValuePair<string, object>>();

            SetTypeOfValues = setTypeOfValues;
        }

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueReader IDataReader<string>.this[string key] => this[key];

        IValueWriter IDataWriter<string>.this[string key] => this[key];

        public IValueRW this[string key] => new ValueCopyer<string>(this, key);

        public IValueRW this[int key] => new ValueCopyer<int>(this, key);

        public IEnumerable<string> Keys => ArrayHelper.CreateNamesIterator(Content.Table);

        IEnumerable<int> IDataRW<int>.Keys => null;

        IEnumerable<int> IDataReader<int>.Keys => null;

        IEnumerable<int> IDataWriter<int>.Keys => null;

        public int Count => Content.Table.Columns.Count;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Fill()
        {
            if (!Initialized && DataRow == null)
            {
                return;
            }

            if (DataRow == null)
            {
                if (typeof(T) == typeof(DataRow))
                {
                    var dataTable = new DataTable();

                    foreach (var item in Items)
                    {
                        dataTable.Columns.Add(item.Key, SetTypeOfValues ? item.Value?.GetType() ?? typeof(object) : typeof(object));
                    }

                    DataRow = Unsafe.As<T>(dataTable.NewRow());

                    for (int i = 0; i < Items.Count; i++)
                    {
                        DataRow[i] = Items[i].Value;
                    }

                    return;
                }

                DataRow = Activator.CreateInstance<T>();
            }

            var columns = DataRow.Table.Columns;

            foreach (var item in Items)
            {
                var column = columns[item.Key];

                if (column == null)
                {
                    columns.Add(item.Key, SetTypeOfValues ? item.Value?.GetType() ?? typeof(object) : typeof(object));
                }

                DataRow[column] = item.Value;
            }
        }

        public void Clear()
        {
            Items.Clear();

            DataRow = null;

            Initialized = false;
        }

        public T Content
        {
            get
            {
                if (Items.Count != 0)
                {
                    Fill();
                }

                return DataRow;
            }
        }

        object IDataReader.ReferenceToken => Content;

        object IDirectContent.DirectContent
        {
            get
            {
                return Content;
            }
            set
            {
                Initialize((T)value);
            }
        }

        public void Initialize(T dataRow)
        {
            Items.Clear();

            DataRow = dataRow;

            Initialized = false;
        }

        public void Initialize()
        {
            Items.Clear();

            Initialized = true;
        }

        public void Initialize(int capacity)
        {
            Initialize();
        }

        public void OnReadAll(IDataWriter<string> dataWriter)
        {
            foreach (DataColumn item in Content.Table.Columns)
            {
                var value = Content[item.Ordinal];

                if (value == DBNull.Value)
                {
                    value = null;
                }

                ValueInterface.WriteValue(dataWriter[item.ColumnName], value);
            }
        }

        public void OnReadAll(IDataWriter<string> dataWriter, IValueFilter<string> valueFilter)
        {
            OnReadAll(new DataFilterWriter<string>(dataWriter, valueFilter));
        }

        public void OnReadValue(string key, IValueWriter valueWriter)
        {
            var value = Content[key];

            if (value == DBNull.Value)
            {
                value = null;
            }

            ValueInterface.WriteValue(valueWriter, value);
        }

        public void OnWriteValue(string key, IValueReader valueReader)
        {
            DataColumn column;

            if (DataRow != null && (column = DataRow.Table.Columns[key]) != null && column.DataType != typeof(object))
            {
                DataRow[column] = ValueInterface.GetInterface(column.DataType).Read(valueReader);

                return;
            }

            Items.Add(new KeyValuePair<string, object>(key, valueReader.DirectRead()));
        }

        public void OnWriteAll(IDataReader<string> dataReader)
        {
            foreach (DataColumn item in Content.Table.Columns)
            {
                Content[item] = ValueInterface.GetInterface(item.DataType).Read(dataReader[item.ColumnName]);
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            var value = Content[key];

            if (value == DBNull.Value)
            {
                value = null;
            }

            ValueInterface.WriteValue(valueWriter, value);
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            var length = Content.Table.Columns.Count;

            for (int i = 0; i < length; i++)
            {
                OnReadValue(i, dataWriter[i]);
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            OnReadAll(new DataFilterWriter<int>(dataWriter, valueFilter));
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            var type = Content.Table.Columns[key].DataType;

            Content[key] = type == typeof(object) ? valueReader.DirectRead() : ValueInterface.GetInterface(type).Read(valueReader);
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
            var length = Content.Table.Columns.Count;

            for (int i = 0; i < length; i++)
            {
                OnWriteValue(i, dataReader[i]);
            }
        }
    }
}