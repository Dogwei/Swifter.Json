using Swifter.Formatters;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static Swifter.Json.JsonCode;
using static Swifter.Json.JsonFormatter;
using static Swifter.Json.JsonFormatterOptions;
using static Swifter.Json.JsonDeserializeModes;
using static Swifter.Tools.StringHelper;
using ArrayType = System.Collections.Generic.List<object>;
using ObjectType = System.Collections.Generic.Dictionary<string, object>;

namespace Swifter.Json
{
    sealed unsafe class JsonDeserializer<TMode> :
        IJsonReader,
        IDataReader<string>,
        IValueReader<Guid>,
        IValueReader<DateTimeOffset>,
        IValueReader<bool[]>, IValueReader<List<bool>>,
        IValueReader<byte[]>, IValueReader<List<byte>>,
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
        IValueReader<DateTime[]>, IValueReader<List<DateTime>>,
        IValueReader<DateTimeOffset[]>, IValueReader<List<DateTimeOffset>>,
        IValueReader<decimal[]>, IValueReader<List<decimal>>,
        IValueReader<Guid[]>, IValueReader<List<Guid>>,
        IValueReader<string[]>, IValueReader<List<string>>,
        IValueReader<object[]>, IValueReader<List<object>>,
        IFormatterReader
        where TMode : struct
    {
        public readonly JsonFormatter jsonFormatter;
        public readonly JsonFormatterOptions options;

        public TMode mode;

        public char* current;
        public readonly char* begin;
        public readonly char* end;

        public readonly int maxDepth;

        public int depth;

        public int Offset => (int)(current - begin);

        public int Length => (int)(end - current);

        public long TargetedId => jsonFormatter?.targeted_id ?? 0;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonDeserializer(char* chars, int length, int maxDepth, JsonFormatterOptions options)
        {
            begin = chars;
            end = chars + length;

            current = chars;

            this.maxDepth = maxDepth;
            this.options = options;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonDeserializer(char* chars, int length, int maxDepth)
        {
            begin = chars;
            end = chars + length;

            current = chars;

            this.maxDepth = maxDepth;
            options = Default;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonDeserializer(JsonFormatter jsonFormatter, char* chars, int length, int maxDepth, JsonFormatterOptions options) : this(chars, length, maxDepth, options)
        {
            this.jsonFormatter = jsonFormatter;
        }

        public ref Reference ReferenceMode => ref Underlying.As<TMode, Reference>(ref mode);

        public IEnumerable<string> Keys => null;

        public int Count => -1;

        public Type ContentType => null;

        public object Content
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public IValueReader this[string key]
        {
            get
            {
                Loop:

                SkipWhiteSpace();

                switch (*current)
                {
                    case ObjectEnding:
                        goto Empty;
                    case ValueEnding:
                        ++current;
                        goto Loop;
                    case FixString:
                    case FixChars:

                        if (TryReadNamedString(key))
                        {
                            break;
                        }

                        goto Empty;
                    default:

                        if (TryReadNamedText(key))
                        {
                            break;
                        }

                        goto Empty;
                }

                if (*current == ObjectEnding)
                {
                    goto Empty;
                }

                SkipWhiteSpace();

                if (IsContent() && *current == KeyEnding)
                {
                    ++current;

                    SkipWhiteSpace();

                    if (IsContent())
                    {
                        return this;
                    }
                }

                throw GetException();

                Empty:

                return RWHelper.DefaultValueReader;
            }
        }

        public JsonToken GetToken()
        {
            if (IsContent())
            {
                switch (*current)
                {
                    case FixString:
                    case FixChars:
                        return JsonToken.String;
                    case FixArray:
                        return JsonToken.Array;
                    case FixObject:
                        return JsonToken.Object;
                    case Fixtrue:
                    case FixTrue:
                    case Fixfalse:
                    case FixFalse:
                        return JsonToken.Boolean;
                    case Fixnull:
                    case FixNull:
                    case Fixundefined:
                    case FixUndefined:
                        return JsonToken.Null;
                    case FixPositive:
                    case FixNegative:
                    case var dight when dight >= FixNumberMin && dight <= FixNumberMax:
                        return JsonToken.Number;
                    default:
                        return JsonToken.Other;
                }
            }

            return JsonToken.End;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ThrowOutOfDepthException()
        {
            if (On(OutOfDepthException))
            {
                throw new JsonOutOfDepthException();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public object DirectRead()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
                    return InternalReadString().ToStringEx();
                case FixArray:
                    return ValueInterface<ArrayType>.ReadValue(this);
                case FixObject:

                    if (IsReferenceMode)
                    {
                        if (IsReference())
                        {
                            return ReadReference();
                        }
                    }

                    return ValueInterface<ObjectType>.ReadValue(this);
                case Fixtrue:
                case FixTrue:
                    if (Verify(trueString))
                    {
                        current += trueString.Length;

                        return true;
                    }
                    break;
                case Fixfalse:
                case FixFalse:
                    if (Verify(falseString))
                    {
                        current += falseString.Length;

                        return false;
                    }
                    break;
                case Fixnull:
                case FixNull:
                    if (Verify(nullString))
                    {
                        current += nullString.Length;

                        return null;
                    }
                    break;
                case Fixundefined:
                case FixUndefined:
                    if (Verify(undefinedString))
                    {
                        current += undefinedString.Length;

                        return null;
                    }
                    break;
                case FixPositive:
                case FixNegative:
                    goto Number;
                case ValueEnding:

                    if (IsTuple())
                    {
                        return DirectRead();
                    }

                    goto Text;

                case var curr when curr >= FixNumberMin && curr <= FixNumberMax:
                    goto Number;
            }

            Text:

            return InternalReadText().ToStringEx();

            Number:

            var numberInfo = NumberHelper.GetNumberInfo(current, Length);

            if (IsEnding(numberInfo.End) && numberInfo.IsNumber && numberInfo.IsCommonRadix(out var radix))
            {
                current += numberInfo.End;

                if (numberInfo.HaveExponent)
                {
                    if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 16 && numberInfo.ExponentCount <= 3)
                    {
                        return numberInfo.ToDouble(radix);
                    }
                }
                else if (numberInfo.HaveFractional)
                {
                    if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 16)
                    {
                        return numberInfo.ToDouble(radix);
                    }

                    if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 28 && numberInfo.IsDecimal)
                    {
                        return numberInfo.ToDecimal();
                    }
                }
                else if (numberInfo.IntegerCount <= 18)
                {
                    var value = numberInfo.ToInt64(radix);

                    if (value <= int.MaxValue && value >= int.MinValue)
                    {
                        return (int)value;
                    }

                    return value;
                }
                else if (numberInfo.IntegerCount <= 28 && numberInfo.IsDecimal)
                {
                    return numberInfo.ToDecimal();
                }

                return numberInfo.ToString();
            }

            goto Text;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool IsTuple()
        {
            var temp = current++;

            SkipWhiteSpace();

            if (IsContent() && *current != ValueEnding)
            {
                return true;
            }

            current = temp;

            return false;
        }

        public T[] ReadArray<T>()
        {
            switch (*current)
            {
                case Fixnull:
                case FixNull:
                    if (Verify(nullString))
                    {
                        current += nullString.Length;

                        return null;
                    }
                    goto default;
                case FixString:
                case FixChars:
                    if (TryReadEmptyString())
                    {
                        return null;
                    }

                    goto default;
                case FixArray:
                    break;
                case ValueEnding:

                    if (IsTuple())
                    {
                        return ReadArray<T>();
                    }

                    goto default;

                default:

                    return XConvert.FromObject<T[]>(DirectRead());
            }

            if (IsReferenceMode)
            {
                ReferenceMode.EnterObject(new ArrayRW<T>());
            }

            var result = InternalReadArray<T>(JsonArrayAppendingInfo<T[]>.AppendingInfo.MostClosestMeanCommonlyUsedLength, out var length);

            JsonArrayAppendingInfo<T[]>.AppendingInfo.AddUsedLength(length);

            if (result.Length != length)
            {
                Array.Resize(ref result, length);
            }

            if (IsReferenceMode)
            {
                ReferenceMode.CurrentObject.Content = result;

                ReferenceMode.LeavaObject();
            }

            return result;
        }

        public List<T> ReadList<T>()
        {
            switch (*current)
            {
                case Fixnull:
                case FixNull:
                    if (Verify(nullString))
                    {
                        current += nullString.Length;

                        return null;
                    }
                    goto default;
                case FixString:
                case FixChars:
                    if (TryReadEmptyString())
                    {
                        return null;
                    }

                    goto default;
                case FixArray:
                    break;
                case ValueEnding:

                    if (IsTuple())
                    {
                        return ReadList<T>();
                    }

                    goto default;

                default:

                    return XConvert.FromObject<List<T>>(DirectRead());
            }

            if (IsReferenceMode)
            {
                ReferenceMode.EnterObject(new ArrayRW<T>());
            }

            var array = InternalReadArray<T>(JsonArrayAppendingInfo<List<T>>.AppendingInfo.MostClosestMeanCommonlyUsedLength, out var length);

            JsonArrayAppendingInfo<List<T>>.AppendingInfo.AddUsedLength(length);

            if (IsReferenceMode)
            {
                ReferenceMode.CurrentObject.Content = array;

                ReferenceMode.LeavaObject();
            }

            return ArrayHelper.CreateList(array, length);
        }
        
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool IsContent() => /*IsDeflateMode || IsStandardMode ||*/ current < end;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T[] InternalReadArray<T>(int defaultCapacity, out int length)
        {
            var array = new T[defaultCapacity];
            var offset = 0;

            if (depth < maxDepth)
            {
                ++current;
                ++depth;

                Loop:

                SkipWhiteSpace();

                if (IsContent())
                {
                    if (*current == ArrayEnding)
                    {
                        goto Return;
                    }

                    if (offset >= array.Length)
                    {
                        Array.Resize(ref array, offset * 2 + 1);

                        if (IsReferenceMode)
                        {
                            ReferenceMode.CurrentObject.Content = array;
                        }
                    }

                    if (IsReferenceMode)
                    {
                        ReferenceMode.SetCurrentKey(offset);
                    }

                    if (ValueInterface<T>.IsNotModified)
                    {
                        array[offset] = ReadValue<T>();
                    }
                    else
                    {
                        array[offset] = ValueInterface<T>.ReadValue(this);
                    }

                    ++offset;

                    SkipWhiteSpace();

                    if (IsContent())
                    {
                        switch (*current)
                        {
                            case ArrayEnding:
                                goto Return;
                            case ValueEnding:
                                ++current;

                                goto Loop;
                        }
                    }
                }

                throw GetException();

                Return:

                --depth;
                ++current;
            }
            else
            {
                ThrowOutOfDepthException();

                SkipValue();
            }

            length = offset;

            return array;
        }

        public void ReadArray(IDataWriter<int> dataWriter)
        {
            switch (*current)
            {
                case Fixnull:
                case FixNull:
                    if (Verify(nullString))
                    {
                        current += nullString.Length;

                        return;
                    }
                    goto default;
                case FixObject:
                    if (IsReferenceMode)
                    {
                        if (IsReference())
                        {
                            goto default;
                        }
                    }
                    ReadObject(dataWriter.As<string>());
                    return;
                case FixArray:
                    break;
                case ValueEnding:

                    if (IsTuple())
                    {
                        ReadArray(dataWriter);

                        return;
                    }

                    goto default;

                default:

                    dataWriter.Content = XConvert.Cast(DirectRead(), dataWriter.ContentType);
                    return;
            }

            dataWriter.Initialize();

            SlowReadArray(dataWriter);
        }

        public void SlowReadArray(IDataWriter<int> dataWriter)
        {
            if (IsReferenceMode)
            {
                ReferenceMode.EnterObject(dataWriter);
            }

            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();

                SkipValue();

                return;
            }

            ++current;
            ++depth;

            var offset = 0;

            Loop:

            SkipWhiteSpace();

            if (IsContent())
            {
                if (*current == ArrayEnding)
                {
                    goto Return;
                }

                if (IsReferenceMode)
                {
                    ReferenceMode.SetCurrentKey(offset);
                }

                dataWriter.OnWriteValue(offset, this);

                ++offset;

                SkipWhiteSpace();

                if (IsContent())
                {
                    switch (*current)
                    {
                        case ArrayEnding:
                            goto Return;
                        case ValueEnding:
                            ++current;

                            goto Loop;
                    }
                }
            }

            throw GetException();

            Return:

            --depth;
            ++current;

            if (IsReferenceMode)
            {
                ReferenceMode.LeavaObject();
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

        public static bool IsDeflateMode
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => typeof(TMode) == typeof(Deflate);
        }
        
        public static bool IsStandardMode
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => typeof(TMode) == typeof(Standard);
        }

        public static bool IsVerifiedMode
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => typeof(TMode) == typeof(Verified);
        }

