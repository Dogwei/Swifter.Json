using LitJson;

namespace Swifter.Benchmarks.Formatters
{
    sealed class LitJsonFormatter : BaseStringFormatter
    {
        public override string FormatterName => "LitJson";


        public override TData Deser<TData>(string meta)
        {
            return JsonMapper.ToObject<TData>(meta);
        }

        public override string Ser<TData>(TData data)
        {
            return JsonMapper.ToJson(data);
        }
    }
}
