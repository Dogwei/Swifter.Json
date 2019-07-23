using Swifter.Formatters;
using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using static Swifter.Json.JsonCode;

namespace Swifter.Json
{
    sealed unsafe class JsonDeserializer<TMode> :
        IJsonReader,
        IFormatterReader,
        IValueReader<Guid>,
        IValueReader<DateTimeOffset>,
        IUsePool
        where TMode : struct
    {
        const NumberStyles NumberStyle =
            NumberStyles.AllowExponent |
            NumberStyles.AllowDecimalPoint |
            NumberStyles.AllowLeadingSign;


        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T FastReadParse<T>(char* chars, int length)
        {
            if (typeof(T) == typeof(DateTime))
            {
                if (DateTimeHelper.TryParseISODateTime(chars, length, out DateTime date_time))
                {
                    return Unsafe.As<DateTime, T>(ref date_time);
                }
            }
            else if (typeof(T) == typeof(DateTimeOffset))
            {
                if (DateTimeHelper.TryParseISODateTime(chars, length, out DateTimeOffset date_time_offset))
                {
                    return Unsafe.As<DateTimeOffset, T>(ref date_time_offset);
                }
            }
            else if (typeof(T) == typeof(Guid))
            {
                if (NumberHelper.TryParse(chars, length, out Guid guid) != 0)
                {
                    return Unsafe.As<Guid, T>(ref guid);
                }
            }
            else if (typeof(T) == typeof(long))
            {
                if (NumberHelper.Decimal.TryParse(chars, length, out long value) == length)
                {
                    return Unsafe.As<long, T>(ref value);
                }
                else
                {
                    var numberInfo = NumberHelper.GetNumberInfo(chars, length, 10);

                    if (numberInfo.IsNumber && numberInfo.End == length)
                    {
                        return Unsafe.As<long, T>(ref Unsafe.AsRef(numberInfo.ToInt64()));
                    }
                }
            }
            else if (typeof(T) == typeof(ulong))
            {
                if (NumberHelper.Decimal.TryParse(chars, length, out ulong value) == length)
                {
                    return Unsafe.As<ulong, T>(ref value);
                }
                else
                {
                    var numberInfo = NumberHelper.GetNumberInfo(chars, length, 10);

                    if (numberInfo.IsNumber && numberInfo.End == length)
                    {
                        return Unsafe.As<ulong, T>(ref Unsafe.AsRef(numberInfo.ToUInt64()));
                    }
                }
            }
            else if (typeof(T) == typeof(double))
            {
                if (NumberHelper.Decimal.TryParse(chars, length, out double value) == length)
                {
                    return Unsafe.As<double, T>(ref value);
                }
                else
                {
                    var numberInfo = NumberHelper.GetNumberInfo(chars, length, 10);

                    if (numberInfo.IsNumber && numberInfo.End == length)
                    {
                        return Unsafe.As<double, T>(ref Unsafe.AsRef(numberInfo.ToDouble()));
                    }
                }
            }
            else if (typeof(T) == typeof(decimal))
            {
                if (NumberHelper.TryParse(chars, length, out decimal value) == length)
                {
                    return Unsafe.As<decimal, T>(ref value);
                }
                else
                {
                    var numberInfo = NumberHelper.GetNumberInfo(chars, length, 10);

                    if (numberInfo.IsNumber && numberInfo.End == length)
                    {
                        return Unsafe.As<decimal, T>(ref Unsafe.AsRef(numberInfo.ToDecimal()));
                    }
                }
            }
            else if (typeof(T) == typeof(RWPathInfo))
            {
                return (T)(object)ParseReference(chars, length);
            }

            return SlowReadParse<T>(new string(chars, 0, length));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T SlowReadParse<T>(string str)
        {
            if (typeof(T) == typeof(DateTime))
            {
                return Unsafe.As<DateTime, T>(ref Unsafe.AsRef(DateTime.Parse(str)));
            }
            else if (typeof(T) == typeof(DateTimeOffset))
            {
                return Unsafe.As<DateTimeOffset, T>(ref Unsafe.AsRef(DateTimeOffset.Parse(str)));
            }
            else if (typeof(T) == typeof(Guid))
            {
                return Unsafe.As<Guid, T>(ref Unsafe.AsRef(new Guid(str)));
            }
            else if (typeof(T) == typeof(long))
            {
                return Unsafe.As<long, T>(ref Unsafe.AsRef(long.Parse(str, NumberStyle)));
            }
            else if (typeof(T) == typeof(ulong))
            {
                return Unsafe.As<ulong, T>(ref Unsafe.AsRef(ulong.Parse(str, NumberStyle)));
            }
            else if (typeof(T) == typeof(double))
            {
                return Unsafe.As<double, T>(ref Unsafe.AsRef(double.Parse(str, NumberStyle)));
            }
            else if (typeof(T) == typeof(decimal))
            {
                return Unsafe.As<decimal, T>(ref Unsafe.AsRef(decimal.Parse(str, NumberStyle)));
            }
            else if (typeof(T) == typeof(RWPathInfo))
            {
                fixed (char* pStr = str)
                {
                    return (T)(object)ParseReference(pStr, str.Length);
                }
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static RWPathInfo ParseReference(char* chars, int length)
        {
            var reference = RWPathInfo.Root;

            // i: Index;
            // j: Item start;
            // k: Item length;
            for (int i = 0, j = 0, k = 0; ; i++)
            {
                if (i < length)
                {
                    switch (chars[i])
                    {
                        case '/':
                            break;
                        case '%':

                            if (i + 2 < length)
                            {
                                GetDigital(chars[++i]);
                                GetDigital(chars[++i]);

                                continue;
                            }

                            throw new FormatException();
                        default:
                            ++k;
                            continue;
                    }
                }

                if (j < length)
                {
                    var item = i - j == k ? StringHelper.ToString(chars + j, k) : GetItem(j, i, k);

                    j = i + 1;
                    k = 0;

                    switch (item)
                    {
                        case ReferenceRootPathName:
                        case ReferenceRootPathName2:
                            if (reference.IsRoot)
                            {
                                continue;
                            }
                            break;
                    }

                    if (item.Length != 0 && item[0] >= FixNumberMin && item[0] <= FixNumberMax && int.TryParse(item, out var result))
                    {
                        reference = RWPathInfo.Create(result, reference);
                    }
                    else
                    {
                        reference = RWPathInfo.Create(item, reference);
                    }

                    continue;
                }

                break;
            }

            return reference;


            string GetItem(int start, int end, int count)
            {
                var bytesCount = (end - start - count) / 3;

                var bytes = stackalloc byte[bytesCount];

                for (int i = start, j = 0; i < end; i++)
                {
                    if (chars[i] == '%')
                    {
                        bytes[j++] = (byte)((GetDigital(chars[++i]) << 4) | GetDigital(chars[++i]));
                    }
                }

                var charsCount = Encoding.UTF8.GetCharCount(bytes, bytesCount);

                var str = StringHelper.MakeString(charsCount + count);

                fixed (char* pStr = str)
                {
                    for (int i = start, j = 0, k = 0; i < end; i++)
                    {
                        if (chars[i] == '%')
                        {
                            var l = 1;

                            for (i += 3; i < end && chars[i] == '%'; i += 3, ++l) ;

                            --i;

                            j += Encoding.UTF8.GetChars(bytes + k, l, pStr + j, charsCount);

                            k += l;
                        }
                        else
                        {
                            pStr[j] = chars[i];

                            ++j;
                        }
                    }
                }

                return str;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetDigital(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return c - '0';
            }

            if (c >= 'a' && c <= 'f')
            {
                return c - 'a' + 10;
            }

            if (c >= 'A' && c <= 'F')
            {
                return c - 'A' + 10;
            }

            throw new FormatException($"Hex : 0x{c}");
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ref char GetRawStringData(string str, out int length)
        {
            length = str.Length;

            return ref StringHelper.GetRawStringData(str);
        }


        readonly JsonFormatter jsonFormatter;

        public JsonDeserializer(JsonFormatter jsonFormatter, char* chars, int length) : this(chars, length)
        {
            this.jsonFormatter = jsonFormatter;
        }

        public JsonDeserializer(char* chars, int length)
        {
            begin = chars;
            end = chars + length;

            current = chars;
        }

        TMode mode;

        char* current;
        readonly char* begin;
        readonly char* end;

        int length => (int)(end - current);

        public ref JsonDeserializeModes.Reference ReferenceMode => ref Unsafe.As<TMode, JsonDeserializeModes.Reference>(ref mode);

        public long TargetedId => jsonFormatter?.id ?? 0;

        public bool IsObject => current < end && *current == FixObject;

        public bool IsArray => current < end && *current == FixArray;

        public bool IsValue => current < end && !IsObject && !IsArray;

        public bool IsString => current < end && (*current == FixString || *current == FixChars);

        public bool IsNumber => current < end && ((*current >= FixNumberMin && *current <= FixNumberMax) || *current == FixNegative || *current == FixPositive);

        public bool IsNull => current < end && (*current == Fixnull || *current == FixNull);

        public bool IsBoolean => current < end && (*current == Fixtrue || *current == FixTrue || *current == Fixfalse || *current == FixFalse);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public object DirectRead()
        {
            switch (*current)
            {
                case FixString:
                    return InternalReadString();
                case FixChars:
                    return InternalReadChars();
                case FixArray:
                    return ValueInterface<List<object>>.ReadValue(this);
                case FixObject:
                    if (typeof(TMode) == typeof(JsonDeserializeModes.Reference))
                    {
                        if (IsReference())
                        {
                            return ReadReference();
                        }
                    }
                    return ValueInterface<Dictionary<string, object>>.ReadValue(this);
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
                default:
                    if (*current >= FixNumberMin && *current <= FixNumberMax)
                    {
                        goto Number;
                    }

                    break;
            }

        Text:

            return InternalReadText();

        Number:

            var numberInfo = NumberHelper.GetNumberInfo(current, length, 10);

            if (IsEnding(numberInfo.End))
            {
                current += numberInfo.End;

                if (numberInfo.HaveExponent)
                {
                    if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 16 && numberInfo.ExponentCount <= 3)
                    {
                        return numberInfo.ToDouble();
                    }

                    return numberInfo.ToString();
                }

                if (numberInfo.IsFloat)
                {
                    if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 16)
                    {
                        return numberInfo.ToDouble();
                    }

                    if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 28 && numberInfo.IsDecimal)
                    {
                        return numberInfo.ToDecimal();
                    }

                    return numberInfo.ToString();
                }

                if (numberInfo.IntegerCount <= 18)
                {
                    var int64 = numberInfo.ToInt64();

                    if (int64 <= int.MaxValue && int64 >= int.MinValue)
                    {
                        return (int)int64;
                    }

                    return int64;
                }

                if (numberInfo.IntegerCount <= 28 && numberInfo.IsDecimal)
                {
                    return numberInfo.ToDecimal();
                }

                return numberInfo.ToString();
            }

            goto Text;
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
                    if (typeof(TMode) == typeof(JsonDeserializeModes.Reference))
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
                default:
                    SetContent(dataWriter, DirectRead());
                    return;
            }

            dataWriter.Initialize();

            if (typeof(TMode) == typeof(JsonDeserializeModes.Reference))
            {
                if (ReferenceMode.IsRoot)
                {
                    ReadRoot(dataWriter);

                    return;
                }
            }

            NoInliningReadArray(dataWriter);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SkipWhiteSpace()
        {
            if (typeof(TMode) != typeof(JsonDeserializeModes.Deflate))
            {
                if (current < end && *current <= 0x20)
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
                    if (current < end)
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
                return false;
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

            if (typeof(TMode) == typeof(JsonDeserializeModes.Deflate))
            {
                return false;
            }
            else if (typeof(TMode) == typeof(JsonDeserializeModes.Standard))
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
            if (typeof(TMode) == typeof(JsonDeserializeModes.Deflate))
            {
                return true;
            }

            if (!IsEnding(lowerstr.Length))
            {
                return false;
            }

            for (int i = 0; i < lowerstr.Length; i++)
            {
                if (StringHelper.ToLower(current[i]) != lowerstr[i])
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
                return curr != FixNumberMin;
            }

            return Convert.ToBoolean(DirectRead());
        }

        public byte ReadByte() => checked((byte)ReadUInt64());

        public char ReadChar()
        {
            if (length >= 3)
            {
                if (current[0] == FixString && current[2] == FixString && current[1] != FixEscape)
                {
                    return current[1];
                }

                if (current[0] == FixChars && current[2] == FixChars && current[1] != FixEscape)
                {
                    return current[1];
                }
            }

            return char.Parse(ReadString());
        }

        public DateTime ReadDateTime()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
                    return ReadParse<DateTime>();
            }

            return Convert.ToDateTime(DirectRead());
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T ReadParse<T>()
        {
            var endingChar = *current;
            var length = 0;

            for (var curr = current + 1; curr < end; ++curr, ++length)
            {
                var chr = *curr;

                if (chr == endingChar)
                {
                    var result = FastReadParse<T>(current + 1, length);

                    current = curr + 1;

                    return result;
                }

                if (chr == FixEscape)
                {
                    return SlowReadParse<T>(InternalReadString());
                }
            }

            throw GetException();
        }

        public decimal ReadDecimal()
        {
            var count = NumberHelper.TryParse(current, length, out decimal value);

            if (count != 0 && IsEnding(count))
            {
                current += count;

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
                    return ReadParse<decimal>();
            }

            var numberInfo = NumberHelper.GetNumberInfo(current, length, 10);

            if (numberInfo.IsNumber)
            {
                if (IsEnding(numberInfo.End))
                {
                    current += numberInfo.End;

                    return numberInfo.ToDecimal();
                }

                return decimal.Parse(InternalReadText(), NumberStyle);
            }

            return Convert.ToDecimal(DirectRead());
        }

        public double ReadDouble()
        {
            var count = NumberHelper.Decimal.TryParse(current, length, out double value);

            if (count != 0 && IsEnding(count))
            {
                current += count;

                return value;
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
                    return ReadParse<double>();
            }

            var numberInfo = NumberHelper.GetNumberInfo(current, length, 10);

            if (numberInfo.IsNumber)
            {
                if (IsEnding(numberInfo.End))
                {
                    current += numberInfo.End;

                    return numberInfo.ToDouble();
                }

                return double.Parse(InternalReadText(), NumberStyle);
            }

            return Convert.ToDouble(DirectRead());
        }

        public short ReadInt16() => checked((short)ReadInt64());

        public int ReadInt32() => checked((int)ReadInt64());

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ReadInt64()
        {
            var count = NumberHelper.Decimal.TryParse(current, length, out long value);

            if (count != 0 && IsEnding(count))
            {
                current += count;

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
                    return ReadParse<long>();
            }

            var numberInfo = NumberHelper.GetNumberInfo(current, length, 10);

            if (numberInfo.IsNumber)
            {
                if (IsEnding(numberInfo.End))
                {
                    current += numberInfo.End;

                    return numberInfo.ToInt64();
                }

                return long.Parse(InternalReadText(), NumberStyle);
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
            }

            if (typeof(T) == typeof(int)) return As(ReadInt32());
            else if (typeof(T) == typeof(double)) return As(ReadDouble());
            else if (typeof(T) == typeof(long)) return As(ReadInt64());
            else if (typeof(T) == typeof(bool)) return As(ReadBoolean());
            else if (typeof(T) == typeof(byte)) return As(ReadByte());
            else if (typeof(T) == typeof(sbyte)) return As(ReadSByte());
            else if (typeof(T) == typeof(short)) return As(ReadInt16());
            else if (typeof(T) == typeof(ushort)) return As(ReadUInt16());
            else if (typeof(T) == typeof(uint)) return As(ReadUInt32());
            else if (typeof(T) == typeof(ulong)) return As(ReadUInt64());
            else if (typeof(T) == typeof(char)) return As(ReadChar());
            else if (typeof(T) == typeof(float)) return As(ReadSingle());
            else if (typeof(T) == typeof(double)) return As(ReadDouble());
            else if (typeof(T) == typeof(decimal)) return As(ReadDecimal());
            else if (typeof(T) == typeof(Guid)) return As(ReadGuid());
            else if (typeof(T) == typeof(DateTime)) return As(ReadDateTime());
            else if (typeof(T) == typeof(DateTimeOffset)) return As(ReadDateTimeOffset());

            return ValueInterface<T>.ReadValue(this);

            T As<TIn>(TIn source) => Unsafe.As<TIn, T>(ref Unsafe.AsRef(source));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool IsReference()
        {
            if (length < 8)
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

            if (current < end)
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

            if (res && current < end && *current == KeyEnding)
            {
                ++current;

                SkipWhiteSpace();
            }

            return res;
        }

        public static void SetContent(IDataWriter dataWriter, object content)
        {
            if (content != null)
            {
                RWHelper.SetContent(dataWriter, content);
            }
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
                case FixArray:
                    ReadArray(dataWriter.As<int>());
                    return;
                case FixObject:
                    if (typeof(TMode) == typeof(JsonDeserializeModes.Reference))
                    {
                        if (IsReference())
                        {
                            goto default;
                        }
                    }
                    break;
                default:
                    SetContent(dataWriter, DirectRead());
                    return;
            }

            dataWriter.Initialize();

            if (typeof(TMode) == typeof(JsonDeserializeModes.Reference))
            {
                if (ReferenceMode.IsRoot)
                {
                    ReadRoot(dataWriter);

                    return;
                }
            }

            if (dataWriter is IId64DataRW<char> fastWriter)
            {
                FastReadObject(fastWriter);
            }
            else
            {
                SlowReadObject(dataWriter);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void ReadRoot(IDataWriter dataWriter)
        {
            ReferenceMode.SetRoot(dataWriter);

            if (dataWriter is IId64DataRW<char> fastObjectWriter)
            {
                FastReadObject(fastObjectWriter);
            }
            else
            if (dataWriter is IDataWriter<string> slowObjectWriter)
            {
                SlowReadObject(slowObjectWriter);
            }
            else if (dataWriter is IDataWriter<int> arrayWriter)
            {
                NoInliningReadArray(arrayWriter);
            }
            else
            {
                throw new NotSupportedException();
            }

            ReferenceMode.Process();
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

        public string ReadString()
        {
            switch (*current)
            {
                case FixString:
                    return InternalReadString();
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

            if (!(current < end && *current == ObjectEnding))
            {
                goto Exception;
            }

            ++current;

            if (reference == null)
            {
                return null;
            }

            return ReferenceMode.CreateJsonReference(reference).value;

        Exception:

            throw new NotSupportedException("Not a reference format.");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string NoInliningReadString()
        {
            switch (*current)
            {
                case FixChars:
                    return InternalReadChars();
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
                default:
                    if (*current >= FixNumberMin && *current <= FixNumberMax)
                    {
                        goto Number;
                    }
                    break;
            }

        Text:

            return InternalReadText();

        Number:

            var numberInfo = NumberHelper.GetNumberInfo(current, length, 10);

            if (IsEnding(numberInfo.End))
            {
                current += numberInfo.End;

                if (numberInfo.HaveExponent)
                {
                    if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 16 && numberInfo.ExponentCount <= 3)
                    {
                        return numberInfo.ToDouble().ToString();
                    }

                    return numberInfo.ToString();
                }

                if (numberInfo.IsFloat)
                {
                    if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 16)
                    {
                        return numberInfo.ToDouble().ToString();
                    }

                    if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 28 && numberInfo.IsDecimal)
                    {
                        return numberInfo.ToDecimal().ToString();
                    }

                    return numberInfo.ToString();
                }

                if (numberInfo.IntegerCount <= 18)
                {
                    return numberInfo.ToInt64().ToString();
                }

                if (numberInfo.IntegerCount <= 28 && numberInfo.IsDecimal)
                {
                    return numberInfo.ToDecimal().ToString();
                }

                return numberInfo.ToString();
            }

            goto Text;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public Exception GetException()
        {
            int line = 1;
            var lineBegin = begin;

            if (current >= end)
            {
                current = end;
            }

            for (var i = begin; i <= current; ++i)
            {
                if (*i == '\n')
                {
                    ++line;
                    lineBegin = i;
                }
            }

            var column = (int)(current - lineBegin + 1);
            var exception = new JsonDeserializeException
            {
                Line = line,
                Column = column,
                Index = (int)(current - begin),
                Text = (*current).ToString()
            };

            return exception;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string InternalReadText()
        {
            var temp = current;

            while (current < end)
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

            if (typeof(TMode) == typeof(JsonDeserializeModes.Deflate))
            {
                return StringHelper.ToString(temp, (int)(current - temp));
            }
            else
            {
                return StringHelper.Trim(temp, (int)(current - temp));
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string InternalReadString()
        {
            var escapes = 0;

            for (var curr = (++current); curr < end; ++curr)
            {
                switch (*curr)
                {
                    case FixString:

                        /* 内容没有转义符，直接截取返回。 */
                        if (escapes == 0)
                        {
                            return new string(current++, 0, -(int)(current - (current = curr + 1)));
                        }

                        return InternalReadEscapeString(curr, escapes);

                    case FixEscape:

                        ++curr;
                        ++escapes;

                        if (curr < end && (*curr == FixUnicodeEscape || *curr == FixunicodeEscape))
                        {
                            curr += 4;
                            escapes += 4;
                        }

                        break;
                }
            }

            throw GetException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string InternalReadChars()
        {
            var escapes = 0;

            for (var curr = (++current); curr < end; ++curr)
            {
                switch (*curr)
                {
                    case FixChars:

                        /* 内容没有转义符，直接截取返回。 */
                        if (escapes == 0)
                        {
                            return new string(current++, 0, -(int)(current - (current = curr + 1)));
                        }

                        return InternalReadEscapeString(curr, escapes);

                    case FixEscape:

                        ++curr;
                        ++escapes;

                        if (curr < end && (*curr == FixUnicodeEscape || *curr == FixunicodeEscape))
                        {
                            curr += 4;
                            escapes += 4;
                        }

                        break;
                }
            }

            throw GetException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string InternalReadEscapeString(char* curr, int escapes)
        {
            var length = ((int)(curr - current)) - escapes;

            var str = StringHelper.MakeString(length);

            fixed (char* chars = str)
            {
                for (var pStr = chars; current < curr; ++current, ++pStr)
                {
                    var chr = *current;

                    if (chr == FixEscape)
                    {
                        ++current;

                        switch (*current)
                        {
                            case EscapedWhiteChar2:
                                chr = WhiteChar2;
                                break;
                            case EscapedWhiteChar3:
                                chr = WhiteChar3;
                                break;
                            case EscapedWhiteChar4:
                                chr = WhiteChar4;
                                break;
                            case EscapedWhiteChar5:
                                chr = WhiteChar5;
                                break;
                            case EscapedWhiteChar6:
                                chr = WhiteChar6;
                                break;
                            case FixunicodeEscape:
                            case FixUnicodeEscape:

                                chr = (char)(
                                    (GetDigital(*++current) << 12) |
                                    (GetDigital(*++current) << 8) |
                                    (GetDigital(*++current) << 4) |
                                    (GetDigital(*++current)));

                                break;
                            default:
                                chr = *current;
                                break;
                        }
                    }

                    *pStr = chr;
                }
            }

            ++current;

            return str;
        }

        public ushort ReadUInt16() => checked((ushort)ReadUInt64());

        public uint ReadUInt32() => checked((uint)ReadUInt64());

        public ulong ReadUInt64()
        {
            var count = NumberHelper.Decimal.TryParse(current, length, out ulong value);

            if (count != 0 && IsEnding(count))
            {
                current += count;

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
                    return ReadParse<ulong>();
            }

            var numberInfo = NumberHelper.GetNumberInfo(current, length, 10);

            if (numberInfo.IsNumber)
            {
                if (IsEnding(numberInfo.End))
                {
                    current += numberInfo.End;

                    return numberInfo.ToUInt64();
                }

                return ulong.Parse(InternalReadText(), NumberStyle);
            }

            return Convert.ToUInt64(DirectRead());
        }

        public Guid ReadGuid()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
                    return ReadParse<Guid>();
            }

            return new Guid(ReadString());
        }

        public DateTimeOffset ReadDateTimeOffset()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
                    return ReadParse<DateTimeOffset>();
            }

            return (DateTimeOffset)Convert.ChangeType(DirectRead(), typeof(DateTimeOffset));
        }

        Guid IValueReader<Guid>.ReadValue() => ReadGuid();

        DateTimeOffset IValueReader<DateTimeOffset>.ReadValue() => ReadDateTimeOffset();

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ref char InternalReadString(out int length)
        {
            var escapes = 0;

            for (var curr = (++current); curr < end; ++curr)
            {
                switch (*curr)
                {
                    case FixString:

                        /* 内容没有转义符，直接截取返回。 */
                        if (escapes == 0)
                        {
                            length = (int)(curr - current);

                            current = curr + 1;

                            return ref curr[-length];
                        }

                        return ref GetRawStringData(InternalReadEscapeString(curr, escapes), out length);

                    case FixEscape:

                        ++curr;
                        ++escapes;

                        if (curr < end && (*curr == FixUnicodeEscape || *curr == FixunicodeEscape))
                        {
                            curr += 4;
                            escapes += 4;
                        }

                        break;
                }
            }

            throw GetException();
        }

        public void FastReadObject(IId64DataRW<char> dataWriter)
        {
            ++current;

        Loop:

            SkipWhiteSpace();

            if (current < end)
            {
                long id64;

                switch (*current)
                {
                    case ObjectEnding:
                        goto Return;
                    case FixString:
                        id64 = dataWriter.GetId64(ref InternalReadString(out var length), length);
                        break;
                    case FixChars:
                        id64 = dataWriter.GetId64Ex(InternalReadChars());
                        break;
                    default:
                        id64 = dataWriter.GetId64Ex(InternalReadText());
                        break;
                }

                SkipWhiteSpace();

                if (current < end && *current == KeyEnding)
                {
                    ++current;

                    SkipWhiteSpace();

                    if (current < end)
                    {
                        dataWriter.OnWriteValue(id64, this);

                        if (typeof(TMode) == typeof(JsonDeserializeModes.Reference))
                        {
                            if (ReferenceMode.IsJsonReference)
                            {
                                ReferenceMode.SetItem(dataWriter, RWPathInfo.Create(id64));
                            }
                        }

                        SkipWhiteSpace();

                        if (current < end)
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

            ++current;
        }

        public void SlowReadObject(IDataWriter<string> dataWriter)
        {
            ++current;

        Loop:

            SkipWhiteSpace();

            if (current < end)
            {
                string name;

                switch (*current)
                {
                    case ObjectEnding:
                        goto Return;
                    case FixString:
                        name = InternalReadString();
                        break;
                    case FixChars:
                        name = InternalReadChars();
                        break;
                    default:
                        name = InternalReadText();
                        break;
                }

                SkipWhiteSpace();

                if (current < end && *current == KeyEnding)
                {
                    ++current;

                    SkipWhiteSpace();

                    if (current < end)
                    {
                        dataWriter.OnWriteValue(name, this);

                        if (typeof(TMode) == typeof(JsonDeserializeModes.Reference))
                        {
                            if (ReferenceMode.IsJsonReference)
                            {
                                ReferenceMode.SetItem(dataWriter, RWPathInfo.Create(name));
                            }
                        }

                        SkipWhiteSpace();

                        if (current < end)
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

            ++current;
        }

        public void NoInliningReadArray(IDataWriter<int> dataWriter)
        {
            var index = 0;

            ++current;

        Loop:

            SkipWhiteSpace();

            if (current < end)
            {
                if (*current == ArrayEnding)
                {
                    goto Return;
                }

                dataWriter.OnWriteValue(index, this);

                if (typeof(TMode) == typeof(JsonDeserializeModes.Reference))
                {
                    if (ReferenceMode.IsJsonReference)
                    {
                        ReferenceMode.SetItem(dataWriter, RWPathInfo.Create(index));
                    }
                }

                ++index;

                SkipWhiteSpace();

                if (current < end)
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

            ++current;
        }

        public void SkipValue()
        {
            switch (*current)
            {
                case FixString:
                case FixChars:
                    SkipString();
                    return;
                case FixArray:
                    foreach (var item in ReadArray())
                    {
                        item.SkipValue();
                    }
                    return;
                case FixObject:
                    foreach (var item in ReadObject())
                    {
                        item.Value.SkipValue();
                    }
                    return;
                case Fixtrue:
                case FixTrue:
                    if (Verify(trueString))
                    {
                        current += trueString.Length;

                        return;
                    }
                    break;
                case Fixfalse:
                case FixFalse:
                    if (Verify(falseString))
                    {
                        current += falseString.Length;

                        return;
                    }
                    break;
                case Fixnull:
                case FixNull:
                    if (Verify(nullString))
                    {
                        current += nullString.Length;

                        return;
                    }
                    break;
                case Fixundefined:
                case FixUndefined:
                    if (Verify(undefinedString))
                    {
                        current += undefinedString.Length;

                        return;
                    }
                    break;
                case FixPositive:
                case FixNegative:
                    goto Number;
                default:
                    if (*current >= FixNumberMin && *current <= FixNumberMax)
                    {
                        goto Number;
                    }
                    break;
            }

            InternalReadText();

            return;

        Number:

            ReadDouble();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SkipString()
        {
            var fixEnding = *current;

            for (++current; current < end; ++current)
            {
                if (*current == fixEnding)
                {
                    ++current;

                    return;
                }

                if (*current == FixEscape)
                {
                    ++current;

                    if (current < end && (*current == FixUnicodeEscape || *current == FixunicodeEscape))
                    {
                        current += 4;
                    }
                }
            }

            throw GetException();
        }

        public IEnumerable<KeyValuePair<string, IJsonReader>> ReadObject()
        {
            if (!IsObject)
            {
                throw new NotSupportedException("The JSON value not an object.");
            }

            if (TryReadBeginObject())
            {
                while (!TryReadEndObject())
                {
                    yield return new KeyValuePair<string, IJsonReader>(ReadPropertyName(), this);
                }
            }
        }

        public IEnumerable<IJsonReader> ReadArray()
        {
            if (!IsArray)
            {
                throw new NotSupportedException("The JSON value not an array.");
            }

            if (TryReadBeginArray())
            {
                while (!TryReadEndArray())
                {
                    yield return this;
                }
            }
        }

        public string ReadPropertyName()
        {
            string name;

            switch (*current)
            {
                case ObjectEnding:
                    goto Exception;
                case FixString:
                    name = InternalReadString();
                    break;
                case FixChars:
                    name = InternalReadChars();
                    break;
                default:
                    name = InternalReadText();
                    break;
            }

            SkipWhiteSpace();

            if (current < end && *current == KeyEnding)
            {
                ++current;

                SkipWhiteSpace();

                return name;
            }

        Exception:

            throw GetException();
        }

        public ref readonly char ReadPropertyName(out int length)
        {
            ref char result = ref Unsafe.AsRef<char>((void*)null);

            switch (*current)
            {
                case ObjectEnding:
                    goto Exception;
                case FixString:
                    result = ref InternalReadString(out length);
                    break;
                case FixChars:
                    result = ref GetRawStringData(InternalReadChars(), out length);
                    break;
                default:
                    result = ref GetRawStringData(InternalReadText(), out length);
                    break;
            }

            SkipWhiteSpace();

            if (current < end && *current == KeyEnding)
            {
                ++current;

                SkipWhiteSpace();

                return ref result;
            }

        Exception:

            throw GetException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryReadBeginObject()
        {
            if (IsObject)
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

            if (current <end)
            {
                switch (*current)
                {
                    case ObjectEnding:

                        ++current;

                        return true;

                    case ValueEnding:

                        ++current;

                        SkipWhiteSpace();

                        if (current < end && *current == ObjectEnding)
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
            if (IsArray)
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

            if (current < end)
            {
                switch (*current)
                {
                    case ArrayEnding:

                        ++current;

                        return true;

                    case ValueEnding:

                        ++current;

                        SkipWhiteSpace();

                        if (current < end && *current == ArrayEnding)
                        {
                            return true;
                        }

                        return false;
                }
            }

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SkipPropertyName()
        {
            switch (*current)
            {
                case ObjectEnding:
                    goto Exception;
                case FixString:
                case FixChars:
                    SkipString();
                    break;
                default:
                    InternalReadText();
                    break;
            }

            SkipWhiteSpace();

            if (current < end && *current == KeyEnding)
            {
                ++current;

                SkipWhiteSpace();

                return;
            }

        Exception:

            throw GetException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ref readonly char ReadString(out int length)
        {
            switch (*current)
            {
                case FixString:
                    return ref InternalReadString(out length);
                case Fixnull:
                case FixNull:
                    if (Verify(nullString))
                    {
                        current += nullString.Length;

                        length = 0;

                        return ref Unsafe.AsRef<char>((void*)null);
                    }
                    break;
            }

            return ref GetRawStringData(NoInliningReadString(), out length);
        }

        public void MakeTargetedId() { }
    }
}