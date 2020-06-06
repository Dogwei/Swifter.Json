namespace Swifter.Data.Sql
{
    /// <summary>
    /// Select 语句信息。
    /// </summary>
    public sealed class SelectStatement : ITable, IValue
    {
        /// <summary>
        /// 构建 Select 语句信息
        /// </summary>
        /// <param name="mainTable">表</param>
        public SelectStatement(ITable mainTable)
        {
            Table = mainTable;

            Columns = new SelectColumns();
            Joins = new Joins();
            Where = new Conditions();
            OrderBies = new OrderBies();
            GroupBies = new GroupBies();
            Having = new Conditions();
        }

        /// <summary>
        /// 构建 Select 语句信息
        /// </summary>
        /// <param name="mainTable">表</param>
        public SelectStatement(Table mainTable) : this((ITable)mainTable)
        {
        }

        /// <summary>
        /// 需要查询的列的集合。
        /// </summary>
        public SelectColumns Columns { get; }

        /// <summary>
        /// 主表。
        /// </summary>
        public ITable Table { get; }

        /// <summary>
        /// 要关联的表集合。
        /// </summary>
        public Joins Joins { get; }

        /// <summary>
        /// 查询条件。
        /// </summary>
        public Conditions Where { get; }

        /// <summary>
        /// 排序列的集合。
        /// </summary>
        public OrderBies OrderBies { get; }

        /// <summary>
        /// 分组列的集合。
        /// </summary>
        public GroupBies GroupBies { get; }

        /// <summary>
        /// 分组查询的条件。
        /// </summary>
        public Conditions Having { get; }

        /// <summary>
        /// 结果集偏移行数。
        /// </summary>
        public int? Offset { get; set; }

        /// <summary>
        /// 结果集行数数量。
        /// </summary>
        public int? Limit { get; set; }
    }
}