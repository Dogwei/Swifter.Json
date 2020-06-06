
using Swifter.RW;
using Swifter.Tools;

using System;
using System.Collections.Generic;

namespace Swifter.Json
{
    sealed class JsonReferenceWriter : Dictionary<object, RWPathInfo>, IValueWriter, IDataWriter<string>, IDataWriter<int>
    {
        public RWPathInfo Reference;

        public JsonReferenceWriter() : base(TypeHelper.ReferenceComparer)
        {
            Reference = RWPathInfo.Root;
        }

        public IValueWriter this[string key]
        {
            get
            {
                RWPathInfo.SetPath(Reference, key);

                return this;
            }
        }

        public IValueWriter this[int key]
        {
            get
            {
                RWPathInfo.SetPath(Reference, key);

                return this;
            }
        }

        IEnumerable<string> IDataWriter<string>.Keys => null;

        IEnumerable<int> IDataWriter<int>.Keys => null;

        public Type ContentType => throw new NotSupportedException();

        public object Content
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

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

        public void WriteEnum<T>(T value) where T : struct, Enum
        {
        }

        public void WriteObject(IDataReader<string> dataReader)
        {
            SaveReference(dataReader);
        }

        public void SaveReference(IDataReader dataReader)
        {
            if (dataReader.ContentType?.IsValueType == false)
            {
                this.TryAdd(dataReader.Content, Reference.Clone());
            }
        }

        public void WriteArray(IDataReader<int> dataReader)
        {
            SaveReference(dataReader);
        }

        public void DirectWrite(object value)
        {
        }
    }
}