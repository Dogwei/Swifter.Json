using Swifter.RW;
using Swifter.Tools;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

using static Swifter.Tools.StringHelper;
using static Swifter.Tools.TypeHelper;

namespace Swifter.Json
{
    static unsafe class JsonCode
    {
        public const NumberStyles NumberStyle =
            NumberStyles.AllowExponent |
            NumberStyles.AllowDecimalPoint |
            NumberStyles.AllowLeadingSign;

        public const string trueString = "true";
        public const string falseString = "false";
        public const string nullString = "null";
        public const ulong nullBits = ((ulong)'n' << 0) | ((ulong)'u' << 16) | ((ulong)'l' << 32) | ((ulong)'l' << 48);
        public const ulong nullBitsBigEndian = ((ulong)'n' << 48) | ((ulong)'u' << 32) | ((ulong)'l' << 16) | ((ulong)'l' << 0);
        public const string undefinedString = "undefined";
        public const string RefKey = "$ref";
        public const string RefKeyString = "\"$ref\"";
        public const string RefKeyChars = "'$ref'";

        public const string ReferenceRegexText = "((^|/)(?<Name>[^/\\[]*))|(\\[(?<Name>[^\\]]+)\\])";
        public const string ReferenceRegexItemName = "Name";

        public const string ReferenceRootPathName = "#";
        public const string ReferenceRootPathName2 = "";
        public const char ReferenceSeparator = '/';

        public static string TrueString => true.ToString();

        public static string FalseString => false.ToString();

        public const char FixString = '"';
        public const char FixChars = '\'';

        public const char FixObject = '{';
        public const char FixArray = '[';

        public const char ObjectEnding = '}';
        public const char ArrayEnding = ']';

        public const char ValueEnding = ',';
        public const char KeyEnding = ':';

        public const char FixNumberMin = '0';
        public const char FixNumberMax = '9';

        public const char FixPositive = '+';
        public const char FixNegative = '-';

        public const char Fixtrue = 't';
        public const char FixTrue = 'T';

        public const char Fixfalse = 'f';
        public const char FixFalse = 'F';

        public const char Fixnull = 'n';
        public const char FixNull = 'N';

        public const char Fixundefined = 'u';
        public const char FixUndefined = 'U';

        public const char WhiteChar1 = ' ';
        public const char WhiteChar2 = '\b';
        public const char WhiteChar3 = '\f';
        public const char WhiteChar4 = '\n';
        public const char WhiteChar5 = '\t';
        public const char WhiteChar6 = '\r';

        public const char EscapedWhiteChar2 = 'b';
        public const char EscapedWhiteChar3 = 'f';
        public const char EscapedWhiteChar4 = 'n';
        public const char EscapedWhiteChar5 = 't';
        public const char EscapedWhiteChar6 = 'r';

        public const char FixEscape = '\\';
        public const char FixUnicodeEscape = 'U';
        public const char FixunicodeEscape = 'u';

        public const char MaxWhileChar = (char)0x20;
        public const char MaxEscapeChar = (char)0x5c;

