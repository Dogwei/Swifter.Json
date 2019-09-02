using Swifter.Formatters;

using Swifter.RW;
using Swifter.Tools;

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

using static Swifter.Json.JsonCode;

namespace Swifter.Json
{
    sealed unsafe class JsonSerializer<TMode> :
        IJsonWriter,
        IFormatterWriter,
        IValueWriter<Guid>,
        IValueWriter<DateTimeOffset>,
        IDataWriter<string>,
        IDataWriter<int>,
        IValueFilter<string>,
        IValueFilter<int>
        where TMode : struct
    {
        const JsonFormatterOptions ReturnHG = ((JsonFormatterOptions)0x4000) | JsonFormatterOptions.Default;

        public readonly JsonFormatterOptions Options;
        public readonly HGlobalCache<char> HGCache;
        public readonly int MaxDepth;

        public TextWriter TextWriter;
        public JsonFormatter JsonFormatter;

        public TMode mode;

        public int depth;
        public int offset;

        public ref string LineBreakChars => ref Unsafe.As<TMode, JsonSerializeModes.ComplexMode>(ref mode).LineBreakChars;

        public ref string IndentedChars => ref Unsafe.As<TMode, JsonSerializeModes.ComplexMode>(ref mode).IndentedChars;

        public ref string MiddleChars => ref Unsafe.As<TMode, JsonSerializeModes.ComplexMode>(ref mode).MiddleChars;

        public ref bool IsInArray => ref Unsafe.As<TMode, JsonSerializeModes.ComplexMode>(ref mode).IsInArray;

        public ref JsonReferenceWriter References => ref Unsafe.As<TMode, JsonSerializeModes.ReferenceMode>(ref mode).References;

        public ref RWPathInfo Reference => ref Unsafe.As<TMode, JsonSerializeModes.ReferenceMode>(ref mode).References.Reference;

        public void Flush()
        {
            // 1: ,
            HGCache.Count = offset - 1;
        }

        public void Clear()
        {
            offset = 0;
        }

        public JsonSerializer(HGlobalCache<char> hGCache, int maxDepth)
        {
            HGCache = hGCache;
            MaxDepth = maxDepth;
        }

        public JsonSerializer(HGlobalCache<char> hGCache, int maxDepth, JsonFormatterOptions options)
            : this(hGCache, maxDepth)
        {
            Options = options;
        }

        public JsonSerializer(HGlobalCache<char> hGCache, int maxDepth, TextWriter textWriter)
            : this(hGCache, maxDepth)
        {
            TextWriter = textWriter;
        }

        public JsonSerializer(HGlobalCache<char> hGCache, int maxDepth, TextWriter textWriter, JsonFormatterOptions options)
            : this(hGCache, maxDepth)
        {
            TextWriter = textWriter;
            Options = options;
        }

        public JsonSerializer(JsonFormatter jsonFormatter, HGlobalCache<char> hGCache, int maxDepth, JsonFormatterOptions options)
            : this(hGCache, maxDepth)
        {
            JsonFormatter = jsonFormatter;
            Options = options;
        }

        public JsonSerializer(JsonFormatter jsonFormatter, HGlobalCache<char> hGCache, int maxDepth, TextWriter textWriter, JsonFormatterOptions options)
            : this(jsonFormatter, hGCache,  maxDepth, options)
        {
            TextWriter = textWriter;
        }

        public IValueWriter this[string key]
        {
            get
            {
                if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode))
                {
                    Reference = RWPathInfo.Create(key, Reference.Parent);
                }

                AppendKeyBefore();

                InternalWriteString(key);

                AppendKeyAfter();

                return this;
            }
        }

        public IValueWriter this[int key]
        {
            get
            {
                if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode))
                {
                    Reference = RWPathInfo.Create(key, Reference.Parent);
                }

                return this;
            }
        }

        public long TargetedId => JsonFormatter?.targeted_id ?? 0;

        public IEnumerable<string> Keys => null;

        public int Count => 0;

        IEnumerable<int> IDataWriter<int>.Keys => null;

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
                        WriteString((string)value);
                        return;
                }
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

            WriteString(value.ToString());
        }

        public bool Filter(ValueFilterInfo<string> valueInfo) => Filter(valueInfo.ValueCopyer);

        public bool Filter(ValueFilterInfo<int> valueInfo) => Filter(valueInfo.ValueCopyer);

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
            this[key].DirectWrite(valueReader.DirectRead());
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            ValueInterface.WriteValue(this[key], valueReader.DirectRead());
        }

        public void WriteArray(IDataReader<int> dataReader)
        {
            if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode))
            {
                if ((Options & (JsonFormatterOptions.MultiReferencingNull | JsonFormatterOptions.LoopReferencingNull)) != 0 &&
                    (Options & JsonFormatterOptions.IgnoreNull) != 0 &&
                    (Options & JsonFormatterOptions.ArrayOnFilter) != 0 && !Reference.IsRoot)
                {
                }
                else if (!CheckObjectReference(dataReader))
                {
                    return;
                }
            }

            WriteValueBefore();

            Expand(2);

            Append(FixArray);

            AppendStructBefore();

            var isInArray = false;

            if (typeof(TMode) != typeof(JsonSerializeModes.SimpleMode))
            {
                isInArray = IsInArray;

                IsInArray = true;
            }

            int tOffset = offset;

            if (AddDepth())
            {
                if (typeof(TMode) == typeof(JsonSerializeModes.SimpleMode))
                {
                    dataReader.OnReadAll(this);
                }
                else
                {
                    if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode))
                    {
                        if (dataReader.Count != 0)
                        {
                            dataReader.OnReadAll(References);
                        }

                        Reference = RWPathInfo.Create(string.Empty, Reference);
                    }

                    if (Options >= JsonFormatterOptions.IgnoreNull && (Options & JsonFormatterOptions.ArrayOnFilter) != 0)
                    {
                        dataReader.OnReadAll(this, this);
                    }
                    else
                    {
                        dataReader.OnReadAll(this);
                    }

                    if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode))
                    {
                        Reference = Reference.Parent;
                    }
                }
            }

            SubtractDepth();

            Expand(2);

            if (tOffset != offset)
            {
                --offset;
            }

            AppendStructAfter();

            Append(ArrayEnding);

            if (typeof(TMode) != typeof(JsonSerializeModes.SimpleMode))
            {
                IsInArray = isInArray;
            }

            AppendValueAfter();
        }

        public void WriteBoolean(bool value)
        {
            WriteValueBefore();

            Expand(6);

            if (value)
            {
                // true
                Append('t');
                Append('r');
                Append('u');
                Append('e');
            }
            else
            {
                // false
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
            Expand(6);

            Append(FixString);

            if (value <= MaxEscapeChar && EscapeMap[value] != default)
            {
                Append(FixEscape);
                Append(EscapeMap[value]);
            }
            else
            {
                Append(value);
            }

            Append(FixString);

            AppendValueAfter();
        }

        public void WriteDateTime(DateTime value)
        {
            WriteValueBefore();

            Expand(32);

            Append(FixString);

            offset += DateTimeHelper.ToISOString(value, HGCache.GetPointer() + offset);

            Append(FixString);

            AppendValueAfter();
        }

        public void WriteDecimal(decimal value)
        {
            WriteValueBefore();

            Expand(33);

            offset += NumberHelper.ToString(value, HGCache.GetPointer() + offset);

            AppendValueAfter();
        }

        public void WriteDouble(double value)
        {
            WriteValueBefore();

            if (value >= double.MinValue && value <= double.MaxValue)
            {
                Expand(24);

                offset += NumberHelper.Decimal.ToString(value, HGCache.GetPointer() + offset);
            }
            else
            {
                InternalWriteDoubleString(value);
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

        public void InternalWriteInt64(long value)
        {
            Expand(21);

            offset += NumberHelper.Decimal.ToString(value, HGCache.GetPointer() + offset);
        }

        public void WriteObject(IDataReader<string> dataReader)
        {
            if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode))
            {
                if ((Options & (JsonFormatterOptions.MultiReferencingNull | JsonFormatterOptions.LoopReferencingNull)) != 0 &&
                    (Options & JsonFormatterOptions.IgnoreNull) != 0 && !Reference.IsRoot)
                {
                }
                else if (!CheckObjectReference(dataReader))
                {
                    return;
                }
            }

            WriteValueBefore();

            Expand(2);

            Append(FixObject);

            AppendStructBefore();

            var isInArray = false;

            if (typeof(TMode) != typeof(JsonSerializeModes.SimpleMode))
            {
                isInArray = IsInArray;

                IsInArray = false;
            }

            int tOffset = offset;

            if (AddDepth())
            {
                if (typeof(TMode) == typeof(JsonSerializeModes.SimpleMode))
                {
                    dataReader.OnReadAll(this);
                }
                else
                {
                    if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode))
                    {
                        if (dataReader.Count != 0)
                        {
                            dataReader.OnReadAll(References);
                        }

                        Reference = RWPathInfo.Create(string.Empty, Reference);
                    }

                    if (Options >= JsonFormatterOptions.IgnoreNull)
                    {
                        dataReader.OnReadAll(this, this);
                    }
                    else
                    {
                        dataReader.OnReadAll(this);
                    }

                    if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode))
                    {
                        Reference = Reference.Parent;
                    }
                }
            }

            SubtractDepth();

            Expand(2);

            if (tOffset != offset)
            {
                --offset;
            }

            AppendStructAfter();

            Append(ObjectEnding);

            if (typeof(TMode) != typeof(JsonSerializeModes.SimpleMode))
            {
                IsInArray = isInArray;
            }

            AppendValueAfter();
        }

        public void WriteSByte(sbyte value) => WriteInt64(value);

        public void WriteSingle(float value) => WriteDouble(value);

        public void WriteString(string value)
        {
            if (value == null)
            {
                WriteNull();
            }
            else
            {
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

        public void InternalWriteUInt64(ulong value)
        {
            Expand(21);

            offset += NumberHelper.Decimal.ToString(value, HGCache.GetPointer() + offset);
        }

        void IValueWriter<Guid>.WriteValue(Guid value) => WriteGuid(value);

        void IValueWriter<DateTimeOffset>.WriteValue(DateTimeOffset value) => WriteDateTimeOffset(value);

        public void WriteGuid(Guid value)
        {
            WriteValueBefore();

            Expand(40);

            Append(FixString);

            offset += NumberHelper.ToString(value, HGCache.GetPointer() + offset);

            Append(FixString);

            AppendValueAfter();
        }

        public void WriteDateTimeOffset(DateTimeOffset value)
        {
            WriteValueBefore();

            Expand(32);

            Append('"');

            offset += DateTimeHelper.ToISOString(value, HGCache.GetPointer() + offset);

            Append('"');

            AppendValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Expand(int expandMinSize)
        {
            if (HGCache.Capacity - offset < expandMinSize)
            {
                InternalExpand(expandMinSize);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InternalExpand(int expandMinSize)
        {
            if (HGCache.Capacity == HGlobalCache<char>.MaxSize && TextWriter != null && offset != 0)
            {
                HGCache.Count = offset;

                HGCache.WriteTo(TextWriter);

                offset = 0;

                Expand(expandMinSize);
            }
            else
            {
                HGCache.Expand(expandMinSize);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append(char c)
        {
            HGCache.GetPointer()[offset] = c;

            ++offset;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Append(string value)
        {
            var length = value.Length;

            Expand(length + 2);

            var pointer = HGCache.GetPointer() + offset;

            for (int i = 0; i < length; ++i)
            {
                pointer[i] = value[i];
            }

            offset += length;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void WriteValueBefore()
        {
            if (typeof(TMode) != typeof(JsonSerializeModes.SimpleMode))
            {
                if ((Options & JsonFormatterOptions.Indented) != 0)
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
            if (typeof(TMode) != typeof(JsonSerializeModes.SimpleMode))
            {
                if ((Options & JsonFormatterOptions.Indented) != 0)
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
            if (typeof(TMode) != typeof(JsonSerializeModes.SimpleMode))
            {
                if ((Options & JsonFormatterOptions.Indented) != 0)
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
            if (typeof(TMode) != typeof(JsonSerializeModes.SimpleMode))
            {
                if ((Options & JsonFormatterOptions.Indented) != 0)
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
        public void InternalWriteString(string value)
        {
            Expand(value.Length + 5);

            Append(FixString);

            var pointer = HGCache.GetPointer() + offset;

            for (int i = 0; i < value.Length; i++)
            {
                var item = value[i];

                if (item <= MaxEscapeChar && EscapeMap[item] != default)
                {
                    pointer[i] = FixEscape;

                    if (HGCache.Capacity - offset < value.Length + 5)
                    {
                        InternalExpand(5);
                    }

                    ++offset;

                    pointer = HGCache.GetPointer() + offset;

                    item = EscapeMap[item];
                }

                pointer[i] = item;
            }

            offset += value.Length;

            Append(FixString);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void InternalWriteDoubleString(double value)
        {
            // NaN, PositiveInfinity, NegativeInfinity, Or Other...
            InternalWriteString(value.ToString());
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteNull()
        {
            WriteValueBefore();

            Expand(6);

            // null
            Append('n');
            Append('u');
            Append('l');
            Append('l');

            AppendValueAfter();
        }

        public bool Filter(ValueCopyer valueCopyer)
        {
            var basicType = valueCopyer.TypeCode;

            if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode))
            {
                if (basicType == TypeCode.Object && (Options & (JsonFormatterOptions.MultiReferencingNull | JsonFormatterOptions.LoopReferencingNull)) != 0 && valueCopyer.Value is IDataReader dataReader)
                {
                    var reference = GetReference(dataReader);

                    if (reference != null && ((Options & JsonFormatterOptions.MultiReferencingNull) != 0 || IsLoopReferencing(reference)))
                    {
                        valueCopyer.DirectWrite(null);

                        basicType = TypeCode.Empty;
                    }
                }
            }

            if ((Options & JsonFormatterOptions.IgnoreNull) != 0 && basicType == TypeCode.Empty)
            {
                return false;
            }

            if ((Options & JsonFormatterOptions.IgnoreZero) != 0)
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

            if ((Options & JsonFormatterOptions.IgnoreEmptyString) != 0
                && basicType == TypeCode.String
                && valueCopyer.ReadString() == string.Empty)
            {
                return false;
            }

            return true;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool AddDepth()
        {
            ++depth;

            if (depth <= MaxDepth)
            {
                return true;
            }

            ThrowOutOfDepthException();

            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ThrowOutOfDepthException()
        {
            if ((Options & JsonFormatterOptions.OutOfDepthException) != 0)
            {
                throw new JsonOutOfDepthException();
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SubtractDepth()
        {
            --depth;
        }

        public RWPathInfo GetReference(IDataReader dataReader)
        {
            var token = dataReader.ReferenceToken;

            if (token != null && References.TryGetValue(token, out var reference) && !Reference.Equals(reference))
            {
                return reference;
            }

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
        public bool CheckObjectReference(IDataReader dataReader)
        {
            if (Reference.IsRoot)
            {
                var token = dataReader.ReferenceToken;

                if (token != null)
                {
                    References.DirectAdd(token, Reference);
                }
            }
            else
            {
                var reference = GetReference(dataReader);

                if (reference != null)
                {
                    if ((Options & JsonFormatterOptions.MultiReferencingReference) != 0)
                    {
                        WriteReference(reference);

                        return false;
                    }
                    else if ((Options & JsonFormatterOptions.MultiReferencingNull) != 0)
                    {
                        WriteNull();

                        return false;
                    }
                    else if (IsLoopReferencing(reference))
                    {
                        if ((Options & JsonFormatterOptions.LoopReferencingException) != 0)
                        {
                            throw new JsonLoopReferencingException(reference, Reference);
                        }
                        else /*if ((options & JsonFormatterOptions.LoopReferencingNull) != 0)*/
                        {
                            WriteNull();
                        }

                        return false;
                    }
                }
            }

            return true;
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
                var name = key.ToString();

                Expand(name.Length * 3);

                for (int i = 0; i < name.Length; i++)
                {
                    var c = name[i];

                    switch (c)
                    {
                        case '\\':
                            // %5C
                            Append('%');
                            Append('5');
                            Append('C');
                            break;
                        case '/':
                            // %2F
                            Append('%');
                            Append('2');
                            Append('F');
                            break;
                        case '"':
                            // %22
                            Append('%');
                            Append('2');
                            Append('2');
                            break;
                        default:
                            Append(c);
                            break;
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

            AddDepth();
        }

        public void WriteEndObject()
        {
            SubtractDepth();

            Expand(2);

            if (offset > 0 && HGCache.GetPointer()[--offset] != ValueEnding)
            {
                ++offset;
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

            AddDepth();
        }

        public void WriteEndArray()
        {
            SubtractDepth();

            Expand(2);

            if (offset > 0 && HGCache.GetPointer()[--offset] != ValueEnding)
            {
                ++offset;
            }

            AppendStructAfter();

            Append(ArrayEnding);

            AppendValueAfter();
        }

        public void WritePropertyName(string name)
        {
            AppendKeyBefore();

            InternalWriteString(name);

            AppendKeyAfter();
        }

        public override string ToString()
        {
            Flush();

            return HGCache.ToStringEx();
        }

        public void MakeTargetedId()
        {
        }
    }
}