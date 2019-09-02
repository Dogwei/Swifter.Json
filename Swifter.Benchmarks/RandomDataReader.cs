using Swifter.RW;
using System;
using System.Text;

namespace Swifter.Benchmarks
{
    sealed class RandomDataReader : IValueReader, IValueReader<DateTimeOffset>, IValueReader<TimeSpan>, IValueReader<Guid>
    {
        public int MinStringSize = 1;
        public int MaxStringSize = 1000;

        public int MinObjectSize = 0;
        public int MaxObjectSize = 1000;

        public int MinArraySize = 0;
        public int MaxArraySize = 10000;

        readonly Random RandomInstance = new Random();

        public object DirectRead()
        {
            throw new NotImplementedException();
        }

        public void ReadArray(IDataWriter<int> valueWriter)
        {
            valueWriter.Initialize();

            if (valueWriter.Count != 0 && valueWriter.Keys != null)
            {
                foreach (var key in valueWriter.Keys)
                {
                    valueWriter.OnWriteValue(key, this);
                }
            }
            else
            {
                var length = RandomInstance.Next(MinArraySize, MaxArraySize);

                for (int i = 0; i < length; i++)
                {
                    valueWriter.OnWriteValue(i, this);
                }
            }
        }

        public bool ReadBoolean()
        {
            return (RandomInstance.Next() & 1) == 1;
        }

        public byte ReadByte()
        {
            return (byte)RandomInstance.Next();
        }

        public char ReadChar()
        {
            while (true)
            {
                var utf16 = (char)RandomInstance.Next();

                if (utf16 >= 0xd800 && utf16 <= 0xdfff)
                {
                    continue;
                }

                return utf16;
            }
        }

        public DateTime ReadDateTime()
        {
            unsafe
            {
                Span<byte> bytes = stackalloc byte[sizeof(DateTime)];

                while (true)
                {
                    RandomInstance.NextBytes(bytes);

                    try
                    {
                        var dt = Unsafe.As<byte, DateTime>(ref bytes[0]);


                        if (dt >= DateTime.MinValue && dt <= DateTime.MaxValue)
                        {
                            dt.ToString();

                            return dt;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public decimal ReadDecimal()
        {
            unsafe
            {
                Span<byte> bytes = stackalloc byte[sizeof(decimal)];

                while (true)
                {
                    RandomInstance.NextBytes(bytes);

                    try
                    {
                        var dec = Unsafe.As<byte, decimal>(ref bytes[0]);

                        if (dec >= decimal.MinValue && dec <= decimal.MaxValue)
                        {
                            dec.ToString();

                            return dec;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        public double ReadDouble()
        {
            while (true)
            {
                var dou = Unsafe.As<long, double>(ref Unsafe.AsRef(ReadInt64()));

                if (dou >= double.MinValue && dou <= double.MaxValue)
                {
                    return dou;
                }
            }
        }

        public short ReadInt16()
        {
            return (short)RandomInstance.Next();
        }

        public int ReadInt32()
        {
            return RandomInstance.Next();
        }

        public long ReadInt64()
        {
            var lon = 0L;

            unsafe
            {
                RandomInstance.NextBytes(new Span<byte>(&lon, sizeof(long)));
            }

            return lon;
        }

        public T? ReadNullable<T>() where T : struct
        {
            if ((RandomInstance.Next()&3) == 3)
            {
                return null;
            }

            return ValueInterface<T>.ReadValue(this);
        }

        public void ReadObject(IDataWriter<string> valueWriter)
        {
            valueWriter.Initialize();

            if (valueWriter.Keys != null)
            {
                foreach (var key in valueWriter.Keys)
                {
                    valueWriter.OnWriteValue(key, this);
                }
            }
            else
            {
                for (int i = RandomInstance.Next(MinObjectSize, MaxObjectSize); i >= 0; --i)
                {
                    valueWriter.OnWriteValue(ReadString(), this);
                }
            }
        }

        public sbyte ReadSByte()
        {
            return (sbyte)RandomInstance.Next();
        }

        public float ReadSingle()
        {
            while (true)
            {
                var flo = Unsafe.As<int, float>(ref Unsafe.AsRef(ReadInt32()));

                if (flo >= float.MinValue && flo <= float.MaxValue)
                {
                    return flo;
                }
            }
        }

        public string ReadString()
        {
            var sb = new StringBuilder();

            for (int i = RandomInstance.Next(MinStringSize, MaxStringSize); i >=0; --i)
            {
                var utf32 = RandomInstance.Next(0, 0x10ffff);

                if (utf32>= 0xd800 && utf32<=0xdfff)
                {
                    continue;
                }

                sb.Append(char.ConvertFromUtf32(utf32));
            }

            return sb.ToString();
        }

        public ushort ReadUInt16()
        {
            return (ushort)RandomInstance.Next();
        }

        public uint ReadUInt32()
        {
            return (uint)RandomInstance.Next();
        }

        public ulong ReadUInt64()
        {
            return (ulong)ReadInt64();
        }

        DateTimeOffset IValueReader<DateTimeOffset>.ReadValue()
        {
            unsafe
            {
                Span<byte> bytes = stackalloc byte[sizeof(DateTimeOffset)];

                while (true)
                {
                    RandomInstance.NextBytes(bytes);

                    try
                    {
                        var dto = Unsafe.As<byte, DateTimeOffset>(ref bytes[0]);


                        if (dto >= DateTimeOffset.MinValue && dto <= DateTimeOffset.MaxValue)
                        {
                            dto.ToString();

                            return dto;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        TimeSpan IValueReader<TimeSpan>.ReadValue()
        {
            unsafe
            {
                Span<byte> bytes = stackalloc byte[sizeof(TimeSpan)];

                while (true)
                {
                    RandomInstance.NextBytes(bytes);

                    try
                    {
                        var ts = Unsafe.As<byte, TimeSpan>(ref bytes[0]);


                        if (ts >= TimeSpan.MinValue && ts <= TimeSpan.MaxValue)
                        {
                            ts.ToString();

                            return ts;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        Guid IValueReader<Guid>.ReadValue()
        {
            return Guid.NewGuid();
        }
    }
}
