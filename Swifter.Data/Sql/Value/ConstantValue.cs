namespace Swifter.Data.Sql
{
    /// <summary>
    /// 表示一个常量值。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ConstantValue<T> : IValue
    {
        /// <summary>
        /// 值
        /// </summary>
        public T Value { get; }

        /// <summary>
        /// 获取该值的字符串表现形式。
        /// </summary>
        /// <returns>返回一个字符串</returns>
        public override string ToString()
        {
            return Value?.ToString();
        }

        internal ConstantValue(T value)
        {
            Value = value;
        }
    }
}