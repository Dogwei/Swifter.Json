using fastJSON;

namespace Swifter.Benchmarks.Formatters
{
    sealed class FastJSONFormatter : BaseStringFormatter
    {
        public override string FormatterName => "fastJSON";


        public override TData Deser<TData>(string meta)
        {
            return JSON.ToObject<TData>(meta);
        }

        public override string Ser<TData>(TData data)
        {
            return JSON.ToJSON(data);
        }
    }
}
