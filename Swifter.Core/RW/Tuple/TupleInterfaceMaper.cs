﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.RW
{
    sealed class TupleInterfaceMaper : IValueInterfaceMaper
    {
        public static readonly Dictionary<Type, Type> Map = new Dictionary<Type, Type>
        {
            { typeof(ValueTuple), typeof(TupleInterface) },
            { typeof(ValueTuple<>), typeof(TupleInterface<>) },
            { typeof(ValueTuple<,>), typeof(TupleInterface<,>) },
            { typeof(ValueTuple<,,>), typeof(TupleInterface<,,>) },
            { typeof(ValueTuple<,,,>), typeof(TupleInterface<,,,>) },
            { typeof(ValueTuple<,,,,>), typeof(TupleInterface<,,,,>) },
            { typeof(ValueTuple<,,,,,>), typeof(TupleInterface<,,,,,>) },
            { typeof(ValueTuple<,,,,,,>), typeof(TupleInterface<,,,,,,>) },
            { typeof(ValueTuple<,,,,,,,>), typeof(TupleInterface<,,,,,,,>) },
        };

#if NET7_0_OR_GREATER
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TupleInterface))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TupleInterface<>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TupleInterface<,>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TupleInterface<,,>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TupleInterface<,,,>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TupleInterface<,,,,>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TupleInterface<,,,,,>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TupleInterface<,,,,,,>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(TupleInterface<,,,,,,,>))]
#endif
        public IValueInterface<T>? TryMap<T>()
        {
            if (ValueInterface.ValueTupleSupport)
            {
                if (typeof(T).IsValueType)
                {
                    if (typeof(T).IsGenericType)
                    {
                        var definition = typeof(T).GetGenericTypeDefinition();
                        var arguments = typeof(T).GetGenericArguments();

                        if (Map.TryGetValue(definition, out var @interface))
                        {
                            return (IValueInterface<T>)Activator.CreateInstance(@interface.MakeGenericType(arguments))!;
                        }
                    }
                    else if (Map.TryGetValue(typeof(T), out var @interface))
                    {
                        return (IValueInterface<T>)Activator.CreateInstance(@interface)!;
                    }
                }
            }

            return null;
        }
    }
}