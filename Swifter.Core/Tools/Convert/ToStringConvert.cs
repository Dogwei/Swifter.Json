namespace Swifter.Tools
{
    internal sealed class ToStringConvert<T> : IXConverter<T, string>
    {
        public string Convert(T value) => value?.ToString();
    }
}