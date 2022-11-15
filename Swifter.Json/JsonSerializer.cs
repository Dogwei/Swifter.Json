using Swifter.Formatters;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

using static Swifter.Json.JsonCode;
using static Swifter.Json.JsonFormatter;
#if !NO_OPTIONS
using static Swifter.Json.JsonFormatterOptions;
#endif

namespace Swifter.Json
{
    /// <summary>
    /// Json 序列化 (写入) 器。
    /// </summary>
    public sealed unsafe class JsonSerializer :
        IFormatterWriter,
        IDataWriter<string>,
        IDataWriter<int>,
#if !NO_OPTIONS
        IValueFilter<string>,
        IValueFilter<int>,
#endif
        IRWPathNodeVisitor,
        IFastArrayValueWriter
    {
        /// <summary>
        /// Json 格式化器。
        /// </summary>
        public readonly JsonFormatter? JsonFormatter;

        /// <summary>
        /// 分段写入器 或 全局内存缓存。
        /// </summary>
        internal readonly object segmenterOrHGCache;

        /// <summary>
        /// 最大深度限制。
        /// </summary>
        public readonly int MaxDepth;

#if !NO_OPTIONS

        readonly object? mode;

        /// <summary>
        /// 当前配置项。
        /// </summary>
        public readonly JsonFormatterOptions Options;
#endif

        char* begin;
        char* end;

        char* current;

        int depth;

        /// <summary>
        /// 获取已写入的字符数。
        /// </summary>
        public int Offset
            => (int)(current - begin);

        /// <summary>
        /// 获取缓存剩余字符数。
        /// </summary>
        int Rest
            => (int)(end - current);

        /// <summary>
        /// 全局内存缓存。
        /// </summary>
        public HGlobalCache<char> HGCache =>
            segmenterOrHGCache is JsonSegmentedContent segmenter ?
            segmenter.hGCache :
            Unsafe.As<HGlobalCache<char>>(segmenterOrHGCache);

#if !NO_OPTIONS

        ComplexMode ComplexModeInstance
            => Unsafe.As<ComplexMode>(mode!);

        ReferenceMode ReferenceModeInstance
            => Unsafe.As<ReferenceMode>(mode!);

        bool IsSimpleMode
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => (Options & (ComplexOptions | ReferenceOptions)) is 0;
        }

        bool IsFilterMode
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => Options >= OnFilter;
        }

#endif

        #region -- 构造函数 --

        /// <summary>
        /// 初始化默认配置的 Json 序列号 (写入) 器。
        /// </summary>
        /// <param name="hGCache">全局内存缓存</param>
        public JsonSerializer(HGlobalCache<char> hGCache)
        {
            segmenterOrHGCache = hGCache;
            MaxDepth = DefaultMaxDepth;

#if !NO_OPTIONS
            Options = JsonFormatterOptions.Default;
#endif

            begin = hGCache.First;
            end = hGCache.Last;

            current = begin;
        }

        /// <summary>
        /// 初始化指定格式化器的 Json 序列号 (写入) 器。
        /// </summary>
        /// <param name="jsonFormatter">指定格式化器</param>
        /// <param name="hGCache">全局内存缓存</param>
        public JsonSerializer(JsonFormatter jsonFormatter, HGlobalCache<char> hGCache)
            : this(hGCache
#if !NO_OPTIONS
                  , jsonFormatter.Options
#endif
                  )
        {
            JsonFormatter = jsonFormatter;
            MaxDepth = jsonFormatter.MaxDepth;
        }

#if !NO_OPTIONS

        /// <summary>
        /// 初始化指定配置的 Json 序列号 (写入) 器。
        /// </summary>
        /// <param name="hGCache">全局内存缓存</param>
        /// <param name="options">指定配置</param>
        public JsonSerializer(HGlobalCache<char> hGCache, JsonFormatterOptions options)
            : this(hGCache)
        {
            Options = options;

            if (On(ReferenceOptions))
            {
                mode = new ReferenceMode(this);
            }
            else if (On(ComplexOptions))
            {
                mode = new ComplexMode(this);
            }
        }

        /// <summary>
        /// 初始化指定配置的 Json 序列号 (写入) 器。
        /// </summary>
        /// <param name="segmenter">分段读取器</param>
        /// <param name="options">指定配置</param>
        public JsonSerializer(JsonSegmentedContent segmenter, JsonFormatterOptions options) : this(segmenter.hGCache, options)
        {
            segmenterOrHGCache = segmenter;
        }
