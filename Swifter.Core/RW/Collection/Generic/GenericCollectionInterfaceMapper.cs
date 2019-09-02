using Swifter.RW;
using System;
using System.Collections.Generic;
using System.Text;

namespace Swifter.RW
{
    internal sealed class GenericCollectionInterfaceMapper : IValueInterfaceMaper
    {
        public IValueInterface<T> TryMap<T>()
        {
            if (typeof(T).IsArray)
            {
                return null;
            }

            if (TryMap(typeof(T), out var interfaceType))
            {
                return (IValueInterface<T>)Activator.CreateInstance(interfaceType);
            }

            foreach (var item in typeof(T).GetInterfaces())
            {
                if (TryMap(item, out interfaceType))
                {
                    return (IValueInterface<T>)Activator.CreateInstance(interfaceType);
                }
            }

            return null;

            static bool TryMap(Type type, out Type interfaceType)
            {
                if (type.IsGenericType)
                {
                    var typeDefinition = type.GetGenericTypeDefinition();
                    var genericArguments = type.GetGenericArguments();

                    if (typeDefinition == typeof(IDictionary<,>))
                    {
                        interfaceType = typeof(DictionaryInterface<,,>).MakeGenericType(typeof(T), genericArguments[0], genericArguments[1]);

                        return true;
                    }

                    if (typeDefinition == typeof(IList<>))
                    {
                        interfaceType = typeof(ListInterface<,>).MakeGenericType(typeof(T), genericArguments[0]);

                        return true;
                    }

                    if (typeDefinition == typeof(ICollection<>))
                    {
                        interfaceType = typeof(CollectionInterface<,>).MakeGenericType(typeof(T), genericArguments[0]);

                        return true;
                    }

                    if (typeDefinition == typeof(IEnumerable<>))
                    {
                        interfaceType = typeof(EnumerableInterface<,>).MakeGenericType(typeof(T), genericArguments[0]);

                        return true;
                    }

                    if (typeDefinition == typeof(IEnumerator<>))
                    {
                        interfaceType = typeof(EnumeratorInterface<,>).MakeGenericType(typeof(T), genericArguments[0]);

                        return true;
                    }
                }

                interfaceType = default;

                return false;
            }
        }
    }
}