namespace Swifter.Data.Sql
{
    /// <summary>
    /// Sum 聚合函数信息。
    /// </summary>
    public sealed class SumFunction : AggregateFunction
    {
        /// <summary>
        /// 构建 Sum 聚合函数信息
        /// </summary>
        /// <param name="value">聚合值</param>
        public SumFunction(IValue value) : base(value)
        {
        }
    }
}