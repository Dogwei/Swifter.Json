using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;

namespace Swifter.Json
{
    sealed class PriorCheckReferenceWriter : IValueWriter, IDataWriter<string>, IDataWriter<int>
    {
        private readonly ReferenceCache<TargetPathInfo> References;
        private readonly TargetPathInfo ParentReference;

        private TargetPathInfo CurrentReference;

        public PriorCheckReferenceWriter(ReferenceCache<TargetPathInfo> references, TargetPathInfo parentReference)
        {
            References = references;
            ParentReference = parentReference;
        }

        public IValueWriter this[string key]
        {
            get
            {
                CurrentReference = new TargetPathInfo(key, ParentReference);

                return this;
            }
        }

        public IValueWriter this[int key]
        {
            get
            {
                CurrentReference = new TargetPathInfo(key, ParentReference);

                return this;
            }
        }

        IEnumerable<string> IDataWriter<string>.Keys => null;

        IEnumerable<int> IDataWriter<int>.Keys => null;

        public int Count => 0;

        public void Initialize()
        {
        }

        public void Initialize(int capacity)
        {
        }

        public void OnWriteValue(string key, IValueReader valueReader)
        {
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
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
            var token = dataReader.ReferenceToken;

            if (token != null)
            {
                References.TryAdd(token, CurrentReference);
            }
        }

        public void WriteArray(IDataReader<int> dataReader)
        {
            var token = dataReader.ReferenceToken;

            if (token != null)
            {
                References.TryAdd(token, CurrentReference);
            }
        }

        public void DirectWrite(object value)
        {
        }
    }
}