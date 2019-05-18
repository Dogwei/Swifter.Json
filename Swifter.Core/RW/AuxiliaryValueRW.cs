using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;

namespace Swifter.RW
{
    sealed class AuxiliaryValueRW : IValueRW, IValueWriter<IDataReader>
    {
        const string WritingExceptionText_Type = "Unable to create data reader/writer of the type '{0}'.";

        public object read_writer;
        
        public object DirectRead()
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Object)));
        }

        public void DirectWrite(object value)
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Object)));
        }

        public void ReadArray(IDataWriter<int> valueWriter)
        {
            read_writer = valueWriter;
        }

        public bool ReadBoolean()
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Boolean)));
        }

        public byte ReadByte()
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Byte)));
        }

        public char ReadChar()
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Char)));
        }

        public DateTime ReadDateTime()
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(DateTime)));
        }

        public decimal ReadDecimal()
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Decimal)));
        }

        public double ReadDouble()
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Double)));
        }

        public short ReadInt16()
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Int16)));
        }

        public int ReadInt32()
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Int32)));
        }

        public long ReadInt64()
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Int64)));
        }

        public T? ReadNullable<T>() where T : struct
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, typeof(T?).ToString()));
        }

        public void ReadObject(IDataWriter<string> valueWriter)
        {
            read_writer = valueWriter;
        }

        public sbyte ReadSByte()
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(SByte)));
        }

        public float ReadSingle()
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Single)));
        }

        public string ReadString()
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(String)));
        }

        public ushort ReadUInt16()
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(UInt16)));
        }

        public uint ReadUInt32()
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(UInt32)));
        }

        public ulong ReadUInt64()
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(UInt64)));
        }

        public void WriteArray(IDataReader<int> dataReader)
        {
            read_writer = dataReader;
        }

        public void WriteBoolean(bool value)
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Boolean)));
        }

        public void WriteByte(byte value)
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Byte)));
        }

        public void WriteChar(char value)
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Char)));
        }

        public void WriteDateTime(DateTime value)
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(DateTime)));
        }

        public void WriteDecimal(decimal value)
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Decimal)));
        }

        public void WriteDouble(double value)
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Double)));
        }

        public void WriteInt16(short value)
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Int16)));
        }

        public void WriteInt32(int value)
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Int32)));
        }

        public void WriteInt64(long value)
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Int64)));
        }

        public void WriteObject(IDataReader<string> dataReader)
        {
            read_writer = dataReader;
        }

        public void WriteSByte(sbyte value)
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(SByte)));
        }

        public void WriteSingle(float value)
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(Single)));
        }

        public void WriteString(string value)
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(String)));
        }

        public void WriteUInt16(ushort value)
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(UInt16)));
        }

        public void WriteUInt32(uint value)
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(UInt32)));
        }

        public void WriteUInt64(ulong value)
        {
            throw new NotSupportedException(StringHelper.Format(WritingExceptionText_Type, nameof(UInt64)));
        }

        public void WriteValue(IDataReader value)
        {
            read_writer = value;
        }
    }
}
