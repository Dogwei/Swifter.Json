namespace Swifter.Test.WPF.Serializers
{
    public sealed class KoobooJsonSerializer : BaseSerializer<string>
    {
        public override TObject Deserialize<TObject>(string symbols)
        {
            return Kooboo.Json.JsonSerializer.ToObject<TObject>(symbols);
        }

        public override string Serialize<TObject>(TObject obj)
        {
            return Kooboo.Json.JsonSerializer.ToJson(obj);
        }
    }
}
