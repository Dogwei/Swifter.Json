using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class EnumerableInterface<T, TValue> : IValueInterface<T> where T : IEnumerable<TValue>
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (typeof(T).IsAssignableFrom(typeof(TValue[])))
            {
                return (T)(object)ValueInterface<TValue[]>.ReadValue(valueReader);
            }
            else if (typeof(T).IsAssignableFrom(typeof(List<TValue>)))
            {
                return (T)(object)ValueInterface<List<TValue>>.ReadValue(valueReader);
            }

            throw new NotSupportedException();
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            var enumerator = value?.GetEnumerator();

            if (enumerator == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var enumeratorReader = new EnumeratorReader<IEnumerator<TValue>, TValue>();

            enumeratorReader.Initialize(enumerator);

            valueWriter.WriteArray(enumeratorReader);
        }
    }
}