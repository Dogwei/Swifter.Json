namespace Swifter.Data.Sql
{
    /// <summary>
    /// 赋值信息
    /// </summary>
    public sealed class AssignValue
    {
        /// <summary>
        /// 初始化赋值信息
        /// </summary>
        /// <param name="column">要赋值的列</param>
        /// <param name="value">值</param>
        public AssignValue(Column column, IValue value)
        {
            Column = column;
            Value = value;
        }

        /// <summary>
        /// 要赋值的列
        /// </summary>
        public Column Column { get; }

        /// <summary>
        /// 值
        /// </summary>
        public IValue Value { get; }
    }
}