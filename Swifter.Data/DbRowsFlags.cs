namespace Swifter.Data
{
    /// <summary>
    /// 数据行集合标识符。
    /// </summary>
    public enum DbRowsFlags : byte
    {
        /// <summary>
        /// 表示此数据行集合中的每一行，在 OnReadAll 时都跳过 Null 值。
        /// </summary>
        SkipNull = 1,
        /// <summary>
        /// 表示此数据行集合中的每一行，在 OnReadAll 时都跳过 Default 值。
        /// </summary>
        SkipDefault = 2
    }
}