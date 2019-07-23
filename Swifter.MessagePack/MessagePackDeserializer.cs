using Swifter.Formatters;
using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using static Swifter.MessagePack.EncodingHelper;
using static Swifter.MessagePack.MessagePackCode;

namespace Swifter.MessagePack
{
    sealed unsafe class MessagePackDeserializer :
        IFormatterReader,
        IValueReader<byte[]>,
        IValueReader<Guid>,
        IValueReader<DateTimeOffset>,
        IMapValueReader
    {
        public readonly MessagePackForamtter foramtter;
        public readonly HGlobalCache<char> hGlobal;

        public readonly byte* begin;
        public readonly byte* end;

        public byte* current;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public MessagePackDeserializer(
            byte* bytes,
            int length,
            HGlobalCache<char> hGlobal,
            MessagePackForamtter foramtter) : this(bytes, length, hGlobal)
        {
            this.foramtter = foramtter;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public MessagePackDeserializer(
            byte* bytes,
            int length,
            HGlobalCache<char> hGlobal)
        {
            if (bytes == null || length <= 0)
            {
                throw new ArgumentNullException(nameof(bytes));
            }

            begin = bytes;
            end = bytes + length;

            current = bytes;
            
            this.hGlobal = hGlobal;
        }

        public long TargetedId => foramtter?.id ?? 0;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public object DirectRead()
        {
            var curr = Read();

            int length;

            if (curr >= FixInt && curr <= FixIntMax)
            {
                return curr/* - FixInt*/;
            }

            if (curr >= FixNegativeInt && curr <= FixNegativeIntMax)
            {
                return (sbyte)curr;
            }

            if (curr >= FixMap && curr <= FixMapMax)
            {
                length = curr - FixMap;

                goto Map;
            }

            if (curr >= FixArray && curr <= FixArrayMax)
            {
                length = curr - FixArray;

                goto Array;
            }

            if (curr >= FixStr && curr <= FixStrMax)
            {
                length = curr - FixStr;

                goto String;
            }

            switch (curr)
            {
                case Nil:
                    return null;
                case False:
                    return false;
                case True:
                    return true;
                case Bin8:
                    length = Read();
                    goto Binary;
                case Bin16:
                    length = Read2();
                    goto Binary;
                case Bin32:
                    length = checked((int)Read4());
                    goto Binary;
                case Ext8:
                    length = Read();
                    goto Extension;
                case Ext16:
                    length = Read2();
                    goto Extension;
                case Ext32:
                    length = checked((int)Read4());
                    goto Extension;
                case FixExt1:
                    length = 1;
                    goto Extension;
                case FixExt2:
                    length = 2;
                    goto Extension;
                case FixExt4:
                    length = 4;
                    goto Extension;
                case FixExt8:
                    length = 8;
                    goto Extension;
                case FixExt16:
                    length = 16;
                    goto Extension;
                case Float32:
                    return Unsafe.As<uint, float>(ref Unsafe.AsRef(Read4()));
                case Float64:
                    return Unsafe.As<ulong, double>(ref Unsafe.AsRef(Read8()));
                case UInt8:
                    return Read();
                case MessagePackCode.UInt16:
                    return Read2();
                case MessagePackCode.UInt32:
                    return Read4();
                case MessagePackCode.UInt64:
                    return Read8();
                case Int8:
                    return (sbyte)Read();
                case MessagePackCode.Int16:
                    return (short)Read2();
                case MessagePackCode.Int32:
                    return (int)Read4();
                case MessagePackCode.Int64:
                    return (long)Read8();
                case Str8:
                    length = Read();
                    goto String;
                case Str16:
                    length = Read2();
                    goto String;
                case Str32:
                    length = checked((int)Read4());
                    goto String;
                case Array16:
                case Array32:
                    goto Array;
                case Map16:
                case Map32:
                    goto Map;
                default:
                    throw new InvalidOperationException("NeverUsed bit.");
            }

        Map:
            BackOff();

            return ValueInterface<MessagePackMap>.ReadValue(this);


        Array:
            BackOff();

            return ValueInterface<MessagePackArray>.ReadValue(this);


        String:
            return ReadStringMode2(length);

        Binary:
            var binary = new byte[length];

            ReadBinary(ref binary[0], length);

            return binary;


        Extension:
            var code = Read();

            switch (code)
            {
                case MessagePackExtensionCode.Timestamp:
                    return ReadExtDateTime(length);
            }

            return new MessagePackExtension(length, code, this);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool IsArrayHead()
        {
            var curr = Current();

            return (curr >= FixArray && curr <= FixArrayMax) || curr == Array16 || curr == Array32;
        }

        public int TryReadArrayHead()
        {
            var curr = Read();

            if (curr >= FixArray && curr <= FixArrayMax)
            {
                return curr - FixArray;
            }

            switch (curr)
            {
                case Array16:
                    return Read2();
                case Array32:
                    return checked((int)Read4());
            }

            BackOff();

            return -1;
        }

        public void ReadArray(IDataWriter<int> valueWriter)
        {
            var length = TryReadArrayHead();

            if (length >= 0)
            {
                valueWriter.Initialize(length);

                for (int i = 0; i < length; i++)
                {
                    valueWriter.OnWriteValue(i, this);
                }

                return;
            }

            if (TryReadNull()) return; // Null direct return;

            if (IsMapHead())
            {
                ReadMap(valueWriter);

                return;
            }

            throw new InvalidOperationException("The msgpack node is not an array.");
        }

        public bool ReadBoolean()
        {
            var curr = Read();

            switch (curr)
            {
                case True:
                    return true;
                case False:
                case Nil:
                case FixInt:
                    return false;
            }

            if (curr > FixInt && curr <= FixIntMax) return true;

            if (curr >= FixNegativeInt && curr <= FixNegativeIntMax) return true;

            BackOff();

            return Convert.ToBoolean(DirectRead());
        }

        public byte ReadByte() => checked((byte)ReadUInt64());

        public char ReadChar() => char.Parse(ReadString());

        private DateTime ReadExtDateTime(int size)
        {
            ulong nanoseconds = 0;
            ulong seconds;

            switch (size)
            {
                case sizeof(uint):
                    seconds = Read4();
                    break;
                case sizeof(ulong):
                    seconds = Read8();
                    nanoseconds = (uint)(seconds >> 34);
                    seconds &= UInt34MaxValue;
                    break;
                case sizeof(uint) + sizeof(ulong):
                    nanoseconds = Read4();
                    seconds = Read8();
                    break;
                default:
                    throw new FormatException("Unrecognized timestamp format.");
            }

            var ticks = checked((long)unchecked(
                seconds * DateTimeHelper.TicksOfOneSecond +
                nanoseconds / DateTimeHelper.NanosecondsOfTick));

            return new DateTime(checked(
                DateTimeHelper.TicksOfUnixEpoch +
                DateTimeHelper.TicksOfUTCDifference +
                ticks));
        }

        public bool TryReadExtDateTime(out DateTime result)
        {
            result = default;

            var size = TryReadExtensionHead();

            if (size >= 1)
            {
                var code = Read();

                if (code == MessagePackExtensionCode.Timestamp)
                {
                    result = ReadExtDateTime(size);
                }
                else
                {
                    result = Convert.ToDateTime(new MessagePackExtension(size, code, this));
                }

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool TryReadParsedOfStr<T>(out T value)
        {
            value = default;

            var strBytesLength = TryReadStringHead();

            if (strBytesLength > 0)
            {
                ReadStringToHGlobal(strBytesLength);

                if (typeof(T) == typeof(DateTime) && DateTimeHelper.TryParseISODateTime(hGlobal.GetPointer(), hGlobal.Count, out Unsafe.As<T, DateTime>(ref value)))
                {
                    return true;
                }

                if (typeof(T) == typeof(DateTimeOffset) && DateTimeHelper.TryParseISODateTime(hGlobal.GetPointer(), hGlobal.Count, out Unsafe.As<T, DateTimeOffset>(ref value)))
                {
                    return true;
                }

                if (typeof(T) == typeof(Guid) && NumberHelper.TryParse(hGlobal.GetPointer(), hGlobal.Count, out Unsafe.As<T, Guid>(ref value)) == hGlobal.Count)
                {
                    return true;
                }

                if (typeof(T) == typeof(decimal))
                {
                    var num = NumberHelper.TryParse(hGlobal.GetPointer(), hGlobal.Count, out Unsafe.As<T, decimal>(ref value));

                    if (num != hGlobal.Count)
                    {
                        var numberInfo = NumberHelper.GetNumberInfo(hGlobal.GetPointer(), hGlobal.Count, 10);

                        if (numberInfo.IsNumber)
                        {
                            Unsafe.As<T, decimal>(ref value) = numberInfo.ToDecimal();

                            num = numberInfo.End;
                        }
                    }

                    if (num == hGlobal.Count)
                    {
                        return true;
                    }
                }

                if (typeof(T) == typeof(double))
                {
                    var num = NumberHelper.Decimal.TryParse(hGlobal.GetPointer(), hGlobal.Count, out Unsafe.As<T, double>(ref value));

                    if (num != hGlobal.Count)
                    {
                        var numberInfo = NumberHelper.GetNumberInfo(hGlobal.GetPointer(), hGlobal.Count, 10);

                        if (numberInfo.IsNumber)
                        {
                            Unsafe.As<T, double>(ref value) = numberInfo.ToDouble();

                            num = numberInfo.End;
                        }
                    }

                    if (num == hGlobal.Count)
                    {
                        return true;
                    }
                }

                if (typeof(T) == typeof(long))
                {
                    var num = NumberHelper.Decimal.TryParse(hGlobal.GetPointer(), hGlobal.Count, out Unsafe.As<T, long>(ref value));

                    if (num != hGlobal.Count)
                    {
                        var numberInfo = NumberHelper.GetNumberInfo(hGlobal.GetPointer(), hGlobal.Count, 10);

                        if (numberInfo.IsNumber)
                        {
                            Unsafe.As<T, long>(ref value) = numberInfo.ToInt64();

                            num = numberInfo.End;
                        }
                    }

                    if (num == hGlobal.Count)
                    {
                        return true;
                    }
                }

                if (typeof(T) == typeof(ulong))
                {
                    var num = NumberHelper.Decimal.TryParse(hGlobal.GetPointer(), hGlobal.Count, out Unsafe.As<T, ulong>(ref value));

                    if (num != hGlobal.Count)
                    {
                        var numberInfo = NumberHelper.GetNumberInfo(hGlobal.GetPointer(), hGlobal.Count, 10);

                        if (numberInfo.IsNumber)
                        {
                            Unsafe.As<T, ulong>(ref value) = numberInfo.ToUInt64();

                            num = numberInfo.End;
                        }
                    }

                    if (num == hGlobal.Count)
                    {
                        return true;
                    }
                }

                var str = new string(hGlobal.GetPointer(), 0, hGlobal.Count);

                if (typeof(T) == typeof(DateTime))
                {
                    Unsafe.As<T, DateTime>(ref value) = DateTime.Parse(str);

                    return true;
                }

                if (typeof(T) == typeof(DateTimeOffset))
                {
                    Unsafe.As<T, DateTimeOffset>(ref value) = DateTimeOffset.Parse(str);

                    return true;
                }

                if (typeof(T) == typeof(Guid))
                {
                    Unsafe.As<T, Guid>(ref value) = new Guid(str);

                    return true;
                }

                if (typeof(T) == typeof(decimal))
                {
                    Unsafe.As<T, decimal>(ref value) = decimal.Parse(str);

                    return true;
                }

                if (typeof(T) == typeof(double))
                {
                    Unsafe.As<T, double>(ref value) = double.Parse(str);

                    return true;
                }

                if (typeof(T) == typeof(long))
                {
                    Unsafe.As<T, long>(ref value) = long.Parse(str);

                    return true;
                }

                if (typeof(T) == typeof(ulong))
                {
                    Unsafe.As<T, ulong>(ref value) = ulong.Parse(str);

                    return true;
                }

                //value = XConvert<T>.Convert(str);

                //return true;
            }

            return false;
        }

        public DateTime ReadDateTime()
        {
            var size = TryReadExtensionHead();

            if (size >= 0)
            {
                var code = Read();

                switch (code)
                {
                    case MessagePackExtensionCode.Timestamp:
                        return ReadExtDateTime(size);
                }

                return Convert.ToDateTime(new MessagePackExtension(size, code, this));
            }

            if (TryReadParsedOfStr(out DateTime result))
            {
                return result;
            }

            return Convert.ToDateTime(DirectRead());
        }

        public decimal ReadDecimal()
        {
            var curr = Read();

            if (curr >= FixInt && curr <= FixIntMax)
            {
                return curr/* - FixInt */;
            }

            if (curr >= FixNegativeInt && curr <= FixNegativeIntMax)
            {
                return (sbyte)curr;
            }

            switch (curr)
            {
                case Int8:
                    return (sbyte)Read();
                case MessagePackCode.Int16:
                    return (short)Read2();
                case MessagePackCode.Int32:
                    return (int)Read4();
                case MessagePackCode.Int64:
                    return (long)Read8();
                case UInt8:
                    return Read();
                case MessagePackCode.UInt16:
                    return Read2();
                case MessagePackCode.UInt32:
                    return Read4();
                case MessagePackCode.UInt64:
                    return Read8();
                case Float32:
                    return checked((decimal)Unsafe.As<uint, float>(ref Unsafe.AsRef(Read4())));
                case Float64:
                    return checked((decimal)Unsafe.As<ulong, double>(ref Unsafe.AsRef(Read8())));
            }

            BackOff();

            if (TryReadParsedOfStr(out decimal value)) return value;

            return Convert.ToDecimal(DirectRead());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Exception GetException()
        {
            throw new NotImplementedException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int TryReadExtensionHead()
        {
            switch (Read())
            {
                case FixExt1:
                    return 1;
                case FixExt2:
                    return 2;
                case FixExt4:
                    return 4;
                case FixExt8:
                    return 8;
                case FixExt16:
                    return 16;
                case Ext8:
                    return Read();
                case Ext16:
                    return Read2();
                case Ext32:
                    return checked((int)Read4());
            }

            BackOff();

            return -1;
        }

        public MessagePackExtension TryReadExtension()
        {
            int size = TryReadExtensionHead();

            if (size >= 0)
            {
                return new MessagePackExtension(size, Read(), this);
            }

            return null;
        }

        public double ReadDouble()
        {
            var curr = Read();

            if (curr >= FixInt && curr <= FixIntMax)
            {
                return curr/* - FixInt */;
            }

            if (curr >= FixNegativeInt && curr <= FixNegativeIntMax)
            {
                return (sbyte)curr;
            }

            switch (curr)
            {
                case Int8:
                    return (sbyte)Read();
                case MessagePackCode.Int16:
                    return (short)Read2();
                case MessagePackCode.Int32:
                    return (int)Read4();
                case MessagePackCode.Int64:
                    return (long)Read8();
                case UInt8:
                    return Read();
                case MessagePackCode.UInt16:
                    return Read2();
                case MessagePackCode.UInt32:
                    return Read4();
                case MessagePackCode.UInt64:
                    return Read8();
                case Float32:
                    return Unsafe.As<uint, float>(ref Unsafe.AsRef(Read4()));
                case Float64:
                    return Unsafe.As<ulong, double>(ref Unsafe.AsRef(Read8()));
            }

            BackOff();

            if (TryReadParsedOfStr(out double value)) return value;

            return Convert.ToDouble(DirectRead());
        }

        public short ReadInt16() => checked((short)ReadInt64());

        public int ReadInt32() => checked((int)ReadInt64());

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ReadInt64()
        {
            var curr = Read();

            if (curr >= FixInt && curr <= FixIntMax)
            {
                return curr/* - FixInt*/;
            }

            if (curr >= FixNegativeInt && curr <= FixNegativeIntMax)
            {
                return (sbyte)curr;
            }

            switch (curr)
            {
                case Int8:
                    return (sbyte)Read();
                case MessagePackCode.Int16:
                    return (short)Read2();
                case MessagePackCode.Int32:
                    return (int)Read4();
                case MessagePackCode.Int64:
                    return (long)Read8();
                case UInt8:
                    return Read();
                case MessagePackCode.UInt16:
                    return Read2();
                case MessagePackCode.UInt32:
                    return Read4();
                case MessagePackCode.UInt64:
                    return checked((long)Read8());
                case Float32:
                    return checked((long)Unsafe.As<uint, float>(ref Unsafe.AsRef(Read4())));
                case Float64:
                    return checked((long)Unsafe.As<ulong, double>(ref Unsafe.AsRef(Read8())));
            }

            BackOff();

            if (TryReadParsedOfStr(out long value)) return value;

            return Convert.ToInt64(DirectRead());
        }

        public void ReadMap<TKey>(IDataWriter<TKey> mapWriter)
        {
            var length = TryReadMapHead();

            if (length >= 0)
            {
                mapWriter.Initialize(length);

                for (int i = 0; i < length; i++)
                {
                    mapWriter.OnWriteValue(ValueInterface<TKey>.ReadValue(this), this);
                }

                return;
            }

            if (TryReadNull()) return;

            if (IsArrayHead())
            {
                ReadArray(mapWriter.As<int>());

                return;
            }

            throw new InvalidOperationException("The msgpack node is not a map.");
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool IsMapHead()
        {
            var curr = Current();

            return (curr >= FixMap && curr <= FixMapMax) || curr == Map16 || curr == Map32;
        }

        public int TryReadMapHead()
        {
            var curr = Read();

            if (curr >= FixMap && curr <= FixMapMax)
            {
                return curr - FixMap;
            }

            switch (curr)
            {
                case Map16:
                    return Read2();
                case Map32:
                    return checked((int)Read4());
            }

            BackOff();

            return -1;
        }

        public T? ReadNullable<T>() where T : struct
        {
            if (TryReadNull())
            {
                return null;
            }

            return ValueInterface<T>.ReadValue(this);
        }

        public void ReadObject(IDataWriter<string> valueWriter)
        {
            var size = TryReadMapHead();

            if (size >= 0)
            {
                valueWriter.Initialize(size);

                if (valueWriter is IId64DataRW<byte> fastWriter)
                {
                    for (int i = 0; i < size; i++)
                    {
                        var bytesLength = TryReadStringHead();

                        if (bytesLength >= 0)
                        {
                            var id64 = fastWriter.GetId64(ref current[0], bytesLength);

                            current += bytesLength;

                            fastWriter.OnWriteValue(id64, this);
                        }
                        else
                        {
                            valueWriter.OnWriteValue(ReadString(), this);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < size; i++)
                    {
                        valueWriter.OnWriteValue(ReadString(), this);
                    }
                }


                return;
            }

            if (TryReadNull()) return;

            if (IsArrayHead())
            {
                ReadArray(valueWriter.As<int>());

                return;
            }

            throw new InvalidOperationException("The msgpack node is not an object.");
        }

        public sbyte ReadSByte() => checked((sbyte)ReadInt64());

        public float ReadSingle()
        {
            var value = ReadDouble();

            if (value >= float.MinValue && value <= float.MaxValue)
            {
                return (float)value;
            }

            throw new OverflowException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public byte Current()
        {
            if (current < end) return *current;

            throw GetException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public byte Read()
        {
            if (current < end) return *current++;

            throw GetException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ushort Read2()
        {
            if (current + 2 <= end) return (ushort)(((*current++) << 8) | (*current++));
            
            throw GetException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public uint Read4()
        {
            if (current + 4 <= end) return
                    (((uint)*current++) << 24) |
                    (((uint)*current++) << 16) |
                    (((uint)*current++) << 8) |
                    (*current++);

            throw GetException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong Read8()
        {
            if (current + 8 <= end) return
                    (((ulong)*current++) << 56) |
                    (((ulong)*current++) << 48) |
                    (((ulong)*current++) << 40) |
                    (((ulong)*current++) << 32) |
                    (((uint)*current++) << 24) |
                    (((uint)*current++) << 16) |
                    (((uint)*current++) << 8) |
                    (*current++);

            throw GetException();
        }


        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void BackOff()
        {
            --current;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int TryReadStringHead()
        {
            var curr = Read();

            if (curr >= FixStr && curr <= FixStrMax)
            {
                return curr - FixStr;
            }

            switch (curr)
            {
                case Str8:
                    return Read();
                case Str16:
                    return Read2();
                case Str32:
                    return checked((int)Read4());
            }

            BackOff();

            return -1;
        }

        public string ReadStringMode1(int bytesLength)
        {
            var charsLength = GetUtf8CharsLength(current, bytesLength);

            var str = StringHelper.MakeString(charsLength);

            fixed (char* chars = str)
            {
                GetUtf8Chars(current, bytesLength, chars);
            }

            current += bytesLength;

            return str;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void ReadStringToHGlobal(int bytesLength)
        {
            if (hGlobal.Capacity <= GetUtf8MaxCharsLength(bytesLength))
            {
                hGlobal.Expand(GetUtf8MaxCharsLength(bytesLength));
            }

            hGlobal.Count = GetUtf8Chars(current, bytesLength, hGlobal.GetPointer());

            current += bytesLength;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string ReadStringMode2(int bytesLength)
        {
            ReadStringToHGlobal(bytesLength);

            return hGlobal.ToStringEx();
        }

        public string ReadString()
        {
            var bytesLength = TryReadStringHead();

            if (bytesLength >= 0) return ReadStringMode2(bytesLength);

            if (TryReadNull()) return null;

            return Convert.ToString(DirectRead());
        }

        public ushort ReadUInt16() => checked((ushort)ReadUInt64());

        public uint ReadUInt32() => checked((uint)ReadUInt64());

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            var curr = Read();

            if (curr >= FixInt && curr <= FixIntMax)
            {
                return curr/* - FixInt*/;
            }

            if (curr >= FixNegativeInt && curr <= FixNegativeIntMax)
            {
                return checked((ulong)unchecked((sbyte)curr));
            }

            switch (curr)
            {
                case Int8:
                    return checked((ulong)unchecked((sbyte)Read()));
                case MessagePackCode.Int16:
                    return checked((ulong)unchecked((short)Read2()));
                case MessagePackCode.Int32:
                    return checked((ulong)unchecked((int)Read4()));
                case MessagePackCode.Int64:
                    return checked((ulong)unchecked((long)Read8()));
                case UInt8:
                    return Read();
                case MessagePackCode.UInt16:
                    return Read2();
                case MessagePackCode.UInt32:
                    return Read4();
                case MessagePackCode.UInt64:
                    return Read8();
                case Float32:
                    return checked((ulong)Unsafe.As<uint, float>(ref Unsafe.AsRef(Read4())));
                case Float64:
                    return checked((ulong)Unsafe.As<ulong, double>(ref Unsafe.AsRef(Read8())));
            }

            BackOff();

            if (TryReadParsedOfStr(out ulong value)) return value;

            return Convert.ToUInt64(DirectRead());
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryReadNull()
        {
            if (Current() == Nil)
            {
                ++current;

                return true;
            }

            return false;
        }

        public byte[] ReadBinary()
        {
            var length = TryReadBinaryHead();

            if (length >= 0)
            {
                var result = new byte[length];

                ReadBinary(ref result[0], length);

                return result;
            }

            if (TryReadNull()) return null;

            if (IsArrayHead())
            {
                return ValueInterface<byte[]>.ReadValue(this);
            }

            return (byte[])Convert.ChangeType(DirectRead(), typeof(byte[]));
        }

        byte[] IValueReader<byte[]>.ReadValue() => ReadBinary();

        public void ReadBinary(ref byte firstByte, int length)
        {
            Unsafe.CopyBlock(ref firstByte, ref current[0], (uint)length);

            current += length;
        }

        public int TryReadBinaryHead()
        {
            var curr = Read();

            switch (curr)
            {
                case Bin8:
                    return Read();
                case Bin16:
                    return Read2();
                case Bin32:
                    return checked((int)Read4());
            }

            BackOff();

            return -1;
        }

        public Guid ReadGuid()
        {
            if (TryReadParsedOfStr(out Guid result))
            {
                return result;
            }

            return (Guid)Convert.ChangeType(DirectRead(), typeof(Guid));
        }

        Guid IValueReader<Guid>.ReadValue() => ReadGuid();

        public DateTimeOffset ReadDateTimeOffset()
        {
            var size = TryReadExtensionHead();

            if (size >= 0)
            {
                var code = Read();

                switch (code)
                {
                    case MessagePackExtensionCode.Timestamp:
                        return ReadExtDateTime(size);
                }

                return XConvert<DateTimeOffset>.Convert(new MessagePackExtension(size, code, this));
            }

            if (TryReadParsedOfStr(out DateTimeOffset tResult))
            {
                return tResult;
            }

            return (DateTimeOffset)Convert.ChangeType(DirectRead(), typeof(DateTimeOffset));
        }

        DateTimeOffset IValueReader<DateTimeOffset>.ReadValue() => ReadDateTimeOffset();

        public void MakeTargetedId() { }
    }

    public sealed class MessagePackMap : MessagePackObject
    {

    }

    public sealed class MessagePackArray : MessagePackObject
    {

    }

    public abstract class MessagePackObject
    {

    }

    public sealed class MessagePackBinary : MessagePackObject
    {
        private static void ReadBasic(ref int length, ref Basic basic, MessagePackDeserializer deserializer)
        {
            if (length >= 8)
            {
                length -= 8;

                basic.UInt64_0 = deserializer.Read8();

                return;
            }

            if (length >= 4)
            {
                length -= 4;

                basic.UInt32_0 = deserializer.Read4();

                if (length >= 2)
                {
                    length -= 2;

                    basic.UInt16_2 = deserializer.Read2();

                    if (length >= 1)
                    {
                        length -= 1;

                        basic.Byte_6 = deserializer.Read();
                    }
                }

                return;
            }

            if (length >= 2)
            {
                length -= 2;

                basic.UInt16_0 = deserializer.Read2();

                if (length >= 1)
                {
                    length -= 1;

                    basic.Byte_2 = deserializer.Read();
                }

                return;
            }

            if (length >= 1)
            {
                length -= 1;

                basic.Byte_0 = deserializer.Read();
            }
        }

        private readonly Basic Basic1;
        private readonly Basic Basic2;
        private readonly Basic Basic3;
        private readonly Basic Basic4;

        public readonly int Length;

        private readonly byte[] Extended;

        internal MessagePackBinary(int length, MessagePackDeserializer deserializer)
        {
            Length = length;

            ReadBasic(ref length, ref Basic1, deserializer);
            ReadBasic(ref length, ref Basic2, deserializer);
            ReadBasic(ref length, ref Basic3, deserializer);
            ReadBasic(ref length, ref Basic4, deserializer);

            if (length >= 1)
            {
                Extended = new byte[length];

                var index = 0;

                for (; length >= 8; length -= 8, index += 8)
                {
                    Unsafe.As<byte, ulong>(ref Extended[index]) = deserializer.Read8();
                }

                for (; length > 0; --length, ++index)
                {
                    Extended[index] = deserializer.Read();
                }
            }
        }


        [StructLayout(LayoutKind.Explicit)]
        struct Basic
        {
            [FieldOffset(0)]
            public byte Byte_0;
            [FieldOffset(1)]
            public byte Byte_1;
            [FieldOffset(2)]
            public byte Byte_2;
            [FieldOffset(3)]
            public byte Byte_3;
            [FieldOffset(4)]
            public byte Byte_4;
            [FieldOffset(5)]
            public byte Byte_5;
            [FieldOffset(6)]
            public byte Byte_6;
            [FieldOffset(7)]
            public byte Byte_7;


            [FieldOffset(0)]
            public ushort UInt16_0;
            [FieldOffset(2)]
            public ushort UInt16_1;
            [FieldOffset(4)]
            public ushort UInt16_2;
            [FieldOffset(6)]
            public ushort UInt16_3;


            [FieldOffset(0)]
            public uint UInt32_0;
            [FieldOffset(4)]
            public uint UInt32_1;


            [FieldOffset(0)]
            public ulong UInt64_0;
        }
    }

    public sealed class MessagePackExtension : MessagePackObject
    {
        public readonly MessagePackBinary Binary;
        public readonly byte Code;

        internal MessagePackExtension(int length, byte code, MessagePackDeserializer deserializer)
        {
            Code = code;

            Binary = new MessagePackBinary(length, deserializer);
        }
    }
}