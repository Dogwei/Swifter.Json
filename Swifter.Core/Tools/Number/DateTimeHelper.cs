using System;
using System.Runtime.CompilerServices;

using static Swifter.Tools.NumberHelper;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供对象日期和时间操作的方法。
    /// </summary>
    public unsafe static class DateTimeHelper
    {
        /// <summary>
        /// 1970-01-01 的 Tick 值。
        /// </summary>
        public const long TicksPerUnixEpoch = TimeSpan.TicksPerDay * 719162;

        /// <summary>
        /// 本地时间与 UTC 时间的时差 Tick 值。
        /// </summary>
        public static readonly long TicksPerUTCDifference;

        /// <summary>
        /// 1 Tick 的纳秒值。
        /// </summary>
        public const long NanosecondsPerTick = 100;

        /// <summary>
        /// ISO 格式日期字符串的最大长度。
        /// </summary>
        public const int ISOStringMaxLength = 50;

        /// <summary>
        /// Db 格式日期字符串的最大长度。
        /// </summary>
        public const int DbStringMaxLength = 35;

        static DateTimeHelper()
        {
            var tempDateTime = DateTime.Now;

            TicksPerUTCDifference = (tempDateTime - tempDateTime.ToUniversalTime()).Ticks;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static int ToISOString(DateTime value, char* chars, long uTCDifference)
        {
            var c = chars;
            var dec = NumberHelper.Decimal;

            uint year = (uint)value.Year;

            DecimalAppendD1(c, year / 1000); ++c;
            DecimalAppendD3(c, year % 1000); c += 3;

            *c = '-';++c;

            DecimalAppendD2(c, (uint)value.Month); c += 2;

            *c = '-'; ++c;

            DecimalAppendD2(c, (uint)value.Day); c += 2;

            *c = 'T'; ++c;

            DecimalAppendD2(c, (uint)value.Hour); c += 2;

            *c = ':'; ++c;

            DecimalAppendD2(c, (uint)value.Minute); c += 2;

            *c = ':'; ++c;

            DecimalAppendD2(c, (uint)value.Second); c += 2;

            *c = '.'; ++c;

            DecimalAppendD3(c, (uint)value.Millisecond); c += 3;

            if (uTCDifference > 0)
            {
                *c = '+'; ++c;
            }
            else if (uTCDifference < 0)
            {
                *c = '-'; ++c;

                uTCDifference = -uTCDifference;
            }
            else
            {
                *c = 'Z'; ++c;

                goto Return;
            }


            long tDHour = uTCDifference / TimeSpan.TicksPerHour;

            if (tDHour < 100)
            {
                DecimalAppendD2(c, (uint)tDHour); c += 2;
            }
            else
            {
                throw new FormatException("UTC Time Difference too big.");
            }

            *c = ':'; ++c;

            DecimalAppendD2(c, (uint)((uTCDifference % TimeSpan.TicksPerHour) / TimeSpan.TicksPerMinute)); c += 2;

        Return:
            return (int)(c - chars);
        }

        /// <summary>
        /// 将日期和时间以 ISO8061 格式字符串写入到字符串中。
        /// </summary>
        /// <param name="value">日期和时间</param>
        /// <param name="chars">字符串</param>
        /// <returns>返回写入结束位置，最后一个字符写入位置 + 1。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int ToISOString(DateTime value, char* chars)
        {
            return ToISOString(value, chars, TicksPerUTCDifference);
        }

        /// <summary>
        /// 将日期和时间点以 ISO8061 格式字符串写入到字符串中。
        /// </summary>
        /// <param name="value">日期和时间点</param>
        /// <param name="chars">字符串</param>
        /// <returns>返回写入结束位置，最后一个字符写入位置 + 1。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int ToISOString(DateTimeOffset value, char* chars)
        {
            return ToISOString(value.DateTime, chars, value.Offset.Ticks);
        }

        /// <summary>
        /// 将日期和时间的 UTC 时间以 ISO8061 格式字符串写入到字符串中。
        /// </summary>
        /// <param name="value">日期和时间</param>
        /// <param name="chars">字符串</param>
        /// <returns>返回写入结束位置，最后一个字符写入位置 + 1。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int ToUTCISOString(DateTime value, char* chars)
        {
            return ToISOString(value.AddTicks(-TicksPerUTCDifference), chars, 0);
        }

        /// <summary>
        /// 将日期和时间以 ISO8061 格式化字符串。
        /// </summary>
        /// <param name="value">日期和时间</param>
        /// <returns>返回一个字符串。</returns>
        public unsafe static string ToISOString(DateTime value)
        {
            char* chars = stackalloc char[ISOStringMaxLength];

            int length = ToISOString(value, chars, TicksPerUTCDifference);

            return new string(chars, 0, length);
        }

        /// <summary>
        /// 将日期和时间点以 ISO8061 格式化字符串。
        /// </summary>
        /// <param name="value">日期和时间点</param>
        /// <returns>返回一个字符串。</returns>
        public unsafe static string ToISOString(DateTimeOffset value)
        {
            char* chars = stackalloc char[ISOStringMaxLength];

            int length = ToISOString(value.DateTime, chars, value.Offset.Ticks);

            return new string(chars, 0, length);
        }

        /// <summary>
        /// 将日期和时间的 UTC 时间以 ISO8061 格式化字符串。
        /// </summary>
        /// <param name="value">日期和时间</param>
        /// <returns>返回一个字符串。</returns>
        public unsafe static string ToUTCISOString(DateTime value)
        {
            char* chars = stackalloc char[ISOStringMaxLength];

            int length = ToUTCISOString(value, chars);

            return new string(chars, 0, length);
        }

        /// <summary>
        /// 将日期和时间格式化为数据库格式的字符串。格式化字符串：'yyyy-MM-dd HH:mm:ss.fffffff'。
        /// </summary>
        /// <param name="value">日期和时间</param>
        /// <returns>返回一个字符串</returns>
        public static string ToDbString(DateTime value)
        {
            char* chars = stackalloc char[DbStringMaxLength];

            int length = ToDbString(value, chars);

            return new string(chars, 0, length);
        }


        /// <summary>
        /// 将时间格式化为数据库格式的字符串。格式化字符串：'HH:mm:ss.fffffff'。
        /// </summary>
        /// <param name="value">时间</param>
        /// <returns>返回一个字符串</returns>
        public static string ToDbString(TimeSpan value)
        {
            char* chars = stackalloc char[DbStringMaxLength];

            int length = ToDbString(value, chars);

            return new string(chars, 0, length);
        }


        /// <summary>
        /// 将日期和时间格式化为数据库格式的字符串。格式化字符串：'yyyy-MM-dd HH:mm:ss.fffffff zzz'。
        /// </summary>
        /// <param name="value">日期和时间</param>
        /// <returns>返回一个字符串</returns>
        public static string ToDbString(DateTimeOffset value)
        {
            char* chars = stackalloc char[DbStringMaxLength];

            int length = ToDbString(value, chars);

            return new string(chars, 0, length);
        }

        /// <summary>
        /// 将日期和时间格式化为数据库格式的字符串。格式化字符串：'yyyy-MM-dd HH:mm:ss.fffffff'。
        /// </summary>
        /// <param name="value">日期和时间</param>
        /// <param name="chars">字符串</param>
        /// <returns>返回字符串长度</returns>
        public static int ToDbString(DateTime value, char* chars)
        {
            return (int)(InternalToDbString(value, chars) - chars);
        }

        /// <summary>
        /// 将时间格式化为数据库格式的字符串。格式化字符串：'HH:mm:ss.fffffff'。
        /// </summary>
        /// <param name="value">时间</param>
        /// <param name="chars">字符串</param>
        /// <returns>返回字符串长度</returns>
        public static int ToDbString(TimeSpan value, char* chars)
        {
            return (int)(InternalToDbString(value, chars, false) - chars);
        }

        /// <summary>
        /// 将日期和时间格式化为数据库格式的字符串。格式化字符串：'yyyy-MM-dd HH:mm:ss.fffffff zzz'。
        /// </summary>
        /// <param name="value">日期和时间</param>
        /// <param name="chars">字符串</param>
        /// <returns>返回字符串长度</returns>
        public static int ToDbString(DateTimeOffset value, char* chars)
        {
            return (int)(InternalToDbString(value, chars) - chars);
        }

        private static char* InternalToDbString(TimeSpan value, char* c, bool isOffset)
        {
            DecimalAppendD2(c, (uint)value.Hours); c += 2;

            *c = ':'; ++c;

            DecimalAppendD2(c, (uint)value.Minutes); c += 2;

            if (value.Ticks % TimeSpan.TicksPerMinute != 0 || !isOffset)
            {
                *c = ':'; ++c;

                DecimalAppendD2(c, (uint)value.Seconds); c += 2;

                if (value.Ticks % TimeSpan.TicksPerSecond != 0)
                {
                    *c = '.'; ++c;

                    DecimalAppendD3(c, (uint)value.Milliseconds); c += 3;

                    var tick = (uint)(value.Ticks % TimeSpan.TicksPerMillisecond);

                    if (tick != 0)
                    {
                        DecimalAppendD1(c, tick / 1000); ++c;
                        DecimalAppendD3(c, tick % 1000); c += 3;
                    }
                }
            }

            return c;
        }

        private static char* InternalToDbString(DateTime value, char* c)
        {
            var year = (uint)value.Year;

            DecimalAppendD1(c, year / 1000); ++c;
            DecimalAppendD3(c, year % 1000); c += 3;

            *c = '-'; ++c;

            DecimalAppendD2(c, (uint)value.Month); c += 2;

            *c = '-'; ++c;

            DecimalAppendD2(c, (uint)value.Day); c += 2;

            if (value.TimeOfDay != default)
            {
                *c = ' '; ++c;

                c = InternalToDbString(value.TimeOfDay, c, false);
            }

            return c;
        }

        private static char* InternalToDbString(DateTimeOffset value, char* c)
        {
            c = InternalToDbString(value.DateTime, c);

            *c = ' '; ++c;

            if (value.Offset >= TimeSpan.Zero)
            {
                *c = '+'; ++c;
            }
            else
            {
                *c = '-'; ++c;
            }

            c = InternalToDbString(value.Offset, c, true);

            return c;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private unsafe static void SkipNoMeta(char* chars, ref int begin, bool ignoreMinu)
        {
            char c = chars[begin];

            if (c >= '0' && c <= '9')
            {
                return;
            }

            if (c >= 'a' && c <= 'z')
            {
                return;
            }

            if (c >= 'A' && c <= 'Z')
            {
                return;
            }

            if (c == '+')
            {
                return;
            }

            if (ignoreMinu && c == '-')
            {
                return;
            }

            ++begin;
        }

        /// <summary>
        /// 尝试解析 ISO8061 格式日期和时间字符串。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">解析结束位置。</param>
        /// <param name="value">成功返回日期和时间对象，失败返回日期和时间最小值。</param>
        /// <param name="difference">返回解析出的时间差</param>
        /// <returns>返回成功或失败。</returns>
        public static bool TryParseISODateTime(char* chars, int length, out DateTime value, out long difference)
        {
            var index = 0;

            int year = 1,
                month = 1,
                day = 1,
                hour = 0,
                minute = 0,
                second = 0,
                millisecond = 0,
                week = 0,
                ticks = 0; // if 0 then no using.

            difference = TicksPerUTCDifference; // if 0 then no using.

            // Date
            // yyyy-MM-dd
            // yyyyMMdd
            // yyyy-ddd
            // yyyyddd
            // yyyy-Www-d
            // yyyyWwwd

            // Time
            // hh:mm:ss.iii
            // hh:mm:ss
            // hh:mm
            // hhmmssiii
            // hhmmss
            // hhmm

            // yyyyddd
            if (length < 7)
            {
                goto False;
            }

            /* 读取前四位数字为年份，如果读取失败则返回 false */
            if (!DecimalTryParse(chars + index, 4, out year))
            {
                goto False;
            }

            index += 4;

            SkipNoMeta(chars, ref index, false);

            switch (chars[index])
            {
                case 'W':
                case 'w':
                    goto Wwwd;
            }

            // ddd
            switch (length - index)
            {
                case 0:
                case 1:
                case 2:
                    goto False;
                case 3:
                    goto ddd;

            }

            switch (chars[index + 3])
            {
                case 'T':
                case 't':
                    goto ddd;
            }

            /* 读取两位数字为月份，如果读取失败则返回 false */
            if (!DecimalTryParse(chars + index, 2, out month))
            {
                goto False;
            }

            index += 2;

            SkipNoMeta(chars, ref index, false);

            // dd
            if (length - index < 2)
            {
                goto False;
            }

            /* 读取两位数字为天份，如果读取失败则返回 false */
            if (!DecimalTryParse(chars + index, 2, out day))
            {
                goto False;
            }

            index += 2;

            goto Time;
            ddd:

            /* 读取三位数字为一年第几天，如果读取失败则返回 false */
            if (!DecimalTryParse(chars + index, 3, out day))
            {
                goto False;
            }

            index += 3;

            goto Time;

            Wwwd:

            // Wwwd
            if (length - index < 4)
            {
                goto False;
            }

            ++index;

            /* 读取两位数字为第几个星期，如果读取失败则返回 false */
            if (!DecimalTryParse(chars + index, 2, out week))
            {
                goto False;
            }

            index += 2;

            SkipNoMeta(chars, ref index, false);

            // d
            if (length - index < 1)
            {
                goto False;
            }

            /* 读取一位数字为星期的第几天，如果读取失败则返回 false */
            if (!DecimalTryParse(chars + index, 1, out day))
            {
                goto False;
            }

            ++index;

            Time:

            if (length - index < 1)
            {
                goto True;
            }

            SkipNoMeta(chars, ref index, true);

            if (length - index < 1)
            {
                goto False;
            }

            switch (chars[index])
            {
                case '+':
                case '-':
                case 'Z':
                    goto Difference;
                case 'T':
                case 't':
                    ++index;
                    break;
            }

            // Z
            if (length - index < 1)
            {
                goto False;
            }

            SkipNoMeta(chars, ref index, true);

            if (length - index < 1)
            {
                goto False;
            }

            switch (chars[index])
            {
                case '+':
                case '-':
                case 'Z':
                    goto Difference;
            }

            // hhmm
            if (length - index < 4)
            {
                goto False;
            }

            /* 读取两位数字为小时，如果读取失败则返回 false */
            if (!DecimalTryParse(chars + index, 2, out hour))
            {
                goto False;
            }

            index += 2;

            SkipNoMeta(chars, ref index, true);

            if (length - index < 2)
            {
                goto False;
            }

            /* 读取两位数字为分钟，如果读取失败则返回 false */
            if (!DecimalTryParse(chars + index, 2, out minute))
            {
                goto False;
            }

            index += 2;

            if (length - index < 1)
            {
                goto True;
            }

            SkipNoMeta(chars, ref index, true);

            if (length - index < 1)
            {
                goto False;
            }

            switch (chars[index])
            {
                case '+':
                case '-':
                case 'Z':
                    goto Difference;
            }

            if (length - index < 2)
            {
                goto False;
            }

            /* 读取两位数字为秒钟，如果读取失败则返回 false */
            if (!DecimalTryParse(chars + index, 2, out second))
            {
                goto False;
            }

            index += 2;

            if (length - index < 1)
            {
                goto True;
            }

            SkipNoMeta(chars, ref index, true);

            if (length - index < 1)
            {
                goto False;
            }

            switch (chars[index])
            {
                case '+':
                case '-':
                case 'Z':
                    goto Difference;
            }

            if (length - index < 3)
            {
                goto False;
            }

            /* 读取三位数字为毫秒，如果读取失败则返回 false */
            if (!DecimalTryParse(chars + index, 3, out millisecond))
            {
                goto False;
            }

            index += 3;

            if (length - index < 1)
            {
                goto True;
            }

            /* .Net 平台包含 Ticks 值 */
            if (length - index >= 4)
            {
                /* 读取四位数为 Ticks，如果读取失败则返回 false */
                if (DecimalTryParse(chars + index, 4, out var _ticks))
                {
                    ticks = _ticks;

                    index += 4;
                }
            }

            SkipNoMeta(chars, ref index, true);

            if (length - index < 1)
            {
                goto False;
            }

            switch (chars[index])
            {
                case '0':
                case '1':
                case '2':
                case '+':
                case '-':
                case 'Z':
                    goto Difference;
            }

            goto False;

            Difference:

            bool isPlus;

            switch (chars[index])
            {
                case '0':
                case '1':
                case '2':
                    --index;
                    isPlus = true;
                    break;
                case '+':
                    isPlus = true;
                    break;
                case '-':
                    isPlus = false;
                    break;
                case 'Z':
                    difference = 0;

                    goto True;
                default:
                    goto False;
            }

            ++index;

            if (length - index < 2)
            {
                goto False;
            }

            int dHour;
            int dMinute = 0;

            /* 读取两位数为时差小时部分 */
            if (!DecimalTryParse(chars + index, 2, out dHour))
            {
                goto False;
            }

            index += 2;

            if (length - index != 0)
            {
                SkipNoMeta(chars, ref index, true);

                if (length - index < 2)
                {
                    goto False;
                }

                /* 读取两位数为时差分钟部分 */
                if (!DecimalTryParse(chars + index, 2, out dMinute))
                {
                    goto False;
                }

                index += 2;
            }

            if (isPlus)
            {
                difference = (dHour * TimeSpan.TicksPerHour) + (dMinute * TimeSpan.TicksPerMinute);
            }
            else
            {
                difference = -((dHour * TimeSpan.TicksPerHour) + (dMinute * TimeSpan.TicksPerMinute));
            }

            True:

            value = new DateTime(year, month, day, hour, minute, second, millisecond);

            if (week != 0)
            {
                value = value.AddDays(week * 7);
            }

            if (ticks != 0)
            {
                value = value.AddTicks(ticks);
            }

            return true;

            False:

            value = DateTime.MinValue;

            return false;
        }


        /// <summary>
        /// 尝试解析 ISO8061 格式日期和时间字符串。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">解析结束位置。</param>
        /// <param name="value">成功返回日期和时间对象，失败返回日期和时间最小值。</param>
        /// <returns>返回成功或失败。</returns>
        public unsafe static bool TryParseISODateTime(char* chars, int length, out DateTime value)
        {
            var r = TryParseISODateTime(chars, length, out value, out var difference);

            if (r)
            {
                value = value.AddTicks(TicksPerUTCDifference - difference);

                return true;
            }

            value = DateTime.MinValue;

            return false;
        }

        /// <summary>
        /// 尝试解析 ISO8061 格式日期和时间字符串。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="value">成功返回日期和时间对象，失败返回日期和时间最小值。</param>
        /// <returns>返回成功或失败。</returns>
        public unsafe static bool TryParseISODateTime(
#if Span
            ReadOnlySpan<char>
#else
            string
#endif
            text, out DateTime value)
        {
            fixed (char* chars = text)
            {
                return TryParseISODateTime(chars, text.Length, out value);
            }
        }


        /// <summary>
        /// 尝试解析 ISO8061 格式日期和时间点字符串。
        /// </summary>
        /// <param name="chars">字符串</param>
        /// <param name="length">解析结束位置。</param>
        /// <param name="value">成功返回日期和时间点对象，失败返回日期和时间点最小值。</param>
        /// <returns>返回成功或失败。</returns>
        public unsafe static bool TryParseISODateTime(char* chars, int length, out DateTimeOffset value)
        {
            var r = TryParseISODateTime(chars, length, out var dateTime, out var difference);

            if (r)
            {
                value = new DateTimeOffset(dateTime, new TimeSpan(difference));

                return true;
            }

            value = DateTimeOffset.MinValue;

            return false;
        }

        /// <summary>
        /// 尝试解析 ISO8061 格式日期和时间点字符串。
        /// </summary>
        /// <param name="text">字符串</param>
        /// <param name="value">成功返回日期和时间点对象，失败返回日期和时间点最小值。</param>
        /// <returns>返回成功或失败。</returns>
        public unsafe static bool TryParseISODateTime(
#if Span
            ReadOnlySpan<char>
#else
            string
#endif
            text, out DateTimeOffset value)
        {
            fixed (char* chars = text)
            {
                return TryParseISODateTime(chars, text.Length, out value);
            }
        }
    }
}