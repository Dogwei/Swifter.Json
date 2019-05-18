namespace Swifter.RW
{
    internal sealed class InternalInstance<T> where T : class
    {
        public T Instance;

        public bool IsUsed;
    }
}