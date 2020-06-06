
using Swifter.RW;
using Swifter.Tools;

using System;

namespace Swifter.Reflection
{
    /// <summary>
    /// 提供 XObjectRW 的读写接口。
    /// Swifter 默认的对象读写器是 FastObjectRW.
    /// FastObjectRW 对比 XObjectRW：
    ///     FastObjectRW 的优势是：效率几乎完美，内存占用也不是很大。
    ///     XObjectRW 的优势是：内存占用非常小，效率也不错，可以调用非共有成员。
    /// 如果要改为使用 XObjectRW，在程序初始化代码中添加 Swifter.RW.ValueInterface.DefaultObjectInterfaceType = typeof(Swifter.Reflection.XObjectInterface&lt;T&gt;);
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class XObjectInterface<T> : IValueInterface<T>
    {
        /// <summary>
        /// 表示是否需要进行派生类检查。
        /// </summary>
        static readonly bool CheckDerivedInstance = !(typeof(T).IsSealed || typeof(T).IsValueType);
        static readonly IntPtr TypeHandle = TypeHelper.GetTypeHandle(typeof(T));

        /// <summary>
        /// 在值读取器中读取该类型的实例。
        /// </summary>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回该类型的实例</returns>
        public T ReadValue(IValueReader valueReader)
        {
            var objectRW = XObjectRW.Create<T>(valueReader is ITargetedBind targeted && targeted.TargetedId != 0 ? targeted.GetXObjectRWFlags() : XBindingFlags.UseDefault);

            valueReader.ReadObject(objectRW);

            return (T)objectRW.Content;
        }

        /// <summary>
        /// 在数据写入器中写入该类型的实例。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="value">该类型的实例</param>
        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (CheckDerivedInstance && TypeHandle != TypeHelper.GetTypeHandle(value))
            {
                /* 父类引用，子类实例时使用 Type 获取写入器。 */

                ValueInterface.WriteValue(valueWriter, value);
            }
            else
            {
                var objectRW = XObjectRW.Create<T>(valueWriter is ITargetedBind targeted && targeted.TargetedId != 0 ? targeted.GetXObjectRWFlags() : XBindingFlags.UseDefault);

                objectRW.Initialize(value);

                valueWriter.WriteObject(objectRW);
            }
        }
    }

    /// <summary>
    /// 提供新反射工具的扩展方法。
    /// </summary>
    public static class XObjectRWExtensions
    {
        /// <summary>
        /// 设置对象读写器接口为 XObjectInterface，并设置一个支持针对性接口的对象的默认绑定标识。
        /// </summary>
        /// <param name="targeted">支持针对性接口的对象</param>
        /// <param name="flags">默认绑定标识</param>
        public static void SetXObjectRWFlags(this ITargetedBind targeted, XBindingFlags flags)
        {
            ValueInterface.DefaultObjectInterfaceType = typeof(XObjectInterface<>);

            ValueInterface<XAssistant>.SetTargetedInterface(targeted, new XAssistant(flags));
        }

        /// <summary>
        /// 获取一个支持针对性接口的对象的默认绑定标识。
        /// </summary>
        /// <param name="targeted">支持针对性接口的对象</param>
        /// <returns>返回绑定标识</returns>
        public static XBindingFlags GetXObjectRWFlags(this ITargetedBind targeted)
        {
            return ValueInterface<XAssistant>.GetTargetedInterface(targeted) is XAssistant assistant ? assistant.Flags : XBindingFlags.UseDefault;
        }

        sealed class XAssistant : IValueInterface<XAssistant>
        {
            public readonly XBindingFlags Flags;

            public XAssistant(XBindingFlags flags)
            {
                Flags = flags;
            }

            public XAssistant ReadValue(IValueReader valueReader) => default;

            public void WriteValue(IValueWriter valueWriter, XAssistant value)
            {
            }
        }
    }
}