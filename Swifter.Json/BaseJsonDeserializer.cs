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
                    return true;
            }

            return false;
        }

        public const NumberStyles NumberStyle = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;

        public readonly char* chars;

        public int index;
        public readonly int length;

        public JsonFormatter jsonFormatter;

        public long Id => jsonFormatter?.id ?? 0;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public BaseJsonDeserializer(char* chars, int length)
        {
            if (index >= length)
            {
                throw new ArgumentException("Json text cannot be empty.");
            }

            this.chars = chars;
            this.length = length;

            index = 0;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public string InternalReadEscapeString(int text_end, int text_length)
        {
            var text = StringHelper.MakeString(text_length);

            fixed (char* ptr = text)
            {
                for (var current = ptr; index < text_end; ++current, ++index)
                {
                    if (chars[index] == '\\')
                    {
                        ++index;

                        switch (chars[index])
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
                                    (GetDigital(chars[index + 1]) << 12) |
                                    (GetDigital(chars[index + 2]) << 8) |
                                    (GetDigital(chars[index + 3]) << 4) |
                                    (GetDigital(chars[index + 4])));

                                index += 4;

                                continue;
                        }
                    }

                    *current = chars[index];
                }
            }

            ++index;

            return text;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string InternalReadString()
        {
            const char escape_char = '\\';
            const char unicode_char = 'u';

            var text_char = chars[index];
            var text_length = 0;

            for (int i = (++index); i < length; ++i, ++text_length)
            {
                var current_char = chars[i];

                if (current_char == text_char)
                {
                    /* 内容没有转义符，直接截取返回。 */
                    if (i - index == text_length)
                    {
                        var result = new string(chars, index, text_length);

                        index = i + 1;

                        return result;
                    }

                    return InternalReadEscapeString(i, text_length);
                }

                if (current_char == escape_char)
                {
                    ++i;

                    if (i < length && chars[i] == unicode_char)
                    {
                        i += 4;
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
            var begin = index;

            if (begin >= length)
            {
                begin = length - 1;
            }

            int line = 1;
            int lineBegin = 0;
            int column = 1;

            for (int i = 0; i < begin; ++i)
            {
                if (chars[i] == '\n')
                {
                    ++line;

                    lineBegin = i;
                }
            }

            column = begin - lineBegin + 1;

            var exception = new JsonDeserializeException
            {
                Line = line,
                Column = column,
                Index = begin,
                Text = chars[begin].ToString()
            };

            return exception;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonValueTypes GetValueType()
        {
            switch (chars[index])
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
            }

            return Other();

            JsonValueTypes Other()
            {
                var start = chars + index;
                var count = length - index;

                switch (*start)
                {
                    case 't':
                    case 'T':
                        if (StringHelper.StartWithByLower(start, count, "true"))
                        {
                            return JsonValueTypes.True;
                        }
                        break;
                    case 'f':
                    case 'F':
                        if (StringHelper.StartWithByLower(start, count, "false"))
                        {
                            return JsonValueTypes.False;
                        }
                        break;
                    case 'n':
                    case 'N':
                        if (StringHelper.StartWithByLower(start, count, "null"))
                        {
                            return JsonValueTypes.Null;
                        }
                        break;
                    case 'u':
                    case 'U':
                        if (StringHelper.StartWithByLower(start, count, "undefined"))
                        {
                            return JsonValueTypes.Undefined;
                        }
                        break;
                }

                throw GetException();
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private double InternalReadDouble()
        {
            var index = NumberHelper.Decimal.TryParse(chars + this.index, length - this.index, out double r);

            if (index != 0)
            {
                this.index += index;

                return r;
            }

            return double.Parse(ReadString(), NumberStyle);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public long ReadNumberInfoToInt64()
        {
            var number = NumberHelper.GetNumberInfo(chars + index, length - index, 10);

            if (number.IsNumber)
            {
                index += number.End;

                return number.ToInt64();
            }

            return ReadOtherToInt64();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public long ReadOtherToInt64()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return long.Parse(InternalReadString());
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                    throw new InvalidCastException("Cannot convert object/array to Number.");
                case JsonValueTypes.True:
                    index += 4;
                    return 1;
                case JsonValueTypes.False:
                    index += 5;
                    return 0;
                case JsonValueTypes.Null:
                    throw new InvalidCastException("Cannot convert NULL to Number.");
                case JsonValueTypes.Undefined:
                    index += 9;
                    return 0;
            }

            return long.Parse(ReadString(), NumberStyle);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ReadInt64()
        {
            var start = chars + index;
            var count = length - index;

            var num = NumberHelper.DecimalTryParse(start, count, out long r);

            if (num != 0)
            {
                if (num < count && IsExp(start[num]))
                {
                    return ReadNumberInfoToInt64();
                }

                index += num;

                return r;
            }

            return ReadOtherToInt64();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public double ReadDouble()
        {
            var type = GetValueType();

            if (type == JsonValueTypes.Number)
            {
                return InternalReadDouble();
            }

            return Other();

            double Other()
            {
                switch (type)
                {
                    case JsonValueTypes.String:
                        return double.Parse(InternalReadString());
                    case JsonValueTypes.True:
                        index += 4;
                        return 1;
                    case JsonValueTypes.False:
                        index += 5;
                        return 0;
                    case JsonValueTypes.Null:
                        throw new InvalidCastException("Cannot convert NULL to Number.");
                    case JsonValueTypes.Undefined:
                        index += 9;
                        return 0;
                }

                throw new InvalidCastException("Cannot convert object/array to Number.");
            }
        }

        public string ReadString()
        {
            var type = GetValueType();

            if (type == JsonValueTypes.String)
            {
                return InternalReadString();
            }

            return Other();

            string Other()
            {
                switch (type)
                {
                    case JsonValueTypes.Number:
                        return Number();
                    case JsonValueTypes.True:
                        index += 4;
                        return true.ToString();
                    case JsonValueTypes.False:
                        index += 5;
                        return false.ToString();
                    case JsonValueTypes.Null:
                        index += 4;
                        return null;
                    case JsonValueTypes.Undefined:
                        index += 9;
                        return null;
                }

                throw new InvalidCastException("Cannot convert object/array to String.");
            }

            string Number()
            {
                int num = index;

                while (num < length)
                {
                    switch (chars[num])
                    {
                        case '-':
                        case '+':
                        case '.':
                        case 'e':
                        case 'E':
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
                            ++num;

                            continue;
                        default:
                            goto Return;
                    }
                }

            Return:

                var r = new string(chars, index, num - index);

                index = num;

                return r;
            }
        }

        public bool ReadBoolean()
        {
            var type = GetValueType();

            switch (type)
            {
                case JsonValueTypes.True:
                    index += 4;
                    return true;
                case JsonValueTypes.False:
                    index += 5;
                    return false;
            }

            return Other();

            bool Other()
            {
                switch (type)
                {
                    case JsonValueTypes.String:
                        return bool.Parse(InternalReadString());
                    case JsonValueTypes.Number:
                        return InternalReadDouble() != 0;
                    case JsonValueTypes.Object:
                    case JsonValueTypes.Array:
                        throw new InvalidCastException("Cannot convert object/array to Boolean.");
                    case JsonValueTypes.Null:
                        index += 4;
                        return false;
                    case JsonValueTypes.Undefined:
                        index += 9;
                        return false;
                }

                return false;
            }
        }

        public byte ReadByte() => checked((byte)ReadInt64());

        public char ReadChar() => char.Parse(ReadString());

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T ReadDataTime<T>() where T : struct
        {
            switch (GetValueType())
            {
                case JsonValueTypes.Number:
                    throw new InvalidCastException("Cannot convert Number to DateTime.");
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                    throw new InvalidCastException("Cannot convert object/array to DateTime.");
                case JsonValueTypes.True:
                case JsonValueTypes.False:
                    throw new InvalidCastException("Cannot convert Boolean to DateTime.");
                case JsonValueTypes.Null:
                case JsonValueTypes.Undefined:
                    throw new InvalidCastException("Cannot convert Null to DateTime.");
            }


            const char escape_char = '\\';

            var text_char = chars[index];
            var text_length = 0;

            for (int i = index + 1; i < length; ++i, ++text_length)
            {
                var current_char = chars[i];

                if (current_char == text_char)
                {
                    var result = InternalParse(chars + index + 1, text_length);

                    index = i + 1;

                    return result;
                }

                if (current_char == escape_char)
                {
                    return DefaultParse(InternalReadString());
                }
            }

            throw GetException();

            T InternalParse(char* chars, int length)
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

                return DefaultParse(new string(chars, 0, length));
            }


            T DefaultParse(string str)
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

                return default;
            }
        }

        public DateTime ReadDateTime() => ReadDataTime<DateTime>();

        public decimal ReadDecimal()
        {
            var index = NumberHelper.TryParse(chars + this.index, length - this.index, out decimal r);

            if (index != 0)
            {
                this.index += index;

                return r;
            }

            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return decimal.Parse(InternalReadString());
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                    throw new InvalidCastException("Cannot convert object/array to Number.");
                case JsonValueTypes.True:
                    this.index += 4;
                    return 1;
                case JsonValueTypes.False:
                    this.index += 5;
                    return 0;
                case JsonValueTypes.Null:
                    this.index += 4;
                    return 0;
                case JsonValueTypes.Undefined:
                    this.index += 9;
                    return 0;
            }

            return decimal.Parse(ReadString(), NumberStyle);
        }

        DateTimeOffset IValueReader<DateTimeOffset>.ReadValue() => ReadDataTime<DateTimeOffset>();

        public short ReadInt16() => checked((short)ReadInt64());

        public int ReadInt32() => checked((int)ReadInt64());

        public sbyte ReadSByte() => checked((sbyte)ReadInt64());

        public float ReadSingle() => checked((float)ReadDouble());

        public ushort ReadUInt16() => checked((ushort)ReadInt64());

        public uint ReadUInt32() => checked((uint)ReadInt64());

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ulong ReadNumberInfoToUInt64()
        {
            var number = NumberHelper.GetNumberInfo(chars + index, length - index, 10);

            if (number.IsNumber)
            {
                index += number.End;

                return number.ToUInt64();
            }

            return ReadOtherToUInt64();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ulong ReadOtherToUInt64()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return ulong.Parse(InternalReadString());
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                    throw new InvalidCastException("Cannot convert object/array to Number.");
                case JsonValueTypes.True:
                    index += 4;
                    return 1;
                case JsonValueTypes.False:
                    index += 5;
                    return 0;
                case JsonValueTypes.Null:
                    throw new InvalidCastException("Cannot convert NULL to Number.");
                case JsonValueTypes.Undefined:
                    index += 9;
                    return 0;
            }

            return ulong.Parse(ReadString(), NumberStyle);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            var chars = this.chars + index;
            var length = this.length - index;

            var num = NumberHelper.DecimalTryParse(chars, length, out ulong r);

            if (num != 0)
            {
                if (num < length && IsExp(chars[num]))
                {
                    return ReadNumberInfoToUInt64();
                }

                index += num;

                return r;
            }

            return ReadOtherToUInt64();
        }

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
                    index += 4;
                    return true;
                case JsonValueTypes.False:
                    index += 5;
                    return false;
                case JsonValueTypes.Null:
                    index += 4;
                    return null;
                case JsonValueTypes.Undefined:
                    index += 9;
                    return null;
                case JsonValueTypes.Number:

                    var numberInfo = NumberHelper.Decimal.GetNumberInfo(chars + index, length - index);

                    if (numberInfo.IsNumber)
                    {
                        index += numberInfo.End;

                        if (numberInfo.HaveExponent)
                        {
                            if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 16 && numberInfo.ExponentCount <= 2)
                            {
                                return NumberHelper.Decimal.ToDouble(numberInfo);
                            }

                            return numberInfo.ToString();
                        }

                        if (numberInfo.IsFloat)
                        {
                            if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 16)
                            {
                                return NumberHelper.Decimal.ToDouble(numberInfo);
                            }

                            if (numberInfo.IntegerCount + numberInfo.FractionalCount <= 28)
                            {
                                return NumberHelper.ToDecimal(numberInfo);
                            }

                            return numberInfo.ToString();
                        }

                        if (numberInfo.IntegerCount <= 18)
                        {
                            var int64 = NumberHelper.Decimal.ToInt64(numberInfo);

                            if (int64 <= int.MaxValue && int64 >= int.MinValue)
                            {
                                return (int)int64;
                            }

                            return int64;
                        }

                        if (numberInfo.IntegerCount <= 28)
                        {
                            return NumberHelper.ToDecimal(numberInfo);
                        }

                        return numberInfo.ToString();
                    }

                    break;
            }

            throw GetException();
        }

        public Guid ReadValue()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.Number:
                    throw new InvalidCastException("Cannot convert Number to Guid.");
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                    throw new InvalidCastException("Cannot convert object/array to Guid.");
                case JsonValueTypes.True:
                case JsonValueTypes.False:
                    throw new InvalidCastException("Cannot convert Boolean to Guid.");
                case JsonValueTypes.Null:
                case JsonValueTypes.Undefined:
                    throw new InvalidCastException("Cannot convert Null to Guid.");
            }

            var index = this.index;

            var textChar = chars[index];

            ++index;

            index = NumberHelper.TryParse(chars + index, length, out Guid r);

            if (index >= 32)
            {
                this.index += index + 1;

                if (chars[this.index] == textChar)
                {
                    ++this.index;
                }


                return r;
            }

            return new Guid(InternalReadString());
        }

        public T? ReadNullable<T>() where T : struct
        {
            switch (GetValueType())
            {
                case JsonValueTypes.Null:
                    index += 4;
                    return null;
                case JsonValueTypes.Undefined:
                    index += 9;
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
                    /* 空对象直接返回 */
                    index += 4;
                    return;
                case JsonValueTypes.Undefined:
                    index += 9;
                    return;
            }
        }

        public abstract void ReadObject(IDataWriter<string> valueWriter);

        public abstract void ReadArray(IDataWriter<int> valueWriter);
    }
}