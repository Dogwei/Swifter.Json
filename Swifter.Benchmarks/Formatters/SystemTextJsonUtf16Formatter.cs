using System.Text.Json.Serialization;

namespace Swifter.Benchmarks.Formatters
{
    sealed class SystemTextJsonUtf16Formatter : BaseStringFormatter
    {
        public override string FormatterName => "SystemTextJsonUtf16";

        public override TData Deser<TData>(string meta)
        {
            return JsonSerializer.Parse<TData>(meta);
        }

        public override string Ser<TData>(TData data)
        {
            return JsonSerializer.ToString(data);
        }
    }
}
