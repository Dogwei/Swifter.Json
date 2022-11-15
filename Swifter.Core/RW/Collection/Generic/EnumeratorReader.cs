using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class EnumeratorReader<T, TValue> : IArrayReader where T : IEnumerator<TValue?>
    {
        T? content;

        public IValueReader this[int key] => throw new NotSupportedException();

        public int Count => -1;

        public Type ContentType => typeof(T);

        public Type ValueType => typeof(TValue);

        public T? Content
        {
            get => content;
            set => content = value;
        }

        public void OnReadAll(IDataWriter<int> dataWriter, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(content));
            }

            int i = 0;

            if (stopToken.CanBeStopped)
            {
                if (stopToken.PopState() is int index)
                {
                    i = index;
                }

                for (; content.MoveNext(); i++)
                {
                    if (stopToken.IsStopRequested)
                    {
                        stopToken.SetState(i);

                        return;
                    }

                    ValueInterface<TValue>.WriteValue(dataWriter[i], content.Current);
                }
            }
            else
            {
                for (; content.MoveNext(); i++)
                {
                    ValueInterface<TValue>.WriteValue(dataWriter[i], content.Current);
                }
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter) => throw new NotSupportedException();

        object? IDataReader.Content { get => Content; set => Content = (T?)value; }
    }
}