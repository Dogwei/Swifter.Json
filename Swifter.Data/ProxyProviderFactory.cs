using Swifter.Tools;
using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace Swifter.Data
{
    sealed class ProxyProviderFactory<
        TConnection,
        TCommand,
        TDataAdapter,
        TParameter,
        TParameterCollection,
        TDataReader,
        TTransaction
        > : DbProviderFactory

        where TConnection : IDbConnection
        where TCommand : IDbCommand
        where TDataReader : IDataReader
        where TParameter : IDataParameter
        where TParameterCollection : IDataParameterCollection
        where TTransaction : IDbTransaction
    {
        public static readonly ProxyProviderFactory<
            TConnection,
            TCommand,
            TDataAdapter,
            TParameter,
            TParameterCollection,
            TDataReader,
            TTransaction> Instance = 
            new ProxyProviderFactory<TConnection, TCommand, TDataAdapter, TParameter, TParameterCollection, TDataReader, TTransaction>();

        public override bool CanCreateDataSourceEnumerator => false;

        public override DbCommand CreateCommand()
        {
            return new ProxyCommand(Activator.CreateInstance<TCommand>());
        }

        public override DbCommandBuilder CreateCommandBuilder()
        {
            return null;
        }

        public override DbConnection CreateConnection()
        {
            return new ProxyConnection(Activator.CreateInstance<TConnection>());
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return null;
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return Activator.CreateInstance<TDataAdapter>() as DbDataAdapter;
        }

        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            return null;
        }

        public override DbParameter CreateParameter()
        {
            return new ProxyParameter(Activator.CreateInstance<TParameter>());
        }

        public override string ToString() => $"{GetType().Namespace}.{"ProxyProviderFactory"}[\"{typeof(TConnection).FullName}\"]";

        public sealed class ProxyConnection : DbConnection
        {
            internal TConnection dbConnection;

            public ProxyConnection(TConnection dbConnection)
            {
                this.dbConnection = dbConnection;
            }
            
            public override string ConnectionString
            {
                get => dbConnection.ConnectionString;
                set
                {
                    var originalState = State;

                    dbConnection.ConnectionString = value;

                    var currentState = State;

                    if (originalState != currentState)
                    {
                        OnStateChange(new StateChangeEventArgs(originalState, currentState));
                    }
                }
            }

            public override string Database => dbConnection.Database;

            public override string DataSource => throw new NotSupportedException();

            public override string ServerVersion => throw new NotSupportedException();

            public override ConnectionState State => dbConnection.State;

            public override void ChangeDatabase(string databaseName)
            {
                var originalState = State;

                dbConnection.ChangeDatabase(databaseName);

                var currentState = State;

                if (originalState != currentState)
                {
                    OnStateChange(new StateChangeEventArgs(originalState, currentState));
                }
            }

            public override void Close()
            {
                var originalState = State;

                dbConnection.Close();

                var currentState = State;

                if (originalState != currentState)
                {
                    OnStateChange(new StateChangeEventArgs(originalState, currentState));
                }
            }

            public override void Open()
            {
                var originalState = State;

                dbConnection.Open();

                var currentState = State;

                if (originalState != currentState)
                {
                    OnStateChange(new StateChangeEventArgs(originalState, currentState));
                }
            }

            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            {
                return new ProxyTransaction((TTransaction)dbConnection.BeginTransaction(isolationLevel));
            }

            protected override DbCommand CreateDbCommand()
            {
                return new ProxyCommand((TCommand)dbConnection.CreateCommand());
            }

            public override int ConnectionTimeout => dbConnection.ConnectionTimeout;

            protected override DbProviderFactory DbProviderFactory => Instance;

            protected override void Dispose(bool disposing)
            {
                Close();
                dbConnection.Dispose();
            }

            public override string ToString() => $"{GetType().Namespace}.{nameof(ProxyConnection)}[\"{typeof(TConnection).FullName}\"]";
        }

        public sealed class ProxyCommand : DbCommand
        {
            internal TCommand dbCommand;

            public ProxyCommand(TCommand dbCommand)
            {
                this.dbCommand = dbCommand;
            }
            
            public override string CommandText { get => dbCommand.CommandText; set => dbCommand.CommandText = value; }
            public override int CommandTimeout { get => dbCommand.CommandTimeout; set => dbCommand.CommandTimeout = value; }
            public override CommandType CommandType { get => dbCommand.CommandType; set => dbCommand.CommandType = value; }
            public override bool DesignTimeVisible { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override UpdateRowSource UpdatedRowSource { get => dbCommand.UpdatedRowSource; set => dbCommand.UpdatedRowSource = value; }
            protected override DbConnection DbConnection
            {
                get => new ProxyConnection((TConnection)dbCommand.Connection);
                set => dbCommand.Connection = ((ProxyConnection)value).dbConnection;
            }

            protected override DbParameterCollection DbParameterCollection => new ProxyParameterCollection((TParameterCollection)dbCommand.Parameters);

            protected override DbTransaction DbTransaction
            {
                get => new ProxyTransaction((TTransaction)dbCommand.Transaction);
                set => dbCommand.Transaction = ((ProxyTransaction)value).transaction;
            }

            public override void Cancel()
            {
                dbCommand.Cancel();
            }

            public override int ExecuteNonQuery()
            {
                return dbCommand.ExecuteNonQuery();
            }

            public override object ExecuteScalar()
            {
                return dbCommand.ExecuteScalar();
            }

            public override void Prepare()
            {
                dbCommand.Prepare();
            }

            protected override DbParameter CreateDbParameter()
            {
                return new ProxyParameter((TParameter)dbCommand.CreateParameter());
            }

            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            {
                return new ProxyDataReader((TDataReader)dbCommand.ExecuteReader(behavior));
            }

            public override string ToString() => $"{GetType().Namespace}.{nameof(ProxyCommand)}[\"{typeof(TCommand).FullName}\"]";
        }

        public sealed class ProxyDataReader : DbDataReader
        {
            internal TDataReader dataReader;

            public ProxyDataReader(TDataReader dataReader)
            {
                this.dataReader = dataReader;
            }
            
            public override object this[int ordinal] => dataReader[ordinal];

            public override object this[string name] => dataReader[name];

            public override int Depth => dataReader.Depth;

            public override int FieldCount => dataReader.FieldCount;

            public override bool HasRows => throw new NotSupportedException();

            public override bool IsClosed => dataReader.IsClosed;

            public override int RecordsAffected => dataReader.RecordsAffected;

            public override void Close()
            {
                dataReader.Close();
            }

            public override bool GetBoolean(int ordinal)
            {
                return dataReader.GetBoolean(ordinal);
            }

            public override byte GetByte(int ordinal)
            {
                return dataReader.GetByte(ordinal);
            }

            public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
            {
                return dataReader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
            }

            public override char GetChar(int ordinal)
            {
                return dataReader.GetChar(ordinal);
            }

            public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
            {
                return dataReader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
            }

            public override string GetDataTypeName(int ordinal)
            {
                return dataReader.GetDataTypeName(ordinal);
            }

            public override DateTime GetDateTime(int ordinal)
            {
                return dataReader.GetDateTime(ordinal);
            }

            public override decimal GetDecimal(int ordinal)
            {
                return dataReader.GetDecimal(ordinal);
            }

            public override double GetDouble(int ordinal)
            {
                return dataReader.GetDouble(ordinal);
            }

            public override IEnumerator GetEnumerator()
            {
                throw new NotSupportedException();
            }

            public override Type GetFieldType(int ordinal)
            {
                return dataReader.GetFieldType(ordinal);
            }

            public override float GetFloat(int ordinal)
            {
                return dataReader.GetFloat(ordinal);
            }

            public override Guid GetGuid(int ordinal)
            {
                return dataReader.GetGuid(ordinal);
            }

            public override short GetInt16(int ordinal)
            {
                return dataReader.GetInt16(ordinal);
            }

            public override int GetInt32(int ordinal)
            {
                return dataReader.GetInt32(ordinal);
            }

            public override long GetInt64(int ordinal)
            {
                return dataReader.GetInt64(ordinal);
            }

            public override string GetName(int ordinal)
            {
                return dataReader.GetName(ordinal);
            }

            public override int GetOrdinal(string name)
            {
                return dataReader.GetOrdinal(name);
            }

            public override DataTable GetSchemaTable()
            {
                return dataReader.GetSchemaTable();
            }

            public override string GetString(int ordinal)
            {
                return dataReader.GetString(ordinal);
            }

            public override object GetValue(int ordinal)
            {
                return dataReader.GetValue(ordinal);
            }

            public override int GetValues(object[] values)
            {
                return dataReader.GetValues(values);
            }

            public override bool IsDBNull(int ordinal)
            {
                return dataReader.IsDBNull(ordinal);
            }

            public override bool NextResult()
            {
                return dataReader.NextResult();
            }

            public override bool Read()
            {
                return dataReader.Read();
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    Close();
                }
            }

            public override string ToString() => $"{GetType().Namespace}.{nameof(ProxyDataReader)}[\"{typeof(TDataReader).FullName}\"]";

        }

        public sealed class ProxyParameter : DbParameter
        {
            internal TParameter parameter;
            
            public ProxyParameter(TParameter parameter)
            {
                this.parameter = parameter;
            }

            public override DbType DbType { get => parameter.DbType; set => parameter.DbType = value; }
            public override ParameterDirection Direction { get => parameter.Direction; set => parameter.Direction = value; }
            public override bool IsNullable { get => parameter.IsNullable; set => throw new NotSupportedException(); }
            public override string ParameterName { get => parameter.ParameterName; set => parameter.ParameterName = value; }
            public override int Size { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override string SourceColumn { get => parameter.SourceColumn; set => parameter.SourceColumn = value; }
            public override bool SourceColumnNullMapping { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override object Value { get => parameter.Value; set => parameter.Value = value; }
            public override DataRowVersion SourceVersion { get => parameter.SourceVersion; set => parameter.SourceVersion = value; }

            public override void ResetDbType()
            {
                throw new NotSupportedException();
            }

            public override string ToString() => $"{GetType().Namespace}.{nameof(ProxyParameter)}[\"{typeof(TParameter).FullName}\"]";
        }

        public sealed class ProxyParameterCollection : DbParameterCollection
        {
            internal TParameterCollection parameterCollection;

            public ProxyParameterCollection(TParameterCollection parameterCollection)
            {
                this.parameterCollection = parameterCollection;
            }

            public override int Count => parameterCollection.Count;

            public override object SyncRoot => parameterCollection.SyncRoot;

            public override bool IsFixedSize => parameterCollection.IsFixedSize;

            public override bool IsReadOnly => parameterCollection.IsReadOnly;

            public override bool IsSynchronized => parameterCollection.IsSynchronized;

            public override int Add(object value)
            {
                if (value is ProxyParameter parameter)
                {
                    value = parameter.parameter;
                }

                return parameterCollection.Add(value);
            }

            public override void AddRange(Array values)
            {
                foreach (var item in values)
                {
                    Add(item);
                }
            }

            public override void Clear()
            {
                parameterCollection.Clear();
            }

            public override bool Contains(object value)
            {
                if (value is ProxyParameter parameter)
                {
                    value = parameter.parameter;
                }

                return parameterCollection.Contains(value);
            }

            public override bool Contains(string value)
            {
                return parameterCollection.Contains(value);
            }

            public override void CopyTo(Array array, int index)
            {
                parameterCollection.CopyTo(array, index);
            }

            public override IEnumerator GetEnumerator()
            {
                return parameterCollection.GetEnumerator();
            }

            public override int IndexOf(object value)
            {
                if (value is ProxyParameter parameter)
                {
                    value = parameter.parameter;
                }

                return parameterCollection.IndexOf(value);
            }

            public override int IndexOf(string parameterName)
            {
                return parameterCollection.IndexOf(parameterName);
            }

            public override void Insert(int index, object value)
            {
                if (value is ProxyParameter parameter)
                {
                    value = parameter.parameter;
                }

                parameterCollection.Insert(index, value);
            }

            public override void Remove(object value)
            {
                if (value is ProxyParameter parameter)
                {
                    value = parameter.parameter;
                }

                parameterCollection.Remove(value);
            }

            public override void RemoveAt(int index)
            {
                parameterCollection.RemoveAt(index);
            }

            public override void RemoveAt(string parameterName)
            {
                parameterCollection.RemoveAt(parameterName);
            }

            protected override DbParameter GetParameter(int index)
            {
                return new ProxyParameter((TParameter)parameterCollection[index]);
            }

            protected override DbParameter GetParameter(string parameterName)
            {
                return new ProxyParameter((TParameter)parameterCollection[parameterName]);
            }

            protected override void SetParameter(int index, DbParameter value)
            {
                object obj = value;

                if (obj is ProxyParameter parameter)
                {
                    obj = parameter.parameter;
                }

                parameterCollection[index] = obj;
            }

            protected override void SetParameter(string parameterName, DbParameter value)
            {
                object obj = value;

                if (obj is ProxyParameter parameter)
                {
                    obj = parameter.parameter;
                }

                parameterCollection[parameterName] = obj;
            }

            public override string ToString() => $"{GetType().Namespace}.{nameof(ProxyParameterCollection)}[\"{typeof(TParameterCollection).FullName}\"]";
        }

        public sealed class ProxyTransaction : DbTransaction
        {
            internal TTransaction transaction;

            public ProxyTransaction(TTransaction transaction)
            {
                this.transaction = transaction;
            }

            public override IsolationLevel IsolationLevel => transaction.IsolationLevel;

            protected override DbConnection DbConnection => new ProxyConnection((TConnection)transaction.Connection);

            public override void Commit()
            {
                transaction.Commit();
            }

            public override void Rollback()
            {
                transaction.Rollback();
            }

            public override string ToString() => $"{GetType().Namespace}.{nameof(ProxyTransaction)}[\"{typeof(TTransaction).FullName}\"]";
        }
    }
}
