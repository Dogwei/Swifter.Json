using System.Text.Json.Serialization;

namespace Swifter.Benchmarks.Formatters
{
    sealed class SystemTextJsonUtf8Formatter : BaseUtf8Formatter
    {
        public override string FormatterName => "SystemTextJsonUtf8";

        public override TData Deser<TData>(byte[] meta)
        {
            return JsonSerializer.Parse<TData>(meta);
        }

        public override byte[] Ser<TData>(TData data)
        {
            return JsonSerializer.ToBytes(data);
        }
    }
}
