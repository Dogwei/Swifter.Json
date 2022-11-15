
using System;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    sealed class AuxiliaryValueRW : IValueRW, IValueWriter<IDataReader>
    {
        object? rw;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public IDataWriter? GetDataWriter()
        {
            if (rw is IAsDataWriter asWriter)
            {
                return asWriter.Original;
            }

            return rw as IDataWriter;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public IDataReader? GetDataReader()
        {
            if (rw is IAsDataReader asReader)
            {
                return asReader.Original;
            }

            return rw as IDataReader;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public IDataRW? GetDataRW()
        {
            if (rw is IAsDataRW asRW)
            {
                return asRW.Original;
            }

            return rw as IDataRW;
        }

        public Type? ValueType => null;

        public void ReadArray(IDataWriter<int> valueWriter) { rw = valueWriter; }

        public void ReadObject(IDataWriter<string> valueWriter) { rw = valueWriter; }

        public object? DirectRead() { return default; }

        public void Pop() { }

        public bool ReadBoolean() { return default; }

        public byte ReadByte() { return default; }

        public char ReadChar() { return default; }

        public DateTime ReadDateTime() { return default; }

        public decimal ReadDecimal() { return default; }

        public double ReadDouble() { return default; }

        public short ReadInt16() { return default; }

        public int ReadInt32() { return default; }

        public long ReadInt64() { return default; }

        public T? ReadNullable<T>() where T : struct { return default; }

        public T ReadEnum<T>() where T : struct, Enum { return default; }

        public sbyte ReadSByte() { return default; }

        public float ReadSingle() { return default; }

        public string? ReadString() { return default; }

        public ushort ReadUInt16() { return default; }

        public uint ReadUInt32() { return default; }

        public ulong ReadUInt64() { return default; }

        public Guid ReadGuid() { return default; }

        public DateTimeOffset ReadDateTimeOffset() { return default; }

        public TimeSpan ReadTimeSpan() { return default; }



        public void WriteArray(IDataReader<int> dataReader) { rw = dataReader; }

        public void WriteObject(IDataReader<string> dataReader) { rw = dataReader; }

        public void DirectWrite(object? value) { }

        public void WriteBoolean(bool value) { }

        public void WriteByte(byte value) { }

        public void WriteChar(char value) { }

        public void WriteDateTime(DateTime value) { }

        public void WriteDecimal(decimal value) { }

        public void WriteDouble(double value) { }

        public void WriteInt16(short value) { }

        public void WriteInt32(int value) { }

        public void WriteInt64(long value) { }

        public void WriteSByte(sbyte value) { }

        public void WriteSingle(float value) { }

        public void WriteString(string? value) { }

        public void WriteUInt16(ushort value) { }

        public void WriteUInt32(uint value) { }

        public void WriteUInt64(ulong value) { }

        public void WriteGuid(Guid value) { }

        public void WriteDateTimeOffset(DateTimeOffset value) { }

        public void WriteTimeSpan(TimeSpan value) { }

        public void WriteEnum<T>(T value) where T : struct, Enum { }

        void IValueWriter<IDataReader>.WriteValue(IDataReader? value) { rw = value; }
    }
}