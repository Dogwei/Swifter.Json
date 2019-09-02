using Newtonsoft.Json;
using Swifter.RW;

namespace Swifter.Benchmarks.Tests
{
    sealed class BooleanArrayTest : ITest<bool[]>
    {
        readonly bool[] data;
        readonly string json;

        public BooleanArrayTest()
        {
            data = ValueInterface<bool[]>.ReadValue(new RandomDataReader { MinArraySize = 1000, MaxArraySize = 10000 });
            json = JsonConvert.SerializeObject(data);
        }


        public bool NeedReset => false;

        public bool Deser => true;

        public bool Ser => true;

        public object Name => $"bool[{data.Length}]";


        public bool[] GetData()
        {
            return data;
        }

        public string GetJson()
        {
            return json;
        }

        public void Reset(bool[] data)
        {
        }

        public void VerifyData(bool[] data)
        {
        }

        public void VerifyJson(string json)
        {
        }
    }
}
