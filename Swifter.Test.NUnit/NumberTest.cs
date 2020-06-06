using NUnit.Framework;
using Swifter.Tools;
using System;

using static NUnit.Framework.Assert;

namespace Swifter.Test
{
    public unsafe class NumberTest
    {
        [Test]
        public void NumberInfoTest()
        {
            fixed (char* chars = &__("-18e+12", out var length))
            {
                var numberInfo = NumberHelper.GetNumberInfo(chars, length);

                AreEqual(numberInfo.ToString(), "-18E+12");

                AreEqual(numberInfo.ToDecimal(), (decimal)-18e+12);
                AreEqual(numberInfo.ToDouble(10), -18e+12);
                AreEqual(numberInfo.ToInt64(10), (long)-18e+12);
            }

            fixed (char* chars = &__("12e18", out var length))
            {
                var numberInfo = NumberHelper.GetNumberInfo(chars, length, 10);

                AreEqual(numberInfo.ToString(), "12E+18");

                AreEqual(numberInfo.ToDecimal(), (decimal)12e18);
                AreEqual(numberInfo.ToDouble(10), 12e18);
                AreEqual(numberInfo.ToUInt64(10), (ulong)12e18);
            }

            fixed (char* chars = &__("9999_9999_9999", out var length))
            {
                var numberInfo = NumberHelper.GetNumberInfo(chars, length);

                AreEqual(numberInfo.ToString(), "9999_9999_9999");

                AreEqual(numberInfo.ToDecimal(), (decimal)9999_9999_9999);
                AreEqual(numberInfo.ToDouble(10), (double)9999_9999_9999);
                AreEqual(numberInfo.ToUInt64(10), (ulong)9999_9999_9999);
                AreEqual(numberInfo.ToInt64(10), (long)9999_9999_9999);
            }

            fixed (char* chars = &__("009_9.9_900E+001_1", out var length))
            {
                var numberInfo = NumberHelper.GetNumberInfo(chars, length);

                AreEqual(numberInfo.ToString(), "9_9.9_9E+1_1");

                AreEqual(numberInfo.ToDecimal(), (decimal)99.99e11);
                AreEqual(numberInfo.ToDouble(10), (double)99.99e11);
                AreEqual(numberInfo.ToUInt64(10), (ulong)99.99e11);
                AreEqual(numberInfo.ToInt64(10), (long)99.99e11);
            }

            fixed (char* chars = &__("1.23e+2", out var length))
            {
                var numberInfo = NumberHelper.GetNumberInfo(chars, length);

                AreEqual(numberInfo.ToString(), "1.23E+2");

                AreEqual(numberInfo.ToDecimal(), (decimal)123);
                AreEqual(numberInfo.ToDouble(10), (double)123);
                AreEqual(numberInfo.ToUInt64(10), (ulong)123);
                AreEqual(numberInfo.ToInt64(10), (long)123);
            }

            fixed (char* chars = &__("12345e-2", out var length))
            {
                var numberInfo = NumberHelper.GetNumberInfo(chars, length);

                AreEqual(numberInfo.ToString(), "12345E-2");

                AreEqual(numberInfo.ToDecimal(), (decimal)12345e-2);
                AreEqual(numberInfo.ToDouble(10), (double)12345e-2);
                AreEqual(numberInfo.ToUInt64(10), (ulong)123);
                AreEqual(numberInfo.ToInt64(10), (long)123);
            }
        }

        [Test]
        public void BigIntegerTest()
        {
            var bigInteger = stackalloc uint[100];
            var bigIntegerLength = 0;

            fixed (char* chars = &__("1218121812181218121812181218121812181218", out var length))
            {
                bigIntegerLength = NumberHelper.Decimal.ParseBigInteger(chars, length, bigInteger).written;
            }

            NumberHelper.Mult(bigInteger, bigIntegerLength, uint.MaxValue, out var carry);

            if (carry != 0)
            {
                bigInteger[bigIntegerLength] = carry;

                ++bigIntegerLength;
            }

            AreEqual(carry, 3U);

            NumberHelper.Add(bigInteger, bigIntegerLength, 123, out carry);

            if (carry != 0)
            {
                bigInteger[bigIntegerLength] = carry;

                ++bigIntegerLength;
            }

            bigIntegerLength = NumberHelper.Sub(bigInteger, bigIntegerLength, 456, out var remainder);

            AreEqual(remainder, 0U);

            bigIntegerLength = NumberHelper.Div(bigInteger, bigIntegerLength, 753, out remainder);

            AreEqual(remainder, 210U);

            var str = stackalloc char[100];

            var strLength = NumberHelper.Decimal.ToString(bigInteger, bigIntegerLength, str);

            AreEqual(StringHelper.ToString(str, strLength), "6947932728611506568983591586250258491983961839");

            strLength = NumberHelper.Hex.ToString(bigInteger, bigIntegerLength, str);

            AreEqual(StringHelper.ToString(str, strLength), "1378e5b2e5219f9f248d9e89890dcc85a029eef");
        }

