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
        /// 字段的类型。
        /// </summary>
        Type BeforeType { get; }

        /// <summary>
        /// ValueInterface 的类型。
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
    }
}
