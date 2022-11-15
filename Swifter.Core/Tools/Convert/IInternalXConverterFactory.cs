namespace Swifter.Tools
{
    interface IInternalXConverterFactory : IXConverterFactory
    {
        XConvertMode Mode { get; }
    }
}