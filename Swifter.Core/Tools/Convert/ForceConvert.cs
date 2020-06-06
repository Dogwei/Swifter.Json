namespace Swifter.Tools
{
    internal sealed class ForceConvert<TSource, TDestination> : IXConverter<TSource, TDestination>
    {
        public TDestination Convert(TSource value) => (TDestination)(object)value;
    }
}