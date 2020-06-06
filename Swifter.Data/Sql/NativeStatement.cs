using System;

namespace Swifter.Data.Sql
{
    /// <summary>
    /// 表示原生 T-SQL 语句。
    /// </summary>
    public sealed class NativeStatement : ITable, IValue
    {
        readonly Func<string> SqlGetter;

        /// <summary>
        /// T-SQL 语句。
        /// </summary>
        public string Sql => SqlGetter();

        /// <summary>
        /// 初始化原生 T-SQL 语句实例。
        /// </summary>
        /// <param name="sqlGetter">获取 T-SQL 的函数</param>
        NativeStatement(Func<string> sqlGetter)
        {
            SqlGetter = sqlGetter;
        }

        /// <summary>
        /// 构建原生 T-SQL 语句。
        /// </summary>
        /// <param name="sql">T-SQL 语句</param>
        public static implicit operator NativeStatement(string sql) => new NativeStatement(() => sql);

        /// <summary>
        /// 构建原生 T-SQL 语句。
        /// </summary>
        /// <param name="sqlGetter">获取 T-SQL 的函数</param>
        public static implicit operator NativeStatement(Func<string> sqlGetter) => new NativeStatement(sqlGetter);
    }
}