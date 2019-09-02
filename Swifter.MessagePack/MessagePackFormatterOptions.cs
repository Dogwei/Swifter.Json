namespace Swifter.MessagePack
{
    /// <summary>
    /// MessagePack 格式化器配置项。
    /// </summary>
    public enum MessagePackFormatterOptions : short
    {
        /// <summary>
        /// 默认配置项。
        /// </summary>
        Default = OutOfDepthException,

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
        /// 优先使用 32 位日期储存格式（自 UTC 时间：1970-01-01 00:00:00 起，精确到秒级）。
        /// </summary>
        UseTimestamp32 = 0x10,

        /// <summary>
        /// 未知类型使用 String 形式写入，否则将使用 Binary 方式写入。
        /// </summary>
        UnknownTypeAsString = 0x20,

        /// <summary>
        /// 序列化时如果 MessagePack 结构深度超出最大深度时抛出异常，否则将不序列化超出部分。
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