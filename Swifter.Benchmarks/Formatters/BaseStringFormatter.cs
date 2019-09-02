namespace Swifter.Benchmarks.Formatters
{
    abstract class BaseStringFormatter : IFormatter<string>
    {
        public abstract string FormatterName { get; } 

        public string ConvertFromJson(string json)
        {
            return json;
        }

        public abstract TData Deser<TData>(string meta);

        public abstract string Ser<TData>(TData data);

        public string ToJsonString(string meta)
        {
            return meta;
        }
    }
}
