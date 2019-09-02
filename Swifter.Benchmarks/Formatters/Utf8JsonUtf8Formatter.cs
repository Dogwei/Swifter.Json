using Utf8Json;

namespace Swifter.Benchmarks.Formatters
{
    sealed class Utf8JsonUtf8Formatter : BaseUtf8Formatter
    {
        public override string FormatterName => "Utf8JsonUtf8";

        public override TData Deser<TData>(byte[] meta)
        {
            return JsonSerializer.Deserialize<TData>(meta);
        }

        public override byte[] Ser<TData>(TData data)
        {
            return JsonSerializer.Serialize(data);
        }
    }
}
