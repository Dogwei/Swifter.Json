namespace Swifter.Reflection
{
    /// <summary>
    /// 创建 XTypeInfo 或 XObjectRW 时指示要获取成员的标识。
    /// </summary>
    public enum XBindingFlags
    {
        /// <summary>
        /// 没有任何标识，通常表示使用默认标识。
        /// </summary>
        None = 0x0,
        /// <summary>
        /// 表示要获取类型的字段。
        /// </summary>
        Field = 0x1,
        /// <summary>
        /// 表示要获取类型的属性。
        /// </summary>
        Property = 0x2,
        /// <summary>
        /// 表示要获取类型的方法。
        /// </summary>
        Method = 0x4,
        /// <summary>
        /// 表示要获取类型的索引器。
        /// </summary>
        Indexer = 0x8,
        /// <summary>
        /// 表示要获取类型的公开成员。
        /// </summary>
        Public = 0x10,
        /// <summary>
        /// 表示要获取类型的非公开成员。
        /// </summary>
        NonPublic = 0x20,
        /// <summary>
        /// 表示要获取类型的静态成员。
        /// </summary>
        Static = 0x40,
        /// <summary>
        /// 表示要获取类型的实例成员。
        /// </summary>
        Instance = 0x80,
        /// <summary>
        /// 表示当属性或索引器调用 set 方法失败时是否抛出异常。
        /// </summary>
        RWCannotSetException = 0x100,
        /// <summary>
        /// 表示当属性或索引器调用 get 方法失败时是否抛出异常。
        /// </summary>
        RWCannotGetException = 0x200,
        /// <summary>
        /// 表示数据读取器的成员名称匹配是否区分大小写。
        /// </summary>
        RWIgnoreCase = 0x400,
        /// <summary>
        /// 表示数据读取器的成员名称无匹配时是否抛出异常。
        /// </summary>
        RWNotFoundException = 0x800,
        /// <summary>
        /// 在 OnReadAll 中跳过具有类型默认值的成员。
        /// </summary>
        RWSkipDefaultValue = 0x1000,
        /// <summary>
        /// 在 OnReadAll 时只读取已定义 RWField(包括继承的类) 特性的成员。
        /// </summary>
        RWMembersOptIn = 0x2000,

        /// <summary>
        /// XTypeInfo 创建时默认的标识。
        /// </summary>
        Default = Public | Instance | Field | Property | Indexer | Method | RWCannotGetException | RWCannotSetException | RWNotFoundException
    }
}