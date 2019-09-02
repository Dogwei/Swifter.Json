using Newtonsoft.Json;

namespace Swifter.Benchmarks.Formatters
{
    sealed class NewtonsoftFormatter : BaseStringFormatter
    {
        public override string FormatterName => "NewtonsoftJson";

        public override TData Deser<TData>(string meta)
        {
            return JsonConvert.DeserializeObject<TData>(meta);
        }

        public override string Ser<TData>(TData data)
        {
            return JsonConvert.SerializeObject(data);
        }
    }

}
