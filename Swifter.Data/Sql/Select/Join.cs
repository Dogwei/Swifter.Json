namespace Swifter.Data.Sql
{
    /// <summary>
    /// 表连接信息
    /// </summary>
    public sealed class Join
    {
        /// <summary>
        /// 构建表连接信息信息
        /// </summary>
        /// <param name="direction">连接方向</param>
        /// <param name="table">要连接的表</param>
        public Join(JoinDirections direction, ITable table)
        {
            Direction = direction;
            Table = table;

            On = new Conditions();
        }

        /// <summary>
        /// 构建表连接信息信息
        /// </summary>
        /// <param name="direction">连接方向</param>
        /// <param name="table">要连接的表</param>
        /// <param name="condition">连接条件</param>
        public Join(JoinDirections direction, ITable table, Condition condition) : this(direction, table)
        {
            On.Add(condition);
        }

        /// <summary>
        /// 要连接的表
        /// </summary>
        public ITable Table { get; }

        /// <summary>
        /// 连接条件
        /// </summary>
        public Conditions On { get; }

        /// <summary>
        /// 连接方向
        /// </summary>
        public JoinDirections Direction { get; }
    }
}