using Swifter.RW;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    /// <summary>
    /// 提供 XObjectRW 的读写接口。
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public sealed class XObjectInterface<T> : IValueInterface<T>
    {
        /// <summary>
        /// 读取或设置默认的绑定标识。
        /// </summary>
        public static XBindingFlags DefaultBindingFlags { get; set; } = XBindingFlags.UseDefault;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static XBindingFlags GetBindingFlags(object valueRW)
        {
            return valueRW is ITargetableValueRW targetable && TargetableSetOptionsHelper<XBindingFlags>.TryGetOptions(targetable, out var flags)
                ? flags
                : DefaultBindingFlags;
        }

        /// <summary>
        /// 在值读取器中读取该类型的实例。
        /// </summary>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回该类型的实例</returns>
        public T? ReadValue(IValueReader valueReader)
        {
            var objectRW = XObjectRW.Create<T>(GetBindingFlags(valueReader));

            valueReader.ReadObject(objectRW);

            return (T?)objectRW.Content;
        }

        /// <summary>
        /// 在数据写入器中写入该类型的实例。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="value">该类型的实例</param>
        public void WriteValue(IValueWriter valueWriter, T? value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (!ValueInterface<T>.IsFinalType && value.GetType() != typeof(T))
            {
                /* 父类引用，子类实例时使用 Type 获取写入器。 */
                ValueInterface.WriteValue(valueWriter, value);
            }
            else
            {
                var objectRW = XObjectRW.Create<T>(GetBindingFlags(valueWriter));

                objectRW.content = value;

                valueWriter.WriteObject(objectRW);
            }
        }
    }
}