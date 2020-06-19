using NUnit.Framework;
using Swifter.Json;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using static NUnit.Framework.Assert;

namespace Swifter.Test
{
    public class JsonTest
    {
        [Test]
        public void DoubleTest()
        {
            AreEqual(JsonFormatter.SerializeObject(0D), "0");
            AreEqual(JsonFormatter.SerializeObject(1D), "1");
            AreEqual(JsonFormatter.SerializeObject(10D), "10");
            AreEqual(JsonFormatter.SerializeObject(100D), "100");
            AreEqual(JsonFormatter.SerializeObject(1000D), "1000");
            AreEqual(JsonFormatter.SerializeObject(10000D), "10000");
            AreEqual(JsonFormatter.SerializeObject(100000D), "100000");
            AreEqual(JsonFormatter.SerializeObject(1000000D), "1000000");
            AreEqual(JsonFormatter.SerializeObject(10000000D), "10000000");
            AreEqual(JsonFormatter.SerializeObject(100000000D), "100000000");
            AreEqual(JsonFormatter.SerializeObject(1000000000D), "1000000000");
            AreEqual(JsonFormatter.SerializeObject(10000000000D), "10000000000");
            AreEqual(JsonFormatter.SerializeObject(100000000000D), "100000000000");
            AreEqual(JsonFormatter.SerializeObject(1000000000000D), "1000000000000");
            AreEqual(JsonFormatter.SerializeObject(10000000000000D), "10000000000000");
            AreEqual(JsonFormatter.SerializeObject(100000000000000D), "100000000000000");
            AreEqual(JsonFormatter.SerializeObject(1000000000000000D), "1000000000000000");
            AreEqual(JsonFormatter.SerializeObject(10000000000000000D), "10000000000000000");

            AreEqual(JsonFormatter.SerializeObject(9D), "9");
            AreEqual(JsonFormatter.SerializeObject(99D), "99");
            AreEqual(JsonFormatter.SerializeObject(999D), "999");
            AreEqual(JsonFormatter.SerializeObject(9999D), "9999");
            AreEqual(JsonFormatter.SerializeObject(99999D), "99999");
            AreEqual(JsonFormatter.SerializeObject(999999D), "999999");
            AreEqual(JsonFormatter.SerializeObject(9999999D), "9999999");
            AreEqual(JsonFormatter.SerializeObject(99999999D), "99999999");
            AreEqual(JsonFormatter.SerializeObject(999999999D), "999999999");
            AreEqual(JsonFormatter.SerializeObject(9999999999D), "9999999999");
            AreEqual(JsonFormatter.SerializeObject(99999999999D), "99999999999");
            AreEqual(JsonFormatter.SerializeObject(999999999999D), "999999999999");
            AreEqual(JsonFormatter.SerializeObject(9999999999999D), "9999999999999");
            AreEqual(JsonFormatter.SerializeObject(99999999999999D), "99999999999999");
            AreEqual(JsonFormatter.SerializeObject(999999999999999D), "999999999999999");

            AreEqual(JsonFormatter.SerializeObject(-0D), "0");
            AreEqual(JsonFormatter.SerializeObject(-1D), "-1");
            AreEqual(JsonFormatter.SerializeObject(-10D), "-10");
            AreEqual(JsonFormatter.SerializeObject(-100D), "-100");
            AreEqual(JsonFormatter.SerializeObject(-1000D), "-1000");
            AreEqual(JsonFormatter.SerializeObject(-10000D), "-10000");
            AreEqual(JsonFormatter.SerializeObject(-100000D), "-100000");
            AreEqual(JsonFormatter.SerializeObject(-1000000D), "-1000000");
            AreEqual(JsonFormatter.SerializeObject(-10000000D), "-10000000");
            AreEqual(JsonFormatter.SerializeObject(-100000000D), "-100000000");
            AreEqual(JsonFormatter.SerializeObject(-1000000000D), "-1000000000");
            AreEqual(JsonFormatter.SerializeObject(-10000000000D), "-10000000000");
            AreEqual(JsonFormatter.SerializeObject(-100000000000D), "-100000000000");
            AreEqual(JsonFormatter.SerializeObject(-1000000000000D), "-1000000000000");
            AreEqual(JsonFormatter.SerializeObject(-10000000000000D), "-10000000000000");
            AreEqual(JsonFormatter.SerializeObject(-100000000000000D), "-100000000000000");
            AreEqual(JsonFormatter.SerializeObject(-1000000000000000D), "-1000000000000000");
            AreEqual(JsonFormatter.SerializeObject(-10000000000000000D), "-10000000000000000");

            AreEqual(JsonFormatter.SerializeObject(-9D), "-9");
            AreEqual(JsonFormatter.SerializeObject(-99D), "-99");
            AreEqual(JsonFormatter.SerializeObject(-999D), "-999");
            AreEqual(JsonFormatter.SerializeObject(-9999D), "-9999");
            AreEqual(JsonFormatter.SerializeObject(-99999D), "-99999");
            AreEqual(JsonFormatter.SerializeObject(-999999D), "-999999");
            AreEqual(JsonFormatter.SerializeObject(-9999999D), "-9999999");
            AreEqual(JsonFormatter.SerializeObject(-99999999D), "-99999999");
            AreEqual(JsonFormatter.SerializeObject(-999999999D), "-999999999");
            AreEqual(JsonFormatter.SerializeObject(-9999999999D), "-9999999999");
            AreEqual(JsonFormatter.SerializeObject(-99999999999D), "-99999999999");
            AreEqual(JsonFormatter.SerializeObject(-999999999999D), "-999999999999");
            AreEqual(JsonFormatter.SerializeObject(-9999999999999D), "-9999999999999");
            AreEqual(JsonFormatter.SerializeObject(-99999999999999D), "-99999999999999");
            AreEqual(JsonFormatter.SerializeObject(-999999999999999D), "-999999999999999");


            AreEqual(JsonFormatter.SerializeObject(1e50), "1E+50");
            AreEqual(JsonFormatter.SerializeObject(1e100), "1E+100");
            AreEqual(JsonFormatter.SerializeObject(1e-50), "1E-50");
            AreEqual(JsonFormatter.SerializeObject(1e-100), "1E-100");

            AreEqual(JsonFormatter.SerializeObject(-1e50), "-1E+50");
            AreEqual(JsonFormatter.SerializeObject(-1e100), "-1E+100");
            AreEqual(JsonFormatter.SerializeObject(-1e-50), "-1E-50");
            AreEqual(JsonFormatter.SerializeObject(-1e-100), "-1E-100");



            AreEqual(JsonFormatter.SerializeObject(3.1415926), "3.1415926");
            AreEqual(JsonFormatter.SerializeObject(3.1415926e7), "31415926");
            AreEqual(JsonFormatter.SerializeObject(3.14159265358979), "3.14159265358979");


            AreEqual(JsonFormatter.SerializeObject(4e-9), "4E-9");
            AreEqual(JsonFormatter.SerializeObject(4e-17), "4E-17");
            AreEqual(JsonFormatter.SerializeObject(4e-18), "4E-18");
            AreEqual(JsonFormatter.SerializeObject(4e-19), "4E-19");
        }

