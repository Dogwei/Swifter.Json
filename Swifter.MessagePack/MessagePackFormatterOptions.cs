namespace Swifter.MessagePack
{
    public enum MessagePackFormatterOptions
    {
        /// <summary>
        /// 优先使用 32 位日期储存格式（自 UTC 时间：1970-01-01 00:00:00 起，精确到秒级）。
        /// </summary>
        UseTimestamp32 = 0x1,
        /// <summary>
        /// 未知类型使用 String 形式写入，否则将使用 Binary 方式写入。
        /// </summary>
        UnknownTypeAsString = 0x2,
        /// <summary>
        /// 序列化时如果 MessagePack 结构深度超出最大深度时抛出异常，否则将不序列化超出部分。
        /// </summary>
        OutOfDepthException = 0x4,
    }
}