        public const char DollarChar = '$';

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static char Descape(ref char* chars)
        {
            ++chars;

            switch (*chars++)
            {
                case EscapedWhiteChar2:
                    return WhiteChar2;
                case EscapedWhiteChar3:
                    return WhiteChar3;
                case EscapedWhiteChar4:
                    return WhiteChar4;
                case EscapedWhiteChar5:
                    return WhiteChar5;
                case EscapedWhiteChar6:
                    return WhiteChar6;
                case FixUnicodeEscape:
                case FixunicodeEscape:
                    return DescapeUnicode(ref chars);
                case var val:
                    return val;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static char DescapeUnicode(ref char* chars)
        {
            var ret = (char)(
                (GetDigital(chars[0]) << 12) |
                (GetDigital(chars[1]) << 8) |
                (GetDigital(chars[2]) << 4) |
                (GetDigital(chars[3]))
                );

            chars += 4;

            return ret;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetDigital(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return c - '0';
            }

            if (c >= 'a' && c <= 'f')
            {
                return c - 'a' + 10;
            }

            if (c >= 'A' && c <= 'F')
            {
                return c - 'A' + 10;
            }

            throw new FormatException($"Hex : 0x{c}");
        }


        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T FastReadParse<T>(Ps<char> str, JsonFormatterOptions options)
        {
            if (typeof(T) == typeof(DateTime))
            {
                if (DateTimeHelper.TryParseISODateTime(str.Pointer, str.Length, out DateTime date_time)) return As<DateTime, T>(date_time);
            }
            else if (typeof(T) == typeof(DateTimeOffset))
            {
                if (DateTimeHelper.TryParseISODateTime(str.Pointer, str.Length, out DateTimeOffset date_time_offset)) As<DateTimeOffset, T>(date_time_offset);
            }
            else if (typeof(T) == typeof(Guid))
            {
                var (_, length, value) = NumberHelper.ParseGuid(str.Pointer, str.Length);

                if (length == str.Length) As<Guid, T>(value);
            }
            else if (typeof(T) == typeof(long))
            {
                var (_, length, value) = NumberHelper.DecimalParseInt64(str.Pointer, str.Length);

                if (length == str.Length) As<long, T>(value);
            }
            else if (typeof(T) == typeof(ulong))
            {
                var (_, length, value) = NumberHelper.DecimalParseUInt64(str.Pointer, str.Length);

                if (length == str.Length) As<ulong, T>(value);
            }
            else if (typeof(T) == typeof(double))
            {
                if ((options & JsonFormatterOptions.UseSystemFloatingPointsMethods) != 0)
                {
#if Span
                    if (double.TryParse(str, out var value))
                    {
                        return As<double, T>(value);

                    }
#endif
                }
                else
                {
                    var (_, length, value) = NumberHelper.DecimalParseDouble(str.Pointer, str.Length);

                    if (length == str.Length) As<double, T>(value);
                }
            }
            else if (typeof(T) == typeof(decimal))
            {
                var (_, length, value) = NumberHelper.ParseDecimal(str.Pointer, str.Length);

                if (length == str.Length) As<decimal, T>(value);
            }
            else if (typeof(T) == typeof(RWPathInfo))
            {
                return As<object, T>(ParseReference(str.Pointer, str.Length));
            }

            if (typeof(T) == typeof(long) ||
                typeof(T) == typeof(ulong) ||
                typeof(T) == typeof(double) ||
                typeof(T) == typeof(decimal))
            {
                var numberInfo = NumberHelper.GetNumberInfo(str.Pointer, str.Length);

                if (numberInfo.End == str.Length && numberInfo.IsNumber && numberInfo.IsCommonRadix(out var radix))
                {
                    if (typeof(T) == typeof(long)) return As<long, T>(numberInfo.ToInt64(radix));
                    if (typeof(T) == typeof(ulong)) return As<ulong, T>(numberInfo.ToUInt64(radix));
                    if (typeof(T) == typeof(double)) return As<double, T>(numberInfo.ToDouble(radix));

                    if (typeof(T) == typeof(decimal) && numberInfo.IsDecimal) return As<decimal, T>(numberInfo.ToDecimal());
                }
            }

            return SlowReadParse<T>(str.ToStringEx());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T SlowReadParse<T>(string str)
        {
            if (typeof(T) == typeof(DateTime))
            {
                return Underlying.As<DateTime, T>(ref Underlying.AsRef(DateTime.Parse(str)));
            }
            else if (typeof(T) == typeof(DateTimeOffset))
            {
                return Underlying.As<DateTimeOffset, T>(ref Underlying.AsRef(DateTimeOffset.Parse(str)));
            }
            else if (typeof(T) == typeof(Guid))
            {
                return Underlying.As<Guid, T>(ref Underlying.AsRef(new Guid(str)));
            }
            else if (typeof(T) == typeof(long))
            {
                return Underlying.As<long, T>(ref Underlying.AsRef(long.Parse(str, NumberStyle)));
            }
            else if (typeof(T) == typeof(ulong))
            {
                return Underlying.As<ulong, T>(ref Underlying.AsRef(ulong.Parse(str, NumberStyle)));
            }
            else if (typeof(T) == typeof(double))
            {
                return Underlying.As<double, T>(ref Underlying.AsRef(double.Parse(str, NumberStyle)));
            }
            else if (typeof(T) == typeof(decimal))
            {
                return Underlying.As<decimal, T>(ref Underlying.AsRef(decimal.Parse(str, NumberStyle)));
            }
            else if (typeof(T) == typeof(RWPathInfo))
            {
                fixed (char* chars = str)
                {
                    return (T)(object)ParseReference(chars, str.Length);
                }
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static RWPathInfo ParseReference(char* chars, int length)
        {
            var reference = RWPathInfo.Root;

            // i: Index;
            // j: Item start;
            // k: Item length;
            for (int i = 0, j = 0, k = 0; ; i++)
            {
                if (i < length)
                {
                    switch (chars[i])
                    {
                        case '/':
                            break;
                        case '%':

                            if (i + 2 < length)
                            {
                                GetDigital(chars[++i]);
                                GetDigital(chars[++i]);

                                continue;
                            }

                            throw new FormatException();
                        default:
                            ++k;
                            continue;
                    }
                }

                if (j < length)
                {
                    var item = i - j == k ? StringHelper.ToString(chars + j, k) : GetItem(j, i, k);

                    j = i + 1;
                    k = 0;

                    switch (item)
                    {
                        case ReferenceRootPathName:
                        case ReferenceRootPathName2:
                            if (reference.IsRoot)
                            {
                                continue;
                            }
                            break;
                    }

                    if (item.Length != 0 && item[0] >= FixNumberMin && item[0] <= FixNumberMax && int.TryParse(item, out var result))
                    {
                        reference = RWPathInfo.Create(result, reference);
                    }
                    else
                    {
                        reference = RWPathInfo.Create(item, reference);
                    }

                    continue;
                }

                break;
            }

            return reference;


            string GetItem(int start, int end, int count)
            {
                var bytesCount = (end - start - count) / 3;

                var bytes = stackalloc byte[bytesCount];

                for (int i = start, j = 0; i < end; i++)
                {
                    if (chars[i] == '%')
                    {
                        bytes[j++] = (byte)((GetDigital(chars[++i]) << 4) | GetDigital(chars[++i]));
                    }
                }

                var charsCount = Encoding.UTF8.GetCharCount(bytes, bytesCount);

                var str = MakeString(charsCount + count);

                fixed (char* pStr = str)
                {
                    for (int i = start, j = 0, k = 0; i < end; i++)
                    {
                        if (chars[i] == '%')
                        {
                            var l = 1;

                            for (i += 3; i < end && chars[i] == '%'; i += 3, ++l) ;

                            --i;

                            j += Encoding.UTF8.GetChars(bytes + k, l, pStr + j, charsCount);

                            k += l;
                        }
                        else
                        {
                            pStr[j] = chars[i];

                            ++j;
                        }
                    }
                }

                return str;
            }
        }
    }
}