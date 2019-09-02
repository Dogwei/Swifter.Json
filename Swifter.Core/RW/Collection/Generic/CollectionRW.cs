
using Swifter.Tools;

using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class CollectionRW<T, TValue> : IDataRW<int>, IDirectContent, IInitialize<T> where T : ICollection<TValue>
    {
        public const int DefaultCapacity = 3;

        internal T content;

        public T Content => content;

        public ValueCopyer<int> this[int key] => new ValueCopyer<int>(this, key);

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(Count);

        public int Count => content.Count;

        object IDirectContent.DirectContent
        {
            get => content;
            set => content = (T)value;
        }

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
            if (typeof(T).IsAssignableFrom(typeof(List<TValue>)))
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
            var index = 0;

            foreach (var item in content)
            {
                ValueInterface<TValue>.WriteValue(dataWriter[index], item);

                ++index;
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            if (content is IList<TValue> list)
            {
                ValueInterface<TValue>.WriteValue(valueWriter, list[key]);

                return;
            }

            throw new NotSupportedException();
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            var value = ValueInterface<TValue>.ReadValue(valueReader);

            if (key >= content.Count)
            {
                content.Add(value);

                return;
            }

            if (content is IList<TValue> list)
            {
                list[key] = value;

                return;
            }

            throw new NotSupportedException($"TODO: '{typeof(T)}' not supported set elements.");
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            var index = 0;

            var valueInfo = new ValueFilterInfo<int>();

            foreach (var item in content)
            {
                ValueInterface<TValue>.WriteValue(valueInfo.ValueCopyer, item);

                valueInfo.Key = index;
                valueInfo.Type = typeof(TValue);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
            if (content is IList<TValue> list)
            {
                var length = Count;

                for (int i = 0; i < length; i++)
                {
                    list[i] = ValueInterface<TValue>.ReadValue(dataReader[i]);
                }

                return;
            }

            throw new NotSupportedException($"TODO: '{typeof(T)}' not supported set elements.");
        }
    }
}