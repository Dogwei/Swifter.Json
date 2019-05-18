namespace Swifter.Tools
{
    internal interface IGenericInvoker
    {
        void Invoke<TKey>();
    }
    
    internal interface IGenericInvoker<TResult>
    {
        TResult Invoke<TKey>();
    }
}