        [Test]
        public void GuidTest()
        {
            var guid = Guid.NewGuid();

            fixed (char* chars = &__(guid.ToString("N"), out var length))
            {
                AreEqual(NumberHelper.ParseGuid(chars, length).value, guid);
            }

            fixed (char* chars = &__(guid.ToString("D"), out var length))
            {
                AreEqual(NumberHelper.ParseGuid(chars, length).value, guid);
            }

            fixed (char* chars = &__(guid.ToString("B"), out var length))
            {
                AreEqual(NumberHelper.ParseGuid(chars, length).value, guid);
            }

            var str = stackalloc char[100];

            var strLength = NumberHelper.ToString(guid, str, true);

            AreEqual(Guid.Parse(StringHelper.ToString(str, strLength)), guid);

        }

        [Test]
        public void TdecimalTest()
        {
            AreEqual(NumberHelper.GetScale(0.123456789M), 9);

            fixed (char* chars = &__(decimal.MaxValue.ToString(), out var length))
            {
                AreEqual(NumberHelper.ParseDecimal(chars, length).value, decimal.MaxValue);
            }

            fixed (char* chars = &__(decimal.MinValue.ToString(), out var length))
            {
                AreEqual(NumberHelper.ParseDecimal(chars, length).value, decimal.MinValue);
            }

            var str = stackalloc char[100];

            var strLength = NumberHelper.ToString(decimal.MaxValue, str);

            AreEqual(StringHelper.ToString(str, strLength), decimal.MaxValue.ToString());

            strLength = NumberHelper.ToString(decimal.MinValue, str);

            AreEqual(StringHelper.ToString(str, strLength), decimal.MinValue.ToString());

            strLength = NumberHelper.ToString(decimal.MinusOne, str);

            AreEqual(StringHelper.ToString(str, strLength), decimal.MinusOne.ToString());

            decimal value = 2;

            try
            {
                for (long i = 0; i < 1000; value *= 2, i++)
                {
                    AreEqual(NumberHelper.ParseDecimal(str, NumberHelper.ToString(value, str)).value, value);
                }
            }
            catch (OverflowException)
            {

            }

            value = 10;

            try
            {
                for (long i = 0; i < 1000; value *= 10, i++)
                {
                    AreEqual(NumberHelper.ParseDecimal(str, NumberHelper.ToString(value, str)).value, value);
                }
            }
            catch (OverflowException)
            {

            }

            value = 9;

            try
            {
                for (long i = 0; i < 1000; value *= 9, i++)
                {
                    AreEqual(NumberHelper.ParseDecimal(str, NumberHelper.ToString(value, str)).value, value);
                }
            }
            catch (OverflowException)
            {

            }


            value = 7;

            try
            {
                for (long i = 0; i < 1000; value *= 7, i++)
                {
                    AreEqual(NumberHelper.ParseDecimal(str, NumberHelper.ToString(value, str)).value, value);
                }
            }
            catch (OverflowException)
            {

            }

        }

