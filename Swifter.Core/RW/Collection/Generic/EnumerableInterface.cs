using Swifter.Tools;
using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class EnumerableInterface<T, TValue> : IValueInterface<T> where T : IEnumerable<TValue?>
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

            if (typeof(T).IsAssignableFrom(typeof(TValue?[])))
            {
                return TypeHelper.As<TValue?[]?, T?>(ValueInterface<TValue?[]>.ReadValue(valueReader));
            }

            if (typeof(T).IsAssignableFrom(typeof(List<TValue?>)))
            {
                return TypeHelper.As<List<TValue?>?, T?>(ValueInterface<List<TValue?>>.ReadValue(valueReader));
            }

            return XConvert.Convert<TValue?[], T>(ValueInterface<TValue?[]>.ReadValue(valueReader));
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
            else if (!ValueInterface<T>.IsFinalType && value.GetType() != typeof(T))
            {
                /* 父类引用，子类实例时使用 Type 获取写入器。 */
                ValueInterface.GetInterface(value).Write(valueWriter, value);
            }
            else
            {
                valueWriter.WriteArray(new EnumeratorReader<IEnumerator<TValue?>, TValue> { Content = value.GetEnumerator() });
            }
        }
    }
}