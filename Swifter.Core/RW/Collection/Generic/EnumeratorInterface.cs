using Swifter.RW;

using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class EnumeratorInterface<T, TValue> : IValueInterface<T> where T : IEnumerator<TValue>
    {
        public T ReadValue(IValueReader valueReader)
        {
            throw new NotSupportedException();
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var enumeratorReader = new EnumeratorReader<T, TValue>();

            enumeratorReader.Initialize(value);

            valueWriter.WriteArray(enumeratorReader);
        }
    }
}