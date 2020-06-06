using MessagePack;

namespace Swifter.Test.WPF.Serializers
{
    public sealed class NeueccMessagePackSerializer : BaseSerializer<byte[]>
    {
        public override TObject Deserialize<TObject>(byte[] symbols)
        {
            return MessagePackSerializer.Deserialize<TObject>(symbols);
        }

        public override byte[] Serialize<TObject>(TObject obj)
        {
            return MessagePackSerializer.Serialize(obj);
        }
    }
}
