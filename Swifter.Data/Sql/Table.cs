namespace Swifter.Data.Sql
{
    /// <summary>
    /// 表信息。
    /// </summary>
    public sealed class Table : ITable
    {
        /// <summary>
        /// 表示不指定表名。
        /// </summary>
        public static readonly Table Empty = null;

        /// <summary>
        /// 构建表信息。
        /// </summary>
        /// <param name="name">表名</param>
        public Table(string name)
        {
            Name = name;
        }
        
        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 构建表信息。
        /// </summary>
        /// <param name="tableName">表名</param>
        public static implicit operator Table(string tableName) => new Table(tableName);
    }
}