
using Swifter.RW;

namespace Swifter.Reflection
{
    /// <summary>
    /// 提供新反射工具的扩展方法。
    /// </summary>
    public static class XObjectRWExtensions
    {
        /// <summary>
        /// 设置对象读写器接口为 XObjectInterface，并设置一个支持针对性接口的对象的默认绑定标识。
        /// </summary>
        /// <param name="targetable">支持针对性接口的对象</param>
        /// <param name="flags">默认绑定标识</param>
        public static void SetXObjectRWFlags(this ITargetableValueRWSource targetable, XBindingFlags flags)
        {
            ValueInterface.DefaultObjectInterfaceType = typeof(XObjectInterface<>);

            TargetableSetOptionsHelper<XBindingFlags>.SetOptions(targetable, flags);
        }

        /// <summary>
        /// 获取一个支持针对性接口的对象的默认绑定标识。
        /// </summary>
        /// <param name="targetable">支持针对性接口的对象</param>
        /// <returns>返回绑定标识</returns>
        public static XBindingFlags GetXObjectRWFlags(this ITargetableValueRWSource targetable)
        {
            if (TargetableSetOptionsHelper<XBindingFlags>.TryGetOptions(targetable, out var flags))
            {
                return flags;
            }

            return XBindingFlags.UseDefault;
        }
    }
}