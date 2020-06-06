using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供 UTF8 编码工具方法。
    /// </summary>
    unsafe partial class StringHelper
    {
        /// <summary>
        /// 一个 UTF16 字符在 UTF8 中最多需要多少 Byte 存储。
        /// </summary>
        public const int NumberOfUtf8PerUtf16 = 3;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="length"></param>
        /// <param name="bytes"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetUtf8Bytes(ref char chars, int length, byte* bytes)
        {
            var offset = bytes;

            while (length >= 4 && (Underlying.As<char, ulong>(ref chars) & 0xff80ff80ff80ff80) == 0)
            {
                *(uint*)offset = Close(Underlying.As<char, ulong>(ref chars));

                chars = ref Underlying.Add(ref chars, 4);
                length -= 4;
                offset += 4;
            }

            for (int i = 0; i < length; i++)
            {
                int @char = Underlying.Add(ref chars, i);

                if (@char <= 0x7f)
                {
                    *offset = (byte)@char; ++offset;
                }
                else if (@char <= 0x7ff)
                {
                    offset[0] = (byte)(0xc0 | (@char >> 6));
                    offset[1] = (byte)(0x80 | (@char & 0x3f));

                    offset += 2;
                }
                else if (@char >= 0xd800 && @char <= 0xdbff)
                {
                    @char = (((@char & 0x3ff) << 10) | (Underlying.Add(ref chars, ++i) & 0x3ff)) + 0x10000;

                    offset[0] = (byte)(0xf0 | (@char >> 18));
                    offset[1] = (byte)(0x80 | ((@char >> 12) & 0x3f));
                    offset[2] = (byte)(0x80 | ((@char >> 6) & 0x3f));
                    offset[3] = (byte)(0x80 | (@char & 0x3f));

                    offset += 4;
                }
                else
                {
                    offset[0] = (byte)(0xe0 | (@char >> 12));
                    offset[1] = (byte)(0x80 | ((@char >> 6) & 0x3f));
                    offset[2] = (byte)(0x80 | (@char & 0x3f));

                    offset += 3;
                }
            }

            return (int)(offset - bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static uint Close(ulong value)
            => Close2((value | (value >> 8)) & 0x00007f7f00007f7fu);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static uint Close2(ulong value)
            => (uint)(value | (value >> 16));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        /// <param name="chars"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetUtf8Chars(byte* bytes, int length, ref char chars)
        {
            var num = 0;

            while (length >= 4 && (*(uint*)bytes & 0x80808080) == 0)
            {
                Underlying.As<char, ulong>(ref Underlying.Add(ref chars, num)) = Open(*(uint*)bytes);

                bytes += 4;
                length -= 4;
                num += 4;
            }

            for (var end = bytes + length; bytes < end; bytes++, num++)
            {
                int @byte = *bytes;

                if (@byte <= 0x7f)
                {
                    Underlying.Add(ref chars, num) = (char)@byte;
                }
                else if (@byte <= 0xdf)
                {
                    Underlying.Add(ref chars, num) = (char)(((@byte & 0x1f) << 6) | (*(++bytes) & 0x3f));
                }
                else if (@byte <= 0xef)
                {
                    Underlying.Add(ref chars, num) = (char)(((@byte & 0xf) << 12) | ((*(++bytes) & 0x3f) << 6) + (*(++bytes) & 0x3f));
                }
                else
                {
                    @byte = (((@byte & 0x7) << 18) | ((*(++bytes) & 0x3f) << 12) | ((*(++bytes) & 0x3f) << 6) + (*(++bytes) & 0x3f)) - 0x10000;

                    Underlying.Add(ref chars, num) = (char)(0xd800 | (@byte >> 10)); ++num;
                    Underlying.Add(ref chars, num) = (char)(0xdc00 | (@byte & 0x3ff));
                }
            }

            return num;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static bool Equal(char x, char y, bool ignoreCase)
            => x == y || (ignoreCase && ToLower(x) == ToLower(y));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="bytesLength"></param>
        /// <param name="firstChar"></param>
        /// <param name="charCount"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool Equals(byte* bytes, int bytesLength, ref char firstChar, int charCount, bool ignoreCase)
        {
            if (charCount <= GetUtf8MaxCharsLength(bytesLength) && bytesLength <= GetUtf8MaxBytesLength(charCount))
            {
                int charOffset = 0;

                for (var end = bytes + bytesLength; bytes < end; bytes++, charOffset++)
                {
                    int @byte = *bytes;

                    if (@byte <= 0x7f)
                    {
                        if (charOffset < charCount && Equal(
                            Underlying.Add(ref firstChar, charOffset),
                            (char)@byte,
                            ignoreCase)) continue;
                    }
                    else if (@byte <= 0xdf)
                    {
                        if (charOffset < charCount && Equal(
                            Underlying.Add(ref firstChar, charOffset),
                            (char)(((@byte & 0x1f) << 6) | (*(++bytes) & 0x3f)),
                            ignoreCase)) continue;
                    }
                    else if (@byte <= 0xef)
                    {
                        if (charOffset < charCount && Equal(
                            Underlying.Add(ref firstChar, charOffset),
                            (char)(((@byte & 0xf) << 12) | ((*(++bytes) & 0x3f) << 6) + (*(++bytes) & 0x3f)),
                            ignoreCase)) continue;
                    }
                    else
                    {
                        @byte = (((@byte & 0x7) << 18) | ((*(++bytes) & 0x3f) << 12) | ((*(++bytes) & 0x3f) << 6) + (*(++bytes) & 0x3f)) - 0x10000;

                        if (charOffset < charCount
                            && Equal(
                                Underlying.Add(ref firstChar, charOffset),
                                (char)(0xd800 | (@byte >> 10)),
                                ignoreCase)
                            && (++charOffset) < charCount
                            && Equal(
                                Underlying.Add(ref firstChar, charOffset),
                                (char)(0xdc00 | (@byte & 0x3ff)),
                                ignoreCase)) continue;
                    }

                    return false;
                }

                return charOffset == charCount;
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static ulong Open(uint value) =>
            Open2((value | ((ulong)value << 16)) & 0x0000ffff0000ffff);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static ulong Open2(ulong value) =>
            (value | (value << 8)) & 0x00ff00ff00ff00ff;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetUtf8CharsLength(byte* bytes, int length)
        {
            var num = 0;

            while (length >= 4 && (*(uint*)bytes & 0x80808080) == 0)
            {
                bytes += 4;
                length -= 4;
                num += 4;
            }

            while (length > 0)
            {
                if (*bytes <= 0x7f) { ++bytes; --length; ++num; }
                else if (*bytes <= 0xdf) { bytes += 2; length -= 2; ++num; }
                else if (*bytes <= 0xef) { bytes += 3; length -= 3; ++num; }
                else { bytes += 4; length -= 4; num += 2; }
            }

            return num;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetUtf8BytesLength(ref char chars, int length)
        {
            var num = 0;

            for (int i = 0; i < length; i++)
            {
                if (chars <= 0x7f) { ++num; }
                else if (chars <= 0x7ff) { num += 2; }
                else if (chars >= 0xd800 && chars <= 0xdbff) { ++i; num += 4; }
                else { num += 3; }
            }

            return num;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="charsLength"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetUtf8MaxBytesLength(int charsLength) 
            => (charsLength + 1) * NumberOfUtf8PerUtf16;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bytesLength"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetUtf8MaxCharsLength(int bytesLength) 
            => bytesLength;
    }
}