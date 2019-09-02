using Jil;

namespace Swifter.Benchmarks.Formatters
{
    sealed class JilFormatter : BaseStringFormatter
    {
        public override string FormatterName => "Jil";


        public override TData Deser<TData>(string meta)
        {
            return JSON.Deserialize<TData>(meta);
        }

        public override string Ser<TData>(TData data)
        {
            return JSON.Serialize(data);
        }
    }
}
