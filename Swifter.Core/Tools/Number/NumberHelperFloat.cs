using System;
using System.Runtime.CompilerServices;


namespace Swifter.Tools
{
    public sealed unsafe partial class NumberHelper
    {
        /// <summary>
        /// 获取整数部分位数。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private int GetPositiveExponent(double value)
        {
            var exponents = positiveExponents;

            var index = 0;

            while (index + 128 < exponents.Length && value >= exponents[index + 128])
            {
                index += 128;
            }

            while (index + 16 < exponents.Length && value >= exponents[index + 16])
            {
                index += 16;
            }

            while (index < exponents.Length && value >= exponents[index])
            {
                ++index;
            }

            return index - 1;
        }

        /// <summary>
        /// 获取数字需要移动多少位才能大于等于 1。
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private int GetNegativeExponent(double value)
        {
            var index = 0;

            var exponents = negativeExponents;

            while (index + 128 < exponents.Length && value < exponents[index + 128])
            {
                index += 128;
            }

            while (index + 16 < exponents.Length && value < exponents[index + 16])
            {
                index += 16;
            }

            while (index < exponents.Length && value < exponents[index])
            {
                ++index;
            }

            return index;
        }

        /// <summary>
        /// 获取 Double 的指数值。
        /// </summary>
        /// <param name="value">Double</param>
        /// <returns>返回指数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int GetExponent(double value)
        {
            if (value >= 1)
            {
                return GetPositiveExponent(value);
            }
            else if (value == 0)
            {
                return 0;
            }

            return -GetNegativeExponent(value);
        }

        /// <summary>
        /// 对浮点数乘以指定次数的进制数。
        /// </summary>
        /// <param name="x">浮点数</param>
        /// <param name="y">指定次数</param>
        /// <returns>返回一个浮点数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public double Pow(double x, int y)
        {
            if (y >= 0 && y < positiveExponents.Length)
            {
                return x * positiveExponents[y];
            }

            if (y < 0 && -y < positiveExponents.Length)
            {
                return x / positiveExponents[-y];
            }

            return Pow(Pow(x, y / 2), y - y / 2);
        }

        static int ToSpecialString(double value, char* chars)
        {
            if (double.IsNaN(value))
            {
                return Copy(chars, NaNSign);
            }

            if (double.IsPositiveInfinity(value))
            {
                return Copy(chars, PositiveInfinitySign);
            }

            if (double.IsNegativeInfinity(value))
            {
                return Copy(chars, NegativeInfinitySign);
            }

            chars[0] = DigitalsZeroValue;

            return 1;
        }

        /// <summary>
        /// 判断一个 <see cref="double"/> 是否为特殊值。特殊值包括 <see cref="double.PositiveInfinity"/>, <see cref="double.NegativeInfinity"/> 和 <see cref="double.NaN"/>。
        /// </summary> 
        /// <param name="value">需要判断的 <see cref="double"/> 值</param>
        /// <returns>返回一个 <see cref="bool"/> 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IsSpecialValue(double value)
        {
            const ulong SpecialValue = 0x7FF0000000000000;

            return (Underlying.As<double, ulong>(ref value) & SpecialValue) == SpecialValue;
        }

        /// <summary>
        /// 判断一个 <see cref="float"/> 是否为特殊值。特殊值包括 <see cref="float.PositiveInfinity"/>, <see cref="float.NegativeInfinity"/> 和 <see cref="float.NaN"/>。
        /// </summary>
        /// <param name="value">需要判断的 <see cref="float"/> 值</param>
        /// <returns>返回一个 <see cref="bool"/> 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IsSpecialValue(float value)
        {
            const uint SpecialValue = 0x7F800000;

            return (Underlying.As<float, uint>(ref value) & SpecialValue) == SpecialValue;
        }

