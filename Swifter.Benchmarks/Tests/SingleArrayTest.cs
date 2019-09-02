using Newtonsoft.Json;
using Swifter.RW;

namespace Swifter.Benchmarks.Tests
{
    sealed class SingleArrayTest : ITest<float[]>
    {
        readonly float[] data;
        readonly string json;

        public SingleArrayTest()
        {
            data = ValueInterface<float[]>.ReadValue(new RandomDataReader { MinArraySize = 100, MaxArraySize = 1000 });
            json = JsonConvert.SerializeObject(data);
        }

        public bool NeedReset => false;

        public bool Deser => true;

        public bool Ser => true;

        public object Name => $"float[{data.Length}]";


        public float[] GetData()
        {
            return data;
        }

        public string GetJson()
        {
            return json;
        }

        public void Reset(float[] data)
        {
        }

        public void VerifyData(float[] data)
        {
        }

        public void VerifyJson(string json)
        {
        }
    }
}
