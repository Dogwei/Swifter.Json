using System.Text.RegularExpressions;

namespace Swifter.Json
{
    static class JsonCode
    {
        public const string trueString = "true";
        public const string falseString = "false";
        public const string nullString = "null";
        public const string undefinedString = "undefined";
        public const string RefKey = "$ref";
        public const string RefKeyString = "\"$ref\"";
        public const string RefKeyChars = "'$ref'";

        public const string ReferenceRegexText = "((^|/)(?<Name>[^/\\[]*))|(\\[(?<Name>[^\\]]+)\\])";
        public const string ReferenceRegexItemName = "Name";

        public const string ReferenceRootPathName = "#";
        public const string ReferenceRootPathName2 = "";
        public const char ReferenceSeparator = '/';

        static Regex _ReferenceRegex;

        public static Regex ReferenceRegex => _ReferenceRegex ?? (_ReferenceRegex = new Regex(ReferenceRegexText));

        public static string TrueString => true.ToString();

        public static string FalseString => false.ToString();

        public static readonly char[] EscapeMap = new char[128];

        static JsonCode()
        {
            EscapeMap[FixString] = FixString;
            EscapeMap[FixEscape] = FixEscape;

            EscapeMap[WhiteChar2] = EscapedWhiteChar2;
            EscapeMap[WhiteChar3] = EscapedWhiteChar3;
            EscapeMap[WhiteChar4] = EscapedWhiteChar4;
            EscapeMap[WhiteChar5] = EscapedWhiteChar5;
            EscapeMap[WhiteChar6] = EscapedWhiteChar6;
        }

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
    }
}