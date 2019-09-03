using System;
using System.Collections.Generic;
using System.Linq;

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

            var das = GetDas().ToList();

            foreach (var (definition, arguments) in das)
            {
                if (definition == typeof(IDictionary<,>))
                {
                    return CreateInstance(typeof(DictionaryInterface<,,>).MakeGenericType(typeof(T), arguments[0], arguments[1]));
                }
            }

            foreach (var (definition, arguments) in das)
            {
                if (definition == typeof(IList<>))
                {
                    return CreateInstance(typeof(ListInterface<,>).MakeGenericType(typeof(T), arguments[0]));
                }
            }

            foreach (var (definition, arguments) in das)
            {
                if (definition == typeof(ICollection<>))
                {
                    return CreateInstance(typeof(CollectionInterface<,>).MakeGenericType(typeof(T), arguments[0]));
                }
            }

            foreach (var (definition, arguments) in das)
            {
                if (definition == typeof(IEnumerable<>))
                {
                    return CreateInstance(typeof(EnumerableInterface<,>).MakeGenericType(typeof(T), arguments[0]));
                }
            }

            foreach (var (definition, arguments) in das)
            {
                if (definition == typeof(IEnumerator<>))
                {
                    return CreateInstance(typeof(EnumeratorInterface<,>).MakeGenericType(typeof(T), arguments[0]));
                }
            }

            return null;

            static IEnumerable<(Type definition, Type[] arguments)> GetDas()
            {
                if (typeof(T).IsInterface && typeof(T).IsGenericType)
                {
                    yield return As(typeof(T));
                }

                foreach (var item in typeof(T).GetInterfaces())
                {
                    if (item.IsGenericType)
                    {
                        yield return As(item);
                    }
                }

                static (Type definition, Type[] arguments) As(Type type) => (type.GetGenericTypeDefinition(), type.GetGenericArguments());
            }

            static IValueInterface <T> CreateInstance(Type type)
            {
                return (IValueInterface<T>)Activator.CreateInstance(type);
            }
        }
    }
}