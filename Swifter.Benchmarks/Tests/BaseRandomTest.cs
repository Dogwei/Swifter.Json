using Newtonsoft.Json;
using Swifter.RW;
using System;

namespace Swifter.Benchmarks.Tests
{
    abstract class BaseRandomTest<T> : ITest<T>
    {
        readonly T data;
        readonly string json;

        public BaseRandomTest(RandomDataReader random)
        {
            data = ValueInterface<T>.ReadValue(random);
            json = ToJson(data);
        }

        public virtual bool NeedReset => false;

        public virtual bool Deser => true;

        public virtual bool Ser => true;

        public virtual object Name => typeof(T).Name;

        public virtual T GetData() => data;

        public virtual string GetJson() => json;

        public virtual void Reset(T data)
        {
        }

        public virtual void VerifyData(T data)
        {
            if (json != ToJson(data))
            {
                throw new Exception();
            }
        }

        public virtual void VerifyJson(string json)
        {
            if (this.json != json)
            {
                throw new Exception();
            }
        }

        public virtual string ToJson(T data) => JsonConvert.SerializeObject(data);
    }
}