        [Test]
        public void SingleTest()
        {
            AreEqual(JsonFormatter.SerializeObject(0F), "0");
            AreEqual(JsonFormatter.SerializeObject(1F), "1");
            AreEqual(JsonFormatter.SerializeObject(10F), "10");
            AreEqual(JsonFormatter.SerializeObject(100F), "100");
            AreEqual(JsonFormatter.SerializeObject(1000F), "1000");
            AreEqual(JsonFormatter.SerializeObject(10000F), "10000");
            AreEqual(JsonFormatter.SerializeObject(100000F), "100000");
            AreEqual(JsonFormatter.SerializeObject(1000000F), "1000000");
            AreEqual(JsonFormatter.SerializeObject(10000000F), "10000000");

            AreEqual(JsonFormatter.SerializeObject(9F), "9");
            AreEqual(JsonFormatter.SerializeObject(99F), "99");
            AreEqual(JsonFormatter.SerializeObject(999F), "999");
            AreEqual(JsonFormatter.SerializeObject(9999F), "9999");
            AreEqual(JsonFormatter.SerializeObject(99999F), "99999");
            AreEqual(JsonFormatter.SerializeObject(999999F), "999999");
            AreEqual(JsonFormatter.SerializeObject(9999999F), "9999999");

            AreEqual(JsonFormatter.SerializeObject(-0F), "0");
            AreEqual(JsonFormatter.SerializeObject(-1F), "-1");
            AreEqual(JsonFormatter.SerializeObject(-10F), "-10");
            AreEqual(JsonFormatter.SerializeObject(-100F), "-100");
            AreEqual(JsonFormatter.SerializeObject(-1000F), "-1000");
            AreEqual(JsonFormatter.SerializeObject(-10000F), "-10000");
            AreEqual(JsonFormatter.SerializeObject(-100000F), "-100000");
            AreEqual(JsonFormatter.SerializeObject(-1000000F), "-1000000");
            AreEqual(JsonFormatter.SerializeObject(-10000000F), "-10000000");

            AreEqual(JsonFormatter.SerializeObject(-9F), "-9");
            AreEqual(JsonFormatter.SerializeObject(-99F), "-99");
            AreEqual(JsonFormatter.SerializeObject(-999F), "-999");
            AreEqual(JsonFormatter.SerializeObject(-9999F), "-9999");
            AreEqual(JsonFormatter.SerializeObject(-99999F), "-99999");
            AreEqual(JsonFormatter.SerializeObject(-999999F), "-999999");
            AreEqual(JsonFormatter.SerializeObject(-9999999F), "-9999999");


            AreEqual(JsonFormatter.SerializeObject(3.14159F), "3.14159");
            AreEqual(JsonFormatter.SerializeObject(3.14e2F), "314");


            AreEqual(JsonFormatter.SerializeObject(4e9F), "4E+9");
            AreEqual(JsonFormatter.SerializeObject(4e17F), "4E+17");
            AreEqual(JsonFormatter.SerializeObject(4e18F), "4E+18");
            AreEqual(JsonFormatter.SerializeObject(4e19F), "4E+19");

            AreEqual(JsonFormatter.SerializeObject(4e-9F), "4E-9");
            AreEqual(JsonFormatter.SerializeObject(4e-17F), "4E-17");
            AreEqual(JsonFormatter.SerializeObject(4e-18F), "4E-18");
            AreEqual(JsonFormatter.SerializeObject(4e-19F), "4E-19");
        }

