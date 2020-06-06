#if Async

using Swifter.Data.Sql;
using System;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static Swifter.Data.Database;

#if ValueTask
using NonQueryTask = System.Threading.Tasks.ValueTask<int>;
#else
using NonQueryTask = System.Threading.Tasks.Task<int>;
#endif

namespace Swifter.Data
{
    partial class SqlHelper
    {
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
        public static async NonQueryTask ExecuteUpdateBuildAsync(this Database database, Table table, Action<UpdateStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return await database.ExecuteNonQueryAsync(
                sql: database.BuildSql(builder =>
                {
                    var update = new UpdateStatement(table);

                    action(update);

                    builder.BuildUpdateStatement(update);
                }, out var parameters),
                parameters: parameters,
                dbConnection: dbConnection,
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
        public static async NonQueryTask ExecuteInsertBuildAsync(this Database database, Table table, Action<InsertStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return await database.ExecuteNonQueryAsync(
                sql: database.BuildSql(builder =>
                {
                    var insert = new InsertStatement(table);

                    action(insert);

                    builder.BuildInsertStatement(insert);
                }, out var parameters),
                parameters: parameters,
                dbConnection: dbConnection,
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
        public static async NonQueryTask ExecuteDeleteBuildAsync(this Database database, Table table, Action<DeleteStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return await database.ExecuteNonQueryAsync(
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
        public static async
#if ValueTask
        ValueTask<DbDataReader>
#else
        Task<DbDataReader>
#endif
        ExecuteSelectBuildAsync(this Database database, Table table, Action<SelectStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return await database.ExecuteReaderAsync(
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
        public static async
#if ValueTask
        ValueTask<DbDataReader>
#else
        Task<DbDataReader>
#endif
        ExecuteSelectBuildAsync(this Database database, ITable table, Action<SelectStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return await database.ExecuteReaderAsync(
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
        public static async
#if ValueTask
        ValueTask<T>
#else
        Task<T>
#endif
        ExecuteScalarBuildAsync<T>(this Database database, Table table, Action<SelectStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return await database.ExecuteScalarAsync<T>(
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
        public static async
#if ValueTask
        ValueTask<T>
#else
        Task<T>
#endif
        ExecuteScalarBuildAsync<T>(this Database database, ITable table, Action<SelectStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return await database.ExecuteScalarAsync<T>(
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
        public static async
#if ValueTask
        ValueTask<object>
#else
        Task<object>
#endif
        ExecuteScalarBuildAsync(this Database database, Table table, Type type, Action<SelectStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return await database.ExecuteScalarAsync(
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
        public static async
#if ValueTask
        ValueTask<object>
#else
        Task<object>
#endif
        ExecuteScalarBuildAsync(this Database database, ITable table, Type type, Action<SelectStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return await database.ExecuteScalarAsync(
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
        public static async
#if ValueTask
        ValueTask<(TData Data, long Total)>
#else
        Task<(TData Data, long Total)>
#endif
        ExecutePagingSelectBuildAsync<TData>(this Database database, Table table, Action<SelectStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return await database.ExecuteScalarAsync<(TData, long)>(
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
        public static async
#if ValueTask
        ValueTask<(TData Data, long Total)>
#else
        Task<(TData Data, long Total)>
#endif
        ExecutePagingSelectBuildAsync<TData>(this Database database, ITable table, Action<SelectStatement> action, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout)
        {
            return await database.ExecuteScalarAsync<(TData, long)>(
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
    }
}

#endif