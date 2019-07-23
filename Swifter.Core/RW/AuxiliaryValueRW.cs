using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    sealed class AuxiliaryValueRW : IValueRW, IValueWriter<IDataReader>, IMapValueRW
    {
        object RW;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public IDataWriter GetDataWriter()
        {
            if (RW is IAsDataWriter asWriter)
            {
                return asWriter.Content;
            }

            return RW as IDataWriter;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public IDataReader GetDataReader()
        {
            if (RW is IAsDataReader asReader)
            {
                return asReader.Content;
            }

            return RW as IDataReader;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public IDataRW GetDataRW()
        {
            if (RW is IAsDataRW asRW)
            {
                return asRW.Content;
            }

            return RW as IDataRW;
        }

        public object DirectRead() => default;

        public void DirectWrite(object value) { }

        public void ReadArray(IDataWriter<int> valueWriter)
        {
            RW = valueWriter;
        }

        public bool ReadBoolean() => default;

        public byte ReadByte() => default;

        public char ReadChar() => default;

        public DateTime ReadDateTime() => default;

        public decimal ReadDecimal() => default;

        public double ReadDouble() => default;

        public short ReadInt16() => default;

        public int ReadInt32() => default;

        public long ReadInt64() => default;

        public void ReadMap<TKey>(IDataWriter<TKey> mapWriter)
        {
            RW = mapWriter;
        }

        public T? ReadNullable<T>() where T : struct => default;

        public void ReadObject(IDataWriter<string> valueWriter)
        {
            RW = valueWriter;
        }

        public sbyte ReadSByte() => default;

        public float ReadSingle() => default;

        public string ReadString() => default;

        public ushort ReadUInt16() => default;

        public uint ReadUInt32() => default;

        public ulong ReadUInt64() => default;

        public void WriteArray(IDataReader<int> dataReader)
        {
            RW = dataReader;
        }

        public void WriteBoolean(bool value) { }

        public void WriteByte(byte value) { }

        public void WriteChar(char value) { }

        public void WriteDateTime(DateTime value) { }

        public void WriteDecimal(decimal value) { }

        public void WriteDouble(double value) { }

        public void WriteInt16(short value) { }

        public void WriteInt32(int value) { }

        public void WriteInt64(long value) { }

        public void WriteMap<TKey>(IDataReader<TKey> mapReader)
        {
            RW = mapReader;
        }

        public void WriteObject(IDataReader<string> dataReader)
        {
            RW = dataReader;
        }

        public void WriteSByte(sbyte value) { }

        public void WriteSingle(float value) { }

        public void WriteString(string value) { }

        public void WriteUInt16(ushort value) { }

        public void WriteUInt32(uint value) { }

        public void WriteUInt64(ulong value) { }

        public void WriteValue(IDataReader value)
        {
            RW = value;
        }
    }
}
