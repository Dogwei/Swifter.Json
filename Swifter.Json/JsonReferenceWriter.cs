
using Swifter.RW;
using Swifter.Tools;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    sealed class JsonReferenceWriter : BaseCache<object, RWPathInfo>, IValueWriter, IDataWriter<string>, IDataWriter<int>
    {
        public RWPathInfo Reference;

        string Name;

        int Index;

        public JsonReferenceWriter() : base(0)
        {
            Reference = RWPathInfo.Root;
        }

        public IValueWriter this[string key]
        {
            get
            {
                Name = key;

                return this;
            }
        }

        public IValueWriter this[int key]
        {
            get
            {
                Index = key;

                return this;
            }
        }

        IEnumerable<string> IDataWriter<string>.Keys => null;

        IEnumerable<int> IDataWriter<int>.Keys => null;

        public void Initialize()
        {
        }

        public void Initialize(int capacity)
        {
        }

        public void OnWriteValue(string key, IValueReader valueReader)
        {
            ValueInterface.WriteValue(this[key], valueReader.DirectRead());
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            ValueInterface.WriteValue(this[key], valueReader.DirectRead());
        }

        public void OnWriteAll(IDataReader<string> dataReader)
        {
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
        }

        public void WriteBoolean(bool value)
        {
        }

        public void WriteByte(byte value)
        {
        }

        public void WriteSByte(sbyte value)
        {
        }

        public void WriteInt16(short value)
        {
        }

        public void WriteChar(char value)
        {
        }

        public void WriteUInt16(ushort value)
        {
        }

        public void WriteInt32(int value)
        {
        }

        public void WriteSingle(float value)
        {
        }

        public void WriteUInt32(uint value)
        {
        }

        public void WriteInt64(long value)
        {
        }

        public void WriteDouble(double value)
        {
        }

        public void WriteUInt64(ulong value)
        {
        }

        public void WriteString(string value)
        {
        }

        public void WriteDateTime(DateTime value)
        {
        }

        public void WriteDecimal(decimal value)
        {
        }

        public void WriteObject(IDataReader<string> dataReader)
        {
            SaveReference(dataReader);
        }

        public void SaveReference(IDataReader dataReader)
        {
            RWPathInfo reference;

            var token = dataReader.ReferenceToken;

            if (Name != null)
            {
                reference = RWPathInfo.Create(Name, Reference);
            }
            else
            {
                reference = RWPathInfo.Create(Index, Reference);
            }

            if (token != null)
            {
                TryAdd(token, reference);
            }
        }

        public void WriteArray(IDataReader<int> dataReader)
        {
            SaveReference(dataReader);
        }

        public void DirectWrite(object value)
        {
        }

        protected override int ComputeHashCode(object key)
        {
            return RuntimeHelpers.GetHashCode(key);
        }

        protected override bool Equals(object key1, object key2)
        {
            return key1 == key2;
        }
    }
}