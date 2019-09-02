using Swifter.Tools;
using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class ListRW<T, TValue> : IDataRW<int>, IDirectContent, IInitialize<T> where T : IList<TValue>
    {
        public const int DefaultCapacity = 3;

        internal T content;

        public T Content => content;

        public ValueCopyer<int> this[int key] => new ValueCopyer<int>(this, key);

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(Count);

        public int Count => content.Count;

        object IDirectContent.DirectContent { get => content; set => content = (T)value; }

        public object ReferenceToken => content;

        IValueRW IDataRW<int>.this[int key] => this[key];

        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        public void Initialize(T content)
        {
            this.content = content;
        }

        public void Initialize(int capacity)
        {
            if (typeof(T) == typeof(List<TValue>) || typeof(T).IsAssignableFrom(typeof(List<TValue>)))
            {
                content = (T)(object)new List<TValue>(capacity);
            }
            else
            {
                // TODO: Capacity
                content = Activator.CreateInstance<T>();
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            int length = content.Count;

            for (int i = 0; i < length; i++)
            {
                ValueInterface<TValue>.WriteValue(dataWriter[i], content[i]);
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.WriteValue(valueWriter, content[key]);
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (key >= content.Count)
            {
                content.Add(ValueInterface<TValue>.ReadValue(valueReader));
            }
            else
            {
                content[key] = ValueInterface<TValue>.ReadValue(valueReader);
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int length = content.Count;

            var valueInfo = new ValueFilterInfo<int>();

            for (int i = 0; i < length; i++)
            {
                var value = content[i];

                ValueInterface<TValue>.WriteValue(valueInfo.ValueCopyer, value);

                valueInfo.Key = i;
                valueInfo.Type = typeof(TValue);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
            var length = Count;

            for (int i = 0; i < length; i++)
            {
                content[i] = ValueInterface<TValue>.ReadValue(dataReader[i]);
            }
        }
    }
}