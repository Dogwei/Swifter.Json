using Newtonsoft.Json;
using Swifter.Benchmarks.Modes;
using System;

namespace Swifter.Benchmarks.Tests
{
    sealed class CommonModeText : ITest<CommonData>
    {
        public bool NeedReset => false;

        public bool Deser => true;

        public bool Ser => true;

        public object Name => "CommonMode";

        public CommonData GetData()
        {
            return new CommonData
            {
                CacheToken = "Commands",
                Description = "新增命令信息",
                Code = "INSEET Commands(Id, Name) VALUES(@Id, @Name)",
                Demo = "{\"Id\":10,\"Name\":\"InsertCommand\"}",
                Id = 1,
                Name = "InsertCommand",
                RowNum = 10,
                Sign = 81,
                Type = "MSSQL-SYSTEM-DB",
                Guid = Guid.NewGuid()
            };
        }

        public string GetJson()
        {
            return JsonConvert.SerializeObject(GetData());
        }

        public void Reset(CommonData data)
        {
        }

        public void VerifyData(CommonData data)
        {
            var other = GetData();

            if (data.Name != other.Name || data.Description != other.Description)
            {
                throw new Exception();
            }
        }

        public void VerifyJson(string json)
        {
            VerifyData(JsonConvert.DeserializeObject<CommonData>(json));
        }
    }
}
