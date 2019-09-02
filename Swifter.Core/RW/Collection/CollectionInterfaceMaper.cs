using System;
using System.Collections;

namespace Swifter.RW
{
    internal sealed class CollectionInterfaceMaper : IValueInterfaceMaper
    {
        public IValueInterface<T> TryMap<T>()
        {
            if (typeof(T).IsArray)
            {
                return null;
            }

            if (typeof(IDictionary).IsAssignableFrom(typeof(T)))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(DictionaryInterface<>).MakeGenericType(typeof(T)));
            }

            if (typeof(IList).IsAssignableFrom(typeof(T)))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(ListInterface<>).MakeGenericType(typeof(T)));
            }

            if (typeof(ICollection).IsAssignableFrom(typeof(T)))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(CollectionInterface<>).MakeGenericType(typeof(T)));
            }

            if (typeof(IEnumerable).IsAssignableFrom(typeof(T)))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(EnumerableInterface<>).MakeGenericType(typeof(T)));
            }

            if (typeof(IEnumerator).IsAssignableFrom(typeof(T)))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(EnumeratorInterface<>).MakeGenericType(typeof(T)));
            }

            return null;
        }
    }
}