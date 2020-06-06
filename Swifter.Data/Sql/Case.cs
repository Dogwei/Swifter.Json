using System;
using System.Collections.Generic;

namespace Swifter.Data.Sql
{
    /// <summary>
    /// Case 语句。
    /// </summary>
    public sealed class Case : IValue
    {
        /// <summary>
        /// Case 项集合。
        /// </summary>
        public List<Item> Items { get; }

        /// <summary>
        /// 底值。
        /// </summary>
        public IValue Else { get; set; }

        /// <summary>
        /// 构建 Case 语句。
        /// </summary>
        public Case()
        {
            Else = SqlHelper.ValueOf(DBNull.Value);
            Items = new List<Item>();
        }

        /// <summary>
        /// Case 项。
        /// </summary>
        public sealed class Item
        {
            /// <summary>
            /// 条件。
            /// </summary>
            public Condition When { get; }

            /// <summary>
            /// 值。
            /// </summary>
            public IValue Then { get; }

            internal Item(Condition when, IValue then)
            {
                When = when;
                Then = then;
            }
        }
    }
}