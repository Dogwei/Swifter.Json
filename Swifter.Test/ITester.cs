using LitJson;
using Newtonsoft.Json;
using Swifter.Json;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Text;

namespace Swifter.Test
{
    public interface ITester
    {
        string Name { get; }

        T Deserialize<T>(string text);

        string Serialize<T>(T obj);
    }

    public class SwifterTester : ITester
    {
        public string Name => "Swifter.Json";

        public T Deserialize<T>(string text)
        {
            return JsonFormatter.DeserializeObject<T>(text);
        }

        public string Serialize<T>(T obj)
        {
            return JsonFormatter.SerializeObject(obj);
        }
    }

    public class NewtonsoftTester : ITester
    {
        public string Name => "Newtonsoft.Json";

        public T Deserialize<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text);
        }

        public string Serialize<T>(T obj)
        {
            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<UnsafeClass>.Int64TypeHandle)
            {
                throw new Exception("Memory");
            }

            return JsonConvert.SerializeObject(obj);
        }
    }

    public class LitJsonTester : ITester
    {
        public string Name => "LitJson";

        public T Deserialize<T>(string text)
        {
            return JsonMapper.ToObject<T>(text);
        }

        public string Serialize<T>(T obj)
        {
            return JsonMapper.ToJson(obj);
        }
    }

    public class JilTester : ITester
    {
        public string Name => "Jil";

        public T Deserialize<T>(string text)
        {
            return Jil.JSON.Deserialize<T>(text);
        }

        public string Serialize<T>(T obj)
        {
            return Jil.JSON.Serialize(obj);
        }
    }

    public class Utf8JsonTester : ITester
    {
        public string Name => "Utf8Json";

        public T Deserialize<T>(string text)
        {
            return Utf8Json.JsonSerializer.Deserialize<T>(text);
        }

        public string Serialize<T>(T obj)
        {
            return Utf8Json.JsonSerializer.ToJsonString(obj);
        }
    }

    public class FastJsonTester : ITester
    {
        public string Name => "fastJSON";

        public T Deserialize<T>(string text)
        {
            return fastJSON.JSON.ToObject<T>(text);
        }

        public string Serialize<T>(T obj)
        {
            return fastJSON.JSON.ToJSON(obj);
        }
    }

    public class NetJSONTestter : ITester
    {
        private NetJSON.NetJSONSettings settings = new NetJSON.NetJSONSettings();

        public NetJSONTestter()
        {
            settings.SkipDefaultValue = false;
        }

        public string Name => "NetJSON";

        public T Deserialize<T>(string text)
        {
            // TODO : Known errors in NetJSON directly return TimeoutException in order to the test program execute normally.

            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<Dictionary<string, object>>.Int64TypeHandle)
            {
                throw new TimeoutException();
            }

            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<Polymorphism>.Int64TypeHandle)
            {
                throw new TimeoutException();
            }

            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<TestStruct>.Int64TypeHandle)
            {
                throw new TimeoutException();
            }

            return NetJSON.NetJSON.Deserialize<T>(text);
        }

        public string Serialize<T>(T obj)
        {
            // TODO : Known errors in NetJSON directly return TimeoutException in order to the test program execute normally.

            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<Dictionary<string, object>>.Int64TypeHandle)
            {
                throw new TimeoutException();
            }

            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<Polymorphism>.Int64TypeHandle)
            {
                throw new TimeoutException();
            }

            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<TestStruct>.Int64TypeHandle)
            {
                throw new TimeoutException();
            }

            return NetJSON.NetJSON.Serialize(obj, settings);
        }
    }

    public class ServiceStackTester : ITester
    {
        public string Name => "ServiceStack";

        public T Deserialize<T>(string text)
        {
            return ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(text);
        }

        public string Serialize<T>(T obj)
        {
            return ServiceStack.Text.JsonSerializer.SerializeToString(obj);
        }
    }
}
