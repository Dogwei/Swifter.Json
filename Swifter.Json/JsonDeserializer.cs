using Swifter.Readers;
using Swifter.RW;
using Swifter.Writers;
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
            if (!IsObject)
            {
                NoObjectOrArray(dataWriter);

                return;
            }

            ++current;

            dataWriter.Initialize();

        Loop:

            SkipWhiteSpace();

            if (current < end)
            {
                string name;

                switch (*current)
                {
                    case '}':
                        goto Return;
                    case '"':
                    case '\'':
                        name = InternalReadString();
                        break;
                    default:
                        name = InternalReadText();
                        break;
                }

                SkipWhiteSpace();

                if (current < end && *current == ':')
                {
                    ++current;

                    SkipWhiteSpace();

                    if (current < end)
                    {
                        dataWriter.OnWriteValue(name, this);

                        SkipWhiteSpace();

                        if (current < end)
                        {
                            switch (*current)
                            {
                                case '}':
                                    goto Return;
                                case ',':
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

        public override void ReadArray(IDataWriter<int> dataWriter)
        {
            if (!IsArray)
            {
                NoObjectOrArray(dataWriter);

                return;
            }

            var i = 0;

            ++current;

            dataWriter.Initialize();

        Loop:

            SkipWhiteSpace();

            if (current < end)
            {
                if (*current == ']')
                {
                    goto Return;
                }

                dataWriter.OnWriteValue(i, this); ++i;

                SkipWhiteSpace();

                if (current < end)
                {
                    switch (*current)
                    {
                        case ']':
                            goto Return;
                        case ',':
                            ++current;

                            goto Loop;
                    }
                }
            }

            throw GetException();

        Return:

            ++current;
        }


        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long InternalReadNameId64(IId64DataRW<char> id64DataRW)
        {
            const char escape_char = '\\';
            const char unicode_char = 'u';

            var text_char = *current;
            var text_length = 0;

            for (var index = (++current); index < end; ++index, ++text_length)
            {
                var current_char = *index;

                if (current_char == text_char)
                {
                    /* 内容没有转义符，直接截取返回。 */
                    if (index - current == text_length)
                    {
                        var result = id64DataRW.GetId64(ref current[0], text_length);

                        current = index + 1;

                        return result;
                    }

                    return id64DataRW.GetId64Ex(InternalReadEscapeString(index, text_length));
                }

                if (current_char == escape_char)
                {
                    ++index;

                    if (index < end && *index == unicode_char)
                    {
                        index += 4;
                    }
                }
            }

            throw GetException();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void FillValue<TDataWriter>(TDataWriter dataWriter) where TDataWriter : IDataRW, IId64DataRW<char>
        {
            if (!IsObject)
            {
                NoObjectOrArray(dataWriter);

                return;
            }

            ++current;

            dataWriter.Initialize();

        Loop:

            SkipWhiteSpace();

            if (current < end)
            {
                long id64;

                switch (*current)
                {
                    case '}':
                        goto Return;
                    case '"':
                    case '\'':
                        id64 = InternalReadNameId64(dataWriter);
                        break;
                    default:
                        id64 = dataWriter.GetId64Ex(InternalReadText());
                        break;
                }

                SkipWhiteSpace();

                if (current < end && *current == ':')
                {
                    ++current;

                    SkipWhiteSpace();

                    dataWriter.OnWriteValue(id64, this);

                    SkipWhiteSpace();

                    if (current < end)
                    {
                        switch (*current)
                        {
                            case '}':
                                goto Return;
                            case ',':
                                ++current;

                                goto Loop;
                        }
                    }
                }
            }

            throw GetException();

        Return:

            ++current;
        }
    }
}