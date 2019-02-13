using Swifter.Readers;
using Swifter.RW;
using Swifter.Writers;
using System.IO;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    unsafe class BaseJsonSerializer: ITargetedBind
    {
        public TextWriter textWriter;
        public HGlobalCache hGlobal;
        public int offset;
        public long id;

        public int depth;

        public BaseJsonSerializer()
        {
            offset = 0;

            hGlobal = HGlobalCache.ThreadInstance;

            Expand(255);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Expand(int expandMinSize)
        {
        Loop:
            if (hGlobal.count - offset < expandMinSize)
            {
                if (hGlobal.count == HGlobalCache.MaxSize && textWriter != null && offset != 0)
                {
                    VersionDifferences.WriteChars(textWriter, hGlobal.chars, offset);

                    offset = 0;

                    goto Loop;
                }
                else
                {
                    hGlobal.Expand(expandMinSize);
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append(char c)
        {
            hGlobal.chars[offset] = c;

            ++offset;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Append(string value)
        {
            int length = value.Length;

            Expand(length + 2);

            var chars = hGlobal.chars;

            for (int i = 0; i < length; ++i)
            {
                chars[offset] = value[i];

                ++offset;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void InternalWriteLongString(string value)
        {
            Expand(2);

            Append('"');

            fixed (char* pValue = value)
            {
                int length = value.Length;

                for (int i = 0; i < value.Length;)
                {
                    int count = length - i;

                    Expand(count + 2);

                    for (int end = count + offset; offset < end; ++i)
                    {
                        var c = pValue[i];

                        switch (c)
                        {
                            case '\\':
                                Append('\\');
                                Append('\\');
                                break;
                            case '"':
                                Append('\\');
                                Append('"');
                                break;
                            case '\n':
                                Append('\\');
                                Append('n');
                                break;
                            case '\r':
                                Append('\\');
                                Append('r');
                                break;
                            case '\t':
                                Append('\\');
                                Append('t');
                                break;
                            default:
                                Append(c);
                                break;
                        }
                    }
                }
            }

            Expand(2);

            Append('"');
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void InternalWriteString(string value)
        {
            int length = value.Length;

            if (length > 300)
            {
                InternalWriteLongString(value);

                return;
            }

            Expand(length * 2 + 2);

            Append('"');

            int offset = this.offset;

            for (int i = 0; i < length; ++i)
            {
                var c = value[i];

                switch (c)
                {
                    case '\\':
                        hGlobal.chars[offset] = '\\';
                        ++offset;
                        hGlobal.chars[offset] = '\\';
                        ++offset;
                        break;
                    case '"':
                        hGlobal.chars[offset] = '\\';
                        ++offset;
                        hGlobal.chars[offset] = '"';
                        ++offset;
                        break;
                    case '\n':
                        hGlobal.chars[offset] = '\\';
                        ++offset;
                        hGlobal.chars[offset] = 'n';
                        ++offset;
                        break;
                    case '\r':
                        hGlobal.chars[offset] = '\\';
                        ++offset;
                        hGlobal.chars[offset] = 'r';
                        ++offset;
                        break;
                    case '\t':
                        hGlobal.chars[offset] = '\\';
                        ++offset;
                        hGlobal.chars[offset] = 't';
                        ++offset;
                        break;
                    default:
                        hGlobal.chars[offset] = c;
                        ++offset;
                        break;
                }
            }

            this.offset = offset;

            Append('"');
        }
        
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override string ToString()
        {
            return new string(hGlobal.chars, 0, offset - 1);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteTo(TextWriter textWriter)
        {
            VersionDifferences.WriteChars(textWriter, hGlobal.chars, offset - 1);
        }

        public void Initialize()
        {
        }

        public void Initialize(int capacity)
        {
        }

        public int Count => 0;

        public void OnWriteValue(string key, IValueReader valueReader)
        {
            ((IDataWriter<string>)this)[key].DirectWrite(valueReader.DirectRead());
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            ((IDataWriter<int>)this)[key].DirectWrite(valueReader.DirectRead());
        }

        public void OnWriteAll(IDataReader<string> dataReader)
        {
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
        }

        public long Id => id;
    }
}
