using Newtonsoft.Json;
using Swifter.RW;

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
}
