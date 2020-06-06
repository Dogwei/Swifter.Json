using System;
using System.Collections.Generic;

namespace Swifter.Data.Sql
{
    /// <summary>
    /// Insert 语句信息。
    /// </summary>
    public sealed class InsertStatement
    {
        /// <summary>
        /// 构建 Insert 语句信息。
        /// </summary>
        /// <param name="table">需要 Insert 的表</param>
        public InsertStatement(Table table)
        {
            if (table is null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            Table = table;

            Values = new AssignValues();
        }

        /// <summary>
        /// 需要 Insert 的表。
        /// </summary>
        public Table Table { get; }

        /// <summary>
        /// 需要赋值的列。
        /// </summary>
        public AssignValues Values { get; }
    }
}