using System.Runtime.CompilerServices;

namespace Swifter.MessagePack
{
    public static unsafe class EncodingHelper
    {
        public const char ASCIIMaxChar = (char)0x7f;
        public const int Utf8MaxBytesCount = 4;

        public static int GetUtf8Bytes(char* chars, int length, byte* bytes)
        {
            var destination = bytes;

            for (int i = 0; i < length; i++)
            {
                int c = chars[i];

                if (c <= 0x7f)
                {
                    *destination = (byte)c; ++destination;
                }
                else if (c <= 0x7ff)
                {
                    *destination = (byte)(0xc0 | (c >> 6)); ++destination;
                    *destination = (byte)(0x80 | (c & 0x3f)); ++destination;
                }
                else if (c >= 0xd800 && c <= 0xdbff)
                {
                    c = ((c & 0x3ff) << 10) + 0x10000;

                    ++i;

                    if (i < length)
                    {
                        c |= chars[i] & 0x3ff;
                    }

                    *destination = (byte)(0xf0 | (c >> 18)); ++destination;
                    *destination = (byte)(0x80 | ((c >> 12) & 0x3f)); ++destination;
                    *destination = (byte)(0x80 | ((c >> 6) & 0x3f)); ++destination;
                    *destination = (byte)(0x80 | (c & 0x3f)); ++destination;
                }
                else
                {
                    *destination = (byte)(0xe0 | (c >> 12)); ++destination;
                    *destination = (byte)(0x80 | ((c >> 6) & 0x3f)); ++destination;
                    *destination = (byte)(0x80 | (c & 0x3f)); ++destination;
                }
            }

            return (int)(destination - bytes);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetUtf8Chars(byte* bytes, int length, char* chars)
        {
            var destination = chars;

            var current = bytes;
            var end = bytes + length;

            for (; current < end; ++current)
            {
                if ((*((byte*)destination) = *current) > 0x7f)
                {
                    return GetGetUtf8Chars(current, end, destination, chars);
                }

                ++destination;
            }

            return (int)(destination - chars);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetGetUtf8Chars(byte* current, byte* end, char* destination, char* chars)
        {
            if (current + Utf8MaxBytesCount < end)
            {
                end -= Utf8MaxBytesCount;

                // Unchecked index.
                for (; current < end; ++current)
                {
                    var byt = *current;

                    if (byt <= 0x7f)
                    {
                        *destination = (char)byt;
                    }
                    else if (byt <= 0xdf)
                    {
                        *destination = (char)(((byt & 0x1f) << 6) | (current[1] & 0x3f));

                        ++current;
                    }
                    else if (byt <= 0xef)
                    {
                        *destination = (char)(((byt & 0xf) << 12) | ((current[1] & 0x3f) << 6) + (current[2] & 0x3f));

                        current += 2;
                    }
                    else
                    {
                        var utf32 = (((byt & 0x7) << 18) | ((current[1] & 0x3f) << 12) | ((current[2] & 0x3f) << 6) + (current[3] & 0x3f)) - 0x10000;

                        *destination = (char)(0xd800 | (utf32 >> 10)); ++destination;
                        *destination = (char)(0xdc00 | (utf32 & 0x3ff));

                        current += 3;
                    }

                    ++destination;
                }

                end += Utf8MaxBytesCount;
            }

            // Checked index.
            for (; current < end; ++current)
            {
                var byt = *current;

                if (byt <= 0x7f)
                {
                    *destination = (char)byt;
                }
                else if (byt <= 0xdf && current + 1 < end)
                {
                    *destination = (char)(((byt & 0x1f) << 6) | (current[1] & 0x3f));

                    ++current;
                }
                else if (byt <= 0xef && current + 2 < end)
                {
                    *destination = (char)(((byt & 0xf) << 12) | ((current[1] & 0x3f) << 6) + (current[2] & 0x3f));

                    current += 2;
                }
                else if (current + 3 < end)
                {
                    var utf32 = (((byt & 0x7) << 18) | ((current[1] & 0x3f) << 12) | ((current[2] & 0x3f) << 6) + (current[3] & 0x3f)) - 0x10000;

                    *destination = (char)(0xd800 | (utf32 >> 10)); ++destination;
                    *destination = (char)(0xdc00 | (utf32 & 0x3ff));

                    current += 3;
                }

                ++destination;
            }

            return (int)(destination - chars);
        }

        public static int GetUtf8CharsLength(byte* bytes, int length)
        {
            int count = 0;

            for (int i = 0; i < length; i += bytes[i] <= 0x7f ? 1 : bytes[i] <= 0xdf ? 2 : 3)
            {
                ++count;
            }

            return count;
        }

        public static int GetUtf8MaxBytesLength(int charsLength)
        {
            return charsLength * Utf8MaxBytesCount;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetUtf8MaxCharsLength(int bytesLength)
        {
            return bytesLength;
        }
    }
}