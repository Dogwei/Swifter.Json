namespace Swifter.Test.WPF.Serializers
{
    public sealed class Utf8JsonSerializer : BaseSerializer<byte[]>
    {
        public override TObject Deserialize<TObject>(byte[] symbols)
        {
            return Utf8Json.JsonSerializer.Deserialize<TObject>(symbols);
        }

        public override byte[] Serialize<TObject>(TObject obj)
        {
            return Utf8Json.JsonSerializer.Serialize(obj);
        }
    }
}
