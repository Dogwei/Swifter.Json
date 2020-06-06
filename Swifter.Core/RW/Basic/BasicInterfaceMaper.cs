using System;
using System.Reflection;

namespace Swifter.RW
{
    sealed class BasicInterfaceMaper : IValueInterfaceMaper
    {
        public IValueInterface<T> TryMap<T>()
        {
            if (typeof(Type).IsAssignableFrom(typeof(T)))
            {
                return (IValueInterface<T>)Activator.CreateInstance((typeof(TypeInfoInterface<>)).MakeGenericType(typeof(T)));
            }

            if (typeof(MemberInfo).IsAssignableFrom(typeof(T)))
            {
                return (IValueInterface<T>)Activator.CreateInstance((typeof(MemberInfoInterface<>)).MakeGenericType(typeof(T)));
            }

            if (typeof(Assembly).IsAssignableFrom(typeof(T)))
            {
                return (IValueInterface<T>)Activator.CreateInstance((typeof(AssemblyInterface<>)).MakeGenericType(typeof(T)));
            }

            if (typeof(T).IsEnum)
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(EnumInterface<>).MakeGenericType(typeof(T)));
            }

            if (typeof(T).IsValueType && typeof(T).IsGenericType && Nullable.GetUnderlyingType(typeof(T)) is Type underlyingType && typeof(T) != underlyingType)
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(NullableInterface<>).MakeGenericType(underlyingType));
            }

            if (typeof(IDataReader).IsAssignableFrom(typeof(T)))
            {
                foreach (var item in typeof(T).GetInterfaces())
                {
                    if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IDataReader<>))
                    {
                        var keyType = item.GetGenericArguments()[0];

                        return (IValueInterface<T>)Activator.CreateInstance((typeof(DataReaderInterface<,>)).MakeGenericType(typeof(T), keyType));
                    }
                }
            }

            if (typeof(T).IsInterface || typeof(T).IsAbstract || typeof(IFormattable).IsAssignableFrom(typeof(T)))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(UnknowTypeInterface<>).MakeGenericType(typeof(T)));
            }

            return null;
        }
    }
}