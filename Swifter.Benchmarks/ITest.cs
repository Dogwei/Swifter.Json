namespace Swifter.Benchmarks
{
    interface ITest<TData>
    {
        TData GetData();

        string GetJson();

        void Reset(TData data);

        void VerifyData(TData data);

        void VerifyJson(string json);

        bool NeedReset { get; }

        bool Deser { get; }

        bool Ser { get; }
        object Name { get; }
    }
}
