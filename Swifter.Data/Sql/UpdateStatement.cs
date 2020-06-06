namespace Swifter.Data.Sql
{
    /// <summary>
    /// Update 语句信息。
    /// </summary>
    public sealed class UpdateStatement
    {
        /// <summary>
        /// 构建 Update 语句信息。
        /// </summary>
        /// <param name="table">要 Update 的表</param>
        public UpdateStatement(Table table)
        {
            Table = table;

            Values = new AssignValues();

            Where = new Conditions();
        }

        /// <summary>
        /// 要 Update 的表。
        /// </summary>
        public Table Table { get; }

        /// <summary>
        /// 要赋值的列的集合。
        /// </summary>
        public AssignValues Values { get; }

        /// <summary>
        /// Update 条件。
        /// </summary>
        public Conditions Where { get; }
    }
}