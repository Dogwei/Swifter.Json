namespace Swifter.Data.Sql
{
    /// <summary>
    /// Count 聚合函数信息。
    /// </summary>
    public sealed class CountFunction : AggregateFunction
    {
        /// <summary>
        /// 构建 Count 聚合函数信息
        /// </summary>
        /// <param name="value">聚合值</param>
        public CountFunction(IValue value) : base(value)
        {
        }
    }
}