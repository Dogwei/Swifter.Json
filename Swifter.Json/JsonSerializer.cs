using Swifter.Formatters;
using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

using static Swifter.Json.JsonCode;

using HGCache = Swifter.Tools.HGlobalCache<char>;

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
        public readonly JsonFormatterOptions options;
        public readonly HGCache hGCache;
        public readonly int maxDepth;

        public TextWriter textWriter;
        public JsonFormatter jsonFormatter;

        public TMode mode;

        public int depth;
        public int offset;


        public ref string LineBreakChars => ref Unsafe.As<TMode, JsonSerializeModes.ComplexMode>(ref mode).LineBreakChars;

        public ref string IndentedChars => ref Unsafe.As<TMode, JsonSerializeModes.ComplexMode>(ref mode).IndentedChars;

        public ref string MiddleChars => ref Unsafe.As<TMode, JsonSerializeModes.ComplexMode>(ref mode).MiddleChars;

        public ref bool IsInArray => ref Unsafe.As<TMode, JsonSerializeModes.ComplexMode>(ref mode).IsInArray;

        public ref JsonReferenceWriter References => ref Unsafe.As<TMode, JsonSerializeModes.ReferenceMode>(ref mode).References;

        public ref RWPathInfo Reference => ref Unsafe.As<TMode, JsonSerializeModes.ReferenceMode>(ref mode).References.Reference;

        public HGCache HGCache
        {
            get
            {
                Flush();

                return hGCache;
            }
        }

        public void Flush()
        {
            // 1: ,
            hGCache.Count = offset - 1;
        }

        public void Clear()
        {
            hGCache.Count = 0;

            offset = 0;
        }

        public JsonSerializer(JsonFormatterOptions options, HGCache hGCache, int maxDepth)
            : this(hGCache, maxDepth)
        {
            this.options = options;
        }

        public JsonSerializer(HGCache hGCache, int maxDepth)
        {
            this.hGCache = hGCache;
            this.maxDepth = maxDepth;
        }

        public JsonSerializer()
        {
            hGCache = new HGCache();

            maxDepth = int.MaxValue;
        }

        public IValueWriter this[string key]
        {
            get
            {
                if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode))
                {
                    Reference = RWPathInfo.Create(key, Reference.Parent);
                }

                WriteKeyBefore();

                InternalWriteString(key);

                WriteKeyAfter();

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

        public long TargetedId => jsonFormatter?.id ?? 0;

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
                if ((options & (JsonFormatterOptions.MultiReferencingNull | JsonFormatterOptions.LoopReferencingNull)) != 0 &&
                    (options & JsonFormatterOptions.IgnoreNull) != 0 &&
                    (options & JsonFormatterOptions.ArrayOnFilter) != 0 && !Reference.IsRoot)
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

            WriteStructBefore();

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

                    if (options >= JsonFormatterOptions.IgnoreNull && (options & JsonFormatterOptions.ArrayOnFilter) != 0)
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

            WriteStructAfter();

            Append(ArrayEnding);

            if (typeof(TMode) != typeof(JsonSerializeModes.SimpleMode))
            {
                IsInArray = isInArray;
            }

            WriteValueAfter();
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

            WriteValueAfter();
        }

        public void WriteByte(byte value) => WriteUInt64(value);

        public void WriteChar(char value) => WriteString(value.ToString());

        public void WriteDateTime(DateTime value)
        {
            WriteValueBefore();

            Expand(32);

            Append(FixString);

            offset += DateTimeHelper.ToISOString(value, hGCache.GetPointer() + offset);

            Append(FixString);

            WriteValueAfter();
        }

        public void WriteDecimal(decimal value)
        {
            WriteValueBefore();

            Expand(33);

            offset += NumberHelper.ToString(value, hGCache.GetPointer() + offset);

            WriteValueAfter();
        }

        public void WriteDouble(double value)
        {
            WriteValueBefore();

            if (value >= double.MinValue && value <= double.MaxValue)
            {
                Expand(24);

                offset += NumberHelper.Decimal.ToString(value, hGCache.GetPointer() + offset);
            }
            else
            {
                InternalWriteDoubleString(value);
            }

            WriteValueAfter();
        }

        public void WriteInt16(short value) => WriteInt64(value);

        public void WriteInt32(int value) => WriteInt64(value);

        public void WriteInt64(long value)
        {
            WriteValueBefore();

            InternalWriteInt64(value);

            WriteValueAfter();
        }

        public void InternalWriteInt64(long value)
        {
            Expand(21);

            offset += NumberHelper.Decimal.ToString(value, hGCache.GetPointer() + offset);
        }

        public void WriteObject(IDataReader<string> dataReader)
        {
            if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode))
            {
                if ((options & (JsonFormatterOptions.MultiReferencingNull | JsonFormatterOptions.LoopReferencingNull)) != 0 &&
                    (options & JsonFormatterOptions.IgnoreNull) != 0 && !Reference.IsRoot)
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

            WriteStructBefore();

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

                    if (options >= JsonFormatterOptions.IgnoreNull)
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

            WriteStructAfter();

            Append(ObjectEnding);

            if (typeof(TMode) != typeof(JsonSerializeModes.SimpleMode))
            {
                IsInArray = isInArray;
            }

            WriteValueAfter();
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

                WriteValueAfter();
            }
        }

        public void WriteUInt16(ushort value) => WriteUInt64(value);

        public void WriteUInt32(uint value) => WriteUInt64(value);

        public void WriteUInt64(ulong value)
        {
            WriteValueBefore();

            InternalWriteUInt64(value);

            WriteValueAfter();
        }

        public void InternalWriteUInt64(ulong value)
        {
            Expand(21);

            offset += NumberHelper.Decimal.ToString(value, hGCache.GetPointer() + offset);
        }

        void IValueWriter<Guid>.WriteValue(Guid value) => WriteGuid(value);

        void IValueWriter<DateTimeOffset>.WriteValue(DateTimeOffset value) => WriteDateTimeOffset(value);

        public void WriteGuid(Guid value)
        {
            WriteValueBefore();

            Expand(40);

            Append(FixString);

            offset += NumberHelper.ToString(value, hGCache.GetPointer() + offset);

            Append(FixString);

            WriteValueAfter();
        }

        public void WriteDateTimeOffset(DateTimeOffset value)
        {
            WriteValueBefore();

            Expand(32);

            Append('"');

            offset += DateTimeHelper.ToISOString(value, hGCache.GetPointer() + offset);

            Append('"');

            WriteValueAfter();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Expand(int expandMinSize)
        {
            if (hGCache.Capacity - offset < expandMinSize)
            {
                InternalExpand(expandMinSize);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void InternalExpand(int expandMinSize)
        {
            if (hGCache.Capacity == HGCache.MaxSize && textWriter != null && offset != 0)
            {
                hGCache.Count = offset;

                hGCache.WriteTo(textWriter);

                offset = 0;

                Expand(expandMinSize);
            }
            else
            {
                hGCache.Expand(expandMinSize);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append(char c)
        {
            hGCache.GetPointer()[offset] = c;

            ++offset;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Append(string value)
        {
            var length = value.Length;

            Expand(length + 2);

            var pointer = hGCache.GetPointer() + offset;

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
                if ((options & JsonFormatterOptions.Indented) != 0)
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
        public void WriteValueAfter()
        {
            Append(ValueEnding);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteKeyBefore()
        {
            if (typeof(TMode) != typeof(JsonSerializeModes.SimpleMode))
            {
                if ((options & JsonFormatterOptions.Indented) != 0)
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
        public void WriteKeyAfter()
        {
            Append(KeyEnding);

            WriteMiddleChars();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteMiddleChars()
        {
            if (typeof(TMode) != typeof(JsonSerializeModes.SimpleMode))
            {
                if ((options & JsonFormatterOptions.Indented) != 0)
                {
                    Append(MiddleChars);
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteStructBefore()
        {
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteStructAfter()
        {
            if (typeof(TMode) != typeof(JsonSerializeModes.SimpleMode))
            {
                if ((options & JsonFormatterOptions.Indented) != 0)
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

            var pointer = hGCache.GetPointer() + offset;

            for (int i = 0; i < value.Length; i++)
            {
                var item = value[i];

                if (item <= MaxEscapeChar && EscapeMap[item] != default)
                {
                    pointer[i] = FixEscape;

                    Expand(value.Length - i + 5);

                    ++offset;

                    pointer = hGCache.GetPointer() + offset;

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

            WriteValueAfter();
        }

        public bool Filter(ValueCopyer valueCopyer)
        {
            var basicType = valueCopyer.TypeCode;

            if (typeof(TMode) == typeof(JsonSerializeModes.ReferenceMode))
            {
                if (basicType == TypeCode.Object && (options & (JsonFormatterOptions.MultiReferencingNull | JsonFormatterOptions.LoopReferencingNull)) != 0 && valueCopyer.Value is IDataReader dataReader)
                {
                    var reference = GetReference(dataReader);

                    if (reference != null && ((options & JsonFormatterOptions.MultiReferencingNull) != 0 || IsLoopReferencing(reference)))
                    {
                        valueCopyer.DirectWrite(null);

                        basicType = TypeCode.Empty;
                    }
                }
            }

            if ((options & JsonFormatterOptions.IgnoreNull) != 0 && basicType == TypeCode.Empty)
            {
                return false;
            }

            if ((options & JsonFormatterOptions.IgnoreZero) != 0)
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

            if ((options & JsonFormatterOptions.IgnoreEmptyString) != 0
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

            if (depth <= maxDepth)
            {
                return true;
            }

            ThrowOutOfDepthException();

            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ThrowOutOfDepthException()
        {
            if ((options & JsonFormatterOptions.OutOfDepthException) != 0)
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
                    if ((options & JsonFormatterOptions.MultiReferencingReference) != 0)
                    {
                        WriteReference(reference);

                        return false;
                    }
                    else if ((options & JsonFormatterOptions.MultiReferencingNull) != 0)
                    {
                        WriteNull();

                        return false;
                    }
                    else if (IsLoopReferencing(reference))
                    {
                        if ((options & JsonFormatterOptions.LoopReferencingException) != 0)
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

            WriteMiddleChars();

            Expand(8);

            // "$ref"
            Append(RefKeyString);

            WriteKeyAfter();

            Expand(2);

            Append(FixString);

            WriteReferenceItem(reference);

            Expand(3);

            Append(FixString);

            WriteMiddleChars();

            Append(ObjectEnding);

            WriteValueAfter();
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

            WriteStructBefore();

            AddDepth();
        }

        public void WriteEndObject()
        {
            SubtractDepth();

            Expand(2);

            if (offset > 0 && hGCache.GetPointer()[--offset] != ValueEnding)
            {
                ++offset;
            }

            WriteStructAfter();

            Append(ObjectEnding);

            WriteValueAfter();
        }

        public void WriteBeginArray()
        {
            WriteValueBefore();

            Expand(2);

            Append(FixArray);

            WriteStructBefore();

            AddDepth();
        }

        public void WriteEndArray()
        {
            SubtractDepth();

            Expand(2);

            if (offset > 0 && hGCache.GetPointer()[--offset] != ValueEnding)
            {
                ++offset;
            }

            WriteStructAfter();

            Append(ArrayEnding);

            WriteValueAfter();
        }

        public void WritePropertyName(string name)
        {
            WriteKeyBefore();

            InternalWriteString(name);

            WriteKeyAfter();
        }

        public override string ToString()
        {
            return HGCache.ToStringEx();
        }
        public void MakeTargetedId()
        {
        }
    }
}