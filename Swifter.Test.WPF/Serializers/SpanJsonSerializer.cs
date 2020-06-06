namespace Swifter.Test.WPF.Serializers
{
    public sealed class SpanJsonSerializer : BaseSerializer<string>
    {
        public override TObject Deserialize<TObject>(string symbols)
        {
            return SpanJson.JsonSerializer.Generic.Utf16.Deserialize<TObject>(symbols);
        }

        public override string Serialize<TObject>(TObject obj)
        {
            return SpanJson.JsonSerializer.Generic.Utf16.Serialize(obj);
        }
    }
}
