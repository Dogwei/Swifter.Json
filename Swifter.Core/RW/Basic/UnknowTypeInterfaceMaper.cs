using System;

namespace Swifter.RW
{
    sealed class UnknowTypeInterfaceMaper : IValueInterfaceMaper
    {
        public IValueInterface<T>? TryMap<T>()
        {
            if (typeof(T).IsInterface || typeof(T).IsAbstract || typeof(IFormattable).IsAssignableFrom(typeof(T)))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(UnknowTypeInterface<>).MakeGenericType(typeof(T)))!;
            }

            return null;
        }
    }
}