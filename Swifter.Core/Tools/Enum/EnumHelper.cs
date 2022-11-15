using InlineIL;
using Swifter.RW;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供枚举和位域的工具方法。
    /// </summary>
    public static class EnumHelper
    {
        /// <summary>
        /// 枚举分隔符。
        /// </summary>
        public const string EnumSeperator = ", ";

        /// <summary>
        /// 解析枚举时的分隔符。
        /// </summary>
        const char EnumParsingSeperator = ',';

        /// <summary>
        /// 尝试将字符串解析为枚举值。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="str">字符串</param>
        /// <param name="value">返回枚举值</param>
        /// <returns>返回是否解析成功</returns>
        public static unsafe bool TryParseEnum<T>(Ps<char> str, out T value) where T : struct, Enum
        {
            const char NumberMin = '0';
            const char NumberMax = '9';

            if (str.Length < 0)
            {
                goto False;
            }

            switch (str.Pointer[0])
            {
                case NumberHelper.NegativeSign:
                case NumberHelper.PositiveSign:
                case var digit when digit >= NumberMin && digit <= NumberMax:
                    goto Number;
            }

            ulong val = 0;

            var splition = str.Split(EnumParsingSeperator);

        Continue:

            while (splition.MoveNext())
            {
                var item = splition.Current;

                item = StringHelper.Trim(item.Pointer, item.Length);

                foreach (var (bits, name) in EnumInfo<T>.Items)
                {
                    if (StringHelper.EqualsWithIgnoreCase(item, name))
                    {
                        val |= bits;

                        goto Continue;
                    }
                }

                if (val != 0)
                    goto False;
                else
                    goto Number;
            }

            value = AsEnum<T>(val);

            return true;

        Number:

            var numberInfo = NumberHelper.GetNumberInfo(str.Pointer, str.Length, 16);

            if (numberInfo.IsNumber && !numberInfo.HaveFractional && !numberInfo.HaveExponent && numberInfo.End == str.Length && numberInfo.IsCommonRadix(out var radix))
            {
                value = numberInfo.IsNegative
                    ? AsEnum<T>((ulong)numberInfo.ToInt64(radix))
                    : AsEnum<T>(numberInfo.ToUInt64(radix));

                return true;
            }

        False:

            value = default;

            return false;
        }

        /// <inheritdoc cref="TryParseEnum{T}(Ps{char}, out T)"/>
        public static unsafe bool TryParseEnum<T>(string str, out T value) where T : struct, Enum
        {
            fixed (char* pStr = str) return TryParseEnum(new Ps<char>(pStr, str.Length), out value);
        }

        /// <summary>
        /// 获取指定枚举值的名称。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">枚举值</param>
        /// <returns>返回枚举名称，没有名称则返回 null</returns>
        public static string? GetEnumName<T>(this T value) where T : struct, Enum
        {
            ulong val = AsUInt64(value);

            foreach (var (Value, Name) in EnumInfo<T>.Items)
            {
                if (Value == val)
                {
                    return Name;
                }
            }

            return null;
        }

        /// <summary>
        /// 对枚举进行标识符格式化。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">枚举值</param>
        /// <param name="chars">字符串</param>
        /// <param name="length">字符串长度</param>
        /// <param name="charsWritten">写入的字符串长度</param>
        /// <returns>返回剩余的枚举值</returns>
        public static unsafe T FormatEnumFlags<T>(T value, char* chars, int length, out int charsWritten) where T : struct, Enum
        {
            int offset = 0;
            bool isInsert = false;

            InternalFormatEnumFlags();

            if (offset > length)
            {
                charsWritten = 0;

                return value;
            }

            charsWritten = offset;
            isInsert = true;

            return InternalFormatEnumFlags();

            T InternalFormatEnumFlags()
            {
                var result = AsUInt64(value);

                var items = EnumInfo<T>.Items;

                var isFirst = true;

                for (int i = items.Length - 1; i >= 0; i--)
                {
                    var (val, name) = items[i];

                    if ((val & result) == val && val != 0)
                    {
                        if (!isFirst)
                        {
                            Insert(EnumSeperator);
                        }

                        Insert(name);

                        result -= val;

                        if (result == 0)
                        {
                            break;
                        }

                        isFirst = false;
                    }
                }

                return AsEnum<T>(result);
            }

            void Insert(string str)
            {
                if (isInsert)
                {
                    offset -= str.Length;

                    Unsafe.CopyBlock(
                        ref Unsafe.As<char, byte>(ref chars[offset]),
                        ref Unsafe.As<char, byte>(ref StringHelper.GetRawStringData(str)),
                        (uint)(str.Length * sizeof(char))
                        );
                }
                else
                {
                    offset += str.Length;
                }
            }
        }

        /// <summary>
        /// 获取枚举类型是否有 Flags 特性标识。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>返回一个布尔值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IsFlagsEnum<T>() where T : struct, Enum => EnumInfo<T>.IsFlags;

        /// <summary>
        /// 获取枚举类型的 <see cref="TypeCode"/> 值。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TypeCode GetEnumTypeCode<T>() where T : struct, Enum => EnumInfo<T>.TypeCode;

        /// <summary>
        /// 将枚举值转换为 <see cref="UInt64"/> 值。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ulong AsUInt64<T>(T value) where T : struct, Enum
        {
            switch (Unsafe.SizeOf<T>())
            {
                case 1: return Unsafe.As<T, byte>(ref value);
                case 2: return Unsafe.As<T, ushort>(ref value);
                case 4: return Unsafe.As<T, uint>(ref value);
                default: return Unsafe.As<T, ulong>(ref value);
            }
        }

        /// <summary>
        /// 将 <see cref="UInt64"/> 值转换为指定枚举类型值。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T AsEnum<T>(ulong value) where T : struct, Enum
        {
            switch (Unsafe.SizeOf<T>())
            {
                case 1: return TypeHelper.As<byte, T>((byte)value);
                case 2: return TypeHelper.As<ushort, T>((ushort)value);
                case 4: return TypeHelper.As<uint, T>((uint)value);
                default: return TypeHelper.As<ulong, T>((ulong)value);
            }
        }

        /// <summary>
        /// 判断位域枚举值 <paramref name="left"/> 是否包含 <paramref name="right"/> 的任意位。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool On<T>(this T left, T right)
            where T : struct, Enum
        {
            IL.Push(left);
            IL.Push(right);

            IL.Emit.And();
            IL.Emit.Ldc_I4_0();
            IL.Emit.Cgt_Un();

            return IL.Return<bool>();
        }
    }
}