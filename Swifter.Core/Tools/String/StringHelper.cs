using System.Runtime.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Buffers;

#if Vector

using System.Numerics;

#endif

namespace Swifter.Tools
{
    /// <summary>
    /// 字符串辅助类
    /// </summary>
    public static unsafe partial class StringHelper
    {
        /// <summary>
        /// HashCode 的乘数。
        /// </summary>
        const int Mult = unchecked((int)0x97121819);

        /// <summary>
        /// 获取指定 utf-8 字符串的长度。
        /// </summary>
        /// <param name="utf8">指定 utf-8 字符串</param>
        /// <returns>返回该字符串的长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetLength(this Ps<Utf8Byte> utf8) => utf8.Length;

        /// <summary>
        /// 获取指定 utf-16 字符串的长度。
        /// </summary>
        /// <param name="utf16">指定 utf-16 字符串</param>
        /// <returns>返回该字符串的长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetLength(this Ps<char> utf16) => utf16.Length;

        /// <summary>
        /// 判断两个 utf-16 字符串是否相等。
        /// </summary>
        /// <param name="utf16x">x</param>
        /// <param name="utf16y">y</param>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool Equals(Ps<char> utf16x, Ps<char> utf16y)
        {
            if (utf16x.Length != utf16y.Length)
            {
                return false;
            }

            var i = utf16x.Length;

            while (i >= 4)
            {
                i -= 4;

                if (*(long*)(utf16x.Pointer + i) != *(long*)(utf16y.Pointer + i))
                {
                    return false;
                }
            }

            if (i >= 2)
            {
                i -= 2;

                if (*(int*)(utf16x.Pointer + i) != *(int*)(utf16y.Pointer + i))
                {
                    return false;
                }
            }

            if (i >= 1)
            {
                if (*(utf16x.Pointer) != *(utf16y.Pointer))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 判断两个 utf-8 字符串是否相等。
        /// </summary>
        /// <param name="utf8x">x</param>
        /// <param name="utf8y">y</param>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool Equals(Ps<Utf8Byte> utf8x, Ps<Utf8Byte> utf8y)
        {
            if (utf8x.Length != utf8y.Length)
            {
                return false;
            }

            var i = utf8x.Length;

            while (i >= 8)
            {
                i -= 8;

                if (*(long*)(utf8x.Pointer + i) != *(long*)(utf8y.Pointer + i))
                {
                    return false;
                }
            }

            if (i >= 4)
            {
                i -= 4;

                if (*(int*)(utf8x.Pointer + i) != *(int*)(utf8y.Pointer + i))
                {
                    return false;
                }
            }

            if (i >= 2)
            {
                i -= 2;

                if (*(short*)(utf8x.Pointer + i) != *(short*)(utf8y.Pointer + i))
                {
                    return false;
                }
            }

            if (i >= 1)
            {
                if (*(utf8x.Pointer) != *(utf8y.Pointer))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 判断两个 utf-16 字符串是否相等。
        /// </summary>
        /// <param name="strx">x</param>
        /// <param name="stry">y</param>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool Equals(string strx, string stry)
            => strx.Equals(stry);

        /// <summary>
        /// 获取一个 utf-16 字符串的 Hash 值。
        /// </summary>
        /// <param name="utf16">字符串</param>
        /// <returns>返回一个 hash 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetHashCode(Ps<char> utf16)
        {
            int r = 0;

            for (int i = 0; i < utf16.Length; ++i)
            {
                r ^= (utf16.Pointer[i] + i) * Mult;
            }

            return r;
        }

        /// <summary>
        /// 获取一个 utf-8 字符串的 Hash 值。
        /// </summary>
        /// <param name="utf8">字符串</param>
        /// <returns>返回一个 hash 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetHashCode(Ps<Utf8Byte> utf8)
        {
            int r = 0;

            for (int i = 0; i < utf8.Length; ++i)
            {
                r ^= (utf8.Pointer[i] + i) * Mult;
            }

            return r;
        }

        /// <summary>
        /// 获取字符串 Hash 值。
        /// </summary>
        /// <param name="str">字符串。</param>
        /// <returns>返回一个 int hash 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetHashCode(string str)
        {
            int r = 0;

            for (int i = 0; i < str.Length; ++i)
            {
                r ^= (str[i] + i) * Mult;
            }

            return r;
        }

        /// <summary>
        /// 判断两个 utf-16 字符串是否相等。
        /// </summary>
        /// <param name="utf16x">x</param>
        /// <param name="utf16y">y</param>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool EqualsWithIgnoreCase(Ps<char> utf16x, Ps<char> utf16y)
        {
            if (utf16x.Length != utf16y.Length)
            {
                return false;
            }

            for (int i = 0; i < utf16x.Length; i++)
            {
                if (utf16x.Pointer[i] != utf16y.Pointer[i] && ToLower(utf16x.Pointer[i]) != ToLower(utf16y.Pointer[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 判断一个字符串和一个 utf-16 字符串是否相等。
        /// </summary>
        /// <param name="utf16">x</param>
        /// <param name="str">y</param>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool EqualsWithIgnoreCase(Ps<char> utf16, string str)
        {
            if (utf16.Length != str.Length)
            {
                return false;
            }

            for (int i = 0; i < utf16.Length; i++)
            {
                if (utf16.Pointer[i] != str[i] && ToLower(utf16.Pointer[i]) != ToLower(str[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 判断一个 utf8 字符串和一个字符串是否相等。
        /// </summary>
        /// <param name="utf8">x</param>
        /// <param name="str">y</param>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool EqualsWithIgnoreCase(Ps<Utf8Byte> utf8, string str)
            => Equals((byte*)utf8.Pointer, utf8.Length, ref GetRawStringData(str), str.Length, true);

        /// <summary>
        /// 判断两个 utf-8 字符串是否相等。
        /// </summary>
        /// <param name="utf8x">x</param>
        /// <param name="utf8y">y</param>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool EqualsWithIgnoreCase(Ps<Utf8Byte> utf8x, Ps<Utf8Byte> utf8y)
        {
            if (utf8x.Length != utf8y.Length)
            {
                return false;
            }

            for (int i = 0; i < utf8x.Length; i++)
            {
                if (utf8x.Pointer[i] != utf8y.Pointer[i] && ToLower(utf8x.Pointer[i]) != ToLower(utf8y.Pointer[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 判断两个 utf-16 字符串是否相等。
        /// </summary>
        /// <param name="strx">x</param>
        /// <param name="stry">y</param>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool EqualsWithIgnoreCase(string strx, string stry)
            => strx.Equals(stry, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 获取一个 utf-16 字符串的 Hash 值。
        /// </summary>
        /// <param name="utf16">字符串</param>
        /// <returns>返回一个 hash 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetHashCodeWithIgnoreCase(Ps<char> utf16)
        {
            int r = 0;

            for (int i = 0; i < utf16.Length; ++i)
            {
                r ^= (ToLower(utf16.Pointer[i]) + i) * Mult;
            }

            return r;
        }

        /// <summary>
        /// 获取一个 utf-8 字符串的 Hash 值。
        /// </summary>
        /// <param name="utf8">字符串</param>
        /// <returns>返回一个 hash 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetHashCodeWithIgnoreCase(Ps<Utf8Byte> utf8)
        {
            int r = 0;

            for (int i = 0; i < utf8.Length; ++i)
            {
                r ^= (ToLower(utf8.Pointer[i]) + i) * Mult;
            }

            return r;
        }

        /// <summary>
        /// 获取字符串 Hash 值。
        /// </summary>
        /// <param name="str">字符串。</param>
        /// <returns>返回一个 int hash 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetHashCodeWithIgnoreCase(string str)
        {
            int r = 0;

            for (int i = 0; i < str.Length; ++i)
            {
                r ^= (ToLower(str[i]) + i) * Mult;
            }

            return r;
        }

        /// <summary>
        /// 将 utf-16 字符集合转换为字符串。
        /// </summary>
        /// <param name="utf16">utf-16 字符集合</param>
        /// <returns>返回一个字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string ToStringEx(this Ps<char> utf16)
        {
            return ToString(utf16.Pointer, utf16.Length);
        }

        /// <summary>
        /// 将 utf-8 字符集合转换为字符串。
        /// </summary>
        /// <param name="utf8">utf-8 字符集合</param>
        /// <returns>返回一个字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string ToStringEx(this Ps<Utf8Byte> utf8)
        {
            var chars = HGlobalCacheExtensions.CharsPool.Current().Grow(GetUtf8MaxCharsLength(utf8.Length)).First;

            return ToString(chars, GetUtf8Chars((byte*)utf8.Pointer, utf8.Length, ref chars[0]));
        }

        /// <summary>
        /// 获取指定 utf-16 字符串中位于指定索引处的字符。
        /// </summary>
        /// <param name="str">指定 utf-16 字符串</param>
        /// <param name="index">指定索引</param>
        /// <returns>返回一个字符</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static char CharAt(this Ps<char> str, int index) => str.Pointer[index];

        /// <summary>
        /// 获取指定 utf-8 字符串中位于指定索引处的字符。
        /// </summary>
        /// <param name="str">指定 utf-8 字符串</param>
        /// <param name="index">指定索引</param>
        /// <returns>返回一个字符</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static Utf8Byte CharAt(this Ps<Utf8Byte> str, int index) => str.Pointer[index];

        /// <summary>
        /// 将 utf-8 字节码转换为字节码。
        /// </summary>
        /// <param name="value">utf-8 字节码</param>
        /// <returns>返回字节码</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static byte AsByte(Utf8Byte value) => Underlying.As<Utf8Byte, byte>(ref value);

        /// <summary>
        /// 去除字符串两端的空白字符，然后返回一个新的字符串。
        /// </summary>
        /// <param name="chars">原始字符串</param>
        /// <param name="length">原始长度</param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static Ps<char> Trim(char* chars, int length)
        {
            while (length > 0 && IsWhiteSpace(*chars))
            {
                ++chars;
                --length;
            }

            while (length > 0 && IsWhiteSpace(chars[length - 1]))
            {
                --length;
            }

            return new Ps<char>(chars, length);
        }
        /// <summary>
        /// 去除字符串头部的空白字符，然后返回一个新的字符串。
        /// </summary>
        /// <param name="chars">原始字符串</param>
        /// <param name="length">原始长度</param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static Ps<char> TrimStart(char* chars, int length)
        {
            while (length > 0 && IsWhiteSpace(*chars))
            {
                ++chars;
                --length;
            }

            return new Ps<char>(chars, length);
        }
        /// <summary>
        /// 去除字符串尾部的空白字符，然后返回一个新的字符串。
        /// </summary>
        /// <param name="chars">原始字符串</param>
        /// <param name="length">原始长度</param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static Ps<char> TrimEnd(char* chars, int length)
        {
            while (length > 0 && IsWhiteSpace(chars[length - 1]))
            {
                --length;
            }

            return new Ps<char>(chars, length);
        }

        /// <summary>
        /// 将小写英文字符转为大写英文字符。
        /// </summary>
        /// <param name="c"></param>
        /// <returns>返回一个字符。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static char ToUpper(char c)
            => c >= 'a' && c <= 'z' ? (char)(c & (~0x20)) : c;

        /// <summary>
        /// 将大写英文字符转为小写英文字符。
        /// </summary>
        /// <param name="c"></param>
        /// <returns>返回一个字符。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static char ToLower(char c)
            => c >= 'A' && c <= 'Z' ? (char)(c | 0x20) : c;

        /// <summary>
        /// 将小写英文字符转为大写英文字符。
        /// </summary>
        /// <param name="c"></param>
        /// <returns>返回一个字符。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static Utf8Byte ToUpper(Utf8Byte c)
            => c >= 'a' && c <= 'z' ? (Utf8Byte)((c) & (~0x20)) : c;

        /// <summary>
        /// 将大写英文字符转为小写英文字符。
        /// </summary>
        /// <param name="c"></param>
        /// <returns>返回一个字符。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static Utf8Byte ToLower(Utf8Byte c)
        {
            if (c >= 'A' && c <= 'Z')
            {
                return (Utf8Byte)((c) | 0x20);
            }

            return c;
        }

        /// <summary>
        /// 快速分配指定长度的字符串。
        /// </summary>
        /// <param name="length">指定长度</param>
        /// <returns>返回一个字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string MakeString(int length)
        {
            return new string(default, length);
        }

        /// <summary>
        /// 判断一个字符是否为空白字符
        /// </summary>
        /// <param name="c">字符</param>
        /// <returns>返回一个 Boolean 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IsWhiteSpace(char c)
        {
            return c == 0x20 || (c >= 0x9 && c <= 0xd) || c == 0x85 || c == 0xa0;
        }

        /// <summary>
        /// 在字符串中找到指定字符的索引，没找到则返回 -1。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="c">字符</param>
        /// <param name="length">字符串长度</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int IndexOf(char* chars, int length, char c)
        {
#if Vector

            if (Vector.IsHardwareAccelerated)
            {
                return FastIndexOf(chars, length, c);
            }

#endif
            return SlowIndexOf(chars, length, c);
        }

        /// <summary>
        /// 在字符串中找到指定两个字符中任意字符的索引，没找到则返回 -1。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="c1">字符1</param>
        /// <param name="c2">字符2</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int IndexOfAny(char* chars, int length, char c1, char c2)
        {
#if Vector

            if (Vector.IsHardwareAccelerated)
            {
                return FastIndexOfAny(chars, length, c1, c2);
            }

#endif
            return SlowIndexOfAny(chars, length, c1, c2);
        }

        /// <summary>
        /// 检索字符串中是否存在指定字符。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="c">指定字符</param>
        /// <returns>返回是否存在</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool Contains(char* chars, int length, char c)
        {
#if Vector

            if (Vector.IsHardwareAccelerated)
            {
                return FastContains(chars, length, c);
            }

#endif
            return SlowContains(chars, length, c);
        }

        /// <summary>
        /// 获取字符串的元数据引用。
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>返回第一个字符的引用</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref char GetRawStringData(string str)
            => ref Underlying.As<IntPtr, char>(ref Underlying.AddByteOffset(ref Underlying.GetMethodTablePointer(str), RuntimeHelpers.OffsetToStringData));

        /// <summary>
        /// 判断一个字符串是否所有的字符都为 ASCII 字符。
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IsASCIIString(string str)
        {
            var length = str.Length;

            ref var first = ref GetRawStringData(str);

#if Vector
            if (Vector.IsHardwareAccelerated && length >= Vector<ushort>.Count)
            {
                var comparison = new Vector<ushort>(0x7f);

                do
                {
                    if (Vector.GreaterThan(Underlying.As<char, Vector<ushort>>(ref first), comparison) != Vector<ushort>.Zero)
                    {
                        return false;
                    }

                    length -= Vector<ushort>.Count;
                    first = ref Underlying.Add(ref first, Vector<ushort>.Count);

                } while (length >= Vector<ushort>.Count);
            }

#endif

            while (length >= 4)
            {
                if ((Underlying.As<char, ulong>(ref first) & 0xff80ff80ff80ff80) != 0)
                {
                    return false;
                }

                length -= 4;
                first = ref Underlying.Add(ref first, 4);
            }

            if (length > 0 && Underlying.Add(ref first, 0) > 0x7f)
                return false;
            if (length > 1 && Underlying.Add(ref first, 1) > 0x7f)
                return false;
            if (length > 2 && Underlying.Add(ref first, 2) > 0x7f)
                return false;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsASCIIString(Ps<Utf8Byte> str)
        {
            var length = str.Length;

            var first = str.Pointer;

            while (length >= 4)
            {
                if (((*(uint*)first) & 0x80808080) != 0)
                {
                    return false;
                }

                length -= 4;
                first += 4;
            }

            if (length > 0 && first[0] > 0x7f)
                return false;
            if (length > 1 && first[1] > 0x7f)
                return false;
            if (length > 2 && first[2] > 0x7f)
                return false;

            return true;
        }

        /// <summary>
        /// 创建一个字符串。
        /// </summary>
        /// <param name="chars">字符串内容</param>
        /// <param name="length">字符串长度</param>
        /// <returns>返回一个新的字符串</returns>
        public static string ToString(char* chars, int length)
        {
            return new string(chars, 0, length);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static int SlowIndexOf(char* chars, int length, char c)
        {
            var index = 0;

            if ((length -= 8) >= 0)
            {
                do
                {
                    if (chars[0] == c) return index + 0;
                    if (chars[1] == c) return index + 1;
                    if (chars[2] == c) return index + 2;
                    if (chars[3] == c) return index + 3;
                    if (chars[4] == c) return index + 4;
                    if (chars[5] == c) return index + 5;
                    if (chars[6] == c) return index + 6;
                    if (chars[7] == c) return index + 7;

                    chars += 8;
                    index += 8;

                } while (index <= length);
            }

            if (index - length <= 7 && chars[0] == c) return index + 0;
            if (index - length <= 6 && chars[1] == c) return index + 1;
            if (index - length <= 5 && chars[2] == c) return index + 2;
            if (index - length <= 4 && chars[3] == c) return index + 3;
            if (index - length <= 3 && chars[4] == c) return index + 4;
            if (index - length <= 2 && chars[5] == c) return index + 5;
            if (index - length <= 1 && chars[6] == c) return index + 6;

            return -1;
        }


        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static int SlowIndexOfAny(char* chars, int length, char c1, char c2)
        {
            var index = 0;

            if ((length -= 8) >= 0)
            {
                do
                {
                    if (chars[0] == c1 || chars[0] == c2) return index + 0;
                    if (chars[1] == c1 || chars[1] == c2) return index + 1;
                    if (chars[2] == c1 || chars[2] == c2) return index + 2;
                    if (chars[3] == c1 || chars[3] == c2) return index + 3;
                    if (chars[4] == c1 || chars[4] == c2) return index + 4;
                    if (chars[5] == c1 || chars[5] == c2) return index + 5;
                    if (chars[6] == c1 || chars[6] == c2) return index + 6;
                    if (chars[7] == c1 || chars[7] == c2) return index + 7;

                    chars += 8;
                    index += 8;

                } while (index <= length);
            }

            if (index - length <= 7 && (chars[0] == c1 || chars[0] == c2)) return index + 0;
            if (index - length <= 6 && (chars[1] == c1 || chars[1] == c2)) return index + 1;
            if (index - length <= 5 && (chars[2] == c1 || chars[2] == c2)) return index + 2;
            if (index - length <= 4 && (chars[3] == c1 || chars[3] == c2)) return index + 3;
            if (index - length <= 3 && (chars[4] == c1 || chars[4] == c2)) return index + 4;
            if (index - length <= 2 && (chars[5] == c1 || chars[5] == c2)) return index + 5;
            if (index - length <= 1 && (chars[6] == c1 || chars[6] == c2)) return index + 6;

            return -1;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static bool SlowContains(char* chars, int length, char c)
        {
            var index = 0;

            if ((length -= 8) >= 0)
            {
                do
                {
                    if (chars[0] == c) return true;
                    if (chars[1] == c) return true;
                    if (chars[2] == c) return true;
                    if (chars[3] == c) return true;
                    if (chars[4] == c) return true;
                    if (chars[5] == c) return true;
                    if (chars[6] == c) return true;
                    if (chars[7] == c) return true;

                    chars += 8;
                    index += 8;

                } while (index <= length);
            }

            if (index - length <= 7 && chars[0] == c) return true;
            if (index - length <= 6 && chars[1] == c) return true;
            if (index - length <= 5 && chars[2] == c) return true;
            if (index - length <= 4 && chars[3] == c) return true;
            if (index - length <= 3 && chars[4] == c) return true;
            if (index - length <= 2 && chars[5] == c) return true;
            if (index - length <= 1 && chars[6] == c) return true;

            return false;
        }


#if Vector

        private const ulong XorPowerOfTwoToHighChar = (0x03ul | 0x02ul << 16 | 0x01ul << 32) + 1;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static int LocateFirstFoundChar(Vector<ushort> matches)
        {
            var match = Underlying.As<Vector<ushort>, Vector<ulong>>(ref matches)[0];
            if (match != 0) return 0 + LocateFirstFoundChar(match);

            match = Underlying.As<Vector<ushort>, Vector<ulong>>(ref matches)[1];
            if (match != 0) return 4 + LocateFirstFoundChar(match);

            match = Underlying.As<Vector<ushort>, Vector<ulong>>(ref matches)[2];
            if (match != 0) return 8 + LocateFirstFoundChar(match);

            match = Underlying.As<Vector<ushort>, Vector<ulong>>(ref matches)[3];
            return 12 + LocateFirstFoundChar(match);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static int LocateFirstFoundChar(ulong match)
            => unchecked((int)(((match ^ (match - 1)) * XorPowerOfTwoToHighChar) >> 49));

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static int FastIndexOf(char* chars, int length, char c)
        {
            var offset = chars;

            if (length >= 8)
            {
                if (offset[0] == c) return 0;
                if (offset[1] == c) return 1;
                if (offset[2] == c) return 2;
                if (offset[3] == c) return 3;
                if (offset[4] == c) return 4;
                if (offset[5] == c) return 5;
                if (offset[6] == c) return 6;
                if (offset[7] == c) return 7;

                offset += 8;
                length -= 8;
            }

            if (length >= Vector<ushort>.Count)
            {
                var comparison = new Vector<ushort>(c);

                do
                {
                    var matches = Vector.Equals(*(Vector<ushort>*)offset, comparison);

                    if (matches != Vector<ushort>.Zero)
                    {
                        return (int)(offset - chars) + LocateFirstFoundChar(matches);
                    }

                    length -= Vector<ushort>.Count;
                    offset += Vector<ushort>.Count;

                } while (length >= Vector<ushort>.Count);
            }

            while (length >= 2)
            {
                if (offset[0] == c) goto Found0;
                if (offset[1] == c) goto Found1;

                offset += 2;
                length -= 2;
            }

            if (length >= 0 && offset[0] == c) goto Found0;

            return -1;

            Found1:
            ++offset;
            Found0:
            return (int)(offset - chars);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static int FastIndexOfAny(char* chars, int length, char c1, char c2)
        {
            var offset = chars;

            if (length >= 8)
            {
                if (offset[0] == c1 || offset[0] == c2) return 0;
                if (offset[1] == c1 || offset[1] == c2) return 1;
                if (offset[2] == c1 || offset[2] == c2) return 2;
                if (offset[3] == c1 || offset[3] == c2) return 3;
                if (offset[4] == c1 || offset[4] == c2) return 4;
                if (offset[5] == c1 || offset[5] == c2) return 5;
                if (offset[6] == c1 || offset[6] == c2) return 6;
                if (offset[7] == c1 || offset[7] == c2) return 7;

                offset += 8;
                length -= 8;
            }

            if (length >= Vector<ushort>.Count)
            {
                var comparison1 = new Vector<ushort>(c1);
                var comparison2 = new Vector<ushort>(c2);

                do
                {
                    var matches = Vector.BitwiseOr(
                        Vector.Equals(*(Vector<ushort>*)offset, comparison1),
                        Vector.Equals(*(Vector<ushort>*)offset, comparison2)
                        );

                    if (matches != Vector<ushort>.Zero)
                    {
                        return (int)(offset - chars) + LocateFirstFoundChar(matches);
                    }

                    length -= Vector<ushort>.Count;
                    offset += Vector<ushort>.Count;

                } while (length >= Vector<ushort>.Count);
            }

            while (length >= 2)
            {
                if (offset[0] == c1 || offset[0] == c2) goto Found0;
                if (offset[1] == c1 || offset[1] == c2) goto Found1;

                offset += 2;
                length -= 2;
            }

            if (length >= 0 && (offset[0] == c1 || offset[0] == c2)) goto Found0;

            return -1;

            Found1:
            ++offset;
            Found0:
            return (int)(offset - chars);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static bool FastContains(char* chars, int length, char c)
        {
            var offset = chars;

            if (length >= 8)
            {
                if (offset[0] == c) return true;
                if (offset[1] == c) return true;
                if (offset[2] == c) return true;
                if (offset[3] == c) return true;
                if (offset[4] == c) return true;
                if (offset[5] == c) return true;
                if (offset[6] == c) return true;
                if (offset[7] == c) return true;

                offset += 8;
                length -= 8;
            }

            if (length >= Vector<ushort>.Count)
            {
                var comparison = new Vector<ushort>(c);

                do
                {
                    var matches = Vector.Equals(*(Vector<ushort>*)offset, comparison);

                    if (matches != Vector<ushort>.Zero)
                    {
                        return true;
                    }

                    length -= Vector<ushort>.Count;
                    offset += Vector<ushort>.Count;

                } while (length >= Vector<ushort>.Count);
            }

            while (length >= 2)
            {
                if (offset[0] == c) return true;
                if (offset[1] == c) return true;

                offset += 2;
                length -= 2;
            }

            if (length >= 0 && offset[0] == c) return true;

            return false;
        }
#endif
    }
}