namespace Swifter.Data.Sql
{
    /// <summary>
    /// 空合并函数。
    /// </summary>
    public sealed class IfNullFunction : BinaryFunction
    {
        /// <summary>
        /// 构建空合并函数。
        /// </summary>
        /// <param name="left">左值</param>
        /// <param name="right">右值</param>
        public IfNullFunction(IValue left, IValue right) : base(left, right)
        {
        }
    }
}