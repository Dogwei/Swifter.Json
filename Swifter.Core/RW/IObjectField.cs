using System;

namespace Swifter.RW
{
    /// <summary>
    /// 对象读写器的字段信息。
    /// </summary>
    public interface IObjectField
    {
        /// <summary>
        /// 字段名称。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 字段的初始类型。
        /// </summary>
        Type BeforeType { get; }

        /// <summary>
        /// 字段在经过处理之后的类型。
        /// </summary>
        Type AfterType { get; }

        /// <summary>
        /// 能否读取。
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// 能否写入。
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// 是否公开的字段。
        /// </summary>
        bool IsPublic { get; }

        /// <summary>
        /// 是否时静态字段。
        /// </summary>
        bool IsStatic { get; }

        /// <summary>
        /// 字段排序值。
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 字段原始信息。
        /// </summary>
        object Original { get; }

        /// <summary>
        /// 该字段在 OnReadAll 时是否忽略默认值。
        /// </summary>
        bool SkipDefaultValue { get; }

        /// <summary>
        /// 当无法获取该字段的值时是否引发异常。
        /// </summary>
        bool CannotGetException { get; }

        /// <summary>
        /// 当无法设置该字段的值时是否引发异常。
        /// </summary>
        bool CannotSetException { get; }
    }
}
