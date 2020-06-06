namespace Swifter.Data.Sql
{
    /// <summary>
    /// 聚合函数
    /// </summary>
    public abstract class AggregateFunction : IValue
    {
        /// <summary>
        /// 聚合值
        /// </summary>
        public IValue Value { get; }

        /// <summary>
        /// 聚合函数必要的构造参数
        /// </summary>
        /// <param name="value">聚合值</param>
        protected AggregateFunction(IValue value)
        {
            Value = value;
        }
    }
}