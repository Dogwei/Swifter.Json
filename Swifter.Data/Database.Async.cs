#if Async

using System;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

#if ValueTask
using NonQueryTask = System.Threading.Tasks.ValueTask<int>;
using ScalarObjectTask = System.Threading.Tasks.ValueTask<object>;
using ReaderTask = System.Threading.Tasks.ValueTask<System.Data.Common.DbDataReader>;
using CreateCommandTask = System.Threading.Tasks.ValueTask<System.Data.Common.DbCommand>;
using OpenConnectionTask = System.Threading.Tasks.ValueTask<System.Data.Common.DbConnection>;
#else
using NonQueryTask = System.Threading.Tasks.Task<int>;
using ScalarObjectTask = System.Threading.Tasks.Task<object>;
using ReaderTask = System.Threading.Tasks.Task<System.Data.Common.DbDataReader>;
using CreateCommandTask = System.Threading.Tasks.Task<System.Data.Common.DbCommand>;
using OpenConnectionTask = System.Threading.Tasks.Task<System.Data.Common.DbConnection>;
#endif

namespace Swifter.Data
{
    partial class Database
    {
        /// <summary>
        /// 异步打开一个新的数据库连接。
        /// </summary>
        /// <returns>返回一个数据库连接</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public async OpenConnectionTask OpenConnectionAsync()
        {
            var dbConnection = DbProviderFactory.CreateConnection();

            dbConnection.ConnectionString = DbConnectionString;

            await dbConnection.OpenAsync();

            OpenedConnection?.Invoke(this, dbConnection);

            return dbConnection;
        }


        /// <summary>
        /// 异步创建一个数据库命令。
        /// </summary>
        /// <param name="sql">T-SQL 语句</param>
        /// <param name="parameters">参数；可以是一个对象，字典或其他键值对类型。</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="OpenConnectionAsync"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="DefaultCommandTimeout"/> 值。</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回一个数据库命令</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public async CreateCommandTask CreateCommandAsync(string sql, object parameters = null, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout, CommandType commandType = CommandType.Text)
        {
            if (dbTransaction != null && dbConnection != null && dbTransaction.Connection != dbConnection)
            {
                throw new InvalidOperationException("The specified DbTransaction does not belong to the specified DbConnection.");
            }

            if (commandTimeout == UseDefaultCommandTimeout)
            {
                commandTimeout = DefaultCommandTimeout;
            }

            DbConnection iDbConnection = null;

            try
            {
                var dbCommand = (dbConnection ?? dbTransaction?.Connection ?? (iDbConnection = await OpenConnectionAsync())).CreateCommand();

                dbCommand.CommandText = sql;

                dbCommand.CommandTimeout = commandTimeout;

                dbCommand.CommandType = commandType;

                dbCommand.Transaction = dbTransaction;

                if (parameters != null)
                {
                    dbCommand.SetParameters(parameters);
                }

                CreatedCommand?.Invoke(this, dbCommand);

                return dbCommand;
            }
            catch
            {
                iDbConnection?.Dispose();

                throw;
            }
        }

        /// <summary>
        /// 异步执行一条查询命令。
        /// </summary>
        /// <param name="sql">T-SQL 语句</param>
        /// <param name="parameters">参数；可以是一个对象，字典或其他键值对类型。</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="DefaultCommandTimeout"/> 值。</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回一个数据读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public async ReaderTask ExecuteReaderAsync(string sql, object parameters = null, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = await CreateCommandAsync(sql, parameters, dbConnection, dbTransaction, commandTimeout, commandType);

            var dbDataReader = await dbCommand.ExecuteReaderAsync();

            if (dbConnection is null && dbTransaction is null)
            {
                return new ResultDbDataReader(dbCommand.Connection, dbCommand, dbDataReader);
            }

            return new ResultDbDataReader(dbCommand, dbDataReader);
        }

        /// <summary>
        /// 异步执行一条查询语句，并将查询结果集映射为指定类型的实例。
        /// 此接口的映射机制非常强大；它可以将结果集映射为：实体类，实体类集合，字典，表格，简单类型等；还可以使用元组读取数据集中的多返回值。
        /// </summary>
        /// <typeparam name="T">返回值类型</typeparam>
        /// <param name="sql">T-SQL 语句</param>
        /// <param name="parameters">参数；可以是一个对象，字典或其他键值对类型。</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="DefaultCommandTimeout"/> 值。</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public async
#if ValueTask
        ValueTask<T>
#else
        Task<T>
#endif
        ExecuteScalarAsync<T>(string sql, object parameters = null, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = await CreateCommandAsync(sql, parameters, dbConnection, dbTransaction, commandTimeout, commandType);

            if (dbConnection is null && dbTransaction is null)
            {
                using (dbCommand.Connection)
                {
                    return await Execute();
                }
            }
            else
            {
                return await Execute();
            }

            async
#if ValueTask
        ValueTask<T>
#else
        Task<T>
#endif
            Execute()
            {
                using (dbCommand)
                {
                    using var dataReader = await dbCommand.ExecuteReaderAsync();

                    return ReadScalar<T>(dataReader);
                }
            }
        }

        /// <summary>
        /// 异步执行一条查询语句，并将查询结果集映射为指定类型的实例。
        /// 此接口的映射机制非常强大；它可以将结果集映射为：实体类，实体类集合，字典，表格，简单类型等；还可以使用元组读取数据集中的多返回值。
        /// </summary>
        /// <param name="sql">T-SQL 语句</param>
        /// <param name="type">返回值类型</param>
        /// <param name="parameters">参数；可以是一个对象，字典或其他键值对类型。</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="DefaultCommandTimeout"/> 值。</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public async ScalarObjectTask ExecuteScalarAsync(string sql, Type type, object parameters = null, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = await CreateCommandAsync(sql, parameters, dbConnection, dbTransaction, commandTimeout, commandType);

            if (dbConnection is null && dbTransaction is null)
            {
                using (dbCommand.Connection)
                {
                    return await Execute();
                }
            }
            else
            {
                return await Execute();
            }

            async ScalarObjectTask Execute()
            {
                using (dbCommand)
                {
                    using var dataReader = await dbCommand.ExecuteReaderAsync();

                    return ReadScalar(dataReader, type);
                }
            }
        }

        /// <summary>
        /// 异步执行一条非查询语句。
        /// </summary>
        /// <param name="sql">T-SQL 语句</param>
        /// <param name="parameters">参数；可以是一个对象，字典或其他键值对类型。</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="DefaultCommandTimeout"/> 值。</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回受影响行数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public async NonQueryTask ExecuteNonQueryAsync(string sql, object parameters = null, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = await CreateCommandAsync(sql, parameters, dbConnection, dbTransaction, commandTimeout, commandType);

            if (dbConnection is null && dbTransaction is null)
            {
                using (dbCommand.Connection)
                {
                    return await Execute();
                }
            }
            else
            {
                return await Execute();
            }

            async NonQueryTask Execute()
            {
                using (dbCommand)
                {
                    return await dbCommand.ExecuteNonQueryAsync();
                }
            }
        }
    }
}

#endif