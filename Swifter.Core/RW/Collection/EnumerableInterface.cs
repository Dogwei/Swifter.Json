using Swifter.Tools;
using System;
using System.Collections;

namespace Swifter.RW
{
    internal sealed class EnumerableInterface<T> : IValueInterface<T> where T : IEnumerable
    {
        public T? ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> reader)
            {
                return reader.ReadValue();
            }

            if (valueReader.ValueType is Type valueType && XConvert.IsEffectiveConvert(valueType, typeof(T)))
            {
                return XConvert.Convert<T>(valueReader.DirectRead());
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
                valueWriter.WriteArray(new EnumeratorReader<IEnumerator> { Content = value.GetEnumerator() });
            }
        }
    }
}