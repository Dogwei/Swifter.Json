
using Swifter.Tools;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Swifter.RW
{
    internal sealed class CollectionRW<T> : IDataRW<int> where T : ICollection
    {
        public const int DefaultCapacity = 3;

        internal T content;

        public ValueCopyer<int> this[int key]=> throw new NotSupportedException();

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
            if (typeof(T).IsAssignableFrom(typeof(ArrayList)))
            {
                Underlying.As<T, ArrayList>(ref content) = new ArrayList(capacity);
            }
            else
            {
                // TODO: Capacity.
                content = Activator.CreateInstance<T>();
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            var index = 0;

            foreach (var item in content)
            {
                ValueInterface.WriteValue(dataWriter[index], item);

                ++index;
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            if (content is IList list)
            {
                ValueInterface.WriteValue(valueWriter, list[key]);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (content is IList list)
            {
                var value = ValueInterface<object>.ReadValue(valueReader);

                if (key == Count)
                {
                    list.Add(value);
                }
                else
                {
                    list[key] = value;
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
            if (content is IList list)
            {
                var length = Count;

                for (int i = 0; i < length; i++)
                {
                    list[i] = ValueInterface<object>.ReadValue(dataReader[i]);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}