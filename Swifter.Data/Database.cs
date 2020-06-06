using Swifter.Data.Sql;
using Swifter.RW;
using System;
using System.Data;
using System.Data.Common;
using System.Runtime.CompilerServices;
using static Swifter.Data.DbHelper;

namespace Swifter.Data
{
    /// <summary>
    /// 数据库实例。
    /// </summary>
    public sealed partial class Database
    {
        /// <summary>
        /// 默认命令执行超时时间，单位：秒。
        /// </summary>
        public int DefaultCommandTimeout = 30;

        /// <summary>
        /// 表示使用默认默认命令执行超时时间。
        /// </summary>
        internal const int UseDefaultCommandTimeout = -1;

        /// <summary>
        /// T-SQL 生成器工厂。
        /// </summary>
        public readonly SqlBuilderFactory SqlBuilderFactory;

        /// <summary>
        /// 数据库供应者工厂。
        /// </summary>
        public readonly DbProviderFactory DbProviderFactory;

        /// <summary>
        /// 数据库连接字符串。
        /// </summary>
        public readonly string DbConnectionString;

        /// <summary>
        /// 初始化数据库实例。
        /// </summary>
        /// <param name="providerName">数据库供应者的包名</param>
        /// <param name="dbConnectionString">数据库连接字符串</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public Database(string providerName, string dbConnectionString)
        {
            DbProviderFactory = GetFactory(providerName);

            DbConnectionString = dbConnectionString;

            SqlBuilderFactory = GetSqlBuilderFactory(providerName);
        }

        /// <summary>
        /// 通过 .config 文件里的 connectionStrings 配置中指定名称的配置项初始化数据库实例；配置项中必须包括 providerName（数据库供应者的包名） 和 connectionString（数据库连接字符串） 这两个字段。
        /// </summary>
        /// <param name="configName">数据库连接的配置项名称</param>
        public Database(string configName)
            : this(GetConfigProviderName(configName, out var dbConnectionString), dbConnectionString)
        {

        }

        /// <summary>
        /// 当有新的命令被创建时触发。
        /// </summary>
        public event DatabaseEventHandler<DbCommand> CreatedCommand;

        /// <summary>
        /// 当有新的数据库连接被打开时触发。
        /// </summary>
        public event DatabaseEventHandler<DbConnection> OpenedConnection;

        /// <summary>
        /// 当有新的 SQL 生成器被创建时触发。
        /// </summary>
        public event DatabaseEventHandler<SqlBuilder> CreatedSqlBuilder;

        /// <summary>
        /// 打开一个数据库连接。
        /// </summary>
        /// <returns>返回一个数据库连接</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public DbConnection OpenConnection()
        {
            var dbConnection = DbProviderFactory.CreateConnection();

            dbConnection.ConnectionString = DbConnectionString;

            dbConnection.Open();

            OpenedConnection?.Invoke(this, dbConnection);

            return dbConnection;
        }

        /// <summary>
        /// 创建一个数据库命令。
        /// </summary>
        /// <param name="sql">T-SQL 语句</param>
        /// <param name="parameters">参数；可以是一个对象，字典或其他键值对类型。</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="DefaultCommandTimeout"/> 值。</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回一个数据库命令</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public DbCommand CreateCommand(string sql, object parameters = null, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout, CommandType commandType = CommandType.Text)
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
                var dbCommand = (dbConnection ?? dbTransaction?.Connection ?? (iDbConnection = OpenConnection())).CreateCommand();

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
        /// 获取当前供应商的 T-SQL 生成器。
        /// </summary>
        /// <returns>返回 T-SQL 生成器</returns>
        /// <exception cref="NotSupportedException">当不支持该供应商时发生。</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public SqlBuilder CreateSQLBuilder()
        {
            if (SqlBuilderFactory == null)
            {
                throw new NotSupportedException("T-SQL builder does not support the provider.");
            }

            var sqlBuilder = SqlBuilderFactory.CreateSqlBuilder();

            CreatedSqlBuilder?.Invoke(this, sqlBuilder);

            return sqlBuilder;
        }

