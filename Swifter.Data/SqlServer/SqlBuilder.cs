using Swifter.Data.Sql;
using Swifter.Tools;
using System;
using System.Collections.Generic;

namespace Swifter.Data.SqlServer
{
    sealed unsafe class SqlBuilder : Sql.SqlBuilder
    {
        /// <summary>
        /// 参数符号
        /// </summary>
        public const string Code_Parameter = "@";

        static readonly Dictionary<string, bool> KeepKeywords = new Dictionary<string, bool>(StringComparer.InvariantCultureIgnoreCase) {
            "ADD", "EXCEPT", "PERCENT", "ALL", "EXEC", "PLAN", "ALTER", "EXECUTE", "PRECISION", "AND", "EXISTS", "PRIMARY", "ANY", "EXIT", "PRINT", "AS",
            "FETCH", "PROC", "ASC", "FILE", "PROCEDURE", "AUTHORIZATION", "FILLFACTOR", "PUBLIC", "BACKUP", "FOR", "RAISERROR", "BEGIN", "FOREIGN",
            "READ", "BETWEEN", "FREETEXT", "READTEXT", "BREAK", "FREETEXTTABLE", "RECONFIGURE", "BROWSE", "FROM", "REFERENCES", "BULK", "FULL",
            "REPLICATION", "BY", "FUNCTION", "RESTORE", "CASCADE", "GOTO", "RESTRICT", "CASE", "GRANT", "RETURN", "CHECK", "GROUP", "REVOKE",
            "CHECKPOINT", "HAVING", "RIGHT", "CLOSE", "HOLDLOCK", "ROLLBACK", "CLUSTERED", "IDENTITY", "ROWCOUNT", "COALESCE", "INSERT",
            "ROWGUIDCOL", "COLLATE", "IDENTITYCOL", "RULE", "COLUMN", "IF", "SAVE", "COMMIT", "IN", "SCHEMA", "COMPUTE", "INDEX", "SELECT",
            "CONSTRAINT", "INNER", "SESSION", "CONTAINS", "SET", "CONTAINSTABLE", "INTERSECT", "SETUSER", "CONTINUE", "INTO",
            "SHUTDOWN", "CONVERT", "IS", "SOME", "CREATE", "JOIN", "STATISTICS", "CROSS", "KEY", "SYSTEM", "KILL", "TABLE",
            "CURRENT", "DATE", "LEFT", "TEXTSIZE", "TIME", "LIKE", "THEN", "TIMESTAMP", "LINENO", "TO", "USER", "LOAD",
            "TOP", "CURSOR", "NATIONAL", "TRAN", "DATABASE", "NOCHECK", "TRANSACTION", "DBCC", "NONCLUSTERED", "TRIGGER", "DEALLOCATE", "NOT",
            "TRUNCATE", "DECLARE", "NULL", "TSEQUAL", "DEFAULT", "NULLIF", "UNION", "DELETE", "OF", "UNIQUE", "DENY", "OFF", "UPDATE", "DESC",
            "OFFSETS", "UPDATETEXT", "DISK", "ON", "USE", "DISTINCT", "OPEN", "DISTRIBUTED", "OPENDATASOURCE", "VALUES", "DOUBLE",
            "OPENQUERY", "VARYING", "DROP", "OPENROWSET", "VIEW", "DUMMY", "OPENXML", "WAITFOR", "DUMP", "OPTION", "WHEN", "ELSE", "OR", "WHERE", "END",
            "ORDER", "WHILE", "ERRLVL", "OUTER", "WITH", "ESCAPE", "OVER", "WRITETEXT"
        };


        /// <summary>
        /// 单次查询最大数据行数。
        /// </summary>
        public static int MaxLimit { get; set; } = 999999999;

        bool IsStandardName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            foreach (var item in name)
            {
                switch (item)
                {
                    case var _ when item >= 'a' && item <= 'z':
                    case var _ when item >= 'A' && item <= 'Z':
                    case var _ when item >= '0' && item <= '9':
                    case '_':
                    case '@':
                    case '$':
                        break;
                    default:
                        return false;
                }
            }

            if (KeepKeywords.ContainsKey(name))
            {
                return false;
            }

            return true;
        }

        bool IsErrorName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return true;
            }

            return name.Contains("[") || name.Contains("]");
        }

        public override void BuildName(string name)
        {
            if (IsStandardName(name))
            {
                Builder.Append(name);
            }
            else if (IsErrorName(name))
            {
                throw new ArgumentException($"Object name format error -- [{name}].", nameof(name));
            }
            else
            {
                Builder.Append(Code_Square_Brackets_Begin);

                Builder.Append(name);

                Builder.Append(Code_Square_Brackets_End);
            }
        }

        public override void BuildSelectTop(SelectStatement selectStatement)
        {
            if (selectStatement.Offset == null && selectStatement.Limit != null)
            {
                Builder.Append(Code_Space);
                Builder.Append(Code_Top);
                Builder.Append(Code_Space);
                Builder.Append(selectStatement.Limit.Value);

                selectStatement.Limit = null;
            }
        }

        public override void BuildSelectLimit(int? offset, int? limit)
        {
            Builder.Append(Code_Space);

            Builder.Append(Code_Offset);

            Builder.Append(Code_Space);

            Builder.Append(Math.Max(offset ?? 0, 0));

            Builder.Append(Code_Space);

            Builder.Append(Code_Rows);

            Builder.Append(Code_Space);

            Builder.Append(Code_Fetch);

            Builder.Append(Code_Space);

            Builder.Append(Code_Next);

            Builder.Append(Code_Space);

            Builder.Append(Math.Min(limit ?? MaxLimit, MaxLimit));

            Builder.Append(Code_Space);

            Builder.Append(Code_Rows);

            Builder.Append(Code_Space);

            Builder.Append(Code_Only);
        }

        public override void BuildValue(ConstantValue<string> value)
        {
            switch (value.Value)
            {
                case Code_Percent:
                    BuildSimpleString(Code_Percent);
                    break;
                default:
                    BuildParameter(Parameters.GetOrAddParameter(value.Value));
                    break;

            }
        }

        public void BuildSimpleString(string value)
        {
            Builder.Append("'");
            Builder.Append(value);
            Builder.Append("'");
        }

        public override void BuildValue(ConstantValue<byte[]> value)
        {
            BuildParameter(Parameters.GetOrAddParameter(value.Value));
        }

        public override void BuildValue(ConstantValue<Guid> value)
        {
            BuildParameter(Parameters.GetOrAddParameter(value.Value));
        }

        public override void BuildValue(ConstantValue<DateTime> value)
        {
            BuildParameter(Parameters.GetOrAddParameter(value.Value));
        }

        public override void BuildValue(ConstantValue<DateTimeOffset> value)
        {
            BuildParameter(Parameters.GetOrAddParameter(value.Value));
        }

        public override void BuildParameter(string name)
        {
            Builder.Append(Code_Parameter);
            Builder.Append(name);
        }

        public override OrderBy GetDefaultOrderBy(SelectStatement selectStatement)
        {
            return new OrderBy(new Column(null, Code_One), OrderByDirections.None);
        }

        public override void BuildStatementEnd()
        {
            Builder.Append(Code_Semicolon);

            Builder.Append(Code_WrapLine);
        }

        public override void BuildTable(SelectStatement selectStatement)
        {
            if (selectStatement.OrderBies.Count != 0 && selectStatement.Limit == null)
            {
                selectStatement.Limit = MaxLimit;
            }

            base.BuildTable(selectStatement);
        }

    }
}