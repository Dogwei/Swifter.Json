using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Swifter.RW
{
    sealed class BasicInterfaceMaper : IValueInterfaceMaper
    {
        static readonly object[] BasicInterfaces =
        {
            new BooleanInterface(),
            new SByteInterface(),
            new Int16Interface(),
            new Int32Interface(),
            new Int64Interface(),
            new ByteInterface(),
            new UInt16Interface(),
            new UInt32Interface(),
            new UInt64Interface(),
            new CharInterface(),
            new SingleInterface(),
            new DoubleInterface(),
            new DecimalInterface(),
            new StringInterface(),
            new ObjectInterface(),
            new DateTimeInterface(),
            new DateTimeOffsetInterface(),
            new TimeSpanInterface(),
            new GuidInterface(),
            new IntPtrInterface(),
            new VersionInterface(),
            new DbNullInterface(),
            new UriInterface(),
        };

#if NET7_0_OR_GREATER
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TypeInfoInterface<>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(MemberInfoInterface<>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(AssemblyInterface<>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(EnumInterface<>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(NullableInterface<>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DataReaderInterface<,>))]
#endif
        public IValueInterface<T>? TryMap<T>()
        {
            foreach (var item in BasicInterfaces)
            {
                if (item is IValueInterface<T> valueInterface)
                {
                    return valueInterface;
                }
            }

            if (typeof(Type).IsAssignableFrom(typeof(T)))
            {
                return CreateInstance((typeof(TypeInfoInterface<>)).MakeGenericType(typeof(T)));
            }

            if (typeof(MemberInfo).IsAssignableFrom(typeof(T)))
            {
                return CreateInstance((typeof(MemberInfoInterface<>)).MakeGenericType(typeof(T)));
            }

            if (typeof(Assembly).IsAssignableFrom(typeof(T)))
            {
                return CreateInstance((typeof(AssemblyInterface<>)).MakeGenericType(typeof(T)));
            }

            if (typeof(T).IsEnum)
            {
                return CreateInstance(typeof(EnumInterface<>).MakeGenericType(typeof(T)));
            }

            if (default(T) is null && Nullable.GetUnderlyingType(typeof(T)) is Type underlyingType)
            {
                return CreateInstance(typeof(NullableInterface<>).MakeGenericType(underlyingType))!;
            }

            if (typeof(IDataReader).IsAssignableFrom(typeof(T)))
            {
                foreach (var item in typeof(T).GetInterfaces())
                {
                    if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IDataReader<>))
                    {
                        var keyType = item.GetGenericArguments()[0];

                        return CreateInstance((typeof(DataReaderInterface<,>)).MakeGenericType(typeof(T), keyType));
                    }
                }
            }


            return null;

            static IValueInterface<T> CreateInstance(Type type)
            {
                return CreateInstance(type)!;
            }
        }
    }
}