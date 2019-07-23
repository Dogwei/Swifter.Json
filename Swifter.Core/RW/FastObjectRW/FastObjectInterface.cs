using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;

namespace Swifter.RW
{
    internal sealed class FastObjectInterface<T> : IValueInterface<T>
    {
        /// <summary>
        /// 表示是否需要进行派生类检查。
        /// </summary>
        static readonly bool CheckDerivedInstance = !TypeInfo<T>.IsFinal;
        static readonly IntPtr TypeHandle = TypeHelper.GetTypeHandle(typeof(T));

        public T ReadValue(IValueReader valueReader)
        {
            var writer = FastObjectRW<T>.Create();

            valueReader.ReadObject(writer);

            return writer.Content;
        }
        
        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
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

            var reader = FastObjectRW<T>.Create();

            reader.Initialize(value);

            valueWriter.WriteObject(reader);
        }
    }
}