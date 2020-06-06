namespace Swifter.Data.Sql
{
    /// <summary>
    /// 列信息
    /// </summary>
    public sealed class Column : IValue
    {
        /// <summary>
        /// 构建列信息
        /// </summary>
        /// <param name="table">表</param>
        /// <param name="name">列名</param>
        public Column(ITable table, string name)
        {
            Table = table;
            Name = name;
        }

        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 表
        /// </summary>
        public ITable Table { get; set; }

        /// <summary>
        /// 构建一个未设置表的列
        /// </summary>
        /// <param name="columnName">列名</param>
        public static implicit operator Column(string columnName) => new Column(null, columnName);
    }
}