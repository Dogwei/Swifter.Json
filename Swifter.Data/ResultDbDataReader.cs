using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace Swifter.Data
{
    internal sealed class ResultDbDataReader : DbDataReader
    {
        private readonly DbDataReader dbDataReader;
        private readonly DbCommand dbCommand;
        private readonly DbConnection dbConnection;

        public ResultDbDataReader(DbConnection dbConnection, DbCommand dbCommand, DbDataReader dbDataReader)
        {
            this.dbConnection = dbConnection;

            this.dbCommand = dbCommand;
            this.dbDataReader = dbDataReader;
        }

        public ResultDbDataReader(DbCommand dbCommand, DbDataReader dbDataReader)
        {
            this.dbCommand = dbCommand;
            this.dbDataReader = dbDataReader;
        }

        public override object this[int ordinal] => dbDataReader[ordinal];

        public override object this[string name] => dbDataReader[name];

        public override int Depth => dbDataReader.Depth;

        public override int FieldCount => dbDataReader.FieldCount;

        public override bool HasRows => dbDataReader.HasRows;

        public override bool IsClosed => dbDataReader.IsClosed;

        public override int RecordsAffected => dbDataReader.RecordsAffected;

        public override void Close()
        {
            dbDataReader.Close();
            dbCommand.Dispose();
            dbConnection?.Close();
        }

        public override bool GetBoolean(int ordinal)
        {
            return dbDataReader.GetBoolean(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            return dbDataReader.GetByte(ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            return dbDataReader.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override char GetChar(int ordinal)
        {
            return dbDataReader.GetChar(ordinal);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return dbDataReader.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override string GetDataTypeName(int ordinal)
        {
            return dbDataReader.GetDataTypeName(ordinal);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            return dbDataReader.GetDateTime(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            return dbDataReader.GetDecimal(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            return dbDataReader.GetDouble(ordinal);
        }

        public override IEnumerator GetEnumerator()
        {
            return dbDataReader.GetEnumerator();
        }

        public override Type GetFieldType(int ordinal)
        {
            return dbDataReader.GetFieldType(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            return dbDataReader.GetFloat(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            return dbDataReader.GetGuid(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            return dbDataReader.GetInt16(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            return dbDataReader.GetInt32(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            return dbDataReader.GetInt64(ordinal);
        }

        public override string GetName(int ordinal)
        {
            return dbDataReader.GetName(ordinal);
        }

        public override int GetOrdinal(string name)
        {
            return dbDataReader.GetOrdinal(name);
        }

        public override DataTable GetSchemaTable()
        {
            return dbDataReader.GetSchemaTable();
        }

        public override string GetString(int ordinal)
        {
            return dbDataReader.GetString(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            return dbDataReader.GetValue(ordinal);
        }

        public override int GetValues(object[] values)
        {
            return dbDataReader.GetValues(values);
        }

        public override bool IsDBNull(int ordinal)
        {
            return dbDataReader.IsDBNull(ordinal);
        }

        public override bool NextResult()
        {
            return dbDataReader.NextResult();
        }

        public override bool Read()
        {
            return dbDataReader.Read();
        }

        public override int VisibleFieldCount => dbDataReader.VisibleFieldCount;

        public override string ToString()
        {
            return dbDataReader.ToString();
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
            }
        }

        public override bool Equals(object obj)
        {
            return dbDataReader.Equals(obj);
        }

        public override int GetHashCode()
        {
            return dbDataReader.GetHashCode();
        }

        public override Type GetProviderSpecificFieldType(int ordinal)
        {
            return dbDataReader.GetProviderSpecificFieldType(ordinal);
        }

        public override object GetProviderSpecificValue(int ordinal)
        {
            return dbDataReader.GetProviderSpecificValue(ordinal);
        }

        public override int GetProviderSpecificValues(object[] values)
        {
            return dbDataReader.GetProviderSpecificValues(values);
        }

        public override object InitializeLifetimeService()
        {
            return dbDataReader.InitializeLifetimeService();
        }
    }
}