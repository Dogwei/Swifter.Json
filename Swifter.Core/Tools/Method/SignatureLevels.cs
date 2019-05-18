namespace Swifter.Tools
{
    /// <summary>
    /// 方法或委托的签名等级。
    /// </summary>
    public enum SignatureLevels : byte
    {
        /// <summary>
        /// 只进行参数和返回值的数量匹配，不对类型匹配。
        /// </summary>
        None,
        /// <summary>
        /// 允许通过转换的参数和返回值类型匹配。
        /// </summary>
        Cast,
        /// <summary>
        /// 完全匹配，要求参数和返回值类型完全一致。
        /// </summary>
        Consistent
    }
}
