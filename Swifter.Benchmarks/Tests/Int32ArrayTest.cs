using Newtonsoft.Json;
using Swifter.RW;

namespace Swifter.Benchmarks.Tests
{
    sealed class Int32ArrayTest : ITest<int[]>
    {
        readonly int[] data;
        readonly string json;

        public Int32ArrayTest()
        {
            data = ValueInterface<int[]>.ReadValue(new RandomDataReader { MinArraySize = 1000, MaxArraySize = 10000 });
            json = JsonConvert.SerializeObject(data);
        }

        public bool NeedReset => false;

        public bool Deser => true;

        public bool Ser => true;

        public object Name => $"int[{data.Length}]";


        public int[] GetData()
        {
            return data;
        }

        public string GetJson()
        {
            return json;
        }

        public void Reset(int[] data)
        {
        }

        public void VerifyData(int[] data)
        {
        }

        public void VerifyJson(string json)
        {
        }
    }
}
