using Swifter.MessagePack;

namespace Swifter.Test.WPF.Serializers
{
    public sealed class SwifterMessagePackSerializer : BaseSerializer<byte[]>
    {
        static SwifterMessagePackSerializer()
        {
            MessagePackFormatter.BytesPool.Ratio = 0;
        }

        public override TObject Deserialize<TObject>(byte[] symbols)
        {
            return MessagePackFormatter.DeserializeObject<TObject>(symbols, MessagePackFormatterOptions.IgnoreNull | MessagePackFormatterOptions.IgnoreZero);
        }

        public override byte[] Serialize<TObject>(TObject obj)
        {
            return MessagePackFormatter.SerializeObject(obj);
        }
    }
}
