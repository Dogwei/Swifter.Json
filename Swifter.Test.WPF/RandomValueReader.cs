using Swifter.RW;
using Swifter.Tools;
using System;
using System.Linq;
using System.Text;

namespace Swifter.Test.WPF
{
    public class RandomValueReader : IValueReader, IValueReader<Guid>, IValueReader<DateTimeOffset>, IValueReader<TimeSpan>
    {
        public readonly Random random;

        public RandomValueReader(int seed)
        {
            random = new Random(seed);
        }

        public object DirectRead()
        {
            try
            {
                return ValueInterface.GetInterface(ReadType()).Read(this);
            }
            catch
            {
                return null;
            }
        }

        public Type ReadType()
        {
            var val = ReadByte();

            while (true)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!type.IsInterface && !type.IsAbstract && !type.IsGenericTypeDefinition)
                        {
                            --val;

                            if (val == 0)
                            {
                                return type;
                            }
                        }
                    }
                }
            }
        }

        public void ReadArray(IDataWriter<int> valueWriter)
        {
            try
            {
                var count = random.Next(0, 300);

                valueWriter.Initialize(count);

                for (int i = 0; i < count; i++)
                {
                    valueWriter.OnWriteValue(i, this);
                }
            }
            catch
            {
                if (valueWriter.ContentType != null)
                {
                    valueWriter.Content = TypeHelper.GetDefaultValue(valueWriter.ContentType);
                }
            }
        }

        public bool ReadBoolean()
        {
            return (random.Next(int.MinValue, int.MaxValue) & 1) == 0;
        }

        public byte ReadByte()
        {
            return (byte)ReadInt32();
        }

        public static readonly char[] chars = "0123456789qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM,./;'[]=-~!@#$%^&*()_+{}:\"<>?\r\n\t\b\0我爱你愛してる№℡©™※★℃℉‱‰％∰∯".ToCharArray();
        public static readonly string[] strings = chars.Select(c => c.ToString()).Concat(new string[] { "🐷", "👩‍👩‍👦‍👦", "😀", "😁", "😂" }).ToArray();

        public char ReadChar()
        {
            return chars[random.Next(chars.Length)];
        }

        public unsafe long NextInt64(long minValue, long maxValue)
        {
            long val = 0;

            random.NextBytes(new Span<byte>(&val, sizeof(long)));

            if (val < 0) val = -val;

            return (val % (maxValue - minValue)) + minValue;
        }

        public DateTime ReadDateTime()
        {
            return new DateTime(NextInt64(DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks + 1));
        }

        public decimal ReadDecimal()
        {
            return new decimal(ReadInt32(), ReadInt32(), ReadInt32(), ReadBoolean(), (byte)random.Next(0, 28));
        }

        public double ReadDouble()
        {
            return TypeHelper.As<long, double>(ReadInt64());
        }

        public short ReadInt16()
        {
            return (short)ReadInt32();
        }

        public int ReadInt32()
        {
            return random.Next(int.MinValue, int.MaxValue);
        }

        public long ReadInt64()
        {
            return NextInt64(int.MinValue, int.MaxValue);
        }

        public T? ReadNullable<T>() where T : struct
        {
            if (ReadBoolean()) return null;

            return ValueInterface<T>.ReadValue(this);
        }

        public void ReadObject(IDataWriter<string> valueWriter)
        {
            try
            {
                valueWriter.Initialize();

                if (valueWriter.Count >= 0)
                {
                    foreach (var name in valueWriter.Keys)
                    {
                        valueWriter.OnWriteValue(name, this);
                    }
                }
                else
                {
                    for (int i = ReadByte(); i >= 0; i--)
                    {
                        valueWriter.OnWriteValue(ReadString(), this);
                    }
                }
            }
            catch
            {
                if (valueWriter.ContentType != null)
                {
                    valueWriter.Content = TypeHelper.GetDefaultValue(valueWriter.ContentType);
                }
            }
        }

        public sbyte ReadSByte()
        {
            return (sbyte)ReadInt32();
        }

        public float ReadSingle()
        {
            return TypeHelper.As<int, float>(ReadInt32());
        }

        public string ReadString()
        {
            var count = random.Next(0, 1000);

            var sb = new StringBuilder();

            for (int i = 0; i < count; i++)
            {
                sb.Append(strings[random.Next(0, strings.Length)]);
            }

            return sb.ToString();
        }

        public ushort ReadUInt16()
        {
            return (ushort)ReadInt32();
        }

        public uint ReadUInt32()
        {
            return (uint)ReadInt32();
        }

        public ulong ReadUInt64()
        {
            return (ulong)ReadInt64();
        }

        T IValueReader.ReadEnum<T>()
        {
            if (ReadBoolean())
            {
                var values = (T[])typeof(T).GetEnumValues();

                return values[random.Next(0, values.Length)];
            }
            else if (ReadBoolean())
            {
                var values = (T[])typeof(T).GetEnumValues();

                ulong val = 0;

                for (int i = random.Next(0, values.Length); i >= 0; i--)
                {
                    val |= EnumHelper.AsUInt64(values[random.Next(0, values.Length)]);
                }

                return EnumHelper.AsEnum<T>(val);
            }
            else
            {
                return EnumHelper.AsEnum<T>(ReadUInt64());
            }
        }

        Guid IValueReader<Guid>.ReadValue()
        {
            return Guid.NewGuid();
        }

        DateTimeOffset IValueReader<DateTimeOffset>.ReadValue()
        {
            return new DateTimeOffset(ReadDateTime(), ((IValueReader<TimeSpan>)this).ReadValue());
        }

        TimeSpan IValueReader<TimeSpan>.ReadValue()
        {
            return new TimeSpan(NextInt64(TimeSpan.MinValue.Ticks, TimeSpan.MaxValue.Ticks + 1));
        }
    }
}
