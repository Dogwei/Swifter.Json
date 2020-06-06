using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供数字类的方法。
    /// 这些方法都是高效的。
    /// </summary>
    public sealed unsafe partial class NumberHelper
    {
        /// <summary>
        /// 支持的最大进制。
        /// </summary>
        public const byte MaxRadix = 64;
        /// <summary>
        /// 支持的最小进制。
        /// </summary>
        public const byte MinRadix = 2;
        /// <summary>
        /// 忽略大小写的前提下支持的最大进制。
        /// </summary>
        public const byte IgnoreCaseMaxRadix = 36;
        /// <summary>
        /// 十进制优先并自动匹配进制数。
        /// </summary>
        public const byte DecimalFirstRadix = unchecked((byte)-10);

        /// <summary>
        /// 最大进制优先，自动匹配进制数。
        /// </summary>
        public const byte MaxFirstRadix = unchecked((byte)-MaxRadix);

        internal const byte HexRadix = 16;
        internal const byte BinaryRadix = 2;
        internal const byte OctalRadix = 8;


        internal const byte DecimalRadix = 10;
        private const byte DecimalMaxScale = 28;
        private const byte DecimalMaxExponent = 29;

        private const char DigitalsMaxValue = '~';
        private const char DigitalsZeroValue = '0';

        /// <summary>
        /// 正负号。
        /// </summary>
        public const char PositiveSign = '+';
        /// <summary>
        /// 负符号。
        /// </summary>
        public const char NegativeSign = '-';
        /// <summary>
        /// 无限大符号。
        /// </summary>
        public const char InfinitySign = '∞';
        /// <summary>
        /// 指数符号。
        /// </summary>
        public const char ExponentSign = 'E';
        /// <summary>
        /// 指数符号（小写）。
        /// </summary>
        public const char exponentSign = 'e';
        /// <summary>
        /// 点符号。
        /// </summary>
        public const char DotSign = '.';
        /// <summary>
        /// 十六进制字符。
        /// </summary>
        public const char HexSign = 'X';
        /// <summary>
        /// 十六进制字符（小写）。
        /// </summary>
        public const char hexSign = 'x';
        /// <summary>
        /// 二进制字符。
        /// </summary>
        public const char BinarySign = 'B';
        /// <summary>
        /// 二进制字符（小写）。
        /// </summary>
        public const char binarySign = 'b';

        /// <summary>
        /// 非数字符号。
        /// </summary>
        public static readonly string NaNSign = double.NaN.ToString();

        /// <summary>
        /// 正无限大符号。
        /// </summary>
        public static readonly string PositiveInfinitySign = double.PositiveInfinity.ToString();

        /// <summary>
        /// 负无限大符号。
        /// </summary>
        public static readonly string NegativeInfinitySign = double.NegativeInfinity.ToString();
        /// <summary>
        /// 非数字符号的首字符。
        /// </summary>
        public const char NSign = 'N';
        /// <summary>
        /// 非数字符号的首字符。
        /// </summary>
        public const char nSign = 'n';
        /// <summary>
        /// 数字之间的分隔符。
        /// </summary>
        public const char SplitSign = '_';

        private const char ErrorDigital = (char)999;
        private const byte ErrorRadix = 99;
        private const byte SplitRadix = 100;
        private const byte ExponentRadix = 14;

        private const double DoubleMinPositive = 4.94065645841246544E-324;
        private const double DoubleMaxPositive = 1.79769313486231570E+308;
        private const double SingleMinPositive = 1.401298E-45;
        private const double SingleMaxPositive = 3.40282347E+38;

        private const uint DecimalDivisor = 1000000000;

        private const byte DecimalDivisorLength = 9;




        /// <summary>
        /// 表示当前实例的进制数。
        /// </summary>
        /* 10 */
        public readonly byte radix;

        /* ['0': 0, '1': 1, '2': 2,... '9': 9, 'a': 10, 'b':11,... 'A': 10, 'B': 11,... 'Z': 35, Other: ErrorRadix] */
        private readonly byte* radixes;

        /* 1000 */
        private readonly uint threeDigitalsLength;
        /* 000, 001, 002, 003, 004,...999 */
        private readonly ThreeChar* threeDigitals;

        /* 20 */
        private readonly byte uInt64NumbersLength;
        /* 10 */
        private readonly byte uInt32NumbersLength;
        /* 19 */
        private readonly byte int64NumbersLength;

        /* 1, 10, 100, 1000, 10000, 100000,... */
        private readonly ulong[] uInt64Numbers;

        /* 1, 10, 100, 1000, 10000, 100000,... */
        private readonly uint[] uInt32Numbers;

        /* 309 */
        private readonly int positiveExponentsLength;
        /* 324 */
        private readonly int negativeExponentsLength;

        /* 1, 10, 100, 1000, 1e4, 1e5, 1e6,... 1e308 */
        private readonly double[] positiveExponents;
        /* 1, 0.1, 0.01, 0.001, 1e-4, 1e-5, 1e-6,... 1e-324 */
        private readonly double[] negativeExponents;

        /* 5 */
        private const int maxFloatingZeroLength = 5;
        /* 16 */
        private readonly int maxDoubleDigitalsLength;
        /* 7 */
        private readonly int maxSingleDigitalsLength;

        /* 1000000000 */
        private readonly uint divisor;

        /* 9 */
        private readonly byte divisorLength;

        /// <summary>
        /// 当前进制下 Double 的最大字符串长度。
        /// </summary>
        public readonly int MaxDoubleStringLength;

        /// <summary>
        /// 当前进制下 Single 的最大字符串长度。
        /// </summary>
        public readonly int MaxSingleStringLength;

        /// <summary>
        /// 十进制下 Double 的最大字符串长度。
        /// </summary>
        public const int DecimalMaxDoubleStringLength = 32;

        /// <summary>
        /// 十进制下 Single 的最大字符串长度。
        /// </summary>
        public const int DecimalMaxSingleStringLength = 24;

        /// <summary>
        /// 当前进制下 UInt64 的最大字符串长度。
        /// </summary>
        public int MaxUInt64NumbersLength => uInt64NumbersLength + 1;

        /// <summary>
        /// 当前进制下 Int64 的最大字符串长度。
        /// </summary>
        public int MaxInt64NumbersLength => uInt64NumbersLength + 2;

        /// <summary>
        /// 十进制下 UInt64 的最大字符串长度。
        /// </summary>
        public const int DecimalMaxUInt64NumbersLength = 21;

        /// <summary>
        /// 十进制下 Int64 的最大字符串长度。
        /// </summary>
        public const int DecimalMaxInt64NumbersLength = 21;


        /// <summary>
        /// 初始化实例。
        /// </summary>
        /// <param name="radix">指定进制</param>
        NumberHelper(byte radix)
        {
            if (radix > MaxRadix || radix < MinRadix)
            {
                throw new ArgumentOutOfRangeException(nameof(radix));
            }

            this.radix = radix;

            if (radix <= IgnoreCaseMaxRadix)
            {
                radixes = IgnoreCaseRadixes;
            }
            else
            {
                radixes = Radixes;
            }

            threeDigitalsLength = (uint)SlowPow(radix, 3);
            threeDigitals = (ThreeChar*)Marshal.AllocHGlobal((int)(threeDigitalsLength * sizeof(ThreeChar)));

            for (uint i = 0; i < threeDigitalsLength; i++)
            {
                threeDigitals[i] = SlowToThreeChar(i);
            }

            int64NumbersLength = SlowGetLength(0x8000000000000000);
            uInt64NumbersLength = SlowGetLength(0xFFFFFFFFFFFFFFFF);
            uInt32NumbersLength = SlowGetLength(0xFFFFFFFF);

            uInt64Numbers = new ulong[uInt64NumbersLength];

            for (uint i = 0; i < uInt64NumbersLength; i++)
            {
                uInt64Numbers[i] = SlowPow(radix, i);
            }

            uInt32Numbers = new uint[uInt32NumbersLength];

            for (uint i = 0; i < uInt32NumbersLength; i++)
            {
                uInt32Numbers[i] = (uint)SlowPow(radix, i);
            }

            positiveExponentsLength = SlowGetLength(DoubleMaxPositive);

            positiveExponents = new double[positiveExponentsLength];

            for (int i = 0; i < positiveExponentsLength; i++)
            {
                positiveExponents[i] = Math.Pow(radix, i);
            }



            negativeExponentsLength = SlowGetFractionalLength(DoubleMinPositive);

            negativeExponents = new double[negativeExponentsLength];

            for (int i = 0; i < negativeExponentsLength; i++)
            {
                negativeExponents[i] = Math.Pow(radix, -i);
            }

            maxDoubleDigitalsLength = SlowGetLength(0xFFFFFFFFFFFFF) + 1 /* Integer: 1 */;
            maxSingleDigitalsLength = SlowGetLength(0x7FFFFF) + 1 /* Integer: 1 */;

            MaxDoubleStringLength = maxDoubleDigitalsLength + 4 /* Sign + Dot + Exp + ExpSign */ + SlowGetLength(0x200);
            MaxSingleStringLength = maxSingleDigitalsLength + 4 /* Sign + Dot + Exp + ExpSign */ + SlowGetLength(0x80);

            divisorLength = (byte)(uInt32NumbersLength - 1);
            divisor = uInt32Numbers[divisorLength];
        }


        private ThreeChar SlowToThreeChar(uint value)
        {
            ThreeChar t;

            t.char1 = Digitals[value / radix / radix % radix];
            t.char2 = Digitals[value / radix % radix];
            t.char3 = Digitals[value % radix];

            return t;
        }

        private byte SlowGetLength(ulong value)
        {
            byte result = 0;

            do
            {
                ++result;

                value /= radix;

            } while (value != 0);

            return result;
        }

        private int SlowGetLength(double value)
        {
            double rP16 = Math.Pow(radix, 16);

            var result = 0;

            while (value >= rP16)
            {
                value /= rP16;

                result += 16;
            }

            while (value >= 1)
            {
                value /= radix;

                ++result;
            }

            return result;
        }

        private int SlowGetFractionalLength(double value)
        {
            double rP16 = Math.Pow(radix, -16);

            var result = 0;

            while (value < rP16)
            {
                value /= rP16;

                result += 16;
            }

            while (value < 1)
            {
                value *= radix;

                ++result;
            }

            return result;
        }






        /// <summary>
        /// 拼接一位数字。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="value">数字，不可大于三位数</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void AppendD1(ref char chars, ulong value)
        {
            chars = ((char*)(threeDigitals + value))[2];
        }

        /// <summary>
        /// 拼接两位数字。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="value">数字，不可大于三位数</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void AppendD2(ref char chars, ulong value)
        {
            Underlying.As<char, int>(ref chars) = *(int*)(((char*)(threeDigitals + value)) + 1); ;
        }

        /// <summary>
        /// 拼接三位数字。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="value">数字，不可大于三位数</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void AppendD3(ref char chars, ulong value)
        {
            Underlying.As<char, long>(ref chars) = *(long*)(threeDigitals + value);
        }

        /// <summary>
        /// 拼接一位数字。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="value">数字，不可大于三位数</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void AppendD1(char* chars, ulong value)
        {
            *chars = ((char*)(threeDigitals + value))[2];
        }

        /// <summary>
        /// 拼接两位数字。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="value">数字，不可大于三位数</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void AppendD2(char* chars, ulong value)
        {
            *(int*)chars = *(int*)(((char*)(threeDigitals + value)) + 1);
        }

        /// <summary>
        /// 拼接三位数字。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="value">数字，不可大于三位数</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void AppendD3(char* chars, ulong value)
        {
            *(long*)chars = *(long*)(threeDigitals + value);
            // *(ThreeChar*)chars = threeDigitals[value];
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void AppendD6(char* chars, ulong value)
        {
            var l = threeDigitalsLength;

            var v = value;

            var a = v / l;

            v -= a * l;

            *(long*)(chars + 0) = *(long*)(threeDigitals + a);
            *(long*)(chars + 3) = *(long*)(threeDigitals + v);
            //*(ThreeChar*)(chars + 0) = threeDigitals[a];
            //*(ThreeChar*)(chars + 3) = threeDigitals[v];
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void AppendD9(char* chars, ulong value)
        {
            var l = threeDigitalsLength;

            var v = value;

            var b = v / l;
            var a = b / l;

            v -= b * l;
            b -= a * l;

            *(long*)(chars + 0) = *(long*)(threeDigitals + a);
            *(long*)(chars + 3) = *(long*)(threeDigitals + b);
            *(long*)(chars + 6) = *(long*)(threeDigitals + v);
            //*(ThreeChar*)(chars + 0) = threeDigitals[a];
            //*(ThreeChar*)(chars + 3) = threeDigitals[b];
            //*(ThreeChar*)(chars + 6) = threeDigitals[v];
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void AppendD12(char* chars, ulong value)
        {
            var l = threeDigitalsLength;
            var v = value;
            var c = v / l;
            var b = c / l;
            var a = b / l;

            v -= c * l;
            c -= b * l;
            b -= a * l;

            *(long*)(chars + 0) = *(long*)(threeDigitals + a);
            *(long*)(chars + 3) = *(long*)(threeDigitals + b);
            *(long*)(chars + 6) = *(long*)(threeDigitals + c);
            *(long*)(chars + 9) = *(long*)(threeDigitals + v);
            //*(ThreeChar*)(chars + 0) = threeDigitals[a];
            //*(ThreeChar*)(chars + 3) = threeDigitals[b];
            //*(ThreeChar*)(chars + 6) = threeDigitals[c];
            //*(ThreeChar*)(chars + 9) = threeDigitals[v];
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void AppendD15(char* chars, ulong value)
        {
            var l = threeDigitalsLength;

            var v = value;

            var d = v / l;
            var c = d / l;
            var b = c / l;
            var a = b / l;

            v -= d * l;
            d -= c * l;
            c -= b * l;
            b -= a * l;

            *(long*)(chars +  0) = *(long*)(threeDigitals + a);
            *(long*)(chars +  3) = *(long*)(threeDigitals + b);
            *(long*)(chars +  6) = *(long*)(threeDigitals + c);
            *(long*)(chars +  9) = *(long*)(threeDigitals + d);
            *(long*)(chars + 12) = *(long*)(threeDigitals + v);
            //*(ThreeChar*)(chars +  0) = threeDigitals[a];
            //*(ThreeChar*)(chars +  3) = threeDigitals[b];
            //*(ThreeChar*)(chars +  6) = threeDigitals[c];
            //*(ThreeChar*)(chars +  9) = threeDigitals[d];
            //*(ThreeChar*)(chars + 12) = threeDigitals[v];
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void AppendD18(char* chars, ulong value)
        {
            var l = threeDigitalsLength;

            var v = value;

            var e = v / l;
            var d = e / l;
            var c = d / l;
            var b = c / l;
            var a = b / l;

            v -= e * l;
            e -= d * l;
            d -= c * l;
            c -= b * l;
            b -= a * l;

            *(long*)(chars +  0) = *(long*)(threeDigitals + a);
            *(long*)(chars +  3) = *(long*)(threeDigitals + b);
            *(long*)(chars +  6) = *(long*)(threeDigitals + c);
            *(long*)(chars +  9) = *(long*)(threeDigitals + d);
            *(long*)(chars + 12) = *(long*)(threeDigitals + e);
            *(long*)(chars + 15) = *(long*)(threeDigitals + v);
            //*(ThreeChar*)(chars + 0) = threeDigitals[a];
            //*(ThreeChar*)(chars + 3) = threeDigitals[b];
            //*(ThreeChar*)(chars + 6) = threeDigitals[c];
            //*(ThreeChar*)(chars + 9) = threeDigitals[d];
            //*(ThreeChar*)(chars + 12) = threeDigitals[e];
            //*(ThreeChar*)(chars + 15) = threeDigitals[v];
        }





        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void DecimalAppendD1(char* chars, ulong value)
        {
            *chars = (char)(((byte)value) + '0');
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void DecimalAppendD2(char* chars, ulong value)
        {
            chars[0] = (char)(((byte)value) / 10 + '0');
            chars[1] = (char)(((byte)value) % 10 + '0');
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void DecimalAppendD3(char* chars, ulong value)
        {
            var digitals = DecimalThreeDigitals;

            *(long*)chars = *(long*)(digitals + value);
            // *(ThreeChar*)chars = digitals[value];
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void DecimalAppendD6(char* chars, ulong value)
        {
            var digitals = DecimalThreeDigitals;

            var v = value;

            var a = v / 1000;

            v -= a * 1000;

            *(long*)(chars + 0) = *(long*)(digitals + a);
            *(long*)(chars + 3) = *(long*)(digitals + v);
            //*((ThreeChar*)chars) = digitals[a];
            //*((ThreeChar*)(chars + 3)) = digitals[v - a * 1000];

        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void DecimalAppendD9(char* chars, ulong value)
        {
            var digitals = DecimalThreeDigitals;

            var v = value;

            var b = v / 1000;
            var a = b / 1000;

            v -= b * 1000;
            b -= a * 1000;

            *(long*)(chars + 0) = *(long*)(digitals + a);
            *(long*)(chars + 3) = *(long*)(digitals + b);
            *(long*)(chars + 6) = *(long*)(digitals + v);
            //*(ThreeChar*)(chars + 0) = digitals[a];
            //*(ThreeChar*)(chars + 3) = digitals[b];
            //*(ThreeChar*)(chars + 6) = digitals[v];
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void DecimalAppendD12(char* chars, ulong value)
        {
            var digitals = DecimalThreeDigitals;

            var v = value;

            var c = v / 1000;
            var b = c / 1000;
            var a = b / 1000;

            v -= c * 1000;
            c -= b * 1000;
            b -= a * 1000;

            *(long*)(chars + 0) = *(long*)(digitals + a);
            *(long*)(chars + 3) = *(long*)(digitals + b);
            *(long*)(chars + 6) = *(long*)(digitals + c);
            *(long*)(chars + 9) = *(long*)(digitals + v);
            //*(ThreeChar*)(chars + 0) = digitals[a];
            //*(ThreeChar*)(chars + 3) = digitals[b];
            //*(ThreeChar*)(chars + 6) = digitals[c];
            //*(ThreeChar*)(chars + 9) = digitals[v];
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void DecimalAppendD15(char* chars, ulong value)
        {
            var digitals = DecimalThreeDigitals;

            var v = value;
            var d = v / 1000;
            var c = d / 1000;
            var b = c / 1000;
            var a = b / 1000;

            v -= d * 1000;
            d -= c * 1000;
            c -= b * 1000;
            b -= a * 1000;

            *(long*)(chars +  0) = *(long*)(digitals + a);
            *(long*)(chars +  3) = *(long*)(digitals + b);
            *(long*)(chars +  6) = *(long*)(digitals + c);
            *(long*)(chars +  9) = *(long*)(digitals + d);
            *(long*)(chars + 12) = *(long*)(digitals + v);
            //*(ThreeChar*)(chars +  0) = digitals[a];
            //*(ThreeChar*)(chars +  3) = digitals[b];
            //*(ThreeChar*)(chars +  6) = digitals[c];
            //*(ThreeChar*)(chars +  9) = digitals[d];
            //*(ThreeChar*)(chars + 12) = digitals[v];
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void DecimalAppendD18(char* chars, ulong value)
        {
            var digitals = DecimalThreeDigitals;

            var v = value;

            var e = v / 1000;
            var d = e / 1000;
            var c = d / 1000;
            var b = c / 1000;
            var a = b / 1000;

            v -= e * 1000;
            e -= d * 1000;
            d -= c * 1000;
            c -= b * 1000;
            b -= a * 1000;

            *(long*)(chars +  0) = *(long*)(digitals + a);
            *(long*)(chars +  3) = *(long*)(digitals + b);
            *(long*)(chars +  6) = *(long*)(digitals + c);
            *(long*)(chars +  9) = *(long*)(digitals + d);
            *(long*)(chars + 12) = *(long*)(digitals + e);
            *(long*)(chars + 15) = *(long*)(digitals + v);
            //*(ThreeChar*)(chars +  0) = digitals[a];
            //*(ThreeChar*)(chars +  3) = digitals[b];
            //*(ThreeChar*)(chars +  6) = digitals[c];
            //*(ThreeChar*)(chars +  9) = digitals[d];
            //*(ThreeChar*)(chars + 12) = digitals[e];
            //*(ThreeChar*)(chars + 15) = digitals[v];
        }








        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private byte ToRadix(char c)
        {
            if (c <= DigitalsMaxValue)
            {
                return radixes[c];
            }

            return ErrorRadix;
        }
    }
}