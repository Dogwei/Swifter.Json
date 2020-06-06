using Swifter.Data.Sql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using static Swifter.Data.Database;

namespace Swifter.Data
{
    /// <summary>
    /// 提供 T-SQL 生成的工具方法。
    /// </summary>
    public static partial class SqlHelper
    {
        /// <summary>
        /// 表示一个 DBNull 的常量。
        /// </summary>
        public static readonly ConstantValue<DBNull> DBNull = ConstantValue(System.DBNull.Value);

        /// <summary>
        /// 动态生成 T-SQL 语句。
        /// </summary>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="action">T-SQL 生成器实例</param>
        /// <param name="parameters">返回 T-SQL 语句的参数集合</param>
        /// <returns>返回生成的 T-SQL 语句</returns>
        public static string BuildSql(this Database database, Action<SqlBuilder> action, out SqlBuilderParameters parameters)
        {
            using var builder = database.CreateSQLBuilder();

            action(builder);

            parameters = builder.Parameters;

            return builder.ToSql();
        }

        /// <summary>
        /// 执行动态生成的 Update 语句。
        /// </summary>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Update 的表</param>
        /// <param name="action">初始化 Update 语句回调</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="Database.OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="Database.DefaultCommandTimeout"/> 值。</param>
        /// <returns>返回受影响行数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int ExecuteUpdateBuild(this Database database, Table table, Action<UpdateStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return database.ExecuteNonQuery(
                sql: database.BuildSql(builder =>
                {
                    var update = new UpdateStatement(table);

                    action(update);

                    builder.BuildUpdateStatement(update);
                }, out var parameters),
                parameters: parameters,
                dbTransaction: dbTransaction,
                commandTimeout: commandTimeout);
        }

        /// <summary>
        /// 执行动态生成的 Insert 语句。
        /// </summary>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Insert 的表</param>
        /// <param name="action">初始化 Insert 语句回调</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="Database.OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="Database.DefaultCommandTimeout"/> 值。</param>
        /// <returns>返回受影响行数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int ExecuteInsertBuild(this Database database, Table table, Action<InsertStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return database.ExecuteNonQuery(
                sql: database.BuildSql(builder =>
                {
                    var insert = new InsertStatement(table);

                    action(insert);

                    builder.BuildInsertStatement(insert);
                }, out var parameters),
                parameters: parameters,
                dbConnection:dbConnection,
                dbTransaction: dbTransaction,
                commandTimeout: commandTimeout);
        }

        /// <summary>
        /// 执行动态生成的 Delete 语句。
        /// </summary>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Delete 的表</param>
        /// <param name="action">初始化 Delete 语句回调</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="Database.OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="Database.DefaultCommandTimeout"/> 值。</param>
        /// <returns>返回受影响行数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int ExecuteDeleteBuild(this Database database, Table table, Action<DeleteStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return database.ExecuteNonQuery(
                sql: database.BuildSql(builder =>
                {
                    var delete = new DeleteStatement(table);

                    action(delete);

                    builder.BuildDeleteStatement(delete);
                }, out var parameters),
                parameters: parameters,
                dbConnection: dbConnection,
                dbTransaction: dbTransaction,
                commandTimeout: commandTimeout);
        }

        /// <summary>
        /// 执行动态生成的 Select 语句。
        /// </summary>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Select 的表</param>
        /// <param name="action">初始化 Select 语句回调</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="Database.OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="Database.DefaultCommandTimeout"/> 值。</param>
        /// <returns>返回一个数据读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static DbDataReader ExecuteSelectBuild(this Database database, Table table, Action<SelectStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return database.ExecuteReader(
                sql: database.BuildSql(builder =>
                {
                    var select = new SelectStatement(table);

                    action(select);

                    builder.BuildSelectStatement(select);
                }, out var parameters),
                parameters: parameters,
                dbConnection: dbConnection,
                dbTransaction: dbTransaction,
                commandTimeout: commandTimeout);
        }

        /// <summary>
        /// 执行动态生成的 Select 语句。
        /// </summary>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Select 的表</param>
        /// <param name="action">初始化 Select 语句回调</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="Database.OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="Database.DefaultCommandTimeout"/> 值。</param>
        /// <returns>返回一个数据读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static DbDataReader ExecuteSelectBuild(this Database database, ITable table, Action<SelectStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return database.ExecuteReader(
                sql: database.BuildSql(builder =>
                {
                    var select = new SelectStatement(table);

                    action(select);

                    builder.BuildSelectStatement(select);
                }, out var parameters),
                parameters: parameters,
                dbConnection: dbConnection,
                dbTransaction: dbTransaction,
                commandTimeout: commandTimeout);
        }

        /// <summary>
        /// 执行动态生成的 Select 语句，并返回指定类型的结果。
        /// 如果返回值类型等于结果集的第一行第一列的值的类型，则返回第一行第一列的值，
        /// 如果返回值类型是一个集合，则返回所有行的数据。
        /// 否则返回第一行的数据对象。
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Select 的表</param>
        /// <param name="action">初始化 Select 语句回调</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="Database.OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="Database.DefaultCommandTimeout"/> 值。</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T ExecuteScalarBuild<T>(this Database database, Table table, Action<SelectStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return database.ExecuteScalar<T>(
                sql: database.BuildSql(builder =>
                {
                    var select = new SelectStatement(table);

                    action(select);

                    builder.BuildSelectStatement(select);
                }, out var parameters),
                parameters: parameters,
                dbConnection: dbConnection,
                dbTransaction: dbTransaction,
                commandTimeout: commandTimeout);
        }

        /// <summary>
        /// 执行动态生成的 Select 语句，并返回指定类型的结果。
        /// 如果返回值类型等于结果集的第一行第一列的值的类型，则返回第一行第一列的值，
        /// 如果返回值类型是一个集合，则返回所有行的数据。
        /// 否则返回第一行的数据对象。
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Select 的表</param>
        /// <param name="action">初始化 Select 语句回调</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="Database.OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="Database.DefaultCommandTimeout"/> 值。</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T ExecuteScalarBuild<T>(this Database database, ITable table, Action<SelectStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return database.ExecuteScalar<T>(
                sql: database.BuildSql(builder =>
                {
                    var select = new SelectStatement(table);

                    action(select);

                    builder.BuildSelectStatement(select);
                }, out var parameters),
                parameters: parameters,
                dbConnection: dbConnection,
                dbTransaction: dbTransaction,
                commandTimeout: commandTimeout);
        }

        /// <summary>
        /// 执行动态生成的 Select 语句，并返回指定类型的结果。
        /// 如果返回值类型等于结果集的第一行第一列的值的类型，则返回第一行第一列的值，
        /// 如果返回值类型是一个集合，则返回所有行的数据。
        /// 否则返回第一行的数据对象。
        /// </summary>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Select 的表</param>
        /// <param name="type">返回值类型</param>
        /// <param name="action">初始化 Select 语句回调</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="Database.OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="Database.DefaultCommandTimeout"/> 值。</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object ExecuteScalarBuild(this Database database, Table table, Type type, Action<SelectStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return database.ExecuteScalar(
                sql: database.BuildSql(builder =>
                {
                    var select = new SelectStatement(table);

                    action(select);

                    builder.BuildSelectStatement(select);
                }, out var parameters),
                parameters: parameters,
                type: type,
                dbConnection: dbConnection,
                dbTransaction: dbTransaction,
                commandTimeout: commandTimeout);
        }

        /// <summary>
        /// 执行动态生成的 Select 语句，并返回指定类型的结果。
        /// 如果返回值类型等于结果集的第一行第一列的值的类型，则返回第一行第一列的值，
        /// 如果返回值类型是一个集合，则返回所有行的数据。
        /// 否则返回第一行的数据对象。
        /// </summary>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Select 的表</param>
        /// <param name="type">返回值类型</param>
        /// <param name="action">初始化 Select 语句回调</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="Database.OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="Database.DefaultCommandTimeout"/> 值。</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object ExecuteScalarBuild(this Database database, ITable table, Type type, Action<SelectStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return database.ExecuteScalar(
                sql: database.BuildSql(builder =>
                {
                    var select = new SelectStatement(table);

                    action(select);

                    builder.BuildSelectStatement(select);
                }, out var parameters),
                parameters: parameters,
                type: type,
                dbConnection: dbConnection,
                dbTransaction: dbTransaction,
                commandTimeout: commandTimeout);
        }

        /// <summary>
        /// 执行动态生成的 Select 语句，并获取该查询条件下的数据总数。
        /// </summary>
        /// <typeparam name="TData">表格数据类型</typeparam>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Select 的表</param>
        /// <param name="action">初始化 Select 语句回调</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="Database.OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="Database.DefaultCommandTimeout"/> 值。</param>
        /// <returns>返回指定类型的值和数据总数</returns>
        public static (TData Data, long Total) ExecutePagingSelectBuild<TData>(this Database database, Table table, Action<SelectStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return database.ExecuteScalar<(TData, long)>(
                sql: database.BuildSql(builder =>
                {
                    var select = new SelectStatement(table);

                    action(select);

                    builder.BuildSelectStatement(select);

                    select.Offset = null;
                    select.Limit = null;

                    select.Columns.Clear();
                    select.OrderBies.Clear();

                    select.SelectCount(ValueOf(1), "Total");

                    builder.BuildSelectStatement(select);
                }, out var parameters),
                parameters: parameters,
                dbConnection: dbConnection,
                dbTransaction: dbTransaction,
                commandTimeout: commandTimeout);
        }

        /// <summary>
        /// 执行动态生成的 Select 语句，并获取该查询条件下的数据总数。
        /// </summary>
        /// <typeparam name="TData">表格数据类型</typeparam>
        /// <param name="database">数据库操作对象实例</param>
        /// <param name="table">需要 Select 的表</param>
        /// <param name="action">初始化 Select 语句回调</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="Database.OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="Database.DefaultCommandTimeout"/> 值。</param>
        /// <returns>返回指定类型的值和数据总数</returns>
        public static (TData Data, long Total) ExecutePagingSelectBuild<TData>(this Database database, ITable table, Action<SelectStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return database.ExecuteScalar<(TData, long)>(
                sql: database.BuildSql(builder =>
                {
                    var select = new SelectStatement(table);

                    action(select);

                    builder.BuildSelectStatement(select);

                    select.Offset = null;
                    select.Limit = null;

                    select.Columns.Clear();
                    select.OrderBies.Clear();

                    select.SelectCount(ValueOf(1), "Total");

                    builder.BuildSelectStatement(select);
                }, out var parameters),
                parameters: parameters,
                dbConnection: dbConnection,
                dbTransaction: dbTransaction,
                commandTimeout: commandTimeout);
        }

        /// <summary>
        /// 为 Insert 语句设置一个赋值参数。
        /// </summary>
        /// <param name="insertStatement">Insert 语句</param>
        /// <param name="columnName">要赋值的列</param>
        /// <param name="value">值</param>
        /// <returns>返回当前实例</returns>
        public static InsertStatement Set(this InsertStatement insertStatement, string columnName, IValue value)
        {
            insertStatement.Values.Add(new AssignValue(new Column(insertStatement.Table, columnName), value));

            return insertStatement;
        }

        /// <summary>
        /// 为 Update 语句设置一个赋值参数。
        /// </summary>
        /// <param name="updateStatement">Update 语句</param>
        /// <param name="columnName">要赋值的列</param>
        /// <param name="value">值</param>
        /// <returns>返回当前实例</returns>
        public static UpdateStatement Set(this UpdateStatement updateStatement, string columnName, IValue value)
        {
            updateStatement.Values.Add(new AssignValue(new Column(updateStatement.Table, columnName), value));

            return updateStatement;
        }

        /// <summary>
        /// 为 Delete 语句设置一个条件。
        /// </summary>
        /// <param name="deleteStatement">Delete 语句</param>
        /// <param name="columnName">要比较的列</param>
        /// <param name="comparison">比较符</param>
        /// <param name="value">值</param>
        /// <param name="condition">连接符</param>
        /// <returns>返回当前实例</returns>
        public static DeleteStatement Where(this DeleteStatement deleteStatement, string columnName, Comparisons comparison, IValue value, ConditionTypes condition = ConditionTypes.And)
        {
            deleteStatement.Where.Add(new Condition(condition, comparison, new Column(deleteStatement.Table, columnName), value));

            return deleteStatement;
        }

        /// <summary>
        /// 为 Delete 语句设置一个条件。
        /// </summary>
        /// <param name="updateStatement">Update 语句</param>
        /// <param name="columnName">要比较的列</param>
        /// <param name="comparison">比较符</param>
        /// <param name="value">值</param>
        /// <param name="condition">连接符</param>
        /// <returns>返回当前实例</returns>
        public static UpdateStatement Where(this UpdateStatement updateStatement, string columnName, Comparisons comparison, IValue value, ConditionTypes condition = ConditionTypes.And)
        {
            updateStatement.Where.Add(new Condition(condition, comparison, new Column(updateStatement.Table, columnName), value));

            return updateStatement;
        }

        /// <summary>
        /// 为 Delete 语句设置一个条件。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="columnName">要比较的主表列</param>
        /// <param name="comparison">比较符</param>
        /// <param name="value">值</param>
        /// <param name="condition">连接符</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement Where(this SelectStatement selectStatement, string columnName, Comparisons comparison, IValue value, ConditionTypes condition = ConditionTypes.And)
        {
            selectStatement.Where.Add(new Condition(condition, comparison, new Column(selectStatement.Table, columnName), value));

            return selectStatement;
        }

        /// <summary>
        /// 为 Delete 语句设置一个条件。
        /// </summary>
        /// <param name="deleteStatement">Delete 语句</param>
        /// <param name="left">左值</param>
        /// <param name="comparison">比较符</param>
        /// <param name="right">右值</param>
        /// <param name="condition">连接符</param>
        /// <returns>返回当前实例</returns>
        public static DeleteStatement Where(this DeleteStatement deleteStatement, IValue left, Comparisons comparison, IValue right, ConditionTypes condition = ConditionTypes.And)
        {
            deleteStatement.Where.Add(new Condition(condition, comparison, left, right));

            return deleteStatement;
        }

        /// <summary>
        /// 为 Delete 语句设置一个条件。
        /// </summary>
        /// <param name="updateStatement">Update 语句</param>
        /// <param name="left">左值</param>
        /// <param name="comparison">比较符</param>
        /// <param name="right">右值</param>
        /// <param name="condition">连接符</param>
        /// <returns>返回当前实例</returns>
        public static UpdateStatement Where(this UpdateStatement updateStatement, IValue left, Comparisons comparison, IValue right, ConditionTypes condition = ConditionTypes.And)
        {
            updateStatement.Where.Add(new Condition(condition, comparison, left, right));

            return updateStatement;
        }

        /// <summary>
        /// 为 Delete 语句设置一个条件。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="left">左值</param>
        /// <param name="comparison">比较符</param>
        /// <param name="right">右值</param>
        /// <param name="condition">连接符</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement Where(this SelectStatement selectStatement, IValue left, Comparisons comparison, IValue right, ConditionTypes condition = ConditionTypes.And)
        {
            selectStatement.Where.Add(new Condition(condition, comparison, left, right));

            return selectStatement;
        }

        /// <summary>
        /// 为 Update 语句的主表设置一个表达式条件。
        /// </summary>
        /// <param name="updateStatement">Update 语句</param>
        /// <param name="conditionExpression">"Regular Format :\[(?&lt;Index&gt;[0-9]+)?(?&lt;Type&gt;&amp;|\||And|Or)?(?&lt;Operator&gt;[A-Za-z!=&gt;&lt;]+)\](?&lt;Name&gt;[A-Za-z_0-9]+)"</param>
        /// <param name="value">值</param>
        /// <returns>返回当前实例</returns>
        public static UpdateStatement Where(this UpdateStatement updateStatement, string conditionExpression, IValue value)
        {
            var condition = Condition.Parse(updateStatement.Table, conditionExpression);

            condition.After = value;

            updateStatement.Where.Add(condition);

            return updateStatement;
        }

        /// <summary>
        /// 为 Delete 语句的主表设置一个表达式条件。
        /// </summary>
        /// <param name="deleteStatement">Delete 语句</param>
        /// <param name="conditionExpression">"Regular Format :\[(?&lt;Index&gt;[0-9]+)?(?&lt;Type&gt;&amp;|\||And|Or)?(?&lt;Operator&gt;[A-Za-z!=&gt;&lt;]+)\](?&lt;Name&gt;[A-Za-z_0-9]+)"</param>
        /// <param name="value">值</param>
        /// <returns>返回当前实例</returns>
        public static DeleteStatement Where(this DeleteStatement deleteStatement, string conditionExpression, IValue value)
        {
            var condition = Condition.Parse(deleteStatement.Table, conditionExpression);

            condition.After = value;

            deleteStatement.Where.Add(condition);

            return deleteStatement;
        }

        /// <summary>
        /// 为 Select 语句的主表设置一个表达式条件。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="conditionExpression">"Regular Format :\[(?&lt;Index&gt;[0-9]+)?(?&lt;Type&gt;&amp;|\||And|Or)?(?&lt;Operator&gt;[A-Za-z!=&gt;&lt;]+)\](?&lt;Name&gt;[A-Za-z_0-9]+)"</param>
        /// <param name="value">值</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement Where(this SelectStatement selectStatement, string conditionExpression, IValue value)
        {
            var condition = Condition.Parse(selectStatement.Table, conditionExpression);

            condition.After = value;

            selectStatement.Where.Add(condition);

            return selectStatement;
        }

        /// <summary>
        /// 为表连接信息添加一个条件。
        /// </summary>
        /// <param name="join">表连接信息</param>
        /// <param name="columnName">要比较的列</param>
        /// <param name="comparison">比较符</param>
        /// <param name="value">值</param>
        /// <param name="condition">连接符</param>
        /// <returns>返回当前实例</returns>
        public static Join On(this Join join, string columnName, Comparisons comparison, IValue value, ConditionTypes condition = ConditionTypes.And)
        {
            join.On.Add(new Condition(condition, comparison, new Column(join.Table, columnName), value));

            return join;
        }

        /// <summary>
        /// 为表连接信息添加一个条件。
        /// </summary>
        /// <param name="join">表连接信息</param>
        /// <param name="left">左值</param>
        /// <param name="comparison">比较符</param>
        /// <param name="right">右值</param>
        /// <param name="condition">连接符</param>
        /// <returns>返回当前实例</returns>
        public static Join On(this Join join, IValue left, Comparisons comparison, IValue right, ConditionTypes condition = ConditionTypes.And)
        {
            join.On.Add(new Condition(condition, comparison, left, right));

            return join;
        }

        /// <summary>
        /// 为表连接信息添加一个条件。
        /// </summary>
        /// <param name="join">表连接信息</param>
        /// <param name="conditionExpression">"Regular Format :\[(?&lt;Index&gt;[0-9a-fA-F]+)?(?&lt;Type&gt;&amp;|\||And|Or)?(?&lt;Operator&gt;[A-Za-z!=&gt;&lt;]+)\](?&lt;Name&gt;[A-Za-z_0-9]+)"</param>
        /// <param name="value">值</param>
        /// <returns>返回当前实例</returns>
        public static Join On(this Join join, string conditionExpression, IValue value)
        {
            var condition = Condition.Parse(join.Table, conditionExpression);

            condition.After = value;

            join.On.Add(condition);

            return join;
        }

        /// <summary>
        /// 为 Select 语句添加一个排序。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="columnName">要排序的列</param>
        /// <param name="direction">排序方向</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement OrderBy(this SelectStatement selectStatement, string columnName, OrderByDirections direction)
        {
            selectStatement.OrderBies.Add(new OrderBy(new Column(null, columnName), direction));

            return selectStatement;
        }

        /// <summary>
        /// 为 Select 语句添加一个排序。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="column">要排序的列</param>
        /// <param name="direction">排序方向</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement OrderBy(this SelectStatement selectStatement, Column column, OrderByDirections direction)
        {
            selectStatement.OrderBies.Add(new OrderBy(column, direction));

            return selectStatement;
        }

        /// <summary>
        /// 为 Select 语句的主表添加一个分组。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="columnName">要分组的主表列</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement GroupBy(this SelectStatement selectStatement, string columnName)
        {
            selectStatement.GroupBies.Add(new Column(selectStatement.Table, columnName));

            return selectStatement;
        }

        /// <summary>
        /// 为 Select 语句添加一个分组。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="column">要分组的列</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement GroupBy(this SelectStatement selectStatement, Column column)
        {
            selectStatement.GroupBies.Add(column);

            return selectStatement;
        }

        /// <summary>
        /// 为 Select 语句的主表添加一个查询列。
        /// </summary>
        /// <param name="selectStatement"></param>
        /// <param name="columnName">主表列</param>
        /// <param name="alias">别名</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement Select(this SelectStatement selectStatement, string columnName, string alias = null) => selectStatement.Select(new Column(selectStatement.Table, columnName), alias);

        /// <summary>
        /// 为 Select 语句添加一个查询列。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="column">列</param>
        /// <param name="alias">别名</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement Select(this SelectStatement selectStatement, IValue column, string alias = null) => selectStatement.Select(new SelectColumn(column, alias));

        /// <summary>
        /// 为 Select 语句添加一个查询列。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="selectColumn">查询列信息</param>
        /// <returns></returns>
        public static SelectStatement Select(this SelectStatement selectStatement, SelectColumn selectColumn)
        {
            selectStatement.Columns.Add(selectColumn);

            return selectStatement;
        }

        /// <summary>
        /// 创建一个 Count 聚合函数信息。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="value">要聚合的值</param>
        /// <param name="alias">别名</param>
        /// <returns>返回一个 Select 列</returns>
        public static SelectStatement SelectCount(this SelectStatement selectStatement, IValue value, string alias = null) => selectStatement.Select(new SelectColumn(new CountFunction(value), alias));

        /// <summary>
        /// 创建一个 Sum 聚合函数信息。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="value">要聚合的值</param>
        /// <param name="alias">别名</param>
        /// <returns>返回一个 Select 列</returns>
        public static SelectStatement SelectSum(this SelectStatement selectStatement, IValue value, string alias = null) => selectStatement.Select(new SelectColumn(new SumFunction(value), alias));

        /// <summary>
        /// 创建一个 Avg 聚合函数信息。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="value">要聚合的值</param>
        /// <param name="alias">别名</param>
        /// <returns>返回一个 Select 列</returns>
        public static SelectStatement SelectAvg(this SelectStatement selectStatement, IValue value, string alias = null) => selectStatement.Select(new SelectColumn(new AvgFunction(value), alias));

        /// <summary>
        /// 创建一个 Max 聚合函数信息。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="value">要聚合的值</param>
        /// <param name="alias">别名</param>
        /// <returns>返回一个 Select 列</returns>
        public static SelectStatement SelectMax(this SelectStatement selectStatement, IValue value, string alias = null) => selectStatement.Select(new SelectColumn(new MaxFunction(value), alias));

        /// <summary>
        /// 创建一个 Min 聚合函数信息。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="value">要聚合的值</param>
        /// <param name="alias">别名</param>
        /// <returns>返回一个 Select 列</returns>
        public static SelectStatement SelectMin(this SelectStatement selectStatement, IValue value, string alias = null) => selectStatement.Select(new SelectColumn(new MinFunction(value), alias));

        /// <summary>
        /// 创建一个 Count 聚合函数信息。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="columnName">要聚合主表的列名</param>
        /// <param name="alias">别名</param>
        /// <returns>返回一个 Select 列</returns>
        public static SelectStatement SelectCount(this SelectStatement selectStatement, string columnName, string alias = null) => selectStatement.Select(new SelectColumn(new CountFunction(selectStatement.ColumnOf(columnName)), alias));

        /// <summary>
        /// 创建一个 Sum 聚合函数信息。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="columnName">要聚合主表的列名</param>
        /// <param name="alias">别名</param>
        /// <returns>返回一个 Select 列</returns>
        public static SelectStatement SelectSum(this SelectStatement selectStatement, string columnName, string alias = null) => selectStatement.Select(new SelectColumn(new SumFunction(selectStatement.ColumnOf(columnName)), alias));

        /// <summary>
        /// 创建一个 Avg 聚合函数信息。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="columnName">要聚合主表的列名</param>
        /// <param name="alias">别名</param>
        /// <returns>返回一个 Select 列</returns>
        public static SelectStatement SelectAvg(this SelectStatement selectStatement, string columnName, string alias = null) => selectStatement.Select(new SelectColumn(new AvgFunction(selectStatement.ColumnOf(columnName)), alias));

        /// <summary>
        /// 创建一个 Max 聚合函数信息。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="columnName">要聚合主表的列名</param>
        /// <param name="alias">别名</param>
        /// <returns>返回一个 Select 列</returns>
        public static SelectStatement SelectMax(this SelectStatement selectStatement, string columnName, string alias = null) => selectStatement.Select(new SelectColumn(new MaxFunction(selectStatement.ColumnOf(columnName)), alias));

        /// <summary>
        /// 创建一个 Min 聚合函数信息。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="columnName">要聚合主表的列名</param>
        /// <param name="alias">别名</param>
        /// <returns>返回一个 Select 列</returns>
        public static SelectStatement SelectMin(this SelectStatement selectStatement, string columnName, string alias = null) => selectStatement.Select(new SelectColumn(new MinFunction(selectStatement.ColumnOf(columnName)), alias));

        /// <summary>
        /// 为 Select 语句设置一个子查询列。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">子查询表</param>
        /// <param name="action">子查询初始化回调</param>
        /// <param name="alias">子查询列别名</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement SelectSubQuery(this SelectStatement selectStatement, Table table, Action<SelectStatement> action, string alias = null)
        {
            var subQuery = new SelectStatement(table);

            selectStatement.Select(new SelectColumn(subQuery, alias));

            action(subQuery);

            return selectStatement;
        }

        /// <summary>
        /// 为 Select 语句添加一个表左关联。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">要关联的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement LeftJoin(this SelectStatement selectStatement, Table table, Action<Join> action) => Join(selectStatement, JoinDirections.Left, table, action);

        /// <summary>
        /// 为 Select 语句添加一个表右关联。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">要关联的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement RightJoin(this SelectStatement selectStatement, Table table, Action<Join> action) => Join(selectStatement, JoinDirections.Right, table, action);

        /// <summary>
        /// 为 Select 语句添加一个表内关联。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">要关联的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement InnerJoin(this SelectStatement selectStatement, Table table, Action<Join> action) => Join(selectStatement, JoinDirections.Inner, table, action);

        /// <summary>
        /// 为 Select 语句添加一个表外关联。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">要关联的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement FullJoin(this SelectStatement selectStatement, Table table, Action<Join> action) => Join(selectStatement, JoinDirections.Full, table, action);

        /// <summary>
        /// 为 Select 语句添加一个表左关联。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">要关联的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement LeftJoin(this SelectStatement selectStatement, ITable table, Action<Join> action) => Join(selectStatement, JoinDirections.Left, table, action);

        /// <summary>
        /// 为 Select 语句添加一个表右关联。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">要关联的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement RightJoin(this SelectStatement selectStatement, ITable table, Action<Join> action) => Join(selectStatement, JoinDirections.Right, table, action);

        /// <summary>
        /// 为 Select 语句添加一个表内关联。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">要关联的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement InnerJoin(this SelectStatement selectStatement, ITable table, Action<Join> action) => Join(selectStatement, JoinDirections.Inner, table, action);

        /// <summary>
        /// 为 Select 语句添加一个表外关联。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="table">要关联的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement FullJoin(this SelectStatement selectStatement, ITable table, Action<Join> action) => Join(selectStatement, JoinDirections.Full, table, action);

        /// <summary>
        /// 为 Select 语句添加一个表连接。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="direction">表连接方向</param>
        /// <param name="table">要连接的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        private static SelectStatement Join(SelectStatement selectStatement, JoinDirections direction, Table table, Action<Join> action) => Join(selectStatement, direction, (ITable)table, action);

        /// <summary>
        /// 为 Select 语句添加一个表连接。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="direction">表连接方向</param>
        /// <param name="table">要连接的表</param>
        /// <param name="action">初始化 Join 回调</param>
        /// <returns>返回当前实例</returns>
        private static SelectStatement Join(SelectStatement selectStatement, JoinDirections direction, ITable table, Action<Join> action)
        {
            var join = new Join(direction, table);

            selectStatement.Joins.Add(join);

            action(join);

            return selectStatement;
        }

        /// <summary>
        /// 为 Select 语句设置分页参数。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="offset">偏移行数</param>
        /// <param name="limit">行数数量</param>
        /// <returns>返回当前实例</returns>
        public static SelectStatement Paging(this SelectStatement selectStatement, int offset, int limit)
        {
            selectStatement.Offset = offset;
            selectStatement.Limit = limit;

            return selectStatement;
        }

        /// <summary>
        /// 创建一个由后期绑定的值。
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <param name="getter">值获取函数</param>
        /// <returns>返回值</returns>
        public static IValue ValueOf<T>(Func<T> getter) => new VariableValue(() => ValueOf(getter));

        /// <summary>
        /// 创建一个 String 值。
        /// </summary>
        /// <param name="value">String</param>
        /// <returns>返回值</returns>
        public static IValue ValueOf(string value) => value is null ? (IValue)DBNull : ConstantValue(value);

        /// <summary>
        /// 创建一个 Boolean 值。
        /// </summary>
        /// <param name="value">Boolean</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(bool value) => ConstantValue(value);

        /// <summary>
        /// 创建一个 SByte 值。
        /// </summary>
        /// <param name="value">SByte</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(sbyte value) => ConstantValue(value);

        /// <summary>
        /// 创建一个 Int16 值。
        /// </summary>
        /// <param name="value">Int16</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(short value) => ConstantValue(value);

        /// <summary>
        /// 创建一个 Int32 值。
        /// </summary>
        /// <param name="value">Int32</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(int value) => ConstantValue(value);

        /// <summary>
        /// 创建一个 Int64 值。
        /// </summary>
        /// <param name="value">Int64</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(long value) => ConstantValue(value);

        /// <summary>
        /// 创建一个 Byte 值。
        /// </summary>
        /// <param name="value">Byte</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(byte value) => ConstantValue(value);

        /// <summary>
        /// 创建一个 UInt16 值。
        /// </summary>
        /// <param name="value">UInt16</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(ushort value) => ConstantValue(value);

        /// <summary>
        /// 创建一个 UInt32 值。
        /// </summary>
        /// <param name="value">UInt32</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(uint value) => ConstantValue(value);

        /// <summary>
        /// 创建一个 UInt64 值。
        /// </summary>
        /// <param name="value">UInt64</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(ulong value) => ConstantValue(value);

        /// <summary>
        /// 创建一个 String 值。
        /// </summary>
        /// <param name="value">String</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(char value) => ConstantValue(value);

        /// <summary>
        /// 创建一个 Single 值。
        /// </summary>
        /// <param name="value">Single</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(float value) => ConstantValue(value);

        /// <summary>
        /// 创建一个 Double 值。
        /// </summary>
        /// <param name="value">Double</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(double value) => ConstantValue(value);

        /// <summary>
        /// 创建一个 Decimal 值。
        /// </summary>
        /// <param name="value">Decimal</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(decimal value) => ConstantValue(value);

        /// <summary>
        /// 创建一个 DateTime 值。
        /// </summary>
        /// <param name="value">DateTime</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(DateTime value) => ConstantValue(value);

        /// <summary>
        /// 创建一个 DateTimeOffset 值。
        /// </summary>
        /// <param name="value">DateTimeOffset</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(DateTimeOffset value) => ConstantValue(value);

        /// <summary>
        /// 创建一个 Guid 值。
        /// </summary>
        /// <param name="value">Guid</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf(Guid value) => ConstantValue(value);

        /// <summary>
        /// 创建一个值的集合。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="collection">元素集合</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf<T>(IEnumerable<T> collection)
        {
            var list = new List<IValue>();

            foreach (var item in collection)
            {
                list.Add(ValueOf(item));
            }

            return ConstantValue(list.ToArray());
        }

        /// <summary>
        /// 创建一个对象值。
        /// </summary>
        /// <param name="value">对象</param>
        /// <returns>返回一个 T-SQL 值</returns>
        public static IValue ValueOf<T>(T value)
        {
            if (value == null || value is DBNull)
                return DBNull;

            if (value is IValue @as)
                return @as;

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.Boolean:
                    return ConstantValue(Convert.ToBoolean(value));
                case TypeCode.Char:
                    return ConstantValue(Convert.ToChar(value));
                case TypeCode.SByte:
                    return ConstantValue(Convert.ToSByte(value));
                case TypeCode.Byte:
                    return ConstantValue(Convert.ToByte(value));
                case TypeCode.Int16:
                    return ConstantValue(Convert.ToInt16(value));
                case TypeCode.UInt16:
                    return ConstantValue(Convert.ToUInt16(value));
                case TypeCode.Int32:
                    return ConstantValue(Convert.ToInt32(value));
                case TypeCode.UInt32:
                    return ConstantValue(Convert.ToUInt32(value));
                case TypeCode.Int64:
                    return ConstantValue(Convert.ToInt64(value));
                case TypeCode.UInt64:
                    return ConstantValue(Convert.ToUInt64(value));
                case TypeCode.Single:
                    return ConstantValue(Convert.ToSingle(value));
                case TypeCode.Double:
                    return ConstantValue(Convert.ToDouble(value));
                case TypeCode.Decimal:
                    return ConstantValue(Convert.ToDecimal(value));
                case TypeCode.DateTime:
                    return ConstantValue(Convert.ToDateTime(value));
                case TypeCode.String:
                    return ConstantValue(Convert.ToString(value));
            }

            if (value is DateTimeOffset dateTimeOffset)
                return ConstantValue(dateTimeOffset);

            if (value is Guid guid)
                return ConstantValue(guid);

            if (value is byte[] bytes)
                return ConstantValue(bytes);

            if (value is IEnumerable collection)
                return ValueOf(collection.Cast<object>());

            return ConstantValue(value.ToString());
        }

        /// <summary>
        /// 创建一个列信息。
        /// </summary>
        /// <param name="table">列所在表</param>
        /// <param name="columnName">列名</param>
        /// <returns>返回一个列信息</returns>
        public static Column ColumnOf(this Table table, string columnName) => new Column(table, columnName);

        /// <summary>
        /// 为 Select 语句的主表创建一个列信息。
        /// </summary>
        /// <param name="selectStatement">Select 语句</param>
        /// <param name="columnName">列名</param>
        /// <returns>返回一个列信息</returns>
        public static Column ColumnOf(this SelectStatement selectStatement, string columnName) => new Column(selectStatement.Table, columnName);

        /// <summary>
        /// 为 Join 信息的主表创建一个列信息。
        /// </summary>
        /// <param name="join">Select 语句</param>
        /// <param name="columnName">列名</param>
        /// <returns>返回一个列信息</returns>
        public static Column ColumnOf(this Join join, string columnName) => new Column(join.Table, columnName);

        internal static ConstantValue<T> ConstantValue<T>(T value) => new ConstantValue<T>(value);
    }
}