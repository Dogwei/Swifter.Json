using Swifter.RW;

namespace Swifter.Reflection
{
    /// <summary>
    /// 创建 <see cref="XTypeInfo"/> 或 <see cref="XObjectRW"/> 时指示要获取成员的标识。
    /// </summary>
    public enum XBindingFlags
    {
        /// <summary>
        /// 没有任何标识，表示使用默认标识。
        /// </summary>
        UseDefault = 0x0,
        /// <summary>
        /// 表示要获取类型的字段。
        /// </summary>
        Field = 0x1,
        /// <summary>
        /// 表示要获取类型的属性。
        /// </summary>
        Property = 0x2,
        /// <summary>
        /// 表示要获取类型的事件。
        /// </summary>
        Event = 0x4,
        /// <summary>
        /// 表示要获取类型的方法。
        /// </summary>
        Method = 0x8,
        /// <summary>
        /// 表示要获取类型的索引器。
        /// </summary>
        Indexer = 0x10,
        /// <summary>
        /// 表示要获取类型的公开成员。
        /// </summary>
        Public = 0x20,
        /// <summary>
        /// 表示要获取类型的非公开成员。
        /// </summary>
        NonPublic = 0x40,
        /// <summary>
        /// 表示要获取类型的静态成员。
        /// </summary>
        Static = 0x80,
        /// <summary>
        /// 表示要获取类型的实例成员。
        /// </summary>
        Instance = 0x100,
        /// <summary>
        /// 表示当属性或索引器调用 set 方法失败时是否抛出异常。
        /// </summary>
        RWCannotSetException = 0x200,
        /// <summary>
        /// 表示当属性或索引器调用 get 方法失败时是否抛出异常。
        /// </summary>
        RWCannotGetException = 0x400,
        /// <summary>
        /// 表示数据读取器的成员名称匹配是否区分大小写。
        /// </summary>
        RWIgnoreCase = 0x800,
        /// <summary>
        /// 表示数据读取器的成员名称无匹配时是否抛出异常。
        /// </summary>
        RWNotFoundException = 0x1000,
        /// <summary>
        /// 在 <see cref="XObjectRW.OnReadAll(RW.IDataWriter{string})"/> 中跳过具有类型默认值的成员。
        /// </summary>
        RWSkipDefaultValue = 0x2000,
        /// <summary>
        /// 在 <see cref="XObjectRW.OnReadAll(RW.IDataWriter{string})"/> 时只读取已定义 <see cref="RWFieldAttribute"/> 特性的成员（包括继承的类）。
        /// </summary>
        RWMembersOptIn = 0x4000,
        /// <summary>
        /// 在 <see cref="XObjectRW.Initialize()"/> 时，不调用构造方法初始化，而是直接从内存中分配这个对象的实例。
        /// </summary>
        RWAllocate = 0x8000,
        /// <summary>
        /// 当属性为自动属性时，直接对该属性对应的字段进行读写。
        /// </summary>
        RWAutoPropertyDirectRW = 0x10000,
        /// <summary>
        /// 表示要包含继承的成员。
        /// </summary>
        InheritedMembers = 0x20000,
        /// <summary>
        /// XTypeInfo 创建时默认的标识。
        /// </summary>
        Default = Public | Instance | Field | Property | Event | Indexer | Method | InheritedMembers | RWCannotGetException | RWCannotSetException | RWNotFoundException,
    }
}