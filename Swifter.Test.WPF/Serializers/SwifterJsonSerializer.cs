using Swifter.Json;

namespace Swifter.Test.WPF.Serializers
{
    public sealed class SwifterJsonSerializer : BaseSerializer<string>
    {
        static SwifterJsonSerializer()
        {
            JsonFormatter.CharsPool.Ratio = 0;
        }

        public override TObject Deserialize<TObject>(string symbols)
        {
            return JsonFormatter.DeserializeObject<TObject>(symbols);
        }

        public override string Serialize<TObject>(TObject obj)
        {
            return JsonFormatter.SerializeObject(obj);
        }
    }
}
