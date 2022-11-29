using System;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.RW
{
    internal sealed class ArrayInterfaceMaper : IValueInterfaceMaper
    {
#if NET7_0_OR_GREATER
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ArrayInterface<>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(MultiDimArrayInterface<,>))]
#endif
        public IValueInterface<T>? TryMap<T>()
        {
            if (!typeof(T).IsArray)
            {
                return null;
            }

            var elementType = typeof(T).GetElementType()!;

            Type interfaceType;

            if (typeof(T).IsSZArray())
            {
                interfaceType = typeof(ArrayInterface<>).MakeGenericType(elementType);
            }
            else
            {
                interfaceType = typeof(MultiDimArrayInterface<,>).MakeGenericType(typeof(T), elementType);
            }

            return (IValueInterface<T>)Activator.CreateInstance(interfaceType)!;
        }
    }
}