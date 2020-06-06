using System;
using System.Runtime.CompilerServices;


namespace Swifter.Tools
{
    public sealed unsafe partial class NumberHelper
    {
        /// <summary>
        /// 获取 UInt64 值的字符串表现形式长度。
        /// </summary>
        /// <param name="value">UInt64 值</param>
        /// <returns>返回字符串表现形式长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public byte GetLength(ulong value)
        {
            var nums = uInt64Numbers;
            var length = nums.Length;

            // 一个二分树。
            // Binary Tree
            if (value >= nums[5])
            {
                if (value >= nums[10])
                    if (length > 15 && value >= nums[15])
                        if (length > 17 && value >= nums[17])
                            if (length > 19 && value >= nums[19])
                                goto GE20;
                            else if (length > 18 && value >= nums[18])
                                return 19;
                            else
                                return 18;
                        else if (length > 16 && value >= nums[16])
                            return 17;
                        else
                            return 16;
                    else if (length > 12 && value >= nums[12])
                        if (length > 14 && value >= nums[14])
                            return 15;
                        else if (length > 13 && value >= nums[13])
                            return 14;
                        else
                            return 13;
                    else if (length > 11 && value >= nums[11])
                        return 12;
                    else
                        return 11;
                else if (value >= nums[7])
                    if (value >= nums[9])
                        return 10;
                    else if (value >= nums[8])
                        return 9;
                    else
                        return 8;
                else if (value >= nums[6])
                    return 7;
                else
                    return 6;
            }
            else if (value >= nums[2])
                if (value >= nums[4])
                    return 5;
                else if (value >= nums[3])
                    return 4;
                else
                    return 3;
            else if (value >= nums[1])
                return 2;
            else
                return 1;

            GE20:

            if (length > 20 && value >= nums[20])
            {
                return (byte)(20 + GetLength(value / nums[20]));
            }

            return 20;
        }

        /// <summary>
        /// 将一个 UInt64 值写入到空间足够的字符串中。
        /// </summary>
        /// <param name="value">UInt64 值</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ToString(ulong value, char* chars)
        {
            if (radix == DecimalRadix)
            {
                return ToDecimalString(value, chars);
            }

            var nums = uInt64Numbers;
            var length = nums.Length;

            if (value >= nums[5])
            {
                if (value >= nums[10])
                    if (length > 15 && value >= nums[15])
                        if (length > 17 && value >= nums[17])
                            if (length > 19 && value >= nums[19]) goto L20;
                            else if (length > 18 && value >= nums[18]) goto L19;
                            else goto L18;
                        else if (length > 16 && value >= nums[16]) goto L17;
                        else goto L16;
                    else if (length > 12 && value >= nums[12])
                        if (length > 14 && value >= nums[14]) goto L15;
                        else if (length > 13 && value >= nums[13]) goto L14;
                        else goto L13;
                    else if (length > 11 && value >= nums[11]) goto L12;
                    else goto L11;
                else if (value >= nums[7])
                    if (value >= nums[9]) goto L10;
                    else if (value >= nums[8]) goto L9;
                    else goto L8;
                else if (value >= nums[6]) goto L7;
                else goto L6;
            }
            else if (value >= nums[2])
                if (value >= nums[4]) goto L5;
                else if (value >= nums[3]) goto L4;
                else goto L3;
            else if (value >= nums[1]) goto L2;
            else goto L1;

            L20:

            var s = value / nums[18];

            if (length > 20 && value >= nums[20])
            {
                var r = ToString(s, chars);

                AppendD18(chars + r, value - s * nums[18]);

                return r + 18;
            }

            AppendD2(chars, s);
            AppendD18(chars + 2, value - s * nums[18]);
            return 20;

            L19:
            s = value / nums[18];
            AppendD1(chars, s);
            AppendD18(chars + 1, value - s * nums[18]);
            return 19;

            L18:
            AppendD18(chars, value);
            return 18;

            L17:
            s = value / nums[15];
            AppendD2(chars, s);
            AppendD15(chars + 2, value - s * nums[15]);
            return 17;

            L16:
            s = value / nums[15];
            AppendD1(chars, s);
            AppendD15(chars + 1, value - s * nums[15]);
            return 16;

            L15:
            AppendD15(chars, value);
            return 15;

            L14:
            s = value / nums[12];
            AppendD2(chars, s);
            AppendD12(chars + 2, value - s * nums[12]);
            return 14;

            L13:
            s = value / nums[12];
            AppendD1(chars, s);
            AppendD12(chars + 1, value - s * nums[12]);
            return 13;

            L12:
            AppendD12(chars, value);
            return 12;

            L11:
            s = value / nums[9];
            AppendD2(chars, s);
            AppendD9(chars + 2, value - s * nums[9]);
            return 11;

            L10:
            s = value / nums[9];
            AppendD1(chars, s);
            AppendD9(chars + 1, value - s * nums[9]);
            return 10;

            L9:
            AppendD9(chars, value);
            return 9;

            L8:
            s = value / nums[6];
            AppendD2(chars, s);
            AppendD6(chars + 2, value - s * nums[6]);
            return 8;

            L7:
            s = value / nums[6];
            AppendD1(chars, s);
            AppendD6(chars + 1, value - s * nums[6]);
            return 7;

            L6:
            AppendD6(chars, value);
            return 6;

            L5:
            s = value / nums[3];
            AppendD2(chars, s);
            AppendD3(chars + 2, value - s * nums[3]);
            return 5;

            L4:
            s = value / nums[3];
            AppendD1(chars, s);
            AppendD3(chars + 1, value - s * nums[3]);
            return 4;

            L3:
            AppendD3(chars, value);
            return 3;

            L2:
            AppendD2(chars, value);
            return 2;

            L1:
            AppendD1(chars, value);
            return 1;
        }

