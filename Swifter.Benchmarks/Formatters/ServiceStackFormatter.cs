using ServiceStack.Text;

namespace Swifter.Benchmarks.Formatters
{
    sealed class ServiceStackFormatter : BaseStringFormatter
    {
        public override string FormatterName => "ServiceStack";


        public override TData Deser<TData>(string meta)
        {
            return JsonSerializer.DeserializeFromString<TData>(meta);
        }

        public override string Ser<TData>(TData data)
        {
            return JsonSerializer.SerializeToString(data);
        }
    }
}
