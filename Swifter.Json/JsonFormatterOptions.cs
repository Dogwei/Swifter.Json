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
        /// 执行假定紧凑（无多余空格）且标准的 JSON 反序列化，此配置有效提高反序列化性能。
        /// </summary>
        DeflateDeserialize = 0x10,

        /// <summary>
        /// 执行假定标准的 JSON 反序列化（即 不执行部分验证）。
        /// </summary>
        StandardDeserialize = 0x20,

        /// <summary>
        /// 执行完全验证的 JSON 反序列化（这是默认行为）。
        /// </summary>
        VerifiedDeserialize = 0x0,

        /// <summary>
        /// 序列化时对 JSON 进行缩进美化。
        /// </summary>
        Indented = 0x80,

        /// <summary>
        /// 超出深度时抛出异常，否则将不序列化超出部分。
        /// </summary>
        OutOfDepthException = 0x100,

        /// <summary>
        /// 启用筛选并筛选掉 Null 值
        /// </summary>
        IgnoreNull = 0x200,

        /// <summary>
        /// 启用筛选并筛选掉 0 值
        /// </summary>
        IgnoreZero = 0x400,

        /// <summary>
        /// 启用筛选并筛选掉 "" 值 (空字符串)
        /// </summary>
        IgnoreEmptyString = 0x800,

        /// <summary>
        /// 数组元素启用筛选
        /// </summary>
        ArrayOnFilter = 0x1000,
    }
}