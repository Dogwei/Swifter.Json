using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;

namespace Swifter.RW
{

    sealed class DefaultValueReader : IValueReader, IValueReader<Guid>, IValueReader<DateTimeOffset>, IValueReader<TimeSpan>
    {
        public static readonly DefaultValueReader Instance = new DefaultValueReader();

        public object DirectRead() => default;

        public void ReadArray(IDataWriter<int> valueWriter) { }

        public bool ReadBoolean() => default;

        public byte ReadByte() => default;

        public char ReadChar() => default;

        public DateTime ReadDateTime() => default;

        public decimal ReadDecimal() => default;

        public double ReadDouble() => default;

        public short ReadInt16() => default;

        public int ReadInt32() => default;

        public long ReadInt64() => default;

        public T? ReadNullable<T>() where T : struct => default;

        public void ReadObject(IDataWriter<string> valueWriter)
        {
        }

        public sbyte ReadSByte() => default;

        public float ReadSingle() => default;

        public string ReadString() => default;

        public ushort ReadUInt16() => default;

        public uint ReadUInt32() => default;

        public ulong ReadUInt64() => default;

        Guid IValueReader<Guid>.ReadValue() => default;

        TimeSpan IValueReader<TimeSpan>.ReadValue() => default;

        DateTimeOffset IValueReader<DateTimeOffset>.ReadValue() => default;
    }
}