        /// <summary>
        /// 将数据读取器的当前结果集映射为指定类型的实例。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="dbDataReader">数据读取器</param>
        /// <returns>返回该类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T ReadScalar<T>(DbDataReader dbDataReader)
        {
            return ValueInterface<T>.ReadValue(new ReadScalarReader(dbDataReader));
        }

        /// <summary>
        /// 将数据读取器的当前结果集映射为指定类型的实例。
        /// </summary>
        /// <param name="dbDataReader">数据读取器</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回该类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object ReadScalar(DbDataReader dbDataReader, Type type)
        {
            return ValueInterface.GetInterface(type).Read(new ReadScalarReader(dbDataReader));
        }

        /// <summary>
        /// 执行一条查询命令。
        /// </summary>
        /// <param name="sql">T-SQL 语句</param>
        /// <param name="parameters">参数；可以是一个对象，字典或其他键值对类型。</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="DefaultCommandTimeout"/> 值。</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回一个数据读取器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public DbDataReader ExecuteReader(string sql, object parameters = null, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = CreateCommand(sql, parameters, dbConnection, dbTransaction, commandTimeout, commandType);

            var dbDataReader = dbCommand.ExecuteReader();

            if (dbConnection is null && dbTransaction is null)
            {
                return new ResultDbDataReader(dbCommand.Connection, dbCommand, dbDataReader);
            }

            return new ResultDbDataReader(dbCommand, dbDataReader);
        }

        /// <summary>
        /// 执行一条查询语句，并将查询结果集映射为指定类型的实例。
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
        public T ExecuteScalar<T>(string sql, object parameters = null, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = CreateCommand(sql, parameters, dbConnection, dbTransaction, commandTimeout, commandType);

            if (dbConnection is null && dbTransaction is null)
            {
                using (dbCommand.Connection)
                {
                    return Execute();
                }
            }
            else
            {
                return Execute();
            }

            T Execute()
            {
                using (dbCommand)
                {
                    using var dataReader = dbCommand.ExecuteReader();

                    return ReadScalar<T>(dataReader);
                }
            }
        }

        /// <summary>
        /// 执行一条查询语句，并将查询结果集映射为指定类型的实例。
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
        public object ExecuteScalar(string sql, Type type, object parameters = null, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = CreateCommand(sql, parameters, dbConnection, dbTransaction, commandTimeout, commandType);

            if (dbConnection is null && dbTransaction is null)
            {
                using (dbCommand.Connection)
                {
                    return Execute();
                }
            }
            else
            {
                return Execute();
            }

            object Execute()
            {
                using (dbCommand)
                {
                    using var dataReader = dbCommand.ExecuteReader();

                    return ReadScalar(dataReader, type);
                }
            }
        }

        /// <summary>
        /// 执行一条非查询语句。
        /// </summary>
        /// <param name="sql">T-SQL 语句</param>
        /// <param name="parameters">参数；可以是一个对象，字典或其他键值对类型。</param>
        /// <param name="dbConnection">数据库连接；如果指定，则使用指定的数据库连接，不指定则使用调用 <see cref="OpenConnection"/> 打开的数据库连接。</param>
        /// <param name="dbTransaction">事务；如果指定，则使用事务与数据库连接。</param>
        /// <param name="commandTimeout">超时时间，单位：秒；-1 表示使用 <see cref="DefaultCommandTimeout"/> 值。</param>
        /// <param name="commandType">命令类型</param>
        /// <returns>返回受影响行数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ExecuteNonQuery(string sql, object parameters = null, DbConnection dbConnection = null, DbTransaction dbTransaction = null, int commandTimeout = UseDefaultCommandTimeout, CommandType commandType = CommandType.Text)
        {
            var dbCommand = CreateCommand(sql, parameters, dbConnection, dbTransaction, commandTimeout, commandType);

            if (dbConnection is null && dbTransaction is null)
            {
                using (dbCommand.Connection)
                {
                    using (dbCommand)
                    {
                        return dbCommand.ExecuteNonQuery();
                    }
                }
            }
            else
            {
                using (dbCommand)
                {
                    return dbCommand.ExecuteNonQuery();
                }
            }
        }
    }
}