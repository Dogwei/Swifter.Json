using System;

namespace Swifter.Tools
{
    /// <summary>
    /// 快速获取泛型类型的信息。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class TypeInfo<T>
    {
        static TypeInfo()
        {
            var type = typeof(T);

            Int64TypeHandle = (long)TypeHelper.GetTypeHandle(type);
            IsValueType = type.IsValueType;
            IsInterface = type.IsInterface;
            IsFinal = type.IsSealed || type.IsValueType;
        }

        /// <summary>
        /// 获取 Int64 类型的 TypeHandle。
        /// </summary>
        public static readonly long Int64TypeHandle;
        /// <summary>
        /// 判断是否为值类型。
        /// </summary>
        public static readonly bool IsValueType;
        /// <summary>
        /// 判断是否为接口。
        /// </summary>
        public static readonly bool IsInterface;
        /// <summary>
        /// 判断是否为最终定义。
        /// </summary>
        public static readonly bool IsFinal;
    }
}
