
using Swifter.Tools;

using System;
using System.Collections.Generic;
using System.Data;

namespace Swifter.RW
{
    /// <summary>
    /// System.Data.DataSet Reader impl.
    /// </summary>
    internal sealed class DataSetRW<T> : IDataRW<int> where T:DataSet
    {
        public IValueRW this[int key] => new ValueCopyer<int>(this, key);

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueWriter IDataWriter<int>.this[int key]=> this[key];

        public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(Count);

        public int Count => dataset.Tables.Count;

        public object Content
        {
            get => dataset;
            set => dataset = (T)value;
        }

        public Type ContentType => typeof(T);

        public T dataset;


        public void Initialize()
        {
            Initialize(1);
        }

        public void Initialize(int capacity)
        {
            if (typeof(T) == typeof(DataSet))
            {
                Underlying.As<T, DataSet>(ref dataset) = new DataSet();
            }
            else
            {
                dataset = Activator.CreateInstance<T>();
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            var length = dataset.Tables.Count;

            for (int i = 0; i < length; i++)
            {
                ValueInterface<DataTable>.WriteValue(dataWriter[i], dataset.Tables[i]);
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            ValueInterface<DataTable>.WriteValue(valueWriter, dataset.Tables[key]);
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
            var length = dataset.Tables.Count;

            dataset.Clear();

            for (int i = 0; i < length; i++)
            {
                dataset.Tables.Add(ValueInterface<DataTable>.ReadValue(dataReader[i]));
            }
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (key == Count)
            {
                dataset.Tables.Add(ValueInterface<DataTable>.ReadValue(valueReader));
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}