
using Swifter.Tools;
using System;

namespace Swifter.RW
{
    internal sealed class ObjectInterface : IValueInterface<object>
    {
        public object? ReadValue(IValueReader valueReader)
        {
            return valueReader.DirectRead();
        }

        public void WriteValue(IValueWriter valueWriter, object? value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            /* 父类引用，子类实例时使用 Type 获取写入器。 */
            else if (value.GetType() != typeof(object))
            {
                ValueInterface.GetInterface(value).Write(valueWriter, value);
            }
            else
            {
                valueWriter.DirectWrite(value);
            }
        }
    }
}