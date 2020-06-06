namespace Swifter.Tools
{
    /// <summary>
    /// 解析的结果代码。
    /// </summary>
    public enum ParseCode
    {
        /// <summary>
        /// 正常解析。
        /// </summary>
        Success = 0,

        /// <summary>
        /// 出现超出基数范围内的字符。
        /// </summary>
        OutOfRadix = 1,

        /// <summary>
        /// 结果超出类型的范围。
        /// </summary>
        OutOfRange = 2,

        /// <summary>
        /// 格式错误。
        /// </summary>
        WrongFormat = 3,

        /// <summary>
        /// 空字符串
        /// </summary>
        Empty = 4
    }
}