using Swifter.RW;
using System;
using System.Runtime.CompilerServices;

using static Swifter.Tools.TypeHelper;

namespace Swifter.Tools
{
    /// <summary>
    /// 
    /// </summary>
    public static class EnumHelper
    {
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

            var spliter = str.Split(EnumParsingSeperator);

            Continue:

            while (spliter.MoveNext())
            {
                var item = spliter.Current;

                item = StringHelper.Trim(item.Pointer, item.Length);

                foreach (var (bits, name) in EnumInterface<T>.Items)
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

        /// <summary>
        /// 获取指定枚举值的名称。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">枚举值</param>
        /// <returns>返回枚举名称，没有名称则返回 null</returns>
        public static string GetEnumName<T>(T value) where T : struct, Enum
        {
            ulong val = AsUInt64(value);

            foreach (var (Value, Name) in EnumInterface<T>.Items)
            {
                if (Value == val)
                {
                    return Name;
                }
            }

            return null;
        }

        /// <summary>
        /// 枚举分隔符。
        /// </summary>
        public const string EnumSeperator = ", ";

        /// <summary>
        /// 解析枚举时的分隔符。
        /// </summary>
        const char EnumParsingSeperator = ',';

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
            var result = AsUInt64(value);

            var items = EnumInterface<T>.Items;

            int offset = 0;

            var firstTime = true;

            // 得出格式化所需的字符串长度。
            for (int i = items.Length - 1; i >= 0; i--)
            {
                var (val, name) = items[i];

                if ((val & result) == val && val != 0)
                {
                    if (!firstTime)
                    {
                        offset += EnumSeperator.Length;
                    }

                    offset += name.Length;

                    result -= val;

                    if (result == 0)
                    {
                        break;
                    }

                    firstTime = false;
                }
            }

            if (offset <= length)
            {
                result = AsUInt64(value);

                charsWritten = offset;

                firstTime = true;

                for (int i = items.Length - 1; i >= 0; i--)
                {
                    var (val, name) = items[i];

                    if ((val & result) == val && val != 0)
                    {
                        if (!firstTime)
                        {
                            Insert(EnumSeperator);
                        }

                        Insert(name);

                        result -= val;

                        if (result == 0)
                        {
                            break;
                        }

                        firstTime = false;
                    }
                }

                return AsEnum<T>(result);

                void Insert(string str)
                {
                    offset -= str.Length;

                    Underlying.CopyBlock(
                        ref Underlying.As<char, byte>(ref chars[offset]),
                        ref Underlying.As<char, byte>(ref StringHelper.GetRawStringData(str)),
                        (uint)(str.Length * sizeof(char))
                        );
                }
            }
            else
            {
                charsWritten = 0;

                return value;
            }
        }

        /// <summary>
        /// 获取枚举类型是否有 Flags 特性标识。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>返回一个布尔值</returns>
        public static bool IsFlagsEnum<T>() where T : struct, Enum => EnumInterface<T>.IsFlags;

        /// <summary>
        /// 获取枚举类型的 TypeCode 值。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>返回一个 TypeCode 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TypeCode GetEnumTypeCode<T>() where T : struct, Enum => EnumInterface<T>.TypeCode;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ulong AsUInt64<T>(T value) where T : struct, Enum
        {
            return (GetEnumTypeCode<T>()) switch
            {
                TypeCode.SByte => (ulong)As<T, sbyte>(value),
                TypeCode.Byte => (ulong)As<T, byte>(value),
                TypeCode.Int16 => (ulong)As<T, short>(value),
                TypeCode.UInt16 => (ulong)As<T, ushort>(value),
                TypeCode.Int32 => (ulong)As<T, int>(value),
                TypeCode.UInt32 => (ulong)As<T, uint>(value),
                TypeCode.Int64 => (ulong)As<T, long>(value),
                _ => (ulong)As<T, ulong>(value),
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T AsEnum<T>(ulong value) where T : struct, Enum
        {
            return (GetEnumTypeCode<T>()) switch
            {
                TypeCode.SByte => As<sbyte, T>((sbyte)value),
                TypeCode.Byte => As<byte, T>((byte)value),
                TypeCode.Int16 => As<short, T>((short)value),
                TypeCode.UInt16 => As<ushort, T>((ushort)value),
                TypeCode.Int32 => As<int, T>((int)value),
                TypeCode.UInt32 => As<uint, T>((uint)value),
                TypeCode.Int64 => As<long, T>((long)value),
                _ => As<ulong, T>((ulong)value),
            };
        }

    }
}
