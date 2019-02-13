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
    internal unsafe class JsonDeserializer : IValueReader, IValueReader<Guid>,IValueReader<DateTimeOffset>, ITargetedBind
    {
        public const NumberStyles NumberStyle = NumberStyles.AllowExponent | NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;

        public readonly char* chars;

        public int index;
        public readonly int length;

        public long id;

        public long Id => id;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonDeserializer(char* chars, int index, int length)
        {
            if (index >= length)
            {
                throw new ArgumentException("Json text cannot be empty.");
            }

            this.chars = chars;
            this.index = index;
            this.length = length;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string InternalReadString()
        {
            char textChar = chars[this.index];

            int textLength = 0;

            ++this.index;

            int index = this.index;

            while (index < length)
            {
                if (chars[index] == textChar)
                {
                    goto String;
                }
                else if (chars[index] == '\\')
                {
                    if (index + 1 < length && chars[index + 1] == 'u')
                    {
                        index += 6;
                    }
                    else
                    {
                        index += 2;
                    }
                }
                else
                {
                    ++index;
                }

                ++textLength;
            }

            throw GetException();

            String:

            string result;

            /* 内容没有转义符，直接截取返回。 */
            if (index - this.index == textLength)
            {
                result = new string(chars, this.index, textLength);

                this.index = index + 1;

                return result;
            }

            result = new string('\0', textLength);

            fixed (char* pResult = result)
            {
                for (int i = 0; this.index < index; ++i, ++this.index)
                {
                    if (chars[this.index] == '\\')
                    {
                        ++this.index;

                        if (this.index >= index)
                        {
                            throw GetException();
                        }

                        switch (chars[this.index])
                        {
                            case 'b':
                                pResult[i] = '\b';
                                break;
                            case 'f':
                                pResult[i] = '\f';
                                break;
                            case 'n':
                                pResult[i] = '\n';
                                break;
                            case 't':
                                pResult[i] = '\t';
                                break;
                            case 'r':
                                pResult[i] = '\r';
                                break;
                            case 'u':

                                if (this.index + 4 >= index)
                                {
                                    throw GetException();
                                }

                                pResult[i] = (char)((GetDigital(chars[this.index + 1]) << 12) | (GetDigital(chars[this.index + 2]) << 8) | (GetDigital(chars[this.index + 3]) << 4) | (GetDigital(chars[this.index + 4])));

                                this.index += 4;

                                break;
                            default:
                                pResult[i] = chars[this.index];
                                break;
                        }
                    }
                    else
                    {
                        pResult[i] = chars[this.index];
                    }
                }
            }

            ++this.index;

            return result;
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

        [MethodImpl(VersionDifferences.AggressiveInlining)]
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

            for (int i = 0; i < begin; i++)
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
                case 't':
                case 'T':
                    if (StringHelper.IgnoreCaseEquals(chars, index, length, "TRUE"))
                    {
                        return JsonValueTypes.True;
                    }
                    break;
                case 'f':
                case 'F':
                    if (StringHelper.IgnoreCaseEquals(chars, index, length, "FALSE"))
                    {
                        return JsonValueTypes.False;
                    }
                    break;
                case 'n':
                case 'N':
                    if (StringHelper.IgnoreCaseEquals(chars, index, length, "NULL"))
                    {
                        return JsonValueTypes.Null;
                    }
                    break;
                case 'u':
                case 'U':
                    if (StringHelper.IgnoreCaseEquals(chars, index, length, "UNDEFINED"))
                    {
                        return JsonValueTypes.Undefined;
                    }
                    break;
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

            throw GetException();
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

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ReadInt64()
        {
            //var index = NumberHelper.Decimal.TryParse(chars + begin, end - begin, out long r); // Faster.
            var index = NumberHelper.Decimal.TryParseExp(chars + this.index, length - this.index, out long r); // JSON Standard.

            if (index != 0)
            {
                this.index += index;

                return r;
            }

            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return long.Parse(InternalReadString());
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
                    throw new InvalidCastException("Cannot convert NULL to Number.");
                case JsonValueTypes.Undefined:
                    this.index += 9;
                    return 0;
            }

            return long.Parse(ReadString(), NumberStyle);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public double ReadDouble()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return double.Parse(InternalReadString());
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

            return InternalReadDouble();
        }

        public string ReadString()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.Number:
                    goto Number;
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                    throw new InvalidCastException("Cannot convert object/array to String.");
                case JsonValueTypes.True:
                    this.index += 4;
                    return "true";
                case JsonValueTypes.False:
                    this.index += 5;
                    return "false";
                case JsonValueTypes.Null:
                    this.index += 4;
                    return null;
                case JsonValueTypes.Undefined:
                    this.index += 9;
                    return null;
            }

            return InternalReadString();
            Number:

            int index = this.index;

            while (index < length)
            {
                switch (chars[index])
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
                        ++index;

                        continue;
                    default:
                        goto Return;
                }
            }

            Return:
            var r = new string(chars, this.index, index - this.index);

            this.index = index;

            return r;
        }

        public bool ReadBoolean()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return bool.Parse(InternalReadString());
                case JsonValueTypes.Number:
                    return InternalReadDouble() != 0;
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                    throw new InvalidCastException("Cannot convert object/array to Boolean.");
                case JsonValueTypes.True:
                    index += 4;

                    return true;
                case JsonValueTypes.False:
                    index += 5;

                    return false;
                case JsonValueTypes.Null:
                    index += 4;

                    return false;
                case JsonValueTypes.Undefined:
                    index += 9;

                    return false;
            }

            return false;
        }

        public byte ReadByte()
        {
            return checked((byte)ReadInt64());
        }

        public char ReadChar()
        {
            return ReadString()[0];
        }

        public DateTime ReadDateTime()
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

            char textChar = chars[this.index];

            int textLength = 0;

            int index = this.index + 1;

            int right = index;

            while (right < length)
            {
                if (chars[right] == textChar)
                {
                    break;
                }
                else if (chars[right] == '\\')
                {
                    goto StringDecode;
                }
                else
                {
                    ++right;
                }

                ++textLength;
            }


            if (DateTimeHelper.TryParseISODateTime(chars + index, right - index, out DateTime result))
            {
                this.index = right + 1;

                return result;
            }

            result = DateTime.Parse(new string(chars, index, right - index));

            this.index = right + 1;

            return result;

            StringDecode:

            result = DateTime.Parse(InternalReadString());

            return result;
        }

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

        public short ReadInt16()
        {
            return checked((short)ReadInt64());
        }

        public int ReadInt32()
        {
            return checked((int)ReadInt64());
        }

        public sbyte ReadSByte()
        {
            return checked((sbyte)ReadInt64());
        }

        public float ReadSingle()
        {
            return checked((float)ReadDouble());
        }

        public ushort ReadUInt16()
        {
            return checked((ushort)ReadInt64());
        }

        public uint ReadUInt32()
        {
            return checked((uint)ReadInt64());
        }

        public ulong ReadUInt64()
        {
            // var index = NumberHelper.Decimal.TryParse(chars + begin, end - begin, out ulong r); // Faster.
            var index = NumberHelper.Decimal.TryParseExp(chars + this.index, length - this.index, out ulong r); // JSON Standard.

            if (index != 0)
            {
                this.index += index;

                return r;
            }

            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return ulong.Parse(InternalReadString());
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
                    throw new InvalidCastException("Cannot convert NULL to Number.");
                case JsonValueTypes.Undefined:
                    this.index += 9;
                    return 0;
            }

            return ulong.Parse(ReadString(), NumberStyle);
        }

        public object DirectRead()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    return InternalReadString();
                case JsonValueTypes.Object:
                    return ValueInterface<Dictionary<string, object>>.Content.ReadValue(this);
                case JsonValueTypes.Array:
                    return ValueInterface<List<object>>.Content.ReadValue(this);
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

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ReadObject(IDataWriter<string> valueWriter)
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    throw new InvalidCastException("Cannot convert String to object.");
                case JsonValueTypes.Number:
                    throw new InvalidCastException("Cannot convert Number to object.");
                case JsonValueTypes.Array:
                    ReadArray(valueWriter.As<int>());
                    return;
                case JsonValueTypes.True:
                case JsonValueTypes.False:
                    throw new InvalidCastException("Cannot convert Boolean to object.");
                case JsonValueTypes.Null:
                    /* 空对象直接返回 */
                    index += 4;
                    return;
                case JsonValueTypes.Undefined:
                    index += 9;
                    return;
            }

            while (index < length)
            {
                switch (chars[index])
                {
                    case ' ':
                    case '\n':
                    case '\r':
                    case '\t':
                        ++index;

                        continue;
                    case '{':

                        valueWriter.Initialize();

                        goto case ',';

                    case '}':
                        EndCase:

                        ++index;

                        goto ReturnValue;
                    case ',':

                        Loop:

                        ++index;

                        if (index >= length)
                        {
                            throw GetException();
                        }

                        char c = chars[index];

                        string name;

                        int flag;

                        switch (c)
                        {
                            case ' ':
                            case '\n':
                            case '\r':
                            case '\t':
                                goto Loop;
                            case '}':
                                goto EndCase;
                            case '"':
                            case '\'':
                                name = InternalReadString();

                                flag = StringHelper.IndexOf(chars, ':', index, length);

                                break;
                            default:
                                flag = StringHelper.IndexOf(chars, ':', index, length);

                                name = StringHelper.Trim(chars, index, flag);

                                break;

                        }

                        if (flag == -1)
                        {
                            goto Exception;
                        }

                        index = flag + 1;

                        while (index < length)
                        {
                            switch (chars[index])
                            {
                                case ' ':
                                case '\n':
                                case '\r':
                                case '\t':
                                    ++index;
                                    continue;
                                default:
                                    goto ReadValue;
                            }
                        }

                        goto Exception;

                        ReadValue:

                        valueWriter.OnWriteValue(name, this);

                        continue;
                    default:
                        goto Exception;
                }
            }


            Exception:
            throw GetException();

            ReturnValue:

            return;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ReadArray(IDataWriter<int> valueWriter)
        {
            switch (GetValueType())
            {
                case JsonValueTypes.String:
                    throw new InvalidCastException("Cannot convert String to array.");
                case JsonValueTypes.Number:
                    throw new InvalidCastException("Cannot convert Number to array.");
                case JsonValueTypes.Object:
                    ReadObject(valueWriter.As<string>());
                    return;
                case JsonValueTypes.True:
                    throw new InvalidCastException("Cannot convert Boolean to array.");
                case JsonValueTypes.Null:
                    /* 空对象直接返回 */
                    this.index += 4;
                    return;
                case JsonValueTypes.Undefined:
                    this.index += 9;
                    return;
            }

            int index = 0;

            while (this.index < length)
            {
                switch (chars[this.index])
                {
                    case ' ':
                    case '\n':
                    case '\r':
                    case '\t':

                        ++this.index;

                        continue;

                    case '[':

                        valueWriter.Initialize();

                        goto case ',';

                    case ']':
                        EndCase:

                        ++this.index;

                        goto ReturnValue;

                    case ',':

                        ++this.index;

                        while (this.index < length)
                        {
                            switch (chars[this.index])
                            {
                                case ' ':
                                case '\n':
                                case '\r':
                                case '\t':
                                    ++this.index;
                                    continue;
                                case ']':
                                    goto EndCase;
                                default:
                                    goto ReadValue;
                            }
                        }

                        goto Exception;

                        ReadValue:

                        valueWriter.OnWriteValue(index, this);

                        ++index;

                        continue;

                    default:

                        goto Exception;
                }
            }

            Exception:
            throw GetException();

            ReturnValue:

            return;
        }

        DateTimeOffset IValueReader<DateTimeOffset>.ReadValue()
        {
            switch (GetValueType())
            {
                case JsonValueTypes.Number:
                    throw new InvalidCastException("Cannot convert Number to DateTimeOffset.");
                case JsonValueTypes.Object:
                case JsonValueTypes.Array:
                    throw new InvalidCastException("Cannot convert object/array to DateTimeOffset.");
                case JsonValueTypes.True:
                case JsonValueTypes.False:
                    throw new InvalidCastException("Cannot convert Boolean to DateTimeOffset.");
                case JsonValueTypes.Null:
                case JsonValueTypes.Undefined:
                    throw new InvalidCastException("Cannot convert Null to DateTimeOffset.");
            }

            char textChar = chars[this.index];

            int textLength = 0;

            int index = this.index + 1;

            int right = index;

            while (right < length)
            {
                if (chars[right] == textChar)
                {
                    break;
                }
                else if (chars[right] == '\\')
                {
                    goto StringDecode;
                }
                else
                {
                    ++right;
                }

                ++textLength;
            }
            
            if (DateTimeHelper.TryParseISODateTime(chars + index, right - index, out DateTimeOffset result))
            {
                this.index = right + 1;

                return result;
            }

            result = DateTimeOffset.Parse(new string(chars, index, right - index));

            this.index = right + 1;

            return result;

        StringDecode:

            result = DateTimeOffset.Parse(InternalReadString());

            return result;
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

            return ValueInterface<T>.Content.ReadValue(this);
        }
    }
}