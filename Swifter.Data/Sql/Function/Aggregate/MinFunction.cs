namespace Swifter.Data.Sql
{
    /// <summary>
    /// Min 聚合函数信息。
    /// </summary>
    public sealed class MinFunction : AggregateFunction
    {
        /// <summary>
        /// 构建 Min 聚合函数信息
        /// </summary>
        /// <param name="value">聚合值</param>
        public MinFunction(IValue value) : base(value)
        {
        }
    }
}