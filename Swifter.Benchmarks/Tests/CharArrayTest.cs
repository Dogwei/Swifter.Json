using Newtonsoft.Json;
using Swifter.RW;

namespace Swifter.Benchmarks.Tests
{
    sealed class CharArrayTest : ITest<char[]>
    {
        readonly char[] data;
        readonly string json;

        public CharArrayTest()
        {
            data = ValueInterface<char[]>.ReadValue(new RandomDataReader { MinArraySize = 1000, MaxArraySize = 10000 });
            json = JsonConvert.SerializeObject(data);
        }

        public bool NeedReset => false;

        public bool Deser => true;

        public bool Ser => true;

        public object Name => $"char[{data.Length}]";


        public char[] GetData()
        {
            return data;
        }

        public string GetJson()
        {
            return json;
        }

        public void Reset(char[] data)
        {
        }

        public void VerifyData(char[] data)
        {
        }

        public void VerifyJson(string json)
        {
        }
    }
}
