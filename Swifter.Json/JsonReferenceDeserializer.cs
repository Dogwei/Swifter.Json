using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Swifter.Json
{
    internal sealed unsafe class JsonReferenceDeserializer : JsonDeserializer, IValueReader
    {
        public LinkedList<ReferenceInfo> references;
        public TargetPathInfo referenceTarget;

        public bool updateBase;

        public IDataWriter writer;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonReferenceDeserializer(char* chars, int index, int length)
            : base(chars, index, length)
        {
            references = new LinkedList<ReferenceInfo>();
        }

        void IValueReader.ReadObject(IDataWriter<string> valueWriter)
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

            var haveRef = false;

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

                        if (TryReadRefReferenceTarget())
                        {
                            valueWriter.OnWriteValue(name, RWHelper.DefaultValueReader);

                            references.AddLast(new ReferenceInfo(referenceTarget, new SourcePathInfo(name, null, valueWriter)));

                            if (valueWriter is IDataReader reader && reader.ReferenceToken == null)
                            {
                                updateBase = true;
                            }

                            haveRef = true;
                        }
                        else
                        {
                            valueWriter.OnWriteValue(name, this);

                            if (updateBase)
                            {
                                var last = references.Last.Value;

                                last.source = new SourcePathInfo(name, last.source, valueWriter);

                                updateBase = false;

                                haveRef = true;
                            }
                        }

                        continue;
                    default:
                        goto Exception;
                }
            }


            Exception:
            throw GetException();

            ReturnValue:

            if (haveRef && valueWriter is IDataReader dataReader && dataReader.ReferenceToken == null)
            {
                writer = valueWriter;

                updateBase = true;
            }

            return;
        }

        void IValueReader.ReadArray(IDataWriter<int> valueWriter)
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

            var haveRef = false;

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

                        if (TryReadRefReferenceTarget())
                        {
                            valueWriter.OnWriteValue(index, RWHelper.DefaultValueReader);

                            references.AddLast(new ReferenceInfo(referenceTarget, new SourcePathInfo(index, null, valueWriter)));

                            if (valueWriter is IDataReader reader && reader.ReferenceToken == null)
                            {
                                updateBase = true;
                            }

                            haveRef = true;
                        }
                        else
                        {
                            valueWriter.OnWriteValue(index, this);

                            if (updateBase)
                            {
                                var last = references.Last.Value;

                                last.source = new SourcePathInfo(index, last.source, valueWriter);

                                updateBase = false;

                                haveRef = true;
                            }
                        }

                        ++index;

                        continue;

                    default:

                        goto Exception;
                }
            }

            Exception:
            throw GetException();

            ReturnValue:

            if (haveRef && valueWriter is IDataReader dataReader && dataReader.ReferenceToken == null)
            {
                writer = valueWriter;

                updateBase = true;
            }

            return;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private bool TryReadRefReferenceTarget()
        {
            if (chars[this.index] != '{')
            {
                return false;
            }

            const string RefName = "$REF";

            var index = this.index;

            ++index;

            while (index < length)
            {
                var c = chars[index];

                switch (c)
                {
                    case ' ':
                    case '\n':
                    case '\r':
                    case '\t':
                        ++index;
                        continue;
                    case '"':
                    case '\'':
                        ++index;

                        if (StringHelper.IgnoreCaseEquals(chars, index, length, RefName) && index + RefName.Length < length && chars[index + RefName.Length] == c)
                        {
                            ++index;

                            index += RefName.Length;

                            goto IsRef;
                        }

                        return false;

                    case '$':

                        if (StringHelper.IgnoreCaseEquals(chars, index, length, RefName) && index + RefName.Length < length)
                        {
                            switch (chars[index + RefName.Length])
                            {
                                case ' ':
                                case '\n':
                                case '\r':
                                case '\t':
                                case ':':
                                    index += RefName.Length;
                                    goto IsRef;
                            }
                        }

                        return false;

                    default:
                        return false;
                }
            }

            return false;

            IsRef:

            index = StringHelper.IndexOf(chars, ':', index, length);

            if (index == -1)
            {
                throw GetException();
            }

            ++index;

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
                        goto ReadReference;
                }
            }

            throw GetException();

            ReadReference:

            this.index = index;

            referenceTarget = ReadRefReferenceTarget();

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
                    case '}':
                        ++this.index;
                        return true;
                    default:
                        goto Error;

                }
            }

            throw GetException();

            Error:

            throw new ArgumentException("Json reference object cannot contains other field.", GetException());
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static bool IsRootReference(StringBuilder sb)
        {
            return (sb.Length == 0 || (sb.Length == 1 && sb[0] == '#'));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static bool IsNumberChar(char c)
        {
            return c >= '0' && c <= '9';
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private TargetPathInfo ReadRefReferenceTarget()
        {
            var nameBuilder = new StringBuilder();

            var targetPathInfo = new TargetPathInfo("#", null);

            char textChar = chars[index];

            ++index;

            while (index < length)
            {
                var c = chars[index];

                if (c == textChar)
                {
                    ++index;

                    if (IsRootReference(nameBuilder) && targetPathInfo.IsRoot)
                    {
                        return targetPathInfo;
                    }

                    return new TargetPathInfo(nameBuilder.ToString(), targetPathInfo);
                }
                else if (c == '/')
                {
                    try
                    {
                        ++index;

                        if (IsRootReference(nameBuilder) && targetPathInfo.IsRoot)
                        {
                            continue;
                        }

                        targetPathInfo = new TargetPathInfo(nameBuilder.ToString(), targetPathInfo);
                    }
                    finally
                    {
                        nameBuilder.Length = 0;
                    }

                    /* Index Reference. */

                    if (index < length && IsNumberChar(chars[index]))
                    {
                        var temp = index;

                        int value = chars[index] - '0';
                        
                        IndexLoop:

                        ++index;

                        if (index >= length)
                        {
                            break;
                        }

                        c = chars[index];
                        
                        if (IsNumberChar(chars[index]))
                        {
                            value = (value * 10) + (c - '0');

                            goto IndexLoop;
                        }

                        targetPathInfo = new TargetPathInfo(value, targetPathInfo);

                        if (c == textChar)
                        {
                            ++index;

                            return targetPathInfo;
                        }

                        if (c == '/')
                        {
                            ++index;
                            
                            continue;
                        }

                        index = temp;
                    }
                }
                else if (c == '\\')
                {
                    ++index;

                    if (index >= length)
                    {
                        throw GetException();
                    }

                    switch (chars[index])
                    {
                        case 'b':
                            nameBuilder.Append('\b');
                            break;
                        case 'f':
                            nameBuilder.Append('\f');
                            break;
                        case 'n':
                            nameBuilder.Append('\n');
                            break;
                        case 't':
                            nameBuilder.Append('\t');
                            break;
                        case 'r':
                            nameBuilder.Append('\r');
                            break;
                        case 'u':

                            if (index + 4 >= length)
                            {
                                throw GetException();
                            }

                            nameBuilder.Append((char)((GetDigital(chars[index + 1]) << 12)
                                | (GetDigital(chars[index + 2]) << 8)
                                | (GetDigital(chars[index + 3]) << 4)
                                | (GetDigital(chars[index + 4]))));

                            index += 4;

                            break;
                        default:
                            nameBuilder.Append(chars[index]);
                            break;
                    }
                }
                else if (c == '%')
                {
                    if (index + 2 >= length)
                    {
                        throw GetException();
                    }

                    ++index;

                    if (NumberHelper.Hex.TryParse(chars + index, 2, out int utf8) != 2)
                    {
                        throw GetException();
                    }

                    index += 2;

                    if (utf8 >= 0B11100000)
                    {
                        if (index + 6 >= length)
                        {
                            throw GetException();
                        }

                        if (chars[index] != '%' && chars[index + 3] != '%')
                        {
                            throw GetException();
                        }

                        if (NumberHelper.Hex.TryParse(chars + index + 1, 2, out int t1) != 2
                            || NumberHelper.Hex.TryParse(chars + index + 4, 2, out int t2) != 2)
                        {
                            throw GetException();
                        }

                        utf8 = ((utf8 & 0B1111) << 12) | ((t1 & 0B111111) << 6) | (t2 & 0B111111);

                        index += 6;
                    }
                    else if (utf8 >= 0B11000000)
                    {
                        if (index + 3 >= length)
                        {
                            throw GetException();
                        }

                        if (chars[index] != '%')
                        {
                            throw GetException();
                        }

                        if (NumberHelper.Hex.TryParse(chars + index + 1, 2, out int t1) != 2)
                        {
                            throw GetException();
                        }

                        utf8 = ((utf8 & 0B11111) << 6) | (t1 & 0B111111);

                        index += 3;
                    }

                    nameBuilder.Append((char)utf8);
                }
                else
                {
                    ++index;

                    nameBuilder.Append(c);
                }
            }

            throw GetException();
        }
    }
}