
using Swifter.Tools;
using System;

namespace Swifter.RW
{
    internal sealed class ObjectInterface : IValueInterface<object>
    {
        static readonly IntPtr TypeHandle = TypeHelper.GetTypeHandle(typeof(object));

        public object ReadValue(IValueReader valueReader)
        {
            return valueReader.DirectRead();
        }

        public void WriteValue(IValueWriter valueWriter, object value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            /* 父类引用，子类实例时使用 Type 获取写入器。 */
            if (TypeHandle != TypeHelper.GetTypeHandle(value))
            {
                ValueInterface.GetInterface(value).Write(valueWriter, value);

                return;
            }

            valueWriter.DirectWrite(value);
        }
    }
}