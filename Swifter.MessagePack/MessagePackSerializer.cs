using Swifter.Formatters;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Swifter.MessagePack.MessagePackCode;
using static Swifter.MessagePack.MessagePackFormatterOptions;
using static Swifter.MessagePack.MessagePackSerializeModes;
using static Swifter.MessagePack.MessagePackFormatter;

namespace Swifter.MessagePack
{
    sealed unsafe class MessagePackSerializer<TMode> :
        IMessagePackWriter,
        IValueWriter<Guid>,
        IValueWriter<DateTimeOffset>,
        IDataWriter<string>,
        IDataWriter<int>,
        IDataWriter<Ps<Utf8Byte>>,
        IValueFilter<string>,
        IValueFilter<int>,
        IValueWriter<byte[]>,
        IValueWriter<MessagePackExtension>,
        IValueWriter<bool[]>, IValueWriter<List<bool>>,
        /* IValueWriter<byte[]>, */IValueWriter<List<byte>>,
        IValueWriter<sbyte[]>, IValueWriter<List<sbyte>>,
        IValueWriter<short[]>, IValueWriter<List<short>>,
        IValueWriter<ushort[]>, IValueWriter<List<ushort>>,
        IValueWriter<char[]>, IValueWriter<List<char>>,
        IValueWriter<int[]>, IValueWriter<List<int>>,
        IValueWriter<uint[]>, IValueWriter<List<uint>>,
        IValueWriter<long[]>, IValueWriter<List<long>>,
        IValueWriter<ulong[]>, IValueWriter<List<ulong>>,
        IValueWriter<float[]>, IValueWriter<List<float>>,
        IValueWriter<double[]>, IValueWriter<List<double>>,
        IValueWriter<DateTime[]>, IValueWriter<List<DateTime>>,
        IValueWriter<DateTimeOffset[]>, IValueWriter<List<DateTimeOffset>>,
        IValueWriter<decimal[]>, IValueWriter<List<decimal>>,
        IValueWriter<Guid[]>, IValueWriter<List<Guid>>,
        IValueWriter<string[]>, IValueWriter<List<string>>,
        IFormatterWriter
        where TMode : struct
    {
        public TMode mode;

        public readonly MessagePackFormatter messagePackFormatter;
        public readonly HGlobalCache<byte> hGCache;
        public readonly int maxDepth;

        public readonly MessagePackFormatterOptions options;

        public int depth;

        public byte* current;

        public uint fieldCount;

        public int Offset
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => (int)(current - hGCache.First);
        }

        public Dictionary<object, int> References
            => Underlying.As<TMode, ReferenceMode>(ref mode).References;

        public void InitReferences()
        {
            Underlying.As<TMode, ReferenceMode>(ref mode).References = new Dictionary<object, int>(TypeHelper.ReferenceComparer);
        }

        public void Flush()
        {
            hGCache.Count = (int)(current - hGCache.First);
        }

