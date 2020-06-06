using System;

namespace Swifter.RW
{
    /// <summary>
    /// FastObjectRW 初始化配置。
    /// </summary>
    [Flags]
    public enum FastObjectRWOptions : int
    {
        /// <summary>
        /// 忽略大小写。
        /// </summary>
        IgnoreCase = 0x1,

        /// <summary>
        /// 字段未找到时发生异常。
        /// </summary>
        NotFoundException = 0x2,

        /// <summary>
        /// 不能读取时发生异常。
        /// </summary>
        CannotGetException = 0x4,

        /// <summary>
        /// 不能写入时发送异常。
        /// </summary>
        CannotSetException = 0x8,

        /// <summary>
        /// 基础类型直接调用方法读写，不经过 <see cref="ValueInterface{T}"/>。
        /// </summary>
        BasicTypeDirectCallMethod = 0x10,

        /// <summary>
        /// 读写器包含属性。
        /// </summary>
        Property = 0x20,

        /// <summary>
        /// 读写器包含字段。
        /// </summary>
        Field = 0x40,

        /// <summary>
        /// 读写器包含继承的成员。
        /// </summary>
        InheritedMembers = 0x80,

        /// <summary>
        /// 在 OnReadAll 中跳过具有类型默认值的成员。
        /// </summary>
        SkipDefaultValue = 0x100,

        /// <summary>
        /// 在 OnReadAll 时只读取已定义 <see cref="RWFieldAttribute"/> 特性的成员（包括继承的类）。
        /// </summary>
        MembersOptIn = 0x200,

        /// <summary>
        /// 在 Initialize 时，不调用构造方法初始化，而是直接从内存中分配这个对象的实例。
        /// </summary>
        Allocate = 0x400,

        /// <summary>
        /// 当属性为自动属性时，直接对该属性对应的字段进行读写。
        /// </summary>
        AutoPropertyDirectRW = 0x800,
    }
}