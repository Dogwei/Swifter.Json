using SpanJson;

namespace Swifter.Benchmarks.Formatters
{
    sealed class SpanJsonUtf8Formatter : BaseUtf8Formatter
    {
        public override string FormatterName => "SpanJsonUtf8";

        public override TData Deser<TData>(byte[] meta)
        {
            return JsonSerializer.Generic.Utf8.Deserialize<TData>(meta);
        }

        public override byte[] Ser<TData>(TData data)
        {
            return JsonSerializer.Generic.Utf8.Serialize(data);
        }
    }
}