        [Test]
        public void DecimalTest()
        {
            fixed (char* chars = &__("3.1415926535897", out var length))
            {
                Less(Math.Abs(NumberHelper.Decimal.ParseDouble(chars, length).value / 3.1415926535897 - 1), 0.00000000000001);
            }

            fixed (char* chars = &__(ulong.MaxValue.ToString(), out var length))
            {
                AreEqual(NumberHelper.Decimal.ParseUInt64(chars, length).value, ulong.MaxValue);
            }

            fixed (char* chars = &__(ulong.MinValue.ToString(), out var length))
            {
                AreEqual(NumberHelper.Decimal.ParseUInt64(chars, length).value, ulong.MinValue);
            }

            fixed (char* chars = &__(long.MaxValue.ToString(), out var length))
            {
                AreEqual(NumberHelper.Decimal.ParseInt64(chars, length).value, long.MaxValue);
            }

            fixed (char* chars = &__(long.MinValue.ToString(), out var length))
            {
                AreEqual(NumberHelper.Decimal.ParseInt64(chars, length).value, long.MinValue);
            }


            var str = stackalloc char[100];

            AreEqual(StringHelper.ToString(str, NumberHelper.Decimal.ToString(3.1415926535897, str)), "3.1415926535897");
            AreEqual(StringHelper.ToString(str, NumberHelper.Decimal.ToString(3.1415F, str)), "3.1415");

            AreEqual(StringHelper.ToString(str, NumberHelper.Decimal.ToString(ulong.MaxValue, str)), ulong.MaxValue.ToString());
            AreEqual(StringHelper.ToString(str, NumberHelper.Decimal.ToString(long.MaxValue, str)), long.MaxValue.ToString());
            AreEqual(StringHelper.ToString(str, NumberHelper.Decimal.ToString(ulong.MinValue, str)), ulong.MinValue.ToString());
            AreEqual(StringHelper.ToString(str, NumberHelper.Decimal.ToString(long.MinValue, str)), long.MinValue.ToString());

            for (long i = 0, value = 2; i < 1000; value *= 2, i++)
            {
                AreEqual(NumberHelper.Decimal.ParseInt64(str, NumberHelper.Decimal.ToString(value, str)).value, value);
            }

            for (long i = 0, value = 10; i < 1000; value *= 10, i++)
            {
                AreEqual(NumberHelper.Decimal.ParseInt64(str, NumberHelper.Decimal.ToString(value, str)).value, value);
            }

            for (long i = 0, value = 9; i < 1000; value *= 9, i++)
            {
                AreEqual(NumberHelper.Decimal.ParseInt64(str, NumberHelper.Decimal.ToString(value, str)).value, value);
            }

            for (long i = 0, value = 7; i < 1000; value *= 7, i++)
            {
                AreEqual(NumberHelper.Decimal.ParseInt64(str, NumberHelper.Decimal.ToString(value, str)).value, value);
            }

            for (ulong i = 0, value = 2; i < 1000; value *= 2, i++)
            {
                AreEqual(NumberHelper.Decimal.ParseUInt64(str, NumberHelper.Decimal.ToString(value, str)).value, value);
            }

            for (ulong i = 0, value = 10; i < 1000; value *= 10, i++)
            {
                AreEqual(NumberHelper.Decimal.ParseUInt64(str, NumberHelper.Decimal.ToString(value, str)).value, value);
            }

            for (ulong i = 0, value = 9; i < 1000; value *= 9, i++)
            {
                AreEqual(NumberHelper.Decimal.ParseUInt64(str, NumberHelper.Decimal.ToString(value, str)).value, value);
            }

            for (ulong i = 0, value = 7; i < 1000; value *= 7, i++)
            {
                AreEqual(NumberHelper.Decimal.ParseUInt64(str, NumberHelper.Decimal.ToString(value, str)).value, value);
            }

            for (ulong i = 0, value = 3; i < 1000; value *= 3, i++)
            {
                var dval = Underlying.As<ulong, double>(ref value);

                if (dval >= double.MinValue && dval <= double.MaxValue && Math.Abs(dval) > 1e-300)
                {
                    
                    // AreAlmost(NumberHelper.Decimal.ParseDouble(str, NumberHelper.Decimal.ToString(dval, str)).value, dval);
                }
            }

            for (uint i = 0, value = 3; i < 1000; value *= 3, i++)
            {
                var fval = Underlying.As<uint, float>(ref value);

                if (fval >= float.MinValue && fval <= float.MaxValue && Math.Abs(fval) > 1e-30)
                {
                    // AreAlmost((float)NumberHelper.Decimal.ParseDouble(str, NumberHelper.Decimal.ToString(fval, str)).value, fval);
                }
            }

        }

