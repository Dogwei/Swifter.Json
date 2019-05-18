using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Data;

namespace Swifter.RW
{
    /// <summary>
    /// System.Data.DataSet Reader impl.
    /// </summary>
    internal sealed class DataSetRW<T> : IDataRW<int>, IInitialize<T>where T:DataSet
    {
        IValueRW IDataRW<int>.this[int key] => new ValueCopyer<int>(this, key);

        IValueReader IDataReader<int>.this[int key] => new ValueCopyer<int>(this, key);

        IValueWriter IDataWriter<int>.this[int key]=> throw new NotSupportedException();

        public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(Count);

        public int Count => Content.Tables.Count;

        public object ReferenceToken => Content;

        public T Content { get; private set; }

        public void Initialize(T obj)
        {
            Content = obj;
        }

        public void Initialize()
        {
            Initialize(1);
        }

        public void Initialize(int capacity)
        {
            if (Content == null)
            {
                if (typeof(T) == typeof(DataSet))
                {
                    Content = (T)new DataSet();
                }
                else
                {
                    Content = Activator.CreateInstance<T>();
                }
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            var length = Content.Tables.Count;

            for (int i = 0; i < length; i++)
            {
                OnReadValue(i, dataWriter[i]);
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            var valueInfo = new ValueFilterInfo<int>();

            for (int i = 0; i < Content.Tables.Count; i++)
            {
                OnReadValue(i, valueInfo.ValueCopyer);

                valueInfo.Key = i;
                valueInfo.Type = typeof(DataTable);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            ValueInterface<DataTable>.WriteValue(valueWriter, Content.Tables[key]);
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
            throw new NotSupportedException($"'{typeof(T)}' not supported set tables.");
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (key == Content.Tables.Count)
            {
                Content.Tables.Add(ValueInterface<DataTable>.ReadValue(valueReader));
            }
            else
            {
                throw new NotSupportedException($"'{typeof(T)}' not supported set tables.");
            }
        }
    }

    internal sealed class DataSetInterface<T> : IValueInterface<T> where T : DataSet
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }
            
            var dataReader = new DataSetRW<T>();

            valueReader.ReadArray(dataReader);

            return dataReader.Content;
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

            var dataReader = new DataSetRW<T>();

            dataReader.Initialize(value);

            valueWriter.WriteArray(dataReader);
        }
    }
}