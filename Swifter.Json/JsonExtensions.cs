using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Swifter.Json
{
    /// <summary>
    /// 提供 Json 格式化工具的扩展方法。
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class JsonExtensions
    {
        /// <summary>
        /// 设置 Json 格式化工具序列化时的 DateTime 格式。
        /// </summary>
        /// <param name="jsonFormatter"></param>
        /// <param name="format"></param>
        public static void SetDateTimeFormat(this JsonFormatter jsonFormatter, string format)
        {
            jsonFormatter.SetValueInterface(new DateTimeInterface(format));
        }

        /// <summary>
        /// 设置 Json 格式化工具序列化时的 DateTimeOffset 格式。
        /// </summary>
        /// <param name="jsonFormatter"></param>
        /// <param name="format"></param>
        public static void SetDateTimeOffsetFormat(this JsonFormatter jsonFormatter, string format)
        {
            jsonFormatter.SetValueInterface(new DateTimeOffsetInterface(format));
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