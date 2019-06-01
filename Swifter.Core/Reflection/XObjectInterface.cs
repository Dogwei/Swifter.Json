using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
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
        private static readonly bool CheckChildrenInstance = typeof(T).IsClass && (!typeof(T).IsSealed);
        private static readonly IntPtr TypeHandle = TypeHelper.GetTypeHandle(typeof(T));
        private static readonly XBindingFlags CurrentBindingFlags = GetFastObjectRWFlags();

        private static XBindingFlags GetFastObjectRWFlags()
        {
            var fastOptions = FastObjectRW<T>.CurrentOptions;
            var xFlags = XBindingFlags.Public | XBindingFlags.Instance;

            if ((fastOptions & FastObjectRWOptions.Allocate) != 0)
                xFlags |= XBindingFlags.RWAllocate;
            if ((fastOptions & FastObjectRWOptions.CannotGetException) != 0)
                xFlags |= XBindingFlags.RWCannotGetException;
            if ((fastOptions & FastObjectRWOptions.CannotSetException) != 0)
                xFlags |= XBindingFlags.RWCannotSetException;
            if ((fastOptions & FastObjectRWOptions.Field) != 0)
                xFlags |= XBindingFlags.Field;
            if ((fastOptions & FastObjectRWOptions.IgnoreCase) != 0)
                xFlags |= XBindingFlags.RWIgnoreCase;
            if ((fastOptions & FastObjectRWOptions.InheritedMembers) != 0)
                xFlags |= XBindingFlags.InheritedMembers;
            if ((fastOptions & FastObjectRWOptions.MembersOptIn) != 0)
                xFlags |= XBindingFlags.RWMembersOptIn;
            if ((fastOptions & FastObjectRWOptions.NotFoundException) != 0)
                xFlags |= XBindingFlags.RWNotFoundException;
            if ((fastOptions & FastObjectRWOptions.Property) != 0)
                xFlags |= XBindingFlags.Property;
            if ((fastOptions & FastObjectRWOptions.SkipDefaultValue) != 0)
                xFlags |= XBindingFlags.RWSkipDefaultValue;

            return xFlags;
        }
        
        /// <summary>
        /// 在值读取器中读取该类型的实例。
        /// </summary>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回该类型的实例</returns>
        public T ReadValue(IValueReader valueReader)
        {
            var objectRW = XObjectRW.Create<T>(CurrentBindingFlags);

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
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            /* 父类引用，子类实例时使用 Type 获取写入器。 */
            if (CheckChildrenInstance && TypeHandle != TypeHelper.GetTypeHandle(value))
            {
                ValueInterface.WriteValue(valueWriter, value);

                return;
            }

            var objectRW = XObjectRW.Create<T>(CurrentBindingFlags);

            objectRW.Initialize(value);

            valueWriter.WriteObject(objectRW);
        }
    }
}