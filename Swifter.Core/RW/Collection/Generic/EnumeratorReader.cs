using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class EnumeratorReader<T, TValue> : IDataReader<int> where T : IEnumerator<TValue>
    {
        internal T content;

        public IValueReader this[int key] => throw new NotSupportedException();

        public IEnumerable<int> Keys => throw new NotSupportedException();

        public int Count => -1;

        public Type ContentType => typeof(T);

        public object Content
        {
            get => content;
            set => content = (T)value;
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            for (int i = 0; content.MoveNext(); i++)
            {
                ValueInterface<TValue>.WriteValue(dataWriter[i], content.Current);
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter) => throw new NotSupportedException();
    }
}