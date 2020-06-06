
using Swifter.RW;

using System;
using System.Collections.Generic;
using System.Data.Common;

namespace Swifter.Data
{
    internal sealed class DbCommandParametersAdder : IDataWriter<string>, IValueWriter
    {
        private readonly DbCommand dbCommand;

        private string name;

        public DbCommandParametersAdder(DbCommand dbCommand)
        {
            this.dbCommand = dbCommand;
        }

        public IValueWriter this[string key]
        {
            get
            {
                name = key;

                return this;
            }
        }

        public IEnumerable<string> Keys => null;

        public int Count => -1;

        public Type ContentType => typeof(DbParameterCollection);

        public object Content
        {
            get => dbCommand.Parameters;
            set => throw new NotSupportedException();
        }

        public void DirectWrite(object value)
        {
            var item = dbCommand.CreateParameter();

            if (value == null)
            {
                value = DBNull.Value;
            }

            item.ParameterName = name;
            item.Value = value;

            dbCommand.Parameters.Add(item);
        }

        public void Initialize()
        {
        }

        public void Initialize(int capacity)
        {
        }

        public void OnWriteAll(IDataReader<string> dataReader)
        {
            foreach (DbParameter item in dbCommand.Parameters)
            {
                item.Value = ValueInterface<object>.ReadValue(dataReader[item.ParameterName]);
            }
        }

        public void OnWriteValue(string key, IValueReader valueReader)
        {
            name = key;

            DirectWrite(valueReader.DirectRead());
        }

        public void WriteArray(IDataReader<int> dataReader)
        {
            DirectWrite(dataReader.Content);
        }

        public void WriteBoolean(bool value)
        {
            DirectWrite(value);
        }

        public void WriteByte(byte value)
        {
            DirectWrite(value);
        }

        public void WriteChar(char value)
        {
            DirectWrite(value);
        }

        public void WriteDateTime(DateTime value)
        {
            DirectWrite(value);
        }

        public void WriteDecimal(decimal value)
        {
            DirectWrite(value);
        }

        public void WriteDouble(double value)
        {
            DirectWrite(value);
        }

        public void WriteInt16(short value)
        {
            DirectWrite(value);
        }

        public void WriteInt32(int value)
        {
            DirectWrite(value);
        }

        public void WriteInt64(long value)
        {
            DirectWrite(value);
        }

        public void WriteObject(IDataReader<string> dataReader)
        {
            DirectWrite(dataReader.Content);
        }

        public void WriteSByte(sbyte value)
        {
            DirectWrite(value);
        }

        public void WriteSingle(float value)
        {
            DirectWrite(value);
        }

        public void WriteString(string value)
        {
            DirectWrite(value);
        }

        public void WriteUInt16(ushort value)
        {
            DirectWrite(value);
        }

        public void WriteUInt32(uint value)
        {
            DirectWrite(value);
        }

        public void WriteUInt64(ulong value)
        {
            DirectWrite(value);
        }

        void IValueWriter.WriteEnum<T>(T value)
        {
            DirectWrite(value);
        }
    }
}
