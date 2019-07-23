using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;

namespace Swifter.RW
{
    internal sealed class UnknowTypeInterface<T> : IValueInterface<T>
    {
        static readonly long Int64TypeHandle = TypeInfo<T>.Int64TypeHandle;
        
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T>)
            {
                return ((IValueReader<T>)valueReader).ReadValue();
            }

            var directValue = valueReader.DirectRead();

            if (directValue is T || directValue == null)
            {
                return (T)directValue;
            }

            return XConvert.FromObject<T>(directValue);
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            if (valueWriter is IValueWriter<T> writer)
            {
                writer.WriteValue(value);

                return;
            }

            if (value is IFormattable)
            {
                valueWriter.DirectWrite(value);

                return;
            }

            if (value is string str)
            {
                valueWriter.WriteString(str);

                return;
            }

            /* 父类引用，子类实例时使用 Type 获取写入器。 */
            if (Int64TypeHandle != (long)TypeHelper.GetTypeHandle(value))
            {
                ValueInterface.GetInterface(value).Write(valueWriter, value);

                return;
            }

            valueWriter.DirectWrite(value);
        }
    }
}