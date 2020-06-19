namespace Swifter.MessagePack
{
    /// <summary>
    /// 
    /// MessagePack 格式化器配置项。
    /// </summary>
    public enum MessagePackFormatterOptions : int
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
        /// 此配置是 <see cref="MultiReferencingNull"/> 和 <see cref="MultiReferencingReference"/> 的可选配置。
        /// 开启此配置将字符串也纳入多引用范畴。
        /// </summary>
        MultiReferencingAlsoString = 0x10,

        /// <summary>
        /// 在反序列化对象时，将执行假定字段顺序对应的解析。
        /// 如果启用此配置；那么当 MessagePack 对象与目标对象的字段顺序相同时效率更快，反之则变慢。
        /// 原理时在反序列化时，将循环一次目标对象的所有字段；
        /// 当目标对象的字段名与当前正在解析的 MessagePack 字段名一致时（忽略大小写），则读取 MessagePack 字段对应的值到目标对象的字段对应的值中；
        /// 如果字段名称不匹配，则跳过读取，并继续循环目标对象的字段。
        /// 当满足顺序要求时，如果目标对象的字段数多于或等于 MessagePack 对象中的字段数，这个假定有序的解析会成功。
        /// 但如果目标对象的字段数少于 MessagePack 对象中的字段数那么假定有序的解析不成功，会降低性能，
        /// </summary>
        AsOrderedObjectDeserialize = 0x20,

        /// <summary>
        /// 反序列化的配置项，当反序列化除字符串和通用类型外的可空类型时，如果 MsgPack 值是 0 长度的字符串，则解析为 Null。
        /// </summary>
        EmptyStringAsNull = 0x40,

        /// <summary>
        /// 反序列化的配置项，当反序列化非字符串和通用类型时，如果 MsgPack 值是 0 长度的字符串，则解析为 Default。
        /// </summary>
        EmptyStringAsDefault = 0x80,

        /// <summary>
        /// 优先使用 32 位日期储存格式（自 UTC 时间：1970-01-01 00:00:00 起，精确到秒级）。
        /// </summary>
        UseTimestamp32 = 0x100,

        /// <summary>
        /// 序列化时如果 MessagePack 结构深度超出最大深度时抛出异常，否则将不序列化超出部分。
        /// </summary>
        OutOfDepthException = 0x400,

        /// <summary>
        /// 序列化对象时，字段名使用驼峰命名法。即：如果字段名首字母为大写，则将首字母写入为小写字母。
        /// </summary>
        CamelCaseWhenSerialize = 0x800,

        /// <summary>
        /// 对象元素启用筛选。
        /// </summary>
        OnFilter = 0x1000,

        /// <summary>
        /// 在序列化或反序列化时忽略 Null 值。
        /// </summary>
        IgnoreNull = 0x2000,

        /// <summary>
        /// 在序列化或反序列化时忽略 0 值。
        /// </summary>
        IgnoreZero = 0x4000,

        /// <summary>
        /// 在序列化时忽略 ""(空字符串) 值。
        /// </summary>
        IgnoreEmptyString = 0x8000,

        /// <summary>
        /// 数组元素启用筛选
        /// </summary>
        ArrayOnFilter = 0x10000,
    }
}