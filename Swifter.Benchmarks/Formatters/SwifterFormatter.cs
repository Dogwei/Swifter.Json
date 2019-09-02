using Swifter.Json;

namespace Swifter.Benchmarks.Formatters
{
    sealed class SwifterFormatter : BaseStringFormatter
    {
        public override string FormatterName => "SwifterJson";


        public override TData Deser<TData>(string meta)
        {
            return JsonFormatter.DeserializeObject<TData>(meta);
        }

        public override string Ser<TData>(TData data)
        {
            return JsonFormatter.SerializeObject(data);
        }
    }
}
