using Swifter.Formatters;

using Swifter.RW;
using Swifter.Tools;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

using static Swifter.MessagePack.EncodingHelper;
using static Swifter.MessagePack.MessagePackCode;
using static Swifter.MessagePack.MessagePackModes;
using static Swifter.MessagePack.MessagePackFormatterOptions;


namespace Swifter.MessagePack
{
    sealed unsafe class MessagePackSerializer<TMode> :
        IFormatterWriter,
        IValueWriter<byte[]>,
        IValueWriter<Guid>,
        IValueWriter<DateTimeOffset>,
        IDataWriter<string>,
        IDataWriter<int>,
        IValueFilter<string>,
        IValueFilter<int>,
        IMapValueWriter
    {
        public readonly MessagePackForamtter MessagePackForamtter;
        public readonly HGlobalCache<byte> HGCache;
        public readonly Stream Stream;
        public readonly MessagePackFormatterOptions Options;
        public readonly int maxDepth;

        public TMode mode;

        public int offset; // 当前写入位置。
        public int depth; // 当前结构深度。

        public ref ReferenceCache<int> References => ref Unsafe.As<TMode, Reference>(ref mode).References;

        public MessagePackSerializer(
            MessagePackFormatterOptions options,
            int maxDepth,
            HGlobalCache<byte> hGlobal,
            MessagePackForamtter foramtter,
            Stream stream) : this(options, maxDepth, hGlobal)
        {
            MessagePackForamtter = foramtter;
            Stream = stream;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public MessagePackSerializer(
            MessagePackFormatterOptions options,
            int maxDepth,
            HGlobalCache<byte> hGCache
            )
        {
            this.Options = options;
            this.maxDepth = maxDepth;
            this.HGCache = hGCache;

            if (typeof(TMode) == typeof(Reference))
            {
                References = new ReferenceCache<int>();
            }
        }

        public long TargetedId => MessagePackForamtter?.id ?? 0;

        public IEnumerable<string> Keys => null;

        public int Count => 0;

        IEnumerable<int> IDataWriter<int>.Keys => null;

        public IValueWriter this[int key] => this;

        public IValueWriter this[string key]
        {
            get
            {
                WriteASCIIString(key);

                return this;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append(byte value)
        {
            HGCache.GetPointer()[offset] = value;

            ++offset;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append2(ushort value)
        {
            Append((byte)(value >> 8));
            Append((byte)value);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append4(uint value)
        {
            Append((byte)(value >> 24));
            Append((byte)(value >> 16));
            Append((byte)(value >> 8));
            Append((byte)value);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append8(ulong value)
        {
            Append((byte)(value >> 56));
            Append((byte)(value >> 48));
            Append((byte)(value >> 40));
            Append((byte)(value >> 32));
            Append((byte)(value >> 24));
            Append((byte)(value >> 16));
            Append((byte)(value >> 8));
            Append((byte)value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Append16(ref ulong value)
        {
            Append8(value);
            Append8(Unsafe.AddByteOffset(ref value, 8));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Expand(int expandMinSize)
        {
            if (HGCache.Capacity - offset < expandMinSize)
            {
                InternalExpand(expandMinSize);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool CheckDepth()
        {
            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();

                return false;
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ThrowOutOfDepthException()
        {
            if ((Options & MessagePackFormatterOptions.OutOfDepthException) != 0)
            {
                throw new OutOfDepthException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InternalExpand(int expandMinSize)
        {
            if (HGCache.Capacity == HGlobalCache<byte>.MaxSize && Stream != null && offset != 0)
            {
                HGCache.Count = offset;

                HGCache.WriteTo(Stream);

                offset = 0;

                Expand(expandMinSize);
            }
            else
            {
                HGCache.Expand(expandMinSize);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void DirectWrite(object value)
        {
            if (value == null)
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
                    case TypeCode.String:
                        WriteString((string)value);
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
                WriteValue(bytes);

                return;
            }


            if ((Options & MessagePackFormatterOptions.UnknownTypeAsString) != 0)
            {
                WriteString(value.ToString());

                return;
            }

            WriteUnknownTypeAsBinary(value);
        }

        public void WriteNull()
        {
            Expand(1);

            Append(Nil);
        }

        public void WriteReference(int address)
        {
            if (address < 0)
            {
                throw new NullReferenceException(nameof(address));
            }

            if (address <= byte.MaxValue)
            {
                WriteExtension(MessagePackExtensionCode.Reference, (byte)address);
            }
            else if (address <= ushort.MaxValue)
            {
                WriteExtension(MessagePackExtensionCode.Reference, (ushort)address);
            }
            else
            {
                WriteExtension(MessagePackExtensionCode.Reference, address);
            }
        }

        public bool TryWriteReference(IDataReader dataReader)
        {
            var token = dataReader.ReferenceToken;

            if (References.TryGetValue(token, out var address))
            {
                if ((Options & LoopReferencingException) != 0)
                {
                    throw new MessagePackLoopReferencingException();
                }
                else if ((Options & (LoopReferencingNull | MultiReferencingNull)) != 0)
                {
                    WriteNull();
                }
                else if((Options & MultiReferencingReference) != 0)
                {
                    WriteReference(address);
                }

                return true;
            }
            else
            {
                References.DirectAdd(token, offset);

                return false;
            }
        }

        public void ReferenceAfter(IDataReader dataReader)
        {
            if ((Options & (LoopReferencingException | LoopReferencingNull)) != 0)
            {
                References.Remove(dataReader.ReferenceToken);
            }
        }

        public void WriteArray(IDataReader<int> dataReader)
        {
            if (typeof(TMode) == typeof(Reference) && TryWriteReference(dataReader))
            {
                return;
            }

            if (CheckDepth())
            {
                WriteArrayHead(dataReader.Count);

                ++depth;

                if (typeof(TMode) == typeof(Simple))
                {
                    dataReader.OnReadAll(this);
                }
                else
                {
                    if ((Options & ArrayOnFilter) != 0 && (Options & (IgnoreNull | IgnoreZero | IgnoreEmptyString)) != 0)
                    {
                        dataReader.OnReadAll(this, this);
                    }
                    else
                    {
                        dataReader.OnReadAll(this);
                    }
                }

                --depth;
            }

            if (typeof(TMode) == typeof(Reference))
            {
                ReferenceAfter(dataReader);
            }
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

        public void WriteBoolean(bool value)
        {
            Expand(1);

            Append(value ? True : False);
        }

        public void WriteByte(byte value) => WriteUInt64(value);

        public void WriteChar(char value)
        {
            Expand(5);

            var bytesLength = GetUtf8Bytes(&value, 1, HGCache.GetPointer() + offset + 1);

            Append((byte)(FixStr | bytesLength));

            offset += bytesLength;
        }

        public void WriteExtension<T>(sbyte extensionCode, T value) where T : struct
        {
            var size = Unsafe.SizeOf<T>();

            switch (size)
            {
                case 1:
                    Append(FixExt1);
                    Append((byte)extensionCode);
                    Append(Unsafe.As<T, byte>(ref value));
                    return;
                case 2:
                    Append(FixExt2);
                    Append((byte)extensionCode);
                    Append2(Unsafe.As<T, ushort>(ref value));
                    return;
                case 4:
                    Append(FixExt4);
                    Append((byte)extensionCode);
                    Append4(Unsafe.As<T, uint>(ref value));
                    return;
                case 8:
                    Append(FixExt8);
                    Append((byte)extensionCode);
                    Append8(Unsafe.As<T, ulong>(ref value));
                    return;
                case 16:
                    Append(FixExt16);
                    Append((byte)extensionCode);
                    Append16(ref Unsafe.As<T, ulong>(ref value));
                    return;
            }

            Internal();

            void Internal()
            {
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

                Append((byte)extensionCode);

                ref var bytes = ref Unsafe.As<T, byte>(ref value);

                while (size >= 8)
                {
                    size -= 8;

                    Append8(Unsafe.As<byte, ulong>(ref Unsafe.AddByteOffset(ref bytes, size)));
                }

                if (size >= 4)
                {
                    size -= 4;

                    Append4(Unsafe.As<byte, uint>(ref Unsafe.AddByteOffset(ref bytes, size)));
                }

                if (size >= 2)
                {
                    size -= 2;

                    Append2(Unsafe.As<byte, ushort>(ref Unsafe.AddByteOffset(ref bytes, size)));
                }

                if (size >= 1)
                {
                    // --size;

                    Append(bytes);
                }
            }
        }

        public void WriteDateTime(DateTime value)
        {
            Expand(15);

            var ticks = value.Ticks - DateTimeHelper.TicksOfUTCDifference - DateTimeHelper.TicksOfUnixEpoch;

            var seconds = ticks / DateTimeHelper.TicksOfOneSecond;

            if (((Options & MessagePackFormatterOptions.UseTimestamp32) != 0 ||
                seconds * DateTimeHelper.TicksOfOneSecond == ticks) &&
                seconds <= UInt32MaxValue)
            {
                WriteExtension(MessagePackExtensionCode.Timestamp, (uint)seconds);

                return;
            }

            var nanoseconds = (ticks % DateTimeHelper.TicksOfOneSecond) * DateTimeHelper.NanosecondsOfTick;

            if (seconds <= UInt34MaxValue)
            {
                WriteExtension(MessagePackExtensionCode.Timestamp, (ulong)((nanoseconds << 34) | (seconds)));

                return;
            }

            WriteExtension(MessagePackExtensionCode.Timestamp, ((uint)nanoseconds, (ulong)seconds));
        }

        public void WriteDecimal(decimal value)
        {
            var scale = NumberHelper.GetScale(&value);

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

            if ((decimal)doubleValue == value)
            {
                WriteDouble(doubleValue);

                return;
            }

            if ((Options & MessagePackFormatterOptions.UnknownTypeAsString) != 0)
            {
                var chars = stackalloc char[NumberHelper.DecimalStringMaxLength];

                var charsLength = NumberHelper.ToString(value, chars);

                WriteASCIIString(chars, charsLength);

                return;
            }

            WriteUnknownTypeAsBinary(value);
        }

        public void WriteUnknownTypeAsBinary<T>(T value)
        {
            WriteBinary(ref Unsafe.As<T, byte>(ref value), Unsafe.SizeOf<T>());
        }

        public void WriteDouble(double value)
        {
            var int64Value = (long)value;

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
                Append4(Unsafe.As<float, uint>(ref singleValue));
            }
            else
            {
                Append(Float64);
                Append8(Unsafe.As<double, ulong>(ref value));
            }
        }

        public void WriteInt16(short value) => WriteInt64(value);

        public void WriteInt32(int value) => WriteInt64(value);

        public void WriteInt64(long value)
        {
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

                goto One;
            }
            else if (value >= Int16MinValue)
            {
                Append(MessagePackCode.Int16);

                goto Two;
            }
            else if (value >= Int32MinValue)
            {
                Append(MessagePackCode.Int32);

                goto Four;
            }
            else
            {
                Append(MessagePackCode.Int64);

                goto Eight;
            }


        Eight:
            Append((byte)(value >> 56));
            Append((byte)(value >> 48));
            Append((byte)(value >> 40));
            Append((byte)(value >> 32));

        Four:
            Append((byte)(value >> 24));
            Append((byte)(value >> 16));

        Two:
            Append((byte)(value >> 8));

        One:
            Append((byte)value);
        }

        public void WriteObject(IDataReader<string> dataReader)
        {
            if (typeof(TMode) == typeof(Reference) && TryWriteReference(dataReader))
            {
                return;
            }

            if (CheckDepth())
            {
                WriteMapHead(dataReader.Count);

                ++depth;

                if (typeof(TMode) == typeof(Simple))
                {
                    dataReader.OnReadAll(this);
                }
                else
                {
                    if ((Options & (IgnoreNull | IgnoreZero | IgnoreEmptyString)) != 0)
                    {
                        dataReader.OnReadAll(this, this);
                    }
                    else
                    {
                        dataReader.OnReadAll(this);
                    }
                }

                --depth;
            }

            if (typeof(TMode) == typeof(Reference))
            {
                ReferenceAfter(dataReader);
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
            Append4(Unsafe.As<float, uint>(ref value));
        }

        public void WriteString(string value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                fixed (char* chars = value)
                {
                    WriteString(chars, value.Length);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void WriteString(char* chars, int length)
        {
            Expand(5);

            var predictedOffset = GetStringHeadOffset(length);

            Expand(GetUtf8MaxBytesLength(length));

            var bytesLength = GetUtf8Bytes(chars, length, HGCache.GetPointer() + offset + predictedOffset);
            var actualOffset = GetStringHeadOffset(bytesLength);

            if (actualOffset != predictedOffset)
            {
                Unsafe.CopyBlockUnaligned(
                    HGCache.GetPointer() + offset + actualOffset, // destination
                    HGCache.GetPointer() + offset + predictedOffset, // source
                    (uint)bytesLength); // byteCount
            }

            WriteStringHead(bytesLength);

            offset += bytesLength;
        }

        public void WriteASCIIString(string value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
                fixed (char* chars = value)
                {
                    WriteASCIIString(chars, value.Length);
                }
            }
        }

        public void WriteASCIIString(char* chars, int length)
        {
            Expand(length + 5);

            var swap = offset;

            WriteStringHead(length);

            var destination = HGCache.GetPointer() + offset;

            for (int i = 0; i < length; i++)
            {
                var chr = chars[i];

                if (chr > ASCIIMaxChar) goto False;

                *destination = (byte)chr;

                ++destination;
            }

            offset += length;

            return;

        False:

            offset = swap;

            WriteString(chars, length);
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

        public void WriteUInt16(ushort value) => WriteUInt64(value);

        public void WriteUInt32(uint value) => WriteUInt64(value);

        public void WriteUInt64(ulong value)
        {
            Expand(9);

            if (value <= FixPositiveIntMaxValue)
            {
                goto One;
            }
            else if (value <= UInt8MaxValue)
            {
                Append(UInt8);

                goto One;
            }
            else if (value <= UInt16MaxValue)
            {
                Append(MessagePackCode.UInt16);

                goto Two;
            }
            else if (value <= UInt32MaxValue)
            {
                Append(MessagePackCode.UInt32);

                goto Four;
            }
            else
            {
                Append(MessagePackCode.UInt64);

                goto Eight;
            }


        Eight:
            Append((byte)(value >> 56));
            Append((byte)(value >> 48));
            Append((byte)(value >> 40));
            Append((byte)(value >> 32));

        Four:
            Append((byte)(value >> 24));
            Append((byte)(value >> 16));

        Two:
            Append((byte)(value >> 8));

        One:
            Append((byte)value);
        }

        public void WriteValue(byte[] value)
        {
            if (value == null)
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
            Expand(5 + length);

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

            Unsafe.CopyBlock(
                ref HGCache.GetPointer()[offset],
                ref firstByte,
                (uint)length);

            offset += length;
        }

        void IValueWriter<Guid>.WriteValue(Guid value) => WriteGuid(value);

        void IValueWriter<DateTimeOffset>.WriteValue(DateTimeOffset value) => WriteDateTimeOffset(value);

        public void WriteDateTimeOffset(DateTimeOffset value)
        {
            if ((Options & UnknownTypeAsString) != 0)
            {
                var chars = stackalloc char[DateTimeHelper.ISOStringMaxLength];

                var charsLength = DateTimeHelper.ToISOString(value, chars);

                WriteASCIIString(chars, charsLength);

                return;
            }

            WriteUnknownTypeAsBinary(value);
        }

        public void WriteGuid(Guid value)
        {
            if ((Options & UnknownTypeAsString) != 0)
            {
                var chars = stackalloc char[NumberHelper.GuidStringLength];

                var charsLength = NumberHelper.ToString(value, chars);

                WriteASCIIString(chars, charsLength);

                return;

            }

            WriteUnknownTypeAsBinary(value);
        }

        public void OnWriteValue(string key, IValueReader valueReader)
        {
            this[key].DirectWrite(valueReader.DirectRead());
        }

        public void OnWriteAll(IDataReader<string> dataReader)
        {
        }

        public void Initialize()
        {
        }

        public void Initialize(int capacity)
        {
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            this[key].DirectWrite(valueReader.DirectRead());
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
        }

        public void WriteMap<TKey>(IDataReader<TKey> mapReader)
        {
            if (typeof(TMode) == typeof(Reference) && TryWriteReference(mapReader))
            {
                return;
            }

            if (CheckDepth())
            {
                WriteMapHead(mapReader.Count);

                ++depth;

                var writer = new MapWriter<TKey>(this);

                if (typeof(TMode) == typeof(Simple))
                {
                    mapReader.OnReadAll(writer);
                }
                else 
                {
                    if ((Options & (IgnoreNull | IgnoreZero | IgnoreEmptyString)) != 0)
                    {
                        mapReader.OnReadAll(writer, writer);
                    }
                    else
                    {
                        mapReader.OnReadAll(writer);
                    }
                }

                --depth;
            }

            if (typeof(TMode) == typeof(Reference))
            {
                ReferenceAfter(mapReader);
            }
        }

        public void MakeTargetedId() { }

        public bool Filter(ValueCopyer valueCopyer)
        {
            var basicType = valueCopyer.TypeCode;

            if (typeof(TMode) == typeof(Reference) &&
                basicType == TypeCode.Object &&
                (Options & (MultiReferencingNull | LoopReferencingNull)) != 0 &&
                valueCopyer.Value is IDataReader dataReader &&
                References.TryGetValue(dataReader.ReferenceToken, out _))
            {
                valueCopyer.DirectWrite(null);

                basicType = TypeCode.Empty;
            }

            if ((Options & IgnoreNull) != 0 && basicType == TypeCode.Empty)
            {
                return false;
            }

            if ((Options & IgnoreZero) != 0)
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

            if ((Options & IgnoreEmptyString) != 0
                && basicType == TypeCode.String
                && valueCopyer.ReadString() == string.Empty)
            {
                return false;
            }

            return true;
        }

        bool IValueFilter<int>.Filter(ValueFilterInfo<int> valueInfo) => Filter(valueInfo.ValueCopyer);

        bool IValueFilter<string>.Filter(ValueFilterInfo<string> valueInfo) => Filter(valueInfo.ValueCopyer);

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
                    ValueInterface<TKey>.WriteValue(serializer, key);

                    return serializer;
                }
            }

            public IEnumerable<TKey> Keys => null;

            public int Count => 0;

            public bool Filter(ValueFilterInfo<TKey> valueInfo) => serializer.Filter(valueInfo.ValueCopyer);

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
    }
}