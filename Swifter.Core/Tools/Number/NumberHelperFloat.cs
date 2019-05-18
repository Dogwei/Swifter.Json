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

            return -GetNegativeExponent(value);
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

        // Remove 0 of num tail.
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void FRemoveTailZero(ref ulong num, ref int numLen)
        {
            while (numLen > 0 && num % radix == 0)
            {
                num /= radix;

                --numLen;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private ulong FRound(ulong num)
        {
            return num / radix + (num % radix >= rounded ? 1U : 0U);
        }

        // [MethodImpl(MethodImplOptions.NoInlining)]
        private void FTryRound(ref ulong num, ref int numLen)
        {
            var tempNum = num;
            var tempNumLen = numLen;

            for (int i = 3; i < 6; i++)
            {
                tempNum = FRound(tempNum); --tempNumLen;
                FRemoveTailZero(ref tempNum, ref tempNumLen);

                if (tempNumLen + i < numLen)
                {
                    num = tempNum;
                    numLen = tempNumLen;

                    break;
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private int ToString(double value, int intLen/* Length of integer part in the value */, char* chars, int maxLength)
        {
            var c = chars;

            var integer = (ulong)value;

            if (integer == value) goto OnlyInteger;

            var numLen = maxLength;

            var fnum = (value - integer);

            if (fnum < CeilingApproximateValueOfZero) goto OnlyInteger;

            if (fnum > FloorApproximateValueOfOne) { ++integer; goto OnlyInteger; }

            var num = (ulong)(fnum * positiveExponents[numLen + 1]);

            num = FRound(num);

            FRemoveTailZero(ref num, ref numLen);

            // if (numLen >= 0xa) FTryRound(ref num, ref numLen);

            ToString(integer, (byte)intLen, c);

            c[intLen] = DotSign;

            ToString(num, (byte)numLen, c + intLen + 1);

            // Len of integer part + Len of dot + Len of num part.
            return intLen + 1 + numLen;

        OnlyInteger:

            ToString(integer, (byte)intLen, c);

            return intLen;
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
            var v = value;
            var c = chars;

            if (IsNaN(v))
            {
                return Append(chars, NaNSign);
            }

            if (v < 0)
            {
                *c = NegativeSign;

                ++c;

                v = -v;
            }

            if (v < DoubleMinPositive)
            {
                v = 0;
            }

            if (v > DoubleMaxPositive)
            {
                *c = InfinitySign;

                return ((int)(c - chars)) + 1;
            }

            var e = GetExponent(v);

            if (e > 0 && e < maxDoubleLength)
            {
                /* 1e1 to 1e16 */
                c += ToString(v, e + 1, c, maxDoubleLength);
            }
            else if (e <= 0 && e > -maxFractionalLength)
            {
                /* 1e-1 to 1e-5 */
                c += ToString(v, 1, c, maxDoubleLength);
            }
            else
            {
                c += ToString(Pow(v, e), 1, c, maxDoubleLength);
                
                *c = ExponentSign; ++c;

                if (e > 0)
                {
                    *c = PositiveSign; ++c;
                }
                else
                {
                    *c = NegativeSign; ++c;
                    e = -e;
                }

                c += ToString(e, c);
            }

            return (int)(c - chars);
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
            if (y > 0)
            {
                return x * negativeExponents[y];
            }

            y = -y;

            if (y < positiveExponentsRight)
            {
                return x * positiveExponents[y];
            }

            return x * positiveExponents[positiveExponentsRight] * positiveExponents[y - positiveExponentsRight];
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
            double v = value;
            var c = chars;

            if (IsNaN(v))
            {
                return Append(chars, NaNSign);
            }

            if (v < 0)
            {
                *c = NegativeSign;

                ++c;

                v = -v;
            }

            if (v < SingleMinPositive)
            {
                v = 0;
            }

            if (v > SingleMaxPositive)
            {
                *c = InfinitySign;

                return ((int)(c - chars)) + 1;
            }

            var e = GetExponent(v);

            if (e > 0 && e < maxSingleLength)
            {
                /* 1e1 to 1e7 */
                c += ToString(v, e + 1, c, maxSingleLength);
            }
            else if (e <= 0 && e > -maxFractionalLength)
            {
                /* 1e-1 to 1e-5 */
                c += ToString(v, 1, c, maxSingleLength);
            }
            else
            {
                c += ToString(Pow(v, e), 1, c, maxSingleLength);

                *c = ExponentSign; ++c;

                if (e > 0)
                {
                    *c = PositiveSign; ++c;
                }
                else
                {
                    *c = NegativeSign; ++c;
                    e = -e;
                }

                c += ToString(e, c);
            }

            return (int)(c - chars);
        }
        
        /// <summary>
        /// 尝试从字符串开始位置解析一个 Double 值。此方法允许指数。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="value">返回一个 Double 值</param>
        /// <param name="exception">当解析到错误时是否引发异常，异常不代表解析失败。</param>
        /// <returns>解析成功则返回解析的长度，失败则返回 0</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int TryParse(char* chars, int length, out double value, bool exception = false)
        {
            double v = 0;
            var n = false;
            var r = radix;
            int i = 0;
            var l = length;
            var m = uInt64NumbersLength - 1;
            var u = uInt64Numbers;
            var p = positiveExponents;
            var k = negativeExponents;
            var f = -1;
            var j = 0;
            long e = 0;

            if (l <= 0)
            {
                goto EmptyLength;
            }

            switch (chars[0])
            {
                case PositiveSign:
                    ++i;
                    break;
                case NegativeSign:
                    ++i;
                    n = true;
                    break;
            }

            ulong t = 0;
            int s = 0;

            while (i < l)
            {
                if (s == m)
                {
                    v = v * u[m] + t;

                    t = 0;
                    s = 0;
                }

                var d = ToRadix(chars[i]);

                if (d >= r)
                {
                    switch (chars[i])
                    {
                        case DotSign:

                            if (f != -1)
                            {
                                goto FormatException;
                            }

                            ++i;

                            f = j;
                            continue;
                        case InfinitySign:
                            if (n)
                            {
                                if (i == 1 && l >= 2)
                                {
                                    value = double.NegativeInfinity;

                                    return 2;
                                }
                            }
                            else if (i == 0 && l >= 1)
                            {
                                value = double.PositiveInfinity;

                                return 1;
                            }
                            goto OutOfRadix;
                        case NSign:
                            if (i == 0 && l >= 3 && chars[1] == NaNSign[1] && chars[2] == NaNSign[2])
                            {
                                value = double.NaN;

                                return 3;
                            }
                            goto OutOfRadix;
                        case ExponentSign:
                        case exponentSign:
                            ++i;
                            goto Exponent;
                        case PositiveSign:
                        case NegativeSign:
                            switch (chars[i - 1])
                            {
                                case ExponentSign:
                                case exponentSign:
                                    if (s == 0)
                                    {
                                        v /= r;
                                    }
                                    else
                                    {
                                        t /= r;
                                        --s;
                                    }

                                    --j;

                                    goto Exponent;
                            }
                            goto OutOfRadix;
                        default:
                            goto OutOfRadix;
                    }
                }

                t = t * r + d;

                ++s;
                ++i;
                ++j;
            }

        Return:

            if (s != 0)
            {
                v = v * u[s] + t;
            }

            if (f != -1)
            {
                if (j == f)
                {
                    goto FormatException;
                }

                e -= j - f;
            }

            if (e > 0)
            {
                if (e >= 1076)
                {
                    goto OutOfRange;
                }

                while (e >= 100)
                {
                    v *= p[100];

                    e -= 100;
                }

                while (e >= 10)
                {
                    v *= p[10];

                    e -= 10;
                }

                while (e >= 1)
                {
                    v *= r;

                    --e;
                }
            }
            else if (e < 0)
            {
                if (e <= -1076)
                {
                    goto OutOfRange;
                }

                while (e <= -100)
                {
                    v *= k[100];

                    e += 100;
                }

                while (e <= -10)
                {
                    v *= k[10];

                    e += 10;
                }

                while (e <= -1)
                {
                    v /= r;

                    ++e;
                }
            }

            if (v > DoubleMaxPositive)
            {
                goto OutOfRange;
            }

            if (n)
            {
                value = -v;
            }
            else
            {
                value = v;
            }

            return i;

        Exponent:

            i += TryParse(chars + i, l - i, out e, exception);

            goto Return;


        EmptyLength:
            if (exception) ThreadException = new FormatException("Double text format error.");
            goto ReturnFalse;
        FormatException:
            if (exception) ThreadException = new FormatException("Double text format error.");
            goto ReturnFalse;
        OutOfRange:
            if (exception) ThreadException = new OverflowException("Value out of Double range.");
            goto ReturnFalse;
        OutOfRadix:
            if (exception) ThreadException = new FormatException("Digit out of radix.");
            goto Return;

        ReturnFalse:
            value = 0;
            return 0;
        }
        
        /// <summary>
        /// 将 Double 值转换为字符串表现形式。
        /// </summary>
        /// <param name="value">Double 值</param>
        /// <returns>返回一个 String 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string ToString(double value)
        {
            var chars = stackalloc char[68];

            var length = ToString(value, chars);

            return new string(chars, 0, length);
        }

        /// <summary>
        /// 将 Single 值转换为字符串表现形式。
        /// </summary>
        /// <param name="value">Single 值</param>
        /// <returns>返回一个 String 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string ToString(float value)
        {
            var chars = stackalloc char[38];

            var length = ToString(value, chars);

            return new string(chars, 0, length);
        }
        
        /// <summary>
        /// 尝试将字符串转换为 Double 值。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="value">返回一个 Double 值</param>
        /// <returns>返回转换是否成功</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryParse(string text, out double value)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                return TryParse(chars, length, out value) == length;
            }
        }
        
        /// <summary>
        /// 将字符串转换为 Double 值。失败将引发异常。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns>返回一个 Double 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public double ParseDouble(string text)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                if (TryParse(chars, length, out double value, true) == length)
                {
                    return value;
                }

                throw ThreadException;
            }
        }
    }
}