using Swifter.Formatters;

using Swifter.RW;
using Swifter.Tools;

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using static Swifter.MessagePack.MessagePackFormatterOptions;
using static Swifter.MessagePack.MessagePackCode;
using static Swifter.MessagePack.MessagePackFormatter;
using static Swifter.MessagePack.MessagePackDeserializeModes;
using ArrayType = System.Collections.Generic.List<object>;
using MapType = System.Collections.Generic.Dictionary<object, object>;

namespace Swifter.MessagePack
{
    sealed unsafe class MessagePackDeserializer<TMode> :
        IMessagePackReader,
        IFormatterReader,
        IDataReader<Ps<Utf8Byte>>,
        IDataReader<string>,
        IValueReader<Guid>,
        IValueReader<DateTimeOffset>,
        IValueReader<byte[]>,
        IValueReader<MessagePackExtension>,
        IValueReader<bool[]>, IValueReader<List<bool>>,
        /* IValueReader<byte[]>,*/ IValueReader<List<byte>>,
        IValueReader<sbyte[]>, IValueReader<List<sbyte>>,
        IValueReader<short[]>, IValueReader<List<short>>,
        IValueReader<ushort[]>, IValueReader<List<ushort>>,
        IValueReader<char[]>, IValueReader<List<char>>,
        IValueReader<int[]>, IValueReader<List<int>>,
        IValueReader<uint[]>, IValueReader<List<uint>>,
        IValueReader<long[]>, IValueReader<List<long>>,
        IValueReader<ulong[]>, IValueReader<List<ulong>>,
        IValueReader<float[]>, IValueReader<List<float>>,
        IValueReader<double[]>, IValueReader<List<double>>,
        IValueReader<decimal[]>, IValueReader<List<decimal>>,
        IValueReader<DateTime[]>, IValueReader<List<DateTime>>,
        IValueReader<DateTimeOffset[]>, IValueReader<List<DateTimeOffset>>,
        IValueReader<string[]>, IValueReader<List<string>>,
        IValueReader<object[]>, IValueReader<List<object>>
        where TMode : struct
    {
        public readonly MessagePackFormatter MessagePackFormatter;
        public readonly MessagePackFormatterOptions options;

        public TMode mode;

        public byte* current;

        public readonly byte* begin;
        public readonly byte* end;
        public readonly int maxDepth;

        public int depth;
        public int fieldCount;

        public int Offset => (int)(current - begin);

        public long TargetedId => MessagePackFormatter?.targeted_id ?? GlobalTargetedId;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public MessagePackDeserializer(byte* bytes, int length, int maxDepth, MessagePackFormatterOptions options)
        {
            begin = bytes;
            end = bytes + length;

            current = bytes;

            this.options = options;
            this.maxDepth = maxDepth;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public MessagePackDeserializer(byte* bytes, int length, int maxDepth)
        {
            begin = bytes;
            end = bytes + length;

            current = bytes;
            this.maxDepth = maxDepth;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public MessagePackDeserializer(MessagePackFormatter messagePackFormatter, byte* bytes, int length, int maxDepth, MessagePackFormatterOptions options) : this(bytes, length, maxDepth, options)
        {
            MessagePackFormatter = messagePackFormatter;
        }

        public Dictionary<int, object> References => Underlying.As<TMode, ReferenceMode>(ref mode).References;

        internal void InitReferences()
        {
            Underlying.As<TMode, ReferenceMode>(ref mode).References = new Dictionary<int, object>();
        }

        public MessagePackToken GetToken()
        {
            if (current < end)
            {
                switch (*current)
                {
                    case Nil:
                        return MessagePackToken.Nil;
                    case False:
                    case True:
                        return MessagePackToken.Bool;
                    case Bin8:
                    case Bin16:
                    case Bin32:
                        return MessagePackToken.Bin;
                    case Ext8:
                    case Ext16:
                    case Ext32:
                    case FixExt1:
                    case FixExt2:
                    case FixExt4:
                    case FixExt8:
                    case FixExt16:
                        return MessagePackToken.Ext;
                    case Float32:
                        return MessagePackToken.Float32;
                    case Float64:
                        return MessagePackToken.Float64;
                    case UInt8:
                    case var i when i >= FixInt && i <= FixIntMax:
                        return MessagePackToken.UInt8;
                    case MessagePackCode.UInt16:
                        return MessagePackToken.UInt16;
                    case MessagePackCode.UInt32:
                        return MessagePackToken.UInt32;
                    case MessagePackCode.UInt64:
                        return MessagePackToken.UInt64;
                    case Int8:
                    case var ni when ni >= FixNegativeInt && ni <= FixNegativeIntMax:
                        return MessagePackToken.Int8;
                    case MessagePackCode.Int16:
                        return MessagePackToken.Int16;
                    case MessagePackCode.Int32:
                        return MessagePackToken.Int32;
                    case MessagePackCode.Int64:
                        return MessagePackToken.Int64;
                    case Str8:
                    case Str16:
                    case Str32:
                    case var str when str >= FixStr && str <= FixStrMax:
                        return MessagePackToken.Str;
                    case Array16:
                    case Array32:
                    case var array when array >= FixArray && array <= FixArrayMax:
                        return MessagePackToken.Array;
                    case Map16:
                    case Map32:
                    case var map when map >= FixMap && map <= FixMapMax:
                        return MessagePackToken.Map;
                    default:
                        return MessagePackToken.NeverUsed;
                }
            }

            return MessagePackToken.End;
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        public object DirectRead()
        {
            switch (Current())
            {
                case Nil:
                    ++current;
                    return null;
                case False:
                case True:
                    return ReadBoolean();
                case Bin8:
                case Bin16:
                case Bin32:
                    return ReadBinary();
                case FixExt1:
                case FixExt2:
                case FixExt4:
                case FixExt8:
                case FixExt16:
                case Ext8:
                case Ext16:
                case Ext32:
                    {
                        var size = TryReadExtensionHead();
                        var code = (sbyte)Read();

                        if (code == MessagePackExtensionCode.Timestamp)
                        {
                            return InternalReadExtDateTime(size);
                        }

                        if (IsReferenceMode && code == MessagePackExtensionCode.Reference)
                        {
                            return InternalReadExtReference(size);
                        }

                        return InternalReadExtension(size, code);
                    }
                case Float32:
                    return ReadSingle();
                case Float64:
                    return ReadDouble();
                case UInt8:
                    return ReadByte();
                case MessagePackCode.UInt16:
                    return ReadUInt16();
                case MessagePackCode.UInt32:
                    return ReadUInt32();
                case MessagePackCode.UInt64:
                    return ReadUInt64();
                case Int8:
                    return ReadSByte();
                case MessagePackCode.Int16:
                    return ReadInt16();
                case MessagePackCode.Int32:
                case var i when i >= FixInt && i <= FixIntMax:
                case var ni when ni >= FixNegativeInt && ni <= FixNegativeIntMax:
                    return ReadInt32();
                case MessagePackCode.Int64:
                    return ReadInt64();
                case Str8:
                case Str16:
                case Str32:
                case var str when str >= FixStr && str <= FixStrMax:
                    return ReadString();
                case Array16:
                case Array32:
                case var array when array >= FixArray && array <= FixArrayMax:
                    return ValueInterface<ArrayType>.ReadValue(this);
                case Map16:
                case Map32:
                case var map when map >= FixMap && map <= FixMapMax:
                    return ValueInterface<MapType>.ReadValue(this);
                default:
                    throw new InvalidOperationException("NeverUsed bit.");
            }
        }

        public object InternalReadExtReference(int size)
        {
            var offset = checked((int)(size switch
            {
                sizeof(byte) => Read(),
                sizeof(ushort) => Read2(),
                sizeof(uint) => Read4(),
                sizeof(ulong) => Read8(),
                _ => throw new FormatException("Unrecognized reference format."),
            }));

            if (offset > Offset)
            {
                throw new FormatException("Reference address cannot be greater than the current offset!");
            }

            if (References.TryGetValue(offset, out var value))
            {
                if (value is IDataWriter dataWriter)
                {
                    return dataWriter.Content;
                }

                return value;
            }

            var curr = current;

            current = begin + offset;

            var result = DirectRead();

            current = curr;

            return result;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool IsArrayHead()
        {
            switch (Current())
            {
                case var a when a >= FixArray && a <= FixArrayMax:
                case Array16:
                case Array32:
                    return true;
                default:
                    return false;
            }
        }

        public int TryReadArrayHead()
        {
            switch (Read())
            {
                case var a when a >= FixArray && a <= FixArrayMax:
                    return a - FixArray;
                case Array16:
                    return Read2();
                case Array32:
                    return checked((int)Read4());
                default:
                    BackOff();

                    return -1;
            }
        }

        public void ReadArray(IDataWriter<int> valueWriter)
        {
            if (TryReadNull())
            {
                return;
            }

            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();

                SkipValue();

                return;
            }

            var offset = Offset;

            var length = TryReadArrayHead();

            if (length >= 0)
            {
                valueWriter.Initialize(length);

                if (IsReferenceMode)
                {
                    References.Add(offset, valueWriter);
                }

                ++depth;

                for (int i = 0; i < length; i++)
                {
                    valueWriter.OnWriteValue(i, this);
                }

                --depth;
            }
            else if (TryReadEmptyString())
            {

            }
            else if (IsMapHead())
            {
                ReadMap(valueWriter);
            }
            else
            {
                valueWriter.Content = XConvert.Cast(DirectRead(), valueWriter.ContentType);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool On(MessagePackFormatterOptions options)
            => (this.options & options) != 0;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ThrowOutOfDepthException()
        {
            if (On(OutOfDepthException))
            {
                throw new MessagePackOutOfDepthException();
            }
        }

        public bool IsReferenceMode
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => typeof(TMode) == typeof(ReferenceMode);
        }

        IEnumerable<Ps<Utf8Byte>> IDataReader<Ps<Utf8Byte>>.Keys => null;

        public int Count => -1;

        public Type ContentType => null;

        public object Content
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        IEnumerable<string> IDataReader<string>.Keys => null;

        public IValueReader this[string key]
        {
            get
            {
                if (fieldCount > 0)
                {
                    var currentBackup = current;

                    var length = TryReadStringHead();

                    if (length >= 0 && StringHelper.EqualsWithIgnoreCase(InternalReadString(length), key))
                    {
                        --fieldCount;

                        return this;
                    }

                    current = currentBackup;
                }

                return RWHelper.DefaultValueReader;
            }
        }

        public IValueReader this[Ps<Utf8Byte> key]
        {
            get
            {
                if (fieldCount > 0)
                {
                    var currentBackup = current;

                    var length = TryReadStringHead();

                    if (length >= 0 && StringHelper.EqualsWithIgnoreCase(InternalReadString(length), key))
                    {
                        --fieldCount;

                        return this;
                    }

                    current = currentBackup;
                }

                return RWHelper.DefaultValueReader;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryReadEmptyString()
        {
            if (On(EmptyStringAsNull | EmptyStringAsDefault) && Current() == FixStr)
            {
                ++current;

                return true;
            }

            return false;
        }

        public void InternalReadArray(IDataWriter<int> valueWriter)
        {
            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();

                SkipValue();

                return;
            }

            var length = TryReadArrayHead();

            if (IsReferenceMode)
            {
                References.Add(Offset, valueWriter);
            }

            ++depth;

            for (int i = 0; i < length; i++)
            {
                valueWriter.OnWriteValue(i, this);
            }

            --depth;
        }

        public T[] ReadArray<T>()
        {
            if (TryReadNull())
            {
                return null;
            }

            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();

                SkipValue();

                return null;
            }

            var offset = Offset;

            var length = TryReadArrayHead();

            if (length >= 0)
            {
                var ret = new T[length];

                if (IsReferenceMode)
                {
                    References.Add(offset, ret);
                }

                ++depth;

                if (ValueInterface<T>.IsNotModified)
                {
                    for (int i = 0; i < length; i++)
                    {
                        ret[i] = ReadValue<T>();
                    }
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        ret[i] = ValueInterface<T>.ReadValue(this);
                    }
                }

                --depth;

                return ret;
            }
            else if (TryReadEmptyString())
            {
                return null;
            }
            else
            {
                return XConvert<T[]>.FromObject(DirectRead());
            }
        }

        public List<T> ReadList<T>()
        {
            if (TryReadNull())
            {
                return null;
            }

            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();

                SkipValue();

                return null;
            }

            var offset = Offset;

            var length = TryReadArrayHead();

            if (length >= 0)
            {
                var ret = new List<T>(length);

                if (IsReferenceMode)
                {
                    References.Add(offset, ret);
                }

                ++depth;

                if (ValueInterface<T>.IsNotModified)
                {
                    for (int i = 0; i < length; i++)
                    {
                        ret.Add(ReadValue<T>());
                    }
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        ret.Add(ValueInterface<T>.ReadValue(this));
                    }
                }

                --depth;

                return ret;
            }
            else if (TryReadEmptyString())
            {
                return null;
            }
            else
            {
                return XConvert<List<T>>.FromObject(DirectRead());
            }
        }

        public bool ReadBoolean()
        {
            switch (Read())
            {
                case True:
                    return true;
                case False:
                case Nil:
                case FixInt:
                    return false;
                case var i when i > FixInt && i <= FixIntMax:
                    return true;
                case var ni when ni >= FixNegativeInt && ni <= FixNegativeIntMax:
                    return true;
            }

            BackOff();

            if (TryReadParsedOfStr(out bool value)) return value;

            return Convert.ToBoolean(DirectRead());
        }

        public byte ReadByte() => checked((byte)ReadUInt64());

        public char ReadChar() => char.Parse(ReadString());

        public DateTime InternalReadExtDateTime(int size)
        {
            uint nanoseconds = 0;
            long seconds;

            switch (size)
            {
                case sizeof(uint):
                    seconds = Read4();
                    break;
                case sizeof(ulong):
                    var merged = Read8();
                    nanoseconds = (uint)(merged >> 34);
                    seconds = (long)(merged & UInt34MaxValue);
                    break;
                case sizeof(uint) + sizeof(ulong):
                    nanoseconds = Read4();
                    seconds = (long)Read8();
                    break;
                default:
                    throw new FormatException("Unrecognized timestamp format.");
            }

            var ticks = seconds * TimeSpan.TicksPerSecond +
                nanoseconds / DateTimeHelper.NanosecondsPerTick;

            return new DateTime(checked(
                DateTimeHelper.TicksPerUnixEpoch +
                DateTimeHelper.TicksPerUTCDifference +
                ticks));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool TryReadParsedOfStr<T>(out T value)
        {
            value = default;

            var length = TryReadStringHead();

            if (length == 0 && (options & EmptyStringAsDefault) != 0)
            {
                return true;
            }

            if (length >= 0)
            {
                var str = InternalReadString(length).ToStringEx();

                if (typeof(T) == typeof(DateTime))
                {
                    if (DateTimeHelper.TryParseISODateTime(str, out Underlying.As<T, DateTime>(ref value)))
                    {
                        return true;
                    }
                }

                if (typeof(T) == typeof(DateTimeOffset))
                {
                    if (DateTimeHelper.TryParseISODateTime(str, out Underlying.As<T, DateTimeOffset>(ref value)))
                    {
                        return true;
                    }
                }

                if (typeof(T) == typeof(bool))
                {
                    if (bool.TryParse(str, out Underlying.As<T, bool>(ref value)))
                    {
                        return true;
                    }
                }

                if (typeof(T) == typeof(long) ||
                    typeof(T) == typeof(ulong) ||
                    typeof(T) == typeof(decimal) ||
                    typeof(T) == typeof(double) ||
                    typeof(T) == typeof(Guid))
                {
                    (ParseCode code, int length, T value) parsed = default;

                    static ref (ParseCode code, int length, TTo value) As<TTo>(ref (ParseCode code, int length, T value) value) =>
                        ref Underlying.As<(ParseCode code, int length, T parsed), (ParseCode code, int length, TTo parsed)>(ref value);

                    fixed (char* chars = str)
                    {
                        if (typeof(T) == typeof(long))
                        {
                            As<long>(ref parsed) = NumberHelper.DecimalParseInt64(chars, str.Length);
                        }
                        else if (typeof(T) == typeof(ulong))
                        {
                            As<ulong>(ref parsed) = NumberHelper.DecimalParseUInt64(chars, str.Length);
                        }
                        else if (typeof(T) == typeof(decimal))
                        {
                            As<decimal>(ref parsed) = NumberHelper.ParseDecimal(chars, str.Length);
                        }
                        else if (typeof(T) == typeof(double))
                        {
                            As<double>(ref parsed) = NumberHelper.DecimalParseDouble(chars, str.Length);
                        }
                        else
                        {
                            As<Guid>(ref parsed) = NumberHelper.ParseGuid(chars, str.Length);
                        }

                        if (parsed.length == str.Length)
                        {
                            value = parsed.value;

                            return true;
                        }

                        if (typeof(T) == typeof(long) ||
                            typeof(T) == typeof(ulong) ||
                            typeof(T) == typeof(decimal) ||
                            typeof(T) == typeof(double))
                        {
                            var numberInfo = NumberHelper.GetNumberInfo(chars, str.Length);

                            if (numberInfo.IsNumber && numberInfo.End == str.Length && numberInfo.IsCommonRadix(out var radix))
                            {
                                if (typeof(T) == typeof(decimal))
                                {
                                    Underlying.As<T, decimal>(ref value) = numberInfo.ToDecimal();
                                }
                                else if (typeof(T) == typeof(double))
                                {
                                    Underlying.As<T, double>(ref value) = numberInfo.ToDouble(radix);
                                }
                                else if (typeof(T) == typeof(long))
                                {
                                    Underlying.As<T, long>(ref value) = numberInfo.ToInt64(radix);
                                }
                                else
                                {
                                    Underlying.As<T, ulong>(ref value) = numberInfo.ToUInt64(radix);
                                }

                                return true;
                            }
                        }
                    }
                }

                if (typeof(T) == typeof(DateTime))
                {
                    Underlying.As<T, DateTime>(ref value) = DateTime.Parse(str);

                    return true;
                }

                if (typeof(T) == typeof(DateTimeOffset))
                {
                    Underlying.As<T, DateTimeOffset>(ref value) = DateTimeOffset.Parse(str);

                    return true;
                }

                if (typeof(T) == typeof(Guid))
                {
                    Underlying.As<T, Guid>(ref value) = new Guid(str);

                    return true;
                }

                if (typeof(T) == typeof(decimal))
                {
                    Underlying.As<T, decimal>(ref value) = decimal.Parse(str);

                    return true;
                }

                if (typeof(T) == typeof(double))
                {
                    Underlying.As<T, double>(ref value) = double.Parse(str);

                    return true;
                }

                if (typeof(T) == typeof(long))
                {
                    Underlying.As<T, long>(ref value) = long.Parse(str);

                    return true;
                }

                if (typeof(T) == typeof(ulong))
                {
                    Underlying.As<T, ulong>(ref value) = ulong.Parse(str);

                    return true;
                }

                value = XConvert.Convert<string, T>(str);

                return true;
            }

            return false;
        }

        public DateTime ReadDateTime()
        {
            var size = TryReadExtensionHead();

            if (size >= 0)
            {
                var code = (sbyte)Read();

                if (code == MessagePackExtensionCode.Timestamp)
                {
                    return InternalReadExtDateTime(size);
                }

                if (IsReferenceMode && code == MessagePackExtensionCode.Reference)
                {
                    return Convert.ToDateTime(InternalReadExtReference(size));
                }

                return Convert.ToDateTime(InternalReadExtension(size, code));
            }

            if (TryReadParsedOfStr(out DateTime result))
            {
                return result;
            }

            return Convert.ToDateTime(DirectRead());
        }

        public MessagePackExtension ReadExtension()
        {
            var size = TryReadExtensionHead();

            var code = (sbyte)Read();

            if (size >= 0)
            {
                return InternalReadExtension(size, code);
            }

            return XConvert.FromObject<MessagePackExtension>(DirectRead());
        }

        public decimal ReadDecimal()
        {
            switch (Read())
            {
                case var i when i >= FixInt && i <= FixIntMax:
                    return i;
                case var ni when ni >= FixNegativeInt && ni <= FixNegativeIntMax:
                    return (sbyte)ni;
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
                    return checked((decimal)Underlying.As<uint, float>(ref Underlying.AsRef(Read4())));
                case Float64:
                    return checked((decimal)Underlying.As<ulong, double>(ref Underlying.AsRef(Read8())));
                default:
                    BackOff();

                    if (TryReadParsedOfStr(out decimal value)) return value;

                    return Convert.ToDecimal(DirectRead());
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ThrowException()
        {
            throw new MessagePackDeserializeException(Offset);
        }

        public MessagePackExtension InternalReadExtension(int size, sbyte code)
        {
            return new MessagePackExtension(code, InternalReadBinary(size));
        }

        public double ReadDouble()
        {
            switch (Read())
            {
                case var i when i >= FixInt && i <= FixIntMax:
                    return i;
                case var ni when ni >= FixNegativeInt && ni <= FixNegativeIntMax:
                    return (sbyte)ni;
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
                    return Underlying.As<uint, float>(ref Underlying.AsRef(Read4()));
                case Float64:
                    return Underlying.As<ulong, double>(ref Underlying.AsRef(Read8()));
                default:
                    BackOff();

                    if (TryReadParsedOfStr(out double value)) return value;

                    return Convert.ToDouble(DirectRead());
            }
        }

        public short ReadInt16() => checked((short)ReadInt64());

        public int ReadInt32() => checked((int)ReadInt64());

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ReadInt64()
        {
            switch (Read())
            {
                case var i when i >= FixInt && i <= FixIntMax:
                    return i;
                case var ni when ni >= FixNegativeInt && ni <= FixNegativeIntMax:
                    return (sbyte)ni;
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
                    return checked((long)Underlying.As<uint, float>(ref Underlying.AsRef(Read4())));
                case Float64:
                    return checked((long)Underlying.As<ulong, double>(ref Underlying.AsRef(Read8())));
                default:

                    BackOff();

                    if (TryReadParsedOfStr(out long value)) return value;

                    return Convert.ToInt64(DirectRead());
            }
        }

        public void ReadMap<TKey>(IDataWriter<TKey> mapWriter)
        {
            if (TryReadNull())
            {
                return;
            }

            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();

                SkipValue();

                return;
            }

            var offset = Offset;

            var size = TryReadMapHead();

            if (size >= 0)
            {
                mapWriter.Initialize(size);

                if (IsReferenceMode)
                {
                    References.Add(offset, mapWriter);
                }

                ++depth;

                if (ValueInterface<TKey>.IsNotModified)
                {
                    for (int i = 0; i < size; i++)
                    {
                        mapWriter.OnWriteValue(ReadValue<TKey>(), this);
                    }
                }
                else
                {
                    for (int i = 0; i < size; i++)
                    {
                        mapWriter.OnWriteValue(ValueInterface<TKey>.ReadValue(this), this);
                    }
                }

                --depth;
            }
            else if (TryReadEmptyString())
            {

            }
            else if (IsArrayHead())
            {
                ReadArray(mapWriter.As<int>());
            }
            else
            {
                mapWriter.Content = XConvert.Cast(DirectRead(), mapWriter.ContentType);
            }
        }

        public void InternalReadMap<TKey>(IDataWriter<TKey> mapWriter)
        {
            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();

                SkipValue();

                return;
            }

            var size = TryReadMapHead();

            if (IsReferenceMode)
            {
                References.Add(Offset, mapWriter);
            }

            ++depth;

            if (ValueInterface<TKey>.IsNotModified)
            {
                for (int i = 0; i < size; i++)
                {
                    mapWriter.OnWriteValue(ReadValue<TKey>(), this);
                }
            }
            else
            {
                for (int i = 0; i < size; i++)
                {
                    mapWriter.OnWriteValue(ValueInterface<TKey>.ReadValue(this), this);
                }
            }

            --depth;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool IsMapHead()
        {
            switch (Current())
            {
                case var m when m >= FixMap && m <= FixMapMax:
                case Map16:
                case Map32:
                    return true;
                default:
                    return false;
            }
        }

        public int TryReadMapHead()
        {
            switch (Read())
            {
                case var m when m >= FixMap && m <= FixMapMax:
                    return m - FixMap;
                case Map16:
                    return Read2();
                case Map32:
                    return checked((int)Read4());
                default:
                    BackOff();

                    return -1;
            }
        }

        public T? ReadNullable<T>() where T : struct
        {
            if (TryReadNull())
            {
                return null;
            }

            if (TryReadEmptyString())
            {
                return null;
            }

            return ReadValue<T>();
        }

        public void ReadObject(IDataWriter<string> valueWriter)
        {
            if (TryReadNull())
            {
                return;
            }

            if (valueWriter is IAsDataWriter @as)
            {
                @as.InvokeTIn(new WriteMapInvoker(this, @as.Original));

                return;
            }

            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();

                SkipValue();

                return;
            }

            var offset = Offset;

            var size = TryReadMapHead();

            if (size >= 0)
            {
                valueWriter.Initialize(size);

                if (IsReferenceMode)
                {
                    References.Add(offset, valueWriter);
                }

                ++depth;

                var count_non_zero = valueWriter.Count > 0;

                if (valueWriter is IDataWriter<Ps<Utf8Byte>> fastWriter)
                {
                    if (On(AsOrderedObjectDeserialize) && count_non_zero)
                    {
                        var fieldCountBackup = fieldCount;

                        fieldCount = size;

                        fastWriter.OnWriteAll(this);

                        size = fieldCount;

                        fieldCount = fieldCountBackup;
                    }

                    for (int i = 0; i < size; i++)
                    {
                        var bytesLength = TryReadStringHead();

                        if (bytesLength >= 0)
                        {
                            var name = InternalReadString(bytesLength);

                            if (count_non_zero)
                            {
                                switch (Current())
                                {
                                    case Nil:
                                        if (On(IgnoreNull))
                                        {
                                            ++current;

                                            continue;
                                        }

                                        break;
                                    case FixInt:
                                        if (On(IgnoreZero))
                                        {
                                            ++current;

                                            continue;
                                        }

                                        break;
                                }
                            }

                            fastWriter.OnWriteValue(name, this);
                        }
                        else
                        {
                            var name = ReadString();

                            if (count_non_zero)
                            {
                                switch (Current())
                                {
                                    case Nil:
                                        if (On(IgnoreNull))
                                        {
                                            ++current;

                                            continue;
                                        }

                                        break;
                                    case FixInt:
                                        if (On(IgnoreZero))
                                        {
                                            ++current;

                                            continue;
                                        }

                                        break;
                                }
                            }

                            valueWriter.OnWriteValue(name, this);
                        }
                    }
                }
                else
                {
                    if (On(AsOrderedObjectDeserialize) && count_non_zero)
                    {
                        var fieldCountBackup = fieldCount;

                        fieldCount = size;

                        valueWriter.OnWriteAll(this);

                        size = fieldCount;

                        fieldCount = fieldCountBackup;
                    }

                    for (int i = 0; i < size; i++)
                    {
                        var name = ReadString();

                        if (count_non_zero)
                        {
                            switch (Current())
                            {
                                case Nil:
                                    if (On(IgnoreNull))
                                    {
                                        ++current;

                                        continue;
                                    }

                                    break;
                                case FixInt:
                                    if (On(IgnoreZero))
                                    {
                                        ++current;

                                        continue;
                                    }

                                    break;
                            }
                        }

                        valueWriter.OnWriteValue(name, this);
                    }
                }

                --depth;
            }
            else if (IsArrayHead())
            {
                ReadArray(valueWriter.As<int>());
            }
            else
            {
                valueWriter.Content = XConvert.Cast(DirectRead(), valueWriter.ContentType);
            }
        }

        public void InternalReadObject(IDataWriter<string> valueWriter)
        {
            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();

                SkipValue();

                return;
            }

            var size = TryReadMapHead();

            if (IsReferenceMode)
            {
                References.Add(Offset, valueWriter);
            }

            ++depth;

            if (valueWriter is IDataWriter<Ps<Utf8Byte>> fastWriter)
            {
                if (On(AsOrderedObjectDeserialize) && fastWriter.Count > 0)
                {
                    var fieldCountBackup = fieldCount;

                    fieldCount = size;

                    fastWriter.OnWriteAll(this);

                    size = fieldCount;

                    fieldCount = fieldCountBackup;
                }

                for (int i = 0; i < size; i++)
                {
                    var bytesLength = TryReadStringHead();

                    if (bytesLength >= 0)
                    {
                        fastWriter.OnWriteValue(InternalReadString(bytesLength), this);
                    }
                    else
                    {
                        valueWriter.OnWriteValue(ReadString(), this);
                    }
                }
            }
            else
            {
                if (On(AsOrderedObjectDeserialize) && valueWriter.Count > 0)
                {
                    var fieldCountBackup = fieldCount;

                    fieldCount = size;

                    valueWriter.OnWriteAll(this);

                    size = fieldCount;

                    fieldCount = fieldCountBackup;
                }

                for (int i = 0; i < size; i++)
                {
                    valueWriter.OnWriteValue(ReadString(), this);
                }
            }

            --depth;
        }

        public sbyte ReadSByte() => checked((sbyte)ReadInt64());

        public float ReadSingle()
        {
            var @double = ReadDouble();
            var @float = (float)@double;

            if (!float.IsInfinity(@float))
            {
                return @float;
            }

            if (double.IsInfinity(@double))
            {
                return @float;
            }

            throw new OverflowException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public byte Current()
        {
            if (current >= end)
            {
                ThrowException();
            }

            return *current;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public byte Read()
        {
            if (current >= end)
            {
                ThrowException();
            }

            return *current++;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ushort Read2()
        {
            var value = *(ushort*)current;

            current += 2;

            if (current > end)
            {
                ThrowException();
            }

            return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public uint Read4()
        {
            var value = *(uint*)current;

            current += 4;

            if (current > end)
            {
                ThrowException();
            }

            return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong Read8()
        {
            var value = *(ulong*)current;

            current += 8;

            if (current > end)
            {
                ThrowException();
            }

            return BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void BackOff()
        {
            --current;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int TryReadStringHead()
        {
            switch (Read())
            {
                case var s when s >= FixStr && s <= FixStrMax:
                    return s - FixStr;
                case Str8:
                    return Read();
                case Str16:
                    return Read2();
                case Str32:
                    return checked((int)Read4());
                default:
                    BackOff();

                    return -1;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public Ps<Utf8Byte> InternalReadString(int bytesLength)
        {
            var ret = new Ps<Utf8Byte>((Utf8Byte*)current, bytesLength);

            current += bytesLength;

            return ret;
        }

        public string ReadString()
        {
            if (TryReadNull()) return null;

            int bytesLength;

            if (IsReferenceMode && On(MultiReferencingAlsoString))
            {
                var offset = Offset;

                bytesLength = TryReadStringHead();

                if (bytesLength >= 0)
                {
                    var ret = InternalReadString(bytesLength).ToStringEx();

                    References.Add(offset, ret);

                    return ret;
                }
            }
            else
            {
                bytesLength = TryReadStringHead();

                if (bytesLength >= 0)
                {
                    return InternalReadString(bytesLength).ToStringEx();
                }
            }

            return Convert.ToString(DirectRead());
        }

        public ushort ReadUInt16() => checked((ushort)ReadUInt64());

        public uint ReadUInt32() => checked((uint)ReadUInt64());

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            switch (Read())
            {
                case var i when i >= FixInt && i <= FixIntMax:
                    return i;
                case var ni when ni >= FixNegativeInt && ni <= FixNegativeIntMax:
                    return checked((ulong)unchecked((sbyte)ni));
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
                    return checked((ulong)Underlying.As<uint, float>(ref Underlying.AsRef(Read4())));
                case Float64:
                    return checked((ulong)Underlying.As<ulong, double>(ref Underlying.AsRef(Read8())));
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
            if (TryReadNull())
            {
                return null;
            }

            var offset = Offset;

            var length = TryReadBinaryHead();

            if (length >= 0)
            {
                return InternalReadBinary(length);
            }

            length = TryReadArrayHead();

            if (length >= 0)
            {
                var ret = new byte[length];

                if (IsReferenceMode)
                {
                    References.Add(offset, ret);
                }

                ++depth;

                if (ValueInterface<byte>.IsNotModified)
                {
                    for (int i = 0; i < length; i++)
                    {
                        ret[i] = ReadByte();
                    }
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        ret[i] = ValueInterface<byte>.ReadValue(this);
                    }
                }

                --depth;

                return ret;
            }

            if (TryReadEmptyString())
            {
                return null;
            }
            else
            {
                return XConvert.FromObject<byte[]>(DirectRead());
            }
        }

        public byte[] InternalReadBinary(int length)
        {
            var result = new byte[length];

            Underlying.CopyBlock(ref result[0], ref current[0], (uint)length);

            current += length;

            return result;
        }

        public int TryReadBinaryHead()
        {
            switch (Read())
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

            return XConvert.FromObject<Guid>(DirectRead());
        }

        public T ReadEnum<T>() where T : struct, Enum
        {
            var bytesLength = TryReadStringHead();

            if (bytesLength == 0 && (options & EmptyStringAsDefault) != 0)
            {
                return default;
            }

            if (bytesLength >= 0)
            {
                var utf8Str = InternalReadString(bytesLength);

                if (utf8Str.Length <= 1024)
                {
                    var chars = stackalloc char[StringHelper.GetUtf8MaxCharsLength(utf8Str.Length)];

                    var length = StringHelper.GetUtf8Chars((byte*)utf8Str.Pointer, utf8Str.Length, ref *chars);

                    if (EnumHelper.TryParseEnum<T>(new Ps<char>(chars, length), out var value))
                    {
                        return value;
                    }

                    return (T)Enum.Parse(typeof(T), StringHelper.ToString(chars, length));
                }
                else
                {
                    return (T)Enum.Parse(typeof(T), utf8Str.ToStringEx());
                }
            }

            switch (Read())
            {
                case var i when i >= FixInt && i <= FixIntMax:
                    return Underlying.As<int, T>(ref Underlying.AsRef((int)i));
                case var ni when ni >= FixNegativeInt && ni <= FixNegativeIntMax:
                    return Underlying.As<int, T>(ref Underlying.AsRef((int)ni));
                case Int8:
                    return Underlying.As<sbyte, T>(ref Underlying.AsRef((sbyte)Read()));
                case MessagePackCode.Int16:
                    return Underlying.As<short, T>(ref Underlying.AsRef((short)Read()));
                case MessagePackCode.Int32:
                    return Underlying.As<int, T>(ref Underlying.AsRef((int)Read()));
                case MessagePackCode.Int64:
                    return Underlying.As<long, T>(ref Underlying.AsRef((long)Read()));
                case UInt8:
                    return Underlying.As<byte, T>(ref Underlying.AsRef((byte)Read()));
                case MessagePackCode.UInt16:
                    return Underlying.As<ushort, T>(ref Underlying.AsRef((ushort)Read()));
                case MessagePackCode.UInt32:
                    return Underlying.As<uint, T>(ref Underlying.AsRef((uint)Read()));
                case MessagePackCode.UInt64:
                    return Underlying.As<ulong, T>(ref Underlying.AsRef((ulong)Read()));
                default:

                    BackOff();

                    return XConvert.FromObject<T>(DirectRead());
            }

        }

        public DateTimeOffset ReadDateTimeOffset()
        {
            var size = TryReadExtensionHead();

            if (size >= 0)
            {
                var code = (sbyte)Read();

                if (code == MessagePackExtensionCode.Timestamp)
                {
                    return InternalReadExtDateTime(size);
                }

                if (IsReferenceMode && code == MessagePackExtensionCode.Reference)
                {
                    return XConvert<DateTimeOffset>.FromObject(InternalReadExtReference(size));
                }

                return XConvert<DateTimeOffset>.Convert(InternalReadExtension(size, code));
            }

            if (TryReadParsedOfStr(out DateTimeOffset tResult))
            {
                return tResult;
            }

            return XConvert.FromObject<DateTimeOffset>(DirectRead());
        }

        public void SkipValue()
        {
            int count = 1;

            Loop:

            switch (Read())
            {
                case Bin8:
                case Bin16:
                case Bin32:
                    BackOff();
                    current = TryReadBinaryHead() + current;
                    break;
                case FixExt1:
                case FixExt2:
                case FixExt4:
                case FixExt8:
                case FixExt16:
                case Ext8:
                case Ext16:
                case Ext32:
                    BackOff();
                    current = TryReadExtensionHead() + current;
                    break;
                case Float32:
                    current += sizeof(float);
                    break;
                case Float64:
                    current += sizeof(double);
                    break;
                case UInt8:
                    current += sizeof(byte);
                    break;
                case MessagePackCode.UInt16:
                    current += sizeof(ushort);
                    break;
                case MessagePackCode.UInt32:
                    current += sizeof(uint);
                    break;
                case MessagePackCode.UInt64:
                    current += sizeof(ulong);
                    break;
                case Int8:
                    current += sizeof(sbyte);
                    break;
                case MessagePackCode.Int16:
                    current += sizeof(short);
                    break;
                case MessagePackCode.Int32:
                    current += sizeof(int);
                    break;
                case MessagePackCode.Int64:
                    current += sizeof(long);
                    break;
                case Str8:
                case Str16:
                case Str32:
                case var str when str >= FixStr && str <= FixStrMax:
                    BackOff();
                    current = TryReadStringHead() + current;
                    break;
                case Array16:
                case Array32:
                case var array when array >= FixArray && array <= FixArrayMax:
                    BackOff();
                    count += TryReadArrayHead();
                    break;
                case Map16:
                case Map32:
                case var map when map >= FixMap && map <= FixMapMax:
                    BackOff();
                    count += TryReadMapHead() * 2 /* Key and Value */;
                    break;
                case Nil:
                case False:
                case True:
                case var i when i >= FixInt && i <= FixIntMax:
                case var ni when ni >= FixNegativeInt && ni <= FixNegativeIntMax:
                default:
                    break;
            }

            --count;

            if (count > 0)
            {
                goto Loop;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T ReadValue<T>()
        {
            if (typeof(T) == typeof(int)) return As(ReadInt32());
            if (typeof(T) == typeof(string)) return As(ReadString());
            if (typeof(T) == typeof(double)) return As(ReadDouble());
            if (typeof(T) == typeof(bool)) return As(ReadBoolean());
            if (typeof(T) == typeof(byte)) return As(ReadByte());
            if (typeof(T) == typeof(sbyte)) return As(ReadSByte());
            if (typeof(T) == typeof(short)) return As(ReadInt16());
            if (typeof(T) == typeof(ushort)) return As(ReadUInt16());
            if (typeof(T) == typeof(uint)) return As(ReadUInt32());
            if (typeof(T) == typeof(long)) return As(ReadInt64());
            if (typeof(T) == typeof(ulong)) return As(ReadUInt64());
            if (typeof(T) == typeof(float)) return As(ReadSingle());
            if (typeof(T) == typeof(char)) return As(ReadChar());
            if (typeof(T) == typeof(decimal)) return As(ReadDecimal());
            if (typeof(T) == typeof(DateTime)) return As(ReadDateTime());
            if (typeof(T) == typeof(Guid)) return As(ReadGuid());
            if (typeof(T) == typeof(DateTimeOffset)) return As(ReadDateTimeOffset());
            if (typeof(T) == typeof(object)) return As(DirectRead());

            return ValueInterface<T>.ReadValue(this);

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            static T As<TInput>(TInput input)
                => Underlying.As<TInput, T>(ref input);
        }

        public void MakeTargetedId() { }

        byte[] IValueReader<byte[]>.ReadValue() => ReadBinary();

        Guid IValueReader<Guid>.ReadValue() => ReadGuid();

        DateTimeOffset IValueReader<DateTimeOffset>.ReadValue() => ReadDateTimeOffset();

        int[] IValueReader<int[]>.ReadValue() => ReadArray<int>();

        bool[] IValueReader<bool[]>.ReadValue() => ReadArray<bool>();

        // byte[] IValueReader<byte[]>.ReadValue() => ReadArray<byte>();

        string[] IValueReader<string[]>.ReadValue() => ReadArray<string>();

        DateTimeOffset[] IValueReader<DateTimeOffset[]>.ReadValue() => ReadArray<DateTimeOffset>();

        DateTime[] IValueReader<DateTime[]>.ReadValue() => ReadArray<DateTime>();

        decimal[] IValueReader<decimal[]>.ReadValue() => ReadArray<decimal>();

        double[] IValueReader<double[]>.ReadValue() => ReadArray<double>();

        float[] IValueReader<float[]>.ReadValue() => ReadArray<float>();

        ulong[] IValueReader<ulong[]>.ReadValue() => ReadArray<ulong>();

        uint[] IValueReader<uint[]>.ReadValue() => ReadArray<uint>();

        char[] IValueReader<char[]>.ReadValue() => ReadArray<char>();

        long[] IValueReader<long[]>.ReadValue() => ReadArray<long>();

        ushort[] IValueReader<ushort[]>.ReadValue() => ReadArray<ushort>();

        short[] IValueReader<short[]>.ReadValue() => ReadArray<short>();

        sbyte[] IValueReader<sbyte[]>.ReadValue() => ReadArray<sbyte>();

        object[] IValueReader<object[]>.ReadValue() => ReadArray<object>();

        List<int> IValueReader<List<int>>.ReadValue() => ReadList<int>();

        List<bool> IValueReader<List<bool>>.ReadValue() => ReadList<bool>();

        List<byte> IValueReader<List<byte>>.ReadValue() => ReadList<byte>();

        List<sbyte> IValueReader<List<sbyte>>.ReadValue() => ReadList<sbyte>();

        List<short> IValueReader<List<short>>.ReadValue() => ReadList<short>();

        List<ushort> IValueReader<List<ushort>>.ReadValue() => ReadList<ushort>();

        List<char> IValueReader<List<char>>.ReadValue() => ReadList<char>();

        List<uint> IValueReader<List<uint>>.ReadValue() => ReadList<uint>();

        List<long> IValueReader<List<long>>.ReadValue() => ReadList<long>();

        List<ulong> IValueReader<List<ulong>>.ReadValue() => ReadList<ulong>();

        List<float> IValueReader<List<float>>.ReadValue() => ReadList<float>();

        List<double> IValueReader<List<double>>.ReadValue() => ReadList<double>();

        List<decimal> IValueReader<List<decimal>>.ReadValue() => ReadList<decimal>();

        List<DateTime> IValueReader<List<DateTime>>.ReadValue() => ReadList<DateTime>();

        List<DateTimeOffset> IValueReader<List<DateTimeOffset>>.ReadValue() => ReadList<DateTimeOffset>();

        List<string> IValueReader<List<string>>.ReadValue() => ReadList<string>();

        List<object> IValueReader<List<object>>.ReadValue() => ReadList<object>();

        public void OnReadValue(Ps<Utf8Byte> key, IValueWriter valueWriter)
        {
            valueWriter.DirectWrite(this[key].DirectRead());
        }

        public void OnReadAll(IDataWriter<Ps<Utf8Byte>> dataWriter)
        {
            // TODO: 
            throw new NotSupportedException();
        }

        public void OnReadValue(string key, IValueWriter valueWriter)
        {
            valueWriter.DirectWrite(this[key].DirectRead());
        }

        public void OnReadAll(IDataWriter<string> dataWriter)
        {
            // TODO: 
            throw new NotSupportedException();
        }

        MessagePackExtension IValueReader<MessagePackExtension>.ReadValue() => ReadExtension();

        sealed class WriteMapInvoker : IGenericInvoker
        {
            readonly MessagePackDeserializer<TMode> deserializer;
            readonly IDataWriter dataWriter;

            public WriteMapInvoker(MessagePackDeserializer<TMode> deserializer, IDataWriter dataWriter)
            {
                this.deserializer = deserializer;
                this.dataWriter = dataWriter;
            }

            public void Invoke<TKey>()
            {
                deserializer.ReadMap(Underlying.As<IDataWriter<TKey>>(dataWriter));
            }
        }

        public sealed class InternalWriteMapInvoker : IGenericInvoker
        {
            readonly MessagePackDeserializer<TMode> deserializer;
            readonly IDataWriter dataWriter;

            public InternalWriteMapInvoker(MessagePackDeserializer<TMode> deserializer, IDataWriter dataWriter)
            {
                this.deserializer = deserializer;
                this.dataWriter = dataWriter;
            }

            public void Invoke<TKey>()
            {
                deserializer.InternalReadMap(Underlying.As<IDataWriter<TKey>>(dataWriter));
            }
        }
    }
}