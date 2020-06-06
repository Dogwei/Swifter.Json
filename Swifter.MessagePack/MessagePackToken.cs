namespace Swifter.MessagePack
{
    /// <summary>
    /// 表示 MessagePack 值的 Token。
    /// </summary>
    public enum MessagePackToken
    {
        /// <summary>
        /// 表示一个 Null 值。
        /// </summary>
        Nil,
        /// <summary>
        /// 表示一个 Boolean 值，包括 True 和 False。
        /// </summary>
        Bool,
        /// <summary>
        /// 表示一个有符号的 8 位整数。
        /// </summary>
        Int8,
        /// <summary>
        /// 表示一个有符号的 16 位整数。
        /// </summary>
        Int16,
        /// <summary>
        /// 表示一个有符号的 32 位整数。
        /// </summary>
        Int32,
        /// <summary>
        /// 表示一个有符号的 64 位整数。
        /// </summary>
        Int64,
        /// <summary>
        /// 表示一个无符号的 8 位整数。
        /// </summary>
        UInt8,
        /// <summary>
        /// 表示一个无符号的 16 位整数。
        /// </summary>
        UInt16,
        /// <summary>
        /// 表示一个无符号的 32 位整数。
        /// </summary>
        UInt32,
        /// <summary>
        /// 表示一个无符号的 64 位整数。
        /// </summary>
        UInt64,
        /// <summary>
        /// 表示一个 32 位浮点数值。
        /// </summary>
        Float32,
        /// <summary>
        /// 表示一个 64 位浮点数值。
        /// </summary>
        Float64,
        /// <summary>
        /// 表示一个字符串值。
        /// </summary>
        Str,
        /// <summary>
        /// 表示一个 Map 值。
        /// </summary>
        Map,
        /// <summary>
        /// 标识一个数组值。
        /// </summary>
        Array,
        /// <summary>
        /// 表示一个二进制内容值。
        /// </summary>
        Bin,
        /// <summary>
        /// 表示一个扩展类型值。
        /// </summary>
        Ext,
        /// <summary>
        /// 表示一个未使用的固定位。
        /// </summary>
        NeverUsed,
        /// <summary>
        /// 这不表示一个值，而是表示一个 MessagePack 的结尾。
        /// </summary>
        End,
    }
}