        [Test]
        public void IntegerTest()
        {
            AreEqual(JsonFormatter.SerializeObject(0), "0");
            AreEqual(JsonFormatter.SerializeObject(1), "1");
            AreEqual(JsonFormatter.SerializeObject(10), "10");
            AreEqual(JsonFormatter.SerializeObject(100), "100");
            AreEqual(JsonFormatter.SerializeObject(1000), "1000");
            AreEqual(JsonFormatter.SerializeObject(10000), "10000");
            AreEqual(JsonFormatter.SerializeObject(100000), "100000");
            AreEqual(JsonFormatter.SerializeObject(1000000), "1000000");
            AreEqual(JsonFormatter.SerializeObject(10000000), "10000000");
            AreEqual(JsonFormatter.SerializeObject(100000000), "100000000");
            AreEqual(JsonFormatter.SerializeObject(1000000000), "1000000000");

            AreEqual(JsonFormatter.SerializeObject(10000000000), "10000000000");
            AreEqual(JsonFormatter.SerializeObject(100000000000), "100000000000");
            AreEqual(JsonFormatter.SerializeObject(1000000000000), "1000000000000");
            AreEqual(JsonFormatter.SerializeObject(10000000000000), "10000000000000");
            AreEqual(JsonFormatter.SerializeObject(100000000000000), "100000000000000");
            AreEqual(JsonFormatter.SerializeObject(1000000000000000), "1000000000000000");
            AreEqual(JsonFormatter.SerializeObject(10000000000000000), "10000000000000000");
            AreEqual(JsonFormatter.SerializeObject(100000000000000000), "100000000000000000");
            AreEqual(JsonFormatter.SerializeObject(1000000000000000000), "1000000000000000000");
            AreEqual(JsonFormatter.SerializeObject(10000000000000000000), "10000000000000000000");

            AreEqual(JsonFormatter.SerializeObject(9), "9");
            AreEqual(JsonFormatter.SerializeObject(99), "99");
            AreEqual(JsonFormatter.SerializeObject(999), "999");
            AreEqual(JsonFormatter.SerializeObject(9999), "9999");
            AreEqual(JsonFormatter.SerializeObject(99999), "99999");
            AreEqual(JsonFormatter.SerializeObject(999999), "999999");
            AreEqual(JsonFormatter.SerializeObject(9999999), "9999999");
            AreEqual(JsonFormatter.SerializeObject(99999999), "99999999");
            AreEqual(JsonFormatter.SerializeObject(999999999), "999999999");
            AreEqual(JsonFormatter.SerializeObject(9999999999), "9999999999");

            AreEqual(JsonFormatter.SerializeObject(99999999999), "99999999999");
            AreEqual(JsonFormatter.SerializeObject(999999999999), "999999999999");
            AreEqual(JsonFormatter.SerializeObject(9999999999999), "9999999999999");
            AreEqual(JsonFormatter.SerializeObject(99999999999999), "99999999999999");
            AreEqual(JsonFormatter.SerializeObject(999999999999999), "999999999999999");
            AreEqual(JsonFormatter.SerializeObject(9999999999999999), "9999999999999999");
            AreEqual(JsonFormatter.SerializeObject(99999999999999999), "99999999999999999");
            AreEqual(JsonFormatter.SerializeObject(999999999999999999), "999999999999999999");
            AreEqual(JsonFormatter.SerializeObject(9999999999999999999), "9999999999999999999");

            AreEqual(JsonFormatter.SerializeObject(-1), "-1");
            AreEqual(JsonFormatter.SerializeObject(-10), "-10");
            AreEqual(JsonFormatter.SerializeObject(-100), "-100");
            AreEqual(JsonFormatter.SerializeObject(-1000), "-1000");
            AreEqual(JsonFormatter.SerializeObject(-10000), "-10000");
            AreEqual(JsonFormatter.SerializeObject(-100000), "-100000");
            AreEqual(JsonFormatter.SerializeObject(-1000000), "-1000000");
            AreEqual(JsonFormatter.SerializeObject(-10000000), "-10000000");
            AreEqual(JsonFormatter.SerializeObject(-100000000), "-100000000");
            AreEqual(JsonFormatter.SerializeObject(-1000000000), "-1000000000");

            AreEqual(JsonFormatter.SerializeObject(-10000000000), "-10000000000");
            AreEqual(JsonFormatter.SerializeObject(-100000000000), "-100000000000");
            AreEqual(JsonFormatter.SerializeObject(-1000000000000), "-1000000000000");
            AreEqual(JsonFormatter.SerializeObject(-10000000000000), "-10000000000000");
            AreEqual(JsonFormatter.SerializeObject(-100000000000000), "-100000000000000");
            AreEqual(JsonFormatter.SerializeObject(-1000000000000000), "-1000000000000000");
            AreEqual(JsonFormatter.SerializeObject(-10000000000000000), "-10000000000000000");
            AreEqual(JsonFormatter.SerializeObject(-100000000000000000), "-100000000000000000");
            AreEqual(JsonFormatter.SerializeObject(-1000000000000000000), "-1000000000000000000");

            AreEqual(JsonFormatter.SerializeObject(-9), "-9");
            AreEqual(JsonFormatter.SerializeObject(-99), "-99");
            AreEqual(JsonFormatter.SerializeObject(-999), "-999");
            AreEqual(JsonFormatter.SerializeObject(-9999), "-9999");
            AreEqual(JsonFormatter.SerializeObject(-99999), "-99999");
            AreEqual(JsonFormatter.SerializeObject(-999999), "-999999");
            AreEqual(JsonFormatter.SerializeObject(-9999999), "-9999999");
            AreEqual(JsonFormatter.SerializeObject(-99999999), "-99999999");
            AreEqual(JsonFormatter.SerializeObject(-999999999), "-999999999");
            AreEqual(JsonFormatter.SerializeObject(-9999999999), "-9999999999");

            AreEqual(JsonFormatter.SerializeObject(-99999999999), "-99999999999");
            AreEqual(JsonFormatter.SerializeObject(-999999999999), "-999999999999");
            AreEqual(JsonFormatter.SerializeObject(-9999999999999), "-9999999999999");
            AreEqual(JsonFormatter.SerializeObject(-99999999999999), "-99999999999999");
            AreEqual(JsonFormatter.SerializeObject(-999999999999999), "-999999999999999");
            AreEqual(JsonFormatter.SerializeObject(-9999999999999999), "-9999999999999999");
            AreEqual(JsonFormatter.SerializeObject(-99999999999999999), "-99999999999999999");
            AreEqual(JsonFormatter.SerializeObject(-999999999999999999), "-999999999999999999");


            AreEqual(JsonFormatter.SerializeObject(ulong.MaxValue), "18446744073709551615");
            AreEqual(JsonFormatter.SerializeObject(uint.MaxValue), "4294967295");
            AreEqual(JsonFormatter.SerializeObject(ushort.MaxValue), "65535");
            AreEqual(JsonFormatter.SerializeObject(byte.MaxValue), "255");

            AreEqual(JsonFormatter.SerializeObject(long.MaxValue), "9223372036854775807");
            AreEqual(JsonFormatter.SerializeObject(long.MinValue), "-9223372036854775808");
            AreEqual(JsonFormatter.SerializeObject(int.MaxValue), "2147483647");
            AreEqual(JsonFormatter.SerializeObject(int.MinValue), "-2147483648");
            AreEqual(JsonFormatter.SerializeObject(short.MaxValue), "32767");
            AreEqual(JsonFormatter.SerializeObject(short.MinValue), "-32768");
            AreEqual(JsonFormatter.SerializeObject(sbyte.MaxValue), "127");
            AreEqual(JsonFormatter.SerializeObject(sbyte.MinValue), "-128");

            AreEqual(JsonFormatter.DeserializeObject<ulong>("1e10"), Convert.ToUInt64(1e10));
            AreEqual(JsonFormatter.DeserializeObject<ulong>("1.123e10"), 11230000000UL);
            AreEqual(JsonFormatter.DeserializeObject<ulong>("1_0_0_0"), 1000UL);
            AreEqual(JsonFormatter.DeserializeObject<long>("1e10"), Convert.ToInt64(1e10));
            AreEqual(JsonFormatter.DeserializeObject<long>("1.123e10"), 11230000000U);


        }

