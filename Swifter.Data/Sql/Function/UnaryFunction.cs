namespace Swifter.Data.Sql
{
    /// <summary>
    /// 一元函数。
    /// </summary>
    public abstract class UnaryFunction : IValue
    {
        /// <summary>
        /// 一元函数的参数。
        /// </summary>
        public IValue Value { get; set; }

        /// <summary>
        /// 构建一元函数。
        /// </summary>
        /// <param name="value">一元函数的参数</param>
        public UnaryFunction(IValue value)
        {
            Value = value;
        }
    }
}