using Newtonsoft.Json;
using Swifter.Benchmarks.Modes;
using Swifter.RW;
using System;
using System.Collections.Generic;

namespace Swifter.Benchmarks.Tests
{
    sealed class LongStringTest : ITest<string>
    {
        readonly string data = "qwertyuiopasdfghjklzxcvbnmqwertyuiopasdfghjklzxcvbnm~!@#$%^&*()_+\"'\\//";
        readonly string json;

        public LongStringTest()
        {
            data = data + data + data + data + data;

            json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }

        public bool NeedReset => false;

        public bool Deser => true;

        public bool Ser => true;

        public object Name => "LongString";

        public string GetData()
        {
            return data;
        }

        public string GetJson()
        {
            return json;
        }

        public void Reset(string data)
        {
        }

        public void VerifyData(string data)
        {
            if (data != this.data)
            {
                throw new Exception();
            }
        }

        public void VerifyJson(string json)
        {
            if (json != this.json && Newtonsoft.Json.JsonConvert.DeserializeObject<string>(json) != data)
            {
                throw new Exception();
            }
        }
    }

    sealed class ShortStringTest : ITest<string>
    {
        readonly string data = "qwertyuiop";
        readonly string json;

        public ShortStringTest()
        {
            data = data + data + data + data + data;

            json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        }

        public bool NeedReset => false;

        public bool Deser => true;

        public bool Ser => true;

        public object Name => "ShortString";

        public string GetData()
        {
            return data;
        }

        public string GetJson()
        {
            return json;
        }

        public void Reset(string data)
        {
        }

        public void VerifyData(string data)
        {
            if (data != this.data)
            {
                throw new Exception();
            }
        }

        public void VerifyJson(string json)
        {
            if (json != this.json && Newtonsoft.Json.JsonConvert.DeserializeObject<string>(json) != data)
            {
                throw new Exception();
            }
        }
    }

    sealed class DictionaryTest : ITest<Dictionary<string, CommonData>>
    {
        readonly Dictionary<string, CommonData> data;
        readonly string json;

        public DictionaryTest()
        {
            var random = new RandomDataReader();

            data = new Dictionary<string, CommonData>
            {
                { ValueInterface<string>.ReadValue(random), new CommonModeText().GetData() },
                { ValueInterface<string>.ReadValue(random), new CommonModeText().GetData() },
                { ValueInterface<string>.ReadValue(random), new CommonModeText().GetData() },
                { ValueInterface<string>.ReadValue(random), new CommonModeText().GetData() },
                { ValueInterface<string>.ReadValue(random), new CommonModeText().GetData() },
            };

            json = JsonConvert.SerializeObject(data);
        }

        public bool NeedReset => false;

        public bool Deser => true;

        public bool Ser => true;

        public object Name => "Dictionary<string, object>";

        public Dictionary<string, CommonData> GetData()
        {
            return data;
        }

        public string GetJson()
        {
            return json;
        }

        public void Reset(Dictionary<string, CommonData> data)
        {
        }

        public void VerifyData(Dictionary<string, CommonData> data)
        {
            if (data.Count != 5)
            {
                throw new Exception();
            }
        }

        public void VerifyJson(string json)
        {
            if (json.Length <= 50)
            {
                throw new Exception();
            }
        }
    }


    ////sealed class ListTest : ITest<List<CommonData>>
    ////{
    ////    readonly List<CommonData> data;
    ////    readonly string json;

    ////    public DictionaryTest()
    ////    {
    ////        var random = new RandomDataReader();

    ////        data = new List<CommonData>
    ////        {
    ////            new CommonModeText().GetData(),
    ////            new CommonModeText().GetData(),
    ////            new CommonModeText().GetData(),
    ////            new CommonModeText().GetData(),
    ////            new CommonModeText().GetData(),
    ////        };

    ////        json = JsonConvert.SerializeObject(data);
    ////    }

    ////    public bool NeedReset => false;

    ////    public bool Deser => true;

    ////    public bool Ser => true;

    ////    public object Name => "List<object>";

    ////    public Dictionary<string, CommonData> GetData()
    ////    {
    ////        return data;
    ////    }

    ////    public string GetJson()
    ////    {
    ////        return json;
    ////    }

    ////    public void Reset(Dictionary<string, CommonData> data)
    ////    {
    ////    }

    ////    public void VerifyData(Dictionary<string, CommonData> data)
    ////    {
    ////        if (data.Count != 5)
    ////        {
    ////            throw new Exception();
    ////        }
    ////    }

    ////    public void VerifyJson(string json)
    ////    {
    ////        if (json.Length <= 50)
    ////        {
    ////            throw new Exception();
    ////        }
    ////    }
    ////}
}
