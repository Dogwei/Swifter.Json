namespace Swifter.Data.Sql
{
    /// <summary>
    /// 字符串 Upper 函数。
    /// </summary>
    public sealed class UpperFunction : UnaryFunction
    {
        /// <summary>
        /// 构建 Upper 函数信息。
        /// </summary>
        /// <param name="value">参数</param>
        public UpperFunction(IValue value) : base(value)
        {
        }
    }
}