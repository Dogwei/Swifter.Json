#if BenchmarkDotNet

using BenchmarkDotNet.Attributes;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace Swifter.Debug
{
    [Config(typeof(MyConfig))]
    public unsafe class Number
    {
        const int str_length = 100;

        [Benchmark]
        public void int64_9_tostring_decimal_system()
        {
            const long val = 9;

            Span<char> span = stackalloc char[str_length];

            val.TryFormat(span, out var charsWritten);
        }

        [Benchmark]
        public void int64_9_tostring_decimal_swifter()
        {
            const long val = 9;

            char* chars = stackalloc char[str_length];

            NumberHelper.Decimal.ToString(val, chars);
        }

        [Benchmark]
        public void uint64_9_tostring_decimal_system()
        {
            const ulong val = 9;

            Span<char> span = stackalloc char[str_length];

            val.TryFormat(span, out var charsWritten);
        }

        [Benchmark]
        public void uint64_9_tostring_decimal_swifter()
        {
            const ulong val = 9;

            char* chars = stackalloc char[str_length];

            NumberHelper.Decimal.ToString(val, chars);
        }




        [Benchmark]
        public void int64_n_999_tostring_decimal_system()
        {
            const long val = -999;

            Span<char> span = stackalloc char[str_length];

            val.TryFormat(span, out var charsWritten);
        }

        [Benchmark]
        public void int64_n_999_tostring_decimal_swifter()
        {
            const long val = -999;

            char* chars = stackalloc char[str_length];

            NumberHelper.Decimal.ToString(val, chars);
        }

        [Benchmark]
        public void int64_999_tostring_decimal_system()
        {
            const long val = 999;

            Span<char> span = stackalloc char[str_length];

            val.TryFormat(span, out var charsWritten);
        }

        [Benchmark]
        public void int64_999_tostring_decimal_swifter()
        {
            const long val = 999;

            char* chars = stackalloc char[str_length];

            NumberHelper.Decimal.ToString(val, chars);
        }

        [Benchmark]
        public void uint64_999_tostring_decimal_system()
        {
            const ulong val = 999;

            Span<char> span = stackalloc char[str_length];

            val.TryFormat(span, out var charsWritten);
        }

        [Benchmark]
        public void uint64_999_tostring_decimal_swifter()
        {
            const ulong val = 999;

            char* chars = stackalloc char[str_length];

            NumberHelper.Decimal.ToString(val, chars);
        }




        [Benchmark]
        public void int64_99999999999999_tostring_decimal_system()
        {
            const long val = 99999999999999;

            Span<char> span = stackalloc char[str_length];

            val.TryFormat(span, out var charsWritten);
        }

        [Benchmark]
        public void int64_99999999999999_tostring_decimal_swifter()
        {
            const long val = 99999999999999;

            char* chars = stackalloc char[str_length];

            NumberHelper.Decimal.ToString(val, chars);
        }

        [Benchmark]
        public void uint64_99999999999999_tostring_decimal_system()
        {
            const ulong val = 99999999999999;

            Span<char> span = stackalloc char[str_length];

            val.TryFormat(span, out var charsWritten);
        }

        [Benchmark]
        public void uint64_99999999999999_tostring_decimal_swifter()
        {
            const ulong val = 99999999999999;

            char* chars = stackalloc char[str_length];

            NumberHelper.Decimal.ToString(val, chars);
        }


        [Benchmark]
        public void int64_99999999999999_tostring_hex_system()
        {
            const long val = 99999999999999;

            Span<char> span = stackalloc char[str_length];

            val.TryFormat(span, out var charsWritten, "X");
        }

        [Benchmark]
        public void int64_99999999999999_tostring_hex_swifter()
        {
            const long val = 99999999999999;

            char* chars = stackalloc char[str_length];

            NumberHelper.Hex.ToString(val, chars);
        }

        [Benchmark]
        public void uint64_99999999999999_tostring_hex_system()
        {
            const ulong val = 99999999999999;

            Span<char> span = stackalloc char[str_length];

            val.TryFormat(span, out var charsWritten, "X");
        }

        [Benchmark]
        public void uint64_99999999999999_tostring_hex_swifter()
        {
            const ulong val = 99999999999999;

            char* chars = stackalloc char[str_length];

            NumberHelper.Hex.ToString(val, chars);
        }

        [Benchmark]
        public void double_99999999999999_tostring_deciaml_system()
        {
            const double val = 99999999999999;

            Span<char> span = stackalloc char[str_length];

            val.TryFormat(span, out var charsWritten);
        }

        [Benchmark]
        public void double_99999999999999_tostring_deciaml_swifter()
        {
            const double val = 99999999999999;

            char* chars = stackalloc char[str_length];

            NumberHelper.Hex.ToString(val, chars);
        }

        [Benchmark]
        public void double_100e100_tostring_deciaml_system()
        {
            const double val = 100e100;

            Span<char> span = stackalloc char[str_length];

            val.TryFormat(span, out var charsWritten);
        }

        [Benchmark]
        public void double_100e100_tostring_deciaml_swifter()
        {
            const double val = 100e100;

            char* chars = stackalloc char[str_length];

            NumberHelper.Hex.ToString(val, chars);
        }

        [Benchmark]
        public void double_100_tostring_deciaml_system()
        {
            const double val = 100;

            Span<char> span = stackalloc char[str_length];

            val.TryFormat(span, out var charsWritten);
        }

        [Benchmark]
        public void double_100_tostring_deciaml_swifter()
        {
            const double val = 100;

            char* chars = stackalloc char[str_length];

            NumberHelper.Hex.ToString(val, chars);
        }

        [Benchmark]
        public void float_100_tostring_deciaml_system()
        {
            const double val = 100;

            Span<char> span = stackalloc char[str_length];

            val.TryFormat(span, out var charsWritten);
        }

        [Benchmark]
        public void float_100_tostring_deciaml_swifter()
        {
            const double val = 100;

            char* chars = stackalloc char[str_length];

            NumberHelper.Hex.ToString(val, chars);
        }

        [Benchmark]
        public void double_n_100_tostring_deciaml_system()
        {
            const double val = -100;

            Span<char> span = stackalloc char[str_length];

            val.TryFormat(span, out var charsWritten);
        }

        [Benchmark]
        public void double_n_100_tostring_deciaml_swifter()
        {
            const double val = -100;

            char* chars = stackalloc char[str_length];

            NumberHelper.Hex.ToString(val, chars);
        }

        [Benchmark]
        public void float_n_100_tostring_deciaml_system()
        {
            const double val = -100;

            Span<char> span = stackalloc char[str_length];

            val.TryFormat(span, out var charsWritten);
        }

        [Benchmark]
        public void float_n_100_tostring_deciaml_swifter()
        {
            const double val = -100;

            char* chars = stackalloc char[str_length];

            NumberHelper.Hex.ToString(val, chars);
        }

        public static readonly char* c_1 = (char*)Underlying.AsPointer(ref StringHelper.GetRawStringData("1"));
        const int c_1_length = 1;
        public static readonly char* c_100 = (char*)Underlying.AsPointer(ref StringHelper.GetRawStringData("100"));
        const int c_100_length = 3;
        public static readonly char* c_1218 = (char*)Underlying.AsPointer(ref StringHelper.GetRawStringData("1218"));
        const int c_1218_length = 4;
        public static readonly char* c_999 = (char*)Underlying.AsPointer(ref StringHelper.GetRawStringData("999"));
        const int c_999_length = 3;
        public static readonly char* c_n_100 = (char*)Underlying.AsPointer(ref StringHelper.GetRawStringData("-100"));
        const int c_n_100_length = 4;
        public static readonly char* c_n_999 = (char*)Underlying.AsPointer(ref StringHelper.GetRawStringData("-999"));
        const int c_n_999_length = 4;
        public static readonly char* c_100e100 = (char*)Underlying.AsPointer(ref StringHelper.GetRawStringData("100e100"));
        const int c_100e100_length = 7;
        public static readonly char* c_99999999999999 = (char*)Underlying.AsPointer(ref StringHelper.GetRawStringData("99999999999999"));
        const int c_99999999999999_length = 14;

        [Benchmark]
        public void int64_1_parse_decimal_system()
        {
            long.TryParse(new ReadOnlySpan<char>(c_1, c_1_length), out long value);
        }

        [Benchmark]
        public void int64_1_parse_decimal_swifter()
        {
            NumberHelper.Decimal.ParseInt64(c_1, c_1_length);
        }

        [Benchmark]
        public void int64_100_parse_decimal_system()
        {
            long.TryParse(new ReadOnlySpan<char>(c_100, c_100_length), out long value);
        }

        [Benchmark]
        public void int64_100_parse_decimal_swifter()
        {
            NumberHelper.Decimal.ParseInt64(c_100, c_100_length);
        }

        [Benchmark]
        public void int64_1218_parse_decimal_system()
        {
            long.TryParse(new ReadOnlySpan<char>(c_1218, c_1218_length), out long value);
        }

        [Benchmark]
        public void int64_1218_parse_decimal_swifter()
        {
            NumberHelper.Decimal.ParseInt64(c_1218, c_1218_length);
        }

        [Benchmark]
        public void int64_n_999_parse_decimal_system()
        {
            long.TryParse(new ReadOnlySpan<char>(c_n_999, c_n_999_length), out long value);
        }

        [Benchmark]
        public void int64_n_999_parse_decimal_swifter()
        {
            NumberHelper.Decimal.ParseInt64(c_n_999, c_n_999_length);
        }

        [Benchmark]
        public void uint64_99999999999999_parse_decimal_system()
        {
            ulong.TryParse(new ReadOnlySpan<char>(c_99999999999999, c_99999999999999_length), out ulong value);
        }

        [Benchmark]
        public void uint64_99999999999999_parse_decimal_swifter()
        {
            NumberHelper.Decimal.ParseUInt64(c_99999999999999, c_99999999999999_length);
        }

        [Benchmark]
        public void double_99999999999999_parse_decimal_system()
        {
            double.TryParse(new ReadOnlySpan<char>(c_99999999999999, c_99999999999999_length), out double value);
        }

        [Benchmark]
        public void double_99999999999999_parse_decimal_swifter()
        {
            NumberHelper.Decimal.ParseDouble(c_99999999999999, c_99999999999999_length);
        }

        [Benchmark]
        public void double_100e100_parse_decimal_system()
        {
            double.TryParse(new ReadOnlySpan<char>(c_100e100, c_100e100_length), out double value);
        }

        [Benchmark]
        public void double_100e100_parse_decimal_swifter()
        {
            NumberHelper.Decimal.ParseDouble(c_100e100, c_100e100_length);
        }

        [Benchmark]
        public void double_n_100_parse_decimal_system()
        {
            double.TryParse(new ReadOnlySpan<char>(c_n_100, c_n_100_length), out double value);
        }

        [Benchmark]
        public void double_n_100_parse_decimal_swifter()
        {
            NumberHelper.Decimal.ParseDouble(c_n_100, c_n_100_length);
        }
    }
}

#endif