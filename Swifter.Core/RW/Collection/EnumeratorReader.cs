using System;
using System.Collections;
using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class EnumeratorReader<T> : IDataReader<int> where T : IEnumerator
    {
        internal T content;

        public IValueReader this[int key] => throw new NotSupportedException();

        public IEnumerable<int> Keys => throw new NotSupportedException();

        public int Count => -1;

        public Type ContentType => typeof(T);

        public object Content
        {
            get => content;
            set => Initialize((T)value);
        }

        public void Initialize(T obj)
        {
            content = obj;
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            int index = 0;

            while (content.MoveNext())
            {
                ValueInterface.WriteValue(dataWriter[index], content.Current);

                ++index;
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            throw new NotSupportedException();
        }
    }
}