
using Swifter.Tools;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Swifter.RW
{
    internal sealed class CollectionRW<T, TValue> : IDataRW<int> where T : ICollection<TValue>
    {
        public const int DefaultCapacity = 3;

        internal T content;

        public ValueCopyer<int> this[int key] => new ValueCopyer<int>(this, key);

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        public IEnumerable<int> Keys => Enumerable.Range(0, Count);

        public int Count => content.Count;

        public object Content
        {
            get => content;
            set => content = (T)value;
        }

        public Type ContentType => typeof(T);

        IValueRW IDataRW<int>.this[int key] => this[key];

        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        public void Initialize(int capacity)
        {
            if (typeof(T).IsAssignableFrom(typeof(List<TValue>)))
            {
                Underlying.As<T, List<TValue>>(ref content) = new List<TValue>(capacity);
            }
#if Linq
            else if (typeof(T).IsAssignableFrom(typeof(HashSet<TValue>)))
            {
                Underlying.As<T, HashSet<TValue>>(ref content) = new HashSet<TValue>();
            }
#endif
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

            if (key == Count)
            {
                content.Add(value);
            }
            else if (content is IList<TValue> list)
            {
                list[key] = value;
            }
            else
            {
                throw new NotSupportedException();
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
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}