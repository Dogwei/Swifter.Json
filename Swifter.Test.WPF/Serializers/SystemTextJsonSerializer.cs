namespace Swifter.Test.WPF.Serializers
{
    public sealed class SystemTextJsonSerializer : BaseSerializer<string>
    {
        public override TObject Deserialize<TObject>(string symbols)
        {
            return System.Text.Json.JsonSerializer.Deserialize<TObject>(symbols);
        }

        public override string Serialize<TObject>(TObject obj)
        {
            return System.Text.Json.JsonSerializer.Serialize(obj);
        }
    }
}
