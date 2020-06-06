
using Swifter.Tools;

using System;

namespace Swifter.RW
{
    /// <summary>
    /// FastObjectRW 基于 Emit 实现的几乎完美效率的对象读写接口。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class FastObjectInterface<T> : IValueInterface<T>
    {
        /// <summary>
        /// 表示是否需要进行派生类检查。
        /// </summary>
        static readonly bool CheckDerivedInstance = !(typeof(T).IsSealed || typeof(T).IsValueType);
        static readonly IntPtr TypeHandle = TypeHelper.GetTypeHandle(typeof(T));

        /// <summary>
        /// 读取一个对象。
        /// </summary>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回一个对象</returns>
        public T ReadValue(IValueReader valueReader)
        {
            var writer = FastObjectRW<T>.Create();

            valueReader.ReadObject(writer);

            return writer.content;
        }
        
        /// <summary>
        /// 写入一个对象。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="value">对象</param>
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

            var reader = FastObjectRW<T>.Create();

            reader.content = value;

            valueWriter.WriteObject(reader);
        }
    }
}