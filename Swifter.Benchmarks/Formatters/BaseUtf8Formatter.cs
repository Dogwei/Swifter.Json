using System.Text;

namespace Swifter.Benchmarks.Formatters
{
    abstract class BaseUtf8Formatter : IFormatter<byte[]>
    {
        public abstract string FormatterName { get; }

        public byte[] ConvertFromJson(string json)
        {
            return Encoding.UTF8.GetBytes(json);
        }

        public abstract TData Deser<TData>(byte[] meta);

        public abstract byte[] Ser<TData>(TData data);

        public string ToJsonString(byte[] meta)
        {
            return Encoding.UTF8.GetString(meta);
        }
    }
}
