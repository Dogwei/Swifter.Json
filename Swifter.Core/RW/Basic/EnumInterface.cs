using Swifter.Tools;
using System;

namespace Swifter.RW
{
    internal sealed class EnumInterface<T> : IValueInterface<T> where T : struct, Enum
    {
        /// <summary>
        /// 枚举的 TypeCode。
        /// </summary>
        public static readonly TypeCode TypeCode = Type.GetTypeCode(typeof(T));

        /// <summary>
        /// 表示此枚举是否是标识符。
        /// </summary>
        public static readonly bool IsFlags = TypeHelper.GetDefinedAttributes<FlagsAttribute>(typeof(T), false)?.Length >= 1;

        /// <summary>
        /// 枚举值和名称的集合。
        /// </summary>
        public static readonly (ulong Value, string Name)[] Items;

        static EnumInterface()
        {
            var values = (T[])Enum.GetValues(typeof(T));

            Items = new (ulong, string)[values.Length];

            for (int i = 0; i < values.Length; i++)
            {
                Items[i] = (EnumHelper.AsUInt64(values[i]), Enum.GetName(typeof(T), values[i]));
            }
        }

        public T ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadEnum<T>();
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            valueWriter.WriteEnum(value);
        }
    }
}