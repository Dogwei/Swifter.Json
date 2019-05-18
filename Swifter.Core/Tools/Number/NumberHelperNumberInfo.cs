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
                numberInfo.integerBegin += add;
                numberInfo.fractionalBegin += add;
                numberInfo.exponentBegin += add;
                numberInfo.end += add;
            }


            return numberInfo;
        }

        /// <summary>
        /// 创建一个 NumberInfo。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="defaultRadix">默认进制数</param>
        /// <param name="gcHandle">返回 Pinned 的字符串 GCHandle</param>
        /// <returns>返回一个 NumberInfo</returns>
        public static unsafe NumberInfo GetNumberInfo(string chars, byte defaultRadix, out GCHandle gcHandle)
        {
            gcHandle = GCHandle.Alloc(chars, GCHandleType.Pinned);
            
            fixed (char* pChars = chars)
            {
                var numberInfo = GetNumberInfo(pChars, chars.Length, defaultRadix);

                return numberInfo;
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

                var beforeZeroCount = 0;

                while (index < length && chars[index] == DigitalsZeroValue)
                {
                    ++beforeZeroCount;
                    ++index;
                }

                var integerBegin = index;
                var integerCount = 0;

                while (index < length && ToRadix(chars[index]) < radix)
                {
                    ++integerCount;
                    ++index;
                }

                if (integerCount != 0)
                {
                    number.integerBegin = integerBegin;
                    number.integerCount = integerCount;
                }
                else if (beforeZeroCount != 0)
                {
                    number.integerBegin = index - 1;
                    number.integerCount = 1;
                }

                if (index < length && chars[index] == DotSign)
                {
                    number.isFloat = true;

                    ++index;

                    var fractionalBegin = index;
                    var fractionalCount = 0;

                    while (index < length && ToRadix(chars[index]) < radix)
                    {
                        ++fractionalCount;
                        ++index;
                    }

                    if (fractionalCount != 0)
                    {

                        for (int fractionalRight = fractionalBegin + fractionalCount - 1; fractionalRight >= 0; --fractionalRight)
                        {
                            if (chars[fractionalRight] != DigitalsZeroValue)
                            {
                                break;
                            }

                            --fractionalCount;
                        }

                        number.haveFractional = fractionalCount != 0;
                        number.fractionalBegin = fractionalBegin;
                        number.fractionalCount = fractionalCount;
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

                        var exponentBeforeZeroCount = 0;

                        while (index < length && chars[index] == DigitalsZeroValue)
                        {
                            ++exponentBeforeZeroCount;
                            ++index;
                        }

                        var exponentCount = 0;

                        while (index < length && ToRadix(chars[index]) < radix)
                        {
                            ++exponentCount;
                            ++index;
                        }

                        if (exponentCount != 0)
                        {
                            number.exponentBegin = exponentBegin;
                            number.exponentCount = exponentCount;
                        }
                        else if (beforeZeroCount != 0)
                        {
                            number.exponentBegin = index - 1;
                            number.exponentCount = 1;
                        }
                    }
                }

                number.end = index;
            }

            return number;
        }

        /// <summary>
        /// 创建一个 NumberInfo。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="gcHandle">返回 Pinned 的字符串 GCHandle</param>
        /// <returns>返回一个 NumberInfo</returns>
        public NumberInfo GetNumberInfo(string chars, out GCHandle gcHandle)
        {
            gcHandle = GCHandle.Alloc(chars, GCHandleType.Pinned);

            fixed (char* pChars = chars)
            {
                var numberInfo = GetNumberInfo(pChars, chars.Length);

                return numberInfo;
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

            byte l = 0;
            var c = count;
            var r = 0UL;

            for (int i = 0, j = numberInfo.integerBegin; i < numberInfo.integerCount; ++i, ++j, --c)
            {
                l = ToRadix(numberInfo.chars[j]);

                if (c <= 1)
                {
                    goto End;
                }

                r = r * radix + l;
            }

            for (int i = 0, j = numberInfo.fractionalBegin; i < numberInfo.fractionalCount; ++i, ++j, --c)
            {
                l = ToRadix(numberInfo.chars[j]);

                if (c <= 1)
                {
                    goto End;
                }

                r = r * radix + l;
            }

            l = 0;

            for (; c > 1; --c)
            {
                r = r * radix;
            }

        End:

            if (count == uInt64NumbersLength && r > (UInt64MaxValue - l) / radix)
            {
                throw new OverflowException("Number out of UInt64 range.");
            }

            r = r * radix + l;

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

            for (int i = 0, j = numberInfo.integerBegin; i < numberInfo.integerCount; ++i, ++j)
            {
                r = r * radix + ToRadix(numberInfo.chars[j]);
            }

            for (int i = 0, j = numberInfo.fractionalBegin; i < numberInfo.fractionalCount; ++i, ++j)
            {
                r = r * radix + ToRadix(numberInfo.chars[j]);
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
