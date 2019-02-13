namespace Swifter.Json
{
    /// <summary>
    /// JSON 格式化器配置项。
    /// </summary>
    public enum JsonFormatterOptions : short
    {
        /// <summary>
        /// 默认配置项。
        /// </summary>
        Default = 0,

        /// <summary>
        /// 序列化时不考虑对象多引用关系。该选项不能和其他引用配置复用。
        /// </summary>
        MultiReferencingNone = 0,

        /// <summary>
        /// 序列化是出现循环引用的对象时将发生异常。该选项不能和其他引用配置复用。
        /// </summary>
        LoopReferencingException = 0x1,

        /// <summary>
        /// 序列化是出现循环引用的对象时将用 Null 表示。该选项不能和其他引用配置复用。
        /// </summary>
        LoopReferencingNull = 0x2,

        /// <summary>
        /// 序列化时跳过已序列化的对象，使用 Null 表示。该选项不能和其他引用配置复用。
        /// </summary>
        MultiReferencingNull = 0x4,

        /// <summary>
        /// 允许使用 $ref 写法表示重复引用的对象。该选项不能和其他引用配置复用。
        /// </summary>
        MultiReferencingReference = 0x8,

        /// <summary>
        /// 在序列化之前先先检查成员的引用，这可以有效均衡分布多引用关系的序列化结构。
        /// 该选项和 MultiReferencingNull 或 MultiReferencingReference 一起使用。
        /// 注: 瞬态数据（如 DbDataReader，IEnumerable 等）不会做该检查。
        /// </summary>
        PriorCheckReferences = 0x10,

        /// <summary>
        /// 序列化时对 JSON 进行缩进美化。
        /// </summary>
        Indented = 0x20,

        /// <summary>
        /// 超出深度时抛出异常，否则将不序列化超出部分。
        /// </summary>
        OutOfDepthException = 0x40,

        /// <summary>
        /// 启用筛选并筛选掉 Null 值
        /// </summary>
        IgnoreNull = 0x80,

        /// <summary>
        /// 启用筛选并筛选掉 0 值
        /// </summary>
        IgnoreZero = 0x100,

        /// <summary>
        /// 启用筛选并筛选掉 "" 值 (空字符串)
        /// </summary>
        IgnoreEmptyString = 0x200,

        /// <summary>
        /// 数组元素启用筛选
        /// </summary>
        ArrayOnFilter = 0x400,
    }
}