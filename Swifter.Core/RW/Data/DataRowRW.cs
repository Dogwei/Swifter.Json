
using Swifter.Tools;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// System.Data.DataRow Reader impl.
    /// </summary>
    internal sealed class DataRowRW<T> : IDataRW<string>, IDataRW<int> where T : DataRow
    {
        const int DefaultCapacity = 3;

        internal T datarow;

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueReader IDataReader<string>.this[string key] => this[key];

        IValueWriter IDataWriter<string>.this[string key] => this[key];

        public IValueRW this[string key] => new ValueCopyer<string>(this, key);

        public IValueRW this[int key] => new ValueCopyer<int>(this, key);

        public IEnumerable<string> Keys => datarow.Table.Columns.Cast<DataColumn>().Select(column => column.ColumnName);

        IEnumerable<int> IDataRW<int>.Keys => null;

        IEnumerable<int> IDataReader<int>.Keys => null;

        IEnumerable<int> IDataWriter<int>.Keys => null;

        public int Count => datarow?.Table.Columns.Count ?? -1;

        public Type ContentType => typeof(T);

        public object Content
        {
            get => datarow;
            set => datarow = (T)value;
        }

        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        public void Initialize(int capacity)
        {
            if (datarow is null)
            {
                if (typeof(T) == typeof(DataRow))
                {
                    datarow = Underlying.As<T>(new DataTable().NewRow());
                }
                else
                {
                    datarow = Activator.CreateInstance<T>();
                }
            }
        }

        public void OnReadAll(IDataWriter<string> dataWriter)
        {
            var columns = datarow.Table.Columns;

            var length = columns.Count;

            for (int i = 0; i < length; i++)
            {
                var column = columns[i];

                WriteValue(datarow[column], dataWriter[column.ColumnName]);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void WriteValue(object value, IValueWriter valueWriter)
        {
            if (value == DBNull.Value)
            {
                value = null;
            }

            ValueInterface.WriteValue(valueWriter, value);
        }

        public void OnReadValue(string key, IValueWriter valueWriter)
        {
            WriteValue(datarow[key], valueWriter);
        }

        public void OnWriteValue(string key, IValueReader valueReader)
        {
            var column = datarow.Table.Columns[key];

            // 此方法允许列不存在时新增列。
            if (column is null)
            {
                column = datarow.Table.Columns.Add(key, typeof(object));
            }

            if (column.DataType == typeof(object))
            {
                datarow[column] = valueReader.DirectRead();
            }
            else
            {
                datarow[column] = ValueInterface.GetInterface(column.DataType).Read(valueReader);
            }
        }

        public void OnWriteAll(IDataReader<string> dataReader)
        {
            var columns = datarow.Table.Columns;

            var length = columns.Count;

            for (int i = 0; i < length; i++)
            {
                var column = columns[i];

                if (column.DataType == typeof(object))
                {
                    datarow[column] = dataReader[column.ColumnName].DirectRead();
                }
                else
                {
                    datarow[column] = ValueInterface.GetInterface(column.DataType).Read(dataReader[column.ColumnName]);
                }
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            WriteValue(datarow[key], valueWriter);
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            var columns = datarow.Table.Columns;

            var length = columns.Count;

            for (int i = 0; i < length; i++)
            {
                var column = columns[i];

                WriteValue(datarow[column], dataWriter[i]);
            }
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            var column = datarow.Table.Columns[key];

            if (column.DataType == typeof(object))
            {
                datarow[column] = valueReader.DirectRead();
            }
            else
            {
                datarow[column] = ValueInterface.GetInterface(column.DataType).Read(valueReader);
            }
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
            var columns = datarow.Table.Columns;

            var length = columns.Count;

            for (int i = 0; i < length; i++)
            {
                var column = columns[i];

                if (column.DataType == typeof(object))
                {
                    datarow[column] = dataReader[i].DirectRead();
                }
                else
                {
                    datarow[column] = ValueInterface.GetInterface(column.DataType).Read(dataReader[i]);
                }
            }
        }
    }
}