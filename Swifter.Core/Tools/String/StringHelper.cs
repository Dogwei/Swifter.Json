using Swifter.RW;
using System.Runtime.CompilerServices;
using System.Text;

namespace Swifter.Tools
{
    /// <summary>
    /// 字符串辅助类
    /// </summary>
    public static class StringHelper
    {
        /// <summary>
        /// HashCode 的乘数。
        /// </summary>
        public const int Mult = 1234567891;

        /// <summary>
        /// 颠倒字符串内容。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns>返回一个新的字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe string Reverse(string text)
        {
            var result = MakeString(text.Length);

            fixed (char* pResult = result)
            {
                for (int i = 0, j = text.Length - 1; j >= 0; i++, --j)
                {
                    pResult[i] = text[j];
                }
            }

            return result;
        }

        /// <summary>
        /// 获取字符串大写形式的 Hash 值。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <returns>返回 Hash 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe int GetUpperedHashCode(char* chars, int length)
        {
            int r = 0;

            for (int i = 0; i < length; ++i)
            {
                r ^= ToUpper(chars[i]) * Mult;
            }

            return r;
        }

        /// <summary>
        /// 获取字符串小写形式的 Hash 值。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <returns>返回 Hash 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe int GetLoweredHashCode(char* chars, int length)
        {
            int r = 0;

            for (int i = 0; i < length; ++i)
            {
                r ^= ToLower(chars[i]) * Mult;
            }

            return r;
        }

        /// <summary>
        /// 获取字符串的 Hash 值。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <returns>返回 Hash 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe int GetHashCode(char* chars, int length)
        {
            int r = 0;

            for (int i = 0; i < length; ++i)
            {
                r ^= chars[i] * Mult;
            }

            return r;
        }

        /// <summary>
        /// 忽略大小写获取字符串 Hash 值。
        /// </summary>
        /// <param name="st">字符串。</param>
        /// <returns>返回一个 int hash 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetUpperedHashCode(string st)
        {
            int r = 0;

            for (int i = 0; i < st.Length; ++i)
            {
                r ^= ToUpper(st[i]) * Mult;
            }

            return r;
        }

        /// <summary>
        /// 忽略大小写获取字符串 Hash 值。
        /// </summary>
        /// <param name="st">字符串。</param>
        /// <returns>返回一个 int hash 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetLoweredHashCode(string st)
        {
            int r = 0;

            for (int i = 0; i < st.Length; ++i)
            {
                r ^= ToLower(st[i]) * Mult;
            }

            return r;
        }

        /// <summary>
        /// 获取字符串 Hash 值。
        /// </summary>
        /// <param name="st">字符串。</param>
        /// <returns>返回一个 int hash 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetHashCode(string st)
        {
            int r = 0;
            
            for (int i = 0; i < st.Length; ++i)
            {
                r ^= st[i] * Mult;
            }

            return r;
        }

