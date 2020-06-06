using Swifter.Data.Sql;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Data
{
    /// <summary>
    /// 提供数据库操作的工具方法
    /// </summary>
    public static partial class DbHelper
    {
        private const string ProviderFactoryInstance = "Instance";

        private static readonly Dictionary<string, DbProviderFactory> ProviderTypesCache;
        private static readonly Dictionary<string, SqlBuilderFactory> SqlBuilderFactories;

        static DbHelper()
        {
            ProviderTypesCache = new Dictionary<string, DbProviderFactory>();
            SqlBuilderFactories = new Dictionary<string, SqlBuilderFactory>();

            RegisterSqlBuilderFactory(new SqlServer.SqlBuilderFactory());
            RegisterSqlBuilderFactory(new MySql.SqlBuilderFactory());
        }

        /// <summary>
        /// 创建数据库连接。
        /// </summary>
        /// <param name="providerName">供应商的包名</param>
        /// <returns>返回一个数据库连接</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static DbConnection CreateConnection(string providerName)
        {
            return GetFactory(providerName).CreateConnection();
        }

        /// <summary>
        /// 获取数据库工厂实例。
        /// </summary>
        /// <param name="providerName">数据库供应者的包名</param>
        /// <returns>返回数据库工厂实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static DbProviderFactory GetFactory(string providerName)
        {
            if (providerName == null)
            {
                throw new ArgumentNullException(nameof(providerName));
            }

            if (ProviderTypesCache.TryGetValue(providerName, out var value))
            {
                return value;
            }

            lock (ProviderTypesCache)
            {
                return ProviderTypesCache.GetOrAdd(providerName, name => SearchFactory(name));
            }
        }

        /// <summary>
        /// 注册数据库供应者工厂。
        /// </summary>
        /// <param name="providerName">数据库供应者的包名</param>
        /// <param name="dbProviderFactory">数据库供应者工厂实例</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void RegisterFactory(string providerName, DbProviderFactory dbProviderFactory)
        {
            if (dbProviderFactory == null)
            {
                throw new ArgumentNullException(nameof(dbProviderFactory));
            }

            if (dbProviderFactory == null)
            {
                throw new ArgumentNullException(nameof(dbProviderFactory));
            }

            lock (ProviderTypesCache)
            {
                ProviderTypesCache.Add(providerName, dbProviderFactory);
            }
        }

        /// <summary>
        /// 注册数据库的 T-SQL 生成器工厂。
        /// </summary>
        /// <param name="factory">T-SQL 生成器工厂实例</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void RegisterSqlBuilderFactory(SqlBuilderFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (factory.ProviderName == null)
            {
                throw new ArgumentNullException(nameof(factory.ProviderName));
            }

            lock (SqlBuilderFactories)
            {
                SqlBuilderFactories.Add(factory.ProviderName, factory);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static DbProviderFactory GetInstance(Type type)
        {
            var members = type.GetMembers(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly);

            foreach (var item in members)
            {
                if (item.Name == ProviderFactoryInstance)
                {
                    if (item is FieldInfo field && field.FieldType == type)
                    {
                        if (field.GetValue(null) is DbProviderFactory result)
                        {
                            return result;
                        }
                    }
                    else if (item is PropertyInfo property && property.PropertyType == type && property.GetIndexParameters().Length == 0)
                    {
                        if (property.GetValue(null, null) is DbProviderFactory result)
                        {
                            return result;
                        }
                    }
                }
            }

            foreach (var item in members)
            {
                if (item is FieldInfo field && field.FieldType == type)
                {
                    if (field.GetValue(null) is DbProviderFactory result)
                    {
                        return result;
                    }
                }
                else if (item is PropertyInfo property && property.PropertyType == type && property.GetIndexParameters().Length == 0)
                {
                    if (property.GetValue(null, null) is DbProviderFactory result)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static DbProviderFactory TestType(ProviderClasses providerClasses, Type type)
        {
            if (typeof(DbProviderFactory).IsAssignableFrom(type))
            {
                var result = GetInstance(type);

                if (result != null)
                {
                    return result;
                }
            }
            else if (typeof(IDbConnection).IsAssignableFrom(type))
            {
                providerClasses.tConnection = type;
            }
            else if (typeof(IDbCommand).IsAssignableFrom(type))
            {
                providerClasses.tCommand = type;
            }
            else if (typeof(IDataAdapter).IsAssignableFrom(type))
            {
                providerClasses.tDataAdapter = type;
            }
            else if (typeof(System.Data.IDataReader).IsAssignableFrom(type))
            {
                providerClasses.tDataReader = type;
            }
            else if (typeof(IDataParameter).IsAssignableFrom(type))
            {
                providerClasses.tParameter = type;
            }
            else if (typeof(IDataParameterCollection).IsAssignableFrom(type))
            {
                providerClasses.tParameterCollection = type;
            }
            else if (typeof(IDbTransaction).IsAssignableFrom(type))
            {
                providerClasses.tTransaction = type;
            }

            return null;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static DbProviderFactory SearchFactory(string providerName)
        {
#if Dpf
            try
            {
                var dbProviderFactory = DbProviderFactories.GetFactory(providerName);

                if (dbProviderFactory != null)
                {
                    return dbProviderFactory;
                }
            }
            catch
            {
            }
#endif

            var providerClasses = new ProviderClasses(providerName);

            var assemblyName = providerName;

            Loop:

            try
            {
                /* 尝试加载供应商程序集 */
                var asembly = Assembly.Load(assemblyName);

                if (asembly != null)
                {
                    foreach (var type in asembly.GetTypes())
                    {
                        var result = TestType(providerClasses, type);

                        if (result != null)
                        {
                            return result;
                        }
                    }

                    if (providerClasses.tConnection != null)
                    {
                        var result = GetInstance(providerClasses.GetDynamicProviderFactoryType());

                        if (result != null)
                        {
                            return result;
                        }
                    }
                }


            }
            catch (Exception)
            {
            }

            var dotIndex = assemblyName.LastIndexOf('.');

            if (dotIndex != -1)
            {
                assemblyName = assemblyName.Substring(0, dotIndex);

                goto Loop;
            }

            /* 尝试查找已加载的程序集 */
            foreach (var asembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asembly.GetTypes())
                {
                    if (type.Namespace == providerName)
                    {
                        var result = TestType(providerClasses, type);

                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }

            if (providerClasses.tConnection != null)
            {
                var result = GetInstance(providerClasses.GetDynamicProviderFactoryType());

                if (result != null)
                {
                    return result;
                }
            }

            throw new ArgumentException($"Data provider not found in the runtime assemblies -- \"{providerName}\".");
        }

        /// <summary>
        /// 创建数据库连接。
        /// </summary>
        /// <param name="providerName">数据库连接的包名</param>
        /// <param name="connectionString">连接字符串</param>
        /// <returns>返回一个数据库连接</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static DbConnection CreateConnection(string providerName, string connectionString)
        {
            var dbConnection = CreateConnection(providerName);

            dbConnection.ConnectionString = connectionString;

            return dbConnection;
        }

        /// <summary>
        /// 将指定对象作为参数集合传递给 T-SQL 命令。
        /// </summary>
        /// <typeparam name="T">参数集合类型</typeparam>
        /// <param name="dbCommand">T-SQL 命令</param>
        /// <param name="obj">参数集合</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SetParameters<T>(this DbCommand dbCommand, T obj)
        {
            RWHelper.CreateReader(obj).As<string>().OnReadAll(new DbCommandParametersAdder(dbCommand));
        }

        /// <summary>
        /// 向 T-SQL 命令中添加具有指定名称和值的参数。
        /// </summary>
        /// <param name="dbCommand">T-SQL 命令</param>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        /// <returns>返回这个参数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static DbParameter AddParameter(this DbCommand dbCommand, string name, object value)
        {
            var dbParameter = dbCommand.CreateParameter();

            dbParameter.ParameterName = name;
            dbParameter.Value = value;

            dbCommand.Parameters.Add(dbParameter);

            return dbParameter;
        }

        /// <summary>
        /// 创建指定供应商名称的 T-SQL 生成器。
        /// </summary>
        /// <param name="providerName">供应商名称</param>
        /// <returns>返回 T-SQL 生成器</returns>
        /// <exception cref="NotSupportedException">当不支持该供应商时发生。</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static SqlBuilder CreateSqlBuilder(string providerName)
        {
            if (providerName == null)
            {
                throw new ArgumentNullException(nameof(providerName));
            }

            if (SqlBuilderFactories.TryGetValue(providerName, out var value))
            {
                return value.CreateSqlBuilder();
            }

            throw new NotSupportedException("T-SQL builder does not support the provider.");
        }

        /// <summary>
        /// 获取指定供应商名称的 T-SQL 生成器工厂。
        /// </summary>
        /// <param name="providerName">供应商名称</param>
        /// <returns>返回 T-SQL 生成器工厂</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static SqlBuilderFactory GetSqlBuilderFactory(string providerName)
        {
            SqlBuilderFactories.TryGetValue(providerName, out var value);

            return value;
        }

        internal static string GetConfigProviderName(string configName, out string dbConnectionString)
        {
            var config = ConfigurationManager.ConnectionStrings[configName];

            dbConnectionString = config.ConnectionString;

            return config.ProviderName;
        }



        static DbRowObjectMap FindMap(IEnumerable<DbRowObject> rows)
        {
            DbRowObjectMap map = null;

            foreach (var item in rows)
            {
                if (map != item.Map)
                {
                    if (map is null)
                    {
                        map = item.Map;
                    }
                    else if (map.Equals(item.Map))
                    {
                        item.Map = map;
                    }
                    else
                    {
                        throw new InvalidOperationException("Rows that are not the same result set.");
                    }
                }
            }

            return map;
        }

        /// <summary>
        /// 在数据集合中添加一些列。
        /// </summary>
        /// <param name="rows">数据集合</param>
        /// <param name="columns">列集合</param>
        public static void AddColumns(this IEnumerable<DbRowObject> rows, params (string ColumnName, Type ColumnType, AddColumnGetValueFunc GetValueFunc)[] columns)
        {
            if (FindMap(rows) is DbRowObjectMap map)
            {
                var count = map.Count;

                for (int i = 0; i < columns.Length; i++)
                {
                    map.Add(columns[i].ColumnName, ValueInterface.GetInterface(columns[i].ColumnType));
                }

                int index = 0;

                var offsey = count + columns.Length;

                foreach (var item in rows)
                {
                    if (item.Values.Length < offsey)
                    {
                        Array.Resize(ref item.Values, offsey);
                    }

                    for (int i = 0; i < columns.Length; i++)
                    {
                        var value = columns[i].GetValueFunc(item, index);

                        item.Values[count + i] = ToValue(value, columns[i].ColumnType);
                    }

                    ++index;
                }
            }

        }

        /// <summary>
        /// 修改数据集合中指定列名的的每个值。
        /// </summary>
        /// <param name="rows">数据集合</param>
        /// <param name="columnType">列新的类型</param>
        /// <param name="columnName">需要修改的列的名称</param>
        /// <param name="getValueFunc">获取每行的新值的函数，</param>
        public static void ModifyColumn(this IEnumerable<DbRowObject> rows, string columnName, Type columnType, ModifyColumnGetValueFunc getValueFunc)
        {
            if (FindMap(rows) is DbRowObjectMap map)
            {
                var index = map.FindIndex(columnName);

                if (index >= 0)
                {
                    int offset = 0;

                    foreach (var item in rows)
                    {
                        ref object value = ref item.Values[index];

                        value = ToValue(getValueFunc(item, offset, item.Values[index]), columnType);

                        ++offset;
                    }

                    map.SetValue(index, ValueInterface.GetInterface(columnType));
                }
                else
                {
                    throw new KeyNotFoundException(columnName);
                }
            }
        }

        /// <summary>
        /// 在数据集合中移除一些列。
        /// </summary>
        /// <param name="rows">数据集合</param>
        /// <param name="columnNames">需要移除的列集合</param>
        public static void RemoveColumns(this IEnumerable<DbRowObject> rows, params string[] columnNames)
        {
            if (FindMap(rows) is DbRowObjectMap map)
            {
                var indexes = new int[columnNames.Length];

                for (int i = 0; i < indexes.Length; i++)
                {
                    indexes[i] = map.FindIndex(columnNames[i]);

                    if (indexes[i] < 0)
                    {
                        throw new KeyNotFoundException(columnNames[i]);
                    }
                }

                Array.Sort(indexes);

                for (int i = indexes.Length - 1; i >= 0; i--)
                {
                    map.RemoveAt(indexes[i]);
                }

                foreach (var row in rows)
                {
                    var values = row.Values;

                    for (int i = 0, j = 0; i < values.Length; i++)
                    {
                        if (j < indexes.Length && i > indexes[j])
                        {
                            ++j;
                        }

                        if (j > 0)
                        {
                            values[i - j] = values[i];
                        }
                    }
                }
            }

        }

        /// <summary>
        /// 设置数据行集合的标识符。
        /// </summary>
        /// <param name="rows">数据行集合</param>
        /// <param name="flags">标识符</param>
        public static void SetDbRowsFlags(this IEnumerable<DbRowObject> rows, DbRowsFlags flags)
        {
            if (FindMap(rows) is DbRowObjectMap map)
            {
                map.flags = flags;
            }
        }

        /// <summary>
        /// 创建一个数据集合。
        /// </summary>
        /// <param name="count">行数</param>
        /// <returns>返回</returns>
        public static DbRowObject[] CreateDbRows(int count)
        {
            var result = new DbRowObject[count];
            var map = new DbRowObjectMap();
            var values = new object[0];

            for (int i = 0; i < count; i++)
            {
                result[i] = new DbRowObject(map, values);
            }

            return result;
        }

        internal static object ToValue(object value, Type columnType)
        {
            if (value is null)
            {
                return null;
            }

            if (columnType.IsInstanceOfType(value))
            {
                return value;
            }

            throw new InvalidCastException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void Add<TKey>(this Dictionary<TKey, bool> dictionary, TKey key, bool value = default)
        {
            dictionary.Add(key, value);
        }
    }


    /// <summary>
    /// 数据集合添加列时，获取列值的委托。
    /// </summary>
    /// <param name="row">当前行</param>
    /// <param name="rowNumber">当前行号</param>
    /// <returns>返回值</returns>
    public delegate object AddColumnGetValueFunc(DbRowObject row, int rowNumber);

    /// <summary>
    /// 数据集合修改列时，获取列值的委托。
    /// </summary>
    /// <param name="row">当前行</param>
    /// <param name="rowNumber">当前行号</param>
    /// <param name="value">旧值</param>
    /// <returns>返回新值</returns>
    public delegate object ModifyColumnGetValueFunc(DbRowObject row, int rowNumber, object value);

}