        [Test]
        public void BooleanTest()
        {
            AreEqual(JsonFormatter.SerializeObject(true), "true");
            AreEqual(JsonFormatter.SerializeObject(false), "false");
            AreEqual(JsonFormatter.DeserializeObject<bool>("true"), true);
            AreEqual(JsonFormatter.DeserializeObject<bool>("false"), false);
            AreEqual(JsonFormatter.DeserializeObject<bool>("TRUE"), true);
            AreEqual(JsonFormatter.DeserializeObject<bool>("FALSE"), false);
            AreEqual(JsonFormatter.DeserializeObject<int>("FALSE"), 0);
            AreEqual(JsonFormatter.DeserializeObject<int>("TRUE"), 1);
            AreEqual(JsonFormatter.DeserializeObject<bool>("1"), true);
            AreEqual(JsonFormatter.DeserializeObject<bool>("0"), false);
        }

        [Test]
        public void StringTest()
        {
            AreEqual(JsonFormatter.SerializeObject(""), "\"\"");
            AreEqual(JsonFormatter.SerializeObject("狗伟abc"), "\"狗伟abc\"");
            AreEqual(JsonFormatter.SerializeObject("\n\r\t\b\f\""), "\"\\n\\r\\t\\b\\f\\\"\"");
            AreEqual(JsonFormatter.SerializeObject("a\nb\rc\td\be\ff\"g"), "\"a\\nb\\rc\\td\\be\\ff\\\"g\"");
            AreEqual(JsonFormatter.SerializeObject("👨‍👩‍👧‍👧👨‍🎨"), "\"👨‍👩‍👧‍👧👨‍🎨\"");

            AreEqual(JsonFormatter.DeserializeObject<string>("\"\""), "");
            AreEqual(JsonFormatter.DeserializeObject<string>("\"狗伟abc\""), "狗伟abc");
            AreEqual(JsonFormatter.DeserializeObject<string>("\"\\n\\r\\t\\b\\f\\\"\""), "\n\r\t\b\f\"");
            AreEqual(JsonFormatter.DeserializeObject<string>("\"a\\nb\\rc\\td\\be\\ff\\\"g\""), "a\nb\rc\td\be\ff\"g");
            AreEqual(JsonFormatter.DeserializeObject<string>("\"👨‍👩‍👧‍👧👨‍🎨\""), "👨‍👩‍👧‍👧👨‍🎨");

            AreEqual(JsonFormatter.DeserializeObject<string>("123"), "123");
            AreEqual(JsonFormatter.DeserializeObject<string>("-1.23e4"), "-1.23E+4");
        }

