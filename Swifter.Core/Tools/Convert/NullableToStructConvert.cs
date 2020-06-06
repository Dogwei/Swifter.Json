namespace Swifter.Tools
{
    internal sealed class NullableToStructConvert<T> : IXConverter<T?, T> where T : struct
    {
        public T Convert(T? value) => value.Value;
    }
}