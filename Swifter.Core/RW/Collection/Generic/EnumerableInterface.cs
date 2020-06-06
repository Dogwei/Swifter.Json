using System;
using System.Collections.Generic;
using Swifter.Tools;

namespace Swifter.RW
{
    internal sealed class EnumerableInterface<T, TValue> : IValueInterface<T> where T : IEnumerable<TValue>
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is ValueCopyer copyer && copyer.InternalObject is T ret)
            {
                return ret;
            }

            if (typeof(T).IsAssignableFrom(typeof(TValue[])))
            {
                return TypeHelper.As<TValue[], T>(ValueInterface<TValue[]>.ReadValue(valueReader));
            }
            else if (typeof(T).IsAssignableFrom(typeof(List<TValue>)))
            {
                return TypeHelper.As<List<TValue>, T>(ValueInterface<List<TValue>>.ReadValue(valueReader));
            }
            else
            {
                return XConvert.Convert<TValue[], T>(ValueInterface<TValue[]>.ReadValue(valueReader));
            }
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            var enumerator = value?.GetEnumerator();

            if (enumerator is null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var enumeratorReader = new EnumeratorReader<IEnumerator<TValue>, TValue>
            {
                content = enumerator
            };

            valueWriter.WriteArray(enumeratorReader);
        }
    }
}