using NUnit.Framework;
using Swifter.Json;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Swifter.Test.NUnit
{
    public abstract class Json<T>
    {
        public static readonly Random random = new Random();

        public JsonFormatter jsonFormatter;

        public JsonFormatter indentedJsonFormatter;

        public IEqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;

        public T[] s;
        public T[,] ss;
        public T[,,] sss;

        public virtual T RandomNext()
        {
            var guid = new Guid(
                random.Next(int.MinValue, int.MaxValue),
                (short)random.Next(short.MinValue, short.MaxValue),
                (short)random.Next(short.MinValue, short.MaxValue),
                (byte)random.Next(byte.MinValue, byte.MaxValue),
                (byte)random.Next(byte.MinValue, byte.MaxValue),
                (byte)random.Next(byte.MinValue, byte.MaxValue),
                (byte)random.Next(byte.MinValue, byte.MaxValue),
                (byte)random.Next(byte.MinValue, byte.MaxValue),
                (byte)random.Next(byte.MinValue, byte.MaxValue),
                (byte)random.Next(byte.MinValue, byte.MaxValue),
                (byte)random.Next(byte.MinValue, byte.MaxValue));

            return Unsafe.As<Guid, T>(ref guid);
        }

        [SetUp]
        public void Setup()
        {
            jsonFormatter = new JsonFormatter();

            indentedJsonFormatter = new JsonFormatter(JsonFormatterOptions.Indented);

            s = new T[random.Next(100, 1000)];

            for (int i = 0; i < s.Length; i++)
            {
                s[i] = RandomNext();
            }

            ss = new T[random.Next(10, 100), random.Next(10, 100)];

            for (int i = 0; i < ss.GetLength(0); i++)
            {
                for (int j = 0; j < ss.GetLength(1); j++)
                {
                    ss[i, j] = RandomNext();
                }
            }

            sss = new T[random.Next(5, 50), random.Next(5, 50), random.Next(5, 50)];

            for (int i = 0; i < sss.GetLength(0); i++)
            {
                for (int j = 0; j < sss.GetLength(1); j++)
                {
                    for (int k = 0; k < sss.GetLength(2); k++)
                    {
                        sss[i, j, k] = RandomNext();
                    }
                }
            }
        }

        [Test]
        public void S()
        {
            var json1 = JsonFormatter.SerializeObject(s);

            var json2 = jsonFormatter.Serialize(s);

            AreEqual(json1, json2);

            var my = jsonFormatter.Deserialize<T[]>(json1);

            AreEqual(s, my);

            Assert.Pass();
        }

        [Test]
        public void SS()
        {
            var json1 = JsonFormatter.SerializeObject(ss);

            var json2 = jsonFormatter.Serialize(ss);

            AreEqual(json1, json2);

            var my = jsonFormatter.Deserialize<T[,]>(json1);

            AreEqual(ss, my);

            // Assert.Pass();
        }

        [Test]
        public void SSS()
        {
            var json1 = JsonFormatter.SerializeObject(sss);

            var json2 = jsonFormatter.Serialize(sss);

            AreEqual(json1, json2);

            var my = jsonFormatter.Deserialize<T[,,]>(json1);

            AreEqual(sss, my);

            // Assert.Pass();
        }

        [Test]
        public void SyncS()
        {
            string json1;
            string json2;

            using (StringWriter sw = new StringWriter())
            {
                JsonFormatter.SerializeObject(s, sw);

                json1 = sw.ToString();
            }

            using (StringWriter sw = new StringWriter())
            {
                jsonFormatter.Serialize(s, sw);

                json2 = sw.ToString();
            }

            AreEqual(json1, json2);

            T[] my;

            using (StringReader sr = new StringReader(json1))
            {
                my = jsonFormatter.Deserialize<T[]>(sr);
            }

            AreEqual(s, my);
        }


        [Test]
        public void SyncSS()
        {
            string json1;

            string json2;

            using (StringWriter sw = new StringWriter())
            {
                JsonFormatter.SerializeObject(ss, sw);

                json1 = sw.ToString();
            }

            using (StringWriter sw = new StringWriter())
            {
                jsonFormatter.Serialize(ss, sw);

                json2 = sw.ToString();
            }

            AreEqual(json1, json2);

            T[,] my;

            using (StringReader sr = new StringReader(json1))
            {
                my = jsonFormatter.Deserialize<T[,]>(sr);
            }

            AreEqual(ss, my);

            Assert.Pass();
        }

        [Test]
        public void SyncSSS()
        {
            string json1;

            string json2;

            using (StringWriter sw = new StringWriter())
            {
                JsonFormatter.SerializeObject(sss, sw);

                json1 = sw.ToString();
            }

            using (StringWriter sw = new StringWriter())
            {
                jsonFormatter.Serialize(sss, sw);

                json2 = sw.ToString();
            }

            AreEqual(json1, json2);

            T[,,] my;

            using (StringReader sr = new StringReader(json1))
            {
                my = jsonFormatter.Deserialize<T[,,]>(sr);
            }

            AreEqual(sss, my);

            Assert.Pass();
        }

        [Test]
        public void AsyncS()
        {
            string json1;
            string json2;

            using (StringWriter sw = new StringWriter())
            {
                JsonFormatter.SerializeObjectAsync(s, sw).Wait();

                json1 = sw.ToString();
            }

            using (StringWriter sw = new StringWriter())
            {
                jsonFormatter.SerializeAsync(s, sw).Wait();

                json2 = sw.ToString();
            }

            AreEqual(json1, json2);

            T[] my;

            using (StringReader sr = new StringReader(json1))
            {
                my = jsonFormatter.DeserializeAsync<T[]>(sr).Result;
            }

            AreEqual(s, my);
        }


        [Test]
        public void AsyncSS()
        {
            string json1;

            string json2;

            using (StringWriter sw = new StringWriter())
            {
                JsonFormatter.SerializeObjectAsync(ss, sw).Wait();

                json1 = sw.ToString();
            }

            using (StringWriter sw = new StringWriter())
            {
                jsonFormatter.SerializeAsync(ss, sw).Wait();

                json2 = sw.ToString();
            }

            AreEqual(json1, json2);

            T[,] my;

            using (StringReader sr = new StringReader(json1))
            {
                my = jsonFormatter.DeserializeAsync<T[,]>(sr).Result;
            }

            AreEqual(ss, my);

            Assert.Pass();
        }

        [Test]
        public void AsyncSSS()
        {
            Task.Run(async () =>
            {
                string json1;

                string json2;

                using (StringWriter sw = new StringWriter())
                {
                    await JsonFormatter.SerializeObjectAsync(sss, sw);

                    json1 = sw.ToString();
                }

                using (StringWriter sw = new StringWriter())
                {
                    await jsonFormatter.SerializeAsync(sss, sw);

                    json2 = sw.ToString();
                }

                AreEqual(json1, json2);

                T[,,] my;

                using (StringReader sr = new StringReader(json1))
                {
                    my = await jsonFormatter.DeserializeAsync<T[,,]>(sr);
                }

                AreEqual(sss, my);

            }).Wait();

            Assert.Pass();
        }





        public void AreEqual(string expected, string actual)
        {
            if (expected != actual)
            {
                Assert.AreEqual(expected, actual);
            }
        }

        public void AreEqual(T[] expected, T[] actual)
        {
            if (expected.Length != actual.Length)
            {
                Assert.AreEqual(expected, actual);
            }

            for (int i = 0; i < expected.Length; i++)
            {
                if (!Equal(expected[i], actual[i]))
                {
                    Assert.AreEqual(expected, actual);
                }
            }
        }

        public void AreEqual(T[,] expected, T[,] actual)
        {
            if (expected.GetLength(0) != actual.GetLength(0))
            {
                Assert.AreEqual(expected, actual);
            }

            if (expected.GetLength(1) != actual.GetLength(1))
            {
                Assert.AreEqual(expected, actual);
            }

            for (int i = 0; i < expected.GetLength(0); i++)
            {
                for (int j = 0; j < expected.GetLength(1); j++)
                {
                    if (!Equal(expected[i, j], actual[i, j]))
                    {
                        Assert.AreEqual(expected, actual);
                    }
                }
            }
        }

        public void AreEqual(T[,,] expected, T[,,] actual)
        {
            if (expected.GetLength(0) != actual.GetLength(0))
            {
                Assert.AreEqual(expected, actual);
            }

            if (expected.GetLength(1) != actual.GetLength(1))
            {
                Assert.AreEqual(expected, actual);
            }

            if (expected.GetLength(2) != actual.GetLength(2))
            {
                Assert.AreEqual(expected, actual);
            }

            for (int i = 0; i < expected.GetLength(0); i++)
            {
                for (int j = 0; j < expected.GetLength(1); j++)
                {
                    for (int k = 0; k < expected.GetLength(2); k++)
                    {
                        if (!Equal(expected[i, j, k], actual[i, j, k]))
                        {
                            Assert.AreEqual(expected, actual);
                        }
                    }
                }
            }
        }

        public virtual bool Equal(T expected, T actual)
        {
            return equalityComparer.Equals(expected, actual);
        }
    }

    public class _Int8 : Json<sbyte>
    {
    }

    public class _Int16 : Json<short>
    {

    }

    public class _Int32 : Json<int>
    {

    }

    public class _Int64 : Json<long>
    {

    }

    public class _UInt8 : Json<byte>
    {

    }

    public class _UInt16 : Json<ushort>
    {

    }

    public class _UInt32 : Json<uint>
    {

    }

    public class _UInt64 : Json<ulong>
    {

    }

    public class _Boolean : Json<bool>
    {
        public override bool RandomNext()
        {
            return random.Next(0, 2) == 0;
        }
    }

    public class _Char : Json<char>
    {

    }

    public class _DateTime : Json<DateTime>
    {
        public override DateTime RandomNext()
        {
            return DateTime.MinValue.AddSeconds(random.Next(0, 60 * 60 * 24 * 30)).AddYears(random.Next(0, 9900));
        }
    }

    public class _DateTimeOffset : Json<DateTimeOffset>
    {
        public override DateTimeOffset RandomNext()
        {
            return DateTimeOffset.MinValue.AddSeconds(random.Next(0, 60 * 60 * 24 * 30)).AddYears(random.Next(0, 9900));
        }
    }

    public class _Decimal : Json<decimal>
    {
        public override decimal RandomNext()
        {
            var value = (decimal)random.NextDouble();

            for (int j = random.Next(28); j > 0; --j)
            {
                value = value / (decimal)Math.Max(random.NextDouble(), 0.001);
            }

            return value;
        }
    }

    public class _Guid : Json<Guid>
    {

    }

    public class _Double : Json<double>
    {
        public override bool Equal(double expected, double actual)
        {
            if (expected == actual)
            {
                return true;
            }

            if (actual.ToString() == actual.ToString())
            {
                return true;
            }

            var num = expected / actual;

            // 允许 double 运算有一定容错率。
            return num >= 0.9999 && num <= 1.0001;
        }
    }

    public class _Single : Json<float>
    {
        public override bool Equal(float expected, float actual)
        {
            if (expected == actual)
            {
                return true;
            }

            if (actual.ToString() == actual.ToString())
            {
                return true;
            }

            var num = expected / actual;

            // 允许 float 运算有一定容错率。
            return num >= 0.99 && num <= 1.01;
        }
    }

    public class _String : Json<string>
    {
        public override string RandomNext()
        {
            var str = new string('\0', random.Next(0, 500));

            ref var ref_str = ref StringHelper.GetRawStringData(str);

            for (int i = 0; i < str.Length; i++)
            {
                Unsafe.Add(ref ref_str, i) = (char)random.Next(int.MinValue, int.MaxValue);
            }

            return str;
        }

        public override bool Equal(string expected, string actual)
        {
            return expected == actual;
        }
    }

    public class _Object : Json<object>
    {
        public _Int32 int32 = new _Int32();
        public _String @string = new _String();

        public override object RandomNext()
        {
            switch (random.Next(0,5))
            {
                case 0:
                    return new { Id = int32.RandomNext(), Name = @string.RandomNext() };
                case 1:
                    return new[] { int32.RandomNext(), int32.RandomNext(), int32.RandomNext() };
                case 2:
                    return new ValueTuple<int, string>(int32.RandomNext(), @string.RandomNext());
                case 3:
                    return new Tuple<int, string>(int32.RandomNext(), @string.RandomNext());
                default:
                    return null;
            }

        }

        public override bool Equal(object expected, object actual)
        {
            if (expected == null || actual == null)
            {
                return expected == null && actual == null;
            }

            var reader1 = RWHelper.CreateReader(expected).As<string>();
            var reader2 = RWHelper.CreateReader(actual).As<string>();

            if (reader1.Count != reader2.Count)
            {
                return false;
            }

            foreach (var item in reader1.Keys)
            {
                if (!Equals(reader1[item].DirectRead(), reader2[item].DirectRead()))
                {
                    return false;
                }
            }

            return true;
        }
    }
}