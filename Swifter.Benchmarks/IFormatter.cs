namespace Swifter.Benchmarks
{
    interface IFormatter<TMeta>
    {
        string FormatterName { get; }

        TData Deser<TData>(TMeta meta);

        TMeta Ser<TData>(TData data);

        string ToJsonString(TMeta meta);

        TMeta ConvertFromJson(string json);
    }
}