        [Test]
        public void DateTimeTest()
        {
            var dtn = DateTime.Parse("2020-02-02 20:00:02");
            var dtsn = DateTimeOffset.Parse("2020-02-02 20:00:02.202+02:20");
            var tsn = TimeSpan.Parse("12:18:00");

            AreEqual(JsonFormatter.DeserializeObject<DateTime>(JsonFormatter.SerializeObject(dtn)), dtn);
            AreEqual(JsonFormatter.DeserializeObject<DateTimeOffset>(JsonFormatter.SerializeObject(dtsn)), dtsn);

            AreEqual(JsonFormatter.DeserializeObject<TimeSpan>(JsonFormatter.SerializeObject(tsn)), tsn);
            AreEqual(JsonFormatter.SerializeObject(tsn), JsonFormatter.SerializeObject(tsn.ToString()));

            AreEqual(JsonFormatter.SerializeObject(DateTimeOffset.Parse("1997-12-18 21:50:20.999+08:00")), JsonFormatter.SerializeObject("1997-12-18T21:50:20.999+08:00"));

            var jsonFormatter = new JsonFormatter();

            jsonFormatter.SetDateTimeFormat("yyyy-MM-dd HH:mm:ss");

            AreEqual(jsonFormatter.Serialize(dtn), "\"2020-02-02 20:00:02\"");

        }

        [Test]
        public void GuidTest()
        {
            var gn = Guid.NewGuid();

            AreEqual(JsonFormatter.SerializeObject(gn), JsonFormatter.SerializeObject(gn.ToString("D")));
            AreEqual(JsonFormatter.DeserializeObject<Guid>(JsonFormatter.SerializeObject(gn)), gn);
        }

        [Test]
        public void ObjectTest()
        {
            var obj = new { Id = 123, Name = "Dogwei" };

            AreEqual(JsonFormatter.SerializeObject(obj), "{\"Id\":123,\"Name\":\"Dogwei\"}");

            dynamic deo = JsonFormatter.DeserializeObject(JsonFormatter.SerializeObject(obj), obj.GetType());

            Assert.AreEqual(deo.Id, obj.Id);
            Assert.AreEqual(deo.Name, obj.Name);
            AreEqual(((object)deo).GetType(), obj.GetType());

        }

        [Test]
        public void ArrayTest()
        {
            AreEqual(JsonFormatter.SerializeObject(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }), "[0,1,2,3,4,5,6,7,8,9]");

