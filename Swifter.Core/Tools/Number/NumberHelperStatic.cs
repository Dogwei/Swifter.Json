using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    public sealed unsafe partial class NumberHelper
    {
        /// <summary>
        /// 十进制数字字符串最大可能的长度。
        /// </summary>
        public const int DecimalStringMaxLength = 30;

        /// <summary>
        /// Guid 字符串包含分隔符的长度。
        /// </summary>
        public const int GuidStringWithSeparatorsLength = 36;

        /// <summary>
        /// Guid 字符串的长度。
        /// </summary>
        public const int GuidStringLength = 32;


        /* ['0', '1', '2', '3',... 'a', 'b', 'c',... 'A', 'B', 'C',... 'Z', '~', '!'] */
        private static readonly char* Digitals;
        /* ['0': 0, '1': 1, '2': 2,... '9': 9, 'a': 10, 'b':11,... 'A': 36, 'B': 37,... 'Z': 61, '~': 62, '!': 63, Other: ErrorRadix] */
        private static readonly byte* Radixes;
        /* ['0': 0, '1': 1, '2': 2,... '9': 9, 'a': 10, 'b':11,... 'A': 10, 'B': 11,... 'Z': 35, Other: ErrorRadix] */
        private static readonly byte* IgnoreCaseRadixes;

        private static readonly ThreeChar* DecimalThreeDigitals;
        private static readonly ulong[] DecimalUInt64Numbers;

        private const byte DecimalInt64NumbersLength = 19;
        private const byte DecimalUInt64NumbersLength = 20;

        /// <summary>
        /// 十进制实例。
        /// </summary>
        public static readonly NumberHelper Decimal;
        /// <summary>
        /// 十六进制实例。
        /// </summary>
        public static NumberHelper Hex => GetOrCreateInstance(HexRadix);
        /// <summary>
        /// 八进制实例。
        /// </summary>
        public static NumberHelper Octal => GetOrCreateInstance(OctalRadix);
        /// <summary>
        /// 二进制实例。
        /// </summary>
        public static NumberHelper Binary => GetOrCreateInstance(BinaryRadix);

        static readonly NumberHelper[] Instances;
        static readonly object InstancesLock;

        /// <summary>
        /// 获取指定进制的 NumberHelper 实例。
        /// </summary>
        /// <param name="radix">进制数</param>
        /// <returns>返回 NumberHelper 实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static NumberHelper GetOrCreateInstance(byte radix)
        {
            if (radix > MaxRadix || radix < MinRadix)
            {
                throw new ArgumentOutOfRangeException(nameof(radix));
            }

            var instance = Instances[radix];

            if (instance is null)
            {
                lock (InstancesLock)
                {
                    instance = Instances[radix];

                    if (instance is null)
                    {
                        instance = new NumberHelper(radix);

                        Instances[radix] = instance;
                    }
                }
            }

            return instance;
        }

        static NumberHelper()
        {
            Instances = new NumberHelper[MaxRadix + 1];
            InstancesLock = new object();

            Digitals = (char*)Marshal.AllocHGlobal(MaxRadix * sizeof(char));
            Radixes = (byte*)Marshal.AllocHGlobal((DigitalsMaxValue + 1) * sizeof(byte));
            IgnoreCaseRadixes = (byte*)Marshal.AllocHGlobal((DigitalsMaxValue + 1) * sizeof(byte));

            for (int i = 0; i < DigitalsMaxValue; i++)
            {
                Radixes[i] = ErrorRadix;
                IgnoreCaseRadixes[i] = ErrorRadix;
            }

            Radixes[SplitSign] = SplitRadix;
            IgnoreCaseRadixes[SplitSign] = SplitRadix;

            for (uint i = 0; i < MaxRadix; i++)
            {
                char digital = SlowToDigital(i);

                Digitals[i] = digital;
                Radixes[digital] = SlowToRadix(digital);
                IgnoreCaseRadixes[digital] = SlowToRadixIgnoreCase(digital);
            }

            Decimal = GetOrCreateInstance(DecimalRadix);

            DecimalThreeDigitals = Decimal.threeDigitals;
            DecimalUInt64Numbers = Decimal.uInt64Numbers;
        }




        private static char SlowToDigital(uint value)
        {
            if (value >= 0 && value <= 9)
            {
                return (char)(value + '0');
            }

            if (value >= 10 && value <= 35)
            {
                return (char)(value - 10 + 'a');
            }

            if (value >= 36 && value <= 61)
            {
                return (char)(value - 36 + 'A');
            }

            switch (value)
            {
                case 62:
                    return '~';
                case 63:
                    return '!';
                default:
                    return ErrorDigital;
            }
        }

        private static byte SlowToRadix(char value)
        {
            if (value >= '0' && value <= '9')
            {
                return (byte)(value - '0');
            }

            if (value >= 'a' && value <= 'z')
            {
                return (byte)(value - 'a' + 10);
            }

            if (value >= 'A' && value <= 'Z')
            {
                return (byte)(value - 'A' + 36);
            }

            switch (value)
            {
                case '~':
                    return 62;
                case '!':
                    return 63;
                default:
                    return ErrorRadix;
            }
        }

        private static byte SlowToRadixIgnoreCase(char value)
        {
            if (value >= '0' && value <= '9')
            {
                return (byte)(value - '0');
            }

            if (value >= 'a' && value <= 'z')
            {
                return (byte)(value - 'a' + 10);
            }

            if (value >= 'A' && value <= 'Z')
            {
                return (byte)(value - 'A' + 10);
            }

            return ErrorRadix;
        }

        private static ulong SlowPow(ulong x, uint y)
        {
            ulong result = 1;

            while (y > 0)
            {
                result *= x;

                --y;
            }

            return result;
        }


        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static int Copy(char* destination, string source)
        {
            var index = source.Length - 1;

            while (index >= 0)
            {
                destination[index] = source[index];

                --index;
            }

            return source.Length;
        }


        /// <summary>
        /// 将一个 Guid 值写入到一个空间足够的字符串中。
        /// </summary>
        /// <param name="value">Guid 值</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <param name="separator">是否包含分隔符</param>
        /// <returns>返回写入长度。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int ToString(Guid value, char* chars, bool separator)
        {
            var hex = Hex;
            var offset = chars;
            var ptr = (GuidStruct*)(&value);

            hex.AppendD2(offset, ptr->_a1); offset += 2;
            hex.AppendD2(offset, ptr->_a2); offset += 2;
            hex.AppendD2(offset, ptr->_a3); offset += 2;
            hex.AppendD2(offset, ptr->_a4); offset += 2;

            if (separator)
            {
                *offset = '-'; ++offset;
            }

            hex.AppendD2(offset, ptr->_b1); offset += 2;
            hex.AppendD2(offset, ptr->_b2); offset += 2;

            if (separator)
            {
                *offset = '-'; ++offset;
            }

            hex.AppendD2(offset, ptr->_c1); offset += 2;
            hex.AppendD2(offset, ptr->_c2); offset += 2;

            if (separator)
            {
                *offset = '-'; ++offset;
            }

            hex.AppendD2(offset, ptr->_d); offset += 2;
            hex.AppendD2(offset, ptr->_e); offset += 2;

            if (separator)
            {
                *offset = '-'; ++offset;
            }

            hex.AppendD2(offset, ptr->_f); offset += 2;
            hex.AppendD2(offset, ptr->_g); offset += 2;
            hex.AppendD2(offset, ptr->_h); offset += 2;
            hex.AppendD2(offset, ptr->_i); offset += 2;
            hex.AppendD2(offset, ptr->_j); offset += 2;
            hex.AppendD2(offset, ptr->_k); offset += 2;

            return (int)(offset - chars);
        }


        /// <summary>
        /// 获取一个十进制数字的小数刻度。
        /// </summary>
        /// <param name="value">十进制数字</param>
        /// <returns>返回小数的位数</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetScale(decimal value)
        {
            return Underlying.As<decimal, DecimalStruct>(ref value).Scale;
        }

        /// <summary>
        /// 获取十进制数字的长度。
        /// </summary>
        /// <param name="value">数字</param>
        /// <returns>返回长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetDecimalLength(ulong value)
        {
            if (value >= 100000)
            {
                if (value >= 10000000000)
                    if (value >= 1000000000000000)
                        if (value >= 100000000000000000)
                            if (value >= 10000000000000000000) return 20;
                            else if (value >= 1000000000000000000) return 19;
                            else return 18;
                        else if (value >= 10000000000000000) return 17;
                        else return 16;
                    else if (value >= 1000000000000)
                        if (value >= 100000000000000) return 15;
                        else if (value >= 10000000000000) return 14;
                        else return 13;
                    else if (value >= 100000000000) return 12;
                    else return 11;
                else if (value >= 10000000)
                    if (value >= 1000000000) return 10;
                    else if (value >= 100000000) return 9;
                    else return 8;
                else if (value >= 1000000) return 7;
                else return 6;
            }
            else if (value >= 100)
                if (value >= 10000) return 5;
                else if (value >= 1000) return 4;
                else return 3;
            else if (value >= 10) return 2;
            else return 1;
        }

        /// <summary>
        /// 将一个 Decimal 值写入到一个空间足够的字符串中。
        /// </summary>
        /// <param name="value">Decimal 值</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int ToString(decimal value, char* chars)
        {
            var offset = chars;

            var pDecimal = (DecimalStruct*)&value;
            var scale = pDecimal->Scale;
            var sign = pDecimal->Sign;

            pDecimal->GetBits((int*)pDecimal);

            if (sign != 0)
            {
                offset[0] = NegativeSign;

                sign = 1;
                ++offset;
            }

            var length = DirectOperateToDecimalString((uint*)pDecimal, 3, offset);

            if (scale == 0)
            {
                // Only integer.

                return length + sign;
            }
            else if (scale >= length)
            {
                // Only fractional.

                // = scale + lengthOf(0.)
                var i = scale + 2;

                while (length > 0)
                {
                    --length;
                    --i;

                    offset[i] = offset[length];
                }

                while (i > 2)
                {
                    --i;

                    offset[i] = DigitalsZeroValue;
                }

                offset[1] = DotSign;
                offset[0] = DigitalsZeroValue;

                return scale + sign + 2;
            }
            else
            {
                // Integer and fractional.

                var i = length;

                while (scale > 0)
                {
                    offset[i] = offset[--i];

                    --scale;
                }

                offset[i] = DotSign;

                return length + sign + 1;
            }
        }

        /// <summary>
        /// 将一个 UInt64 值以十进制格式写入到一个空间足够的字符串中。
        /// </summary>
        /// <param name="value">UInt64 值</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int ToDecimalString(ulong value, char* chars)
        {
            if (value >= 100000)
            {
                if (value >= 10000000000)
                    if (value >= 1000000000000000)
                        if (value >= 100000000000000000)
                            if (value >= 10000000000000000000) goto L20;
                            else if (value >= 1000000000000000000) goto L19;
                            else goto L18;
                        else if (value >= 10000000000000000) goto L17;
                        else goto L16;
                    else if (value >= 1000000000000)
                        if (value >= 100000000000000) goto L15;
                        else if (value >= 10000000000000) goto L14;
                        else goto L13;
                    else if (value >= 100000000000) goto L12;
                    else goto L11;
                else if (value >= 10000000)
                    if (value >= 1000000000) goto L10;
                    else if (value >= 100000000) goto L9;
                    else goto L8;
                else if (value >= 1000000) goto L7;
                else goto L6;
            }
            else if (value >= 100)
                if (value >= 10000) goto L5;
                else if (value >= 1000) goto L4;
                else goto L3;
            else if (value >= 10) goto L2;
            else goto L1;

            L20:
            ulong s = value / 1000000000000000000;
            DecimalAppendD2(chars, s);
            DecimalAppendD18(chars + 2, value - s * 1000000000000000000);
            return 20;

        L19:
            s = value / 1000000000000000000;
            DecimalAppendD1(chars, s);
            DecimalAppendD18(chars + 1, value - s * 1000000000000000000);
            return 19;

        L18:
            DecimalAppendD18(chars, value);
            return 18;

        L17:
            s = value / 1000000000000000;
            DecimalAppendD2(chars, s);
            DecimalAppendD15(chars + 2, value - s * 1000000000000000);
            return 17;

        L16:
            s = value / 1000000000000000;
            DecimalAppendD1(chars, s);
            DecimalAppendD15(chars + 1, value - s * 1000000000000000);
            return 16;

        L15:
            DecimalAppendD15(chars, value);
            return 15;

        L14:
            s = value / 1000000000000;
            DecimalAppendD2(chars, s);
            DecimalAppendD12(chars + 2, value - s * 1000000000000);
            return 14;

        L13:
            s = value / 1000000000000;
            DecimalAppendD1(chars, s);
            DecimalAppendD12(chars + 1, value - s * 1000000000000);
            return 13;

        L12:
            DecimalAppendD12(chars, value);
            return 12;

        L11:
            s = value / 1000000000;
            DecimalAppendD2(chars, s);
            DecimalAppendD9(chars + 2, value - s * 1000000000);
            return 11;

        L10:
            s = value / 1000000000;
            DecimalAppendD1(chars, s);
            DecimalAppendD9(chars + 1, value - s * 1000000000);
            return 10;

        L9:
            DecimalAppendD9(chars, value);
            return 9;

        L8:
            s = value / 1000000;
            DecimalAppendD2(chars, s);
            DecimalAppendD6(chars + 2, value - s * 1000000);
            return 8;

        L7:
            s = value / 1000000;
            DecimalAppendD1(chars, s);
            DecimalAppendD6(chars + 1, value - s * 1000000);
            return 7;

        L6:
            DecimalAppendD6(chars, value);
            return 6;

        L5:
            s = value / 1000;
            DecimalAppendD2(chars, s);
            DecimalAppendD3(chars + 2, value - s * 1000);
            return 5;

        L4:
            s = value / 1000;
            DecimalAppendD1(chars, s);
            DecimalAppendD3(chars + 1, value - s * 1000);
            return 4;

        L3:
            DecimalAppendD3(chars, value);
            return 3;

        L2:
            DecimalAppendD2(chars, value);
            return 2;

        L1:
            DecimalAppendD1(chars, value);
            return 1;
        }

        /// <summary>
        /// 将一个 Int64 值写入到空间足够的字符串中。
        /// </summary>
        /// <param name="value">Int64 值</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int ToDecimalString(long value, char* chars)
        {
            if (value >= 0)
            {
                return ToDecimalString((ulong)(value), chars);
            }
            else
            {
                *chars = NegativeSign;

                return ToDecimalString((ulong)(-value), chars + 1) + 1;
            }
        }


        /// <summary>
        /// 将指定长度的 UInt64 值以十进制格式写入到空间足够的字符串中。
        /// </summary>
        /// <param name="value">UInt64 值</param>
        /// <param name="length">指定长度</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void ToDecimalString(ulong value, byte length, char* chars)
        {
            switch (length)
            {
                case 0:
                    return;
                case 1:
                    goto L1;
                case 2:
                    goto L2;
                case 3:
                    goto L3;
                case 4:
                    goto L4;
                case 5:
                    goto L5;
                case 6:
                    goto L6;
                case 7:
                    goto L7;
                case 8:
                    goto L8;
                case 9:
                    goto L9;
                case 10:
                    goto L10;
                case 11:
                    goto L11;
                case 12:
                    goto L12;
                case 13:
                    goto L13;
                case 14:
                    goto L14;
                case 15:
                    goto L15;
                case 16:
                    goto L16;
                case 17:
                    goto L17;
                case 18:
                    goto L18;
                case 19:
                    goto L19;
            }

            ulong s = value / 1000000000000000000;
            DecimalAppendD2(chars, s);
            DecimalAppendD18(chars + 2, value - s * 1000000000000000000);
            return;

        L19:
            s = value / 1000000000000000000;
            DecimalAppendD1(chars, s);
            DecimalAppendD18(chars + 1, value - s * 1000000000000000000);
            return;

        L18:
            DecimalAppendD18(chars, value);
            return;

        L17:
            s = value / 1000000000000000;
            DecimalAppendD2(chars, s);
            DecimalAppendD15(chars + 2, value - s * 1000000000000000);
            return;

        L16:
            s = value / 1000000000000000;
            DecimalAppendD1(chars, s);
            DecimalAppendD15(chars + 1, value - s * 1000000000000000);
            return;

        L15:
            DecimalAppendD15(chars, value);
            return;

        L14:
            s = value / 1000000000000;
            DecimalAppendD2(chars, s);
            DecimalAppendD12(chars + 2, value - s * 1000000000000);
            return;

        L13:
            s = value / 1000000000000;
            DecimalAppendD1(chars, s);
            DecimalAppendD12(chars + 1, value - s * 1000000000000);
            return;

        L12:
            DecimalAppendD12(chars, value);
            return;

        L11:
            s = value / 1000000000;
            DecimalAppendD2(chars, s);
            DecimalAppendD9(chars + 2, value - s * 1000000000);
            return;

        L10:
            s = value / 1000000000;
            DecimalAppendD1(chars, s);
            DecimalAppendD9(chars + 1, value - s * 1000000000);
            return;

        L9:
            DecimalAppendD9(chars, value);
            return;

        L8:
            s = value / 1000000;
            DecimalAppendD2(chars, s);
            DecimalAppendD6(chars + 2, value - s * 1000000);
            return;

        L7:
            s = value / 1000000;
            DecimalAppendD1(chars, s);
            DecimalAppendD6(chars + 1, value - s * 1000000);
            return;

        L6:
            DecimalAppendD6(chars, value);
            return;

        L5:
            s = value / 1000;
            DecimalAppendD2(chars, s);
            DecimalAppendD3(chars + 2, value - s * 1000);
            return;

        L4:
            s = value / 1000;
            DecimalAppendD1(chars, s);
            DecimalAppendD3(chars + 1, value - s * 1000);
            return;

        L3:
            DecimalAppendD3(chars, value);
            return;

        L2:
            DecimalAppendD2(chars, value);
            return;

        L1:
            DecimalAppendD1(chars, value);
            return;
        }



        /// <summary>
        /// 将一个字节正整数写入到空间足够的字符串中。此方法对字节正整数直接运算，所以会改变它的值。
        /// </summary>
        /// <param name="value">字节正整数</param>
        /// <param name="length">字节正整数的长度</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int DirectOperateToDecimalString(uint* value, int length, char* chars)
        {
            length = Div(value, length, DecimalDivisor, out uint remainder);

            if (length == 0)
            {
                return ToDecimalString(remainder, chars);
            }

            length = length == 1 ?
                ToDecimalString(*value, chars) :
                DirectOperateToDecimalString(value, length, chars);

            ToDecimalString(remainder, 9, chars + length);

            return length + 9;
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
            GuidStruct value = default;

            var offset = chars;

            if (length < 32)
            {
                goto Error;
            }

            if (*offset == '{')
            {
                ++offset;
                --length;
            }

            if (!TryParseHexByte(ref offset, ref value._a1)) goto False;
            if (!TryParseHexByte(ref offset, ref value._a2)) goto False;
            if (!TryParseHexByte(ref offset, ref value._a3)) goto False;
            if (!TryParseHexByte(ref offset, ref value._a4)) goto False;


            if (*offset == '-')
            {
                ++offset;
                --length;
            }

            if (!TryParseHexByte(ref offset, ref value._b1)) goto False;
            if (!TryParseHexByte(ref offset, ref value._b2)) goto False;

            if (*offset == '-')
            {
                ++offset;
                --length;
            }

            if (!TryParseHexByte(ref offset, ref value._c1)) goto False;
            if (!TryParseHexByte(ref offset, ref value._c2)) goto False;

            if (*offset == '-')
            {
                ++offset;
                --length;
            }

            if (!TryParseHexByte(ref offset, ref value._d)) goto False;
            if (!TryParseHexByte(ref offset, ref value._e)) goto False;

            if (*offset == '-')
            {
                ++offset;
                --length;
            }

            if (length < 32)
            {
                goto Error;
            }

            if (!TryParseHexByte(ref offset, ref value._f)) goto False;
            if (!TryParseHexByte(ref offset, ref value._g)) goto False;
            if (!TryParseHexByte(ref offset, ref value._h)) goto False;
            if (!TryParseHexByte(ref offset, ref value._i)) goto False;
            if (!TryParseHexByte(ref offset, ref value._j)) goto False;
            if (!TryParseHexByte(ref offset, ref value._k)) goto False;

            if (*offset == '}')
            {
                ++offset;
                --length;
            }

            return (ParseCode.Success, (int)(offset - chars), Underlying.As<GuidStruct, Guid>(ref value));

            Error:
            return (ParseCode.WrongFormat, 0, default);

            False:
            return (ParseCode.OutOfRadix, 0, default);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static bool TryParseHexByte(ref char* chars, ref byte value)
        {
            var a = Hex.ToRadix(chars[0]);
            var b = Hex.ToRadix(chars[1]);

            if ((a | b) >= 16)
            {
                return false;
            }

            value = (byte)((a << 4) | b);

            chars += 2;

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static (ParseCode code, int length, decimal value) ParseDecimal(char* chars, int length)
        {
            var isNegative = false;
            var exponent = 0L;
            var floating = -1;
            const byte radix = DecimalRadix;

            var offset = 0;

            switch (chars[offset])
            {
                case PositiveSign:
                    ++offset;
                    break;
                case NegativeSign:
                    ++offset;

                    isNegative = true;
                    break;
            }

            var value = default(decimal);
            var lengthOfValueBits = 1;
            var code = ParseCode.Success;

            Loop:

            var swap = 0U;
            var number = 0;

            for (; number < DecimalDivisorLength && offset < length; ++offset)
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
                Mult((uint*)&value, lengthOfValueBits, (uint)DecimalUInt64Numbers[number], out var carry);

                if (carry != 0)
                {
                    ((uint*)&value)[lengthOfValueBits] = carry;

                    ++lengthOfValueBits;
                }

                Add((uint*)&value, lengthOfValueBits, swap, out carry);

                if (carry != 0)
                {
                    ((uint*)&value)[lengthOfValueBits] = carry;

                    ++lengthOfValueBits;
                }
            }

            if (lengthOfValueBits > 3)
            {
                code = ParseCode.OutOfRange;

                length = 0;
            }

            if (offset < length)
            {
                goto Loop;
            }

            if (floating >= 1)
            {
                exponent -= floating;
            }

            if (exponent > 0)
            {
                if (exponent > DecimalMaxExponent)
                {
                    code = ParseCode.OutOfRange;
                }
                else
                {
                    do
                    {
                        number = Math.Min(DecimalDivisorLength, (int)exponent);

                        exponent -= number;

                        Mult((uint*)&value, lengthOfValueBits, (uint)DecimalUInt64Numbers[number], out var carry);

                        if (carry != 0)
                        {
                            ((uint*)&value)[lengthOfValueBits] = carry;

                            ++lengthOfValueBits;

                            if (lengthOfValueBits > 3)
                            {
                                code = ParseCode.OutOfRange;

                                break;
                            }
                        }

                    } while (exponent > 0);
                }
            }

            ((DecimalStruct*)(&value))->SetBits((int*)(&value));

            if (exponent < 0)
            {
                if (exponent < -DecimalMaxScale)
                {
                    code = ParseCode.OutOfRange;
                }
                else
                {
                    ((DecimalStruct*)(&value))->Scale = (int)(-exponent);
                }
            }

            if (isNegative)
            {
                ((DecimalStruct*)(&value))->Sign = 1;
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




        /// <summary>
        /// 字节正整数乘以 UInt32 值。
        /// </summary>
        /// <param name="number">字节正整数</param>
        /// <param name="length">字节正整数的长度</param>
        /// <param name="value">UInt32 值</param>
        /// <param name="carry">进位值</param>
        /// <returns>返回字节正整数的长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int Mult(uint* number, int length, uint value, out uint carry)
        {
            ulong c = 0;

            for (int i = 0; i < length; ++i)
            {
                c = ((ulong)number[i]) * value + c;

                number[i] = (uint)c;

                c >>= 32;
            }

            if (c == 0)
            {
                carry = 0;

                --number;

                while (length >= 2 && number[length] == 0)
                {
                    --length;
                }

                return length;
            }

            carry = (uint)c;

            return length;
        }

        /// <summary>
        /// 字节正整数除以 UInt32 值。
        /// </summary>
        /// <param name="number">字节正整数</param>
        /// <param name="length">字节正整数的长度</param>
        /// <param name="value">UInt32 值</param>
        /// <param name="remainder">余数</param>
        /// <returns>返回字节正整数的长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int Div(uint* number, int length, uint value, out uint remainder)
        {
            ulong carry = 0;

            if (value == 0)
            {
                throw new DivideByZeroException();
            }

            if (value == 1)
            {
                remainder = 0;

                return length;
            }

            int i = length - 1;

        Top:

            if (i >= 0)
            {
                ulong t = number[i] + (carry << 32);

                if (t < value)
                {
                    carry = t;

                    number[i] = 0;

                    --length;
                    --i;

                    goto Top;
                }

                goto Div;

            Loop:

                t = number[i] + (carry << 32);

                if (t < value)
                {
                    carry = t;

                    number[i] = 0;

                    goto Next;
                }

            Div:
                carry = t / value;

                number[i] = (uint)carry;

                carry = (t - carry * value);

            Next:
                --i;

                if (i >= 0)
                {
                    goto Loop;
                }
            }

            remainder = (uint)carry;

            return length;
        }

        /// <summary>
        /// 字节正整数加上 UInt32 值。
        /// </summary>
        /// <param name="number">字节正整数</param>
        /// <param name="length">字节正整数的长度</param>
        /// <param name="value">UInt32 值</param>
        /// <param name="carry">进位值</param>
        /// <returns>返回字节正整数的长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int Add(uint* number, int length, uint value, out uint carry)
        {
            if (length <= 0)
            {
                carry = value;

                return 0;
            }

            var t = *number;

            *number = t + value;

            if (t > uint.MaxValue - value)
            {
                for (int i = 1; i < length; ++i)
                {
                    if (number[i] != uint.MaxValue)
                    {
                        ++number[i];

                        carry = 0;

                        return length;
                    }

                    number[i] = 0;
                }

                carry = 1;

                return length;
            }

            carry = 0;

            return length;
        }

        /// <summary>
        /// 字节正整数减去 UInt32 值。
        /// </summary>
        /// <param name="number">字节正整数</param>
        /// <param name="length">字节正整数的长度</param>
        /// <param name="value">UInt32 值</param>
        /// <param name="remainder">余数</param>
        /// <returns>返回字节正整数的长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int Sub(uint* number, int length, uint value, out uint remainder)
        {
            if (length <= 0)
            {
                remainder = value;

                return 0;
            }

            if (*number >= value)
            {
                *number -= value;

                remainder = 0;

                if (*number == 0 && length == 1)
                {
                    return 0;
                }

                return length;
            }

            if (length != 1)
            {
                *number = (uint)((0x100000000 + *number) - value);

                for (int i = 1; i < length; ++i)
                {
                    if (number[i] == 0)
                    {
                        number[i] = 0xFFFFFFFF;
                    }
                    else if (number[i] == 1 && i + 1 == length)
                    {
                        number[i] = 0;

                        remainder = 0;

                        return length - 1;
                    }
                    else
                    {
                        --number[i];

                        remainder = 0;

                        return length;
                    }
                }
            }

            remainder = value - *number;

            *number = 0;

            return 0;
        }
    }
}