        public void Clear()
        {
            current = hGCache.First;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public MessagePackSerializer(HGlobalCache<byte> hGCache, int maxDepth)
        {
            this.hGCache = hGCache;
            this.maxDepth = maxDepth;
            options = Default;

            current = hGCache.First;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public MessagePackSerializer(HGlobalCache<byte> hGCache, int maxDepth, MessagePackFormatterOptions options)
        {
            this.hGCache = hGCache;
            this.maxDepth = maxDepth;
            this.options = options;

            current = hGCache.First;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public MessagePackSerializer(MessagePackFormatter messagePackFormatter, HGlobalCache<byte> hGCache, int maxDepth, MessagePackFormatterOptions options)
            : this(hGCache, maxDepth, options)
        {
            this.messagePackFormatter = messagePackFormatter;
        }

        public IValueWriter this[Ps<Utf8Byte> key]
        {
            get
            {
                ++fieldCount;

                WriteUtf8String(key);

                if (On(CamelCaseWhenSerialize) && key.Length > 0)
                {
                    ref var firstChar = ref current[-key.Length];

                    firstChar = StringHelper.ToLower(firstChar);
                }

                return this;
            }
        }

        public IValueWriter this[string key]
        {
            get
            {
                if (key is null)
                {
                    // TODO: MessagePack 对象的键不允许为 Null。
                    throw new ArgumentNullException(nameof(key));
                }

                ++fieldCount;

                var bytesLength = WriteUnicodeString(ref StringHelper.GetRawStringData(key), key.Length);

                if (On(CamelCaseWhenSerialize) && bytesLength > 0)
                {
                    ref var firstChar = ref current[-bytesLength];

                    firstChar = StringHelper.ToLower(firstChar);
                }

                return this;
            }
        }

        public IValueWriter this[int key]
        {
            get
            {
                ++fieldCount;

                return this;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append(byte value)
        {
            *current = value; ++current;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append2(ushort value)
        {
            *(ushort*)current =
                BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(value)
                : value;

            current += 2;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append4(uint value)
        {
            *(uint*)current =
                BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(value)
                : value;

            current += 4;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append8(ulong value)
        {
            *(ulong*)current =
                BitConverter.IsLittleEndian
                ? BinaryPrimitives.ReverseEndianness(value)
                : value;

            current += 8;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Expand(int expandMinSize)
        {
            if (current + expandMinSize > hGCache.Last)
            {
                InternalExpand(expandMinSize);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InternalExpand(int expandMinSize)
        {
            var offset = Offset;

            hGCache.Expand(expandMinSize);

            current = hGCache.First + offset;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ThrowOutOfDepthException()
        {
            if ((options & OutOfDepthException) != 0)
            {
                throw new MessagePackOutOfDepthException();
            }
        }

        public bool Filter(ValueFilterInfo<string> valueInfo)
        {
            if (messagePackFormatter != null)
            {
                return messagePackFormatter.OnObjectFilter(this, valueInfo, Filter(valueInfo.ValueCopyer));
            }
            else
            {
                return Filter(valueInfo.ValueCopyer);
            }
        }

        public bool Filter(ValueFilterInfo<int> valueInfo)
        {
            if (messagePackFormatter != null)
            {
                return messagePackFormatter.OnArrayFilter(this, valueInfo, Filter(valueInfo.ValueCopyer));
            }
            else
            {
                return Filter(valueInfo.ValueCopyer);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool On(MessagePackFormatterOptions options)
            => (this.options & options) != 0;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool Are(MessagePackFormatterOptions options)
            => (this.options & options) == options;

        public static bool IsReferenceMode
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => typeof(TMode) == typeof(ReferenceMode);
        }

        public bool IsFilterMode
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => options >= OnFilter;
        }

        public bool Filter(ValueCopyer valueCopyer)
        {
            var basicType = valueCopyer.TypeCode;

            if (IsReferenceMode)
            {
                if (
                    (
                        On(MultiReferencingNull | LoopReferencingNull) &&
                        basicType == TypeCode.Object &&
                        valueCopyer.InternalObject is IDataReader dataReader &&
                        dataReader.ContentType?.IsValueType == false &&
                        References.ContainsKey(dataReader.Content)
                    )
                    ||
                    (
                        Are(MultiReferencingNull | MultiReferencingAlsoString) &&
                        basicType == TypeCode.String &&
                        valueCopyer.InternalObject is string str &&
                        References.ContainsKey(str)
                    )
                   )
                {
                    valueCopyer.DirectWrite(null);

                    basicType = TypeCode.Empty;
                }
            }

            if (basicType == TypeCode.Empty && On(IgnoreNull))
            {
                return false;
            }

            if (On(IgnoreZero))
            {
                switch (basicType)
                {
                    case TypeCode.SByte:
                        return valueCopyer.ReadSByte() != 0;
                    case TypeCode.Int16:
                        return valueCopyer.ReadInt16() != 0;
                    case TypeCode.Int32:
                        return valueCopyer.ReadInt32() != 0;
                    case TypeCode.Int64:
                        return valueCopyer.ReadInt64() != 0;
                    case TypeCode.Byte:
                        return valueCopyer.ReadByte() != 0;
                    case TypeCode.UInt16:
                        return valueCopyer.ReadUInt16() != 0;
                    case TypeCode.UInt32:
                        return valueCopyer.ReadUInt32() != 0;
                    case TypeCode.UInt64:
                        return valueCopyer.ReadUInt64() != 0;
                    case TypeCode.Single:
                        return valueCopyer.ReadSingle() != 0;
                    case TypeCode.Double:
                        return valueCopyer.ReadDouble() != 0;
                    case TypeCode.Decimal:
                        return valueCopyer.ReadDecimal() != 0;
                }
            }

            if (basicType == TypeCode.String && On(IgnoreEmptyString) && valueCopyer.ReadString() == string.Empty)
            {
                return false;
            }

            return true;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteNull()
        {
            Expand(1);

            Append(Nil);
        }

        public void WriteReference(int address)
        {
            // 循环引用的标识符是扩展类型 -10 + 被引用对象的字节地址。

            if (address < 0)
            {
                throw new NullReferenceException(nameof(address));
            }

            if (address <= byte.MaxValue)
            {
                WriteExtensionHead(sizeof(byte), MessagePackExtensionCode.Reference);

                Append((byte)address);
            }
            else if (address <= ushort.MaxValue)
            {
                WriteExtensionHead(sizeof(ushort), MessagePackExtensionCode.Reference);

                Append2((ushort)address);
            }
            else
            {
                WriteExtensionHead(sizeof(uint), MessagePackExtensionCode.Reference);

                Append4((uint)address);
            }
        }

        public bool TryWriteReference(object token)
        {
            if (References.TryGetValue(token, out var address))
            {
                if (On(LoopReferencingException))
                {
                    // TODO: 因为 MessagePack 的引用无需记录路径，所以在异常信息中增加循环引用的路径。
                    throw new MessagePackLoopReferencingException(token);
                }
                else if (On(LoopReferencingNull | MultiReferencingNull))
                {
                    WriteNull();
                }
                else if (On(MultiReferencingReference))
                {
                    WriteReference(address);
                }

                return true;
            }
            else
            {
                address = (int)(current - hGCache.First);

                References.Add(token, address);

                return false;
            }
        }

        public void ReferenceAfter(object token)
        {
            if (On(LoopReferencingException | LoopReferencingNull))
            {
                References.Remove(token);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void DirectWrite(object value)
        {
            if (IsReferenceMode && On(MultiReferencingAlsoString) && On(MultiReferencingReference | MultiReferencingNull) && TryWriteReference(value))
            {
                return;
            }

            Loop:

            if (value is null)
            {
                WriteNull();

                return;
            }

            if (value is IConvertible convertion)
            {
                switch (convertion.GetTypeCode())
                {
                    case TypeCode.Boolean:
                        WriteBoolean((bool)value);
                        return;
                    case TypeCode.Byte:
                        WriteByte((byte)value);
                        return;
                    case TypeCode.Char:
                        WriteChar((char)value);
                        return;
                    case TypeCode.DateTime:
                        WriteDateTime((DateTime)value);
                        return;
                    case TypeCode.DBNull:
                        WriteNull();
                        return;
                    case TypeCode.Decimal:
                        WriteDecimal((decimal)value);
                        return;
                    case TypeCode.Double:
                        WriteDouble((double)value);
                        return;
                    case TypeCode.Int16:
                        WriteInt64((short)value);
                        return;
                    case TypeCode.Int32:
                        WriteInt32((int)value);
                        return;
                    case TypeCode.Int64:
                        WriteInt64((long)value);
                        return;
                    case TypeCode.SByte:
                        WriteSByte((sbyte)value);
                        return;
                    case TypeCode.Single:
                        WriteSingle((float)value);
                        return;
                    case TypeCode.UInt16:
                        WriteUInt16((ushort)value);
                        return;
                    case TypeCode.UInt32:
                        WriteUInt32((uint)value);
                        return;
                    case TypeCode.UInt64:
                        WriteUInt64((ulong)value);
                        return;
                    case TypeCode.String:
                        WriteUnicodeString(ref StringHelper.GetRawStringData((string)value), Underlying.As<string>(value).Length);
                        return;
                }
            }

            if (value is Guid guid)
            {
                WriteGuid(guid);

                return;
            }

            if (value is DateTimeOffset dto)
            {
                WriteDateTimeOffset(dto);

                return;
            }

            if (value is byte[] bytes)
            {
                WriteBinary(bytes);

                return;
            }

            value = value.ToString();

            goto Loop;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteArrayHead(int length)
        {
            Expand(5);

            if (length >= 0 && length <= FixArrayMaxCount)
            {
                Append((byte)(FixArray | length));
            }
            else if (length >= 0 && length <= Array16MaxCount)
            {
                Append(Array16);

                Append2((ushort)length);
            }
            else
            {
                Append(Array32);

                Append4(checked((uint)length));
            }
        }

        public void WriteArray(IDataReader<int> dataReader)
        {
            // 仅在满足以下条件时才会检查对象引用并写入引用地址。
            if (IsReferenceMode && dataReader.ContentType?.IsValueType == false && TryWriteReference(dataReader.Content))
            {
                return;
            }

            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();
            }
            else
            {
                ++depth;

                var fieldCountBackup = fieldCount;
                var offset = Offset;
                var count = dataReader.Count;

                fieldCount = 0;

                // 如果是未知长度的 Array，则先写入一个 Array32 的长度，后期再改为实际长度。
                if (count < 0)
                {
                    count = int.MaxValue;
                }

                WriteArrayHead(count);

                if (IsFilterMode)
                {
                    dataReader.OnReadAll(new DataFilterWriter<int>(this, this));
                }
                else
                {
                    dataReader.OnReadAll(this);
                }

                if (fieldCount != count)
                {
                    ModifyArrayLength(hGCache.First + offset, fieldCount);
                }

                fieldCount = fieldCountBackup;

                --depth;
            }

            // 需要在对象结束序列化时进行某些引用相关的操作
            if (IsReferenceMode && dataReader.ContentType?.IsValueType == false)
            {
                ReferenceAfter(dataReader.Content);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void ModifyArrayLength(byte* address, uint length)
        {
            switch (*address)
            {
                case Array32:

                    ++address;

                    *(uint*)address = BitConverter.IsLittleEndian
                    ? BinaryPrimitives.ReverseEndianness(length)
                    : length;

                    break;

                case Array16:

                    if (length > Array16MaxCount)
                    {
                        ThrowException();
                    }

                    ++address;

                    *(ushort*)address = BitConverter.IsLittleEndian
                    ? BinaryPrimitives.ReverseEndianness((ushort)length)
                    : (ushort)length;

                    break;

                default:

                    if (length > FixArrayMaxCount)
                    {
                        ThrowException();
                    }

                    *address = (byte)(FixArray | length);

                    break;

            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void ThrowException()
                => throw new NotSupportedException();
        }

        public void WriteArray<T>(T[] array)
        {
            if (IsReferenceMode && TryWriteReference(array))
            {
                return;
            }

            InternalWriteArray(array, array.Length);

            if (IsReferenceMode)
            {
                ReferenceAfter(array);
            }
        }

        public void WriteList<T>(List<T> list)
        {
            if (IsReferenceMode && TryWriteReference(list))
            {
                return;
            }

            InternalWriteArray(ArrayHelper.GetRawData(list), list.Count);

            if (IsReferenceMode)
            {
                ReferenceAfter(list);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void InternalWriteArray<T>(T[] array, int length)
        {
            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();
            }
            else
            {
                ++depth;

                if (IsFilterMode && On(ArrayOnFilter))
                {
                    var offset = Offset;

                    WriteArrayHead(length);

                    uint count = 0;

                    var filterInfo = new ValueFilterInfo<int>();

                    for (int i = 0; i < length; i++)
                    {
                        filterInfo.Key = i;
                        filterInfo.Type = typeof(T);

                        ValueInterface.WriteValue(filterInfo.ValueCopyer, array[i]);

                        if (Filter(filterInfo))
                        {
                            ++count;

                            filterInfo.ValueCopyer.WriteTo(this);
                        }
                    }

                    if (count != length)
                    {
                        ModifyArrayLength(hGCache.First + offset, count);
                    }
                }
                else
                {
                    WriteArrayHead(length);

                    // 如果 T 的写入器是默认的，则无需经过 ValueInterface，以提高性能。
                    if (ValueInterface<T>.IsNotModified)
                    {
                        for (int i = 0; i < length; i++)
                        {
                            WriteValue(array[i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < length; i++)
                        {
                            ValueInterface.WriteValue(this, array[i]);
                        }
                    }
                }

                --depth;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteValue<T>(T value)
        {
            if (typeof(T) == typeof(int)) WriteInt32(Underlying.As<T, int>(ref value));
            else if (typeof(T) == typeof(string)) WriteString(Underlying.As<T, string>(ref value));
            else if (typeof(T) == typeof(double)) WriteDouble(Underlying.As<T, double>(ref value));
            else if (typeof(T) == typeof(bool)) WriteBoolean(Underlying.As<T, bool>(ref value));
            else if (typeof(T) == typeof(byte)) WriteByte(Underlying.As<T, byte>(ref value));
            else if (typeof(T) == typeof(sbyte)) WriteSByte(Underlying.As<T, sbyte>(ref value));
            else if (typeof(T) == typeof(short)) WriteInt16(Underlying.As<T, short>(ref value));
            else if (typeof(T) == typeof(ushort)) WriteUInt16(Underlying.As<T, ushort>(ref value));
            else if (typeof(T) == typeof(uint)) WriteUInt32(Underlying.As<T, uint>(ref value));
            else if (typeof(T) == typeof(long)) WriteInt64(Underlying.As<T, long>(ref value));
            else if (typeof(T) == typeof(ulong)) WriteUInt64(Underlying.As<T, ulong>(ref value));
            else if (typeof(T) == typeof(float)) WriteSingle(Underlying.As<T, float>(ref value));
            else if (typeof(T) == typeof(char)) WriteChar(Underlying.As<T, char>(ref value));
            else if (typeof(T) == typeof(decimal)) WriteDecimal(Underlying.As<T, decimal>(ref value));
            else if (typeof(T) == typeof(DateTime)) WriteDateTime(Underlying.As<T, DateTime>(ref value));
            else if (typeof(T) == typeof(Guid)) WriteGuid(Underlying.As<T, Guid>(ref value));
            else if (typeof(T) == typeof(DateTimeOffset)) WriteDateTimeOffset(Underlying.As<T, DateTimeOffset>(ref value));
            else ValueInterface<T>.WriteValue(this, value);
        }

        public void WriteBoolean(bool value)
        {
            Expand(1);

            Append(value ? True : False);
        }

        public void WriteByte(byte value) => WriteUInt64(value);

        public void WriteChar(char value)
        {
            // 字符在 MessagePack 标准中属于一个长度的字符串。
            WriteUnicodeString(ref value, 1);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteExtensionHead(int size, sbyte code)
        {
            Expand(6);

            switch (size)
            {
                case 1:
                    Append(FixExt1);
                    break;
                case 2:
                    Append(FixExt2);
                    break;
                case 4:
                    Append(FixExt4);
                    break;
                case 8:
                    Append(FixExt8);
                    break;
                case 16:
                    Append(FixExt16);
                    break;
                default:
                    if (size <= byte.MaxValue)
                    {
                        Append(Ext8);
                        Append((byte)size);
                    }
                    else if (size <= ushort.MaxValue)
                    {
                        Append(Ext16);
                        Append2((ushort)size);
                    }
                    else
                    {
                        Append(Ext32);
                        Append4((uint)size);
                    }

                    break;
            }

            Append((byte)code);
        }

        public void WriteExtension(MessagePackExtension extension)
        {
            WriteExtensionHead(extension.Binary.Length, extension.Code);

            Expand(extension.Binary.Length);

            Underlying.CopyBlock(
                ref *current,
                ref extension.Binary[0],
                (uint)extension.Binary.Length);

            current += extension.Binary.Length;
        }

        public void WriteDateTime(DateTime value)
        {
            Expand(15);

            var ticks = value.Ticks - DateTimeHelper.TicksPerUTCDifference - DateTimeHelper.TicksPerUnixEpoch;

            var seconds = ticks / TimeSpan.TicksPerSecond;
            var seconds_ticks = seconds * TimeSpan.TicksPerSecond;

            if ((On(UseTimestamp32) || seconds_ticks == ticks) && seconds >= 0 && seconds <= UInt32MaxValue)
            {
                WriteExtensionHead(sizeof(uint), MessagePackExtensionCode.Timestamp);

                Append4((uint)seconds);

                return;
            }

            if (ticks < seconds_ticks)
            {
                --seconds;

                seconds_ticks = seconds * TimeSpan.TicksPerSecond;
            }

            var nanoseconds = (ticks - seconds_ticks) * DateTimeHelper.NanosecondsPerTick;

            if (seconds >= 0 && seconds <= UInt34MaxValue)
            {
                WriteExtensionHead(sizeof(ulong), MessagePackExtensionCode.Timestamp);

                Append8((ulong)((nanoseconds << 34) | (seconds)));

                return;
            }

            WriteExtensionHead(sizeof(uint) + sizeof(ulong), MessagePackExtensionCode.Timestamp);

            Append4((uint)nanoseconds);
            Append8((ulong)seconds);
        }

        public void WriteDecimal(decimal value)
        {
            const double DecimalMin = -7.92281625142643E+28;
            const double DecimalMax = 7.92281625142643E+28;

            var scale = NumberHelper.GetScale(value);

            if (scale == 0)
            {
                if (value >= ulong.MinValue && value <= ulong.MaxValue)
                {
                    WriteUInt64((ulong)value);

                    return;
                }

                if (value >= long.MinValue && value <= long.MaxValue)
                {
                    WriteInt64((long)value);

                    return;
                }
            }

            var doubleValue = (double)value;

            if (doubleValue >= DecimalMin && doubleValue <= DecimalMax && (decimal)doubleValue == value)
            {
                WriteDouble(doubleValue);

                return;
            }

            // 十进制数字在特别大或特别小时，在 MessagePack 下无法容下，所以将特别大或特别小的十进制数字转换为处理。

            var chars = stackalloc char[NumberHelper.DecimalStringMaxLength];

            var length = NumberHelper.ToString(value, chars);

            WriteASCIIString(chars, length);
        }

        public void WriteDouble(double value)
        {
            var int64Value = (long)value;

            // 如果 value 是整数，则使用 Int64 格式序列化。

            if (int64Value == value)
            {
                WriteInt64(int64Value);

                return;
            }

            Expand(9);

            var singleValue = (float)value;

            if (singleValue == value)
            {
                Append(Float32);
                Append4(Underlying.As<float, uint>(ref singleValue));
            }
            else
            {
                Append(Float64);
                Append8(Underlying.As<double, ulong>(ref value));
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt16(short value) => WriteInt64(value);

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt32(int value) => WriteInt64(value);

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt64(long value)
        {
            // 我在这里检查了 value 的数值，保证了数值不丢失的前提下以使用最少的字节数序列化整数。

            if (value >= 0)
            {
                WriteUInt64((ulong)value);

                return;
            }

            Expand(9);

            if (value >= FixNegativeIntMinValue)
            {
                Append((byte)(FixNegativeInt | value));

                return;
            }
            else if (value >= Int8MinValue)
            {
                Append(Int8);

                Append((byte)value);
            }
            else if (value >= Int16MinValue)
            {
                Append(MessagePackCode.Int16);

                Append2((ushort)value);
            }
            else if (value >= Int32MinValue)
            {
                Append(MessagePackCode.Int32);

                Append4((uint)value);
            }
            else
            {
                Append(MessagePackCode.Int64);

                Append8((ulong)value);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteMapHead(int length)
        {
            Expand(5);

            if (length >= 0 && length <= FixMapMaxCount)
            {
                Append((byte)(FixMap | length));
            }
            else if (length >= 0 && length <= Map16MaxCount)
            {
                Append(Map16);

                Append2((ushort)length);
            }
            else
            {
                Append(Map32);

                Append4((uint)length);
            }
        }

        public void WriteObject(IDataReader<string> dataReader)
        {
            // 因为 MessagePack 里允许任何类型的键，所以如果此读写器是由其他类型的键转换为 string 键的读写器，则将序列化原始读写器到。
            if (dataReader is IAsDataReader @as)
            {
                @as.InvokeTIn(new WriteMapInvoker(this, @as.Original));

                return;
            }

            // 仅在满足以下条件时才会检查对象引用并写入引用地址。
            if (IsReferenceMode && dataReader.ContentType?.IsValueType == false && TryWriteReference(dataReader.Content))
            {
                return;
            }

            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();
            }
            else
            {
                ++depth;

                var fieldCountBackup = fieldCount;
                var offset = Offset;
                var count = dataReader.Count;

                fieldCount = 0;

                // 如果是未知长度的 Object，则先写入一个 Map32 的长度，后期再改为实际长度。
                if (count < 0)
                {
                    count = int.MaxValue;
                }

                WriteMapHead(count);

                if (IsFilterMode)
                {
                    dataReader.OnReadAll(new DataFilterWriter<string>(this, this));
                }
                else if(dataReader is IDataReader<Ps<Utf8Byte>> fastReader)
                {
                    fastReader.OnReadAll(this);
                }
                else
                {
                    dataReader.OnReadAll(this);
                }

                if (count != fieldCount)
                {
                    ModifyMapLength(hGCache.First + offset, fieldCount);
                }

                fieldCount = fieldCountBackup;

                --depth;
            }

            // 需要在对象结束序列化时进行某些引用相关的操作
            if (IsReferenceMode && !dataReader.ContentType.IsValueType)
            {
                ReferenceAfter(dataReader.Content);
            }
        }

        public void WriteSByte(sbyte value) => WriteInt64(value);

        public void WriteSingle(float value)
        {
            var int32Value = (int)value;

            if (int32Value == value)
            {
                WriteInt32(int32Value);

                return;
            }

            Expand(5);

            Append(Float32);
            Append4(Underlying.As<float, uint>(ref value));
        }

        public void WriteString(string value)
        {
            if (value is null)
            {
                WriteNull();
            }
            else
            {
                if (IsReferenceMode &&
                    On(MultiReferencingAlsoString) &&
                    On(MultiReferencingReference | MultiReferencingNull) &&
                    TryWriteReference(value))
                {
                    return;
                }

                WriteUnicodeString(ref StringHelper.GetRawStringData(value), value.Length);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int GetStringHeadOffset(int length)
        {
            if (length <= FixStrMaxLength) return 1;
            else if (length <= Str8MaxLength) return 2;
            else if (length <= Str16MaxLength) return 3;
            else return 5;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteStringHead(int length)
        {
            if (length <= FixStrMaxLength)
            {
                Append((byte)(FixStr | length));
            }
            else if (length <= Str8MaxLength)
            {
                Append(Str8);
                Append((byte)length);
            }
            else if (length <= Str16MaxLength)
            {
                Append(Str16);
                Append2((ushort)length);
            }
            else
            {
                Append(Str32);
                Append4((uint)length);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int WriteUnicodeString(ref char first, int length)
        {
            Expand(StringHelper.GetUtf8MaxBytesLength(length) + 5);

            var predictedOffset = GetStringHeadOffset(length); // 预测偏移量

            var bytesLength = StringHelper.GetUtf8Bytes(ref first, length, current + predictedOffset);

            var actualOffset = GetStringHeadOffset(bytesLength); // 实际偏移量

            if (actualOffset != predictedOffset)
            {
                Underlying.CopyBlock(
                    current + actualOffset, // destination
                    current + predictedOffset, // source
                    (uint)bytesLength); // byteCount
            }

            WriteStringHead(bytesLength);

            current += bytesLength;

            return bytesLength;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteASCIIString(char* chars, int length)
        {
            Expand(length + 5);

            WriteStringHead(length);

            while (length >= 4)
            {
                *(uint*)current = CloseRight(*(ulong*)chars);

                current += 4;
                chars += 4;
                length -= 4;
            }

            while (length >= 1)
            {
                *current = (byte)*chars;

                ++current;
                ++chars;
                --length;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUtf8String(Ps<Utf8Byte> value)
        {
            Expand(value.Length + 5);

            WriteStringHead(value.Length);

            Underlying.CopyBlock(current, value.Pointer, ((uint)value.Length) * sizeof(char));

            current += value.Length;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt16(ushort value) => WriteUInt64(value);

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt32(uint value) => WriteUInt64(value);

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt64(ulong value)
        {
            Expand(9);

            // 我在这里检查了 value 的数值，保证了数值不丢失的前提下以使用最少的字节数序列化整数。
            if (value <= FixPositiveIntMaxValue)
            {
                Append((byte)value);
            }
            else if (value <= UInt8MaxValue)
            {
                Append(UInt8);

                Append((byte)value);
            }
            else if (value <= UInt16MaxValue)
            {
                Append(MessagePackCode.UInt16);

                Append2((ushort)value);
            }
            else if (value <= UInt32MaxValue)
            {
                Append(MessagePackCode.UInt32);

                Append4((uint)value);
            }
            else
            {
                Append(MessagePackCode.UInt64);

                Append8(value);
            }
        }

        public void WriteBinary(byte[] value)
        {
            if (value is null)
            {
                WriteNull();
            }
            else
            {
                WriteBinary(ref value[0], value.Length);
            }
        }

        public void WriteBinary(ref byte firstByte, int length)
        {
            Expand(8 + length);

            if (length <= UInt8MaxValue)
            {
                Append(Bin8);
                Append((byte)length);
            }
            else if (length <= UInt16MaxValue)
            {
                Append(Bin16);
                Append2((ushort)length);
            }
            else
            {
                Append(Bin32);
                Append4(checked((uint)length));
            }

            Underlying.CopyBlock(
                ref *current,
                ref firstByte,
                (uint)length);

            current += length;
        }

        public void WriteDateTimeOffset(DateTimeOffset value)
        {
            // TODO: DateTimeOffset 目前在 MessagePack 标准中未定义，所以转换为字符串处理。

            var chars = stackalloc char[DateTimeHelper.ISOStringMaxLength];

            var length = DateTimeHelper.ToISOString(value, chars);

            WriteASCIIString(chars, length);
        }

        public void WriteGuid(Guid value)
        {
            // TODO: Guid 目前在 MessagePack 标准中未定义，所以转换为字符串处理。

            var chars = stackalloc char[NumberHelper.GuidStringWithSeparatorsLength];

            var length = NumberHelper.ToString(value, chars, true);

            WriteASCIIString(chars, length);

        }

        public void WriteEnum<T>(T value) where T : struct, Enum
        {
            // TODO: 枚举不是未知类型，这里直接用字符串格式或整数格式序列化。

            if (EnumHelper.IsFlagsEnum<T>() && EnumHelper.AsUInt64<T>(value) != 0)
            {
                // TODO: 此处限制了 Enum Flags 的最大字符串长度；如果超过了这个长度，则以整数形式序列化。

                const int MaxEnumLength = 256;

                var chars = stackalloc char[MaxEnumLength];

                if (EnumHelper.AsUInt64(EnumHelper.FormatEnumFlags(value, chars, MaxEnumLength, out var charsWritten)) == 0)
                {
                    // 枚举的名字有可能存在多字节。

                    WriteUnicodeString(ref *chars, charsWritten);

                    return;
                }
            }
            else if (EnumHelper.GetEnumName(value) is string name)
            {
                // 无需考虑 name 为 Null 的情况，这种情况不合法。

                WriteUnicodeString(ref StringHelper.GetRawStringData(name), name.Length);

                return;
            }

            switch (EnumHelper.GetEnumTypeCode<T>())
            {
                case TypeCode.SByte:
                    WriteInt64(Underlying.As<T, sbyte>(ref value));
                    break;
                case TypeCode.Int16:
                    WriteInt64(Underlying.As<T, short>(ref value));
                    break;
                case TypeCode.Int32:
                    WriteInt64(Underlying.As<T, int>(ref value));
                    break;
                case TypeCode.Int64:
                    WriteInt64(Underlying.As<T, long>(ref value));
                    break;
                default:
                    WriteUInt64(EnumHelper.AsUInt64(value));
                    break;
            }
        }

        public void WriteMap<TKey>(IDataReader<TKey> mapReader)
        {
            // 仅在满足以下条件时才会检查对象引用并写入引用地址。
            if (IsReferenceMode && mapReader.ContentType?.IsValueType == false && TryWriteReference(mapReader.Content))
            {
                return;
            }

            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();
            }
            else
            {
                ++depth;

                var fieldCountBackup = fieldCount;
                var offset = Offset;
                var count = mapReader.Count;

                fieldCount = 0;

                var mapWriter = new MapWriter<TKey>(this);

                // 如果是未知长度的 Map，则先写入一个 Map32 的长度，后期再改为实际长度。
                if (count < 0)
                {
                    count = int.MaxValue;
                }

                WriteMapHead(count);

                if (IsFilterMode)
                {
                    mapReader.OnReadAll(new DataFilterWriter<TKey>(mapWriter, mapWriter));
                }
                else
                {
                    mapReader.OnReadAll(mapWriter);
                }

                if (count != fieldCount)
                {
                    ModifyMapLength(hGCache.First + offset, fieldCount);
                }

                fieldCount = fieldCountBackup;

                --depth;
            }

            // 需要在对象结束序列化时进行某些引用相关的操作
            if (IsReferenceMode && mapReader.ContentType?.IsValueType == false)
            {
                ReferenceAfter(mapReader.Content);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void ModifyMapLength(byte* address, uint length)
        {
            switch (*address)
            {
                case Map32:

                    ++address;

                    *(uint*)address = BitConverter.IsLittleEndian
                    ? BinaryPrimitives.ReverseEndianness(length)
                    : length;

                    break;

                case Map16:

                    if (length > Map16MaxCount)
                    {
                        ThrowException();
                    }

                    ++address;

                    *(ushort*)address = BitConverter.IsLittleEndian
                    ? BinaryPrimitives.ReverseEndianness((ushort)length)
                    : (ushort)length;

                    break;

                default:

                    if (length > FixMapMaxCount)
                    {
                        ThrowException();
                    }

                    *address = (byte)(FixMap | length);

                    break;

            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static void ThrowException() 
                => throw new NotSupportedException();
        }

        void IValueWriter<Guid>.WriteValue(Guid value) => WriteGuid(value);

        void IValueWriter<DateTimeOffset>.WriteValue(DateTimeOffset value) => WriteDateTimeOffset(value);

        void IValueWriter<byte[]>.WriteValue(byte[] value) => WriteBinary(value);

        public byte[] ToBytes()
        {
            Flush();

            return hGCache.ToArray();
        }

        public long TargetedId => messagePackFormatter?.targeted_id ?? GlobalTargetedId;

        public int Count => -1;

        IEnumerable<string> IDataWriter<string>.Keys => null;

        IEnumerable<int> IDataWriter<int>.Keys => null;

        IEnumerable<Ps<Utf8Byte>> IDataWriter<Ps<Utf8Byte>>.Keys => null;

        public Type ContentType => null;

        public object Content
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public void OnWriteValue(string key, IValueReader valueReader)
        {
            this[key].DirectWrite(valueReader.DirectRead());
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            this[key].DirectWrite(valueReader.DirectRead());
        }

        public void OnWriteValue(Ps<Utf8Byte> key, IValueReader valueReader)
        {
            this[key].DirectWrite(valueReader.DirectRead());
        }

        public void OnWriteAll(IDataReader<string> dataReader)
        {
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
        }

        public void OnWriteAll(IDataReader<Ps<Utf8Byte>> dataReader)
        {
        }

        public void Initialize()
        {
        }

        public void Initialize(int capacity)
        {
        }

        public void MakeTargetedId() 
        {
        }

        void IValueWriter<bool[]>.WriteValue(bool[] value) => WriteArray(value);

        // void IValueWriter<byte[]>.WriteValue(byte[] value) => WriteArray(value);

        void IValueWriter<sbyte[]>.WriteValue(sbyte[] value) => WriteArray(value);

        void IValueWriter<short[]>.WriteValue(short[] value) => WriteArray(value);

        void IValueWriter<ushort[]>.WriteValue(ushort[] value) => WriteArray(value);

        void IValueWriter<char[]>.WriteValue(char[] value) => WriteArray(value);

        void IValueWriter<int[]>.WriteValue(int[] value) => WriteArray(value);

        void IValueWriter<uint[]>.WriteValue(uint[] value) => WriteArray(value);

        void IValueWriter<long[]>.WriteValue(long[] value) => WriteArray(value);

        void IValueWriter<ulong[]>.WriteValue(ulong[] value) => WriteArray(value);

        void IValueWriter<float[]>.WriteValue(float[] value) => WriteArray(value);

        void IValueWriter<double[]>.WriteValue(double[] value) => WriteArray(value);

        void IValueWriter<DateTime[]>.WriteValue(DateTime[] value) => WriteArray(value);

        void IValueWriter<DateTimeOffset[]>.WriteValue(DateTimeOffset[] value) => WriteArray(value);

        void IValueWriter<decimal[]>.WriteValue(decimal[] value) => WriteArray(value);

        void IValueWriter<Guid[]>.WriteValue(Guid[] value) => WriteArray(value);

        void IValueWriter<string[]>.WriteValue(string[] value) => WriteArray(value);

        void IValueWriter<List<bool>>.WriteValue(List<bool> value) => WriteList(value);

        void IValueWriter<List<byte>>.WriteValue(List<byte> value) => WriteList(value);

        void IValueWriter<List<sbyte>>.WriteValue(List<sbyte> value) => WriteList(value);

        void IValueWriter<List<short>>.WriteValue(List<short> value) => WriteList(value);

        void IValueWriter<List<ushort>>.WriteValue(List<ushort> value) => WriteList(value);

        void IValueWriter<List<char>>.WriteValue(List<char> value) => WriteList(value);

        void IValueWriter<List<int>>.WriteValue(List<int> value) => WriteList(value);

        void IValueWriter<List<uint>>.WriteValue(List<uint> value) => WriteList(value);

        void IValueWriter<List<long>>.WriteValue(List<long> value) => WriteList(value);

        void IValueWriter<List<ulong>>.WriteValue(List<ulong> value) => WriteList(value);

        void IValueWriter<List<float>>.WriteValue(List<float> value) => WriteList(value);

        void IValueWriter<List<double>>.WriteValue(List<double> value) => WriteList(value);

        void IValueWriter<List<DateTime>>.WriteValue(List<DateTime> value) => WriteList(value);

        void IValueWriter<List<DateTimeOffset>>.WriteValue(List<DateTimeOffset> value) => WriteList(value);

        void IValueWriter<List<decimal>>.WriteValue(List<decimal> value) => WriteList(value);

        void IValueWriter<List<Guid>>.WriteValue(List<Guid> value) => WriteList(value);

        void IValueWriter<List<string>>.WriteValue(List<string> value) => WriteList(value);

        void IValueWriter<MessagePackExtension>.WriteValue(MessagePackExtension value) => WriteExtension(value);

        sealed class MapWriter<TKey> : IDataWriter<TKey>, IValueFilter<TKey>
        {
            private readonly MessagePackSerializer<TMode> serializer;

            public MapWriter(MessagePackSerializer<TMode> serializer)
            {
                this.serializer = serializer;
            }

            public IValueWriter this[TKey key]
            {
                get
                {
                    ++serializer.fieldCount;

                    if (ValueInterface<TKey>.IsNotModified)
                    {
                        serializer.WriteValue(key);
                    }
                    else
                    {
                        ValueInterface<TKey>.WriteValue(serializer, key);
                    }

                    return serializer;
                }
            }

            public IEnumerable<TKey> Keys => null;

            public int Count => -1;

            public Type ContentType => throw new NotSupportedException();

            public object Content
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public bool Filter(ValueFilterInfo<TKey> valueInfo)
            {
                return serializer.Filter(valueInfo.ValueCopyer);
            }

            public void Initialize()
            {
            }

            public void Initialize(int capacity)
            {
            }

            public void OnWriteAll(IDataReader<TKey> dataReader)
            {
            }

            public void OnWriteValue(TKey key, IValueReader valueReader)
            {
                this[key].DirectWrite(valueReader.DirectRead());
            }
        }

        sealed class WriteMapInvoker : IGenericInvoker
        {
            readonly MessagePackSerializer<TMode> serializer;
            readonly IDataReader dataReader;

            public WriteMapInvoker(MessagePackSerializer<TMode> serializer, IDataReader dataReader)
            {
                this.serializer = serializer;
                this.dataReader = dataReader;
            }

            public void Invoke<TKey>()
            {
                serializer.WriteMap(Underlying.As<IDataReader<TKey>>(dataReader));
            }
        }
    }
}