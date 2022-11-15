
using Swifter.RW;
using Swifter.Tools;

using System;

namespace Swifter.Reflection
{
    sealed class XSkipDefaultValueWriter<TKey> : IValueWriter, IValueWriter<Guid>, IValueWriter<DateTimeOffset>, IValueWriter<TimeSpan> where TKey : notnull
    {
        public readonly IDataWriter<TKey> DataWriter;
        public readonly TKey Key;

        public XSkipDefaultValueWriter(IDataWriter<TKey> dataWriter, TKey key)
        {
            DataWriter = dataWriter;
            Key = key;
        }

        public IValueWriter ValueWriter => DataWriter[Key];

        public Type? ValueType => null;

        public void DirectWrite(object? value)
        {
            if (!TypeHelper.IsEmptyValue(value)) ValueWriter.DirectWrite(value);
        }

        public void WriteArray(IDataReader<int> dataReader)
        {
            if (dataReader.ContentType is Type contentType && contentType.IsValueType && TypeHelper.IsEmptyValue(dataReader.Content))
            {
                return;
            }

            ValueWriter.WriteArray(dataReader);
        }

        public void WriteObject(IDataReader<string> dataReader)
        {
            if (dataReader.ContentType is Type contentType && contentType.IsValueType && TypeHelper.IsEmptyValue(dataReader.Content))
            {
                return;
            }

            ValueWriter.WriteObject(dataReader);
        }

        public void WriteBoolean(bool value)
        {
            if (value != default) ValueWriter.WriteBoolean(value);
        }

        public void WriteByte(byte value)
        {
            if (value != default) ValueWriter.WriteByte(value);
        }

        public void WriteChar(char value)
        {
            if (value != default) ValueWriter.WriteChar(value);
        }

        public void WriteDateTime(DateTime value)
        {
            if (value != default) ValueWriter.WriteDateTime(value);
        }

        public void WriteDecimal(decimal value)
        {
            if (value != default) ValueWriter.WriteDecimal(value);
        }

        public void WriteDouble(double value)
        {
            if (value != default) ValueWriter.WriteDouble(value);
        }

        public void WriteInt16(short value)
        {
            if (value != default) ValueWriter.WriteInt16(value);
        }

        public void WriteInt32(int value)
        {
            if (value != default) ValueWriter.WriteInt32(value);
        }

        public void WriteInt64(long value)
        {
            if (value != default) ValueWriter.WriteInt64(value);
        }

        public void WriteSByte(sbyte value)
        {
            if (value != default) ValueWriter.WriteSByte(value);
        }

        public void WriteSingle(float value)
        {
            if (value != default) ValueWriter.WriteSingle(value);
        }

        public void WriteString(string? value)
        {
            if (value != default) ValueWriter.WriteString(value);
        }

        public void WriteUInt16(ushort value)
        {
            if (value != default) ValueWriter.WriteUInt16(value);
        }

        public void WriteUInt32(uint value)
        {
            if (value != default) ValueWriter.WriteUInt32(value);
        }

        public void WriteUInt64(ulong value)
        {
            if (value != default) ValueWriter.WriteUInt64(value);
        }

        void IValueWriter.WriteEnum<T>(T value)
        {
            if (EnumHelper.AsUInt64(value) != default) ValueWriter.WriteEnum(value);
        }

        public void WriteGuid(Guid value)
        {
            if (value != default)
            {
                var valueWriter = ValueWriter;

                if (valueWriter is IValueWriter<Guid> writer)
                {
                    writer.WriteValue(value);
                }
                else
                {
                    ValueInterface.WriteValue(valueWriter, value);
                }
            }
        }

        public void WriteDateTimeOffset(DateTimeOffset value)
        {
            if (value != default)
            {
                var valueWriter = ValueWriter;

                if (valueWriter is IValueWriter<DateTimeOffset> writer)
                {
                    writer.WriteValue(value);
                }
                else
                {
                    ValueInterface.WriteValue(valueWriter, value);
                }
            }
        }

        public void WriteTimeSpan(TimeSpan value)
        {
            if (value != default)
            {
                var valueWriter = ValueWriter;

                if (valueWriter is IValueWriter<TimeSpan> writer)
                {
                    writer.WriteValue(value);
                }
                else
                {
                    ValueInterface.WriteValue(valueWriter, value);
                }
            }
        }

        void IValueWriter<Guid>.WriteValue(Guid value) => WriteGuid(value);

        void IValueWriter<DateTimeOffset>.WriteValue(DateTimeOffset value) => WriteDateTimeOffset(value);

        void IValueWriter<TimeSpan>.WriteValue(TimeSpan value) => WriteTimeSpan(value);
    }
}