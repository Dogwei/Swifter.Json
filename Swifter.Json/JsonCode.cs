using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    static class JsonCode
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
        public const char RefSeparater = '/';
        public const char RefEscape = '%';
        public const string MultiLineCommentPrefix = "/*";
        public const string MultiLineCommentSuffix = "*/";
        public const string SingleLineCommentPrefix = "//";
        public const char LineFeedChar = '\n';

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

        public const char FixComment = '/';

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
        public const char AsteriskChar = '*';

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IsGeneralChar(char chr)
        {
            return chr != FixString && chr != FixEscape && chr >= 0x20;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IsRefGeneralChar(char chr)
        {
            return chr != FixString && chr != FixEscape && chr != RefSeparater && chr != RefEscape && chr > 0x20;
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

#if Async
        public static async System.Threading.Tasks.ValueTask FlushAsync(this JsonSerializer jsonSerializer)
        {
            if (jsonSerializer.Offset is 0)
            {
                return;
            }

            if (jsonSerializer.segmenterOrHGCache is JsonSegmentedContent segmenter)
            {
                segmenter.hGCache.Count = jsonSerializer.Offset;

                var isValueEnding = segmenter.hGCache.Context[segmenter.hGCache.Offset + segmenter.hGCache.Count - 1] is ValueEnding;

                if (isValueEnding)
                {
                    --segmenter.hGCache.Count;
                }

                await segmenter.WriteSegmentAsync();

                segmenter.hGCache.Count = 0;

                jsonSerializer.InternalClear();

                if (isValueEnding)
                {
                    jsonSerializer.Append(ValueEnding);

                    ++segmenter.hGCache.Count;
                }
            }
            else
            {
                throw new NotSupportedException("Segmented writes are not supported!");
            }
        }
#endif
    }
}

namespace System.Numerics
{

}