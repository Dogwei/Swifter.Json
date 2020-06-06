using Swifter.Tools;
using System;

namespace Swifter.Data.Sql
{
    /// <summary>
    /// 条件信息
    /// </summary>
    public sealed class Condition : IComparable<Condition>, IValue
    {
        internal static readonly Condition Empty = new Condition();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="expression"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        public static bool TryParse(ITable table, string expression, out Condition condition)
        {
            const char match_begin = '[';
            const char match_end = ']';

            condition = default;

            if (string.IsNullOrEmpty(expression))
            {
                return false;
            }

            if (expression[0] != match_begin)
            {
                return false;
            }

            var match_length = expression.IndexOf(match_end);

            if (match_length < 0)
            {
                return false;
            }

            var index = "0";
            var type = ConditionTypes.And;

            var match_index = 1;

            var index_length = 0;

            for (int i = match_index; i < match_length; i++)
            {
                if (expression[i] >= '0' && expression[i] <= '9')
                {
                    ++index_length;
                }
                else
                {
                    break;
                }
            }

            if (index_length >= 1)
            {
                index = expression.Substring(match_index, index_length);
            }

            match_index += index_length;

            var type_length = 0;

            switch (char.ToUpper(expression[match_index]))
            {
                case 'A':
                    if (expression.IndexOf("AND", StringComparison.OrdinalIgnoreCase) == match_index)
                    {
                        type_length += 3;
                    }
                    break;
                case 'O':
                    if (expression.IndexOf("OR", StringComparison.OrdinalIgnoreCase) == match_index)
                    {
                        type_length += 2;
                    }
                    break;
                case '&':
                case '|':
                    ++type_length;
                    break;
            }

            if (type_length >= 1)
            {
                switch (expression.Substring(match_index, type_length).ToUpper())
                {
                    case "AND":
                    case "&":
                    case "&&":
                        type = ConditionTypes.And;
                        break;
                    case "OR":
                    case "|":
                    case "||":
                        type = ConditionTypes.Or;
                        break;
                    default:
                        type_length = 0;
                        break;
                }
            }

            match_index += type_length;

            var comparison_length = 0;

            for (int i = match_index; i < match_length; i++)
            {
                switch (expression[i])
                {
                    case var lower when lower >= 'a' && lower <= 'z':
                    case var upper when upper >= 'A' && upper <= 'Z':
                    case '>':
                    case '<':
                    case '=':
                    case '!':
                        ++comparison_length;
                        continue;
                }

                break;
            }

            if (!(comparison_length >= 1))
            {
                return false;
            }

            if (!ComparisonAttribute.TryGetComparison(expression.Substring(match_index, comparison_length), out var comparison))
            {
                return false;
            }

            match_index += comparison_length;

            if (match_index != match_length)
            {
                return false;
            }

            var equal_index = expression.IndexOf('=', match_length);

            string before;
            string after = null;

            if (equal_index >= 0)
            {
                before = expression.Substring(match_length + 1, equal_index - match_length - 1);
                after = expression.Substring(equal_index + 1);
            }
            else
            {
                before = expression.Substring(match_length + 1);
            }

            condition = new Condition(index, type, comparison, new Column(table, before), SqlHelper.ValueOf(after));

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="table"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static Condition Parse(ITable table, string expression)
        {
            TryParse(table, expression, out var condition);

            return condition ?? throw new FormatException("Regular Format : \"\\[([0-9]+)?(&|\\||And|Or)?(?<Operator>[A-Za-z!=><]+)\\](?<Name>[A-Za-z_0-9]+)\"");
        }

        readonly string stIndex;

        Condition() : this("0")
        {
        }

        /// <summary>
        /// 构建条件信息
        /// </summary>
        /// <param name="index">十六索引</param>
        public Condition(string index)
        {
            if (string.IsNullOrEmpty(index))
            {
                index = "0";
            }

            stIndex = index;

            unsafe
            {
                var chars = stackalloc char[index.Length];

                for (int i = 0, j = index.Length - 1; i < index.Length; ++i,--j)
                {
                    chars[j] = index[i];
                }

                var (code, length, value) = NumberHelper.Hex.ParseInt64(chars, index.Length);

                if (code != ParseCode.Success)
                {
                    throw new FormatException(nameof(index));
                }

                Index = (int)value;
            }
        }

        /// <summary>
        /// 构建条件信息
        /// </summary>
        /// <param name="index">十六索引</param>
        /// <param name="type">连接符</param>
        /// <param name="comparison">比较符</param>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public Condition(string index, ConditionTypes type, Comparisons comparison, IValue before, IValue after) : this(index)
        {
            Type = type;
            Comparison = comparison;
            Before = before;
            After = after;
        }

        /// <summary>
        /// 构建条件信息
        /// </summary>
        /// <param name="type">连接符</param>
        /// <param name="comparison">比较符</param>
        /// <param name="before">前置值</param>
        /// <param name="after">后置值</param>
        public Condition(ConditionTypes type, Comparisons comparison, IValue before, IValue after) : this()
        {
            Type = type;
            Comparison = comparison;
            Before = before;
            After = after;
        }

        /// <summary>
        /// 一个倒置的索引值。
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 连接符。
        /// </summary>
        public ConditionTypes Type { get; set; }

        /// <summary>
        /// 比较符。
        /// </summary>
        public Comparisons Comparison { get; set; }

        /// <summary>
        /// 前置值。
        /// </summary>
        public IValue Before { get; set; }

        /// <summary>
        /// 后置值。
        /// </summary>
        public IValue After { get; set; }

        /// <summary>
        /// 和另一个条件比较顺序。
        /// </summary>
        /// <param name="other">另一个条件</param>
        /// <returns>返回比较结果</returns>
        public int CompareTo(Condition other)
        {
            return stIndex.CompareTo(other.stIndex);
        }
    }
}