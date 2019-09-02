using Swifter.RW;

namespace Swifter.Benchmarks.Tests
{
    sealed class ByteArrayTest : ITest<byte[]>
    {
        readonly byte[] data;
        readonly string json;

        public ByteArrayTest()
        {
            data = ValueInterface<byte[]>.ReadValue(new RandomDataReader { MinArraySize = 1000, MaxArraySize = 10000 });
            json = LitJson.JsonMapper.ToJson(data);
        }

        public bool NeedReset => false;

        public bool Deser => true;

        public bool Ser => true;

        public object Name => $"byte[{data.Length}]";


        public byte[] GetData()
        {
            return data;
        }

        public string GetJson()
        {
            return json;
        }

        public void Reset(byte[] data)
        {
        }

        public void VerifyData(byte[] data)
        {
            VerifyJson(LitJson.JsonMapper.ToJson(data));
        }

        public void VerifyJson(string json)
        {
            if (json != this.json)
            {
                throw new System.Exception();
            }
        }
    }
}
