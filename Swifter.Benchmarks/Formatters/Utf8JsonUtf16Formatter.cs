using Utf8Json;

namespace Swifter.Benchmarks.Formatters
{
    sealed class Utf8JsonUtf16Formatter : BaseStringFormatter
    {
        public override string FormatterName => "Utf8JsonUtf16";

        public override TData Deser<TData>(string meta)
        {
            return JsonSerializer.Deserialize<TData>(meta);
        }

        public override string Ser<TData>(TData data)
        {
            return JsonSerializer.ToJsonString(data);
        }
    }
}
