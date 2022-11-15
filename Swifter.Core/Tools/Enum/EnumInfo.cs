using System;

namespace Swifter.Tools
{
    /// <summary>
    /// 枚举信息静态类。
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    internal static class EnumInfo<T> where T : struct, Enum
    {
        /// <summary>
        /// 枚举的 TypeCode。
        /// </summary>
        public static readonly TypeCode TypeCode = Type.GetTypeCode(typeof(T));

        /// <summary>
        /// 表示此枚举是否是标识符。
        /// </summary>
        public static readonly bool IsFlags = typeof(T).IsDefined(typeof(FlagsAttribute), false);

        /// <summary>
        /// 枚举值和名称的集合。
        /// </summary>
        public static readonly (ulong Value, string Name)[] Items;

        static EnumInfo()
        {
            var values = (T[])Enum.GetValues(typeof(T));

            Items = new (ulong, string)[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                Items[i] = (EnumHelper.AsUInt64(values[i]), Enum.GetName(typeof(T), values[i])!);
            }
        }
    }
}