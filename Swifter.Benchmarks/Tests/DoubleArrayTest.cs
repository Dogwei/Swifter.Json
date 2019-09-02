using Newtonsoft.Json;
using Swifter.RW;

namespace Swifter.Benchmarks.Tests
{
    sealed class DoubleArrayTest : ITest<double[]>
    {
        readonly double[] data;
        readonly string json;

        public DoubleArrayTest()
        {
            data = ValueInterface<double[]>.ReadValue(new RandomDataReader { MinArraySize = 100, MaxArraySize = 1000 });
            json = JsonConvert.SerializeObject(data);
        }

        public bool NeedReset => false;

        public bool Deser => true;

        public bool Ser => true;

        public object Name => $"double[{data.Length}]";


        public double[] GetData()
        {
            return data;
        }

        public string GetJson()
        {
            return json;
        }

        public void Reset(double[] data)
        {
        }

        public void VerifyData(double[] data)
        {
        }

        public void VerifyJson(string json)
        {
        }
    }
}
