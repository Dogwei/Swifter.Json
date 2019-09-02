using Newtonsoft.Json;
using Swifter.RW;

namespace Swifter.Benchmarks.Tests
{
    sealed class UInt32ArrayTest : ITest<uint[]>
    {
        readonly uint[] data;
        readonly string json;

        public UInt32ArrayTest()
        {
            data = ValueInterface<uint[]>.ReadValue(new RandomDataReader { MinArraySize = 1000, MaxArraySize = 10000 });
            json = JsonConvert.SerializeObject(data);
        }

        public bool NeedReset => false;

        public bool Deser => true;

        public bool Ser => true;

        public object Name => $"uint[{data.Length}]";


        public uint[] GetData()
        {
            return data;
        }

        public string GetJson()
        {
            return json;
        }

        public void Reset(uint[] data)
        {
        }

        public void VerifyData(uint[] data)
        {
        }

        public void VerifyJson(string json)
        {
        }
    }
}
