namespace Swifter.Tools
{
    internal sealed class ValueToNullableConvert<TSource, TDestination> : IXConverter<TSource, TDestination?> where TDestination : struct
    {
        public TDestination? Convert(TSource value)
        {
            if (value is null)
            {
                return null;
            }

            return XConvert.Convert<TSource, TDestination>(value);
        }
    }
}