        [Test]
        public void HexTest()
        {
            fixed (char* chars = &__("ff.ffe+f", out var length))
            {
                AreEqual(NumberHelper.Hex.ParseDouble(chars, length).value, 0xffff * Math.Pow(16, 0xd));
            }

            fixed (char* chars = &__("FFFFFFFFFFFFFFFF", out var length))
            {
                AreEqual(NumberHelper.Hex.ParseUInt64(chars, length).value, 0xFFFFFFFFFFFFFFFF);
            }

            fixed (char* chars = &__("0", out var length))
            {
                AreEqual(NumberHelper.Hex.ParseUInt64(chars, length).value, 0);
            }

            fixed (char* chars = &__("7FFFFFFFFFFFFFFF", out var length))
            {
                AreEqual(NumberHelper.Hex.ParseInt64(chars, length).value, 0x7FFFFFFFFFFFFFFF);
            }

            fixed (char* chars = &__("-8000000000000000", out var length))
            {
                AreEqual(NumberHelper.Hex.ParseInt64(chars, length).value, -0x8000000000000000);
            }

            var str = stackalloc char[100];

            AreEqual(StringHelper.ToString(str, NumberHelper.Hex.ToString(0x31415926535 * Math.Pow(16, -0xA), str)), "3.1415926535");
            AreEqual(StringHelper.ToString(str, NumberHelper.Hex.ToString(0x31415 * (float)Math.Pow(16, -0x4), str)), "3.1415");

            AreEqual(StringHelper.ToString(str, NumberHelper.Hex.ToString(ulong.MaxValue, str)), "ffffffffffffffff");
            AreEqual(StringHelper.ToString(str, NumberHelper.Hex.ToString(long.MaxValue, str)), "7fffffffffffffff");
            AreEqual(StringHelper.ToString(str, NumberHelper.Hex.ToString(ulong.MinValue, str)), "0");
            AreEqual(StringHelper.ToString(str, NumberHelper.Hex.ToString(long.MinValue, str)), "-8000000000000000");

            for (long i = 0, value = 2; i < 1000; value *= 2, i++)
            {
                AreEqual(NumberHelper.Hex.ParseInt64(str, NumberHelper.Hex.ToString(value, str)).value, value);
            }

            for (long i = 0, value = 10; i < 1000; value *= 10, i++)
            {
                AreEqual(NumberHelper.Hex.ParseInt64(str, NumberHelper.Hex.ToString(value, str)).value, value);
            }

            for (long i = 0, value = 9; i < 1000; value *= 9, i++)
            {
                AreEqual(NumberHelper.Hex.ParseInt64(str, NumberHelper.Hex.ToString(value, str)).value, value);
            }

            for (long i = 0, value = 7; i < 1000; value *= 7, i++)
            {
                AreEqual(NumberHelper.Hex.ParseInt64(str, NumberHelper.Hex.ToString(value, str)).value, value);
            }

            for (ulong i = 0, value = 2; i < 1000; value *= 2, i++)
            {
                AreEqual(NumberHelper.Hex.ParseUInt64(str, NumberHelper.Hex.ToString(value, str)).value, value);
            }

            for (ulong i = 0, value = 10; i < 1000; value *= 10, i++)
            {
                AreEqual(NumberHelper.Hex.ParseUInt64(str, NumberHelper.Hex.ToString(value, str)).value, value);
            }

            for (ulong i = 0, value = 9; i < 1000; value *= 9, i++)
            {
                AreEqual(NumberHelper.Hex.ParseUInt64(str, NumberHelper.Hex.ToString(value, str)).value, value);
            }

            for (ulong i = 0, value = 7; i < 1000; value *= 7, i++)
            {
                AreEqual(NumberHelper.Hex.ParseUInt64(str, NumberHelper.Hex.ToString(value, str)).value, value);
            }

            for (double i = 0, value = 3; i < 1000; value *= 3, i++)
            {
                AreEqual(NumberHelper.Hex.ParseDouble(str, NumberHelper.Hex.ToString(value, str)).value, value);
            }

            for (double i = 0, value = 3; i < 1000; value *= 3, i++)
            {
                AreEqual(NumberHelper.Binary.ParseDouble(str, NumberHelper.Binary.ToString(value, str)).value, value);
            }
        }

        static ref char __(string str, out int length)
        {
            length = str.Length;

            return ref StringHelper.GetRawStringData(str);
        }

    }
}
