using System;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 
    /// </summary>
    public sealed unsafe class GuidHelper
    {
        /// <summary>
        /// Guid 字符串包含分隔符的长度。
        /// </summary>
        public const int GuidStringWithSeparatorsLength = 36;

        /// <summary>
        /// Guid 字符串的长度。
        /// </summary>
        public const int GuidStringLength = 32;

        /// <summary>
        /// Guid 分隔符
        /// </summary>
        public const char Separator = '-';

        /// <summary>
        /// Guid 可选开始符
        /// </summary>
        public const char BeginCharacter = '{';

        /// <summary>
        /// Guid 可选结束符
        /// </summary>
        public const char EndCharacter = '}';


        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static bool TryParseHexByte(char* chars, out byte value)
        {
            var a = NumberHelper.Hex.ToRadix(chars[0]);
            var b = NumberHelper.Hex.ToRadix(chars[1]);

            value = (byte)((a << 4) | b);

            return (a | b) < 16;
        }

        /// <summary>
        /// 尝试从字符串开始位置解析一个 Guid 值。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <returns>解析成功则返回解析的长度，失败则返回 0</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static (ParseCode code, int length, Guid value) ParseGuid(char* chars, int length)
        {
            if (length < 32)
            {
                goto Error;
            }

            var offset = chars;

            var hasBeginCharacter = false;

            if (*offset == BeginCharacter)
            {
                hasBeginCharacter = true;

                ++offset;
                --length;
            }

            if (!TryParseHexByte(offset, out var a1)) goto False; offset += 2;
            if (!TryParseHexByte(offset, out var a2)) goto False; offset += 2;
            if (!TryParseHexByte(offset, out var a3)) goto False; offset += 2;
            if (!TryParseHexByte(offset, out var a4)) goto False; offset += 2;


            if (*offset == Separator)
            {
                ++offset;
                --length;
            }

            if (!TryParseHexByte(offset, out var b1)) goto False; offset += 2;
            if (!TryParseHexByte(offset, out var b2)) goto False; offset += 2;

            if (*offset == Separator)
            {
                ++offset;
                --length;
            }

            if (!TryParseHexByte(offset, out var c1)) goto False; offset += 2;
            if (!TryParseHexByte(offset, out var c2)) goto False; offset += 2;

            if (*offset == Separator)
            {
                ++offset;
                --length;
            }

            if (!TryParseHexByte(offset, out var d)) goto False; offset += 2;
            if (!TryParseHexByte(offset, out var e)) goto False; offset += 2;

            if (*offset == Separator)
            {
                ++offset;
                --length;
            }

            if (length < 32)
            {
                goto Error;
            }

            if (!TryParseHexByte(offset, out var f)) goto False; offset += 2;
            if (!TryParseHexByte(offset, out var g)) goto False; offset += 2;
            if (!TryParseHexByte(offset, out var h)) goto False; offset += 2;
            if (!TryParseHexByte(offset, out var i)) goto False; offset += 2;
            if (!TryParseHexByte(offset, out var j)) goto False; offset += 2;
            if (!TryParseHexByte(offset, out var k)) goto False; offset += 2;

            if (*offset == EndCharacter && hasBeginCharacter)
            {
                ++offset;
                --length;
            }
            else if (hasBeginCharacter)
            {
                goto Error;
            }

            return (ParseCode.Success, (int)(offset - chars), new Guid(
                (int)(a1 | (a2 << 8) | (a3 << 16) | (a4 << 24)),
                (short)(b1 | (b2 << 8)),
                (short)(c1 | (c2 << 8)),
                d, e, f, g, h, i, j, k
                ));

            Error:
            return (ParseCode.WrongFormat, 0, default);

            False:
            return (ParseCode.OutOfRadix, 0, default);
        }

        /// <summary>
        /// 尝试解析一个 Guid 值。
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="value">返回 一个 Guid 值</param>
        /// <returns>返回是否解析成功</returns>
        public static bool TryParseGuid(Ps<char> str, out Guid value)
        {
            int length;

            (_, length, value) = ParseGuid(str.Pointer, str.Length);

            return length == str.Length;
        }

        /// <summary>
        /// 尝试解析一个 Guid 值。
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="value">返回 一个 Guid 值</param>
        /// <returns>返回是否解析成功</returns>
        public static bool TryParseGuid(string str, out Guid value)
        {
            fixed (char* pStr = str) return TryParseGuid(new Ps<char>(pStr, str.Length), out value);
        }

        /// <summary>
        /// 将一个 Guid 值写入到一个空间足够的字符串中。
        /// </summary>
        /// <param name="value">Guid 值</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <param name="withSeparator">是否包含分隔符</param>
        /// <returns>返回写入长度。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int ToString(Guid value, char* chars, bool withSeparator)
        {
            var hex = NumberHelper.Hex;
            var offset = chars;
            var ptr = (GuidStruct*)&value;

            hex.AppendD2(offset, ptr->_a1); offset += 2;
            hex.AppendD2(offset, ptr->_a2); offset += 2;
            hex.AppendD2(offset, ptr->_a3); offset += 2;
            hex.AppendD2(offset, ptr->_a4); offset += 2;

            if (withSeparator)
            {
                *offset = Separator; ++offset;
            }

            hex.AppendD2(offset, ptr->_b1); offset += 2;
            hex.AppendD2(offset, ptr->_b2); offset += 2;

            if (withSeparator)
            {
                *offset = Separator; ++offset;
            }

            hex.AppendD2(offset, ptr->_c1); offset += 2;
            hex.AppendD2(offset, ptr->_c2); offset += 2;

            if (withSeparator)
            {
                *offset = Separator; ++offset;
            }

            hex.AppendD2(offset, ptr->_d); offset += 2;
            hex.AppendD2(offset, ptr->_e); offset += 2;

            if (withSeparator)
            {
                *offset = Separator; ++offset;
            }

            hex.AppendD2(offset, ptr->_f); offset += 2;
            hex.AppendD2(offset, ptr->_g); offset += 2;
            hex.AppendD2(offset, ptr->_h); offset += 2;
            hex.AppendD2(offset, ptr->_i); offset += 2;
            hex.AppendD2(offset, ptr->_j); offset += 2;
            hex.AppendD2(offset, ptr->_k); offset += 2;

            return (int)(offset - chars);
        }
    }
}
