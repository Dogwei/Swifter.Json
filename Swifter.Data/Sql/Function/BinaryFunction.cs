namespace Swifter.Data.Sql
{
    /// <summary>
    /// 二元函数。
    /// </summary>
    public abstract class BinaryFunction : IValue
    {
        /// <summary>
        /// 参数 1
        /// </summary>
        public IValue Left { get; set; }

        /// <summary>
        /// 参数2
        /// </summary>
        public IValue Right { get; set; }

        /// <summary>
        /// 构建二元运算函数信息。
        /// </summary>
        /// <param name="left">参数 1</param>
        /// <param name="right">参数 2</param>
        public BinaryFunction(IValue left, IValue right)
        {
            Left = left;
            Right = right;
        }
    }
}