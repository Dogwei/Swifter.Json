
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.RW
{
    internal sealed class EnumeratorInterface<T> : IValueInterface<T> where T : IEnumerator
    {
        public T? ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> reader)
            {
                return reader.ReadValue();
            }

            throw new NotSupportedException();
        }

        public void WriteValue(IValueWriter valueWriter, T? value)
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
                valueWriter.WriteArray(new EnumeratorReader<T> { Content = value });
            }
        }
    }
}