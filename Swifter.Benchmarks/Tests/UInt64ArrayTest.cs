using Newtonsoft.Json;
using Swifter.RW;
using System;

namespace Swifter.Benchmarks.Tests
{
    sealed class UInt64ArrayTest : ITest<ulong[]>
    {
        readonly ulong[] data;
        readonly string json;

        public UInt64ArrayTest()
        {
            data = ValueInterface<ulong[]>.ReadValue(new RandomDataReader { MinArraySize = 1000, MaxArraySize = 10000 });
            json = JsonConvert.SerializeObject(data);
        }

        public bool NeedReset => false;

        public bool Deser => true;

        public bool Ser => true;

        public object Name => $"ulong[{data.Length}]";


        public ulong[] GetData()
        {
            return data;
        }

        public string GetJson()
        {
            return json;
        }

        public void Reset(ulong[] data)
        {
        }

        public void VerifyData(ulong[] data)
        {
        }

        public void VerifyJson(string json)
        {
        }
    }

    sealed class TwoDimensionaArrayTest : ITest<int[,]>
    {
        readonly int[,] data;
        readonly string json;

        public TwoDimensionaArrayTest()
        {
            var random = new RandomDataReader() { MinArraySize = 10, MaxArraySize = 1000 };

            data = ValueInterface<int[,]>.ReadValue(random);

            json = JsonConvert.SerializeObject(data);
        }

        public bool NeedReset => false;

        public bool Deser => true;

        public bool Ser => true;

        public object Name => $"int[{data.GetLength(0)},{data.GetLength(1)}]";

        public int[,] GetData() => data;

        public string GetJson() => json;

        public void Reset(int[,] data)
        {
        }

        public void VerifyData(int[,] data)
        {
            VerifyJson(JsonConvert.SerializeObject(data));
        }

        public void VerifyJson(string json)
        {
            if (this.json  != json)
            {
                throw new Exception();
            }
        }
    }
}
