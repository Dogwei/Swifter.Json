
using System;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    sealed class AuxiliaryValueRW : IValueRW, IValueRW<Guid>, IValueRW<DateTimeOffset>, IValueRW<TimeSpan>, IValueWriter<IDataReader>
    {
        object rw;
        Type type;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public IDataWriter GetDataWriter()
        {
            if (rw is IAsDataWriter asWriter)
            {
                return asWriter.Original;
            }

            return rw as IDataWriter;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public IDataReader GetDataReader()
        {
            if (rw is IAsDataReader asReader)
            {
                return asReader.Original;
            }

            return rw as IDataReader;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public IDataRW GetDataRW()
        {
            if (rw is IAsDataRW asRW)
            {
                return asRW.Original;
            }

            return rw as IDataRW;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public Type GetValueType()
        {
            if (type != null)
            {
                return type;
            }

            if (rw is IDataReader reader)
            {
                return reader.ContentType;
            }

            if (rw is IDataWriter writer)
            {
                return writer.ContentType;
            }

            throw new NotSupportedException();
        }

        public object DirectRead() { type = typeof(object); return default; }

        public void DirectWrite(object value) { type = typeof(object); }

        public void ReadArray(IDataWriter<int> valueWriter) { rw = valueWriter; }

        public bool ReadBoolean() { type = typeof(bool); return default; }

        public byte ReadByte() { type = typeof(byte); return default; }

        public char ReadChar() { type = typeof(char); return default; }

        public DateTime ReadDateTime() { type = typeof(DateTime); return default; }

        public decimal ReadDecimal() { type = typeof(decimal); return default; }

        public double ReadDouble() { type = typeof(double); return default; }

        public short ReadInt16() { type = typeof(short); return default; }

        public int ReadInt32() { type = typeof(int); return default; }

        public long ReadInt64() { type = typeof(long); return default; }

        public void ReadMap<TKey>(IDataWriter<TKey> mapWriter) { rw = mapWriter; }

        public T? ReadNullable<T>() where T : struct { type = typeof(T?); return default; }

        public T ReadEnum<T>() where T : struct, Enum { type = typeof(T); return default; }

        public void ReadObject(IDataWriter<string> valueWriter) { rw = valueWriter; }

        public sbyte ReadSByte() { type = typeof(sbyte); return default; }

        public float ReadSingle() { type = typeof(float); return default; }

        public string ReadString() { type = typeof(string); return default; }

        public ushort ReadUInt16() { type = typeof(ushort); return default; }

        public uint ReadUInt32() { type = typeof(uint); return default; }

        public ulong ReadUInt64() { type = typeof(ulong); return default; }

        public void WriteArray(IDataReader<int> dataReader) { rw = dataReader; }

        public void WriteBoolean(bool value) { type = typeof(bool); }

        public void WriteByte(byte value) { type = typeof(byte); }

        public void WriteChar(char value) { type = typeof(char); }

        public void WriteDateTime(DateTime value) { type = typeof(DateTime); }

        public void WriteDecimal(decimal value) { type = typeof(decimal); }

        public void WriteDouble(double value) { type = typeof(double); }

        public void WriteInt16(short value) { type = typeof(short); }

        public void WriteInt32(int value) { type = typeof(int); }

        public void WriteInt64(long value) { type = typeof(long); }

        public void WriteMap<TKey>(IDataReader<TKey> mapReader) { rw = mapReader; }

        public void WriteObject(IDataReader<string> dataReader) { rw = dataReader; }

        public void WriteSByte(sbyte value) { type = typeof(sbyte); }

        public void WriteSingle(float value) { type = typeof(float); }

        public void WriteString(string value) { type = typeof(string); }

        public void WriteUInt16(ushort value) { type = typeof(ushort); }

        public void WriteUInt32(uint value) { type = typeof(uint); }

        public void WriteUInt64(ulong value) { type = typeof(ulong); }

        public void WriteValue(IDataReader value) { rw = value; }

        public void WriteEnum<T>(T value) where T : struct, Enum { type = typeof(T); }

        Guid IValueReader<Guid>.ReadValue() { type = typeof(Guid);return default; }

        void IValueWriter<Guid>.WriteValue(Guid value) { type = typeof(Guid); }

        DateTimeOffset IValueReader<DateTimeOffset>.ReadValue() { type = typeof(DateTimeOffset); return default; }

        void IValueWriter<DateTimeOffset>.WriteValue(DateTimeOffset value) { type = typeof(DateTimeOffset); }

        TimeSpan IValueReader<TimeSpan>.ReadValue() { type = typeof(TimeSpan); return default; }

        void IValueWriter<TimeSpan>.WriteValue(TimeSpan value) { type = typeof(TimeSpan); }
    }
}