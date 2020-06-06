namespace Swifter.Data.Sql
{
    /// <summary>
    /// 除法函数。
    /// </summary>
    public sealed class DivideFunction : BinaryFunction
    {
        /// <summary>
        /// 构建除法函数信息。
        /// </summary>
        /// <param name="left">左值</param>
        /// <param name="right">右值</param>
        public DivideFunction(IValue left, IValue right) : base(left, right)
        {
        }
    }
}