

using System;

namespace Swifter.RW
{
    /// <summary>
    /// 提供 ValueInterface 的扩展方法。
    /// </summary>
    public static class ValueInterfaceExtensions
    {
        /// <summary>
        /// 设置支持针对性接口的对象指定类型的值读写接口。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="targeted">支持针对性接口的对象</param>
        /// <param name="valueInterface">值读写接口</param>
        public static void SetValueInterface<T>(this ITargetedBind targeted, IValueInterface<T> valueInterface)
        {
            ValueInterface<T>.SetTargetedInterface(targeted, valueInterface);
        }

        /// <summary>
        /// 设置支持针对性接口的对象指定类型的格式。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="targeted">支持针对性接口的对象</param>
        /// <param name="format">格式</param>
        public static void SetValueFormat<T>(this ITargetedBind targeted, string format) where T : IFormattable
        {
            ValueInterface<T>.SetTargetedInterface(targeted, new SetValueFormatInterface<T>(ValueInterface<T>.Content, format));
        }

        /// <summary>
        /// 设置支持针对性接口的对象的 DateTime 格式。
        /// </summary>
        /// <param name="targeted">支持针对性接口的对象</param>
        /// <param name="format">格式</param>
        public static void SetDateTimeFormat(this ITargetedBind targeted, string format)
        {
            ValueInterface<DateTime>.SetTargetedInterface(targeted, new DateTimeInterface(format));
        }

        /// <summary>
        /// 设置支持针对性接口的对象的 DateTimeOffset 格式。
        /// </summary>
        /// <param name="targeted">支持针对性接口的对象</param>
        /// <param name="format">格式</param>
        public static void SetDateTimeOffsetFormat(this ITargetedBind targeted, string format)
        {
            ValueInterface<DateTimeOffset>.SetTargetedInterface(targeted, new DateTimeOffsetInterface(format));
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
                return DateTime.Parse(valueReader.ReadString());
            }

            public void WriteValue(IValueWriter valueWriter, DateTimeOffset value)
            {
                valueWriter.WriteString(value.ToString(format));
            }
        }
    }
}
