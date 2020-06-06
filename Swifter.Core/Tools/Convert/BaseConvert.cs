namespace Swifter.Tools
{
    internal sealed class BaseConvert<TBase, T> : IXConverter<TBase, T> where T : TBase
    {
        public T Convert(TBase value) => (T)value;
    }
}