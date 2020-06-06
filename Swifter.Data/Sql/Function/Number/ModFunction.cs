namespace Swifter.Data.Sql
{
    /// <summary>
    /// 求余函数。
    /// </summary>
    public sealed class ModFunction : BinaryFunction
    {
        /// <summary>
        /// 构建求余函数。
        /// </summary>
        /// <param name="left">左值</param>
        /// <param name="right">右值</param>
        public ModFunction(IValue left, IValue right) : base(left, right)
        {
        }
    }
}