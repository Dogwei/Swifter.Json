using System;
using System.Collections;

namespace Swifter.RW
{
    internal sealed class CollectionInterfaceMaper : IValueInterfaceMaper
    {
        public IValueInterface<T> TryMap<T>()
        {
            if (typeof(Array).IsAssignableFrom(typeof(T)))
            {
                return null;
            }

            if (typeof(IDictionary).IsAssignableFrom(typeof(T)))
            {
                return CreateInstance(typeof(DictionaryInterface<>).MakeGenericType(typeof(T)));
            }

            if (typeof(IList).IsAssignableFrom(typeof(T)))
            {
                return CreateInstance(typeof(ListInterface<>).MakeGenericType(typeof(T)));
            }

            if (typeof(ICollection).IsAssignableFrom(typeof(T)))
            {
                return CreateInstance(typeof(CollectionInterface<>).MakeGenericType(typeof(T)));
            }

            if (typeof(IEnumerable).IsAssignableFrom(typeof(T)))
            {
                return CreateInstance(typeof(EnumerableInterface<>).MakeGenericType(typeof(T)));
            }

            if (typeof(IEnumerator).IsAssignableFrom(typeof(T)))
            {
                return CreateInstance(typeof(EnumeratorInterface<>).MakeGenericType(typeof(T)));
            }

            return null;

            static IValueInterface<T> CreateInstance(Type type)
            {
                return (IValueInterface<T>)Activator.CreateInstance(type);
            }
        }
    }
}