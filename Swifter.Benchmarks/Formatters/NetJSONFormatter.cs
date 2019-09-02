namespace Swifter.Benchmarks.Formatters
{
    sealed class NetJSONFormatter : BaseStringFormatter
    {
        static readonly NetJSON.NetJSONSettings settings = new NetJSON.NetJSONSettings();

        static NetJSONFormatter()
        {
            settings.SkipDefaultValue = false;
        }

        public override string FormatterName => "NetJSON";


        public override TData Deser<TData>(string meta)
        {
            return NetJSON.NetJSON.Deserialize<TData>(meta);
        }

        public override string Ser<TData>(TData data)
        {
            return NetJSON.NetJSON.Serialize(data, settings);
        }
    }
}
