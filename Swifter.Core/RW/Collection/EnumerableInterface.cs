using Swifter.RW;

using System;
using System.Collections;

namespace Swifter.RW
{
    internal sealed class EnumerableInterface<T> : IValueInterface<T> where T : IEnumerable
    {
        public T ReadValue(IValueReader valueReader)
        {
            throw new NotSupportedException();
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var enumeratorReader = new EnumeratorReader<IEnumerator>();

            enumeratorReader.Initialize(value.GetEnumerator());

            valueWriter.WriteArray(enumeratorReader);
        }
    }
}