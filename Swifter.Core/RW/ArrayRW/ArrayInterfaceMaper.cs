using System;

namespace Swifter.RW
{
    internal sealed class ArrayInterfaceMaper : IValueInterfaceMaper
    {
        public IValueInterface<T> TryMap<T>()
        {
            if (typeof(T).IsArray)
            {
                var elementType = typeof(T).GetElementType();

                Type interfaceType;

                if (typeof(T).GetArrayRank() == 1 && typeof(T) == elementType.MakeArrayType())
                {
                    interfaceType = typeof(ArrayInterface<>).MakeGenericType(elementType);
                }
                else
                {
                    interfaceType = typeof(MultiDimArrayInterface<,>).MakeGenericType(typeof(T), elementType);
                }

                return Underlying.As<IValueInterface<T>>(Activator.CreateInstance(interfaceType));
            }

            return null;
        }
    }
}