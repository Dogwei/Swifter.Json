using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.RW
{
    internal sealed class EnumeratorInterface<T, TValue> : IValueInterface<T> where T : IEnumerator<TValue?>
    {
        public T ReadValue(IValueReader valueReader)
        {
            throw new NotSupportedException();
        }

        public void WriteValue(IValueWriter valueWriter, [AllowNull] T value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (valueWriter is IValueWriter<T> writer)
            {
                writer.WriteValue(value);
            }
            else
            {
                valueWriter.WriteArray(new EnumeratorReader<T, TValue> { Content = value });
            }
        }
    }
}