        /// <summary>
        /// 将指定长度的 UInt64 值写入到空间足够的字符串中。
        /// </summary>
        /// <param name="value">UInt64 值</param>
        /// <param name="length">指定长度</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ToString(ulong value, byte length, char* chars)
        {
            if (radix == DecimalRadix)
            {
                ToDecimalString(value, length, chars);

                return;
            }

            var nums = uInt64Numbers;

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

            var s = value / nums[18];

            if (length > 20)
            {
                length -= 18;

                ToString(s, length, chars);

                AppendD18(chars + length, value - s * nums[18]);

                return;
            }

            AppendD2(chars, s);
            AppendD18(chars + 2, value - s * nums[18]);
            return;

            L19:
            s = value / nums[18];
            AppendD1(chars, s);
            AppendD18(chars + 1, value - s * nums[18]);
            return;

            L18:
            AppendD18(chars, value);
            return;

            L17:
            s = value / nums[15];
            AppendD2(chars, s);
            AppendD15(chars + 2, value - s * nums[15]);
            return;

            L16:
            s = value / nums[15];
            AppendD1(chars, s);
            AppendD15(chars + 1, value - s * nums[15]);
            return;

            L15:
            AppendD15(chars, value);
            return;

            L14:
            s = value / nums[12];
            AppendD2(chars, s);
            AppendD12(chars + 2, value - s * nums[12]);
            return;

            L13:
            s = value / nums[12];
            AppendD1(chars, s);
            AppendD12(chars + 1, value - s * nums[12]);
            return;

            L12:
            AppendD12(chars, value);
            return;

            L11:
            s = value / nums[9];
            AppendD2(chars, s);
            AppendD9(chars + 2, value - s * nums[9]);
            return;

            L10:
            s = value / nums[9];
            AppendD1(chars, s);
            AppendD9(chars + 1, value - s * nums[9]);
            return;

            L9:
            AppendD9(chars, value);
            return;

            L8:
            s = value / nums[6];
            AppendD2(chars, s);
            AppendD6(chars + 2, value - s * nums[6]);
            return;

            L7:
            s = value / nums[6];
            AppendD1(chars, s);
            AppendD6(chars + 1, value - s * nums[6]);
            return;

            L6:
            AppendD6(chars, value);
            return;

            L5:
            s = value / nums[3];
            AppendD2(chars, s);
            AppendD3(chars + 2, value - s * nums[3]);
            return;

            L4:
            s = value / nums[3];
            AppendD1(chars, s);
            AppendD3(chars + 1, value - s * nums[3]);
            return;

            L3:
            AppendD3(chars, value);
            return;

            L2:
            AppendD2(chars, value);
            return;

            L1:
            AppendD1(chars, value);
            return;
        }

        /// <summary>
        /// 将一个 Int64 值写入到空间足够的字符串中。
        /// </summary>
        /// <param name="value">Int64 值</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ToString(long value, char* chars)
        {
            if (value >= 0)
            {
                return ToString((ulong)(value), chars);
            }
            else
            {
                *chars = NegativeSign;

                return ToString((ulong)(-value), chars + 1) + 1;
            }
        }

