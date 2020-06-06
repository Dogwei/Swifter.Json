using Newtonsoft.Json;

namespace Swifter.Test.WPF.Serializers
{
    public sealed class NewtonsoftJsonSerializer : BaseSerializer<string>
    {
        public override TObject Deserialize<TObject>(string symbols)
        {
            return JsonConvert.DeserializeObject<TObject>(symbols);
        }

        public override string Serialize<TObject>(TObject obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