        /// <summary>
        /// 将一个 Double 值写入到空间足够的字符串中。
        /// </summary>
        /// <param name="value">Double 值</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ToString(double value, char* chars)
        {
            if (IsSpecialValue(value))
            {
                return ToSpecialString(value, chars);
            }

            var offset = chars;

            if (value < 0)
            {
                *offset = NegativeSign;

                ++offset;

                value = -value;
            }

            var exponent = GetExponent(value);

            var lengthOfInteger = exponent >= 0 && exponent < maxDoubleDigitalsLength ? exponent + 1 : 1;

            var lengthOfFractional = maxDoubleDigitalsLength - lengthOfInteger;

            if (exponent < maxDoubleDigitalsLength && exponent > -maxFloatingZeroLength)
            {
                exponent = 0;
            }

            var mantissa = (ulong)Pow(value, lengthOfFractional - exponent);

            if (radix == DecimalRadix)
            {
                const int power4 = 10000;
                const int radix = DecimalRadix;

                ulong temp;

                if (lengthOfFractional >= 2)
                {
                    temp = mantissa / 100;

                    var tails = mantissa - temp * 100;

                    if (!(tails > 30 && tails < 70))
                    {
                        if (tails > 30)
                        {
                            ++temp;
                        }

                        mantissa = temp;

                        lengthOfFractional -= 2;
                    }
                }

                // Remove 0 of tails.
                while (lengthOfFractional >= 4 && mantissa == (temp = mantissa / power4) * power4)
                {
                    mantissa = temp;

                    lengthOfFractional -= 4;
                }

                while (lengthOfFractional > 0 && mantissa == (temp = mantissa / radix) * radix)
                {
                    mantissa = temp;

                    --lengthOfFractional;
                }
            }
            else
            {
                var power4 = uInt64Numbers[4];

                ulong temp;

                // Remove 0 of tails.
                while (lengthOfFractional >= 4 && mantissa == (temp = mantissa / power4) * power4)
                {
                    mantissa = temp;

                    lengthOfFractional -= 4;
                }

                while (lengthOfFractional > 0 && mantissa == (temp = mantissa / radix) * radix)
                {
                    mantissa = temp;

                    --lengthOfFractional;
                }
            }

            if (lengthOfFractional == 0)
            {
                var length = ToString(mantissa, offset);

                if (length > lengthOfInteger && exponent != 0)
                {
                    ++exponent;
                    --length;
                }

                offset += length;
            }
            else
            {
                ToString(mantissa, (byte)(lengthOfInteger + lengthOfFractional), offset);
                
                offset += lengthOfInteger;

                for (int i = lengthOfFractional; i > 0;)
                {
                    offset[i] = offset[--i];
                }

                *offset = DotSign;

                offset += lengthOfFractional + 1;
            }

            if (exponent != 0)
            {
                *offset = ExponentSign; ++offset;

                if (exponent > 0)
                {
                    *offset = PositiveSign; ++offset;
                }
                else
                {
                    *offset = NegativeSign; ++offset;
                    exponent = -exponent;
                }

                offset += ToString((ulong)exponent, offset);
            }

            return (int)(offset - chars);
        }

