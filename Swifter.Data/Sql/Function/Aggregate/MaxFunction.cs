namespace Swifter.Data.Sql
{
    /// <summary>
    /// Max 聚合函数信息。
    /// </summary>
    public sealed class MaxFunction : AggregateFunction
    {
        /// <summary>
        /// 构建 Max 聚合函数信息
        /// </summary>
        /// <param name="value">聚合值</param>
        public MaxFunction(IValue value) : base(value)
        {
        }
    }
}