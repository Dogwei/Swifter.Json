using Swifter.Data.Sql;
using Swifter.Tools;
using System;

namespace Swifter.Data.MySql
{
    sealed class SqlBuilder : Sql.SqlBuilder
    {
        /// <summary>
        /// 参数符号
        /// </summary>
        public const string Code_Parameter = "?";

        /// <summary>
        /// 单次查询最大数据行数。
        /// </summary>
        public static int MaxLimit { get; set; } = 999999999;

        bool IsErrorName(string name)
        {
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
                        return true;
                }
            }

            return false;
        }

        public override void BuildName(string name)
        {
            if (IsErrorName(name))
            {
                throw new ArgumentException($"Object name format error -- [{name}].", nameof(name));
            }
            else
            {
                Builder.Append(name);
            }
        }

        public override void BuildSelectTop(SelectStatement selectStatement)
        {
        }

        public override void BuildSelectLimit(int? offset, int? limit)
        {
            Builder.Append(Code_Space);

            Builder.Append(Code_Limit);

            Builder.Append(Code_Space);

            Builder.Append(Math.Max(offset ?? 0, 0));

            Builder.Append(Code_Comma);

            Builder.Append(Code_Space);

            Builder.Append(Math.Min(limit ?? MaxLimit, MaxLimit));
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

        public override void BuildParameter(string name)
        {
            Builder.Append(Code_Parameter);
            Builder.Append(name);
        }
    }
}