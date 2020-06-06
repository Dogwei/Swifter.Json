using System;

namespace Swifter.Data.Sql
{
    /// <summary>
    /// 查询列。
    /// </summary>
    public sealed class SelectColumn
    {
        static readonly char[] separator = { ' ', '\b', '\f', '\n', '\t', '\r' };

        /// <summary>
        /// 尝试将查询列表达式解析为查询列信息。
        /// </summary>
        /// <param name="table">表达式所属表</param>
        /// <param name="expression">表达式</param>
        /// <param name="selectColumn">返回查询列信息</param>
        /// <returns>返回是否解析成功</returns>
        public static bool TryParse(ITable table, string expression, out SelectColumn selectColumn)
        {
            selectColumn = null;

            if (string.IsNullOrEmpty(expression))
            {
                return false;
            }

            var expressions = expression.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            if (expressions.Length == 3 && "AS".Equals(expressions[1], StringComparison.InvariantCultureIgnoreCase))
            {
                selectColumn = new SelectColumn(new Column(table, expressions[0]), expressions[2]);
            }
            else if (expressions.Length == 2)
            {
                selectColumn = new SelectColumn(new Column(table, expressions[0]), expressions[1]);
            }
            else if (expressions.Length == 1)
            {
                selectColumn = new SelectColumn(new Column(table, expressions[0]));
            }

            return true;
        }

        /// <summary>
        /// 构建查询列。
        /// </summary>
        /// <param name="column">列信息</param>
        /// <param name="alias">别名</param>
        public SelectColumn(IValue column, string alias = null)
        {
            Column = column;
            Alias = alias;
        }

        /// <summary>
        /// 列信息。
        /// </summary>
        public IValue Column { get; }

        /// <summary>
        /// 别名。
        /// </summary>
        public string Alias { get; set; }
    }
}