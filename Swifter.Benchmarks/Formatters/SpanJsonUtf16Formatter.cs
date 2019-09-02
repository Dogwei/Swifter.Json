using SpanJson;

namespace Swifter.Benchmarks.Formatters
{
    sealed class SpanJsonUtf16Formatter : BaseStringFormatter
    {
        public override string FormatterName => "SpanJsonUtf16";


        public override TData Deser<TData>(string meta)
        {
            return JsonSerializer.Generic.Utf16.Deserialize<TData>(meta);
        }

        public override string Ser<TData>(TData data)
        {
            return JsonSerializer.Generic.Utf16.Serialize(data);
        }
    }
}
