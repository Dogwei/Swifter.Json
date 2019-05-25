using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    public sealed unsafe partial class NumberHelper
    {
        /// <summary>
        /// 创建一个 NumberInfo。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="defaultRadix">默认进制数</param>
        /// <returns>返回一个 NumberInfo</returns>
        public static NumberInfo GetNumberInfo(char* chars, int length, byte defaultRadix)
        {
            var c = chars;
            var isNegative = false;

            if (length > 0)
            {
                var signChar = c[0];

                switch (signChar)
                {
                    case NegativeSign:
                    case PositiveSign:

                        ++c;
                        --length;

                        isNegative = signChar == NegativeSign;
                        break;
                }

                if (length >= 3)
                {
                    var zeroChar = c[0];

                    if (zeroChar == DigitalsZeroValue)
                    {
                        var radixChar = c[1];

                        switch (radixChar)
                        {
                            case hexSign:
                            case HexSign:
                                defaultRadix = 16;
                                break;
                            case binarySign:
                            case BinarySign:
                                defaultRadix = 2;
                                break;
                            default:
                                goto GetNumberInfo;
                        }

                        c += 2;
                        length -= 2;
                    }
                }
            }

        GetNumberInfo:

            var numberInfo = InstanceByRadix(defaultRadix).GetNumberInfo(c, length);

            numberInfo.isNegative = isNegative;

            if (c != chars)
            {
                var add = (int)(c - chars);

                numberInfo.chars = chars;

                if (numberInfo.integerBegin != -1)
                {
                    numberInfo.integerBegin += add;
                }
                if (numberInfo.fractionalBegin != -1)
                {
                    numberInfo.fractionalBegin += add;
                }
                if (numberInfo.exponentBegin != -1)
                {
                    numberInfo.exponentBegin += add;
                }
                if (numberInfo.end != -1)
                {
                    numberInfo.end += add;
                }
            }

            return numberInfo;
        }

        /// <summary>
        /// 创建一个 NumberInfo，注意：实用此方法请保证字符串内容不会被移址。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="defaultRadix">默认进制数</param>
        /// <returns>返回一个 NumberInfo</returns>
        public static unsafe NumberInfo GetNumberInfo(string chars, byte defaultRadix)
        {
            fixed (char* pChars = chars)
            {
                return GetNumberInfo(pChars, chars.Length, defaultRadix);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void GetNumberCount(char* chars, int length,ref int index, ref int count, ref int splitCount)
        {
        Loop:

            while (index < length && ToRadix(chars[index]) < radix)
            {
                ++count;
                ++index;
            }

            if (index < length && ToRadix(chars[index]) == SplitRadix)
            {
                ++splitCount;
                ++index;

                goto Loop;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void SubBeginingZeroOrSplitAndEndingSplit(char* chars, ref int begin, ref int numberCount, ref int splitCount)
        {
            while (numberCount > 1)
            {
                switch (ToRadix(chars[begin]))
                {
                    case 0:
                        ++begin;
                        --numberCount;
                        continue;
                    case SplitRadix:
                        ++begin;
                        --splitCount;
                        continue;
                }

                break;
            }

            while (ToRadix(chars[begin]) == SplitRadix)
            {
                ++begin;
                --splitCount;
            }

            var end = begin + numberCount + splitCount - 1;

            while (ToRadix(chars[end]) == SplitRadix)
            {
                --end;
                --splitCount;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void SubBeginingSplitAndEndingZeroOrSplit(char* chars, ref int begin, ref int numberCount, ref int splitCount)
        {
            var end = begin + numberCount + splitCount - 1;

            while (numberCount > 0)
            {
                switch (ToRadix(chars[end]))
                {
                    case 0:
                        --end;
                        --numberCount;
                        continue;
                    case SplitRadix:
                        --end;
                        --splitCount;
                        continue;
                }

                break;
            }

            if (numberCount > 1)
            {
                while (ToRadix(chars[end]) == SplitRadix)
                {
                    --end;
                    --splitCount;
                }

                while (ToRadix(chars[begin]) == SplitRadix)
                {
                    ++begin;
                    --splitCount;
                }
            }
        }

        /// <summary>
        /// 创建一个 NumberInfo。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <returns>返回一个 NumberInfo</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public NumberInfo GetNumberInfo(char* chars, int length)
        {
            var number = new NumberInfo
            {
                integerBegin = -1,
                fractionalBegin = -1,
                exponentBegin = -1,

                chars = chars,
                radix = radix
            };

            if (length > 0)
            {
                var index = 0;

                var signChar = chars[0];

                switch (signChar)
                {
                    case NegativeSign:
                    case PositiveSign:
                        ++index;
                        number.isNegative = signChar == NegativeSign;
                        break;
                }

                var integerBegin = index;
                var integerCount = 0;
                var integerSplitCount = 0;

                GetNumberCount(chars, length, ref index, ref integerCount, ref integerSplitCount);

                if (integerCount != 0)
                {
                    SubBeginingZeroOrSplitAndEndingSplit(chars, ref integerBegin, ref integerCount, ref integerSplitCount);

                    number.integerBegin = integerBegin;
                    number.integerCount = integerCount;
                    number.integerSplitCount = integerSplitCount;
                }

                if (index < length && chars[index] == DotSign)
                {
                    number.isFloat = true;

                    ++index;

                    var fractionalBegin = index;
                    var fractionalCount = 0;
                    var fractionalSplitCount = 0;

                    GetNumberCount(chars, length, ref index, ref fractionalCount, ref fractionalSplitCount);

                    if (fractionalCount != 0)
                    {
                        SubBeginingSplitAndEndingZeroOrSplit(chars, ref integerBegin, ref integerCount, ref integerSplitCount);

                        number.haveFractional = fractionalCount != 0;
                        number.fractionalBegin = fractionalBegin;
                        number.fractionalCount = fractionalCount;
                        number.fractionalSplitCount = fractionalSplitCount;
                    }
                }

                if (index < length)
                {
                    bool haveExponent = false;

                    switch (chars[index])
                    {
                        case exponentSign:
                        case ExponentSign:

                            ++index;

                            haveExponent = true;

                            break;
                        case PositiveSign:
                        case NegativeSign:
                            switch (chars[index - 1])
                            {
                                case exponentSign:
                                case ExponentSign:
                                    if (number.isFloat)
                                    {
                                        if (number.fractionalCount >= 2)
                                        {
                                            --number.fractionalCount;

                                            haveExponent = true;
                                        }
                                    }
                                    else if (number.integerCount >= 2)
                                    {
                                        --number.integerCount;

                                        haveExponent = true;
                                    }
                                    break;
                            }
                            break;
                    }

                    if (haveExponent && index < length)
                    {
                        var exponentSignChar = chars[index];

                        switch (exponentSignChar)
                        {
                            case NegativeSign:
                            case PositiveSign:
                                ++index;
                                number.exponentIsNegative = exponentSignChar == NegativeSign;
                                break;
                        }

                        var exponentBegin = index;
                        var exponentCount = 0;
                        var exponentSplitCount = 0;

                        GetNumberCount(chars, length, ref index, ref exponentCount, ref exponentSplitCount);

                        if (exponentCount != 0)
                        {
                            SubBeginingZeroOrSplitAndEndingSplit(chars, ref exponentBegin, ref exponentCount, ref exponentSplitCount);

                            number.exponentBegin = exponentBegin;
                            number.exponentCount = exponentCount;
                            number.exponentSplitCount = exponentSplitCount;
                        }
                    }
                }

                number.end = index;
            }

            return number;
        }

        /// <summary>
        /// 获取一个字符串的 NumberInfo。注意：实用此方法请保证字符串内容不会被移址。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <returns>返回一个 NumberInfo</returns>
        public NumberInfo GetNumberInfo(string chars)
        {
            fixed (char* pChars = chars)
            {
                return GetNumberInfo(pChars, chars.Length);
            }
        }


        /// <summary>
        /// 将 NumberInfo 转换为 UInt64。失败将引发异常。
        /// </summary>
        /// <param name="numberInfo">NumberInfo</param>
        /// <returns>返回一个 UInt64</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong ToUInt64(NumberInfo numberInfo)
        {
            if (numberInfo.radix != radix)
            {
                throw new FormatException("radix");
            }

            if (numberInfo.exponentCount > 5)
            {
                throw new OverflowException("Exponent too big.");
            }

            var exponent = UncheckedParse(numberInfo.chars + numberInfo.exponentBegin, numberInfo.exponentCount);

            if (numberInfo.exponentIsNegative)
            {
                exponent = -exponent;
            }

            int count = exponent + numberInfo.integerCount;

            if (count > uInt64NumbersLength)
            {
                throw new OverflowException("Number out of UInt64 range.");
            }

            if (count <= 0)
            {
                return 0;
            }

            var c = count;
            var r = 0UL;

            byte digit;

            for (int i = 0, j = numberInfo.integerBegin; i < numberInfo.integerCount; ++j)
            {
                digit = ToRadix(numberInfo.chars[j]);

                if (c <= 1)
                {
                    goto End;
                }

                if (digit < radix)
                {
                    r = r * radix + digit;

                    --c;
                    ++i;
                }
            }

            for (int i = 0, j = numberInfo.fractionalBegin; i < numberInfo.fractionalCount; ++j)
            {
                digit = ToRadix(numberInfo.chars[j]);

                if (c <= 1)
                {
                    goto End;
                }

                if (digit < radix)
                {
                    r = r * radix + digit;

                    --c;
                    ++i;
                }
            }

            digit = 0;

            for (; c > 1; --c)
            {
                r *= radix;
            }

        End:

            if (count == uInt64NumbersLength && r > (UInt64MaxValue - digit) / radix)
            {
                throw new OverflowException("Number out of UInt64 range.");
            }

            r = r * radix + digit;

            return r;
        }

        /// <summary>
        /// 将 NumberInfo 转换为 Int64。失败将引发异常。
        /// </summary>
        /// <param name="numberInfo">NumberInfo</param>
        /// <returns>返回一个 Int64</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ToInt64(NumberInfo numberInfo)
        {
            if (numberInfo.radix != radix)
            {
                throw new FormatException("radix");
            }

            ulong uInt64 = ToUInt64(numberInfo);

            if (numberInfo.isNegative)
            {
                if (uInt64 > PositiveInt64MinValue)
                {
                    throw new OverflowException("Number out of Int64 range.");
                }

                if (uInt64 == PositiveInt64MinValue)
                {
                    return long.MinValue;
                }

                return -(long)uInt64;
            }

            if (uInt64 > Int64MaxValue)
            {
                throw new OverflowException("Number out of Int64 range.");
            }

            return (long)uInt64;
        }

        /// <summary>
        /// 将 NumberInfo 转换为 Double。失败将引发异常。
        /// </summary>
        /// <param name="numberInfo">NumberInfo</param>
        /// <returns>返回一个 Double</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public double ToDouble(NumberInfo numberInfo)
        {
            if (numberInfo.radix != radix)
            {
                throw new FormatException("radix");
            }

            if (numberInfo.exponentCount > 11)
            {
                throw new OverflowException("Exponent too big.");
            }

            var exponent = UncheckedParse(numberInfo.chars + numberInfo.exponentBegin, numberInfo.exponentCount);

            if (numberInfo.exponentIsNegative)
            {
                exponent = -exponent;
            }

            var r = 0D;

            for (int i = 0, j = numberInfo.integerBegin; i < numberInfo.integerCount; ++j)
            {
                var digit = ToRadix(numberInfo.chars[j]);

                if (digit < radix)
                {
                    r = r * radix + digit;

                    ++i;
                }
            }

            for (int i = 0, j = numberInfo.fractionalBegin; i < numberInfo.fractionalCount; ++j)
            {
                var digit = ToRadix(numberInfo.chars[j]);

                if (digit < radix)
                {
                    r = r * radix + digit;

                    ++i;
                }
            }

            var e = exponent - numberInfo.fractionalCount;

            var exponents = positiveExponents;

            if (e < 0)
            {
                exponents = negativeExponents;

                e = -e;
            }

            if (e > 0)
            {
                while (e >= 100)
                {
                    r *= exponents[100];

                    e -= 100;
                }

                while (e >= 10)
                {
                    r *= exponents[10];

                    e -= 10;
                }

                while (e >= 1)
                {
                    r *= exponents[1];

                    --e;
                }
            }

            if (r > DoubleMaxPositive)
            {
                throw new OverflowException("Number out of Double range.");
            }

            if (numberInfo.isNegative)
            {
                return -r;
            }

            return r;
        }
    }
}
