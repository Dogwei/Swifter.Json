using System.Runtime.CompilerServices;

using static Swifter.Tools.NumberHelper;

namespace Swifter.Tools
{
    /// <summary>
    /// 表示一个字符串数字信息。
    /// </summary>
    public unsafe ref struct NumberInfo
    {
        internal char* chars;

        internal bool isNegative;

        internal int integerBegin;
        internal int integerCount;
        internal int integerSplitCount;

        internal int fractionalBegin;
        internal int fractionalCount;
        internal int fractionalSplitCount;

        internal bool exponentIsNegative;

        internal int exponentBegin;
        internal int exponentCount;
        internal int exponentSplitCount;

        internal byte max_digit;
        internal byte max_radix;

        internal int end;

        /// <summary>
        /// 获取该字符串的指针。
        /// </summary>
        public char* Chars => chars;

        /// <summary>
        /// 获取该字符串是否是一个数字。
        /// </summary>
        public bool IsNumber =>
            (integerBegin != -1 && integerCount != 0) &&
            (fractionalBegin == -1 || fractionalCount != 0) &&
            (exponentBegin == -1 || exponentCount != 0);

        /// <summary>
        /// 获取该数字是否为负数。
        /// </summary>
        public bool IsNegative => isNegative;

        /// <summary>
        /// 获取此数字的整数部分的开始位置。
        /// </summary>
        public int IntegerBegin => integerBegin;

        /// <summary>
        /// 获取此数字的整数部分数字的数量。
        /// </summary>
        public int IntegerCount => integerCount;

        /// <summary>
        /// 获取此数字的整数部分长度。
        /// </summary>
        public int IntegerLength => integerCount + integerSplitCount;

        /// <summary>
        /// 获取此数字是否存在小数。
        /// </summary>
        public bool HaveFractional => fractionalBegin != -1 && fractionalCount != 0;

        /// <summary>
        /// 获取此数字的小数部分的开始位置。
        /// </summary>
        public int FractionalBegin => integerBegin;

        /// <summary>
        /// 获取此数字的小数部分数字的数量。
        /// </summary>
        public int FractionalCount => fractionalCount;

        /// <summary>
        /// 获取此数字的小数部分长度。
        /// </summary>
        public int FractionalLength => fractionalCount + fractionalSplitCount;

        /// <summary>
        /// 获取此数字是否存在指数。
        /// </summary>
        public bool HaveExponent => exponentBegin != -1 && exponentCount != 0;

        /// <summary>
        /// 获取此数字的指数是否为负数。
        /// </summary>
        public bool ExponentIsNegative => exponentIsNegative;

        /// <summary>
        /// 获取此数字的指数部分的开始位置（不含符号位）。
        /// </summary>
        public int ExponentlBegin => integerBegin;

        /// <summary>
        /// 获取此数字的指数部分数字的数量。
        /// </summary>
        public int ExponentCount => exponentCount;

        /// <summary>
        /// 获取此数字的指数部分长度。
        /// </summary>
        public int ExponentLength => exponentCount + exponentSplitCount;

        /// <summary>
        /// 获取此数字出现的最大数字。
        /// </summary>
        public byte MaxDigit => max_digit;

        /// <summary>
        /// 获取此数字允许的最大进制数。
        /// </summary>
        public byte MaxRadix => MaxRadix;

        /// <summary>
        /// 获取是否可以为十进制。
        /// </summary>
        public bool IsDecimal => max_digit < DecimalRadix && max_radix >= DecimalRadix;

        /// <summary>
        /// 获取是否可以为二进制。
        /// </summary>
        public bool IsBinary => max_digit < BinaryRadix && max_radix >= BinaryRadix;

        /// <summary>
        /// 获取是否可以为十六进制。
        /// </summary>
        public bool IsHex => max_digit < HexRadix && max_radix >= HexRadix;

        /// <summary>
        /// 获取是否可以为八进制数字。
        /// </summary>
        public bool IsOctal => max_digit < OctalRadix && max_radix >= OctalRadix;

        /// <summary>
        /// 获取此数字在字符串中的结束位置，数字内容不包含此位置。
        /// </summary>
        public int End => end;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void CopyTo(char* destination)
        {
            if (IsNumber)
            {
                if (IsNegative)
                {
                    *destination = NegativeSign; ++destination;
                }

                Underlying.CopyBlock(
                    destination,
                    chars + integerBegin,
                    (uint)(IntegerLength * sizeof(char))
                    );

                destination += IntegerLength;

                if (HaveFractional)
                {
                    *destination = DotSign; ++destination;

                    Underlying.CopyBlock(
                        destination,
                        chars + fractionalBegin,
                        (uint)(FractionalLength * sizeof(char))
                        );

                    destination += FractionalLength;
                }

                if (HaveExponent)
                {
                    *destination = ExponentSign; ++destination;

                    *destination = exponentIsNegative ? NegativeSign : PositiveSign; ++destination;

                    Underlying.CopyBlock(
                        destination,
                        chars + exponentBegin,
                        (uint)(ExponentLength * sizeof(char))
                        );

                    destination += ExponentLength;
                }
            }
            else
            {
                fixed (char* pNaNSign = NaNSign)
                {
                    Underlying.CopyBlock(
                        destination,
                        pNaNSign,
                        (uint)(NaNSign.Length * sizeof(char))
                        );
                }
            }
        }

        /// <summary>
        /// 获取此数字的字符串长度。
        /// </summary>
        public int StringLength
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                if (IsNumber)
                {
                    return
                        IntegerLength
                        + (IsNegative ? 1 : 0)
                        + (HaveFractional ? 1/*length of '.'*/ + FractionalLength : 0)
                        + (HaveExponent ? 1/*length of 'e'*/ + 1 /*length of exponent sign*/ + ExponentLength : 0)
                        ;
                }

                return NaNSign.Length;
            }
        }

        /// <summary>
        /// 获取此数字的字符串表现形式。
        /// </summary>
        /// <returns>返回一个 string 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override string ToString()
        {
            if (IsNumber)
            {
                var result = StringHelper.MakeString(StringLength);

                fixed (char* chars = result)
                {
                    CopyTo(chars);
                }

                return result;
            }
            else
            {
                return NaNSign;
            }
        }

        /// <summary>
        /// 转换为 Double。失败将引发异常。
        /// </summary>
        /// <returns>返回一个 Double</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public double ToDouble(byte radix)
        {
            return GetOrCreateInstance(radix).ToDouble(this);
        }

        /// <summary>
        /// 转换为 Double。失败将引发异常。
        /// </summary>
        /// <returns>返回一个 Double</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public decimal ToDecimal()
        {
            return NumberHelper.ToDecimal(this);
        }

        /// <summary>
        /// 转换为 UInt64。失败将引发异常。
        /// </summary>
        /// <returns>返回一个 Double</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong ToUInt64(byte radix)
        {
            return GetOrCreateInstance(radix).ToUInt64(this);
        }

        /// <summary>
        /// 转换为 Int64。失败将引发异常。
        /// </summary>
        /// <returns>返回一个 Double</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ToInt64(byte radix)
        {
            return GetOrCreateInstance(radix).ToInt64(this);
        }

        /// <summary>
        /// 尝试获取常用进制数。
        /// </summary>
        /// <param name="radix"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool IsCommonRadix(out byte radix)
        {
            radix = 0;

            if (max_radix > max_digit && max_digit < HexRadix && max_radix >= BinaryRadix)
            {
                if (max_radix >= DecimalRadix && max_digit < DecimalRadix) radix = DecimalRadix;
                else if (max_radix >= HexRadix && max_digit < HexRadix) radix = HexRadix;
                else if (max_radix >= BinaryRadix && max_digit < BinaryRadix) radix = BinaryRadix;
                else if (max_radix >= OctalRadix && max_digit < OctalRadix) radix = OctalRadix;
            }

            return radix != 0;
        }
    }
}