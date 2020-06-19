using Swifter.Tools;
using System;

namespace Swifter.RW
{
    sealed class NonPublicFastObjectCreater<T> : IFastObjectRWCreater<T>, IValueInterface<T>
    {
        static readonly bool CheckDerivedInstance = !(typeof(T).IsSealed || typeof(T).IsValueType);
        static readonly IntPtr TypeHandle = TypeHelper.GetTypeHandle(typeof(T));

        public FastObjectRW<T> Create()
        {
            return new NonPublicFastObjectRW<T>();
        }

        public T ReadValue(IValueReader valueReader)
        {
            var writer = new NonPublicFastObjectRW<T>();

            valueReader.ReadObject(writer);

            return writer.content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            /* 父类引用，子类实例时使用 Type 获取写入器。 */
            if (CheckDerivedInstance && TypeHandle != TypeHelper.GetTypeHandle(value))
            {
                ValueInterface.GetInterface(value).Write(valueWriter, value);

                return;
            }

            valueWriter.WriteObject(new NonPublicFastObjectRW<T>
            {
                content = value
            });
        }
    }
}