using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    internal sealed unsafe class JsonDeserializer : BaseJsonDeserializer, IId64Filler<char>
    {
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public JsonDeserializer(char* chars, int length) : base(chars, length)
        {

        }

        public override void ReadObject(IDataWriter<string> dataWriter)
        {
            if (GetValueType() != JsonValueTypes.Object)
            {
                NoObjectOrArray(dataWriter);

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

                        dataWriter.Initialize();

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

                                flag = StringHelper.IndexOf(chars, index, length, ':');

                                break;
                            default:
                                flag = StringHelper.IndexOf(chars, index, length, ':');

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

                        dataWriter.OnWriteValue(name, this);

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

        public override void ReadArray(IDataWriter<int> dataWriter)
        {
            if (GetValueType() != JsonValueTypes.Array)
            {
                NoObjectOrArray(dataWriter);

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

                        dataWriter.Initialize();

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

                        dataWriter.OnWriteValue(index, this);

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




        [MethodImpl(MethodImplOptions.NoInlining)]
        public IEnumerable<char> GetNameId64(int end)
        {
            char CharAt(int i) => chars[i];

            for (; index < end; ++index)
            {
                if (CharAt(index) == '\\')
                {
                    ++index;

                    switch (CharAt(index))
                    {
                        case 'b':
                            yield return '\b';
                            continue;
                        case 'f':
                            yield return '\f';
                            continue;
                        case 'n':
                            yield return '\n';
                            continue;
                        case 't':
                            yield return '\t';
                            continue;
                        case 'r':
                            yield return '\r';
                            continue;
                        case 'u':

                            yield return (char)(
                                (GetDigital(CharAt(index + 1)) << 12) |
                                (GetDigital(CharAt(index + 2)) << 8) |
                                (GetDigital(CharAt(index + 3)) << 4) |
                                (GetDigital(CharAt(index + 4)))
                                );

                            index += 4;

                            continue;
                    }
                }

                yield return CharAt(index);
            }

            ++index;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long InternalReadNameId64(IId64DataRW<char> id64DataRW)
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
                        var result = id64DataRW.GetId64(ref chars[index], text_length);

                        index = i + 1;

                        return result;
                    }

                    return id64DataRW.GetId64(GetNameId64(i));
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        public long InternalTrimNameId64<TDataWriter>(TDataWriter id64DataRW, int begin, int end) where TDataWriter : IDataRW, IId64DataRW<char>
        {
            while (begin < end && StringHelper.IsWhiteSpace(chars[begin]))
            {
                ++begin;
            }

            do
            {
                --end;
            } while (end >= begin && StringHelper.IsWhiteSpace(chars[end]));

            if (end >= begin)
            {
                return id64DataRW.GetId64(ref chars[begin], end - begin + 1);
            }

            return id64DataRW.GetId64(ref chars[begin], 0);
        }

        public void FillValue<TDataWriter>(TDataWriter dataWriter) where TDataWriter : IDataRW, IId64DataRW<char>
        {
            if (GetValueType() != JsonValueTypes.Object)
            {
                NoObjectOrArray(dataWriter);

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

                        dataWriter.Initialize();

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

                        long id64;

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
                                id64 = InternalReadNameId64(dataWriter);

                                flag = StringHelper.IndexOf(chars, index, length, ':');

                                break;
                            default:
                                flag = StringHelper.IndexOf(chars, index, length, ':');

                                id64 = InternalTrimNameId64(dataWriter, index, flag);

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

                        dataWriter.OnWriteValue(id64, this);

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
    }
}