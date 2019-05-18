using System;

namespace Swifter.RW
{
    internal sealed class CallbackEvents
    {
        public event Action<Type> TypeCreated;

        public void OnTypeCreated(Type type)
        {
            TypeCreated?.Invoke(type);
        }
    }
}