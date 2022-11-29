using System;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.RW
{
    sealed class UnknowTypeInterfaceMaper : IValueInterfaceMaper
    {
#if NET7_0_OR_GREATER
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(UnknowTypeInterface<>))]
#endif
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