using System;
using System.Collections;

namespace Swifter.RW
{
    internal sealed class EnumeratorReader<T> : IArrayReader where T : IEnumerator
    {
        T? content;

        public IValueReader this[int key] => throw new NotSupportedException();

        public int Count => -1;

        public Type ContentType => typeof(T);

        public Type ValueType => typeof(object);

        public T? Content
        {
            get => content;
            set => content = value;
        }

        public void OnReadAll(IDataWriter<int> dataWriter, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

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

        object? IDataReader.Content { get => Content; set => Content = (T?)value; }
    }
}