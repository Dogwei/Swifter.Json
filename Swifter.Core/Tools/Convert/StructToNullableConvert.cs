namespace Swifter.Tools
{
    internal sealed class StructToNullableConvert<T> : IXConverter<T, T?> where T : struct
    {
        public T? Convert(T value) => value;
    }
}