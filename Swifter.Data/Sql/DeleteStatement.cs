namespace Swifter.Data.Sql
{
    /// <summary>
    /// Delete 语句信息
    /// </summary>
    public sealed class DeleteStatement
    {
        /// <summary>
        /// 构建 Delete 语句信息
        /// </summary>
        /// <param name="table">需要 Delete 的表</param>
        public DeleteStatement(Table table)
        {
            Table = table;

            Where = new Conditions();
        }

        /// <summary>
        /// 需要 Delete 的表。
        /// </summary>
        public Table Table { get; }

        /// <summary>
        /// Delete 条件。
        /// </summary>
        public Conditions Where { get; }
    }
}