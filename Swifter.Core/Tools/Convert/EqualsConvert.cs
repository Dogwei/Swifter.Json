namespace Swifter.Tools
{
    internal sealed class EqualsConvert<T> : IXConverter<T, T>
    {
        public T Convert(T value) => value;
    }
}