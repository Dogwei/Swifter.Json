using Swifter.Formatters;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

using static Swifter.Json.JsonFormatter;
using static Swifter.Json.JsonCode;
using static Swifter.Json.JsonFormatterOptions;
using static Swifter.Json.JsonSerializeModes;

namespace Swifter.Json
{
    sealed unsafe class JsonSerializer<TMode> :
        IJsonWriter,
        IValueWriter<Guid>,
        IValueWriter<DateTimeOffset>,
        IDataWriter<string>,
        IDataWriter<int>,
        IValueFilter<string>,
        IValueFilter<int>,
        IValueWriter<bool[]>, IValueWriter<List<bool>>,
        IValueWriter<byte[]>, IValueWriter<List<byte>>,
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

        public readonly JsonFormatter jsonFormatter;
        public readonly HGlobalCache<char> hGCache;
        public readonly TextWriter textWriter;
        public readonly int maxDepth;

        public readonly JsonFormatterOptions options;

        public int depth;

        public char* current;

        public int Offset
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => (int)(current - hGCache.First);
        }

        public int Rest
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => (int)(hGCache.Last - current);
        }

        public ref string LineBreakChars => ref Underlying.As<TMode, ComplexMode>(ref mode).LineBreakChars;

        public ref string IndentedChars => ref Underlying.As<TMode, ComplexMode>(ref mode).IndentedChars;

        public ref string MiddleChars => ref Underlying.As<TMode, ComplexMode>(ref mode).MiddleChars;

        public ref bool IsInArray => ref Underlying.As<TMode, ComplexMode>(ref mode).IsInArray;

        public ref JsonReferenceWriter References => ref Underlying.As<TMode, ReferenceMode>(ref mode).References;

        public ref RWPathInfo Reference => ref Underlying.As<TMode, ReferenceMode>(ref mode).References.Reference;

        public void Flush()
        {
            // 1: ,
            hGCache.Count = Offset - 1;
        }

        public void Clear()
        {
            current = hGCache.First;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonSerializer(HGlobalCache<char> hGCache, int maxDepth)
        {
            this.hGCache = hGCache;
            this.maxDepth = maxDepth;
            options = Default;

            current = hGCache.First;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonSerializer(HGlobalCache<char> hGCache, int maxDepth, JsonFormatterOptions options)
        {
            this.hGCache = hGCache;
            this.maxDepth = maxDepth;
            this.options = options;

            current = hGCache.First;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonSerializer(HGlobalCache<char> hGCache, int maxDepth, TextWriter textWriter)
            : this(hGCache, maxDepth)
        {
            this.textWriter = textWriter;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonSerializer(HGlobalCache<char> hGCache, int maxDepth, TextWriter textWriter, JsonFormatterOptions options)
            : this(hGCache, maxDepth, options)
        {
            this.textWriter = textWriter;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonSerializer(JsonFormatter jsonFormatter, HGlobalCache<char> hGCache, int maxDepth, JsonFormatterOptions options)
            : this(hGCache, maxDepth, options)
        {
            this.jsonFormatter = jsonFormatter;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonSerializer(JsonFormatter jsonFormatter, HGlobalCache<char> hGCache, int maxDepth, TextWriter textWriter, JsonFormatterOptions options)
            : this(jsonFormatter, hGCache, maxDepth, options)
        {
            this.textWriter = textWriter;
        }

        public IValueWriter this[string key]
        {
            get
            {
                if (key is null)
                {
                    // TODO: Json 对象的键不允许为 Null。
                    throw new ArgumentNullException(nameof(key));
                }

                if (IsReferenceMode)
                {
                    RWPathInfo.SetPath(Reference, key);
                }

                WritePropertyName(key);

                return this;
            }
        }

        public IValueWriter this[int key]
        {
            get
            {
                if (IsReferenceMode)
                {
                    RWPathInfo.SetPath(Reference, key);
                }

                return this;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append(char c)
        {
            *current = c; ++current;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append(string value)
        {
            var length = value.Length;

            Expand(length + 2);

            Underlying.CopyBlock(
                ref Underlying.As<char, byte>(ref *current), 
                ref Underlying.As<char, byte>(ref StringHelper.GetRawStringData(value)), 
                (uint)(length * 2));

            current += length;
        }


        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool On(JsonFormatterOptions options)
            => (this.options & options) != 0;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool Are(JsonFormatterOptions options)
            => (this.options & options) == options;

        public static bool IsReferenceMode
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => typeof(TMode) == typeof(ReferenceMode);
        }

        public static bool IsSimpleMode
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => typeof(TMode) == typeof(SimpleMode);
        }

        public static bool IsComplexMode
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => typeof(TMode) == typeof(ComplexMode);
        }

        public bool IsFilterMode
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => options >= OnFilter;
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
            if (hGCache.Context.Length >= HGlobalCache<char>.MaxSize && textWriter != null && current != hGCache.First)
            {
                hGCache.Count = Offset;

                hGCache.WriteTo(textWriter);

                current = hGCache.First;

                Expand(expandMinSize);
            }
            else
            {
                var offset = Offset;

                hGCache.Expand(expandMinSize);

                current = hGCache.First + offset;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ThrowOutOfDepthException()
        {
            if (On(OutOfDepthException))
            {
                throw new JsonOutOfDepthException();
            }
        }

        public bool Filter(ValueFilterInfo<string> valueInfo)
        {
            if (IsReferenceMode)
            {
                RWPathInfo.SetPath(Reference, valueInfo.Key);
            }

            var result = Filter(valueInfo.ValueCopyer);

            if (jsonFormatter != null && On(OnFilter))
            {
                return jsonFormatter.OnObjectFilter(this, valueInfo, result);
            }

            return result;
        }

        public bool Filter(ValueFilterInfo<int> valueInfo)
        {
            if (IsReferenceMode)
            {
                RWPathInfo.SetPath(Reference, valueInfo.Key);
            }

            var result = Filter(valueInfo.ValueCopyer);

            if (jsonFormatter != null)
            {
                return jsonFormatter.OnArrayFilter(this, valueInfo, result);
            }

            return result;
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
                        GetReference(dataReader.Content) is RWPathInfo reference &&
                        (On(MultiReferencingNull) || IsLoopReferencing(reference))
                    )
                    ||
                    (
                        Are(MultiReferencingNull | MultiReferencingAlsoString) &&
                        basicType == TypeCode.String &&
                        valueCopyer.InternalObject is string str &&
                        GetReference(str) != null
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

            if (On(IgnoreEmptyString) &&
                basicType == TypeCode.String &&
                valueCopyer.ReadString() == string.Empty)
            {
                return false;
            }

            return true;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteNull()
        {
            WriteValueBefore();

            Expand(6);

            //Append(nullString);

            // 考虑到 Null 值在 .Net 程序中非常常见，所以这里使用了比较极端的优化。
            *(ulong*)current = BitConverter.IsLittleEndian
                ? nullBits
                : nullBitsBigEndian;

            current += 4;

            AppendValueAfter();
        }

        public void DirectWrite(object value)
        {
            if (IsReferenceMode && On(MultiReferencingAlsoString) && On(MultiReferencingReference | MultiReferencingNull) && CheckObjectReference(value))
            {
                return;
            }

            Loop:

            if (value is null)
            {
                WriteNull();

                return;
            }

            switch (Type.GetTypeCode(value.GetType()))
            {
                case TypeCode.DBNull:
                    WriteNull();
                    return;
                case TypeCode.Boolean:
                    WriteBoolean((bool)value);
                    return;
                case TypeCode.Char:
                    WriteChar((char)value);
                    return;
                case TypeCode.SByte:
                    WriteSByte((sbyte)value);
                    return;
                case TypeCode.Byte:
                    WriteByte((byte)value);
                    return;
                case TypeCode.Int16:
                    WriteInt16((short)value);
                    return;
                case TypeCode.UInt16:
                    WriteUInt16((ushort)value);
                    return;
                case TypeCode.Int32:
                    WriteInt32((int)value);
                    return;
                case TypeCode.UInt32:
                    WriteUInt32((uint)value);
                    return;
                case TypeCode.Int64:
                    WriteInt64((long)value);
                    return;
                case TypeCode.UInt64:
                    WriteUInt64((ulong)value);
                    return;
                case TypeCode.Single:
                    WriteSingle((float)value);
                    return;
                case TypeCode.Double:
                    WriteDouble((double)value);
                    return;
                case TypeCode.Decimal:
                    WriteDecimal((decimal)value);
                    return;
                case TypeCode.DateTime:
                    WriteDateTime((DateTime)value);
                    return;
                case TypeCode.String:
                    WriteValueBefore();

                    InternalWriteString((string)value);

                    AppendValueAfter();
                    return;
            }

            if (value is Guid g)
            {
                WriteGuid(g);

                return;
            }

            if (value is DateTimeOffset dto)
            {
                WriteDateTimeOffset(dto);

                return;
            }

            value = value.ToString();

            goto Loop;
        }

        public void WriteArray(IDataReader<int> dataReader)
        {
            if (IsReferenceMode && dataReader.ContentType?.IsValueType == false && CheckObjectReference(dataReader.Content))
            {
                return;
            }

            WriteValueBefore();

            Expand(2);

            Append(FixArray);

            AppendStructBefore();

            var isInArray = false;

            if (!IsSimpleMode)
            {
                isInArray = IsInArray;

                IsInArray = true;
            }

            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();
            }
            else
            {
                var tCurrent = current;

                ++depth;

                if (IsSimpleMode)
                {
                    dataReader.OnReadAll(this);
                }
                else
                {
                    if (IsReferenceMode)
                    {
                        Reference = RWPathInfo.Create(default(int), Reference);

                        if (On(MultiReferencingOptimizeLayout))
                        {
                            if (dataReader.Count > 0)
                            {
                                dataReader.OnReadAll(References);
                            }
                        }
                    }

                    if (IsFilterMode && On(ArrayOnFilter))
                    {
                        dataReader.OnReadAll(new DataFilterWriter<int>(this, this));
                    }
                    else
                    {
                        dataReader.OnReadAll(this);
                    }

                    if (IsReferenceMode)
                    {
                        Reference = Reference.Parent;
                    }
                }

                --depth;

                Expand(2);

                if (tCurrent != current)
                {
                    --current;
                }
            }

            AppendStructAfter();

            Append(ArrayEnding);

            if (!IsSimpleMode)
            {
                IsInArray = isInArray;
            }

            AppendValueAfter();
        }

        public void WriteArray<T>(T[] array)
        {
            if (IsReferenceMode && CheckObjectReference(array))
            {
                return;
            }

            InternalWriteArray(array, array.Length);
        }

        public void WriteList<T>(List<T> list)
        {
            if (IsReferenceMode && CheckObjectReference(list))
            {
                return;
            }

            InternalWriteArray(ArrayHelper.GetRawData(list), list.Count);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void InternalWriteArray<T>(T[] array, int length)
        {
            WriteValueBefore();

            Expand(2);

            Append(FixArray);

            AppendStructBefore();

            var isInArray = false;

            if (!IsSimpleMode)
            {
                isInArray = IsInArray;

                IsInArray = true;
            }

            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();
            }
            else
            {
                var tCurrent = current;

                ++depth;

                if (!IsSimpleMode && IsFilterMode && On(ArrayOnFilter))
                {
                    var filterInfo = new ValueFilterInfo<int>();

                    for (int i = 0; i < length; i++)
                    {
                        filterInfo.Key = i;
                        filterInfo.Type = typeof(T);

                        ValueInterface.WriteValue(filterInfo.ValueCopyer, array[i]);

                        if (Filter(filterInfo))
                        {
                            filterInfo.ValueCopyer.WriteTo(this);
                        }
                    }
                }
                else if (ValueInterface<T>.IsNotModified)
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

                --depth;

                Expand(2);

                if (tCurrent != current)
                {
                    --current;
                }
            }

            AppendStructAfter();

            Append(ArrayEnding);

            if (!IsSimpleMode)
            {
                IsInArray = isInArray;
            }

            AppendValueAfter();
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
            else ValueInterface.WriteValue(this, value);
        }

        public void WriteBoolean(bool value)
        {
            WriteValueBefore();

            Expand(6);

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

            AppendValueAfter();
        }

        public void WriteByte(byte value) => WriteUInt64(value);

        public void WriteChar(char value)
        {
            WriteValueBefore();

            InternalWriteString(ref value, 1);

            AppendValueAfter();
        }

        public void WriteDateTime(DateTime value)
        {
            WriteValueBefore();

            Expand(DateTimeHelper.ISOStringMaxLength + 4);

            Append(FixString);

            current += DateTimeHelper.ToISOString(value, current);

            Append(FixString);

            AppendValueAfter();
        }

        public void WriteDecimal(decimal value)
        {
            WriteValueBefore();

            Expand(NumberHelper.DecimalStringMaxLength);

            current += NumberHelper.ToString(value, current);

            AppendValueAfter();
        }

        public void WriteDouble(double value)
        {
            WriteValueBefore();

            if (NumberHelper.IsSpecialValue(value))
            {
                InternalWriteDoubleString(value);
            }
            else
            {
                Expand(NumberHelper.DecimalMaxDoubleStringLength);

                if (On(UseSystemFloatingPointsMethods))
                {
#if Span
                    value.TryFormat(new Span<char>(current, Rest), out var charsWritter); // 在空间足够，此处一定成功。

                    current += charsWritter;
#else
                    Append(value.ToString());
#endif
                }
                else
                {
                    current += NumberHelper.Decimal.ToString(value, current);
                }
            }

            AppendValueAfter();
        }

        public void WriteInt16(short value) => WriteInt64(value);

        public void WriteInt32(int value) => WriteInt64(value);

        public void WriteInt64(long value)
        {
            WriteValueBefore();

            InternalWriteInt64(value);

            AppendValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void InternalWriteInt64(long value)
        {
            Expand(NumberHelper.DecimalMaxInt64NumbersLength);

            current += NumberHelper.ToDecimalString(value, current);
        }

        public void WriteObject(IDataReader<string> dataReader)
        {
            if (IsReferenceMode && dataReader.ContentType?.IsValueType == false && CheckObjectReference(dataReader.Content))
            {
                return;
            }

            WriteValueBefore();

            Expand(2);

            Append(FixObject);

            AppendStructBefore();

            var isInArray = false;

            if (!IsSimpleMode)
            {
                isInArray = IsInArray;

                IsInArray = false;
            }

            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();
            }
            else
            {
                var tCurrent = current;

                ++depth;

                if (IsSimpleMode)
                {
                    dataReader.OnReadAll(this);
                }
                else
                {
                    if (IsReferenceMode)
                    {
                        Reference = RWPathInfo.Create(default(string), Reference);

                        if (On(MultiReferencingOptimizeLayout))
                        {
                            if (dataReader.Count > 0)
                            {
                                dataReader.OnReadAll(References);
                            }
                        }
                    }

                    if (IsFilterMode)
                    {
                        dataReader.OnReadAll(new DataFilterWriter<string>(this, this));
                    }
                    else
                    {
                        dataReader.OnReadAll(this);
                    }

                    if (IsReferenceMode)
                    {
                        Reference = Reference.Parent;
                    }
                }

                --depth;

                Expand(2);

                if (tCurrent != current)
                {
                    --current;
                }
            }

            AppendStructAfter();

            Append(ObjectEnding);

            if (!IsSimpleMode)
            {
                IsInArray = isInArray;
            }

            AppendValueAfter();
        }

        public void WriteSByte(sbyte value) => WriteInt64(value);

        public void WriteSingle(float value)
        {
            WriteValueBefore();

            if (NumberHelper.IsSpecialValue(value))
            {
                InternalWriteDoubleString(value);
            }
            else
            {
                Expand(NumberHelper.DecimalMaxSingleStringLength);

                if (On(UseSystemFloatingPointsMethods))
                {
#if Span
                    value.TryFormat(new Span<char>(current, Rest), out var charsWritter); // 在空间足够，此处一定成功。

                    current += charsWritter;
#else
                    Append(value.ToString());
#endif
                }
                else
                {
                    current += NumberHelper.Decimal.ToString(value, current);
                }
            }

            AppendValueAfter();
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
                    CheckObjectReference(value))
                {
                    return;
                }

                WriteValueBefore();

                InternalWriteString(value);

                AppendValueAfter();
            }
        }

        public void WriteUInt16(ushort value) => WriteUInt64(value);

        public void WriteUInt32(uint value) => WriteUInt64(value);

        public void WriteUInt64(ulong value)
        {
            WriteValueBefore();

            InternalWriteUInt64(value);

            AppendValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void InternalWriteUInt64(ulong value)
        {
            Expand(NumberHelper.DecimalMaxUInt64NumbersLength);

            current += NumberHelper.ToDecimalString(value, current);
        }

        public void WriteEnum<T>(T value) where T : struct, Enum
        {
            Expand(128);

            WriteValueBefore();

            if (EnumHelper.IsFlagsEnum<T>() && EnumHelper.AsUInt64(value) != 0)
            {
                // TODO: 枚举值不应该出现 '"' '\n' '\r' 等违法字符；如果存在违法字符串，这里序列化的 Json 将格式错误。
                // TODO: 通常情况下，hGCache 剩余的空间完全足够任何枚举的 Flags 字符串，这里如果超出了，将使用整数格式。

                if (EnumHelper.AsUInt64(
                    EnumHelper.FormatEnumFlags(value, current + 1, (hGCache.Available - Offset - 18), out var written)
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
                    InternalWriteInt64(Underlying.As<T, sbyte>(ref value));
                    break;
                case TypeCode.Int16:
                    InternalWriteInt64(Underlying.As<T, short>(ref value));
                    break;
                case TypeCode.Int32:
                    InternalWriteInt64(Underlying.As<T, int>(ref value));
                    break;
                case TypeCode.Int64:
                    InternalWriteInt64(Underlying.As<T, long>(ref value));
                    break;
                default:
                    InternalWriteUInt64(EnumHelper.AsUInt64(value));
                    break;
            }

            After:

            AppendValueAfter();
        }

        void IValueWriter<Guid>.WriteValue(Guid value) => WriteGuid(value);

        void IValueWriter<DateTimeOffset>.WriteValue(DateTimeOffset value) => WriteDateTimeOffset(value);

        public void WriteGuid(Guid value)
        {
            WriteValueBefore();

            Expand(NumberHelper.GuidStringWithSeparatorsLength + 4);

            Append(FixString);

            current += NumberHelper.ToString(value, current, true);

            Append(FixString);

            AppendValueAfter();
        }

        public void WriteDateTimeOffset(DateTimeOffset value)
        {
            WriteValueBefore();

            Expand(DateTimeHelper.ISOStringMaxLength + 4);

            Append(FixString);

            current += DateTimeHelper.ToISOString(value, current);

            Append(FixString);

            AppendValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void WriteValueBefore()
        {
            if (!IsSimpleMode)
            {
                if (On(Indented))
                {
                    if (IsInArray)
                    {
                        Append(LineBreakChars);

                        for (int i = depth; i > 0; --i)
                        {
                            Append(IndentedChars);
                        }
                    }
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void AppendValueAfter()
        {
            Append(ValueEnding);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void AppendKeyBefore()
        {
            if (!IsSimpleMode)
            {
                if (On(Indented))
                {
                    if (!IsInArray)
                    {
                        Append(LineBreakChars);

                        for (int i = depth; i > 0; --i)
                        {
                            Append(IndentedChars);
                        }
                    }
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void AppendKeyAfter()
        {
            Append(KeyEnding);

            AppendMiddleChars();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void AppendMiddleChars()
        {
            if (!IsSimpleMode)
            {
                if (On(Indented))
                {
                    Append(MiddleChars);
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void AppendStructBefore()
        {
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void AppendStructAfter()
        {
            if (!IsSimpleMode)
            {
                if (On(Indented))
                {
                    Append(LineBreakChars);

                    for (int i = depth; i > 0; --i)
                    {
                        Append(IndentedChars);
                    }
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void InternalWriteString(ref char firstChar, int length)
        {
            var expand = length + 5;

            Expand(expand);

            Append(FixString);

            var current = this.current;

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
                            expand += 4;

                            *current = FixEscape; ++current;
                            *current = FixunicodeEscape; ++current;
                            *current = FixNumberMin; ++current;
                            *current = FixNumberMin; ++current;

                            NumberHelper.Hex.AppendD2(current, chr); current += 2;
                            break;
                    }

                    ++expand;

                    if (this.current + expand > hGCache.Last)
                    {
                        var offset = current - this.current;

                        InternalExpand(expand);

                        current = this.current + offset;
                    }
                }

                firstChar = ref Underlying.Add(ref firstChar, 1);
            }

            this.current = current;

            Append(FixString);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void InternalWriteString(string value)
        {
            InternalWriteString(ref StringHelper.GetRawStringData(value), value.Length);
        }

        public void InternalWriteStringWithCamelCase(string value)
        {
            // TODO: Fast
            var offset = Offset;

            InternalWriteString(value);

            ref var firstChar = ref current[(offset - Offset) + 1];

            firstChar = char.ToLower(firstChar);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void InternalWriteDoubleString(double value)
        {
            // NaN, PositiveInfinity, NegativeInfinity, Or Other...

            // TODO: Fast
            InternalWriteString(value.ToString());
        }

        public RWPathInfo GetReference(object content)
        {
            if (References.TryGetValue(content, out var reference))
            {
                if (Reference.Equals(reference))
                {
                    return null;
                }

                return reference;
            }

            References.Add(content, Reference.Clone());

            return null;
        }

        public bool IsLoopReferencing(RWPathInfo reference)
        {
            var curr = Reference;

            while ((curr = curr.Parent) != null)
            {
                if (curr.Equals(reference))
                {
                    return true;
                }
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool CheckObjectReference(object content)
        {
            if (Reference.IsRoot)
            {
                if (References.Count > 0)
                {
                    References.Clear();
                }

                References.Add(content, Reference);

                return false;
            }

            var reference = GetReference(content);

            if (reference is null)
            {
                return false;
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

            if (IsLoopReferencing(reference))
            {
                if (On(LoopReferencingException))
                {
                    throw new JsonLoopReferencingException(reference, Reference, content);
                }
                else
                {
                    WriteNull();
                }

                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void WriteReference(RWPathInfo reference)
        {
            WriteValueBefore();

            Expand(2);

            Append(FixObject);

            AppendMiddleChars();

            Expand(8);

            // "$ref"
            Append(RefKeyString);

            AppendKeyAfter();

            Expand(2);

            Append(FixString);

            WriteReferenceItem(reference);

            Expand(3);

            Append(FixString);

            AppendMiddleChars();

            Append(ObjectEnding);

            AppendValueAfter();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void WriteReferenceItem(RWPathInfo reference)
        {
            if (reference.Parent != null)
            {
                WriteReferenceItem(reference.Parent);
            }

            if (reference.IsRoot)
            {
                Expand(2);

                // Root
                Append(ReferenceRootPathName);

                return;
            }

            Expand(2);

            Append(ReferenceSeparator);

            var key = reference.GetKey();

            if (key is int intKey)
            {
                InternalWriteInt64(intKey);
            }
            else
            {
                var strKey = Convert.ToString(key);

                Expand(strKey.Length * 3);

                foreach (var item in strKey)
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
        }

        public void WriteBeginObject()
        {
            WriteValueBefore();

            Expand(2);

            Append(FixObject);

            AppendStructBefore();

            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();
            }

            ++depth;
        }

        public void WriteEndObject()
        {
            --depth;

            Expand(2);

            if (current != hGCache.First && *(--current) != ValueEnding)
            {
                ++current;
            }

            AppendStructAfter();

            Append(ObjectEnding);

            AppendValueAfter();
        }

        public void WriteBeginArray()
        {
            WriteValueBefore();

            Expand(2);

            Append(FixArray);

            AppendStructBefore();

            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();
            }

            ++depth;
        }

        public void WriteEndArray()
        {
            --depth;

            Expand(2);

            if (current != hGCache.First && *(--current) != ValueEnding)
            {
                ++current;
            }

            AppendStructAfter();

            Append(ArrayEnding);

            AppendValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WritePropertyName(string name)
        {
            AppendKeyBefore();

            if (On(CamelCaseWhenSerialize) && name.Length > 0 && char.IsUpper(name[0]))
            {
                InternalWriteStringWithCamelCase(name);
            }
            else
            {
                InternalWriteString(name);
            }

            AppendKeyAfter();
        }

        public override string ToString()
        {
            Flush();

            return hGCache.ToStringEx();
        }

        public int Count => -1;

        long ITargetedBind.TargetedId => jsonFormatter?.targeted_id ?? GlobalTargetedId;

        IEnumerable<string> IDataWriter<string>.Keys => null;

        IEnumerable<int> IDataWriter<int>.Keys => null;

        Type IDataWriter.ContentType => null;

        object IDataWriter.Content
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public void MakeTargetedId()
        {
        }

        public void Initialize()
        {
        }

        public void Initialize(int capacity)
        {
        }

        public void OnWriteAll(IDataReader<string> dataReader)
        {
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
        }

        public void OnWriteValue(string key, IValueReader valueReader)
        {
            ValueInterface.WriteValue(this[key], valueReader.DirectRead());
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            ValueInterface.WriteValue(this[key], valueReader.DirectRead());
        }

        void IValueWriter<bool[]>.WriteValue(bool[] value) => WriteArray(value);

        void IValueWriter<byte[]>.WriteValue(byte[] value) => WriteArray(value);

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
    }
}