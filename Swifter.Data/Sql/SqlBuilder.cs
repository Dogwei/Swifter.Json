using Swifter.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Swifter.Data.Sql
{
    /// <summary>
    /// T-SQL 生成器父类。
    /// </summary>
    public abstract class SqlBuilder : IDisposable
    {
        #region - Constant -

        /// <summary>
        /// T
        /// </summary>
        public const string Code_T = "T";
        /// <summary>
        /// O
        /// </summary>
        public const string Code_O = "O";
        /// <summary>
        /// =
        /// </summary>
        public const string Code_Equal = "=";
        /// <summary>
        /// &lt;&gt;
        /// </summary>
        public const string Code_NotEqual = "<>";
        /// <summary>
        /// &gt;
        /// </summary>
        public const string Code_GreaterThan = ">";
        /// <summary>
        /// &lt;
        /// </summary>
        public const string Code_LessThan = "<";
        /// <summary>
        /// ||
        /// </summary>
        public const string Code_DoubleVertical = "||";
        /// <summary>
        /// SELECT
        /// </summary>
        public const string Code_Select = "SELECT";
        /// <summary>
        /// 1 = 1
        /// </summary>
        public const string Code_True_Expression = "1 = 1";
        /// <summary>
        /// 1 &lt;&gt; 1
        /// </summary>
        public const string Code_False_Expression = "1 <> 1";
        /// <summary>
        /// %
        /// </summary>
        public const string Code_Percent = "%";
        /// <summary>
        /// NOT
        /// </summary>
        public const string Code_Not = "NOT";
        /// <summary>
        /// IS
        /// </summary>
        public const string Code_Is = "IS";
        /// <summary>
        /// AND
        /// </summary>
        public const string Code_And = "AND";
        /// <summary>
        /// ORDER
        /// </summary>
        public const string Code_Order = "ORDER";
        /// <summary>
        /// GROUP
        /// </summary>
        public const string Code_Group = "GROUP";
        /// <summary>
        /// HAVING
        /// </summary>
        public const string Code_Having = "HAVING";
        /// <summary>
        /// \r\n
        /// </summary>
        public const string Code_WrapLine = "\r\n";
        /// <summary>
        /// LIKE
        /// </summary>
        public const string Code_Like = "LIKE";
        /// <summary>
        /// BY
        /// </summary>
        public const string Code_By = "BY";
        /// <summary>
        /// ASC
        /// </summary>
        public const string Code_ASC = "ASC";
        /// <summary>
        /// AS
        /// </summary>
        public const string Code_As = "AS";
        /// <summary>
        /// DESC
        /// </summary>
        public const string Code_DESC = "DESC";
        /// <summary>
        /// OR
        /// </summary>
        public const string Code_Or = "OR";
        /// <summary>
        /// NULL
        /// </summary>
        public const string Code_Null = "NULL";
        /// <summary>
        /// FROM
        /// </summary>
        public const string Code_From = "FROM";
        /// <summary>
        /// WHERE
        /// </summary>
        public const string Code_Where = "WHERE";
        /// <summary>
        /// LEFT
        /// </summary>
        public const string Code_Left = "LEFT";
        /// <summary>
        /// RIGHT
        /// </summary>
        public const string Code_Right = "RIGHT";
        /// <summary>
        /// INNER
        /// </summary>
        public const string Code_Inner = "INNER";
        /// <summary>
        /// OUTER
        /// </summary>
        public const string Code_Outer = "OUTER";
        /// <summary>
        /// Full
        /// </summary>
        public const string Code_Full = "FULL";
        /// <summary>
        /// ALL
        /// </summary>
        public const string Code_All = "ALL";
        /// <summary>
        /// JOIN
        /// </summary>
        public const string Code_Join = "JOIN";
        /// <summary>
        /// ON
        /// </summary>
        public const string Code_On = "ON";
        /// <summary>
        /// ON
        /// </summary>
        public const string Code_Concat = "CONCAT";
        /// <summary>
        /// CAST
        /// </summary>
        public const string Code_Cast = "CAST";
        /// <summary>
        ///  
        /// </summary>
        public const string Code_Space = " ";
        /// <summary>
        /// "
        /// </summary>
        public const string Code_DoubleQuote = "\"";
        /// <summary>
        /// '
        /// </summary>
        public const string Code_SingleQuote = "'";
        /// <summary>
        /// ,
        /// </summary>
        public const string Code_Comma = ",";
        /// <summary>
        /// ;
        /// </summary>
        public const string Code_Semicolon = ";";
        /// <summary>
        /// .
        /// </summary>
        public const string Code_Dot = ".";
        /// <summary>
        /// *
        /// </summary>
        public const string Code_Asterisk = "*";
        /// <summary>
        /// &amp;
        /// </summary>
        public const string Code_Amp = "&";
        /// <summary>
        /// |
        /// </summary>
        public const string Code_Vertical = "|";
        /// <summary>
        /// [
        /// </summary>
        public const string Code_Square_Brackets_Begin = "[";
        /// <summary>
        /// ]
        /// </summary>
        public const string Code_Square_Brackets_End = "]";
        /// <summary>
        /// (
        /// </summary>
        public const string Code_Parenthesis_Bracket_Begin = "(";
        /// <summary>
        /// )
        /// </summary>
        public const string Code_Parenthesis_Bracket_End = ")";
        /// <summary>
        /// {
        /// </summary>
        public const string Code_Angle_Bracket_Begin = "{";
        /// <summary>
        /// }
        /// </summary>
        public const string Code_Angle_Bracket_End = "}";
        /// <summary>
        /// OFFSET
        /// </summary>
        public const string Code_Offset = "OFFSET";
        /// <summary>
        /// ROWS
        /// </summary>
        public const string Code_Rows = "ROWS";
        /// <summary>
        /// FETCH
        /// </summary>
        public const string Code_Fetch = "FETCH";
        /// <summary>
        /// NEXT
        /// </summary>
        public const string Code_Next = "NEXT";
        /// <summary>
        /// ONLY
        /// </summary>
        public const string Code_Only = "ONLY";
        /// <summary>
        /// LIMIT
        /// </summary>
        public const string Code_Limit = "LIMIT";
        /// <summary>
        /// IN
        /// </summary>
        public const string Code_In = "IN";
        /// <summary>
        /// BETWEEN
        /// </summary>
        public const string Code_Between = "BETWEEN";
        /// <summary>
        /// CREATE
        /// </summary>
        public const string Code_Create = "CREATE";
        /// <summary>
        /// DELETE
        /// </summary>
        public const string Code_Delete = "DELETE";
        /// <summary>
        /// UPDATE
        /// </summary>
        public const string Code_Update = "UPDATE";
        /// <summary>
        /// SET
        /// </summary>
        public const string Code_Set = "SET";
        /// <summary>
        /// INSERT
        /// </summary>
        public const string Code_Insert = "INSERT";
        /// <summary>
        /// INTO
        /// </summary>
        public const string Code_Into = "INTO";
        /// <summary>
        /// VALUES
        /// </summary>
        public const string Code_Values = "VALUES";
        /// <summary>
        /// DROP
        /// </summary>
        public const string Code_Drop = "DROP";
        /// <summary>
        /// TABLE
        /// </summary>
        public const string Code_Table = "TABLE";
        /// <summary>
        /// INDEX
        /// </summary>
        public const string Code_Index = "INDEX";
        /// <summary>
        /// WHEN
        /// </summary>
        public const string Code_When = "WHEN";
        /// <summary>
        /// THEN
        /// </summary>
        public const string Code_Then = "THEN";
        /// <summary>
        /// CASE
        /// </summary>
        public const string Code_Case = "CASE";
        /// <summary>
        /// END
        /// </summary>
        public const string Code_End = "END";
        /// <summary>
        /// TOP
        /// </summary>
        public const string Code_Top = "TOP";
        /// <summary>
        /// 1
        /// </summary>
        public const string Code_One = "1";
        /// <summary>
        /// 0
        /// </summary>
        public const string Code_Zero = "0";
        /// <summary>
        /// SUM
        /// </summary>
        public const string Code_Sum = "SUM";
        /// <summary>
        /// AVG
        /// </summary>
        public const string Code_Avg = "AVG";
        /// <summary>
        /// COUNT
        /// </summary>
        public const string Code_Count = "COUNT";
        /// <summary>
        /// MAX
        /// </summary>
        public const string Code_Max = "MAX";
        /// <summary>
        /// MIN
        /// </summary>
        public const string Code_Min = "MIN";
        /// <summary>
        /// UPEER
        /// </summary>
        public const string Code_Upper = "UPPER";
        /// <summary>
        /// LOWER
        /// </summary>
        public const string Code_Lower = "LOWER";
        /// <summary>
        /// +
        /// </summary>
        public const string Code_Add = "+";
        /// <summary>
        /// -
        /// </summary>
        public const string Code_Subtract = "-";
        /// <summary>
        /// *
        /// </summary>
        public const string Code_Multiply = "*";
        /// <summary>
        /// /
        /// </summary>
        public const string Code_Divide = "/";

        #endregion

        /// <summary>
        /// 获取 SQL 生成器的全局缓存池。
        /// </summary>
        public static readonly HGlobalCachePool<char> CharsPool = HGlobalCacheExtensions.CharsPool;

        /// <summary>
        /// 十六进制进阶数字。
        /// </summary>
        static readonly int[] HexDigital = { 0xf, 0xf, 0xf0, 0xf00, 0xf000, 0xf0000, 0xf00000, 0xf000000 };

        /// <summary>
        /// %
        /// </summary>
        static readonly ConstantValue<string> Value_Percent = new ConstantValue<string>(Code_Percent);

        /// <summary>
        /// 参数集合。
        /// </summary>
        public readonly SqlBuilderParameters Parameters;

        /// <summary>
        /// T-SQL 缓存。
        /// </summary>
        internal protected readonly HGlobalCache<char> Builder;

        /// <summary>
        /// 别名集合。
        /// </summary>
        internal protected readonly Dictionary<object, string> Aliases;

        /// <summary>
        /// 当前别名序号。
        /// </summary>
        int AliasNum;

        /// <summary>
        /// 0 表示未释放，1 表示已释放。
        /// </summary>
        int Disposed = 0;

        /// <summary>
        /// 初始化必要的构造参数。
        /// </summary>
        protected SqlBuilder()
        {
            Aliases = new Dictionary<object, string>(TypeHelper.ReferenceComparer);
            Parameters = new SqlBuilderParameters();

            Builder = CharsPool.Rent();

            AliasNum = 0;
        }

        /// <summary>
        /// 获取已生成的 T-SQL 语句。
        /// </summary>
        /// <returns>返回 T-SQL 语句</returns>
        public string ToSql()
        {
            return Builder.ToStringEx();
        }

        /// <summary>
        /// 清空当前实例的信息。
        /// </summary>
        public void Clear()
        {
            Aliases.Clear();
            Parameters.ClearParameters();
            Builder.Clear();

            AliasNum = 0;
        }

        /// <summary>
        /// 释放此 SQL 生成器的资源。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// 释放此 SQL 生成器的资源。
        /// </summary>
        /// <param name="disposing">是否释放托管资源</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Interlocked.Exchange(ref Disposed, 1) == 0)
            {
                CharsPool.Return(Builder);
            }
        }

        /// <summary>
        /// 分配对象别名。
        /// 注: 有别名的表中的列会自动附带表的别名。
        /// </summary>
        /// <param name="obj">对象</param>
        public void MakeAlias(object obj)
        {
            Aliases.TryAdd(obj, $"{(obj is ITable ? Code_T : Code_O)}{++AliasNum}");
        }

        /// <summary>
        /// 获取对象别名。
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>返回该对象的别名或 Null</returns>
        public string GetAlias(object obj)
        {
            if (Aliases.TryGetValue(obj, out var alias))
            {
                return alias;
            }

            return null;
        }

        /// <summary>
        /// 向后拼接一个名称。
        /// </summary>
        /// <param name="name">名称</param>
        public abstract void BuildName(string name);

        /// <summary>
        /// 向后拼接一个参数。
        /// </summary>
        /// <param name="name">参数名称</param>
        public abstract void BuildParameter(string name);

        /// <summary>
        /// 向后拼接一个语句结尾。
        /// </summary>
        public abstract void BuildStatementEnd();

        /// <summary>
        /// 向后拼接 Select 的 Top 语句。
        /// </summary>
        /// <param name="selectStatement"></param>
        public virtual void BuildSelectTop(SelectStatement selectStatement)
        {

        }

        /// <summary>
        /// 向后拼接一个原生语句。
        /// </summary>
        /// <param name="nativeStatement"></param>
        public virtual void BuildNativeStatement(NativeStatement nativeStatement)
        {
            Builder.Append(nativeStatement.Sql);
        }

        #region - Select -

        /// <summary>
        /// 向后拼接 Select 语句
        /// </summary>
        /// <param name="selectStatement">Select 语句信息</param>
        public virtual void BuildSelect(SelectStatement selectStatement)
        {
            // Initialize
            if (selectStatement.Offset != null || selectStatement.Limit != null)
            {
                if (selectStatement.OrderBies != null && selectStatement.OrderBies.Count == 0)
                {
                    selectStatement.OrderBies.Add(GetDefaultOrderBy(selectStatement));
                }
            }

            MakeAlias(selectStatement.Table);

            if (selectStatement.Joins != null && selectStatement.Joins.Count != 0)
            {
                foreach (var item in selectStatement.Joins)
                {
                    MakeAlias(item.Table);
                }
            }

            // SELECT

            Builder.Append(Code_Select);

            BuildSelectTop(selectStatement);

            if (selectStatement.Columns != null && selectStatement.Columns.Count != 0)
            {
                Builder.Append(Code_Space);

                BuildSelectColumns(selectStatement.Columns);
            }
            else
            {
                Builder.Append(Code_Space);
                Builder.Append(Code_Asterisk);
            }

            if (selectStatement.Table != null)
            {
                // FROM
                Builder.Append(Code_Space);

                Builder.Append(Code_From);

                Builder.Append(Code_Space);

                BuildTable(selectStatement.Table);

                var alias = GetAlias(selectStatement.Table);

                if (alias != null)
                {
                    Builder.Append(Code_Space);

                    BuildName(alias);
                }

                if (selectStatement.Joins != null && selectStatement.Joins.Count != 0)
                {
                    foreach (var join in selectStatement.Joins)
                    {
                        BuildJoin(join);
                    }
                }
            }

            // WHERE
            if (selectStatement.Where != null && selectStatement.Where.Count != 0)
            {
                Builder.Append(Code_Space);

                Builder.Append(Code_Where);

                Builder.Append(Code_Space);

                BuildConditions(selectStatement.Where);
            }

            // GROUP BY
            if (selectStatement.GroupBies != null && selectStatement.GroupBies.Count != 0)
            {
                Builder.Append(Code_Space);

                Builder.Append(Code_Group);

                Builder.Append(Code_Space);

                Builder.Append(Code_By);

                Builder.Append(Code_Space);

                BuildGroupBies(selectStatement.GroupBies);

                // HAVING
                if (selectStatement.Having != null && selectStatement.Having.Count != 0)
                {
                    Builder.Append(Code_Space);

                    Builder.Append(Code_Having);

                    Builder.Append(Code_Space);

                    BuildConditions(selectStatement.Having);
                }
            }

            var buildLimit = false;

            // ORDER BY
            if (selectStatement.OrderBies != null && selectStatement.OrderBies.Count != 0)
            {
                buildLimit = true;

                Builder.Append(Code_Space);

                Builder.Append(Code_Order);

                Builder.Append(Code_Space);

                Builder.Append(Code_By);

                Builder.Append(Code_Space);

                BuildOrderBies(selectStatement.OrderBies);
            }

            if (buildLimit && (selectStatement.Offset != null || selectStatement.Limit != null))
            {
                BuildSelectLimit(selectStatement.Offset, selectStatement.Limit);
            }
        }

        /// <summary>
        /// 向后拼接 Select 语句，并拼接语句结尾。
        /// </summary>
        /// <param name="selectStatement">Select 语句信息</param>
        public virtual void BuildSelectStatement(SelectStatement selectStatement)
        {
            BuildSelect(selectStatement);

            BuildStatementEnd();
        }

        /// <summary>
        /// 向后拼接 Limit 参数。
        /// </summary>
        /// <param name="offset">偏移行数</param>
        /// <param name="limit">行数数量</param>
        public abstract void BuildSelectLimit(int? offset, int? limit);

        #endregion

        #region - Select Column -


        /// <summary>
        /// 向后拼接 Select 列。
        /// </summary>
        /// <param name="selectColumn">Select 列信息</param>
        public virtual void BuildSelectColumn(SelectColumn selectColumn)
        {
            BuildValue(selectColumn.Column);

            if (!string.IsNullOrEmpty(selectColumn.Alias))
            {
                Builder.Append(Code_Space);

                BuildName(selectColumn.Alias);
            }
        }

        /// <summary>
        /// 向后拼接 Select 列集合。
        /// </summary>
        /// <param name="selectColumns">Select 列集合</param>
        public virtual void BuildSelectColumns(SelectColumns selectColumns)
        {
            var isFirst = true;

            foreach (var item in selectColumns)
            {
                if (!isFirst)
                {
                    Builder.Append(Code_Comma);

                    Builder.Append(Code_Space);
                }

                BuildSelectColumn(item);

                isFirst = false;
            }
        }

        #endregion

        #region - Insert -

        /// <summary>
        /// 向后拼接 Insert 语句。
        /// </summary>
        /// <param name="insertStatement">Insert 语句信息</param>
        public virtual void BuildInsertStatement(InsertStatement insertStatement)
        {
            if (insertStatement.Values != null && insertStatement.Values.Count > 0)
            {
                Builder.Append(Code_Insert);

                Builder.Append(Code_Space);

                BuildTable(insertStatement.Table);

                Builder.Append(Code_Parenthesis_Bracket_Begin);

                var isFirst = true;

                foreach (var item in insertStatement.Values)
                {
                    if (!isFirst)
                    {
                        Builder.Append(Code_Comma);

                        Builder.Append(Code_Space);
                    }

                    BuildColumn(item.Column);

                    isFirst = false;
                }

                Builder.Append(Code_Parenthesis_Bracket_End);

                Builder.Append(Code_Space);

                Builder.Append(Code_Values);

                Builder.Append(Code_Parenthesis_Bracket_Begin);

                isFirst = true;

                foreach (var item in insertStatement.Values)
                {
                    if (!isFirst)
                    {
                        Builder.Append(Code_Comma);

                        Builder.Append(Code_Space);
                    }

                    BuildValue(item.Value);

                    isFirst = false;
                }


                Builder.Append(Code_Parenthesis_Bracket_End);

                BuildStatementEnd();
            }
        }

        #endregion

        #region - Update -

        /// <summary>
        /// 向后拼接 Update 语句。
        /// </summary>
        /// <param name="updateStatement">Update 语句信息</param>
        public virtual void BuildUpdateStatement(UpdateStatement updateStatement)
        {
            if (updateStatement.Values != null && updateStatement.Values.Count != 0)
            {
                Builder.Append(Code_Update);

                Builder.Append(Code_Space);

                BuildTable(updateStatement.Table);

                Builder.Append(Code_Space);

                Builder.Append(Code_Set);

                Builder.Append(Code_Space);

                var isFirst = true;

                foreach (var item in updateStatement.Values)
                {
                    if (!isFirst)
                    {
                        Builder.Append(Code_Comma);

                        Builder.Append(Code_Space);
                    }

                    BuildColumn(item.Column);

                    Builder.Append(Code_Space);

                    Builder.Append(Code_Equal);

                    Builder.Append(Code_Space);

                    BuildValue(item.Value);

                    isFirst = false;
                }

                if (updateStatement.Where != null && updateStatement.Where.Count != 0)
                {
                    Builder.Append(Code_Space);

                    Builder.Append(Code_Where);

                    Builder.Append(Code_Space);

                    BuildConditions(updateStatement.Where);
                }

                BuildStatementEnd();
            }
        }

        #endregion

        #region - Delete -

        /// <summary>
        /// 向后拼接 Delete 语句。
        /// </summary>
        /// <param name="deleteStatement">Delete 语句信息</param>
        public virtual void BuildDeleteStatement(DeleteStatement deleteStatement)
        {
            Builder.Append(Code_Delete);

            Builder.Append(Code_Space);

            BuildTable(deleteStatement.Table);

            if (deleteStatement.Where != null && deleteStatement.Where.Count != 0)
            {
                Builder.Append(Code_Space);

                Builder.Append(Code_Where);

                Builder.Append(Code_Space);

                BuildConditions(deleteStatement.Where);
            }

            BuildStatementEnd();
        }

        #endregion

        #region - Column -

        /// <summary>
        /// 向后拼接一个列。
        /// </summary>
        /// <param name="column">列信息</param>
        public virtual void BuildColumn(Column column)
        {
            string alias;

            if (column.Table != null && (alias = GetAlias(column.Table)) != null)
            {
                BuildName(alias);

                Builder.Append(Code_Dot);
            }

            BuildName(column.Name);
        }

        #endregion

        #region - Table -

        /// <summary>
        /// 向后拼接一个表。
        /// </summary>
        /// <param name="table">表信息</param>
        public virtual void BuildTable(Table table)
        {
            BuildName(table.Name);
        }

        /// <summary>
        /// 向后拼接一个子查询的表。
        /// </summary>
        /// <param name="selectStatement">子查询信息</param>
        public virtual void BuildTable(SelectStatement selectStatement)
        {
            Builder.Append(Code_Parenthesis_Bracket_Begin);

            BuildSelect(selectStatement);

            Builder.Append(Code_Parenthesis_Bracket_End);

            MakeAlias(selectStatement);
        }

        /// <summary>
        /// 向后拼接一个原生语句。
        /// </summary>
        /// <param name="nativeStatement"></param>
        public virtual void BuildTable(NativeStatement nativeStatement)
        {
            Builder.Append(Code_Parenthesis_Bracket_Begin);

            Builder.Append(nativeStatement.Sql);

            Builder.Append(Code_Parenthesis_Bracket_End);

            MakeAlias(nativeStatement);
        }

        /// <summary>
        /// 向后拼接一个表格。
        /// </summary>
        /// <param name="table"></param>
        public void BuildTable(ITable table)
        {
            if (table is Table t)
            {
                BuildTable(t);
            }
            else if (table is SelectStatement ss)
            {
                BuildTable(ss);
            }
            else if (table is NativeStatement ns)
            {
                BuildTable(ns);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        #region - Value -

        /// <summary>
        /// 向后拼接一个 Int32 值。
        /// </summary>
        /// <param name="value">Int32</param>
        public virtual void BuildValue(ConstantValue<int> value) => Builder.Append(value.Value);

        /// <summary>
        /// 向后拼接一个 Int64 值。
        /// </summary>
        /// <param name="value">Int64</param>
        public virtual void BuildValue(ConstantValue<long> value) => Builder.Append(value.Value);

        /// <summary>
        /// 向后拼接一个 Float 值。
        /// </summary>
        /// <param name="value">Float</param>
        public virtual void BuildValue(ConstantValue<float> value) => Builder.Append(value.Value);

        /// <summary>
        /// 向后拼接一个 Double 值。
        /// </summary>
        /// <param name="value">Double</param>
        public virtual void BuildValue(ConstantValue<double> value) => Builder.Append(value.Value);

        /// <summary>
        /// 向后拼接一个 Boolean 值。
        /// </summary>
        /// <param name="value">Boolean</param>
        public virtual void BuildValue(ConstantValue<bool> value) => Builder.Append(value.Value ? "1" : "0");

        /// <summary>
        /// 向后拼接一个 Decimal 值。
        /// </summary>
        /// <param name="value">Decimal</param>
        public virtual void BuildValue(ConstantValue<decimal> value) => Builder.Append(value.Value);

        /// <summary>
        /// 向后拼接一个 Int16 值。
        /// </summary>
        /// <param name="value">Int16</param>
        public virtual void BuildValue(ConstantValue<short> value) => Builder.Append(value.Value);

        /// <summary>
        /// 向后拼接一个 Byte 值。
        /// </summary>
        /// <param name="value">Byte</param>
        public virtual void BuildValue(ConstantValue<byte> value) => Builder.Append(value.Value);

        /// <summary>
        /// 向后拼接一个 String 值。
        /// </summary>
        /// <param name="value">String</param>
        public abstract void BuildValue(ConstantValue<string> value);

        /// <summary>
        /// 向后拼接一个 byte[] 值。
        /// </summary>
        /// <param name="value"></param>
        public unsafe virtual void BuildValue(ConstantValue<byte[]> value)
        {
            if (value.Value == null)
            {
                BuildValue(SqlHelper.ValueOf(DBNull.Value));

                return;
            }

            Builder.Append("0x");

            foreach (var item in value.Value)
            {
                Builder.Grow(16);

                NumberHelper.Hex.AppendD2(Builder.First + Builder.Count, item);

                Builder.Count += 2;

                // Builder.Append(item, radix: 16, fix: 2);
            }
        }

        /// <summary>
        /// 向后拼接一个 DateTime 值。
        /// </summary>
        /// <param name="value">DateTime</param>
        public virtual void BuildValue(ConstantValue<DateTime> value) => BuildValue(new ConstantValue<string>(value.Value.ToString("O")));

        /// <summary>
        /// 向后拼接一个 DateTimeOffset 值。
        /// </summary>
        /// <param name="value">DateTimeOffset</param>
        public virtual void BuildValue(ConstantValue<DateTimeOffset> value) => BuildValue(new ConstantValue<string>(value.Value.ToString("O")));

        /// <summary>
        /// 向后拼接一个 Guid 值。
        /// </summary>
        /// <param name="value">Guid</param>
        public virtual void BuildValue(ConstantValue<Guid> value) => BuildValue(new ConstantValue<string>(value.Value.ToString("D")));

        /// <summary>
        /// 向后拼接一个数组值。
        /// </summary>
        /// <param name="value">数组</param>
        public virtual void BuildValue(ConstantValue<IValue[]> value)
        {
            if (value.Value != null && value.Value.Length != 0)
            {
                var isFirst = true;

                foreach (var item in value.Value)
                {
                    if (!isFirst)
                    {
                        Builder.Append(Code_Comma);

                        Builder.Append(Code_Space);
                    }

                    BuildValue(item);

                    isFirst = false;
                }
            }
            else
            {
                BuildValue(SqlHelper.ValueOf(null));
            }
        }

        /// <summary>
        /// 向后拼接一个 DbNull。
        /// </summary>
        /// <param name="value">空值</param>
        public virtual void BuildValue(ConstantValue<DBNull> value) => Builder.Append(Code_Null);

        /// <summary>
        /// 向后拼接一个后期绑定的值。
        /// </summary>
        /// <param name="value">后期绑定的值</param>
        public void BuildValue(VariableValue value) => BuildValue(value.Value);

        /// <summary>
        /// 向后拼接一个列值。
        /// </summary>
        /// <param name="column">列信息</param>
        public virtual void BuildValue(Column column)
        {
            string alias;

            if (column.Table != null && (alias = GetAlias(column.Table)) != null)
            {
                BuildName(alias);

                Builder.Append(Code_Dot);
            }

            BuildName(column.Name);
        }

        /// <summary>
        /// 向后拼接一个条件结果值。
        /// </summary>
        /// <param name="condition"></param>
        public virtual void BuildValue(Condition condition)
        {
            var withBracket = false;

            switch (condition.Comparison)
            {
                case Comparisons.And:
                case Comparisons.Or:
                    withBracket = true;
                    break;
            }

            if (withBracket)
            {
                Builder.Append(Code_Parenthesis_Bracket_Begin);
            }

            BuildCondition(condition);

            if (withBracket)
            {
                Builder.Append(Code_Parenthesis_Bracket_End);
            }
        }

        /// <summary>
        /// 向后拼接一个 Count 聚合列。
        /// </summary>
        /// <param name="count">Count 聚合信息</param>
        public virtual void BuildValue(CountFunction count)
        {
            Builder.Append(Code_Count);

            Builder.Append(Code_Parenthesis_Bracket_Begin);

            BuildValue(count.Value);

            Builder.Append(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个 Sum 聚合列。
        /// </summary>
        /// <param name="sum">Sum 聚合信息</param>
        public virtual void BuildValue(SumFunction sum)
        {
            Builder.Append(Code_Sum);

            Builder.Append(Code_Parenthesis_Bracket_Begin);

            BuildValue(sum.Value);

            Builder.Append(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个 Avg 聚合列。
        /// </summary>
        /// <param name="avg">Avg 聚合信息</param>
        public virtual void BuildValue(AvgFunction avg)
        {
            Builder.Append(Code_Avg);

            Builder.Append(Code_Parenthesis_Bracket_Begin);

            BuildValue(avg.Value);

            Builder.Append(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个 Max 聚合列。
        /// </summary>
        /// <param name="max">Max 聚合信息</param>
        public virtual void BuildValue(MaxFunction max)
        {
            Builder.Append(Code_Max);

            Builder.Append(Code_Parenthesis_Bracket_Begin);

            BuildValue(max.Value);

            Builder.Append(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个 Min 聚合列。
        /// </summary>
        /// <param name="min">Min 聚合信息</param>
        public virtual void BuildValue(MinFunction min)
        {
            Builder.Append(Code_Min);

            Builder.Append(Code_Parenthesis_Bracket_Begin);

            BuildValue(min.Value);

            Builder.Append(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个 Upper 函数。
        /// </summary>
        /// <param name="upper">Upper 函数信息</param>
        public virtual void BuildValue(UpperFunction upper)
        {
            Builder.Append(Code_Upper);

            Builder.Append(Code_Parenthesis_Bracket_Begin);

            BuildValue(upper.Value); ;

            Builder.Append(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个 Lower 函数。
        /// </summary>
        /// <param name="lower">Lower 函数信息</param>
        public virtual void BuildValue(LowerFunction lower)
        {
            Builder.Append(Code_Lower);

            Builder.Append(Code_Parenthesis_Bracket_Begin);

            BuildValue(lower.Value);

            Builder.Append(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个 Add 函数。
        /// </summary>
        /// <param name="add"></param>
        public virtual void BuildValue(AddFunction add)
        {
            BuildValue(add.Left);

            Builder.Append(Code_Space);
            Builder.Append(Code_Add);
            Builder.Append(Code_Space);

            BuildValue(add.Right);
        }

        /// <summary>
        /// 向后拼接一个 Subtract 函数。
        /// </summary>
        /// <param name="subtract"></param>
        public virtual void BuildValue(SubtractFunction subtract)
        {
            BuildValue(subtract.Left);

            Builder.Append(Code_Space);
            Builder.Append(Code_Subtract);
            Builder.Append(Code_Space);

            BuildValue(subtract.Right);
        }

        /// <summary>
        /// 向后拼接一个 Multiply 函数。
        /// </summary>
        /// <param name="multiply"></param>
        public virtual void BuildValue(MultiplyFunction multiply)
        {
            BuildValue(multiply.Left);

            Builder.Append(Code_Space);
            Builder.Append(Code_Multiply);
            Builder.Append(Code_Space);

            BuildValue(multiply.Right);
        }

        /// <summary>
        /// 向后拼接一个 Divide 函数。
        /// </summary>
        /// <param name="divide"></param>
        public virtual void BuildValue(DivideFunction divide)
        {
            BuildValue(divide.Left);

            Builder.Append(Code_Space);
            Builder.Append(Code_Divide);
            Builder.Append(Code_Space);

            BuildValue(divide.Right);
        }

        /// <summary>
        /// 向后拼接一个子查询语句。
        /// </summary>
        /// <param name="selectStatement">子查询语句</param>
        public virtual void BuildValue(SelectStatement selectStatement)
        {
            Builder.Append(Code_Parenthesis_Bracket_Begin);

            BuildSelect(selectStatement);

            Builder.Append(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个原生语句。
        /// </summary>
        /// <param name="nativeStatement"></param>
        public void BuildValue(NativeStatement nativeStatement)
        {
            Builder.Append(Code_Parenthesis_Bracket_Begin);

            Builder.Append(nativeStatement.Sql);

            Builder.Append(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个 SQL 值。
        /// </summary>
        /// <param name="value">SQL 值</param>
        public void BuildValue(IValue value)
        {
            if (value is null) BuildValue(SqlHelper.DBNull);
            else if (value is ConstantValue<DBNull>) BuildValue(SqlHelper.DBNull);
            else if (value is ConstantValue<int> @int) BuildValue(@int);
            else if (value is ConstantValue<long> @long) BuildValue(@long);
            else if (value is ConstantValue<float> @float) BuildValue(@float);
            else if (value is ConstantValue<double> @double) BuildValue(@double);
            else if (value is ConstantValue<decimal> @decimal) BuildValue(@decimal);
            else if (value is ConstantValue<short> @short) BuildValue(@short);
            else if (value is ConstantValue<byte> @byte) BuildValue(@byte);
            else if (value is ConstantValue<string> @string) BuildValue(@string);
            else if (value is ConstantValue<byte[]> bytearray) BuildValue(bytearray);
            else if (value is ConstantValue<DateTime> dt) BuildValue(dt);
            else if (value is ConstantValue<DateTimeOffset> dto) BuildValue(dto);
            else if (value is ConstantValue<Guid> guid) BuildValue(guid);
            else if (value is ConstantValue<IValue[]> array) BuildValue(array);
            else if (value is VariableValue @var) BuildValue(@var);
            else if (value is Column column) BuildValue(column);
            else if (value is Condition condition) BuildValue(condition);
            else if (value is CountFunction count) BuildValue(count);
            else if (value is SumFunction sum) BuildValue(sum);
            else if (value is AvgFunction avg) BuildValue(avg);
            else if (value is MaxFunction max) BuildValue(max);
            else if (value is MinFunction min) BuildValue(min);
            else if (value is UpperFunction upper) BuildValue(upper);
            else if (value is LowerFunction lower) BuildValue(lower);
            else if (value is AddFunction add) BuildValue(add);
            else if (value is SubtractFunction subtract) BuildValue(subtract);
            else if (value is MultiplyFunction multiply) BuildValue(multiply);
            else if (value is DivideFunction divide) BuildValue(divide);
            else if (value is SelectStatement ss) BuildValue(ss);
            else if (value is NativeStatement ns) BuildValue(ns);
            else if (value.GetType().IsGenericType && value.GetType().GetGenericTypeDefinition() == typeof(ConstantValue<>)) BuildValue(SqlHelper.ConstantValue(value.ToString()));
            else throw new NotSupportedException();
        }

        #endregion

        #region - Group By -

        /// <summary>
        /// 向后拼接一个分组列。
        /// </summary>
        /// <param name="column">列信息</param>
        public virtual void BuildGroupBy(Column column)
        {
            string alias;

            if (column.Table != null && (alias = GetAlias(column.Table)) != null)
            {
                BuildName(alias);

                Builder.Append(Code_Dot);
            }

            BuildName(column.Name);
        }

        /// <summary>
        /// 向后拼接一个分组集合。
        /// </summary>
        /// <param name="columns"></param>
        public virtual void BuildGroupBies(GroupBies columns)
        {
            var isFirst = true;

            foreach (var item in columns)
            {
                if (!isFirst)
                {
                    Builder.Append(Code_Comma);

                    Builder.Append(Code_Space);
                }

                BuildGroupBy(item);

                isFirst = false;
            }
        }

        #endregion

        #region - Order By -

        /// <summary>
        /// 当没有为查询语句设置排序列是获取它的默认排序列。
        /// </summary>
        /// <param name="selectStatement">查询语句信息</param>
        /// <returns>返回一个排序列信息</returns>
        public abstract OrderBy GetDefaultOrderBy(SelectStatement selectStatement);

        /// <summary>
        /// 向后拼接一个排序列。
        /// </summary>
        /// <param name="orderBy">排序列信息</param>
        public virtual void BuildOrderBy(OrderBy orderBy)
        {
            string alias;

            if (orderBy.Column.Table != null && (alias = GetAlias(orderBy.Column.Table)) != null)
            {
                BuildName(alias);

                Builder.Append(Code_Dot);
            }

            BuildName(orderBy.Column.Name);

            BuildOrderByDirections(orderBy.Direction);
        }

        /// <summary>
        /// 向后拼接一个排序集合。
        /// </summary>
        /// <param name="orders">排序集合</param>
        public virtual void BuildOrderBies(OrderBies orders)
        {
            var isFirst = true;

            foreach (var item in orders)
            {
                if (!isFirst)
                {
                    Builder.Append(Code_Comma);

                    Builder.Append(Code_Space);
                }

                BuildOrderBy(item);

                isFirst = false;
            }
        }

        /// <summary>
        /// 向后拼接一个排序方法。
        /// </summary>
        /// <param name="orderByDirection">排序方向</param>
        public virtual void BuildOrderByDirections(OrderByDirections orderByDirection)
        {
            switch (orderByDirection)
            {
                case OrderByDirections.None:
                    break;
                case OrderByDirections.ASC:
                    Builder.Append(Code_Space);
                    Builder.Append(Code_ASC);
                    break;
                case OrderByDirections.DESC:
                    Builder.Append(Code_Space);
                    Builder.Append(Code_DESC);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        #endregion

        #region - Comparison -

        /// <summary>
        /// 向后拼接一个 And 条件。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public virtual void BuildAndComparison(Condition before, Condition after)
        {
            BuildValue(before);

            Builder.Append(Code_Space);
            Builder.Append(Code_And);
            Builder.Append(Code_Space);

            BuildValue(after);
        }

        /// <summary>
        /// 向后拼接一个 Or 条件。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public virtual void BuildOrComparison(Condition before, Condition after)
        {
            BuildValue(before);

            Builder.Append(Code_Space);
            Builder.Append(Code_Or);
            Builder.Append(Code_Space);

            BuildValue(after);
        }

        /// <summary>
        /// 向后拼接一个等于比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public virtual void BuildEqualComparison(IValue before, IValue after)
        {
            if (!Internal(before, after) && !Internal(after, before))
            {
                if (!Null(before, after) && !Null(before, after))
                {
                    BuildValue(before);

                    Builder.Append(Code_Space);
                    Builder.Append(Code_Equal);
                    Builder.Append(Code_Space);

                    BuildValue(after);
                }

            }

            bool Null(IValue x, IValue y)
            {
                if (y is ConstantValue<DBNull>)
                {
                    BuildValue(x);

                    Builder.Append(Code_Space);
                    Builder.Append(Code_Is);
                    Builder.Append(Code_Space);
                    Builder.Append(Code_Null);

                    return true;
                }

                return false;
            }


            bool Internal(IValue x, IValue y)
            {
                if (x is Condition condition && y is ConstantValue<bool> boolValue)
                {
                    if (!boolValue.Value)
                    {
                        Builder.Append(Code_Not);
                        Builder.Append(Code_Parenthesis_Bracket_Begin);
                        BuildCondition(condition);
                        Builder.Append(Code_Parenthesis_Bracket_End);
                    }
                    else
                    {
                        BuildCondition(condition);
                    }

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 向后拼接一个不等于比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public virtual void BuildNotEqualComparison(IValue before, IValue after)
        {
            if (!Internal(before, after) && !Internal(after, before))
            {
                if (!Null(before, after) && !Null(after, before))
                {
                    BuildValue(before);

                    Builder.Append(Code_Space);
                    Builder.Append(Code_NotEqual);
                    Builder.Append(Code_Space);

                    BuildValue(after);
                }
            }

            bool Null(IValue x, IValue y)
            {
                if (y is ConstantValue<DBNull>)
                {
                    BuildValue(x);

                    Builder.Append(Code_Space);
                    Builder.Append(Code_Is);
                    Builder.Append(Code_Space);
                    Builder.Append(Code_Not);
                    Builder.Append(Code_Space);
                    Builder.Append(Code_Null);

                    return true;
                }

                return false;
            }

            bool Internal(IValue x, IValue y)
            {
                if (x is Condition condition && y is ConstantValue<bool> boolValue)
                {
                    if (boolValue.Value)
                    {
                        Builder.Append(Code_Not);
                        Builder.Append(Code_Parenthesis_Bracket_Begin);
                        BuildCondition(condition);
                        Builder.Append(Code_Parenthesis_Bracket_End);
                    }
                    else
                    {
                        BuildCondition(condition);
                    }

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// 向后拼接一个大于比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public virtual void BuildGreaterThanComparison(IValue before, IValue after)
        {
            BuildValue(before);

            Builder.Append(Code_Space);
            Builder.Append(Code_GreaterThan);
            Builder.Append(Code_Space);

            BuildValue(after);
        }

        /// <summary>
        /// 向后拼接一个大于比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public virtual void BuildLessThanComparison(IValue before, IValue after)
        {
            BuildValue(before);

            Builder.Append(Code_Space);
            Builder.Append(Code_LessThan);
            Builder.Append(Code_Space);

            BuildValue(after);
        }

        /// <summary>
        /// 向后拼接一个大于比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public virtual void BuildGreaterEqualComparison(IValue before, IValue after)
        {
            BuildValue(before);

            Builder.Append(Code_Space);
            Builder.Append(Code_GreaterThan);
            Builder.Append(Code_Equal);
            Builder.Append(Code_Space);

            BuildValue(after);
        }

        /// <summary>
        /// 向后拼接一个小于比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public virtual void BuildLessEqualComparison(IValue before, IValue after)
        {
            BuildValue(before);

            Builder.Append(Code_Space);
            Builder.Append(Code_LessThan);
            Builder.Append(Code_Equal);
            Builder.Append(Code_Space);

            BuildValue(after);
        }

        /// <summary>
        /// 向后拼接一个包含该值的比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public virtual void BuildContainsComparison(IValue before, IValue after)
        {
            BuildValue(before);

            Builder.Append(Code_Space);
            Builder.Append(Code_Like);
            Builder.Append(Code_Space);

            Builder.Append(Code_Concat);
            Builder.Append(Code_Parenthesis_Bracket_Begin);
            BuildValue(Value_Percent);
            Builder.Append(Code_Comma);
            Builder.Append(Code_Space);
            BuildValue(after);
            Builder.Append(Code_Comma);
            Builder.Append(Code_Space);
            BuildValue(Value_Percent);
            Builder.Append(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个在一些值中的比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置数组</param>
        public virtual void BuildInComparison(IValue before, IValue after)
        {
            BuildValue(before);

            Builder.Append(Code_Space);
            Builder.Append(Code_In);
            Builder.Append(Code_Space);

            Builder.Append(Code_Parenthesis_Bracket_Begin);

            BuildValue(after);

            Builder.Append(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个在两个值之间的比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置数组，必须两个长度</param>
        public virtual void BuildBetweenAndComparison(IValue before, IValue after)
        {
            if (after is ConstantValue<IValue[]> array && array.Value.Length == 2)
            {
                BuildValue(before);

                Builder.Append(Code_Space);
                Builder.Append(Code_Between);
                Builder.Append(Code_Space);

                BuildValue(array.Value[0]);

                Builder.Append(Code_Space);
                Builder.Append(Code_And);
                Builder.Append(Code_Space);

                BuildValue(array.Value[1]);

                return;
            }

            throw new NotSupportedException("Between And : After value must is a array and the length must be 2.");
        }

        /// <summary>
        /// 向后拼接一个开头与其相符的比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public virtual void BuildStartWithComparison(IValue before, IValue after)
        {
            BuildValue(before);

            Builder.Append(Code_Space);
            Builder.Append(Code_Like);
            Builder.Append(Code_Space);

            Builder.Append(Code_Concat);
            Builder.Append(Code_Parenthesis_Bracket_Begin);
            BuildValue(Value_Percent);
            Builder.Append(Code_Comma);
            Builder.Append(Code_Space);
            BuildValue(after);
            Builder.Append(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个结尾与其相符的比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public virtual void BuildEndWithComparison(IValue before, IValue after)
        {
            BuildValue(before);

            Builder.Append(Code_Space);
            Builder.Append(Code_Like);
            Builder.Append(Code_Space);

            Builder.Append(Code_Concat);
            Builder.Append(Code_Parenthesis_Bracket_Begin);
            BuildValue(after);
            Builder.Append(Code_Comma);
            Builder.Append(Code_Space);
            BuildValue(Value_Percent);
            Builder.Append(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个开头与其不相符的比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public virtual void BuildNotStartWithComparison(IValue before, IValue after)
        {
            BuildValue(before);

            Builder.Append(Code_Space);
            Builder.Append(Code_Not);
            Builder.Append(Code_Space);
            Builder.Append(Code_Like);
            Builder.Append(Code_Space);

            Builder.Append(Code_Concat);
            Builder.Append(Code_Parenthesis_Bracket_Begin);
            BuildValue(Value_Percent);
            Builder.Append(Code_Comma);
            Builder.Append(Code_Space);
            BuildValue(after);
            Builder.Append(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个结尾与其不相符的比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public virtual void BuildNotEndWithComparison(IValue before, IValue after)
        {
            BuildValue(before);

            Builder.Append(Code_Space);
            Builder.Append(Code_Not);
            Builder.Append(Code_Space);
            Builder.Append(Code_Like);
            Builder.Append(Code_Space);

            Builder.Append(Code_Concat);
            Builder.Append(Code_Parenthesis_Bracket_Begin);
            BuildValue(after);
            Builder.Append(Code_Comma);
            Builder.Append(Code_Space);
            BuildValue(Value_Percent);
            Builder.Append(Code_Parenthesis_Bracket_End);
        }

        /// <summary>
        /// 向后拼接一个按位与不为空的比较运算。
        /// </summary>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public virtual void BuildBitAndComparison(IValue before, IValue after)
        {
            BuildValue(before);

            Builder.Append(Code_Space);
            Builder.Append(Code_Amp);
            Builder.Append(Code_Space);

            BuildValue(after);

            Builder.Append(Code_Space);
            Builder.Append(Code_NotEqual);
            Builder.Append(Code_Space);

            Builder.Append(Code_Zero);
        }
        #endregion

        #region - Condition -

        /// <summary>
        /// 向后拼接一个条件。
        /// </summary>
        /// <param name="condition">条件信息</param>
        public virtual void BuildCondition(Condition condition)
        {
            switch (condition.Comparison)
            {
                case Comparisons.Equal:
                    BuildEqualComparison(condition.Before, condition.After);
                    break;
                case Comparisons.NotEqual:
                    BuildNotEqualComparison(condition.Before, condition.After);
                    break;
                case Comparisons.GreaterThan:
                    BuildGreaterThanComparison(condition.Before, condition.After);
                    break;
                case Comparisons.LessThan:
                    BuildLessThanComparison(condition.Before, condition.After);
                    break;
                case Comparisons.GreaterEqual:
                    BuildGreaterEqualComparison(condition.Before, condition.After);
                    break;
                case Comparisons.LessEqual:
                    BuildLessEqualComparison(condition.Before, condition.After);
                    break;
                case Comparisons.Contains:
                    BuildContainsComparison(condition.Before, condition.After);
                    break;
                case Comparisons.In:
                    BuildInComparison(condition.Before, condition.After);
                    break;
                case Comparisons.BetweenAnd:
                    BuildBetweenAndComparison(condition.Before, condition.After);
                    break;
                case Comparisons.StartWith:
                    BuildStartWithComparison(condition.Before, condition.After);
                    break;
                case Comparisons.EndWith:
                    BuildEndWithComparison(condition.Before, condition.After);
                    break;
                case Comparisons.NotStartWith:
                    BuildNotStartWithComparison(condition.Before, condition.After);
                    break;
                case Comparisons.NotEndWith:
                    BuildNotEndWithComparison(condition.Before, condition.After);
                    break;
                case Comparisons.BitAnd:
                    BuildBitAndComparison(condition.Before, condition.After);
                    break;
                case Comparisons.And:
                    BuildAndComparison((Condition)condition.Before, (Condition)condition.After);
                    break;
                case Comparisons.Or:
                    BuildOrComparison((Condition)condition.Before, (Condition)condition.After);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        /// <summary>
        /// 向后拼接一个条件集合。
        /// </summary>
        /// <param name="conditions">条件集合</param>
        public virtual void BuildConditions(Conditions conditions)
        {
            int end = conditions.Count - 1;
            int depth = 0;

            for (int i = 0; i < conditions.Count; i++)
            {
                var prev = i == 0 ? Condition.Empty : conditions[i - 1];
                var next = i == end ? Condition.Empty : conditions[i + 1];
                var item = conditions[i];

                if (prev != Condition.Empty)
                {
                    if (prev.Index != item.Index)
                    {
                        while (depth != 0 && (prev.Index & HexDigital[depth]) != (item.Index & HexDigital[depth]))
                        {
                            Builder.Append(Code_Parenthesis_Bracket_End);

                            --depth;
                        }
                    }

                    Builder.Append(Code_Space);
                    BuildConditionType(item.Type);
                    Builder.Append(Code_Space);
                }

                if (prev.Index != item.Index && (next.Index & HexDigital[depth]) == (item.Index & HexDigital[depth]))
                {
                    Builder.Append(Code_Parenthesis_Bracket_Begin);

                    ++depth;
                }

                BuildCondition(item);
            }

            while (depth != 0)
            {
                Builder.Append(Code_Parenthesis_Bracket_End);

                --depth;
            }

            if (conditions.Count == 0)
            {
                Builder.Append(Code_True_Expression);
            }
        }

        /// <summary>
        /// 向后拼接一个条件连接符。
        /// </summary>
        /// <param name="conditionType">条件连接符</param>
        public virtual void BuildConditionType(ConditionTypes conditionType)
        {
            switch (conditionType)
            {
                case ConditionTypes.And:
                    Builder.Append(Code_And);
                    break;
                case ConditionTypes.Or:
                    Builder.Append(Code_Or);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        #endregion

        #region - Join -

        /// <summary>
        /// 拼接一个表连接。
        /// </summary>
        /// <param name="join">表连接信息</param>
        public virtual void BuildJoin(Join join)
        {
            Builder.Append(Code_Space);

            BuildJoinDirection(join.Direction);

            Builder.Append(Code_Space);

            Builder.Append(Code_Join);

            Builder.Append(Code_Space);

            BuildTable(join.Table);

            Builder.Append(Code_Space);

            var alias = GetAlias(join.Table);

            if (alias != null)
            {
                BuildName(alias);

                Builder.Append(Code_Space);
            }

            Builder.Append(Code_On);

            Builder.Append(Code_Space);

            if (join.On != null && join.On.Count != 0)
            {
                BuildConditions(join.On);
            }
            else
            {
                Builder.Append(Code_True_Expression);
            }
        }

        /// <summary>
        /// 拼接一个表连接方向。
        /// </summary>
        /// <param name="joinDirection">表连接方向</param>
        public virtual void BuildJoinDirection(JoinDirections joinDirection)
        {
            switch (joinDirection)
            {
                case JoinDirections.Left:
                    Builder.Append(Code_Left);
                    break;
                case JoinDirections.Right:
                    Builder.Append(Code_Right);
                    break;
                case JoinDirections.Inner:
                    Builder.Append(Code_Inner);
                    break;
                case JoinDirections.Full:
                    Builder.Append(Code_Full);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        #endregion
    }

    /// <summary>
    /// T-SQL 生成器使用的参数列表。
    /// </summary>
    public sealed class SqlBuilderParameters : Dictionary<string, object>
    {
        const string ParamName = "__PARAM";

        readonly Dictionary<object, string> Map;

        int ParametersNum;

        /// <summary>
        /// 初始化 T-SQL 生成器使用的参数列表。
        /// </summary>
        public SqlBuilderParameters():base(StringComparer.OrdinalIgnoreCase)
        {
            Map = new Dictionary<object, string>();
        }

        /// <summary>
        /// 添加或获取参数。
        /// </summary>
        /// <param name="value">参数值</param>
        /// <returns>返回参数名</returns>
        public string GetOrAddParameter(object value)
        {
            if (!Map.TryGetValue(value, out var name))
            {
                name = $"{ParamName}{++ParametersNum}";

                Map.Add(value, name);
                Add(name, value);
            }

            return name;
        }

        /// <summary>
        /// 清空参数集合。
        /// </summary>
        public void ClearParameters()
        {
            Clear();
            Map.Clear();
        }
    }
}