using System;
using System.Collections.Generic;

namespace Swifter.Data.Sql
{
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