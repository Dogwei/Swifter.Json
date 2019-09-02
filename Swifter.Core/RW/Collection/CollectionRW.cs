
using Swifter.Tools;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class CollectionRW<T> : IDataRW<int>, IDirectContent where T : ICollection
    {
        public const int DefaultCapacity = 3;

        internal T content;

        public T Content => content;

        public ValueCopyer<int> this[int key]=> throw new NotSupportedException();

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(content.Count);

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
            if (typeof(T).IsAssignableFrom(typeof(ArrayList)))
            {
                content = (T)(object)new ArrayList(capacity);
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

                return;
            }

            throw new NotSupportedException();
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (content is IList list)
            {
                var value = ValueInterface<object>.ReadValue(valueReader);

                if (key < content.Count)
                {
                    list[key] = value;
                }
                else
                {
                    list.Add(value);
                }

                return;
            }

            throw new NotSupportedException();
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            var index = 0;

            var valueInfo = new ValueFilterInfo<int>();

            foreach (var item in content)
            {
                ValueInterface.WriteValue(valueInfo.ValueCopyer, item);

                valueInfo.Key = index;
                valueInfo.Type = typeof(object);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
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

                return;
            }

            throw new NotSupportedException($"TODO: '{typeof(T)}' not supported set elements.");
        }
    }
}