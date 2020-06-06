namespace Swifter.Data.Sql
{
    /// <summary>
    /// 字符串 Lower 函数。
    /// </summary>
    public sealed class LowerFunction : UnaryFunction
    {
        /// <summary>
        /// 构建 Lower 函数信息。
        /// </summary>
        /// <param name="value">参数</param>
        public LowerFunction(IValue value) : base(value)
        {
        }
    }
}