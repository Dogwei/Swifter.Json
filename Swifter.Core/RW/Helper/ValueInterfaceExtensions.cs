using System;
using System.Globalization;

namespace Swifter.RW
{
    /// <summary>
    /// 提供 ValueInterface 的扩展方法。
    /// </summary>
    public static class ValueInterfaceExtensions
    {
        public static void SetValueInterface<T>(this ITargetableValueRWSource targetable, IValueInterface<T> valueInterface)
        {
            targetable.ValueInterfaceMapSource.SetValueInterface(valueInterface);
        }

        public static IValueInterface<T>? GetValueInterface<T>(this ITargetableValueRWSource targetable)
        {
            return targetable.ValueInterfaceMapSource.GetValueInterface<T>();
        }

        public static bool RemoveValueInterface<T>(this ITargetableValueRWSource targetable)
        {
            return targetable.ValueInterfaceMapSource.RemoveValueInterface<T>();
        }

        public static void ClearValueInterfaces(this ITargetableValueRWSource targetable)
        {
            targetable.ValueInterfaceMapSource.ClearValueInterfaces();
        }

        public static IValueInterface<T>? GetValueInterface<T>(this ITargetableValueRW targetable)
        {
            return targetable.ValueInterfaceMap.GetValueInterface<T>();
        }

        /// <summary>
        /// 设置支持针对性接口的对象指定类型的格式。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="targetable">支持针对性接口的对象</param>
        /// <param name="format">格式</param>
        /// <param name="formatProvider">格式提供者, 为 <see langword="null"/> 将使用 <see cref="CultureInfo.CurrentCulture"/>.</param>
        public static void SetValueFormat<T>(this ITargetableValueRWSource targetable, string format, IFormatProvider? formatProvider = null) where T : IFormattable
        {
            targetable.SetValueInterface(new SetValueFormatInterface<T>(ValueInterface<T>.GetInterface(), format, formatProvider));
        }

        /// <summary>
        /// 设置支持针对性接口的对象的 DateTime 格式。
        /// </summary>
        /// <param name="targetable">支持针对性接口的对象</param>
        /// <param name="format">格式</param>
        public static void SetDateTimeFormat(this ITargetableValueRWSource targetable, string format)
        {
            targetable.SetValueInterface(new DateTimeInterface(format));
        }

        /// <summary>
        /// 设置支持针对性接口的对象的 DateTimeOffset 格式。
        /// </summary>
        /// <param name="targetable">支持针对性接口的对象</param>
        /// <param name="format">格式</param>
        public static void SetDateTimeOffsetFormat(this ITargetableValueRWSource targetable, string format)
        {
            targetable.SetValueInterface(new DateTimeOffsetInterface(format));
        }


        sealed class DateTimeInterface : IValueInterface<DateTime>
        {
            private readonly string format;

            public DateTimeInterface(string format)
            {
                this.format = format;
            }

            public DateTime ReadValue(IValueReader valueReader)
            {
                return valueReader.ReadDateTime();
            }

            public void WriteValue(IValueWriter valueWriter, DateTime value)
            {
                valueWriter.WriteString(value.ToString(format));
            }
        }

        sealed class DateTimeOffsetInterface : IValueInterface<DateTimeOffset>
        {
            private readonly string format;

            public DateTimeOffsetInterface(string format)
            {
                this.format = format;
            }

            public DateTimeOffset ReadValue(IValueReader valueReader)
            {
                if (valueReader is IValueReader<DateTimeOffset> reader)
                {
                    return reader.ReadValue();
                }

                var str = valueReader.ReadString();

                if (str is null)
                {
                    return default;
                }

                return DateTime.Parse(str);
            }

            public void WriteValue(IValueWriter valueWriter, DateTimeOffset value)
            {
                if (valueWriter is IValueWriter<DateTimeOffset> writer)
                {
                    writer.WriteValue(value);

                    return;
                }

                valueWriter.WriteString(value.ToString(format));
            }
        }
    }
}