            AreEqual(JsonFormatter.SerializeObject(JsonFormatter.DeserializeObject<int[]>(JsonFormatter.SerializeObject(new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 }))), "[0,1,2,3,4,5,6,7,8,9]");

        }

        [Test]
        public void WriterTest()
        {
            var hGCache = new HGlobalCache<char>();

            var jsonWriter = JsonFormatter.CreateJsonWriter(hGCache);

            jsonWriter.WriteBeginObject();

            jsonWriter.WritePropertyName("Id");

            jsonWriter.WriteInt32(123);

            jsonWriter.WritePropertyName("Name");

            jsonWriter.WriteString("Dogwei");

            jsonWriter.WriteEndObject();

            jsonWriter.Flush();

            var json = hGCache.ToStringEx();

            dynamic dyc = JsonFormatter.DeserializeObject<JsonValue>(json);

            Assert.AreEqual(dyc.Id, 123);
            Assert.AreEqual(dyc.Name, "Dogwei");

            jsonWriter.Clear();

            jsonWriter.WriteBeginArray();

            jsonWriter.WriteInt32(1);
            jsonWriter.WriteInt32(2);
            jsonWriter.WriteInt32(3);

            jsonWriter.WriteEndArray();

            jsonWriter.Flush();

            AreEqual(hGCache.ToStringEx(), "[1,2,3]");
        }

        [Test]
        public void ReaderTest()
        {
            var hGCache = new HGlobalCache<char>();

            JsonFormatter.SerializeObject(new object[] { 123, "Dogwei", true, null, new { Id = 123 } }, hGCache);

            var jsonReader = JsonFormatter.CreateJsonReader(hGCache);

            AreEqual(jsonReader.GetToken(), JsonToken.Array);
            AreEqual(jsonReader.TryReadBeginArray(), true);

            AreEqual(jsonReader.TryReadEndArray(), false);
            AreEqual(jsonReader.GetToken(), JsonToken.Number);
            AreEqual(jsonReader.ReadInt32(), 123);

            AreEqual(jsonReader.TryReadEndArray(), false);
            AreEqual(jsonReader.GetToken(), JsonToken.String);
            AreEqual(jsonReader.ReadString(), "Dogwei");

            AreEqual(jsonReader.TryReadEndArray(), false);
            AreEqual(jsonReader.GetToken(), JsonToken.Boolean);
            AreEqual(jsonReader.ReadBoolean(), true);

            AreEqual(jsonReader.TryReadEndArray(), false);
            AreEqual(jsonReader.GetToken(), JsonToken.Null);
            AreEqual(jsonReader.DirectRead(), null);

            AreEqual(jsonReader.TryReadEndArray(), false);
            AreEqual(jsonReader.GetToken(), JsonToken.Object);
            AreEqual(JsonFormatter.SerializeObject(ValueInterface<Dictionary<string, object>>.ReadValue(jsonReader)), "{\"Id\":123}");

            AreEqual(jsonReader.TryReadEndArray(), true);
            AreEqual(jsonReader.GetToken(), JsonToken.End);
        }

        [Test]
        public void DeserializeToTest()
        {
            var obj = new { Id = 0, Name = "" };

            var jsonFormatter = new JsonFormatter();

            jsonFormatter.DeserializeTo("{\"Id\":123,\"Name\":\"Dogwei\"}", RWHelper.CreateWriter(obj));

            AreEqual(obj.Id, 123);
            AreEqual(obj.Name, "Dogwei");

            var arr = new int[3];

            jsonFormatter.DeserializeTo("[1,2,3]", RWHelper.CreateWriter(arr));

            AreEqual(arr[0], 1);
            AreEqual(arr[1], 2);
            AreEqual(arr[2], 3);
        }

        [Test]
        public void IndentedTest()
        {
            var jsonFormatter = new JsonFormatter(JsonFormatterOptions.Indented) { IndentedChars = " ", MiddleChars = "  ", LineBreakChars = "   " };

            AreEqual(jsonFormatter.Serialize(new { Id = 123, Name = "Dogwei" }), "{    \"Id\":  123,    \"Name\":  \"Dogwei\"   }");

            AreEqual(jsonFormatter.Serialize(new int[] { 1, 2, 3 }), "[    1,    2,    3   ]");
        }

        [Test]
        public void IgnoreNullTest()
        {
            var obj = new { Id = 123, Name = (string)null };
            var arr = new object[] { 123, null };

            AreEqual(JsonFormatter.SerializeObject(obj, JsonFormatterOptions.IgnoreNull), "{\"Id\":123}");
            AreEqual(JsonFormatter.SerializeObject(arr, JsonFormatterOptions.IgnoreNull), "[123,null]");
            AreEqual(JsonFormatter.SerializeObject(arr, JsonFormatterOptions.IgnoreNull | JsonFormatterOptions.ArrayOnFilter), "[123]");
        }

        [Test]
        public void IgnoreZeroTest()
        {
            var obj = new { Id = 123, Zero = 0 };
            var arr = new object[] { 123, 0 };

            AreEqual(JsonFormatter.SerializeObject(obj, JsonFormatterOptions.IgnoreZero), "{\"Id\":123}");
            AreEqual(JsonFormatter.SerializeObject(arr, JsonFormatterOptions.IgnoreZero), "[123,0]");
            AreEqual(JsonFormatter.SerializeObject(arr, JsonFormatterOptions.IgnoreZero | JsonFormatterOptions.ArrayOnFilter), "[123]");
        }

        [Test]
        public void IgnoreEmptyStringTest()
        {
            var obj = new { Id = 123, EmptyString = "" };
            var arr = new object[] { 123, "" };

            AreEqual(JsonFormatter.SerializeObject(obj, JsonFormatterOptions.IgnoreEmptyString), "{\"Id\":123}");
            AreEqual(JsonFormatter.SerializeObject(arr, JsonFormatterOptions.IgnoreEmptyString), "[123,\"\"]");
            AreEqual(JsonFormatter.SerializeObject(arr, JsonFormatterOptions.IgnoreEmptyString | JsonFormatterOptions.ArrayOnFilter), "[123]");
        }

        [Test]
        public void IgnoreEmptyStringOrZeroOrNullTest()
        {
            var obj = new { Id = 123, EmptyString = "", Null = (string)null, Zero = 0, One = 1 };
            var arr = new object[] { 123, "", null, 0, 1 };

            AreEqual(JsonFormatter.SerializeObject(obj, JsonFormatterOptions.IgnoreEmptyString | JsonFormatterOptions.IgnoreZero | JsonFormatterOptions.IgnoreNull), "{\"Id\":123,\"One\":1}");
            AreEqual(JsonFormatter.SerializeObject(arr, JsonFormatterOptions.IgnoreEmptyString | JsonFormatterOptions.IgnoreZero | JsonFormatterOptions.IgnoreNull), "[123,\"\",null,0,1]");
            AreEqual(JsonFormatter.SerializeObject(arr, JsonFormatterOptions.IgnoreEmptyString | JsonFormatterOptions.IgnoreZero | JsonFormatterOptions.IgnoreNull | JsonFormatterOptions.ArrayOnFilter), "[123,1]");
        }

        [Test]
        public void ReferencingTest()
        {
            var obj = new { Id = 123, Name = "Dogwei", Obj = (object)null };
            var arr = new object[2];

            arr[0] = new { Id = 123 };
            arr[1] = arr[0];

            RWHelper.CreateRW(obj).As<string>()["Obj"].DirectWrite(obj);

            AreEqual(
                JsonFormatter.SerializeObject(obj, JsonFormatterOptions.LoopReferencingNull),
                "{\"Id\":123,\"Name\":\"Dogwei\",\"Obj\":null}"
                );

            AreEqual(
                JsonFormatter.SerializeObject(obj, JsonFormatterOptions.MultiReferencingNull),
                "{\"Id\":123,\"Name\":\"Dogwei\",\"Obj\":null}"
                );

            AreEqual(
                JsonFormatter.SerializeObject(obj, JsonFormatterOptions.MultiReferencingReference),
                "{\"Id\":123,\"Name\":\"Dogwei\",\"Obj\":{\"$ref\":\"#\"}}"
                );

            Catch<JsonOutOfDepthException>(() => JsonFormatter.SerializeObject(obj));
            Catch<JsonLoopReferencingException>(() => JsonFormatter.SerializeObject(obj, JsonFormatterOptions.LoopReferencingException));

            AreEqual(
                JsonFormatter.SerializeObject(arr, JsonFormatterOptions.LoopReferencingNull),
                "[{\"Id\":123},{\"Id\":123}]"
                );

            AreEqual(
                JsonFormatter.SerializeObject(arr, JsonFormatterOptions.MultiReferencingNull),
                "[{\"Id\":123},null]"
                );

            AreEqual(
                JsonFormatter.SerializeObject(arr, JsonFormatterOptions.MultiReferencingReference),
                "[{\"Id\":123},{\"$ref\":\"#/0\"}]"
                );

            JsonFormatter.SerializeObject(arr);
            JsonFormatter.SerializeObject(arr, JsonFormatterOptions.LoopReferencingException);

            AreEqual(
                JsonFormatter.SerializeObject(ValueInterface<DataTable>.ReadValue(ValueCopyer.ValueOf(arr)), JsonFormatterOptions.MultiReferencingNull),
                "[{\"Id\":123},{\"Id\":123}]"
                );

            AreEqual(
                JsonFormatter.SerializeObject(ValueInterface<DataTable>.ReadValue(ValueCopyer.ValueOf(arr)).CreateDataReader(), JsonFormatterOptions.MultiReferencingNull),
                "[{\"Id\":123},{\"Id\":123}]"
                );

            AreEqual(
                JsonFormatter.SerializeObject(Enumerable.Range(0, 10), JsonFormatterOptions.MultiReferencingNull),
                "[0,1,2,3,4,5,6,7,8,9]"
                );

            var mar = new object[,] { { arr[0], arr[0] }, { arr[0], arr[0] } };

            AreEqual(
                JsonFormatter.SerializeObject(mar, JsonFormatterOptions.LoopReferencingNull),
                "[[{\"Id\":123},{\"Id\":123}],[{\"Id\":123},{\"Id\":123}]]"
                );

            AreEqual(
                JsonFormatter.SerializeObject(mar, JsonFormatterOptions.MultiReferencingNull),
                "[[{\"Id\":123},null],[null,null]]"
                );

            AreEqual(
                JsonFormatter.SerializeObject(mar, JsonFormatterOptions.MultiReferencingReference),
                "[[{\"Id\":123},{\"$ref\":\"#/0/0\"}],[{\"$ref\":\"#/0/0\"},{\"$ref\":\"#/0/0\"}]]"
                );

            JsonFormatter.SerializeObject(mar);
            JsonFormatter.SerializeObject(mar, JsonFormatterOptions.LoopReferencingException);

            var dic2 = JsonFormatter.DeserializeObject<object[]>("[{\"Id\":123},{\"$ref\":\"#/0\"}]");

            AreEqual(JsonFormatter.SerializeObject(dic2[1]), "{\"$ref\":\"#/0\"}");

            var mar2 = JsonFormatter.DeserializeObject<object[,]>("[[{\"Id\":123},{\"$ref\":\"#/0/0\"}],[{\"$ref\":\"#/0/0\"},{\"$ref\":\"#/0/0\"}]]", JsonFormatterOptions.MultiReferencingReference);

            AreEqual(mar2[0, 0], mar2[0, 1]);
            AreEqual(mar2[1, 0], mar2[0, 1]);
            AreEqual(mar2[1, 0], mar2[1, 1]);
        }

        [Test]
        public void EnumTest()
        {
            var jsonFormatter = new JsonFormatter();

            AreEqual(
                jsonFormatter.Serialize(JsonFormatterOptions.ArrayOnFilter), 
                JsonFormatter.SerializeObject(JsonFormatterOptions.ArrayOnFilter.ToString())
                );

            AreEqual(
                jsonFormatter.Serialize(JsonFormatterOptions.ArrayOnFilter | JsonFormatterOptions.DeflateDeserialize),
                JsonFormatter.SerializeObject((JsonFormatterOptions.ArrayOnFilter | JsonFormatterOptions.DeflateDeserialize).ToString())
                );
        }

        [Test]
        public void NestingTest()
        {
            var obj = new NestingObject();
            var jsonFormatter = new JsonFormatter();

            AreEqual(
                JsonFormatter.SerializeObject(new { Id = 123, Name = "Dogwei", Json = JsonFormatter.SerializeObject(new { Id = 123, Name = "Dogwei" }) }),
                jsonFormatter.Serialize(obj));

            dynamic dya = jsonFormatter.Deserialize<NestingObject>(jsonFormatter.Serialize(obj)).Json;

            AreEqual(123, dya["Id"]);
            AreEqual("Dogwei", dya["Name"]);
        }

        [Test]
        public void LinqTest()
        {
            AreEqual(
                "[0,1,2,3,4,5,6,7,8,9]",
                JsonFormatter.SerializeObject(JsonFormatter.DeserializeObject<object>(JsonFormatter.SerializeObject(Enumerable.Range(0, 10))))
                );

            AreEqual(
                "[[0,0],[1,1]]",
                JsonFormatter.SerializeObject(
                    JsonFormatter.DeserializeObject<object>(
                        JsonFormatter.SerializeObject(
                            Enumerable.Range(0, 2).Concat(Enumerable.Range(0, 2)).GroupBy(item => item)
                            )
                        )
                    )
                );

            AreEqual(
                "[8,7,6,5,4,3,2,1]",
                JsonFormatter.SerializeObject(
                    JsonFormatter.DeserializeObject<object>(
                        JsonFormatter.SerializeObject(
                            Enumerable.Range(0, 10).Reverse().Skip(1).Take(8)
                            )
                        )
                    )
                );

            AreEqual(
                "[[0,0],[1,1]]",
                JsonFormatter.SerializeObject(
                    JsonFormatter.DeserializeObject<object>(
                        JsonFormatter.SerializeObject(
                            Enumerable.Range(0, 2).Concat(Enumerable.Range(0, 2)).ToLookup(item => item)
                            )
                        )
                    )
                );


            AreEqual(
                "[1,2,3,4,5,6,7,8]",
                JsonFormatter.SerializeObject(
                    JsonFormatter.DeserializeObject<object>(
                        JsonFormatter.SerializeObject(
                            Enumerable.Range(0, 10).Reverse().Skip(1).Take(8).OrderBy(item => item)
                            )
                        )
                    )
                );

        }

        [Test]
        public void DataTableTest()
        {
            var datatable = JsonFormatter.DeserializeObject<DataTable>(
                JsonFormatter.SerializeObject(
                Enumerable.Range(1, 2).Select(item => new { Id = item, Name = $"Dogwei{item}" })
                ));

            var jsonFormatter = new JsonFormatter();

            jsonFormatter.SetDataTableRWOptions(DataTableRWOptions.WriteToArrayFromBeginningSecondRows | DataTableRWOptions.SetFirstRowsTypeToColumnTypes);

            AreEqual(
                "[{\"Id\":1,\"Name\":\"Dogwei1\"},[2,\"Dogwei2\"]]",
                jsonFormatter.Serialize(datatable));

            AreEqual(
                "[{\"Id\":1,\"Name\":\"Dogwei1\"},{\"Id\":2,\"Name\":\"Dogwei2\"}]",
                JsonFormatter.SerializeObject(JsonFormatter.DeserializeObject<DataTable>(jsonFormatter.Serialize(datatable.CreateDataReader()))));
        }

        [Test]
        public void MultDimArrayTest()
        {
            var jsonFormatter = new JsonFormatter(JsonFormatterOptions.LoopReferencingNull);

            var json = JsonFormatter.SerializeObject(Enumerable.Range(0, 10).Select(_ => Enumerable.Range(0, 10).Select(_ => Enumerable.Range(0, 10))));

            var array = JsonFormatter.DeserializeObject<int[,,]>(json);
            var arrays = JsonFormatter.DeserializeObject<int[][][]>(json);

            AreEqual(json, jsonFormatter.Serialize(array));

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    for (int k = 0; k < 10; k++)
                    {
                        AreEqual(k, array[i, j, k]);
                        AreEqual(k, arrays[i][j][k]);
                    }
                }
            }

            var obj = new { Id = 123 };

            json = JsonFormatter.SerializeObject(Enumerable.Range(0, 10).Select(_ => Enumerable.Range(0, 10).Select(_ => obj)));

            var objs = JsonFormatter.DeserializeObject<object[,]>(json);

            AreEqual(json, jsonFormatter.Serialize(objs));

            json = JsonFormatter.SerializeObject(Enumerable.Range(0, 10).Select(_ => Enumerable.Range(0, 10).Select(_ => obj)), JsonFormatterOptions.MultiReferencingReference);

            objs = JsonFormatter.DeserializeObject<object[,]>(json, JsonFormatterOptions.MultiReferencingReference);

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 10; j++)
                {
                    IsTrue(objs[0, 0] == objs[i, j]);
                }
            }
        }

        [Test]
        public void EmptyStringTest()
        {
            var jsonFormatter = new JsonFormatter(JsonFormatterOptions.EmptyStringAsNull);

            IsNull(jsonFormatter.Deserialize<int?>("\"\""));

            Catch(() => jsonFormatter.Deserialize<int?>("\" \""));

            //jsonFormatter.Options = JsonFormatterOptions.WhiteSpaceStringAsDefault;

            //IsNull(jsonFormatter.Deserialize<int?>("\" \""));
            //AreEqual(0, jsonFormatter.Deserialize<int>("\" \n\r \t \""));


            //Catch(() => jsonFormatter.Deserialize<int?>("\" \n\r \t \\\"\""));
        }

        public class NestingObject
        {
            [JsonObject]
            public object Json { get; set; } = new { Id = 123, Name = "Dogwei" };

            public int Id => 123;

            public string Name => "Dogwei";
        }

        public class JsonObjectAttribute : RWFieldAttribute
        {
            public T ReadValue<T>(IValueReader valueReader)
            {
                var json = valueReader.ReadString();

                return JsonFormatter.DeserializeObject<T>(json);
            }

            public void WriteValue<T>(IValueWriter valueWriter, T value)
            {
                valueWriter.WriteString(JsonFormatter.SerializeObject(value));
            }
        }
    }
}