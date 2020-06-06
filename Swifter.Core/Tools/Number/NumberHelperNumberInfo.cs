using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    public sealed unsafe partial class NumberHelper
    {
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static byte ToDigit(char c)
        {
            if (c < DigitalsMaxValue)
            {
                return IgnoreCaseRadixes[c];
            }

            return ErrorRadix;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static void GetNumberCount(char* chars, int length, byte max_radix, ref int index, ref int count, ref int splitCount, ref byte max_digit)
        {
            Loop:

            if (index < length)
            {
                switch (ToDigit(chars[index]))
                {
                    case SplitRadix:
                        ++splitCount;
                        ++index;
                        goto Loop;
                    case var digit when digit < max_radix:
                        ++index;

                        if (digit == ExponentRadix && index < length && IsSign(chars[index]))
                        {
                            --index;
                            break;
                        }

                        if (digit > max_digit) max_digit = digit;
                        ++count;
                        goto Loop;
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static bool IsSign(char c)
        {
            return c switch
            {
                NegativeSign => true,
                PositiveSign => true,
                _ => false,
            };
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static void SubBeginingZeroOrSplitAndEndingSplit(char* chars, ref int begin, ref int numberCount, ref int splitCount)
        {
            while (numberCount > 1)
            {
                switch (ToDigit(chars[begin]))
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

            while (ToDigit(chars[begin]) == SplitRadix)
            {
                ++begin;
                --splitCount;
            }

            var end = begin + numberCount + splitCount - 1;

            while (ToDigit(chars[end]) == SplitRadix)
            {
                --end;
                --splitCount;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static void SubBeginingSplitAndEndingZeroOrSplit(char* chars, ref int begin, ref int numberCount, ref int splitCount)
        {
            var end = begin + numberCount + splitCount - 1;

            while (numberCount > 0)
            {
                switch (ToDigit(chars[end]))
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
                while (ToDigit(chars[end]) == SplitRadix)
                {
                    --end;
                    --splitCount;
                }

                while (ToDigit(chars[begin]) == SplitRadix)
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
        /// <param name="max_radix">最大进制数</param>
        /// <returns>返回一个 NumberInfo</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static NumberInfo GetNumberInfo(char* chars, int length, byte max_radix = DecimalFirstRadix)
        {
            var number = new NumberInfo
            {
                integerBegin = -1,
                fractionalBegin = -1,
                exponentBegin = -1,

                chars = chars
            };

            if (length > 0)
            {
                var index = 0;

                switch (chars[index])
                {
                    case NegativeSign:
                        number.isNegative = true;
                        ++index;
                        break;
                    case PositiveSign:
                        number.isNegative = false;
                        ++index;
                        break;
                }

                if (max_radix >= MaxFirstRadix)
                {
                    max_radix = (byte)(-(sbyte)max_radix);

                    if (length >= 3 && chars[index] == DigitalsZeroValue)
                    {
                        switch (chars[index + 1])
                        {
                            case hexSign:
                            case HexSign:
                                index += 2;
                                number.max_digit = HexRadix - 1;
                                max_radix = HexRadix;
                                break;
                            case binarySign:
                            case BinarySign:
                                index += 2;
                                number.max_digit = BinaryRadix - 1;
                                max_radix = BinaryRadix;
                                break;
                        }
                    }
                }

                number.max_radix = max_radix;

                var integerBegin = index;
                var integerCount = 0;
                var integerSplitCount = 0;

                GetNumberCount(chars, length, max_radix, ref index, ref integerCount, ref integerSplitCount, ref number.max_digit);

                if (integerCount != 0)
                {
                    SubBeginingZeroOrSplitAndEndingSplit(chars, ref integerBegin, ref integerCount, ref integerSplitCount);

                    number.integerBegin = integerBegin;
                    number.integerCount = integerCount;
                    number.integerSplitCount = integerSplitCount;
                }

                if (index < length && chars[index] == DotSign)
                {
                    ++index;

                    var fractionalBegin = index;
                    var fractionalCount = 0;
                    var fractionalSplitCount = 0;

                    GetNumberCount(chars, length, max_radix, ref index, ref fractionalCount, ref fractionalSplitCount, ref number.max_digit);

                    if (fractionalCount != 0)
                    {
                        SubBeginingSplitAndEndingZeroOrSplit(chars, ref fractionalBegin, ref fractionalCount, ref fractionalSplitCount);

                        number.fractionalBegin = fractionalBegin;
                        number.fractionalCount = fractionalCount;
                        number.fractionalSplitCount = fractionalSplitCount;
                    }
                }

                if (index < length)
                {
                    var haveExponent = false;

                    switch (chars[index])
                    {
                        case exponentSign:
                        case ExponentSign:

                            ++index;

                            haveExponent = true;

                            break;
                    }

                    if (haveExponent && index < length)
                    {
                        switch (chars[index])
                        {
                            case NegativeSign:
                                number.exponentIsNegative = true;
                                ++index;
                                break;
                            case PositiveSign:
                                number.exponentIsNegative = false;
                                ++index;
                                break;
                        }

                        var exponentBegin = index;
                        var exponentCount = 0;
                        var exponentSplitCount = 0;

                        GetNumberCount(chars, length, max_radix, ref index, ref exponentCount, ref exponentSplitCount, ref number.max_digit);

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

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private ulong ForcedParseUInt64(char* chars, int count)
        {
            var value = 0ul;

            Loop:

            var digit = ToRadix(*chars);

            ++chars;

            if (digit < radix)
            {
                --count;

                if (count == 0)
                {
                    return checked(value * radix + digit);
                }

                value = value * radix + digit;
            }

            goto Loop;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private double ForcedParseDouble(char* chars, int count)
        {
            var max_segment_length = uInt64NumbersLength - 1;

            var value = 0d;

            var segment = 0ul;
            var segment_length = 0;

            Loop:

            var digit = ToRadix(*chars);

            ++chars;

            if (digit < radix)
            {
                --count;
                ++segment_length;

                segment = segment * radix + digit;

                if (count == 0)
                {
                    return value * uInt64Numbers[segment_length] + segment;
                }
                
                if (segment_length >= max_segment_length)
                {
                    value = value * uInt64Numbers[segment_length] + segment;

                    segment_length = 0;
                    segment = 0;
                }
            }

            goto Loop;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static ulong DecimalForcedParseUInt64(char* chars, int count)
        {
            var value = 0ul;

            Loop:

            var digit = (uint)(*chars - DigitalsZeroValue);

            ++chars;

            if (digit < DecimalRadix)
            {
                --count;

                if (count == 0)
                {
                    return checked(value * DecimalRadix + digit);
                }

                value = value * DecimalRadix + digit;
            }

            goto Loop;
        }

        /// <summary>
        /// 将 NumberInfo 转换为 UInt64。失败将引发异常。
        /// </summary>
        /// <param name="numberInfo">NumberInfo</param>
        /// <returns>返回一个 UInt64</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal ulong ToUInt64(NumberInfo numberInfo)
        {
            const int MaxExponent = 6;

            if (numberInfo.max_digit >= radix || numberInfo.max_radix < radix)
            {
                throw new FormatException("radix");
            }

            var integerCount = numberInfo.integerCount;

            if (numberInfo.HaveExponent)
            {
                if (numberInfo.ExponentCount >= MaxExponent)
                {
                    goto Overflow;
                }

                var exponent = (int)ForcedParseUInt64(numberInfo.chars + numberInfo.exponentBegin, numberInfo.exponentCount);

                if (numberInfo.exponentIsNegative)
                {
                    exponent = -exponent;
                }

                integerCount += exponent;
            }

            if (integerCount <= uInt64NumbersLength)
            {
                /* 十进制优化 */
                if (radix == DecimalRadix && 
                    integerCount <= numberInfo.integerCount && 
                    integerCount < DecimalUInt64NumbersLength)
                {
                    return DecimalForcedParseUInt64(numberInfo.chars + numberInfo.integerBegin, integerCount);
                }

                var numberCount = Math.Min(integerCount, numberInfo.integerCount + numberInfo.fractionalCount);

                var value = ForcedParseUInt64(numberInfo.chars + numberInfo.integerBegin, numberCount);

                integerCount -= numberCount;

                while (integerCount > 4)
                {
                    value = checked(value * uInt64Numbers[4]);

                    integerCount -= 4;
                }

                while (integerCount > 0)
                {
                    value = checked(value * radix);

                    --integerCount;
                }

                return value;
            }

            Overflow:

            throw new OverflowException();
        }

        /// <summary>
        /// 将 NumberInfo 转换为 Int64。失败将引发异常。
        /// </summary>
        /// <param name="numberInfo">NumberInfo</param>
        /// <returns>返回一个 Int64</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal long ToInt64(NumberInfo numberInfo)
        {
            const int MaxExponent = 6;

            if (numberInfo.max_digit >= radix || numberInfo.max_radix < radix)
            {
                throw new FormatException("radix");
            }

            var integerCount = numberInfo.integerCount;

            if (numberInfo.HaveExponent)
            {
                if (numberInfo.ExponentCount >= MaxExponent)
                {
                    goto Overflow;
                }

                var exponent = (int)ForcedParseUInt64(numberInfo.chars + numberInfo.exponentBegin, numberInfo.exponentCount);

                if (numberInfo.exponentIsNegative)
                {
                    exponent = -exponent;
                }

                integerCount += exponent;
            }

            if (integerCount <= uInt64NumbersLength)
            {
                ulong value;

                /* 十进制优化 */
                if (radix == DecimalRadix && 
                    integerCount <= numberInfo.integerCount && 
                    integerCount < (DecimalUInt64NumbersLength - 1))
                {
                    value = DecimalForcedParseUInt64(numberInfo.chars + numberInfo.integerBegin, integerCount);
                }
                else
                {
                    var numberCount = Math.Min(integerCount, numberInfo.integerCount + numberInfo.fractionalCount);

                    value = ForcedParseUInt64(numberInfo.chars + numberInfo.integerBegin, numberCount);

                    integerCount -= numberCount;

                    while (integerCount > 4)
                    {
                        value = checked(value * uInt64Numbers[4]);

                        integerCount -= 4;
                    }

                    while (integerCount > 0)
                    {
                        value = checked(value * radix);

                        --integerCount;
                    }
                }

                if (numberInfo.isNegative)
                {
                    if (value > unchecked((ulong)-long.MinValue))
                    {
                        return checked(-(long)value);
                    }

                    return -(long)value;
                }
                else
                {
                    return checked((long)value);
                }
            }

            Overflow:

            throw new OverflowException();
        }

        /// <summary>
        /// 将 NumberInfo 转换为 Double。失败将引发异常。
        /// </summary>
        /// <param name="numberInfo">NumberInfo</param>
        /// <returns>返回一个 Double</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal double ToDouble(NumberInfo numberInfo)
        {
            const int MaxExponent = 6;

            if (numberInfo.max_digit >= radix || numberInfo.max_radix < radix)
            {
                throw new FormatException("radix");
            }

            var value = ForcedParseDouble(numberInfo.chars + numberInfo.integerBegin, numberInfo.integerCount + numberInfo.fractionalCount);

            var fractionalCount = numberInfo.fractionalCount;

            if (numberInfo.HaveExponent)
            {
                if (numberInfo.ExponentCount >= MaxExponent)
                {
                    goto Overflow;
                }

                var exponent = (int)ForcedParseUInt64(numberInfo.chars + numberInfo.exponentBegin, numberInfo.exponentCount);

                if (numberInfo.exponentIsNegative)
                {
                    exponent = -exponent;
                }

                fractionalCount -= exponent;
            }

            value = Pow(value, -fractionalCount);

            if (numberInfo.isNegative)
            {
                value = -value;
            }

            if (value >= double.MinValue && value <= double.MaxValue)
            {
                return value;
            }

            Overflow:

            throw new OverflowException();
        }

        /// <summary>
        /// 将一个 NumberInfo 转换为 Decimal。转换失败则引发异常。
        /// </summary>
        /// <param name="numberInfo">NumberInfo</param>
        /// <returns>返回一个 Decimal</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static decimal ToDecimal(NumberInfo numberInfo)
        {
            const int MaxExponent = 5;

            if (numberInfo.max_digit >= DecimalRadix || numberInfo.max_radix < DecimalRadix)
            {
                throw new FormatException("decimal radix.");
            }

            if (numberInfo.integerCount + numberInfo.fractionalCount < DecimalStringMaxLength)
            {
                var value = default(decimal);
                var length = 0;

                ToDecimalNumber(numberInfo.chars + numberInfo.integerBegin, numberInfo.integerCount, (uint*)&value, ref length);

                ToDecimalNumber(numberInfo.chars + numberInfo.fractionalBegin, numberInfo.fractionalCount, (uint*)&value, ref length);

                if (length > 3)
                {
                    goto Overflow;
                }

                var scale = numberInfo.fractionalCount;

                if (numberInfo.HaveExponent)
                {
                    if (numberInfo.ExponentCount >= MaxExponent)
                    {
                        goto Overflow;
                    }

                    var exponent = (int)DecimalForcedParseUInt64(numberInfo.chars + numberInfo.exponentBegin, numberInfo.exponentCount);

                    if (numberInfo.exponentIsNegative)
                    {
                        exponent = -exponent;
                    }

                    scale -= exponent;
                }

                if (scale < 0)
                {
                    scale = -scale;

                    if (scale > DecimalMaxScale - (numberInfo.integerCount + numberInfo.fractionalCount))
                    {
                        goto Overflow;
                    }

                    while (scale > 0)
                    {
                        uint carry;

                        if (scale >= DecimalDivisorLength)
                        {
                            Mult((uint*)&value, length, DecimalDivisor, out carry);
                        }
                        else
                        {
                            Mult((uint*)&value, length, (uint)DecimalUInt64Numbers[scale], out carry);
                        }

                        if (carry != 0)
                        {
                            ((uint*)&value)[length] = carry;

                            ++length;

                            if (length > 3)
                            {
                                goto Overflow;
                            }
                        }

                        scale -= 9;
                    }
                }

                ((DecimalStruct*)&value)->SetBits((int*)&value);

                if (scale > 0)
                {
                    ((DecimalStruct*)&value)->Scale = scale;
                }

                if (numberInfo.isNegative)
                {
                    ((DecimalStruct*)&value)->Sign = 1;
                }

                return value;
            }

            Overflow:

            throw new OverflowException();
        }


        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static void ToDecimalNumber(char* chars, int count, uint* number, ref int length)
        {
            while(count > 0)
            {
                uint segment = 0;
                byte segment_length = 0;

                while (segment_length < DecimalDivisorLength && count > 0)
                {
                    var digit = (uint)(*chars - DigitalsZeroValue); 
                    
                    ++chars;

                    if (digit < DecimalRadix)
                    {
                        segment = segment * DecimalRadix + digit;

                        ++segment_length;
                        --count;
                    }
                }

                uint carry;

                if (segment_length == DecimalDivisorLength)
                {
                    Mult(number, length, DecimalDivisor, out carry);
                }
                else
                {
                    Mult(number, length, (uint)DecimalUInt64Numbers[segment_length], out carry);
                }

                if (carry != 0)
                {
                    number[length] = carry;

                    ++length;
                }

                Add(number, length, segment, out carry);

                if (carry != 0)
                {
                    number[length] = carry;

                    ++length;
                }
            }
        }
    }
}