        public static bool IsReferenceMode
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => typeof(TMode) == typeof(Reference);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SkipWhiteSpace()
        {
            if (!IsDeflateMode)
            {
                if (IsContent() && *current <= 0x20)
                {
                    NoInliningSkipWhiteSpace();
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void NoInliningSkipWhiteSpace()
        {
            Loop:

            switch (*current)
            {
                case WhiteChar1:
                case WhiteChar2:
                case WhiteChar3:
                case WhiteChar4:
                case WhiteChar5:
                case WhiteChar6:
                    ++current;
                    if (IsContent())
                    {
                        goto Loop;
                    }
                    break;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool IsEnding(int offset)
        {
            if (current + offset >= end)
            {
                return current + offset == end;
            }

            Loop:

            var curr = current[offset];

            switch (curr)
            {
                case ObjectEnding:
                case ArrayEnding:
                case KeyEnding:
                case ValueEnding:
                    return true;
            }

            if (IsDeflateMode)
            {
                return false;
            }
            else if (IsStandardMode)
            {
                switch (curr)
                {
                    case WhiteChar1:
                    case WhiteChar2:
                    case WhiteChar3:
                    case WhiteChar4:
                    case WhiteChar5:
                    case WhiteChar6:
                        return true;
                }

                return false;
            }
            else
            {
                switch (curr)
                {
                    case WhiteChar1:
                    case WhiteChar2:
                    case WhiteChar3:
                    case WhiteChar4:
                    case WhiteChar5:
                    case WhiteChar6:
                        ++offset;

                        goto Loop;
                }

                return false;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool Verify(string lowerstr)
        {
            if (IsDeflateMode)
            {
                return true;
            }

            if (!IsEnding(lowerstr.Length))
            {
                return false;
            }

            for (int i = 0; i < lowerstr.Length; i++)
            {
                if (current[i] != lowerstr[i] && ToLower(current[i]) != lowerstr[i])
                {
                    return false;
                }
            }

            return true;
        }

        public bool ReadBoolean()
        {
            var curr = *current;

            switch (curr)
            {
                case Fixtrue:
                case FixTrue:
                    if (Verify(trueString))
                    {
                        current += trueString.Length;

                        return true;
                    }
                    break;
                case FixString:
                case FixChars:
                    if ((options & EmptyStringAsDefault) != 0 && TryReadEmptyString())
                    {
                        return default;
                    }

                    break;
                case Fixfalse:
                case FixFalse:
                    if (Verify(falseString))
                    {
                        current += falseString.Length;

                        return false;
                    }
                    break;
            }

            if (curr >= FixNumberMin && curr <= FixNumberMax && IsEnding(1))
            {
                ++current;

                return curr != FixNumberMin;
            }

            return Convert.ToBoolean(DirectRead());
        }

        public byte ReadByte() => checked((byte)ReadUInt64());

        public char ReadChar()
        {
            if (Length >= 3 && current[0] == current[2] && (current[0] == FixString || current[0] == FixChars) && current[1] != FixEscape)
            {
                current += 3;

                return current[-2];
            }

            if ((options & EmptyStringAsDefault) != 0 && (*current == FixString || *current == FixChars) && TryReadEmptyString())
            {
                return default;
            }



            return char.Parse(ReadString());
        }

        public DateTime ReadDateTime()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
                    if ((options & EmptyStringAsDefault) != 0 && TryReadEmptyString())
                    {
                        return default;
                    }

                    return ReadParse<DateTime>();
            }

            return Convert.ToDateTime(DirectRead());
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T ReadParse<T>()
        {
            return FastReadParse<T>(InternalReadString(), options);
        }

        public decimal ReadDecimal()
        {
            var (_, length, value) = NumberHelper.ParseDecimal(current, Length);

            if (length != 0 && IsEnding(length))
            {
                current += length;

                return value;
            }

            return NoInliningReadDecimal();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public decimal NoInliningReadDecimal()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
                    if ((options & EmptyStringAsDefault) != 0 && TryReadEmptyString())
                    {
                        return default;
                    }

                    return ReadParse<decimal>();
            }

            var numberInfo = NumberHelper.GetNumberInfo(current, Length);

            if (numberInfo.IsNumber)
            {
                if (IsEnding(numberInfo.End) && numberInfo.IsDecimal)
                {
                    current += numberInfo.End;

                    return numberInfo.ToDecimal();
                }

                return decimal.Parse(InternalReadText().ToStringEx(), NumberStyle);
            }

            return Convert.ToDecimal(DirectRead());
        }

        public double ReadDouble()
        {
            if (On(UseSystemFloatingPointsMethods))
            {
                var currentBackup = current;

                switch (*currentBackup)
                {
                    case FixPositive:
                    case FixNegative:
                    case var digit when digit >= FixNumberMin && digit <= FixNumberMax:
#if Span
                        if (double.TryParse(InternalReadText(), out var value))
                        {
                            return value;
                        }

#else

                        if (double.TryParse(InternalReadText().ToStringEx(), out var value))
                        {
                            return value;
                        }

#endif
                        break;
                }

                current = currentBackup;

            }
            else
            {
                var (_, length, value) = NumberHelper.DecimalParseDouble(current, Length);

                if (length != 0 && IsEnding(length))
                {
                    current += length;

                    return value;
                }
            }

            return NoInliningReadDouble();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public double NoInliningReadDouble()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
                    if ((options & EmptyStringAsDefault) != 0 && TryReadEmptyString())
                    {
                        return default;
                    }

                    return ReadParse<double>();
            }

            var numberInfo = NumberHelper.GetNumberInfo(current, Length);

            if (numberInfo.IsNumber)
            {
                if (IsEnding(numberInfo.End) && numberInfo.IsCommonRadix(out var radix))
                {
                    current += numberInfo.End;

                    return numberInfo.ToDouble(radix);
                }

                return double.Parse(InternalReadText().ToStringEx(), NumberStyle);
            }

            return Convert.ToDouble(DirectRead());
        }

        public short ReadInt16() => checked((short)ReadInt64());

        public int ReadInt32() => checked((int)ReadInt64());

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ReadInt64()
        {
            var (_, length, value) = NumberHelper.DecimalParseInt64(current, Length);

            if (length != 0 && IsEnding(length))
            {
                current += length;

                return value;
            }

            return NoInliningReadInt64();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private long NoInliningReadInt64()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
                    if ((options & EmptyStringAsDefault) != 0 && TryReadEmptyString())
                    {
                        return default;
                    }

                    return ReadParse<long>();
            }

            var numberInfo = NumberHelper.GetNumberInfo(current, Length);

            if (numberInfo.IsNumber)
            {
                if (IsEnding(numberInfo.End) && numberInfo.IsCommonRadix(out var radix))
                {
                    current += numberInfo.End;

                    return numberInfo.ToInt64(radix);
                }

                return long.Parse(InternalReadText().ToStringEx(), NumberStyle);
            }

            return Convert.ToInt64(DirectRead());
        }

        public T? ReadNullable<T>() where T : struct
        {
            var curr = *current;

            switch (curr)
            {
                case Fixnull:
                case FixNull:
                    if (Verify(nullString))
                    {
                        current += nullString.Length;

                        return null;
                    }
                    break;
                case Fixundefined:
                case FixUndefined:
                    if (Verify(undefinedString))
                    {
                        current += undefinedString.Length;

                        return null;
                    }
                    break;
                case FixString:
                case FixChars:
                    if (TryReadEmptyString())
                    {
                        return default;
                    }

                    break;
            }

            return ReadValue<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool IsReference()
        {
            if (Length < 8)
            {
                return false;
            }

            if (current[1] == FixString) if (current[2] == DollarChar) goto Could; else return false;
            if (current[2] == FixString) if (current[3] == DollarChar) goto Could; else return false;
            if (current[3] == FixString) if (current[4] == DollarChar) goto Could; else return false;
            if (current[4] == FixString) if (current[5] == DollarChar) goto Could; else return false;
            if (current[5] == FixString) if (current[6] == DollarChar) goto Could; else return false;
            if (current[6] == FixString) if (current[7] == DollarChar) goto Could; else return false;
            if (current[7] == FixString) if (current[8] == DollarChar) goto Could; else return false;

            Could:

            return NoInliningIsReference();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool NoInliningIsReference()
        {
            var swap = current;

            var res = SkipReferenceKey();

            current = swap;

            return res;
        }

        public bool SkipReferenceKey()
        {
            ++current;

            SkipWhiteSpace();

            var res = false;

            if (IsContent())
            {
                switch (current[0])
                {
                    case DollarChar:
                        res = Verify(RefKey);
                        current += RefKey.Length;
                        break;
                    case FixString:
                        res = Verify(RefKeyString);
                        current += RefKeyString.Length;
                        break;
                    case FixChars:
                        res = Verify(RefKeyChars);
                        current += RefKeyChars.Length;
                        break;
                }
            }

            SkipWhiteSpace();

            if (res && IsContent() && *current == KeyEnding)
            {
                ++current;

                SkipWhiteSpace();
            }

            return res;
        }

        public void ReadObject(IDataWriter<string> dataWriter)
        {
            switch (*current)
            {
                case Fixnull:
                case FixNull:
                    if (Verify(nullString))
                    {
                        current += nullString.Length;

                        return;
                    }
                    goto default;
                case FixString:
                case FixChars:
                    if (TryReadEmptyString())
                    {

                        return;
                    }

                    goto default;
                case FixArray:
                    ReadArray(dataWriter.As<int>());
                    return;
                case FixObject:
                    if (IsReferenceMode)
                    {
                        if (IsReference())
                        {
                            goto default;
                        }
                    }
                    break;
                case ValueEnding:

                    if (IsTuple())
                    {
                        ReadObject(dataWriter);

                        return;
                    }

                    goto default;

                default:
                    dataWriter.Content = XConvert.Cast(DirectRead(), dataWriter.ContentType);
                    return;
            }

            dataWriter.Initialize();

            if (dataWriter is IDataWriter<Ps<char>> fastWriter)
            {
                FastReadObject(fastWriter);
            }
            else
            {
                SlowReadObject(dataWriter);
            }
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

        public string ReadString()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
                    return InternalReadString().ToStringEx();
                case Fixnull:
                case FixNull:
                    if (Verify(nullString))
                    {
                        current += nullString.Length;

                        return null;
                    }
                    break;
            }

            return NoInliningReadString();
        }

        public object ReadReference()
        {
            if (!SkipReferenceKey())
            {
                goto Exception;
            }

            RWPathInfo reference = null;

            switch (*current)
            {
                case FixString:
                case FixChars:
                    reference = ReadParse<RWPathInfo>();
                    break;
                default:
                    var str = ReadString();

                    if (str != null)
                    {
                        reference = SlowReadParse<RWPathInfo>(str);
                    }

                    break;
            }

            SkipWhiteSpace();

            if (!(IsContent() && *current == ObjectEnding))
            {
                goto Exception;
            }

            ++current;

            return ReferenceMode.GetValue(reference);

            Exception:

            throw new NotSupportedException("Not a reference format.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string NoInliningReadString()
        {
            switch (*current)
            {
                case Fixtrue:
                case FixTrue:
                    if (Verify(trueString))
                    {
                        current += trueString.Length;

                        return TrueString;
                    }
                    break;
                case Fixfalse:
                case FixFalse:
                    if (Verify(falseString))
                    {
                        current += falseString.Length;

                        return FalseString;
                    }
                    break;
                case Fixundefined:
                case FixUndefined:
                    if (Verify(undefinedString))
                    {
                        current += undefinedString.Length;

                        return null;
                    }
                    break;
                case FixObject:
                case FixArray:
                    return Convert.ToString(DirectRead());
                case FixPositive:
                case FixNegative:
                    goto Number;
                case ValueEnding:

                    if (IsTuple())
                    {
                        return ReadString();
                    }

                    goto default;

                default:
                    if (*current >= FixNumberMin && *current <= FixNumberMax)
                    {
                        goto Number;
                    }
                    break;
            }

            Text:

            return InternalReadText().ToStringEx();

            Number:

            var numberInfo = NumberHelper.GetNumberInfo(current, Length);

            if (IsEnding(numberInfo.End) && numberInfo.IsNumber)
            {
                current += numberInfo.End;

                return numberInfo.ToString();
            }

            goto Text;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Exception GetException()
        {
            return new JsonDeserializeException(Offset);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ThrowException()
        {
            throw GetException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Ps<char> InternalReadText()
        {
            var temp = current;

            while (IsContent())
            {
                switch (*current)
                {
                    case KeyEnding:
                    case ValueEnding:
                    case ObjectEnding:
                    case ArrayEnding:

                        goto Return;

                    default:

                        ++current;

                        continue;
                }
            }

            Return:

            if (IsDeflateMode)
            {
                return new Ps<char>(temp, (int)(current - temp));
            }
            else
            {
                return Trim(temp, (int)(current - temp));
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public Ps<char> InternalReadString()
        {
            var fixStr = *current;

            ++current;

            var offset = IndexOfAny(current, Length, fixStr, FixEscape);

            if (offset == -1)
            {
                ThrowException();
            }

            if (current[offset] != FixEscape)
            {
                var ret = new Ps<char>(current, offset);

                current += offset + 1;

                return ret;
            }

            var hGCache = CharsPool.Current();

            var strStart = hGCache.First;
            var strLength = 0;

            Loop:

            if (strLength + offset > 20480)
            {
                hGCache.Grow(strLength + offset);

                strStart = hGCache.First;
            }

            Underlying.CopyBlock(strStart + strLength, current, checked((uint)offset * sizeof(char)));

            current += offset;
            strLength += offset;

            if (*current != FixEscape)
            {
                ++current;

                return new Ps<char>(strStart, strLength);
            }

            strStart[strLength] = Descape(ref current);

            ++strLength;

            offset = IndexOfAny(current, Length, fixStr, FixEscape);

            if (offset == -1)
            {
                ThrowException();
            }

            goto Loop;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool TryReadNamedText(string name)
        {
            var backup = current;

            if (EqualsWithIgnoreCase(InternalReadText(), name))
            {
                return true;
            }

            current = backup;

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryReadNamedString(string name)
        {
            var backup = current;
            var fixStr = *current;

            ++current;

            if (Length <= name.Length)
            {
                goto False;
            }

            for (int i = 0; i < name.Length; i++)
            {
                var chr = *current;

                if (chr == FixEscape)
                {
                    chr = Descape(ref current);
                }
                else
                {
                    ++current;
                }

                if (name[i] != chr && ToLower(name[i]) != ToLower(chr))
                {
                    goto False;
                }
            }

            if (*current == fixStr)
            {
                ++current;

                return true;
            }

            False:

            current = backup;

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryReadEmptyString()
        {
            if(On(EmptyStringAsNull | EmptyStringAsDefault))
            {
                if (current + 1 < end && *(current + 1) == *current)
                {
                    current += 2;

                    return true;
                }

#if WhiteSpaceStringAsNull

                if (On(WhiteSpaceStringAsNull & WhiteSpaceStringAsDefault))
                {
                    return TryReadWhiteSpaceString();
                }

#endif

            }

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool On(JsonFormatterOptions options)
            => (this.options & options) != 0;


#if WhiteSpaceStringAsNull

        public bool TryReadWhiteSpaceString()
        {
            var temp = current;

            var fixStr = *temp;

            ++temp;

            while (temp < end)
            {
                var chr = *temp;

                if (chr == fixStr)
                {
                    current = temp + 1;

                    return true;
                }

                if (chr == FixEscape)
                {
                    chr = Descape(ref temp);
                }
                else
                {
                    ++temp;
                }

                if (!IsWhiteSpace(chr))
                {
                    break;
                }
            }

            return false;
        }

#endif


        public ushort ReadUInt16() => checked((ushort)ReadUInt64());

        public uint ReadUInt32() => checked((uint)ReadUInt64());

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            var (_, length, value) = NumberHelper.DecimalParseUInt64(current, Length);

            if (length != 0 && IsEnding(length))
            {
                current += length;

                return value;
            }

            return NoInliningReadUInt64();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private ulong NoInliningReadUInt64()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
                    if ((options & EmptyStringAsDefault) != 0 && TryReadEmptyString())
                    {
                        return default;
                    }

                    return ReadParse<ulong>();
            }

            var numberInfo = NumberHelper.GetNumberInfo(current, Length);

            if (numberInfo.IsNumber)
            {
                if (IsEnding(numberInfo.End) && numberInfo.IsCommonRadix(out var radix))
                {
                    current += numberInfo.End;

                    return numberInfo.ToUInt64(radix);
                }

                return ulong.Parse(InternalReadText().ToStringEx(), NumberStyle);
            }

            return Convert.ToUInt64(DirectRead());
        }

        public Guid ReadGuid()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
                    if ((options & EmptyStringAsDefault) != 0 && TryReadEmptyString())
                    {
                        return default;
                    }

                    return ReadParse<Guid>();
            }

            return XConvert.FromObject<Guid>(DirectRead());
        }

        public DateTimeOffset ReadDateTimeOffset()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
                    if ((options & EmptyStringAsDefault) != 0 && TryReadEmptyString())
                    {
                        return default;
                    }

                    return ReadParse<DateTimeOffset>();
            }

            return XConvert.FromObject<DateTimeOffset>(DirectRead());
        }

        public T ReadEnum<T>() where T : struct, Enum
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
                    if ((options & EmptyStringAsDefault) != 0 && TryReadEmptyString())
                    {
                        return default;
                    }


                    var str = InternalReadString();

                    if (EnumHelper.TryParseEnum(str, out T value))
                    {
                        return value;
                    }

                    return (T)Enum.Parse(typeof(T), str.ToStringEx());
                case FixNegative:

                    var psd = NumberHelper.Decimal.ParseInt64(current, Length);

                    if (psd.length != 0 && IsEnding(psd.length))
                    {
                        current += psd.length;

                        EnumHelper.AsEnum<T>((ulong)psd.value);
                    }

                    break;
                case var curr when curr >= FixNumberMin && curr <= FixNumberMax:

                    var upsd = NumberHelper.Decimal.ParseUInt64(current, Length);

                    if (upsd.length != 0 && IsEnding(upsd.length))
                    {
                        current += upsd.length;

                        EnumHelper.AsEnum<T>(upsd.value);
                    }

                    break;
            }
            return XConvert.FromObject<T>(DirectRead());
        }

        Guid IValueReader<Guid>.ReadValue() => ReadGuid();

        DateTimeOffset IValueReader<DateTimeOffset>.ReadValue() => ReadDateTimeOffset();

        public void FastReadObject(IDataWriter<Ps<char>> dataWriter)
        {
            if (IsReferenceMode)
            {
                ReferenceMode.EnterObject(dataWriter);
            }

            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();

                SkipValue();

                return;
            }

            ++current;
            ++depth;

            var count_non_zero = dataWriter.Count > 0;

            if ((options & AsOrderedObjectDeserialize) != 0 && count_non_zero)
            {
                Underlying.As<IDataWriter<string>>(dataWriter).OnWriteAll(this); // dataWriter 一定是 IDataWriter<string>

                switch (*current)
                {
                    case ObjectEnding:
                        goto Return;
                    case ValueEnding:
                        ++current;
                        break;
                }
            }

            Loop:

            SkipWhiteSpace();

            if (IsContent())
            {
                Ps<char> name;

                switch (*current)
                {
                    case ObjectEnding:
                        goto Return;
                    case FixString:
                    case FixChars:
                        name = InternalReadString();
                        break;
                    default:
                        name = InternalReadText();
                        break;
                }

                SkipWhiteSpace();

                if (IsContent() && *current == KeyEnding)
                {
                    ++current;

                    SkipWhiteSpace();

                    if (IsContent())
                    {
                        switch (*current)
                        {
                            case FixNull:
                            case Fixnull:
                                if (count_non_zero && On(IgnoreNull) && Verify(nullString))
                                {
                                    current += nullString.Length;

                                    break;
                                }

                                goto default;
                            case FixNumberMin:
                                if (count_non_zero && On(IgnoreZero) && IsEnding(1))
                                {
                                    current += 1/*length of 0*/;

                                    break;
                                }

                                goto default;
                            default:

                                if (IsReferenceMode)
                                {
                                    ReferenceMode.SetCurrentKey(name);
                                }

                                dataWriter.OnWriteValue(name, this);
                                break;
                        }

                        SkipWhiteSpace();

                        if (IsContent())
                        {
                            switch (*current)
                            {
                                case ObjectEnding:
                                    goto Return;
                                case ValueEnding:
                                    ++current;

                                    goto Loop;
                            }
                        }
                    }
                }
            }

            throw GetException();

            Return:

            --depth;
            ++current;

            if (IsReferenceMode)
            {
                ReferenceMode.LeavaObject();
            }
        }

        public void SlowReadObject(IDataWriter<string> dataWriter)
        {
            if (IsReferenceMode)
            {
                ReferenceMode.EnterObject(dataWriter);
            }

            if (depth >= maxDepth)
            {
                ThrowOutOfDepthException();

                SkipValue();

                return;
            }

            ++current;
            ++depth;

            var count_non_zero = dataWriter.Count > 0;

            if ((options & AsOrderedObjectDeserialize) != 0 && count_non_zero)
            {
                dataWriter.OnWriteAll(this);

                switch (*current)
                {
                    case ObjectEnding:
                        goto Return;
                    case ValueEnding:
                        ++current;
                        break;
                }
            }

            Loop:

            SkipWhiteSpace();

            if (IsContent())
            {
                Ps<char> name;

                switch (*current)
                {
                    case ObjectEnding:
                        goto Return;
                    case FixString:
                    case FixChars:
                        name = InternalReadString();
                        break;
                    default:
                        name = InternalReadText();
                        break;
                }

                SkipWhiteSpace();

                if (IsContent() && *current == KeyEnding)
                {
                    ++current;

                    SkipWhiteSpace();

                    if (IsContent())
                    {
                        switch (*current)
                        {
                            case FixNull:
                            case Fixnull:
                                if (count_non_zero && On(IgnoreNull) && Verify(nullString))
                                {
                                    current += nullString.Length;

                                    break;
                                }

                                goto default;
                            case FixNumberMin:
                                if (count_non_zero && On(IgnoreZero) && IsEnding(1))
                                {
                                    current += 1/*length of 0*/;

                                    break;
                                }

                                goto default;
                            default:

                                var str_name = name.ToStringEx();

                                if (IsReferenceMode)
                                {
                                    ReferenceMode.SetCurrentKey(str_name);
                                }

                                dataWriter.OnWriteValue(str_name, this);
                                break;
                        }

                        SkipWhiteSpace();

                        if (IsContent())
                        {
                            switch (*current)
                            {
                                case ObjectEnding:
                                    goto Return;
                                case ValueEnding:
                                    ++current;

                                    goto Loop;
                            }
                        }
                    }
                }
            }

            throw GetException();

            Return:

            --depth;
            ++current;

            if (IsReferenceMode)
            {
                ReferenceMode.LeavaObject();
            }
        }



        public void SkipValue()
        {
            var depth = 0;

            Loop:

            switch (*current)
            {
                case FixString:
                case FixChars:
                    SkipString();
                    break;
                case Fixtrue:
                case FixTrue:
                    if (!Verify(trueString)) goto default;

                    current += trueString.Length;
                    break;
                case Fixfalse:
                case FixFalse:
                    if (!Verify(falseString)) goto default;

                    current += falseString.Length;
                    break;
                case Fixnull:
                case FixNull:
                    if (!Verify(nullString)) goto default;

                    current += nullString.Length;
                    break;
                case Fixundefined:
                case FixUndefined:
                    if (!Verify(undefinedString)) goto default;

                    current += undefinedString.Length;
                    break;
                case FixArray:
                case FixObject:

                    ++depth;
                    ++current;

                    break;
                case ArrayEnding:
                case ObjectEnding:

                    --depth;
                    ++current;

                    SkipWhiteSpace();
                    break;
                case FixPositive:
                case FixNegative:
                case var digit when digit >= FixNumberMin && digit <= FixNumberMax:
                    ReadDouble();
                    break;
                default:
                    InternalReadText();
                    break;
            }

            SkipWhiteSpace();

            if (IsContent())
            {
                switch (*current)
                {
                    case KeyEnding:
                    case ValueEnding:

                        ++current;

                        SkipWhiteSpace();

                        break;
                }
            }

            if (depth <= 0) return;

            if (current >= end) ThrowException();

            goto Loop;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SkipString()
        {
            var fixEnding = *current;

            for (++current; IsContent(); ++current)
            {
                if (*current == fixEnding)
                {
                    ++current;

                    return;
                }

                if (*current == FixEscape)
                {
                    ++current;

                    if (IsContent() && (*current == FixUnicodeEscape || *current == FixunicodeEscape))
                    {
                        current += 4;
                    }
                }
            }

            throw GetException();
        }

        public string ReadPropertyName()
        {
            Ps<char> name;

            switch (*current)
            {
                case ObjectEnding:
                    goto Exception;
                case FixString:
                case FixChars:
                    name = InternalReadString();
                    break;
                default:
                    name = InternalReadText();
                    break;
            }

            SkipWhiteSpace();

            if (IsContent() && *current == KeyEnding)
            {
                ++current;

                SkipWhiteSpace();

                return name.ToStringEx();
            }

            Exception:

            throw GetException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryReadBeginObject()
        {
            if (IsContent() && *current == FixObject)
            {
                ++current;

                SkipWhiteSpace();

                return true;
            }

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryReadEndObject()
        {
            SkipWhiteSpace();

            if (IsContent())
            {
                switch (*current)
                {
                    case ObjectEnding:

                        ++current;

                        return true;

                    case ValueEnding:

                        ++current;

                        SkipWhiteSpace();

                        if (IsContent() && *current == ObjectEnding)
                        {
                            return true;
                        }

                        return false;
                }
            }

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryReadBeginArray()
        {
            if (IsContent() && *current == FixArray)
            {
                ++current;

                SkipWhiteSpace();

                return true;
            }

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryReadEndArray()
        {
            SkipWhiteSpace();

            if (IsContent())
            {
                switch (*current)
                {
                    case ArrayEnding:

                        ++current;

                        return true;

                    case ValueEnding:

                        ++current;

                        SkipWhiteSpace();

                        if (IsContent() && *current == ArrayEnding)
                        {
                            return true;
                        }

                        return false;
                }
            }

            return false;
        }

        public void MakeTargetedId() { }

        bool[] IValueReader<bool[]>.ReadValue() => ReadArray<bool>();

        byte[] IValueReader<byte[]>.ReadValue() => ReadArray<byte>();

        sbyte[] IValueReader<sbyte[]>.ReadValue() => ReadArray<sbyte>();

        short[] IValueReader<short[]>.ReadValue() => ReadArray<short>();

        ushort[] IValueReader<ushort[]>.ReadValue() => ReadArray<ushort>();

        char[] IValueReader<char[]>.ReadValue() => ReadArray<char>();

        int[] IValueReader<int[]>.ReadValue() => ReadArray<int>();

        uint[] IValueReader<uint[]>.ReadValue() => ReadArray<uint>();

        long[] IValueReader<long[]>.ReadValue() => ReadArray<long>();

        ulong[] IValueReader<ulong[]>.ReadValue() => ReadArray<ulong>();

        float[] IValueReader<float[]>.ReadValue() => ReadArray<float>();

        double[] IValueReader<double[]>.ReadValue() => ReadArray<double>();

        DateTime[] IValueReader<DateTime[]>.ReadValue() => ReadArray<DateTime>();

        DateTimeOffset[] IValueReader<DateTimeOffset[]>.ReadValue() => ReadArray<DateTimeOffset>();

        decimal[] IValueReader<decimal[]>.ReadValue() => ReadArray<decimal>();

        Guid[] IValueReader<Guid[]>.ReadValue() => ReadArray<Guid>();

        string[] IValueReader<string[]>.ReadValue() => ReadArray<string>();

        object[] IValueReader<object[]>.ReadValue() => ReadArray<object>();

        List<bool> IValueReader<List<bool>>.ReadValue() => ReadList<bool>();

        List<byte> IValueReader<List<byte>>.ReadValue() => ReadList<byte>();

        List<sbyte> IValueReader<List<sbyte>>.ReadValue() => ReadList<sbyte>();

        List<short> IValueReader<List<short>>.ReadValue() => ReadList<short>();

        List<ushort> IValueReader<List<ushort>>.ReadValue() => ReadList<ushort>();

        List<char> IValueReader<List<char>>.ReadValue() => ReadList<char>();

        List<int> IValueReader<List<int>>.ReadValue() => ReadList<int>();

        List<uint> IValueReader<List<uint>>.ReadValue() => ReadList<uint>();

        List<long> IValueReader<List<long>>.ReadValue() => ReadList<long>();

        List<ulong> IValueReader<List<ulong>>.ReadValue() => ReadList<ulong>();

        List<float> IValueReader<List<float>>.ReadValue() => ReadList<float>();

        List<double> IValueReader<List<double>>.ReadValue() => ReadList<double>();

        List<DateTime> IValueReader<List<DateTime>>.ReadValue() => ReadList<DateTime>();

        List<DateTimeOffset> IValueReader<List<DateTimeOffset>>.ReadValue() => ReadList<DateTimeOffset>();

        List<decimal> IValueReader<List<decimal>>.ReadValue() => ReadList<decimal>();

        List<Guid> IValueReader<List<Guid>>.ReadValue() => ReadList<Guid>();

        List<string> IValueReader<List<string>>.ReadValue() => ReadList<string>();

        List<object> IValueReader<List<object>>.ReadValue() => ReadList<object>();

        void IDataReader<string>.OnReadValue(string key, IValueWriter valueWriter)
        {
            valueWriter.DirectWrite(this[key].DirectRead());
        }

        void IDataReader<string>.OnReadAll(IDataWriter<string> dataWriter)
        {
            // TODO: 
            throw new NotSupportedException();
        }
    }
}