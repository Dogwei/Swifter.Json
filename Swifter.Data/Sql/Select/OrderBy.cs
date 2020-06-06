namespace Swifter.Data.Sql
{
    /// <summary>
    /// 排序信息
    /// </summary>
    public sealed class OrderBy
    {
        /// <summary>
        /// 构建排序信息
        /// </summary>
        /// <param name="column">要排序的列</param>
        /// <param name="direction">排序方向</param>
        public OrderBy(Column column, OrderByDirections direction)
        {
            Column = column;
            Direction = direction;
        }

        /// <summary>
        /// 要排序的列。
        /// </summary>
        public Column Column { get; }

        /// <summary>
        /// 排序方向。
        /// </summary>
        public OrderByDirections Direction { get; }
    }
}