using Newtonsoft.Json;
using Swifter.RW;

namespace Swifter.Benchmarks.Tests
{
    sealed class Int64ArrayTest : ITest<long[]>
    {
        readonly long[] data;
        readonly string json;

        public Int64ArrayTest()
        {
            data = ValueInterface<long[]>.ReadValue(new RandomDataReader { MinArraySize = 1000, MaxArraySize = 10000 });
            json = JsonConvert.SerializeObject(data);
        }

        public bool NeedReset => false;

        public bool Deser => true;

        public bool Ser => true;

        public object Name => $"long[{data.Length}]";


        public long[] GetData()
        {
            return data;
        }

        public string GetJson()
        {
            return json;
        }

        public void Reset(long[] data)
        {
        }

        public void VerifyData(long[] data)
        {
        }

        public void VerifyJson(string json)
        {
        }
    }
}
