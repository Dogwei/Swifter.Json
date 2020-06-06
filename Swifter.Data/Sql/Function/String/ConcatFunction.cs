namespace Swifter.Data.Sql
{
    /// <summary>
    /// 拼接字符串函数。
    /// </summary>
    public sealed class ConcatFunction
    {
        /// <summary>
        /// 构建拼接字符串函数。
        /// </summary>
        /// <param name="values">值数组</param>
        public ConcatFunction(IValue[] values)
        {
            Values = values;
        }

        /// <summary>
        /// 值数组。
        /// </summary>
        public IValue[] Values { get; set; }
    }
}