#endif


        /// <summary>
        /// 初始化默认配置的 Json 序列号 (写入) 器。
        /// </summary>
        /// <param name="segmenter">分段读取器</param>
        public JsonSerializer(JsonSegmentedContent segmenter) : this(segmenter.hGCache)
        {
            segmenterOrHGCache = segmenter;
        }

        /// <summary>
        /// 初始化指定格式化器的 Json 序列号 (写入) 器。
        /// </summary>
        /// <param name="jsonFormatter">指定格式化器</param>
        /// <param name="segmenter">分段读取器</param>
        public JsonSerializer(JsonFormatter jsonFormatter, JsonSegmentedContent segmenter)
            : this(jsonFormatter, segmenter.hGCache)
        {
            segmenterOrHGCache = segmenter;
        }

        #endregion

        #region -- 辅助函数 --

        /// <summary>
        /// 将已写入的 JSON 内容长度设置到 HGCache 的内容数量中。
        /// </summary>
        public void Flush()
        {
            if (Offset is 0)
            {
                return;
            }

            var isValueEnding = current[-1] is ValueEnding;

            if (segmenterOrHGCache is JsonSegmentedContent segmenter)
            {
                segmenter.hGCache.Count = Offset;

                if (isValueEnding)
                {
                    --segmenter.hGCache.Count;
                }

                segmenter.WriteSegment();

                segmenter.hGCache.Count = 0;

                InternalClear();

                if (isValueEnding)
                {
                    Append(ValueEnding);

                    ++segmenter.hGCache.Count;
                }
            }
            else
            {
                var hGCache = Unsafe.As<HGlobalCache<char>>(segmenterOrHGCache);

                hGCache.Count = (int)(current - hGCache.First);

                if (isValueEnding)
                {
                    --hGCache.Count;
                }
            }
        }

        /// <summary>
        /// 重置 JSON 写入位置。
        /// </summary>
        public void Clear()
        {
            ThrowIfSegmented();

            InternalClear();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void InternalClear()
        {
            begin = HGCache.First;
            end = HGCache.Last;

            current = begin;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void Append(char c)
        {
            *current = c; ++current;
        }

#if !NO_OPTIONS

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        bool On(JsonFormatterOptions options)
             => (Options & options) != 0;
#endif

        /// <summary>
        /// 移动偏移量。
        /// </summary>
        /// <param name="moveSize">向后移动的距离，可以是负数。</param>
        /// <exception cref="IndexOutOfRangeException">移动后超出了缓存区区域</exception>
        public void MoveOffset(int moveSize)
        {
            var newCurrent = current + moveSize;

            if (newCurrent < begin || newCurrent > end)
            {
                throw new IndexOutOfRangeException();
            }

            current = newCurrent;
        }

        /// <summary>
        /// 扩容。
        /// </summary>
        /// <param name="growMinSize">表示缓存区至少需要的内存空间</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Grow(int growMinSize)
        {
            if (growMinSize > end - current)
            {
                InternalGrow(growMinSize);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void InternalGrow(int growMinSize)
        {
            HGlobalCache<char> hGCache;

            if (segmenterOrHGCache is JsonSegmentedContent segmenter)
            {
                hGCache = segmenter.hGCache;

                if (current != hGCache.First)
                {
                    hGCache.Count = (int)(current - hGCache.First);

                    segmenter.WriteSegment();

                    hGCache.Count = 0;

                    begin = hGCache.First;
                    end = hGCache.Last;

                    current = begin;
                }
            }
            else
            {
                hGCache = Unsafe.As<HGlobalCache<char>>(segmenterOrHGCache);
            }

            if (current + growMinSize > hGCache.Last)
            {
                var offset = (int)(current - hGCache.First);

                hGCache.Grow(growMinSize);

                begin = hGCache.First;
                end = hGCache.Last;

                current = begin + offset;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void ThrowOutOfDepthException()
        {
#if !NO_OPTIONS
            if (On(OutOfDepthException))
#endif
            {
                throw new JsonOutOfDepthException();
            }
        }

        void ThrowIfSegmented()
        {
            if (segmenterOrHGCache is JsonSegmentedContent)
            {
                throw new NotSupportedException("The method is not supported for serializers that already support segmented writing.");
            }
        }

        /// <summary>
        /// 在写入值之前写入。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteValueBefore()
        {
#if !NO_OPTIONS
            if (On(Indented))
            {
                NoInliningWriteValueBefore();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            void NoInliningWriteValueBefore()
            {
                if (current > begin)
                {
                    switch (current[-1])
                    {
                        case FixArray:
                        case ValueEnding:

                            WriteRaw(ComplexModeInstance.LineBreakChars);

                            for (int i = depth; i > 0; --i)
                            {
                                WriteRaw(ComplexModeInstance.IndentedChars);
                            }

                            break;
                    }
                }
            }
#endif
        }

        /// <summary>
        /// 在写入值之后写入。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteValueAfter()
        {
            Append(ValueEnding);
        }

        /// <summary>
        /// 在写入键之前写入。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteKeyBefore()
        {
#if !NO_OPTIONS
            if (On(Indented))
            {
                NoInliningAppendKeyBefore();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            void NoInliningAppendKeyBefore()
            {
                WriteRaw(ComplexModeInstance.LineBreakChars);

                for (int i = depth; i > 0; --i)
                {
                    WriteRaw(ComplexModeInstance.IndentedChars);
                }
            }
#endif
        }

        /// <summary>
        /// 在写入键之后写入。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteKeyAfter()
        {
            Append(KeyEnding);

            WriteMiddleChars();
        }

        /// <summary>
        /// 在写入对象或数组前写入。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteStructBefore()
        {
        }

        /// <summary>
        /// 在写入对象或数组后写入。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteStructAfter()
        {
#if !NO_OPTIONS
            if (On(Indented))
            {
                NoInliningAppendStructAfter();
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            void NoInliningAppendStructAfter()
            {
                WriteRaw(ComplexModeInstance.LineBreakChars);

                for (int i = depth; i > 0; --i)
                {
                    WriteRaw(ComplexModeInstance.IndentedChars);
                }
            }
#endif
        }

        /// <summary>
        /// 在写入键后且写入值之前写入。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteMiddleChars()
        {
#if !NO_OPTIONS
            if (On(Indented))
            {
                WriteRaw(ComplexModeInstance.MiddleChars);
            }
#endif
        }

#if !NO_OPTIONS
        bool Filter(ValueCopyer valueCopyer)
        {
            var basicType = valueCopyer.TypeCode;

            if (On(ReferenceOptions))
            {
                if (basicType is RWTypeCode.Object or RWTypeCode.Array
                    && On(MultiReferencingNull | LoopReferencingNull)
                    && valueCopyer.InternalValue is IDataReader dataReader
                    && dataReader.ContentType?.IsValueType == false
                    && dataReader.Content is object content
                    && ReferenceModeInstance.GetReference(content) is RWPath reference
                    && (On(MultiReferencingNull) || ReferenceModeInstance.ReferenceCompare(reference) is 1))
                {
                    goto AsNull;
                }

                if (basicType is RWTypeCode.String
                    && On(MultiReferencingNull)
                    && On(MultiReferencingAlsoString)
                    && valueCopyer.InternalValue is string str
                    && ReferenceModeInstance.GetReference(str) is not null)
                {
                    goto AsNull;
                }

                goto Break;

            AsNull:

                valueCopyer.DirectWrite(null);

                basicType = RWTypeCode.Null;

            Break:;

            }

            if (basicType is RWTypeCode.Null && On(IgnoreNull))
            {
                return false;
            }

            if (On(IgnoreZero))
            {
                switch (basicType)
                {
                    case RWTypeCode.SByte:
                        return valueCopyer.ReadSByte() != 0;
                    case RWTypeCode.Int16:
                        return valueCopyer.ReadInt16() != 0;
                    case RWTypeCode.Int32:
                        return valueCopyer.ReadInt32() != 0;
                    case RWTypeCode.Int64:
                        return valueCopyer.ReadInt64() != 0;
                    case RWTypeCode.Byte:
                        return valueCopyer.ReadByte() != 0;
                    case RWTypeCode.UInt16:
                        return valueCopyer.ReadUInt16() != 0;
                    case RWTypeCode.UInt32:
                        return valueCopyer.ReadUInt32() != 0;
                    case RWTypeCode.UInt64:
                        return valueCopyer.ReadUInt64() != 0;
                    case RWTypeCode.Single:
                        return valueCopyer.ReadSingle() != 0;
                    case RWTypeCode.Double:
                        return valueCopyer.ReadDouble() != 0;
                    case RWTypeCode.Decimal:
                        return valueCopyer.ReadDecimal() != 0;
                }
            }

            if (On(IgnoreEmptyString) &&
                basicType is RWTypeCode.String &&
                valueCopyer.ReadString() == string.Empty)
            {
                return false;
            }

            return true;
        }

#endif

        /// <summary>
        /// 将当前内容转换为字符串。
        /// </summary>
        /// <exception cref="NotSupportedException">指定了写入段落回调</exception>
        public override string ToString()
        {
            ThrowIfSegmented();

            Flush();

            return HGCache.ToStringEx();
        }

        #endregion

        #region -- 公共值 --

        /// <summary>
        /// 写入元内容。
        /// </summary>
        /// <param name="rawContentLength">元内容长度</param>
        /// <returns>返回用于写入元内容的内存</returns>
        public Ps<char> WriteRaw(int rawContentLength)
        {
            Grow(rawContentLength);

            var rawSpan = new Ps<char>(current, rawContentLength);

            current += rawContentLength;

            return rawSpan;
        }

        /// <summary>
        /// 写入元内容。
        /// </summary>
        /// <param name="value">元内容</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteRaw(string value)
        {
            var length = value.Length;

            Grow(length + 2);

            Unsafe.CopyBlock(
                ref Unsafe.As<char, byte>(ref *current),
                ref Unsafe.As<char, byte>(ref StringHelper.GetRawStringData(value)),
                (uint)(length * sizeof(char)));

            current += length;
        }

        /// <summary>
        /// 写入元内容。
        /// </summary>
        /// <param name="value">元内容</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void InternalWriteRaw(Ps<char> value)
        {
            Grow(value.Length + 2);

            Unsafe.CopyBlock(
                current,
                value.Pointer,
                (uint)(value.Length * sizeof(char)));

            current += value.Length;
        }

        /// <summary>
        /// 写入元内容。
        /// </summary>
        /// <param name="firstChar">元内容首个字符引用</param>
        /// <param name="length">元内容长度</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void InternalWriteRaw(ref char firstChar, int length)
        {
            Grow(length + 2);

            Unsafe.CopyBlock(
                ref Unsafe.As<char, byte>(ref *current),
                ref Unsafe.As<char, byte>(ref firstChar),
                (uint)(length * sizeof(char)));

            current += length;
        }

        /// <summary>
        /// 写入一个 <see langword="null"/> 值。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteNull()
        {
            WriteValueBefore();

            Grow(6);

            //Append(nullString);

            // 考虑到 Null 值在 .Net 程序中非常常见，所以这里使用了比较极端的优化。
            *(ulong*)current = BitConverter.IsLittleEndian
                ? nullBits
                : nullBitsBigEndian;

            current += 4;

            WriteValueAfter();
        }

        /// <summary>
        /// 直接写入一个基础类型的值。
        /// </summary>
        /// <remarks>
        /// 注意: 此方法不支持写入复杂类型 (如: 对象类型或数组类型) 的值。
        /// </remarks>
        public void DirectWrite(object? value)
        {
            if (value is null)
            {
                WriteNull();

                return;
            }

#if !NO_OPTIONS
            if (On(MultiReferencingAlsoString) && On(MultiReferencingReference | MultiReferencingNull) && CheckObjectReference(value))
            {
                return;
            }
#endif

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.DBNull:
                    WriteNull();
                    return;
                case TypeCode.Boolean:
                    WriteBoolean(TypeHelper.Unbox<bool>(value));
                    return;
                case TypeCode.Char:
                    WriteChar(TypeHelper.Unbox<char>(value));
                    return;
                case TypeCode.SByte:
                    WriteSByte(TypeHelper.Unbox<sbyte>(value));
                    return;
                case TypeCode.Byte:
                    WriteByte(TypeHelper.Unbox<byte>(value));
                    return;
                case TypeCode.Int16:
                    WriteInt16(TypeHelper.Unbox<short>(value));
                    return;
                case TypeCode.UInt16:
                    WriteUInt16(TypeHelper.Unbox<ushort>(value));
                    return;
                case TypeCode.Int32:
                    WriteInt32(TypeHelper.Unbox<int>(value));
                    return;
                case TypeCode.UInt32:
                    WriteUInt32(TypeHelper.Unbox<uint>(value));
                    return;
                case TypeCode.Int64:
                    WriteInt64(TypeHelper.Unbox<long>(value));
                    return;
                case TypeCode.UInt64:
                    WriteUInt64(TypeHelper.Unbox<ulong>(value));
                    return;
                case TypeCode.Single:
                    WriteSingle(TypeHelper.Unbox<float>(value));
                    return;
                case TypeCode.Double:
                    WriteDouble(TypeHelper.Unbox<double>(value));
                    return;
                case TypeCode.Decimal:
                    WriteDecimal(TypeHelper.Unbox<decimal>(value));
                    return;
                case TypeCode.DateTime:
                    WriteDateTime(TypeHelper.Unbox<DateTime>(value));
                    return;
                case TypeCode.String:
                    goto String;
            }

            switch (value)
            {
                case Guid:
                    WriteGuid(TypeHelper.Unbox<Guid>(value));
                    return;
                case DateTimeOffset:
                    WriteDateTimeOffset(TypeHelper.Unbox<DateTimeOffset>(value));
                    return;
            }

            value = value.ToString();

            if (value is null)
            {
                WriteNull();

                return;
            }

        String:

            WriteValueBefore();

            InternalWriteString(Unsafe.As<string>(value));

            WriteValueAfter();

            return;
        }

        /// <summary>
        /// 写入一个已知类型的值。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteValue<T>(T? value)
        {
            if (typeof(T) == typeof(int)) WriteInt32(As<int>(value));
            else if (typeof(T) == typeof(string)) WriteString(As<string>(value));
            else if (typeof(T) == typeof(double)) WriteDouble(As<double>(value));
            else if (typeof(T) == typeof(bool)) WriteBoolean(As<bool>(value));
            else if (typeof(T) == typeof(byte)) WriteByte(As<byte>(value));
            else if (typeof(T) == typeof(sbyte)) WriteSByte(As<sbyte>(value));
            else if (typeof(T) == typeof(short)) WriteInt16(As<short>(value));
            else if (typeof(T) == typeof(ushort)) WriteUInt16(As<ushort>(value));
            else if (typeof(T) == typeof(uint)) WriteUInt32(As<uint>(value));
            else if (typeof(T) == typeof(long)) WriteInt64(As<long>(value));
            else if (typeof(T) == typeof(ulong)) WriteUInt64(As<ulong>(value));
            else if (typeof(T) == typeof(float)) WriteSingle(As<float>(value));
            else if (typeof(T) == typeof(char)) WriteChar(As<char>(value));
            else if (typeof(T) == typeof(decimal)) WriteDecimal(As<decimal>(value));
            else if (typeof(T) == typeof(DateTime)) WriteDateTime(As<DateTime>(value));
            else if (typeof(T) == typeof(Guid)) WriteGuid(As<Guid>(value));
            else if (typeof(T) == typeof(DateTimeOffset)) WriteDateTimeOffset(As<DateTimeOffset>(value));
            else if (typeof(T) == typeof(TimeSpan)) WriteTimeSpan(As<TimeSpan>(value));
            else ValueInterface.WriteValue(this, value);

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            static TOutput As<TOutput>(T? value)
                => Unsafe.As<T?, TOutput>(ref value);
        }

        /// <summary>
        /// 写入一个枚举值。
        /// </summary>
        public void WriteEnum<T>(T value) where T : struct, Enum
        {
            Grow(128);

            WriteValueBefore();

            if (EnumHelper.IsFlagsEnum<T>() && EnumHelper.AsUInt64(value) != 0)
            {
                // TODO: 枚举值不应该出现 '"' '\n' '\r' 等违法字符；如果存在违法字符串，这里序列化的 Json 将格式错误。
                // TODO: 通常情况下，hGCache 剩余的空间完全足够枚举的 Flags 字符串，这里如果超出了，将使用整数格式。

                if (EnumHelper.AsUInt64(
                    EnumHelper.FormatEnumFlags(value, current + 1, Rest, out var written)
                    ) == 0)
                {
                    Append(FixString);

                    current += written;

                    Append(FixString);

                    goto After;
                }
            }
            else if (EnumHelper.GetEnumName(value) is string name)
            {
                InternalWriteString(name);

                goto After;
            }

            switch (EnumHelper.GetEnumTypeCode<T>())
            {
                case TypeCode.SByte:
                    InternalWriteInt64(Unsafe.As<T, sbyte>(ref value));
                    break;
                case TypeCode.Int16:
                    InternalWriteInt64(Unsafe.As<T, short>(ref value));
                    break;
                case TypeCode.Int32:
                    InternalWriteInt64(Unsafe.As<T, int>(ref value));
                    break;
                case TypeCode.Int64:
                    InternalWriteInt64(Unsafe.As<T, long>(ref value));
                    break;
                default:
                    InternalWriteUInt64(EnumHelper.AsUInt64(value));
                    break;
            }

        After:

            WriteValueAfter();
        }

        /// <summary>
        /// 写入一个路径。
        /// </summary>
        public void WritePath(RWPath path)
        {
            WriteValueBefore();

            Grow(2);

            Append(FixString);

            InternalWritePath(path);

            Grow(3);

            Append(FixString);

            WriteValueAfter();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void InternalWritePath(RWPath reference)
        {
            Grow(2);

            WriteRaw(ReferenceRootPathName);

            for (var node = reference.Nodes.FirstNode; node != null; node = node.Next)
            {
                Grow(2);

                Append(ReferenceSeparator);

                node.Value.Accept(this);
            }
        }

        #endregion

        #region -- 数值 --

        /// <summary>
        /// 写入一个 <see cref="Boolean"/> 值。
        /// </summary>
        public void WriteBoolean(bool value)
        {
            WriteValueBefore();

            Grow(6);

            if (value)
            {
                Append('t');
                Append('r');
                Append('u');
                Append('e');
            }
            else
            {
                Append('f');
                Append('a');
                Append('l');
                Append('s');
                Append('e');
            }

            WriteValueAfter();
        }

        /// <summary>
        /// 写入一个 <see cref="Byte"/> 值。
        /// </summary>
        public void WriteByte(byte value)
            => WriteUInt64(value);

        /// <summary>
        /// 写入一个 <see cref="Char"/> 值。
        /// </summary>
        public void WriteChar(char value)
        {
            WriteValueBefore();

            InternalWriteStringCore(ref value, 1);

            WriteValueAfter();
        }

        /// <summary>
        /// 写入一个 <see cref="DateTime"/> 值。
        /// </summary>
        public void WriteDateTime(DateTime value)
        {
            WriteValueBefore();

            Grow(DateTimeHelper.ISOStringMaxLength + 4);

            Append(FixString);

            current += DateTimeHelper.ToISOString(value, current);

            Append(FixString);

            WriteValueAfter();
        }

        /// <summary>
        /// 写入一个 <see cref="Decimal"/> 值。
        /// </summary>
        public void WriteDecimal(decimal value)
        {
            WriteValueBefore();

            Grow(NumberHelper.DecimalStringMaxLength);

            current += NumberHelper.ToString(value, current);

            WriteValueAfter();
        }

        /// <summary>
        /// 写入一个 <see cref="Double"/> 值。
        /// </summary>
        public void WriteDouble(double value)
        {
            WriteValueBefore();

            if (NumberHelper.IsSpecialValue(value))
            {
                InternalWriteDoubleString(value);
            }
            else
            {
                Grow(NumberHelper.DecimalMaxDoubleStringLength);

#if !NO_OPTIONS
                if (On(UseSystemFloatingPointsMethods))
                {
#if NativeSpan
                    value.TryFormat(new Span<char>(current, Rest), out var charsWritter); // 在空间足够，此处一定成功。

                    current += charsWritter;
#else
                    WriteRaw(value.ToString());
#endif
                }
                else
#endif
                {
                    current += NumberHelper.Decimal.ToString(value, current);
                }
            }

            WriteValueAfter();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void InternalWriteDoubleString(double value)
        {
            // NaN, PositiveInfinity, NegativeInfinity, Or Other...

            // TODO: Fast
            InternalWriteString(value.ToString());
        }

        /// <summary>
        /// 写入一个 <see cref="Int16"/> 值。
        /// </summary>
        public void WriteInt16(short value)
            => WriteInt64(value);

        /// <summary>
        /// 写入一个 <see cref="Int32"/> 值。
        /// </summary>
        public void WriteInt32(int value)
            => WriteInt64(value);

        /// <summary>
        /// 写入一个 <see cref="Int64"/> 值。
        /// </summary>
        public void WriteInt64(long value)
        {
            WriteValueBefore();

            InternalWriteInt64(value);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void InternalWriteInt64(long value)
        {
            Grow(NumberHelper.DecimalMaxInt64NumbersLength);

            current += NumberHelper.ToDecimalString(value, current);
        }

        /// <summary>
        /// 写入一个 <see cref="SByte"/> 值。
        /// </summary>
        public void WriteSByte(sbyte value)
            => WriteInt64(value);

        /// <summary>
        /// 写入一个 <see cref="Single"/> 值。
        /// </summary>
        public void WriteSingle(float value)
        {
            WriteValueBefore();

            if (NumberHelper.IsSpecialValue(value))
            {
                InternalWriteDoubleString(value);
            }
            else
            {
                Grow(NumberHelper.DecimalMaxSingleStringLength);

#if !NO_OPTIONS
                if (On(UseSystemFloatingPointsMethods))
                {
#if NativeSpan
                    value.TryFormat(new Span<char>(current, Rest), out var charsWritter); // 在空间足够，此处一定成功。

                    current += charsWritter;
#else
                    WriteRaw(value.ToString());
#endif
                }
                else
#endif
                {
                    current += NumberHelper.Decimal.ToString(value, current);
                }
            }

            WriteValueAfter();
        }
        /// <summary>
        /// 写入一个 <see cref="UInt16"/> 值。
        /// </summary>
        public void WriteUInt16(ushort value)
            => WriteUInt64(value);

        /// <summary>
        /// 写入一个 <see cref="UInt32"/> 值。
        /// </summary>
        public void WriteUInt32(uint value)
            => WriteUInt64(value);

        /// <summary>
        /// 写入一个 <see cref="UInt64"/> 值。
        /// </summary>
        public void WriteUInt64(ulong value)
        {
            WriteValueBefore();

            InternalWriteUInt64(value);

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void InternalWriteUInt64(ulong value)
        {
            Grow(NumberHelper.DecimalMaxUInt64NumbersLength);

            current += NumberHelper.ToDecimalString(value, current);
        }

        /// <summary>
        /// 写入一个 <see cref="Guid"/> 值。
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="withSeparators">是否包含分隔符</param>
        public void WriteGuid(Guid value, bool withSeparators = true)
        {
            WriteValueBefore();

            Grow(GuidHelper.GuidStringWithSeparatorsLength + 4);

            Append(FixString);

            current += GuidHelper.ToString(value, current, withSeparators);

            Append(FixString);

            WriteValueAfter();
        }

        /// <summary>
        /// 写入一个 <see cref="Guid"/> 值。
        /// </summary>
        public void WriteGuid(Guid value)
        {
            WriteGuid(value, true);
        }

        /// <summary>
        /// 写入一个 <see cref="DateTimeOffset"/> 值。
        /// </summary>
        public void WriteDateTimeOffset(DateTimeOffset value)
        {
            WriteValueBefore();

            Grow(DateTimeHelper.ISOStringMaxLength + 4);

            Append(FixString);

            current += DateTimeHelper.ToISOString(value, current);

            Append(FixString);

            WriteValueAfter();
        }

        /// <summary>
        /// 写入一个 <see cref="TimeSpan"/> 值。
        /// </summary>
        public void WriteTimeSpan(TimeSpan value)
        {
            WriteValueBefore();

            Grow(DateTimeHelper.ISOStringMaxLength + 4);

            Append(FixString);

            current += DateTimeHelper.ToISOString(value, current);

            Append(FixString);

            WriteValueAfter();
        }

        #endregion

        #region -- 结构 --

        /// <summary>
        /// 写入一个数组。
        /// </summary>
        public void WriteArray(IDataReader<int> dataReader)
        {
#if !NO_OPTIONS
            if (On(ReferenceOptions) && dataReader.ContentType?.IsValueType == false && dataReader.Content is object content && CheckObjectReference(content))
            {
                return;
            }
#endif
            if (dataReader is IFastArrayRW fastArrayRW && dataReader.ValueType is not null /* 如果是未知的值的类型则不支持快速写入 */)
            {
                fastArrayRW.WriteTo(this);

                return;
            }

            WriteValueBefore();

            Grow(2);

            Append(FixArray);

            WriteStructBefore();

            if (depth >= MaxDepth)
            {
                ThrowOutOfDepthException();
            }
            else
            {
                var tCurrent = current;

                ++depth;

#if !NO_OPTIONS
                if (!IsSimpleMode)
                {
                    if (On(ReferenceOptions))
                    {
                        ReferenceModeInstance.EnterReferenceScope();

                        if (On(MultiReferencingOptimizeLayout))
                        {
                            if (dataReader.Count > 0)
                            {
                                dataReader.OnReadAll(ReferenceModeInstance.ArrayReferenceWriter);
                            }
                        }
                    }

                    if (IsFilterMode && On(ArrayOnFilter))
                    {
                        dataReader.OnReadAll(new DataFilterWriter<int>(this, this, dataReader));
                    }
                    else
                    {
                        dataReader.OnReadAll(this);
                    }

                    if (On(ReferenceOptions))
                    {
                        ReferenceModeInstance.LeaveReferenceScope();
                    }
                }
                else
#endif
                {
                    dataReader.OnReadAll(this);
                }

                --depth;

                Grow(2);

                if (tCurrent != current)
                {
                    --current;
                }
            }

            WriteStructAfter();

            Append(ArrayEnding);

            WriteValueAfter();
        }

        /// <summary>
        /// 写入一个数组。
        /// </summary>
        public void WriteArray<T>(T?[]? array)
        {
            if (array is null)
            {
                WriteNull();

                return;
            }

#if !NO_OPTIONS
            if (On(ReferenceOptions) && CheckObjectReference(array))
            {
                return;
            }
#endif

            if (array.Length > 0)
            {
                WriteArray(ref array[0], array.Length);
            }
            else
            {
                WriteArray(ref Unsafe.AsRef<T?>(null), 0);
            }
        }

        /// <summary>
        /// 写入一个列表。
        /// </summary>
        public void WriteList<T>(List<T?>? list)
        {
            if (list is null)
            {
                WriteNull();

                return;
            }
#if !NO_OPTIONS
            if (On(ReferenceOptions) && CheckObjectReference(list))
            {
                return;
            }
#endif

            var rawData = ArrayHelper.GetRawData(list);
            var count = list.Count;

            if (rawData != null && rawData.Length > 0 && count > 0)
            {
                WriteArray(ref rawData[0], count);
            }
            else
            {
                WriteArray(ref Unsafe.AsRef<T?>(null), 0);
            }
        }

        /// <summary>
        /// 写入一个数组。
        /// </summary>
        /// <param name="firstElement">第一个元素引用</param>
        /// <param name="length">长度</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteArray<T>(ref T? firstElement, int length)
        {
            WriteValueBefore();

            Grow(2);

            Append(FixArray);

            WriteStructBefore();

            if (depth >= MaxDepth)
            {
                ThrowOutOfDepthException();
            }
            else
            {
                var tCurrent = current;

                ++depth;

#if !NO_OPTIONS
                if (!IsSimpleMode && IsFilterMode && On(ArrayOnFilter))
                {
                    var filterInfo = new ValueFilterInfo<int>();

                    for (int i = 0; i < length; i++)
                    {
                        filterInfo.Key = i;

                        ValueInterface.WriteValue(filterInfo.ValueCopyer, Unsafe.Add(ref firstElement, i));

                        if (((IValueFilter<int>)this).Filter(filterInfo))
                        {
                            filterInfo.ValueCopyer.WriteTo(this);
                        }
                    }
                }
                else
#endif
                if (ValueInterface<T>.IsFastObjectInterface)
                {
                    var fastObjectRW = FastObjectRW<T>.Create();

                    for (int i = 0; i < length; i++)
                    {
                        fastObjectRW.content = Unsafe.Add(ref firstElement, i);

                        WriteObject(fastObjectRW);
                    }
                }
                else if (ValueInterface<T>.IsNotModified)
                {
                    for (int i = 0; i < length; i++)
                    {
                        WriteValue(Unsafe.Add(ref firstElement, i));
                    }
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        ValueInterface.WriteValue(this, Unsafe.Add(ref firstElement, i));
                    }
                }

                --depth;

                Grow(2);

                if (tCurrent != current)
                {
                    --current;
                }
            }

            WriteStructAfter();

            Append(ArrayEnding);

            WriteValueAfter();
        }

        /// <summary>
        /// 写入一个对象。
        /// </summary>
        public void WriteObject(IDataReader<string> dataReader)
        {
#if !NO_OPTIONS
            if (On(ReferenceOptions) && dataReader.ContentType?.IsValueType == false && dataReader.Content is object content && CheckObjectReference(content))
            {
                return;
            }
#endif

            WriteValueBefore();

            Grow(2);

            Append(FixObject);

            WriteStructBefore();

            if (depth >= MaxDepth)
            {
                ThrowOutOfDepthException();
            }
            else
            {
                var tCurrent = current;

                ++depth;

#if !NO_OPTIONS
                if (!IsSimpleMode)
                {
                    if (On(ReferenceOptions))
                    {
                        ReferenceModeInstance.EnterReferenceScope();

                        if (On(MultiReferencingOptimizeLayout))
                        {
                            if (dataReader.Count > 0)
                            {
                                dataReader.OnReadAll(ReferenceModeInstance.ObjectReferenceWriter);
                            }
                        }
                    }

                    if (IsFilterMode)
                    {
                        dataReader.OnReadAll(new DataFilterWriter<string>(this, this, dataReader));
                    }
                    else
                    {
                        dataReader.OnReadAll(this);
                    }

                    if (On(ReferenceOptions))
                    {
                        ReferenceModeInstance.LeaveReferenceScope();
                    }
                }
                else
#endif
                {
                    dataReader.OnReadAll(this);
                }

                --depth;

                Grow(2);

                if (tCurrent != current)
                {
                    --current;
                }
            }

            WriteStructAfter();

            Append(ObjectEnding);

            WriteValueAfter();
        }

        /// <summary>
        /// 写入对象开头。
        /// </summary>
        public void WriteBeginObject()
        {
            WriteValueBefore();

            Grow(2);

            Append(FixObject);

            WriteStructBefore();

            if (depth >= MaxDepth)
            {
                ThrowOutOfDepthException();
            }

            ++depth;
        }

        /// <summary>
        /// 写入对象结尾。
        /// </summary>
        public void WriteEndObject()
        {
            --depth;

            Grow(2);

            if (current > begin && *(--current) != ValueEnding)
            {
                ++current;
            }

            WriteStructAfter();

            Append(ObjectEnding);

            WriteValueAfter();
        }

        /// <summary>
        /// 写入数组开头。
        /// </summary>
        public void WriteBeginArray()
        {
            WriteValueBefore();

            Grow(2);

            Append(FixArray);

            WriteStructBefore();

            if (depth >= MaxDepth)
            {
                ThrowOutOfDepthException();
            }

            ++depth;
        }

        /// <summary>
        /// 写入数组结尾。
        /// </summary>
        public void WriteEndArray()
        {
            --depth;

            Grow(2);

            if (current != begin && *(--current) != ValueEnding)
            {
                ++current;
            }

            WriteStructAfter();

            Append(ArrayEnding);

            WriteValueAfter();
        }

        /// <summary>
        /// 写入一个属性名。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WritePropertyName(string name)
        {
            WriteKeyBefore();

            InternalWriteString(name
#if !NO_OPTIONS
                , On(CamelCaseWhenSerialize)
#endif
                );

            WriteKeyAfter();
        }

        /// <summary>
        /// 写入一个属性名。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void InternalWritePropertyName(Ps<char> name)
        {
            WriteKeyBefore();

            InternalWriteStringCore(ref *name.Pointer, name.Length
#if !NO_OPTIONS
                , On(CamelCaseWhenSerialize)
#endif
                );

            WriteKeyAfter();
        }

        /// <summary>
        /// 写入一个属性名。
        /// </summary>
        /// <param name="firstChar">属性名首个引用</param>
        /// <param name="length">属性名长度</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void InternalWritePropertyName(ref char firstChar, int length)
        {
            WriteKeyBefore();

            InternalWriteStringCore(ref firstChar, length
#if !NO_OPTIONS
                , On(CamelCaseWhenSerialize)
#endif
                );

            WriteKeyAfter();
        }

        #endregion

        #region -- 字符串 --

        /// <summary>
        /// 写入一个字符串。
        /// </summary>
        public void WriteString(string? value)
        {
            if (value is null)
            {
                WriteNull();
            }
            else
            {

#if !NO_OPTIONS
                if (On(MultiReferencingAlsoString)
                    && On(MultiReferencingReference | MultiReferencingNull)
                    && CheckObjectReference(value))
                {
                    return;
                }
#endif

                WriteValueBefore();

                InternalWriteString(value);

                WriteValueAfter();
            }
        }

#if !NO_OPTIONS
        /// <summary>
        /// 写入一个字符串。
        /// </summary>
        /// <param name="value">字符串</param>
        /// <param name="withCamelCase">是否以驼峰命名法写入</param>
#else
        /// <summary>
        /// 写入一个字符串。
        /// </summary>
        /// <param name="value">字符串</param>
#endif
        public void InternalWriteString(Ps<char> value
#if !NO_OPTIONS
            , bool withCamelCase = false
#endif
            )
        {
            WriteValueBefore();

            InternalWriteStringCore(ref *value.Pointer, value.Length
#if !NO_OPTIONS
                , withCamelCase
#endif
                );

            WriteValueAfter();
        }

#if !NO_OPTIONS
        /// <summary>
        /// 写入一个字符串。
        /// </summary>
        /// <param name="firstChar">字符串首个字符引用</param>
        /// <param name="length">字符串长度</param>
        /// <param name="withCamelCase">是否以驼峰命名法写入</param>
#else
        /// <summary>
        /// 写入一个字符串。
        /// </summary>
        /// <param name="firstChar">字符串首个字符引用</param>
        /// <param name="length">字符串长度</param>
#endif
        public void InternalWriteString(ref char firstChar, int length
#if !NO_OPTIONS
            , bool withCamelCase = false
#endif
            )
        {
            WriteValueBefore();

            InternalWriteStringCore(ref firstChar, length
#if !NO_OPTIONS
                , withCamelCase
#endif
                );

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void InternalWriteStringCore(ref char firstChar, int length
#if !NO_OPTIONS
            , bool withCamelCase = false
#endif
            )
        {
            const int ExpandLength = 5;

            Grow(length + ExpandLength);

            var current = this.current;

            var remain = (int)((end - current) - (length + ExpandLength));

            *current = FixString; ++current;

#if !NO_OPTIONS
            if (withCamelCase && length > 0)
            {
                var chr = StringHelper.ToLower(firstChar);

                if (IsGeneralChar(chr))
                {
                    *current = chr; ++current;

                    firstChar = ref Unsafe.Add(ref firstChar, 1);

                    --length;
                }
            }
#endif

            for (; length > 0; --length)
            {
                var chr = firstChar;

                if (IsGeneralChar(chr))
                {
                    *current = chr; ++current;
                }
                else
                {
                    switch (chr)
                    {
                        case FixString:
                            *current = FixEscape; ++current;
                            *current = FixString; ++current;
                            break;
                        case FixEscape:
                            *current = FixEscape; ++current;
                            *current = FixEscape; ++current;
                            break;
                        case WhiteChar2:
                            *current = FixEscape; ++current;
                            *current = EscapedWhiteChar2; ++current;
                            break;
                        case WhiteChar3:
                            *current = FixEscape; ++current;
                            *current = EscapedWhiteChar3; ++current;
                            break;
                        case WhiteChar4:
                            *current = FixEscape; ++current;
                            *current = EscapedWhiteChar4; ++current;
                            break;
                        case WhiteChar5:
                            *current = FixEscape; ++current;
                            *current = EscapedWhiteChar5; ++current;
                            break;
                        case WhiteChar6:
                            *current = FixEscape; ++current;
                            *current = EscapedWhiteChar6; ++current;
                            break;
                        default:
                            remain -= 4;

                            *current = FixEscape; ++current;
                            *current = FixunicodeEscape; ++current;
                            *current = FixNumberMin; ++current;
                            *current = FixNumberMin; ++current;

                            NumberHelper.Hex.AppendD2(current, chr); current += 2;
                            break;
                    }

                    --remain;

                    if (remain < 0)
                    {
                        this.current = current;

                        InternalGrow(length + ExpandLength);

                        current = this.current;

                        remain = (int)((end - current) - (length + ExpandLength));
                    }
                }

                firstChar = ref Unsafe.Add(ref firstChar, 1);
            }

            *current = FixString; ++current;

            this.current = current;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void InternalWriteString(string value
#if !NO_OPTIONS
            , bool withCamelCase = false
#endif
            )
        {
            InternalWriteStringCore(ref StringHelper.GetRawStringData(value), value.Length
#if !NO_OPTIONS
                , withCamelCase
#endif
                );
        }

        #endregion

        #region -- 引用 --

#if !NO_OPTIONS
        [MethodImpl(MethodImplOptions.NoInlining)]
        bool CheckObjectReference(object content)
        {
            if (ReferenceModeInstance.Nodes.Count is 0)
            {
                if (ReferenceModeInstance.ReferenceMap.Count is not 0)
                {
                    ReferenceModeInstance.ReferenceMap.Clear();
                }

                ReferenceModeInstance.ReferenceMap.Add(content, new RWPath()/* Root */);

                return false;
            }

            var referenceIndex = ReferenceModeInstance.ReferenceMap.FindIndex(content);

            if (referenceIndex is -1)
            {
                ReferenceModeInstance.ReferenceMap.Add(content, ReferenceModeInstance.ToRWPath());

                return false;
            }

            var reference = ReferenceModeInstance.ReferenceMap[referenceIndex].Value;

            var comparison = ReferenceModeInstance.ReferenceCompare(reference);

            if (comparison is 0)
            {
                return false;
            }

            if (On(LoopReferencingException | LoopReferencingNull) && comparison is not 1)
            {
                ReferenceModeInstance.ReferenceMap[referenceIndex].Value = ReferenceModeInstance.ToRWPath();
            }

            if (On(MultiReferencingReference))
            {
                WriteReference(reference);

                return true;
            }

            if (On(MultiReferencingNull))
            {
                WriteNull();

                return true;
            }

            if (comparison is 1)
            {
                if (On(LoopReferencingException))
                {
                    throw new JsonLoopReferencingException(reference, ReferenceModeInstance.ToRWPath(), content);
                }
                else
                {
                    WriteNull();
                }

                return true;
            }

            return false;
        }

#endif
        /// <summary>
        /// 写入一个引用值。
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void WriteReference(RWPath reference)
        {
            WriteValueBefore();

            Grow(2);

            Append(FixObject);

            WriteMiddleChars();

            Grow(8);

            // "$ref"
            WriteRaw(RefKeyString);

            WriteKeyAfter();

            Grow(2);

            Append(FixString);

            InternalWritePath(reference);

            Grow(3);

            Append(FixString);

            WriteMiddleChars();

            Append(ObjectEnding);

            WriteValueAfter();
        }

        #endregion

        #region -- 接口实现 --

        int IDataWriter.Count => -1;


        TargetableValueInterfaceMap ITargetableValueRW.ValueInterfaceMap => TargetableValueInterfaceMap.FromSource(JsonFormatter);

        Type? IDataWriter.ContentType => null;

        Type? IDataWriter.ValueType => null;

        Type? IValueWriter.ValueType => null;

        object? IDataWriter.Content
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        IValueWriter IDataWriter<string>.this[string key]
        {
            get
            {
                if (key is null)
                {
                    // TODO: Json 对象的键不允许为 Null。
                    throw new ArgumentNullException(nameof(key));
                }

#if !NO_OPTIONS
                if (On(ReferenceOptions))
                {
                    ReferenceModeInstance.SetReferenceLastNode(key);
                }
#endif

                WritePropertyName(key);

                return this;
            }
        }

        IValueWriter IDataWriter<int>.this[int key]
        {
            get
            {
#if !NO_OPTIONS
                if (On(ReferenceOptions))
                {
                    ReferenceModeInstance.SetReferenceLastNode(key);
                }
#endif

                return this;
            }
        }

        void IDataWriter.Initialize()
        {
        }

        void IDataWriter.Initialize(int capacity)
        {
        }

        void IDataWriter<string>.OnWriteAll(IDataReader<string> dataReader, RWStopToken stopToken)
        {
        }

        void IDataWriter<int>.OnWriteAll(IDataReader<int> dataReader, RWStopToken stopToken)
        {
        }

        void IDataWriter<string>.OnWriteValue(string key, IValueReader valueReader)
            => ValueInterface.WriteValue(((IDataWriter<string>)this)[key], valueReader.DirectRead());

        void IDataWriter<int>.OnWriteValue(int key, IValueReader valueReader)
            => ValueInterface.WriteValue(((IDataWriter<int>)this)[key], valueReader.DirectRead());

        void IRWPathNodeVisitor.VisitConstant<TKey>(RWPathConstantNode<TKey> node)
        {
            switch (Type.GetTypeCode(typeof(TKey)))
            {
                case TypeCode.SByte:
                    InternalWriteInt64(TypeHelper.As<TKey, sbyte>(node.Key));
                    return;
                case TypeCode.Byte:
                    InternalWriteUInt64(TypeHelper.As<TKey, byte>(node.Key));
                    return;
                case TypeCode.Int16:
                    InternalWriteInt64(TypeHelper.As<TKey, short>(node.Key));
                    return;
                case TypeCode.UInt16:
                    InternalWriteUInt64(TypeHelper.As<TKey, ushort>(node.Key));
                    return;
                case TypeCode.Int32:
                    InternalWriteInt64(TypeHelper.As<TKey, int>(node.Key));
                    return;
                case TypeCode.UInt32:
                    InternalWriteUInt64(TypeHelper.As<TKey, uint>(node.Key));
                    return;
                case TypeCode.Int64:
                    InternalWriteInt64(TypeHelper.As<TKey, long>(node.Key));
                    return;
                case TypeCode.UInt64:
                    InternalWriteUInt64(TypeHelper.As<TKey, ulong>(node.Key));
                    return;
            }

            var str
                = typeof(TKey) == typeof(string)
                ? Unsafe.As<RWPathConstantNode<string>>(node).Key
                : XConvert.Convert<TKey, string>(node.Key);

            if (str is null)
            {
                str = string.Empty;
            }

            Grow(str.Length * 3);

            foreach (var item in str)
            {
                if (IsRefGeneralChar(item))
                {
                    Append(item);
                }
                else
                {
                    Append(RefEscape);

                    NumberHelper.Hex.AppendD2(current, item); current += 2;
                }
            }
        }

#if !NO_OPTIONS
        bool IValueFilter<string>.Filter(ValueFilterInfo<string> valueInfo)
        {
            if (On(ReferenceOptions))
            {
                ReferenceModeInstance.SetReferenceLastNode(valueInfo.Key);
            }

            var result = Filter(valueInfo.ValueCopyer);

            if (JsonFormatter != null && On(OnFilter))
            {
                return JsonFormatter.OnObjectFilter(this, valueInfo, result);
            }

            return result;
        }

        bool IValueFilter<int>.Filter(ValueFilterInfo<int> valueInfo)
        {
            if (On(ReferenceOptions))
            {
                ReferenceModeInstance.SetReferenceLastNode(valueInfo.Key);
            }

            var result = Filter(valueInfo.ValueCopyer);

            if (JsonFormatter != null)
            {
                return JsonFormatter.OnArrayFilter(this, valueInfo, result);
            }

            return result;
        }
#endif

        #endregion

        #region -- 辅助类 --

#if !NO_OPTIONS
        class ComplexMode
        {
            public readonly string IndentedChars;
            public readonly string LineBreakChars;
            public readonly string MiddleChars;

            public ComplexMode(JsonSerializer jsonSerializer)
            {
                if (jsonSerializer.JsonFormatter is not null)
                {
                    IndentedChars = jsonSerializer.JsonFormatter.IndentedChars;
                    LineBreakChars = jsonSerializer.JsonFormatter.LineBreakChars;
                    MiddleChars = jsonSerializer.JsonFormatter.MiddleChars;
                }
                else
                {
                    IndentedChars = DefaultIndentedChars;
                    LineBreakChars = DefaultLineBreakChars;
                    MiddleChars = DefaultMiddleChars;
                }
            }
        }

        class ReferenceMode : ComplexMode, IDataWriter<string>, IDataWriter<int>, IValueWriter
        {
            public readonly LinkedList<RWPathNode> Nodes = new();
            public readonly OpenDictionary<object, RWPath> ReferenceMap = new(ReferenceEqualityComparer.Instance);

            public IDataWriter<int> ArrayReferenceWriter => this;

            public IDataWriter<string> ObjectReferenceWriter => this;

            public ReferenceMode(JsonSerializer jsonSerializer) : base(jsonSerializer)
            {
            }

            public bool TrySaveReference(IDataReader dataReader)
            {
                if (dataReader.ContentType?.IsValueType == false && dataReader.Content is object content)
                {
                    var index = ReferenceMap.FindIndex(content);

                    if (index >= 0)
                    {
                        return false;
                    }

                    ReferenceMap.Add(content, ToRWPath());

                    return true;
                }

                return false;
            }

            public RWPath? GetReference(object content)
            {
                var referenceIndex = ReferenceMap.FindIndex(content);

                if (referenceIndex is -1)
                {
                    return null;
                }

                return ReferenceMap[referenceIndex].Value;
            }

            public RWPath ToRWPath()
            {
                var result = new RWPath();

                foreach (var item in Nodes)
                {
                    result.Nodes.AddLast(item);
                }

                return result;
            }

            public float ReferenceCompare(RWPath reference)
            {
                var x = Nodes.First;
                var y = reference.Nodes.FirstNode;

            Loop:

                if (y is null)
                {
                    if (x is null)
                    {
                        return 0;
                    }

                    return 1;
                }

                if (x is null)
                {
                    return -1;
                }

                if (!Equals(x.Value, y.Value))
                {
                    return float.NaN;
                }

                x = x.Next;
                y = y.Next;

                goto Loop;
            }

            public void SetReferenceLastNode<TKey>(TKey key) where TKey : notnull
            {
                var lastNode = Nodes.Last;

                if (lastNode is null)
                {
                    throw new InvalidOperationException();
                }

                lastNode.Value = new RWPathConstantNode<TKey>(key);
            }

            public void EnterReferenceScope()
            {
                Nodes.AddLast(default(RWPathNode)!);
            }

            public void LeaveReferenceScope()
            {
                Nodes.RemoveLast();
            }

            int IDataWriter.Count => -1;

            Type? IDataWriter.ContentType => null;

            Type? IDataWriter.ValueType => null;

            Type? IValueWriter.ValueType => null;

            object? IDataWriter.Content
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            IValueWriter IDataWriter<int>.this[int key]
            {
                get
                {
                    SetReferenceLastNode(key);

                    return this;
                }
            }

            IValueWriter IDataWriter<string>.this[string key]
            {
                get
                {
                    SetReferenceLastNode(key);

                    return this;
                }
            }

            void IDataWriter.Initialize()
            {
            }

            void IDataWriter.Initialize(int capacity)
            {
            }

            void IDataWriter<string>.OnWriteValue(string key, IValueReader valueReader)
            {
                ValueInterface.WriteValue(ObjectReferenceWriter[key], valueReader.DirectRead());
            }

            void IDataWriter<string>.OnWriteAll(IDataReader<string> dataReader, RWStopToken stopToken)
            {
            }

            void IDataWriter<int>.OnWriteValue(int key, IValueReader valueReader)
            {
                ValueInterface.WriteValue(ArrayReferenceWriter[key], valueReader.DirectRead());
            }

            void IDataWriter<int>.OnWriteAll(IDataReader<int> dataReader, RWStopToken stopToken)
            {
            }

            void IValueWriter.WriteBoolean(bool value)
            {
            }

            void IValueWriter.WriteByte(byte value)
            {
            }

            void IValueWriter.WriteSByte(sbyte value)
            {
            }

            void IValueWriter.WriteInt16(short value)
            {
            }

            void IValueWriter.WriteChar(char value)
            {
            }

            void IValueWriter.WriteUInt16(ushort value)
            {
            }

            void IValueWriter.WriteInt32(int value)
            {
            }

            void IValueWriter.WriteSingle(float value)
            {
            }

            void IValueWriter.WriteUInt32(uint value)
            {
            }

            void IValueWriter.WriteInt64(long value)
            {
            }

            void IValueWriter.WriteDouble(double value)
            {
            }

            void IValueWriter.WriteUInt64(ulong value)
            {
            }

            void IValueWriter.WriteString(string? value)
            {
            }

            void IValueWriter.WriteDateTime(DateTime value)
            {
            }

            void IValueWriter.WriteDateTimeOffset(DateTimeOffset value)
            {
                throw new NotImplementedException();
            }

            void IValueWriter.WriteTimeSpan(TimeSpan value)
            {
                throw new NotImplementedException();
            }

            void IValueWriter.WriteGuid(Guid value)
            {
                throw new NotImplementedException();
            }

            void IValueWriter.WriteDecimal(decimal value)
            {
            }

            void IValueWriter.WriteObject(IDataReader<string> dataReader)
            {
                TrySaveReference(dataReader);
            }

            void IValueWriter.WriteArray(IDataReader<int> dataReader)
            {
                TrySaveReference(dataReader);
            }

            void IValueWriter.WriteEnum<T>(T value)
            {
            }

            void IValueWriter.DirectWrite(object? value)
            {
            }
        }

#endif
        #endregion
    }
}