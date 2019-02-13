using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Swifter.Test
{
    public interface ITestInfo
    {
        string Name { get; }

        string Text { get; }

        Type Type { get; }

        int Count { get; }

        bool VerDeser(object obj);

        bool VerSer(string json);
    }

    class BooleanTest : ITestInfo
    {
        public BooleanTest(int length, int count)
        {
            var sb = new StringBuilder();
            var random = new Random();

            sb.Append("[");

            for (int i = 0; i < length; i++)
            {
                sb.Append((random.Next() & 1) != 0 ? "true" : "false");
                sb.Append(",");
            }

            if (length >= 1)
            {
                --sb.Length;
            }

            sb.Append("]");

            Length = length;
            Name = $"bool[{length}]";
            Text = sb.ToString();
            Type = typeof(bool[]);
            Count = count;
        }

        public int Length { get; }

        public string Name { get; }

        public string Text { get; }

        public Type Type { get; }

        public int Count { get; }

        public bool VerDeser(object obj)
        {
            if ((obj is bool[] arr) && arr.Length == Length && arr.Contains(true) && arr.Contains(false))
            {
                var truth = Newtonsoft.Json.JsonConvert.DeserializeObject<bool[]>(Text);

                for (int i = 0; i < truth.Length; i++)
                {
                    if (arr[i] != truth[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public bool VerSer(string json)
        {
            try
            {
                return VerDeser(Newtonsoft.Json.JsonConvert.DeserializeObject(json, Type));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    class Int32Test : ITestInfo
    {
        public Int32Test(int length, int count)
        {
            var sb = new StringBuilder();
            var random = new Random();

            sb.Append("[");

            for (int i = 0; i < length; i++)
            {
                sb.Append(random.Next(int.MinValue, int.MaxValue));
                sb.Append(",");
            }

            if (length >= 1)
            {
                --sb.Length;
            }

            sb.Append("]");

            Length = length;
            Name = $"int[{length}]";
            Text = sb.ToString();
            Type = typeof(int[]);
            Count = count;
        }

        public int Length { get; }

        public string Name { get; }

        public string Text { get; }

        public Type Type { get; }

        public int Count { get; }

        public bool VerDeser(object obj)
        {
            if ((obj is int[] arr) && arr.Length == Length)
            {
                var truth = Newtonsoft.Json.JsonConvert.DeserializeObject<int[]>(Text);

                for (int i = 0; i < truth.Length; i++)
                {
                    if (arr[i] != truth[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public bool VerSer(string json)
        {
            try
            {
                return VerDeser(Newtonsoft.Json.JsonConvert.DeserializeObject(json, Type));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    class TwoDimensionaArrayTest : ITestInfo
    {
        const int length1 = 100;
        const int length2 = 100;

        public TwoDimensionaArrayTest()
        {
            var random = new Random();
            var sb = new StringBuilder();

            sb.Append('[');

            for (int i = 0; i < length1; i++)
            {
                sb.Append('[');

                for (int j = 0; j < length2; j++)
                {
                    sb.Append(random.Next(int.MinValue, int.MaxValue));

                    sb.Append(',');
                }

                --sb.Length;

                sb.Append(']');

                sb.Append(',');
            }

            --sb.Length;

            sb.Append(']');

            Text = sb.ToString();
        }

        public string Name => $"int[{length1},{length2}]";

        public string Text { get; set; }

        public Type Type => typeof(int[,]);

        public int Count => 100;

        public bool VerDeser(object obj)
        {
            return obj is int[,] arr && arr.GetLength(0) == length1 && arr.GetLength(1) == length2 && (arr[0, 0] != 0 || arr[1, 1] != 0);
        }

        public bool VerSer(string json)
        {
            try
            {
                return VerDeser(Newtonsoft.Json.JsonConvert.DeserializeObject(json, Type));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    class ThreeDimensionalArray : ITestInfo
    {
        const int length1 = 50;
        const int length2 = 50;
        const int length3 = 50;

        public ThreeDimensionalArray()
        {
            var random = new Random();
            var sb = new StringBuilder();

            sb.Append('[');

            for (int i = 0; i < length1; i++)
            {
                sb.Append('[');

                for (int j = 0; j < length2; j++)
                {
                    sb.Append('[');

                    for (int k = 0; k < length3; k++)
                    {

                        sb.Append(random.Next(int.MinValue, int.MaxValue));

                        sb.Append(',');
                    }

                    --sb.Length;

                    sb.Append(']');

                    sb.Append(',');
                }

                --sb.Length;

                sb.Append(']');

                sb.Append(',');
            }

            --sb.Length;

            sb.Append(']');

            Text = sb.ToString();
        }

        public string Name => $"int[{length1},{length2},{length3}]";

        public string Text { get; set; }

        public Type Type => typeof(int[,,]);

        public int Count => 100;

        public bool VerDeser(object obj)
        {
            return obj is int[,,] arr && arr.GetLength(0) == length1 && arr.GetLength(1) == length2 && arr.GetLength(2) == length3 && (arr[0, 0, 0] != 0 || arr[1, 1, 1] != 0);
        }

        public bool VerSer(string json)
        {
            try
            {
                return VerDeser(Newtonsoft.Json.JsonConvert.DeserializeObject(json, Type));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    class Int64Test : ITestInfo
    {
        public Int64Test(int length, int count)
        {
            var sb = new StringBuilder();
            var random = new Random();

            sb.Append("[");

            for (int i = 0; i < length; i++)
            {
                sb.Append((long)random.Next(int.MinValue, int.MaxValue) * random.Next(int.MinValue, int.MaxValue) * random.Next(int.MinValue, int.MaxValue));
                sb.Append(",");
            }

            if (length >= 1)
            {
                --sb.Length;
            }

            sb.Append("]");

            Length = length;
            Name = $"long[{length}]";
            Text = sb.ToString();
            Type = typeof(long[]);
            Count = count;
        }

        public int Length { get; }

        public string Name { get; }

        public string Text { get; }

        public Type Type { get; }

        public int Count { get; }

        public bool VerDeser(object obj)
        {
            if ((obj is long[] arr) && arr.Length == Length)
            {
                var truth = Newtonsoft.Json.JsonConvert.DeserializeObject<long[]>(Text);

                for (int i = 0; i < truth.Length; i++)
                {
                    if (arr[i] != truth[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public bool VerSer(string json)
        {
            try
            {
                return VerDeser(Newtonsoft.Json.JsonConvert.DeserializeObject(json, Type));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    class FloatTest : ITestInfo
    {
        public FloatTest(int length, int count)
        {
            var sb = new StringBuilder();
            var random = new Random();

            sb.Append("[");

            for (int i = 0; i < length; i++)
            {
                var value = (float)random.NextDouble();

                for (int j = random.Next(40); j > 0; --j)
                {
                    value = value / (float)Math.Max(random.NextDouble(), 0.001);
                }

                sb.Append(value);
                sb.Append(",");
            }

            if (length >= 1)
            {
                --sb.Length;
            }

            sb.Append("]");

            Length = length;
            Name = $"float[{length}]";
            Text = sb.ToString();
            Type = typeof(float[]);
            Count = count;
        }

        public int Length { get; }

        public string Name { get; }

        public string Text { get; }

        public Type Type { get; }

        public int Count { get; }

        public bool VerDeser(object obj)
        {
            var regex = new Regex("^(?<N>[+-]?[0-9]+(\\.[0-9]+)?)([eE](?<P>[+-]?[0-9]+))?$");

            if ((obj is float[] arr) && arr.Length == Length)
            {
                var truth = Newtonsoft.Json.JsonConvert.DeserializeObject<float[]>(Text);

                for (int i = 0; i < truth.Length; i++)
                {
                    if (!Equal(arr[i], truth[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;

            bool Equal(float x, float y)
            {
                var xm = regex.Match(x.ToString());
                var ym = regex.Match(y.ToString());

                var xp = xm.Groups["P"];
                var yp = ym.Groups["P"];

                if (xp.Success != yp.Success)
                {
                    return false;
                }

                if (xp.Success && int.Parse(xp.Value) != int.Parse(yp.Value))
                {
                    return false;
                }

                var xn = double.Parse(xm.Groups["N"].Value);
                var yn = double.Parse(ym.Groups["N"].Value);

                if (Math.Abs((xn - yn) / xn) >= 1e-2)
                {
                    return false;
                }

                return true;
            }
        }

        public bool VerSer(string json)
        {
            try
            {
                return VerDeser(Newtonsoft.Json.JsonConvert.DeserializeObject(json, Type));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    class DoubleTest : ITestInfo
    {
        public DoubleTest(int length, int count)
        {
            var sb = new StringBuilder();
            var random = new Random();

            sb.Append("[");

            for (int i = 0; i < length; i++)
            {
                var value = random.NextDouble();

                for (int j = random.Next(120); j > 0; --j)
                {
                    value = value / Math.Max(random.NextDouble(), 0.001);
                }

                sb.Append(value);
                sb.Append(",");
            }

            if (length >= 1)
            {
                --sb.Length;
            }

            sb.Append("]");

            Length = length;
            Name = $"double[{length}]";
            Text = sb.ToString();
            Type = typeof(double[]);
            Count = count;
        }

        public int Length { get; }

        public string Name { get; }

        public string Text { get; }

        public Type Type { get; }

        public int Count { get; }

        public bool VerDeser(object obj)
        {
            var regex = new Regex("^(?<N>[+-]?[0-9]+(\\.[0-9]+)?)([eE](?<P>[+-]?[0-9]+))?$");

            if ((obj is double[] arr) && arr.Length == Length)
            {
                var truth = Newtonsoft.Json.JsonConvert.DeserializeObject<double[]>(Text);

                for (int i = 0; i < truth.Length; i++)
                {
                    if (!Equal(arr[i], truth[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;

            bool Equal(double x, double y)
            {
                var xm = regex.Match(x.ToString());
                var ym = regex.Match(y.ToString());

                var xp = xm.Groups["P"];
                var yp = ym.Groups["P"];

                if (xp.Success != yp.Success)
                {
                    return false;
                }

                if (xp.Success && int.Parse(xp.Value) != int.Parse(yp.Value))
                {
                    return false;
                }

                var xn = double.Parse(xm.Groups["N"].Value);
                var yn = double.Parse(ym.Groups["N"].Value);

                if (Math.Abs((xn - yn) / xn) >= 1e-10)
                {
                    return false;
                }

                return true;
            }
        }

        public bool VerSer(string json)
        {
            try
            {
                return VerDeser(Newtonsoft.Json.JsonConvert.DeserializeObject(json, Type));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    class DecimalTest : ITestInfo
    {
        public DecimalTest(int length, int count)
        {
            var sb = new StringBuilder();
            var random = new Random();

            sb.Append("[");

            for (int i = 0; i < length; i++)
            {
                var value = (decimal)random.NextDouble();

                for (int j = random.Next(28); j > 0; --j)
                {
                    value = value / (decimal)Math.Max(random.NextDouble(), 0.001);
                }

                sb.Append(value);
                sb.Append(",");
            }

            if (length >= 1)
            {
                --sb.Length;
            }

            sb.Append("]");

            Length = length;
            Name = $"decimal[{length}]";
            Text = sb.ToString();
            Type = typeof(decimal[]);
            Count = count;
        }

        public int Length { get; }

        public string Name { get; }

        public string Text { get; }

        public Type Type { get; }

        public int Count { get; }

        public bool VerDeser(object obj)
        {
            if ((obj is decimal[] arr) && arr.Length == Length)
            {
                var truth = Newtonsoft.Json.JsonConvert.DeserializeObject<decimal[]>(Text);

                for (int i = 0; i < truth.Length; i++)
                {
                    if (arr[i] != truth[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public bool VerSer(string json)
        {
            try
            {
                return VerDeser(Newtonsoft.Json.JsonConvert.DeserializeObject(json, Type));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    class CSharp7Test : ITestInfo
    {
        public string Name => "C#7.0 Attributes";

        public string Text => "{\"Name\":\"" + Name + "\"}";

        public Type Type => typeof(CSharp7);

        public int Count => 10000;

        public bool VerDeser(object obj)
        {
            return (obj is CSharp7 cs) && cs.Name == Name;
        }

        public bool VerSer(string json)
        {
            try
            {
                var dic = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                if (!Equals(dic["Name"], Name))
                {
                    return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public class CSharp7
        {
            private string name;

            public ref string Name => ref name;
        }
    }

    class PolymorphismTest : ITestInfo
    {
        private const int Id = 9999;

        public string Name => "Polymorphism class";

        public string Text => "{" +
            "\"Name\":\"" + Name + "\"," +
            "\"Count\":\"" + Count + "\"," +
            "\"Id\":\"" + Id + "\"" +
            "}";

        public Type Type => typeof(Polymorphism);

        public int Count => 10000;

        public bool VerDeser(object obj)
        {
            return obj is Polymorphism poly && poly.Name == Name && poly.Count == Count && poly.Id == Id;
        }

        public bool VerSer(string json)
        {
            try
            {
                return VerDeser(Newtonsoft.Json.JsonConvert.DeserializeObject(json, Type));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    class CommonDataTest : ITestInfo
    {
        private const string CacheToken = "Commands";
        private const string Description = "新增命令信息";

        public CommonDataTest(int count)
        {
            Name = "CommonData";
            Text = "{" +
                "\"CacheToken\":\"" + CacheToken + "\"," +
                "\"Code\":\"INSEET Commands(Id, Name) VALUES(@Id, @Name)\"," +
                "\"Demo\":\"{\\\"Id\\\":10,\\\"Name\\\":\\\"InsertCommand\\\"}\"," +
                "\"Description\":\"" + Description + "\"," +
                "\"Id\":1," +
                "\"Name\":\"InsertCommand\"," +
                "\"RowNum\":10," +
                "\"Sign\":81," +
                "\"Type\":\"MSSQL-SYSTEM-DB\"" +
                "}";

            Type = typeof(CommonData);
            Count = count;
        }

        public string Name { get; }

        public string Text { get; }

        public Type Type { get; }

        public int Count { get; }

        public bool VerDeser(object obj)
        {
            return (obj is CommonData data) && data.CacheToken == CacheToken && data.Description == Description;
        }

        public bool VerSer(string json)
        {
            try
            {
                return VerDeser(Newtonsoft.Json.JsonConvert.DeserializeObject(json, Type));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    class CommonDataDictionaryTest : ITestInfo
    {
        private const string CacheToken = "Commands";
        private const string Description = "新增命令信息";

        public CommonDataDictionaryTest(int count)
        {
            Name = "CommonData Dictionary";
            Text = "{" +
                "\"CacheToken\":\"" + CacheToken + "\"," +
                "\"Code\":\"INSEET Commands(Id, Name) VALUES(@Id, @Name)\"," +
                "\"Demo\":\"{\\\"Id\\\":10,\\\"Name\\\":\\\"InsertCommand\\\"}\"," +
                "\"Description\":\"" + Description + "\"," +
                "\"Id\":1," +
                "\"Name\":\"InsertCommand\"," +
                "\"RowNum\":10," +
                "\"Sign\":81," +
                "\"Type\":\"MSSQL-SYSTEM-DB\"" +
                "}";

            Type = typeof(Dictionary<string, object>);
            Count = count;
        }

        public string Name { get; }

        public string Text { get; }

        public Type Type { get; }

        public int Count { get; }

        public bool VerDeser(object obj)
        {
            return (obj is Dictionary<string, object> data) && Equals(data["CacheToken"], CacheToken) && Equals(data["Description"], Description);
        }

        public bool VerSer(string json)
        {
            try
            {
                return VerDeser(Newtonsoft.Json.JsonConvert.DeserializeObject(json, Type));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    class CalalogTest : ITestInfo
    {
        public CalalogTest(int count)
        {
            Name = "Calalog";
            Text = Encoding.UTF8.GetString(Resource.catalog);

            Type = typeof(Catalog);
            Count = count;
        }

        public string Name { get; }

        public string Text { get; }

        public Type Type { get; }

        public int Count { get; }

        public bool VerDeser(object obj)
        {
            return (obj is Catalog data) && data.packages.Length > 0 && !string.IsNullOrWhiteSpace(data.manifestVersion);
        }

        public bool VerSer(string json)
        {
            try
            {
                return VerDeser(Newtonsoft.Json.JsonConvert.DeserializeObject(json, Type));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    class CalalogDictionaryTest : ITestInfo
    {
        public CalalogDictionaryTest(int count)
        {
            Name = "Calalog Dictionary";
            Text = Encoding.UTF8.GetString(Resource.catalog);

            Type = typeof(Dictionary<string, object>);
            Count = count;
        }

        public string Name { get; }

        public string Text { get; }

        public Type Type { get; }

        public int Count { get; }

        public bool VerDeser(object obj)
        {
            return (obj is Dictionary<string, object> data)
                && (data["manifestVersion"] is string manifestVersion)
                && !string.IsNullOrWhiteSpace(manifestVersion)
                && (data["packages"] is object packages)
                && !(packages is string)
                && !(packages is int);
        }

        public bool VerSer(string json)
        {
            try
            {
                return VerDeser(Newtonsoft.Json.JsonConvert.DeserializeObject(json, Type));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    class StructTest : ITestInfo
    {
        public string Name => "Struct Test";

        public string Text => "{" +
            "\"Name\":\"" + Name + "\"," +
            "\"Nullable\":null," +
            "\"Count\":\"" + Count + "\"" +
            "}";

        public Type Type => typeof(TestStruct);

        public int Count => 10000;

        public bool VerDeser(object obj)
        {
            return obj is TestStruct @struct && @struct.Name == Name && @struct.Nullable == null && @struct.Count == Count;
        }

        public bool VerSer(string json)
        {
            try
            {
                return VerDeser(Newtonsoft.Json.JsonConvert.DeserializeObject(json, Type));
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    unsafe class UnsafeTest : ITestInfo
    {
        public const int Address = 123;

        public string Name => "Pointer";

        public string Text => "{" +
            "\"Name\":\"" + Name + "\"," +
            "\"Count\":\"" + Count + "\"," +
            "\"Pointer\":null," +
            "\"Address\":" + Address + "" +
            "}";

        public Type Type => typeof(UnsafeClass);

        public int Count => 10000;

        public bool VerDeser(object obj)
        {
            return obj is UnsafeClass @struct && @struct.Name == Name && @struct.Pointer == null && @struct.Count == Count && (int)@struct.Address == Address;
        }

        public bool VerSer(string json)
        {
            return !string.IsNullOrEmpty(json);
        }
    }
}