        /// <summary>
        /// 匹配两个字符串。
        /// </summary>
        /// <param name="st1">字符串 1</param>
        /// <param name="st2">字符串 2</param>
        /// <returns>返回一个 bool 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool Equals(string st1, string st2)
        {
            int length = st1.Length;

            if (length != st2.Length)
            {
                return false;
            }

            while (--length >= 0)
            {
                if (st1[length] != st2[length])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 忽略大小写匹配两个字符串。请确保字符串 2 是已大写的。
        /// </summary>
        /// <param name="st1">字符串 1</param>
        /// <param name="st2">字符串 2</param>
        /// <returns>返回一个 bool 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IgnoreCaseEqualsByUpper(string st1, string st2)
        {
            int length = st1.Length;

            if (length != st2.Length)
            {
                return false;
            }

            while (--length >= 0)
            {
                if (ToUpper(st1[length]) != st2[length])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 忽略大小写匹配两个字符串。
        /// </summary>
        /// <param name="st1">字符串 1</param>
        /// <param name="st2">字符串 2</param>
        /// <returns>返回一个 bool 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IgnoreCaseEqualsByLower(string st1, string st2)
        {
            int length = st1.Length;

            if (length != st2.Length)
            {
                return false;
            }

            while (--length >= 0)
            {
                if (ToLower(st1[length]) != ToLower(st2[length]))
                {
                    return false;
                }
            }

            return true;
        }
        
        /// <summary>
        /// 比较两个字符串。
        /// </summary>
        /// <param name="chars">字符串 1</param>
        /// <param name="length">字符串 1 长度</param>
        /// <param name="str">字符串 2</param>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe bool Equals(char* chars, int length, string str)
        {
            if (length != str.Length)
            {
                return false;
            }

            while (--length >= 0)
            {
                if (chars[length] != str[length])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 忽略大小写比较两个字符串。
        /// </summary>
        /// <param name="chars">字符串 1</param>
        /// <param name="length">字符串 1 长度</param>
        /// <param name="upperedStr">字符串 2，要求已全部大写</param>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe bool IgnoreCaseEqualsByUpper(char* chars, int length, string upperedStr)
        {
            if (length != upperedStr.Length)
            {
                return false;
            }

            while (--length >= 0)
            {
                if (ToUpper(chars[length]) != upperedStr[length])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 忽略大小写比较两个字符串。
        /// </summary>
        /// <param name="chars">字符串 1</param>
        /// <param name="length">字符串 1 长度</param>
        /// <param name="loweredStr">字符串 2，要求已全部小写</param>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe bool IgnoreCaseEqualsByLower(char* chars, int length, string loweredStr)
        {
            if (length != loweredStr.Length)
            {
                return false;
            }

            while (--length >= 0)
            {
                if (ToLower(chars[length]) != loweredStr[length])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取字符串指定索引处字符的大写形式。
        /// </summary>
        /// <param name="st">字符串</param>
        /// <param name="index">索引</param>
        /// <returns>返回一个字符</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static char UpperCharAt(string st, int index)
        {
            return ToUpper(st[index]);
        }

        /// <summary>
        /// 将字符串中的格式项 ({Index})替换为数组中相应的字符串。可以用 '\' 字符让字符串直接复制下一个字符。
        /// </summary> 
        /// <param name="text">字符串</param>
        /// <param name="args">数组</param>
        /// <returns>返回一个新的字符串。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static string Format(string text, params string[] args)
        {
            if (text == null)
            {
                return null;
            }

            fixed (char* pText = text)
            {
                int resultLength;
                int begin = 0;
                int end = resultLength = text.Length;

                GetLengthLoop:
                int index = IndexOf(pText, begin, end, '\\', '{');

                if (index == -1)
                {
                    if (begin == 0)
                    {
                        return text;
                    }

                    goto Format;
                }

                if (pText[index] == '\\')
                {
                    --resultLength;

                    begin = index + 2;
                }
                else
                {
                    ++index;

                    begin = index;

                    if (index < end && pText[index] >= '0' && pText[index] <= '9')
                    {
                        int number = pText[index] - '0';

                        for (++index; index < end; ++index)
                        {
                            if (pText[index] >= '0' && pText[index] <= '9')
                            {
                                number = number * 10 + pText[index] - '0';
                            }
                            else if (pText[index] == '}')
                            {
                                if (args[number] != null)
                                {
                                    resultLength += args[number].Length;

                                    resultLength -= index - begin + 2;
                                }

                                begin = index + 1;

                                break;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                goto GetLengthLoop;

            Format:
                begin = 0;

                var result = MakeString(resultLength);

                int resultIndex = 0;

                fixed (char* pResult = result)
                {
                    FormatLoop:
                    index = IndexOf(pText, begin, end, '\\', '{');

                    if (index == -1)
                    {
                        index = end;
                    }

                    for (; begin < index; ++begin)
                    {
                        pResult[resultIndex] = pText[begin];

                        ++resultIndex;
                    }

                    if (resultIndex == resultLength)
                    {
                        return result;
                    }

                    if (pText[index] == '\\')
                    {
                        ++index;

                        if (index == end)
                        {
                            return result;
                        }

                        pResult[resultIndex] = pText[index];

                        ++resultIndex;

                        begin = index + 1;
                    }
                    else
                    {
                        ++index;

                        if (index < end && pText[index] >= '0' && pText[index] <= '9')
                        {
                            int number = pText[index] - '0';

                            for (++index; index < end; ++index)
                            {
                                if (pText[index] >= '0' && pText[index] <= '9')
                                {
                                    number = number * 10 + pText[index] - '0';
                                }
                                else if (pText[index] == '}')
                                {
                                    if (args[number] != null)
                                    {
                                        int argsLength = args[number].Length;

                                        fixed (char* pArg = args[number])
                                        {
                                            for (int argsIndex = 0; argsIndex < argsLength; argsIndex++)
                                            {
                                                pResult[resultIndex] = pArg[argsIndex];

                                                ++resultIndex;
                                            }
                                        }
                                    }

                                    begin = index + 1;

                                    goto FormatLoop;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }

                        for (; begin < index; ++begin)
                        {
                            pResult[resultIndex] = pText[begin];

                            ++resultIndex;
                        }
                    }

                    goto FormatLoop;
                }
            }
        }

        /// <summary>
        /// 将字符串中的格式项 ({Index})替换为对象中相应的属性。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="text">字符串</param>
        /// <param name="args">对象</param>
        /// <returns>返回一个新的字符串。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static StringBuilder Format<T>(string text, T args)
        {
            if (text == null)
            {
                return null;
            }

            var reader = RWHelper.CreateReader(args).As<string>();

            var builder = new StringBuilder();

            fixed(char* pChars = text)
            {
                var length = text.Length;

                var startIndex = 0;

                for (int i = 0; i < length; i++)
                {
                    if (pChars[i] == '\\' && i + 1 < length)
                    {
                        builder.Append(text, startIndex, i - startIndex);

                        builder.Append(pChars[i + 1]);

                        ++i;

                        startIndex = i + 1;

                        continue;
                    }

                    if (pChars[i] == '{')
                    {
                        if (length - i >= 3)
                        {
                            builder.Append(text, startIndex, i - startIndex);
                            
                            startIndex = i;

                            for (++i; i < length; i++)
                            {
                                if (pChars[i] == '}')
                                {
                                    var name = new string(pChars, startIndex + 1, i - startIndex - 1);

                                    var value = reader[name].DirectRead();

                                    if (value is StringBuilder sb)
                                    {
                                        var sbLength = sb.Length;

                                        for (int s = 0; s < sbLength; s++)
                                        {
                                            builder.Append(sb[s]);
                                        }
                                    }
                                    else if(value != null)
                                    {
                                        builder.Append(value.ToString());
                                    }

                                    startIndex = i + 1;

                                    break;
                                }
                            }
                        }
                    }
                }
                
                builder.Append(text, startIndex, length - startIndex);
            }

            return builder;
        }

        /// <summary>
        /// 比较两个字符串是否相同。如果字符串 1 比字符串 2 长，但两个字符串前面的内容相同也返回 true。如果字符串 1 比字符串 2 短则直接返回 false。
        /// </summary>
        /// <param name="chars">字符串 1</param>
        /// <param name="length">字符串 1 长度</param>
        /// <param name="str">字符串 2 </param>
        /// <returns>返回一个 bool 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static bool StartWith(char* chars, int length, string str)
        {
            if (length < str.Length)
            {
                return false;
            }

            for (length = str.Length - 1; length >= 0; --length)
            {
                if (chars[length] != str[length])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 比较两个字符串是否相同，忽略英文字符大小写。如果字符串 1 比字符串 2 长，但两个字符串前面的内容相同也返回 true。如果字符串 1 比字符串 2 短则直接返回 false。
        /// </summary>
        /// <param name="chars">字符串 1</param>
        /// <param name="length">字符串 1 长度</param>
        /// <param name="upperStr">字符串 2 </param>
        /// <returns>返回一个 bool 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static bool StartWithByUpper(char* chars, int length, string upperStr)
        {
            if (length < upperStr.Length)
            {
                return false;
            }

            for (length = upperStr.Length - 1; length >= 0; --length)
            {
                if (ToUpper(chars[length]) != upperStr[length])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 比较两个字符串是否相同，忽略英文字符大小写。如果字符串 1 比字符串 2 长，但两个字符串前面的内容相同也返回 true。如果字符串 1 比字符串 2 短则直接返回 false。
        /// </summary>
        /// <param name="chars">字符串 1</param>
        /// <param name="length">字符串 1 长度</param>
        /// <param name="lowerstr">字符串 2</param>
        /// <returns>返回一个 bool 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static bool StartWithByLower(char* chars, int length, string lowerstr)
        {
            if (length < lowerstr.Length)
            {
                return false;
            }

            for (int i = 0; i < lowerstr.Length; i++)
            {
                if (ToLower(chars[i]) != lowerstr[i])
                {
                    return false;
                }
            }

            //for (length = lowerstr.Length - 1; length >= 0; --length)
            //{
            //    if (ToLower(chars[length]) != lowerstr[length])
            //    {
            //        return false;
            //    }
            //}

            return true;
        }
        
        /// <summary>
        /// 将小写英文字符转为大写英文字符。
        /// </summary>
        /// <param name="c"></param>
        /// <returns>返回一个字符。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static char ToUpper(char c)
        {
            if (c >= 'a' && c <= 'z')
            {
                return (char)(c & (~0x20));
            }

            return c;
        }

        /// <summary>
        /// 将大写英文字符转为小写英文字符。
        /// </summary>
        /// <param name="c"></param>
        /// <returns>返回一个字符。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static char ToLower(char c)
        {
            if (c >= 'A' && c <= 'Z')
            {
                return (char)(c | 0x20);
            }

            return c;
        }

        /// <summary>
        /// 将字符串中的小写字符转换为大写字符。
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>返回一个新的字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe string ToUpper(string str)
        {
            var ret = MakeString(str.Length);

            fixed(char* pRet = ret)
            {
                for (int i = 0; i < ret.Length; ++i)
                {
                    pRet[i] = ToUpper(str[i]);
                }
            }

            return ret;
        }

        /// <summary>
        /// 将字符串中的大写字符转换为小写字符。
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>返回一个新的字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe string ToLower(string str)
        {
            var ret = MakeString(str.Length);

            fixed (char* pRet = ret)
            {
                for (int i = 0; i < ret.Length; ++i)
                {
                    pRet[i] = ToLower(str[i]);
                }
            }

            return ret;
        }

        /// <summary>
        /// 快速分配指定长度的字符串。
        /// </summary>
        /// <param name="length">指定长度</param>
        /// <returns>返回一个字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string MakeString(int length)
        {
            return new string('\0', length);
        }

        /// <summary>
        /// 去除字符串两端的的空白字符
        /// </summary>
        /// <param name="pText">字符串</param>
        /// <param name="begin">开始索引</param>
        /// <param name="end">结束索引，裁剪的位置不包含此值。</param>
        /// <returns>返回一个新的字符串。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static string Trim(char* pText, int begin, int end)
        {
            while (begin < end && IsWhiteSpace(pText[begin]))
            {
                ++begin;
            }

            do
            {
                --end;
            } while (end >= begin && IsWhiteSpace(pText[end]));

            if (end >= begin)
            {
                return new string(pText, begin, end - begin + 1);
            }

            return "";
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
        /// 在字符串中找到指定字符的索引，没找到则返回 -1
        /// </summary>
        /// <param name="pText">字符串</param>
        /// <param name="c">字符</param>
        /// <param name="begin">开始查找的位置。</param>
        /// <param name="end">结束查找的位置，不包含此值。</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int IndexOf(char* pText, int begin, int end, char c)
        {
            while (begin < end)
            {
                if (pText[begin] == c)
                {
                    return begin;
                }

                ++begin;
            }

            return -1;
        }

        /// <summary>
        /// 在字符串中找到第一个字符 1 或字符 2 的索引，两个字符都没找到则返回 -1
        /// </summary>
        /// <param name="pText">字符串</param>
        /// <param name="char1">字符 1</param>
        /// <param name="char2">字符 2</param>
        /// <param name="begin">开始查找的位置。</param>
        /// <param name="end">结束查找的位置，不包含此值。</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int IndexOf(char* pText, int begin, int end, char char1, char char2)
        {
            while (begin < end)
            {
                if (pText[begin] == char1 || pText[begin] == char2)
                {
                    return begin;
                }

                ++begin;
            }

            return -1;
        }

        /// <summary>
        /// 在字符串 1 中找到字符串 2 的索引，没找到则返回 -1
        /// </summary>
        /// <param name="pText">字符串 1</param>
        /// <param name="chars">字符串 2</param>
        /// <param name="begin">开始查找的位置</param>
        /// <param name="end">结束查找的位置，不包含此值。</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int IndexOf(char* pText, int begin, int end, string chars)
        {
            if (chars.Length == 0)
            {
                return -1;
            }

            Loop:
            while (begin < end)
            {
                if (pText[begin] == chars[0])
                {
                    for (int i = 1, j = begin + 1; i < chars.Length && j < end; ++i, ++j)
                    {
                        if (chars[i] != pText[j])
                        {
                            goto Loop;
                        }
                    }

                    return begin;
                }

                ++begin;
            }

            return -1;
        }

        /// <summary>
        /// 在字符串中找到字符集合中第一个出现的索引，没找到则返回 -1
        /// </summary>
        /// <param name="pText">字符串</param>
        /// <param name="chars">字符集合</param>
        /// <param name="begin">开始查找的位置。</param>
        /// <param name="end">结束查找的位置，不包含此值。</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int IndexOf(char* pText, int begin, int end, char[] chars)
        {
            while (begin < end)
            {
                for (int i = 0; i < chars.Length; ++i)
                {
                    if (pText[begin] == chars[i])
                    {
                        return begin;
                    }
                }

                ++begin;
            }

            return -1;
        }

        /// <summary>
        /// 在字符串中找到指定字符的索引，没找到则返回 -1
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="c">字符</param>
        /// <param name="length">字符串长度</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int IndexOf(char* chars, int length, char c)
        {
            for (int i = 0; i < length; i++)
            {
                if (chars[i] == c)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 在字符串中找到第一个字符 1 或字符 2 的索引，两个字符都没找到则返回 -1
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="item1">字符 1</param>
        /// <param name="item2">字符 2</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int IndexOf(char* chars, int length, char item1, char item2)
        {
            for (int i = 0; i < length; i++)
            {
                if (chars[i] == item1 || chars[i] == item2)
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 在字符串 1 中找到字符串 2 的索引，没找到则返回 -1
        /// </summary>
        /// <param name="chars">字符串 1</param>
        /// <param name="length">字符串 1 长度</param>
        /// <param name="str">字符串 2</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int IndexOf(char* chars, int length, string str)
        {
            var firstChar = str[0];

        Loop:

            for (int i = 0; i < length; i++)
            {
                if (chars[i] == firstChar)
                {
                    var current = chars + i;

                    for (int j = str.Length - 1; j > 0; --j)
                    {
                        if (current[j] != str[j])
                        {
                            goto Loop;
                        }
                    }

                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// 在字符串中找到字符集合中第一个出现的索引，没找到则返回 -1
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="items">字符集合</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int IndexOf(char* chars, int length, char[] items)
        {
            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < items.Length; j++)
                {
                    if (chars[i] == items[j])
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// 获取偏移量，以字节为单位，可以将给定字符串中的数据。
        /// </summary>
        public static readonly int OffsetToStringData = RuntimeHelpers.OffsetToStringData;

        /// <summary>
        /// 获取字符串的元数据引用。
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns>返回第一个字符的引用</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref char GetRawStringData(string str) => ref Unsafe.AddByteOffset(ref Unsafe.AsRef<char>(str), OffsetToStringData);
    }
}