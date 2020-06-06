namespace Swifter.Data.Sql
{
    /// <summary>
    /// Avg 聚合函数信息。
    /// </summary>
    public sealed class AvgFunction : AggregateFunction
    {
        /// <summary>
        /// 构建 Avg 聚合函数信息
        /// </summary>
        /// <param name="value">聚合值</param>
        public AvgFunction(IValue value) : base(value)
        {
        }
    }
}