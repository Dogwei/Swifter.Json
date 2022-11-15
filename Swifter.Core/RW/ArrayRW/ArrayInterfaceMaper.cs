using System;

namespace Swifter.RW
{
    internal sealed class ArrayInterfaceMaper : IValueInterfaceMaper
    {
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