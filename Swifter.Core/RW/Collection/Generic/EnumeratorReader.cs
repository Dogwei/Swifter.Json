using Swifter.RW;

using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class EnumeratorReader<T, TValue> : IDataReader<int>, IDirectContent, IInitialize<T> where T : IEnumerator<TValue>
    {
        internal T content;

        public IValueReader this[int key] => throw new NotSupportedException();

        public IEnumerable<int> Keys => throw new NotSupportedException();

        public int Count => 0;

        public object ReferenceToken => content;

        public T Content => content;

        public object DirectContent { get => content; set => content = (T)value; }

        public void Initialize(T obj)
        {
            content = obj;
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            int index = 0;

            while (content.MoveNext())
            {
                ValueInterface<TValue>.WriteValue(dataWriter[index], content.Current);

                ++index;
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int index = 0;

            var valueInfo = new ValueFilterInfo<int>();

            while (content.MoveNext())
            {
                ValueInterface<TValue>.WriteValue(valueInfo.ValueCopyer, content.Current);

                valueInfo.Key = index;
                valueInfo.Type = typeof(TValue);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[index]);
                }

                ++index;
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            throw new NotSupportedException();
        }
    }
}