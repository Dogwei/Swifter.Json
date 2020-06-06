using System;

namespace Swifter.Tools
{
    internal sealed class NullableToValueConvert<TSource, TDestination> : IXConverter<TSource?, TDestination> where TSource : struct
    {
        public TDestination Convert(TSource? value)
        {
            if (value == null)
            {
                return default;
            }

            return XConvert.Convert<TSource, TDestination>(value.Value);
        }
    }
}