        /// <summary>
        /// 将一个 Single 值写入到空间足够的字符串中。
        /// </summary>
        /// <param name="value">Single 值</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ToString(float value, char* chars)
        {
            if (IsSpecialValue(value))
            {
                return ToSpecialString(value, chars);
            }

            var offset = chars;

            if (value < 0)
            {
                *offset = NegativeSign;

                ++offset;

                value = -value;
            }

            var exponent = GetExponent(value);

            var lengthOfInteger = exponent >= 0 && exponent < maxSingleDigitalsLength ? exponent + 1 : 1;

            var lengthOfFractional = maxSingleDigitalsLength - lengthOfInteger;

            if (exponent < maxSingleDigitalsLength && exponent > -maxFloatingZeroLength)
            {
                exponent = 0;
            }

            var mantissa = (ulong)Pow(value, lengthOfFractional - exponent);

            if (radix == DecimalRadix)
            {
                const int power4 = 10000;
                const int radix = DecimalRadix;

                ulong temp;

                if (lengthOfFractional >= 1)
                {
                    temp = mantissa / 10;

                    if (mantissa - temp * 10 >= 5)
                    {
                        mantissa = temp + 1;

                        lengthOfFractional -= 1;
                    }
                    else
                    {
                        mantissa = temp;

                        lengthOfFractional -= 1;
                    }
                }

                // Remove 0 of tails.
                while (lengthOfFractional >= 4 && mantissa == (temp = mantissa / power4) * power4)
                {
                    mantissa = temp;

                    lengthOfFractional -= 4;
                }

                while (lengthOfFractional > 0 && mantissa == (temp = mantissa / radix) * radix)
                {
                    mantissa = temp;

                    --lengthOfFractional;
                }
            }
            else
            {
                var power4 = uInt64Numbers[4];

                ulong temp;

                // Remove 0 of tails.
                while (lengthOfFractional >= 4 && mantissa == (temp = mantissa / power4) * power4)
                {
                    mantissa = temp;

                    lengthOfFractional -= 4;
                }

                while (lengthOfFractional > 0 && mantissa == (temp = mantissa / radix) * radix)
                {
                    mantissa = temp;

                    --lengthOfFractional;
                }
            }

            if (lengthOfFractional == 0)
            {
                var length = ToString(mantissa, offset);

                if (length > lengthOfInteger && exponent != 0)
                {
                    ++exponent;
                    --length;
                }

                offset += length;
            }
            else
            {
                ToString(mantissa, (byte)(lengthOfInteger + lengthOfFractional), offset);

                offset += lengthOfInteger;

                for (int i = lengthOfFractional; i > 0;)
                {
                    offset[i] = offset[--i];
                }

                *offset = DotSign;

                offset += lengthOfFractional + 1;
            }

            if (exponent != 0)
            {
                *offset = ExponentSign; ++offset;

                if (exponent > 0)
                {
                    *offset = PositiveSign; ++offset;
                }
                else
                {
                    *offset = NegativeSign; ++offset;
                    exponent = -exponent;
                }

                offset += ToString((ulong)exponent, offset);
            }

            return (int)(offset - chars);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public (ParseCode code, int length, double value) ParseDouble(char* chars, int length)
        {
            if (this.radix == DecimalRadix)
            {
                return DecimalParseDouble(chars, length);
            }

            var isNegative = false;
            var exponent = 0L;
            var floating = -1;
            var radix = this.radix;
            var maxNumber = uInt64NumbersLength - 1;

            var offset = 0;

            switch (chars[offset])
            {
                case PositiveSign:
                    ++offset;

                    if (chars[offset] == InfinitySign)
                    {
                        return (ParseCode.Success, 2, double.PositiveInfinity);
                    }

                    break;
                case NegativeSign:
                    ++offset;

                    if (chars[offset] == InfinitySign)
                    {
                        return (ParseCode.Success, 2, double.NegativeInfinity);
                    }

                    isNegative = true;

                    break;
                case NSign:
                case nSign:

                    if (StringHelper.EqualsWithIgnoreCase(new Ps<char>(chars, length), NaNSign))
                    {
                        return (ParseCode.Success, 3, double.NaN);
                    }

                    break;
                case InfinitySign:

                    return (ParseCode.Success, 1, double.PositiveInfinity);
            }

            var value = 0D;
            var code = ParseCode.Success;

        Loop:

            var swap = 0ul;
            var number = 0;

            for (; number < maxNumber && offset < length; ++offset)
            {
                var digital = ToRadix(chars[offset]);

                if (digital >= radix)
                {
                    switch (chars[offset])
                    {
                        case DotSign:

                            if (floating == -1)
                            {
                                ++floating;

                                continue;
                            }

                            break;
                        case ExponentSign:
                        case exponentSign:
                            ++offset;
                            goto Exponent;
                        case PositiveSign:
                        case NegativeSign:
                            // 在一些进制中 (15 进制以上)，指数符 e 和 数字 e(14) 有歧义。
                            // 这里判断当 e 后面紧随 +- 符号时当作指数符处理，否则按数字处理。
                            switch (chars[offset - 1])
                            {
                                case ExponentSign:
                                case exponentSign:

                                    if (number == 0)
                                    {
                                        value = (value - 0xE) / radix;
                                    }
                                    else
                                    {
                                        swap /= radix;
                                    }

                                    if (number > 0)
                                    {
                                        --number;
                                    }

                                    if (floating > 0)
                                    {
                                        --floating;
                                    }

                                    goto Exponent;
                            }

                            break;
                    }

                    code = ParseCode.OutOfRadix;

                    length = 0;

                    goto Return;
                }

                swap = swap * radix + digital;

                ++number;

                if (floating >= 0)
                {
                    ++floating;
                }
            }

        Return:

            if (number != 0)
            {
                value = value * uInt64Numbers[number] + swap;
            }

            if (offset < length)
            {
                goto Loop;
            }

            if (floating >= 1)
            {
                exponent -= floating;
            }

            if (exponent != 0)
            {
                if ((int)exponent != exponent)
                {
                    code = ParseCode.OutOfRange;

                    value = double.PositiveInfinity;
                }
                else
                {
                    value = Pow(value, (int)exponent);
                }
            }

            if (value > DoubleMaxPositive)
            {
                code = ParseCode.OutOfRange;
            }

            if (isNegative)
            {
                value = -value;
            }

            return (code, offset, value);

        Exponent:

            var expParse = ParseInt64(chars + offset, length - offset);

            if (expParse.code != ParseCode.Success)
            {
                code = expParse.code;
            }

            offset += expParse.length;

            exponent = expParse.value;

            length = 0;

            goto Return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static (ParseCode code, int length, double value) DecimalParseDouble(char* chars, int length)
        {
            var isNegative = false;
            var exponent = 0L;
            var floating = -1;
            const byte radix = DecimalRadix;
            const byte maxNumber = DecimalUInt64NumbersLength - 1;

            var offset = 0;

            switch (chars[offset])
            {
                case PositiveSign:
                    ++offset;

                    if (chars[offset] == InfinitySign)
                    {
                        return (ParseCode.Success, 2, double.PositiveInfinity);
                    }

                    break;
                case NegativeSign:
                    ++offset;

                    if (chars[offset] == InfinitySign)
                    {
                        return (ParseCode.Success, 2, double.NegativeInfinity);
                    }

                    isNegative = true;

                    break;
                case NSign:
                case nSign:

                    if (StringHelper.EqualsWithIgnoreCase(new Ps<char>(chars, length), NaNSign))
                    {
                        return (ParseCode.Success, 3, double.NaN);
                    }

                    break;
                case InfinitySign:

                    return (ParseCode.Success, 1, double.PositiveInfinity);
            }

            var value = 0D;
            var code = ParseCode.Success;

            Loop:

            var swap = 0ul;
            var number = 0;

            for (; number < maxNumber && offset < length; ++offset)
            {
                var digital = (uint)(chars[offset] - DigitalsZeroValue);

                if (digital >= radix)
                {
                    switch (chars[offset])
                    {
                        case DotSign:

                            if (floating == -1)
                            {
                                ++floating;

                                continue;
                            }

                            break;
                        case ExponentSign:
                        case exponentSign:
                            ++offset;
                            goto Exponent;
                    }

                    code = ParseCode.OutOfRadix;

                    length = 0;

                    goto Return;
                }

                swap = swap * radix + digital;

                ++number;

                if (floating >= 0)
                {
                    ++floating;
                }
            }

            Return:

            if (number != 0)
            {
                value = value * DecimalUInt64Numbers[number] + swap;
            }

            if (offset < length)
            {
                goto Loop;
            }

            if (floating >= 1)
            {
                exponent -= floating;
            }

            if (exponent != 0)
            {
                if ((int)exponent != exponent)
                {
                    code = ParseCode.OutOfRange;

                    value = double.PositiveInfinity;
                }
                else
                {
                    value = Decimal.Pow(value, (int)exponent);
                }
            }

            if (value > DoubleMaxPositive)
            {
                code = ParseCode.OutOfRange;
            }

            if (isNegative)
            {
                value = -value;
            }

            return (code, offset, value);

            Exponent:

            var expParse = DecimalParseInt64(chars + offset, length - offset);

            if (expParse.code != ParseCode.Success)
            {
                code = expParse.code;
            }

            offset += expParse.length;

            exponent = expParse.value;

            length = 0;

            goto Return;
        }
    }
}