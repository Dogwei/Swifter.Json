namespace Swifter.Tools
{
    internal sealed class AssignableConvert<T, TBase> : IXConverter<T, TBase> where T : TBase
    {
        public TBase Convert(T value) => value;
    }
}