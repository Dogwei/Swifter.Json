using Swifter.Formatters;
using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    internal abstract unsafe class BaseJsonDeserializer : IFormatReader, IValueReader<Guid>, IValueReader<DateTimeOffset>, ISingleThreadOptimize
    {
        const string TrueString = "true";
        const string FalseString = "false";
        const string NullString = "null";
        const string UndefinedString = "undefined";

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IsExp(char c)
        {
            switch (c)
            {
                case NumberHelper.ExponentSign:
                case NumberHelper.exponentSign:
                case NumberHelper.HexSign:
                case NumberHelper.hexSign:
                case NumberHelper.BinarySign:
                case NumberHelper.binarySign:
                case NumberHelper.DotSign:
                case NumberHelper.SplitSign:
                    return true;
            }

            return false;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool IsEnding(int offset)
        {
            var swap = current;

            current += offset;

            SkipWhiteSpace();

            char c = default;

            if (current < end)
            {
                c = *current;
            }

            current = swap;

            switch (c)
            {
                case '}':
                case ']':
                case ',':
                    return true;
                default:
                    return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool EqualsByLower(string lowerstr)
        {
            var comparison = Length - lowerstr.Length;

            if (comparison < 0)
            {
                return false;
            }

            if (comparison > 0 && !IsEnding(lowerstr.Length))
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

        public const NumberStyles NumberStyle = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;

        public char* current;
        public readonly char* begin;
        public readonly char* end;

        public int Length => (int)(end - current);

        public JsonFormatter jsonFormatter;

        public long Id => jsonFormatter?.id ?? 0;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public BaseJsonDeserializer(char* chars, int length)
        {
            current = chars;

            begin = chars;
            end = chars + length;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void InternalSkipWhiteSpace()
        {
            Loop:

            switch (*current)
            {
                case ' ':
                case '\b':
                case '\f':
                case '\n':
                case '\t':
                case '\r':
                    ++current;
                    if (current < end)
                    {
                        goto Loop;
                    }
                    break;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SkipWhiteSpace()
        {
            if (current < end && *current <= 0x20)
            {
                InternalSkipWhiteSpace();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string InternalReadEscapeString(char* text_end, int text_length)
        {
            var text = StringHelper.MakeString(text_length);

            fixed (char* ptr = text)
            {
                for (var current = ptr; this.current < text_end; ++current, ++this.current)
                {
                    if (*this.current == '\\')
                    {
                        ++this.current;

                        switch (*this.current)
                        {
                            case 'b':
                                *current = '\b';
                                continue;
                            case 'f':
                                *current = '\f';
                                continue;
                            case 'n':
                                *current = '\n';
                                continue;
                            case 't':
                                *current = '\t';
                                continue;
                            case 'r':
                                *current = '\r';
                                continue;
                            case 'u':

                                *current = (char)(
                                    (GetDigital(this.current[1]) << 12) |
                                    (GetDigital(this.current[2]) << 8) |
                                    (GetDigital(this.current[3]) << 4) |
                                    (GetDigital(this.current[4])));

                                this.current += 4;

                                continue;
                        }
                    }

                    *current = *this.current;
                }
            }

            ++current;

            return text;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string InternalReadString()
        {
            const char escape_char = '\\';
            const char unicode_char = 'u';

            var text_char = *current;
            var text_length = 0;

            for (var curr = (++current); curr < end; ++curr, ++text_length)
            {
                var current_char = *curr;

                if (current_char == text_char)
                {
                    /* 内容没有转义符，直接截取返回。 */
                    if (curr - current == text_length)
                    {
                        var result = new string(current, 0, text_length);

                        current = curr + 1;

                        return result;
                    }

                    return InternalReadEscapeString(curr, text_length);
                }

                if (current_char == escape_char)
                {
                    ++curr;

                    if (curr < end && *curr == unicode_char)
                    {
                        curr += 4;
                    }
                }
            }

            throw GetException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int GetDigital(char c)
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

            throw GetException();
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

        public bool IsObject => *current == '{';
        public bool IsArray => *current == '[';

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonValueTypes GetValueType()
        {
            switch (*current)
            {
                case '"':
                case '\'':
                    return JsonValueTypes.String;
                case '{':
                    return JsonValueTypes.Object;
                case '[':
                    return JsonValueTypes.Array;
                case '-':
                case '+':
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    return JsonValueTypes.Number;
                case 't':
                case 'T':
                    return JsonValueTypes.True;
                case 'f':
                case 'F':
                    return JsonValueTypes.False;
                case 'n':
                case 'N':
                    return JsonValueTypes.Null;
                case 'u':
                case 'U':
                    return JsonValueTypes.Undefined;
                default:
                    return JsonValueTypes.Text;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ReadInt64()
        {
            var length = Length;

            var num = NumberHelper.DecimalTryParse(current, length, out long r);

            if (num != 0 && !(num < length && IsExp(current[num])))
            {
                current += num;

                return r;
            }

            return Convert.ToInt64(DirectRead());
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public double ReadDouble()
        {
            var length = Length;

            var num = NumberHelper.Decimal.TryParse(current, length, out double r);

            if (num != 0 && !(num < length && IsExp(current[num])))
            {
                current += num;

                return r;
            }

            return Convert.ToDouble(DirectRead());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string InternalReadText()
        {
            var temp = current;

            while (current < end)
            {
                switch (*current)
                {
                    case ',':
                    case ':':
                    case '}':
                    case ']':
                        goto Return;
                    default:
                        ++current;
                        continue;
                }
            }

        Return:

            return StringHelper.TrimEnd(temp, (int)(current - temp));
        }

        public string ReadString()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return InternalReadString();
                case JsonValueTypes.Text:
                    return InternalReadText();
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                    throw new InvalidCastException("Cannot convert object/array to String.");
            }

            return Convert.ToString(DirectRead());
        }

        public bool ReadBoolean()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.True:
                    if (EqualsByLower(TrueString))
                    {
                        current += TrueString.Length;
                        return true;
                    }
                    break;
                case JsonValueTypes.False:
                    if (EqualsByLower(FalseString))
                    {
                        current += FalseString.Length;
                        return false;
                    }
                    break;
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                    throw new InvalidCastException("Cannot convert object/array to Boolean.");
            }

            return Convert.ToBoolean(DirectRead());
        }

        public byte ReadByte() => checked((byte)ReadInt64());

        public char ReadChar() => char.Parse(ReadString());

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T InternalReadParse<T>() where T : struct
        {
            const char escape_char = '\\';

            var text_char = *current;
            var text_length = 0;

            for (var i = current + 1; i < end; ++i, ++text_length)
            {
                var current_char = *i;

                if (current_char == text_char)
                {
                    var result = InternalReadParse<T>(current + 1, text_length);

                    current = i + 1;

                    return result;
                }

                if (current_char == escape_char)
                {
                    return DefaultReadParse<T>(InternalReadString());
                }
            }

            throw GetException();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T InternalReadParse<T>(char* chars, int length) where T : struct
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

            return DefaultReadParse<T>(new string(chars, 0, length));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T DefaultReadParse<T>(string str) where T : struct
        {
            if (typeof(T) == typeof(DateTime))
            {
                var date_time = DateTime.Parse(str);

                return Unsafe.As<DateTime, T>(ref date_time);
            }
            else if (typeof(T) == typeof(DateTimeOffset))
            {
                var date_time_offset = DateTimeOffset.Parse(str);

                return Unsafe.As<DateTimeOffset, T>(ref date_time_offset);
            }
            else if (typeof(T) == typeof(Guid))
            {
                var guid = new Guid(str);

                return Unsafe.As<Guid, T>(ref guid);
            }

            return default;
        }

        public DateTime ReadDateTime()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return InternalReadParse<DateTime>();
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                    throw new InvalidCastException("Cannot convert object/array to DateTime.");
            }

            return Convert.ToDateTime(DirectRead());
        }

        public decimal ReadDecimal()
        {
            var length = Length;

            var num = NumberHelper.TryParse(current, Length, out decimal r);

            if (num != 0 && !(num < length && IsExp(current[num])))
            {
                current += num;

                return r;
            }

            return Convert.ToDecimal(DirectRead());
        }

        DateTimeOffset IValueReader<DateTimeOffset>.ReadValue()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return InternalReadParse<DateTimeOffset>();
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                    throw new InvalidCastException("Cannot convert object/array to DateTimeOffset.");
            }

            return DateTimeOffset.Parse(ReadString());
        }

        public short ReadInt16() => checked((short)ReadInt64());

        public int ReadInt32() => checked((int)ReadInt64());

        public sbyte ReadSByte() => checked((sbyte)ReadInt64());

        public float ReadSingle() => checked((float)ReadDouble());

        public ushort ReadUInt16() => checked((ushort)ReadInt64());

        public uint ReadUInt32() => checked((uint)ReadInt64());

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            var length = Length;

            var num = NumberHelper.DecimalTryParse(current, length, out ulong r);

            if (num != 0 && !(num < length && IsExp(current[num])))
            {
                current += num;

                return r;
            }

            return Convert.ToUInt64(DirectRead());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public object DirectRead()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return InternalReadString();
                case JsonValueTypes.Object:
                    return ValueInterface<Dictionary<string, object>>.ReadValue(this);
                case JsonValueTypes.Array:
                    return ValueInterface<List<object>>.ReadValue(this);
                case JsonValueTypes.True:
                    if (EqualsByLower(TrueString))
                    {
                        current += TrueString.Length;
                        return true;
                    }
                    break;
                case JsonValueTypes.False:
                    if (EqualsByLower(FalseString))
                    {
                        current += FalseString.Length;
                        return true;
                    }
                    break;
                case JsonValueTypes.Null:
                    if (EqualsByLower(NullString))
                    {
                        current += NullString.Length;
                        return null;
                    }
                    break;
                case JsonValueTypes.Undefined:
                    if (EqualsByLower(UndefinedString))
                    {
                        current += UndefinedString.Length;
                        return null;
                    }
                    break;
                case JsonValueTypes.Number:

                    var numberInfo = NumberHelper.GetNumberInfo(current, Length, 10);

                    if (numberInfo.IsNumber && IsEnding(numberInfo.End))
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
                    break;
            }

            return InternalReadText();
        }

        public Guid ReadValue()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return InternalReadParse<Guid>();
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                    throw new InvalidCastException("Cannot convert object/array to Guid.");
            }

            return new Guid(ReadString());
        }

        public T? ReadNullable<T>() where T : struct
        {
            switch (GetValueType())
            {
                case JsonValueTypes.Null:
                    current += 4;
                    return null;
                case JsonValueTypes.Undefined:
                    current += 9;
                    return null;
            }

            return ValueInterface<T>.ReadValue(this);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void NoObjectOrArray(IDataWriter dataWriter)
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    throw new InvalidCastException("Cannot convert String to object/array.");
                case JsonValueTypes.Number:
                    throw new InvalidCastException("Cannot convert Number to object/array.");
                case JsonValueTypes.Array:
                    ReadArray(dataWriter.As<int>());
                    return;
                case JsonValueTypes.Object:
                    ReadObject(dataWriter.As<string>());
                    return;
                case JsonValueTypes.True:
                case JsonValueTypes.False:
                    throw new InvalidCastException("Cannot convert Boolean to object/array.");
                case JsonValueTypes.Null:
                    if (EqualsByLower(NullString))
                    {
                        current += NullString.Length;
                        return;
                    }
                    break;
                case JsonValueTypes.Undefined:
                    if (EqualsByLower(UndefinedString))
                    {
                        current += UndefinedString.Length;
                        return;
                    }
                    break;
            }

            throw GetException();
        }

        public abstract void ReadObject(IDataWriter<string> valueWriter);

        public abstract void ReadArray(IDataWriter<int> valueWriter);
    }
}