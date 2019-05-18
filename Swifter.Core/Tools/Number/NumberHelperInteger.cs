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
            var l = threeDigitalsLength;

            if (l == 1000)
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
                            if (length > 19 && value >= nums[19])
                                goto L20;
                            else if (length > 18 && value >= nums[18])
                                goto L19;
                            else
                                goto L18;
                        else if (length > 16 && value >= nums[16])
                            goto L17;
                        else
                            goto L16;
                    else if (length > 12 && value >= nums[12])
                        if (length > 14 && value >= nums[14])
                            goto L15;
                        else if (length > 13 && value >= nums[13])
                            goto L14;
                        else
                            goto L13;
                    else if (length > 11 && value >= nums[11])
                        goto L12;
                    else
                        goto L11;
                else if (value >= nums[7])
                    if (value >= nums[9])
                        goto L10;
                    else if (value >= nums[8])
                        goto L9;
                    else
                        goto L8;
                else if (value >= nums[6])
                    goto L7;
                else
                    goto L6;
            }
            else if (value >= nums[2])
                if (value >= nums[4])
                    goto L5;
                else if (value >= nums[3])
                    goto L4;
                else
                    goto L3;
            else if (value >= nums[1])
                goto L2;
            else
                goto L1;

            L20:

            var s = value / nums[18];

            if (length > 20 && value >= nums[20])
            {
                var r = ToString(s, chars);

                AppendD18(chars + r, value - s * nums[18]);

                return r + 18;
            }

            AppendD2(chars, s);
            AppendD18(chars+2, value - s * nums[18]);
            return 20;

        L19:
            s = value / nums[18];
            AppendD1(chars, s);
            AppendD18(chars+1, value - s * nums[18]);
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
            s = value / l;
            AppendD2(chars, s);
            AppendD3(chars + 2, value - s * l);
            return 5;

        L4:
            s = value / l;
            AppendD1(chars, s);
            AppendD3(chars + 1, value - s * l);
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
            var l = threeDigitalsLength;

            if (l == 1000)
            {
                ToDecimalString(value, length, chars);

                return;
            }

            var n = uInt64Numbers;

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

            var s = value / n[18];

            if (length > 20)
            {
                length -= 18;

                ToString(s, length, chars);

                AppendD18(chars + length, value - s * n[18]);

                return;
            }

            AppendD2(chars, s);
            AppendD18(chars + 2, value - s * n[18]);
            return;

        L19:
            s = value / n[18];
            AppendD1(chars, s);
            AppendD18(chars + 1, value - s * n[18]);
            return;

        L18:
            AppendD18(chars, value);
            return;

        L17:
            s = value / n[15];
            AppendD2(chars, s);
            AppendD15(chars + 2, value - s * n[15]);
            return;

        L16:
            s = value / n[15];
            AppendD1(chars, s);
            AppendD15(chars + 1, value - s * n[15]);
            return;

        L15:
            AppendD15(chars, value);
            return;

        L14:
            s = value / n[12];
            AppendD2(chars, s);
            AppendD12(chars + 2, value - s * n[12]);
            return;

        L13:
            s = value / n[12];
            AppendD1(chars, s);
            AppendD12(chars + 1, value - s * n[12]);
            return;

        L12:
            AppendD12(chars, value);
            return;

        L11:
            s = value / n[9];
            AppendD2(chars, s);
            AppendD9(chars + 2, value - s * n[9]);
            return;

        L10:
            s = value / n[9];
            AppendD1(chars, s);
            AppendD9(chars + 1, value - s * n[9]);
            return;

        L9:
            AppendD9(chars, value);
            return;

        L8:
            s = value / n[6];
            AppendD2(chars, s);
            AppendD6(chars + 2, value - s * n[6]);
            return;

        L7:
            s = value / n[6];
            AppendD1(chars, s);
            AppendD6(chars + 1, value - s * n[6]);
            return;

        L6:
            AppendD6(chars, value);
            return;

        L5:
            s = value / l;
            AppendD2(chars, s);
            AppendD3(chars + 2, value - s * l);
            return;

        L4:
            s = value / l;
            AppendD1(chars, s);
            AppendD3(chars + 1, value - s * l);
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
        /// 将 UInt64 值转换为字符串表现形式。
        /// </summary>
        /// <param name="value">UInt64 值</param>
        /// <returns>返回一个 String 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string ToString(ulong value)
        {
            var len = GetLength(value);

            var str = new string('\0', len);

            fixed (char* pStr = str)
            {
                ToString(value, len, pStr);
            }

            return str;
        }

        /// <summary>
        /// 将 Int64 值转换为字符串表现形式。
        /// </summary>
        /// <param name="value">Int64 值</param>
        /// <returns>返回一个 String 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string ToString(long value)
        {
            if (value >= 0)
            {
                return ToString((ulong)value);
            }

            var uValue = (ulong)-value;

            var len = GetLength(uValue);

            var str = new string('\0', len + 1);

            fixed (char* pStr = str)
            {
                pStr[0] = NegativeSign;

                ToString(uValue, len, pStr + 1);
            }

            return str;
        }

        /// <summary>
        /// 将字节正整数转换为字符串表现形式。
        /// </summary>
        /// <param name="value">字节正整数</param>
        /// <param name="length">字节正整数长度</param>
        /// <returns>返回一个 String 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string ToString(uint* value, int length)
        {
            var chars = stackalloc char[(uInt32NumbersLength * length) + 1];

            var writeCount = ToString(value, length, chars);

            return new string(chars, 0, writeCount);
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
            var divisor = this.divisor;

            if (divisor == 1000000000)
            {
                return DirectOperateToDecimalString(value, length, chars);
            }
            
            int l;

            l = Div(value, length, divisor, out var r);

            if (l == 0)
            {
                return ToString((ulong)r, chars);
            }

            int s;

            if (l == 1)
            {
                s = ToString(*value, chars);
            }
            else
            {
                s = DirectOperateToString(value, l, chars);
            }

            ToString(r, divisorLength, chars + s);

            return s + divisorLength;
        }





        /// <summary>
        /// 尝试从字符串开始位置解析一个 Int64 值。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="value">返回一个 Int64 值</param>
        /// <param name="exception">当解析到错误时是否引发异常，异常不代表解析失败。</param>
        /// <returns>解析成功则返回解析的长度，失败则返回 0</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int TryParse(char* chars, int length, out long value, bool exception = false)
        {
            var r = radix;

            if (r == 10)
            {
                return DecimalTryParse(chars, length, out value, exception);
            }

            var v = 0L; // Value
            var c = chars;
            var u = int64NumbersLength;
            var n = false; // IsNegative

            if (length <= 0)
            {
                goto EmptyLength;
            }

            switch (*c)
            {
                case NegativeSign:
                    n = true;
                    goto Sign;
                case PositiveSign:
                    goto Sign;
            }

        IgnoreZero:

            while (*c == DigitalsZeroValue && length != 1)
            {
                ++c;
                --length;
            }

            var e = c + Math.Min(u, length);

        Loop:
            
            var d = ToRadix(*c);

            if (d >= r)
            {
                goto OutOfRadix;
            }

            ++c;

            if (c == e)
            {
                if (length >= u && v < (n ? (Int64MinValue + d) / r : (NegativeInt64MaxValue + d) / r))
                {
                    --c;

                    goto OutOfRange;
                }

                v = v * r - d;

                if (length > u)
                {
                    goto OutOfRange;
                }
            }
            else
            {
                v = v * r - d;

                goto Loop;
            }

        Return:

            value = n ? v : -v;

            return (int)(c - chars);

        OutOfRadix:
            if (exception) ThreadException = new FormatException("Digit out of radix.");
            goto Return;

        OutOfRange:
            if (exception) ThreadException = new OverflowException("value out of range.");
            goto Return;

        Sign:

            if (length != 1)
            {
                ++c;
                --length;

                goto IgnoreZero;
            }

        EmptyLength:
            if (exception) ThreadException = new ArgumentException("Length cna't be less than 1");
            goto Return;
        }
        
        /// <summary>
        /// 尝试从字符串开始位置解析一个 UInt64 值。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="value">返回一个 UInt64 值</param>
        /// <param name="exception">当解析到错误时是否引发异常，异常不代表解析失败。</param>
        /// <returns>解析成功则返回解析的长度，失败则返回 0</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int TryParse(char* chars, int length, out ulong value, bool exception = false)
        {
            var r = radix;

            if (r == 10)
            {
                return DecimalTryParse(chars, length, out value, exception);
            }

            var v = 0UL; // Value
            var c = chars;
            var u = uInt64NumbersLength;

            if (length <= 0)
            {
                goto EmptyLength;
            }

            if (*c == PositiveSign)
            {
                if (length == 1)
                {
                    goto EmptyLength;
                }

                ++c;
                --length;
            }

            while (*c == DigitalsZeroValue && length != 1)
            {
                ++c;
                --length;
            }

            var e = c + Math.Min(u, length);

        Loop:
            var d = ToRadix(*c);

            if (d >= r)
            {
                goto OutOfRadix;
            }

            ++c;

            if (c == e)
            {
                if (length >= u && v > (UInt64MaxValue - d) / r)
                {
                    --c;

                    goto OutOfRange;
                }

                v = v * r + d;

                if (length > u)
                {
                    goto OutOfRange;
                }
            }
            else
            {
                v = v * r + d;

                goto Loop;
            }


        Return:

            value = v;

            return (int)(c - chars);

        OutOfRadix:
            if (exception) ThreadException = new FormatException("Digit out of radix.");
            goto Return;

        OutOfRange:
            if (exception) ThreadException = new OverflowException("value out of range.");
            goto Return;

        EmptyLength:
            if (exception) ThreadException = new ArgumentException("Length cna't be less than 1");
            goto Return;
        }

        /// <summary>
        /// 尝试从字符串开始位置解析一个字节正整数值。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="value">字节正整数空间</param>
        /// <param name="writeCount">返回写入长度</param>
        /// <param name="exception">当解析到错误时是否引发异常，异常不代表解析失败。</param>
        /// <returns>解析成功则返回解析的长度，失败则返回 0</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int TryParse(char* chars, int length, uint* value, out int writeCount, bool exception = false)
        {
            int writeLength = 0;
            var current = chars;
            var num = 0U;
            var numbers = uInt32Numbers;
            var baseCount = divisorLength;
            int count = baseCount;
            var r = radix;

            if (length <= 0)
            {
                goto EmptyLength;
            }

            int i = length / baseCount;

        Loop:

            if (i == 0)
            {
                count = length % baseCount;
            }

            for (var end = current + count; current < end; ++current)
            {
                var digit = ToRadix(*current);

                if (digit >= r)
                {
                    goto OutOfRadix;
                }

                num = num * r + digit;
            }

        MultAndAdd:

            uint carry;

            if (r == 10 && count == 9)
            {
                writeLength = Mult(value, writeLength, DecimalBaseDivisor, out carry);
            }
            else
            {
                writeLength = Mult(value, writeLength, numbers[count], out carry);
            }


            if (carry != 0)
            {
                value[writeLength] = carry;

                ++writeLength;
            }

            writeLength = Add(value, writeLength, num, out carry);

            if (carry != 0)
            {
                value[writeLength] = carry;

                ++writeLength;
            }

            if (i != 0)
            {
                --i;

                num = 0;

                goto Loop;
            }

            writeCount = writeLength;

            return (int)(current - chars);


        OutOfRadix:
            if (exception) ThreadException = new FormatException("Digit out of radix.");
            goto ErrorReturn;

        EmptyLength:
            if (exception) ThreadException = new ArgumentException("Length cna't be less than 1");
            goto ErrorReturn;


        ErrorReturn:

            count = ((int)(current - chars)) % baseCount;

            i = 0;

            goto MultAndAdd;
        }

        /// <summary>
        /// 尝试从字符串开始位置解析一个 Int32 值。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="value">返回一个 Int32 值</param>
        /// <param name="exception">当解析到错误时是否引发异常，异常不代表解析失败。</param>
        /// <returns>解析成功则返回解析的长度，失败则返回 0</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int TryParse(char* chars, int length, out int value, bool exception = false)
        {
            int r = TryParse(chars, length, out long int64Value, exception);

            if (int64Value > int.MaxValue || int64Value < int.MinValue)
            {
                if(exception) ThreadException = new OverflowException("Value out of Int32 range.");
            }

            value = (int)int64Value;

            return r;
        }

        /// <summary>
        /// 尝试将字符串转换为 Int64 值。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="value">返回一个 Int64 值</param>
        /// <returns>返回转换是否成功</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryParse(string text, out long value)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                return TryParse(chars, length, out value) == length;
            }
        }




        /// <summary>
        /// 尝试将字符串转换为 UInt64 值。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="value">返回一个 UInt64 值</param>
        /// <returns>返回转换是否成功</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryParse(string text, out ulong value)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                return TryParse(chars, length, out value) == length;
            }
        }

        /// <summary>
        /// 尝试将字符串转换为字节正整数值。
        /// </summary>
        /// <param name="text"></param>
        /// <param name="value">字节正整数</param>
        /// <param name="length">返回写入长度</param>
        /// <returns>返回转换是否成功</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool TryParse(string text, uint* value, out int length)
        {
            fixed (char* chars = text)
            {
                return TryParse(chars, text.Length, value, out length, true) == text.Length;
            }
        }


        /// <summary>
        /// 将字符串转换为 Int64 值。失败将引发异常。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns>返回一个 Int64 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ParseInt64(string text)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                if (TryParse(chars, length, out long value, true) == length)
                {
                    return value;
                }

                throw ThreadException;
            }
        }

        /// <summary>
        /// 将字符串转换为 UInt64 值。失败将引发异常。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <returns>返回一个 UInt64 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong ParseUInt64(string text)
        {
            var length = text.Length;

            fixed (char* chars = text)
            {
                if (TryParse(chars, length, out ulong value, true) == length)
                {
                    return value;
                }

                throw ThreadException;
            }
        }

        /// <summary>
        /// 将字符串转换为字节正整数值。失败将引发异常。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="value">字节正整数</param>
        /// <returns>返回写入长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ParseBigInteger(string text, uint* value)
        {
            if (TryParse(text, value, out int r))
            {
                return r;
            }

            throw ThreadException;
        }
    }
}
