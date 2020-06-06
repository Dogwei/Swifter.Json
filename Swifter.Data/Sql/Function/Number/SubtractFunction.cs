namespace Swifter.Data.Sql
{
    /// <summary>
    /// 减法函数。
    /// </summary>
    public sealed class SubtractFunction : BinaryFunction
    {
        /// <summary>
        /// 构建减法函数信息。
        /// </summary>
        /// <param name="left">左值</param>
        /// <param name="right">右值</param>
        public SubtractFunction(IValue left, IValue right) : base(left, right)
        {
        }
    }
}