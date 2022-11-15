
using Swifter.Tools;

using System;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.RW
{
    /// <summary>
    /// FastObjectRW 基于 Emit 实现的几乎完美效率的对象读写接口。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class FastObjectInterface<T> : IValueInterface<T>
    {
        /// <summary>
        /// 读取一个对象。
        /// </summary>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回一个对象</returns>
        [return: MaybeNull]
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
        public void WriteValue(IValueWriter valueWriter, [AllowNull]T value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (!ValueInterface<T>.IsFinalType && value.GetType() != typeof(T))
            {
                /* 父类引用，子类实例时使用 Type 获取写入器。 */
                ValueInterface.GetInterface(value).Write(valueWriter, value);
            }
            else
            {
                var reader = FastObjectRW<T>.Create();

                reader.content = value;

                valueWriter.WriteObject(reader);
            }
        }
    }
}