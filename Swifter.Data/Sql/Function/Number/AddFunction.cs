namespace Swifter.Data.Sql
{
    /// <summary>
    /// 加法函数。
    /// </summary>
    public sealed class AddFunction : BinaryFunction
    {
        /// <summary>
        /// 构建加法函数信息。
        /// </summary>
        /// <param name="left">值 1</param>
        /// <param name="right">值 2</param>
        public AddFunction(IValue left, IValue right) : base(left, right)
        {
        }
    }
}