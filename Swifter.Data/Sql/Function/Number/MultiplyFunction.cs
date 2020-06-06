namespace Swifter.Data.Sql
{
    /// <summary>
    /// 乘法函数。
    /// </summary>
    public sealed class MultiplyFunction : BinaryFunction
    {
        /// <summary>
        /// 构建乘法函数信息。
        /// </summary>
        /// <param name="left">值 1</param>
        /// <param name="right">值 2</param>
        public MultiplyFunction(IValue left, IValue right) : base(left, right)
        {
        }
    }
}