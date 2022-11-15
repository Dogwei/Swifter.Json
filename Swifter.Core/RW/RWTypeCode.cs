namespace Swifter.RW
{
    /// <summary>
    /// 类型代码
    /// </summary>
    public enum RWTypeCode
    {
        /// <summary>
        /// <see langword="null"/>
        /// </summary>
        Null = 0,
        /// <summary>
        /// <see cref="Boolean"/>
        /// </summary>
        Boolean,
        /// <summary>
        /// <see cref="Byte"/>
        /// </summary>
        Byte,
        /// <summary>
        /// <see cref="SByte"/>
        /// </summary>
        SByte,
        /// <summary>
        /// <see cref="Char"/>
        /// </summary>
        Char,
        /// <summary>
        /// <see cref="Int16"/>
        /// </summary>
        Int16,
        /// <summary>
        /// <see cref="UInt16"/>
        /// </summary>
        UInt16,
        /// <summary>
        /// <see cref="Int32"/>
        /// </summary>
        Int32,
        /// <summary>
        /// <see cref="UInt32"/>
        /// </summary>
        UInt32,
        /// <summary>
        /// <see cref="Int64"/>
        /// </summary>
        Int64,
        /// <summary>
        /// <see cref="UInt64"/>
        /// </summary>
        UInt64,
        /// <summary>
        /// <see cref="Single"/>
        /// </summary>
        Single,
        /// <summary>
        /// <see cref="Double"/>
        /// </summary>
        Double,
        /// <summary>
        /// <see cref="Decimal"/>
        /// </summary>
        Decimal,
        /// <summary>
        /// <see cref="DateTime"/>
        /// </summary>
        DateTime,
        /// <summary>
        /// <see cref="DateTimeOffset"/>
        /// </summary>
        DateTimeOffset,
        /// <summary>
        /// <see cref="TimeSpan"/>
        /// </summary>
        TimeSpan,
        /// <summary>
        /// <see cref="Guid"/>
        /// </summary>
        Guid,
        /// <summary>
        /// <see cref="String"/>
        /// </summary>
        String,
        /// <summary>
        /// <see cref="Enum"/>
        /// </summary>
        Enum,
        /// <summary>
        /// 对象读取器
        /// </summary>
        Object,
        /// <summary>
        /// 数组读取器
        /// </summary>
        Array,
        /// <summary>
        /// 其他
        /// </summary>
        Other,
    }
}