        /// <summary>
        /// 将一个字节正整数写入到空间足够的字符串中。
        /// </summary>
        /// <param name="value">字节正整数</param>
        /// <param name="length">字节正整数的长度</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ToString(uint* value, int length, char* chars)
        {
            var temp = stackalloc uint[length];

            for (int i = 0; i < length; i++)
            {
                temp[i] = value[i];
            }

            return DirectOperateToString(temp, length, chars);
        }

        /// <summary>
        /// 将一个字节正整数写入到空间足够的字符串中。此方法对字节正整数直接运算，所以会改变它的值。
        /// </summary>
        /// <param name="value">字节正整数</param>
        /// <param name="length">字节正整数的长度</param>
        /// <param name="chars">空间足够的字符串</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int DirectOperateToString(uint* value, int length, char* chars)
        {
            if (radix == DecimalRadix)
            {
                return DirectOperateToDecimalString(value, length, chars);
            }

            length = Div(value, length, divisor, out var r);

            if (length == 0)
            {
                return ToString((ulong)r, chars);
            }

            length = length == 1 ? 
                ToString(*value, chars) : 
                DirectOperateToString(value, length, chars);

            ToString(r, divisorLength, chars + length);

            return length + divisorLength;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public (ParseCode code, int length, long value) ParseInt64(char* chars, int length)
        {
            if (radix == DecimalRadix)
            {
                return DecimalParseInt64(chars, length);
            }

            if (length <= 0)
            {
                return (ParseCode.Empty, 0, 0);
            }

            byte sign = 0;

            var offset = 0;

            switch (chars[offset])
            {
                case NegativeSign:
                    ++offset;
                    sign = 1;
                    break;
                case PositiveSign:
                    ++offset;
                    break;
            }

            while (offset < length && chars[offset] == DigitalsZeroValue)
            {
                ++offset;
            }

            var value = 0L;
            var code = ParseCode.Success;

            for (int right = Math.Min((int64NumbersLength - 1) + offset, length); offset < right; ++offset)
            {
                var digital = ToRadix(chars[offset]);

                if (digital >= radix)
                {
                    code = ParseCode.OutOfRadix;

                    goto Return;
                }

                value = value * radix - digital;
            }

            if (offset < length)
            {
                var digital = ToRadix(chars[offset]);

                if (digital >= radix)
                {
                    code = ParseCode.OutOfRadix;

                    goto Return;
                }

                if (value < (-long.MaxValue - sign + digital) / radix)
                {
                    code = ParseCode.OutOfRange;

                    goto Return;
                }

                value = value * radix - digital;

                ++offset;
            }

            Return:

            if (sign == 0)
            {
                value = -value;
            }

            return (code, offset, value);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static (ParseCode code, int length, long value) DecimalParseInt64(char* chars, int length)
        {
            if (length <= 0)
            {
                return (ParseCode.Empty, 0, 0);
            }

            byte sign = 0;

            var offset = 0;

            switch (chars[offset])
            {
                case NegativeSign:
                    ++offset;
                    sign = 1;
                    break;
                case PositiveSign:
                    ++offset;
                    break;
            }

            while (offset < length && chars[offset] == DigitalsZeroValue)
            {
                ++offset;
            }

            const byte radix = DecimalRadix;
            var value = 0L;
            var code = ParseCode.Success;

            for (int right = Math.Min((DecimalInt64NumbersLength - 1) + offset, length); offset < right; ++offset)
            {
                var digital = (uint)(chars[offset] - DigitalsZeroValue);

                if (digital >= radix)
                {
                    code = ParseCode.OutOfRadix;

                    goto Return;
                }

                value = value * radix - digital;
            }

            if (offset < length)
            {
                var digital = (uint)(chars[offset] - DigitalsZeroValue);

                if (digital >= radix)
                {
                    code = ParseCode.OutOfRadix;

                    goto Return;
                }

                if (value < (-long.MaxValue - sign + digital) / radix)
                {
                    code = ParseCode.OutOfRange;

                    goto Return;
                }

                value = value * radix - digital;

                ++offset;
            }

            Return:

            if (sign == 0)
            {
                value = -value;
            }

            return (code, offset, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public (ParseCode code, int length, ulong value) ParseUInt64(char* chars, int length)
        {
            if (radix == DecimalRadix)
            {
                return DecimalParseUInt64(chars, length);
            }

            if (length <= 0)
            {
                return (ParseCode.Empty, 0, 0);
            }

            var offset = 0;

            switch (chars[offset])
            {
                case PositiveSign:
                    ++offset;
                    break;
            }

            while (offset < length && chars[offset] == DigitalsZeroValue)
            {
                ++offset;
            }

            var value = 0UL;
            var code = ParseCode.Success;

            for (int right = Math.Min((uInt64NumbersLength - 1) + offset, length); offset < right; ++offset)
            {
                var digital = ToRadix(chars[offset]);

                if (digital >= radix)
                {
                    code = ParseCode.OutOfRadix;

                    goto Return;
                }

                value = value * radix + digital;
            }

            if (offset < length)
            {
                var digital = ToRadix(chars[offset]);

                if (digital >= radix)
                {
                    code = ParseCode.OutOfRadix;

                    goto Return;
                }

                if (value > (ulong.MaxValue - digital) / radix)
                {
                    code = ParseCode.OutOfRange;

                    goto Return;
                }

                value = value * radix + digital;

                ++offset;
            }

            Return:

            return (code, offset, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chars"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static (ParseCode code, int length, ulong value) DecimalParseUInt64(char* chars, int length)
        {
            const byte radix = DecimalRadix;

            if (length <= 0)
            {
                return (ParseCode.Empty, 0, 0);
            }

            var offset = 0;

            switch (chars[offset])
            {
                case PositiveSign:
                    ++offset;
                    break;
            }

            while (offset < length && chars[offset] == DigitalsZeroValue)
            {
                ++offset;
            }

            var value = 0UL;
            var code = ParseCode.Success;

            for (int right = Math.Min((DecimalUInt64NumbersLength - 1) + offset, length); offset < right; ++offset)
            {
                var digital = (uint)(chars[offset] - DigitalsZeroValue);

                if (digital >= radix)
                {
                    code = ParseCode.OutOfRadix;

                    goto Return;
                }

                value = value * radix + digital;
            }

            if (offset < length)
            {
                var digital = (uint)(chars[offset] - DigitalsZeroValue);

                if (digital >= radix)
                {
                    code = ParseCode.OutOfRadix;

                    goto Return;
                }

                if (value > (ulong.MaxValue - digital) / radix)
                {
                    code = ParseCode.OutOfRange;

                    goto Return;
                }

                value = value * radix + digital;

                ++offset;
            }

            Return:

            return (code, offset, value);
        }

        /// <summary>
        /// 尝试从字符串开始位置解析一个字节正整数值。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="value">字节正整数空间</param>
        /// <returns>解析成功则返回解析的长度，失败则返回 0</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public (ParseCode code, int length, int written) ParseBigInteger(char* chars, int length, uint* value)
        {
            if (length <= 0)
            {
                return (ParseCode.Empty, 0, 0);
            }

            var code = ParseCode.Success;
            var written = 0;
            var offset = chars;
            var num = 0U;
            var numbers = uInt32Numbers;
            var basen = divisorLength;
            int count = basen;
            var radix = this.radix;


            int i = length / basen;

            Loop:

            if (i == 0)
            {
                count = length % basen;
            }

            for (var end = offset + count; offset < end; ++offset)
            {
                var digit = ToRadix(*offset);

                if (digit >= radix)
                {
                    goto OutOfRadix;
                }

                num = num * radix + digit;
            }

            MultAndAdd:

            uint carry;

            if (radix == DecimalRadix && count == DecimalDivisorLength)
            {
                written = Mult(value, written, DecimalDivisor, out carry);
            }
            else
            {
                written = Mult(value, written, numbers[count], out carry);
            }


            if (carry != 0)
            {
                value[written] = carry;

                ++written;
            }

            written = Add(value, written, num, out carry);

            if (carry != 0)
            {
                value[written] = carry;

                ++written;
            }

            if (i != 0)
            {
                --i;

                num = 0;

                goto Loop;
            }

            return (code, (int)(offset - chars), written);


            OutOfRadix:

            code = ParseCode.OutOfRadix;

            goto ErrorReturn;

            ErrorReturn:

            count = ((int)(offset - chars)) % basen;

            i = 0;

            goto MultAndAdd;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static bool DecimalTryParse(char* chars, int count, out int value)
        {
            unchecked
            {
                value = 0;

                for (int i = 0; i < count; i++)
                {
                    var digit = (uint)(chars[i] - DigitalsZeroValue);

                    if (digit >= DecimalRadix)
                    {
                        return false;
                    }

                    value = value * DecimalRadix + (int)digit;
                }

                return true;
            }
        }
    }
}