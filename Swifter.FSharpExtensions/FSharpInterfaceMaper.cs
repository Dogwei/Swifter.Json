using Microsoft.FSharp.Collections;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Reflection;
using Swifter.Reflection;
using Swifter.RW;
using System;
using System.Reflection;

namespace Swifter.FSharpExtensions
{
    sealed class FSharpInterfaceMaper : IValueInterfaceMaper
    {
        public const BindingFlags AllBindingFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;

        public IValueInterface<T> TryMap<T>()
        {
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(FSharpList<>))
            {
                var elementType = typeof(T).GetGenericArguments()[0];

                return (IValueInterface<T>)Activator.CreateInstance(typeof(FSharpListInterface<>).MakeGenericType(elementType));
            }

            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(FSharpOption<>))
            {
                var elementType = typeof(T).GetGenericArguments()[0];

                return (IValueInterface<T>)Activator.CreateInstance(typeof(FSharpOptionInterface<>).MakeGenericType(elementType));
            }

            if (FSharpType.IsUnion(typeof(T), AllBindingFlags))
            {
                return new FSharpUnionInterface<T>();
            }

            if (FSharpType.IsRecord(typeof(T), AllBindingFlags))
            {
                FastObjectRW<T>.CurrentOptions |= FastObjectRWOptions.Allocate | FastObjectRWOptions.AutoPropertyDirectRW;

                XObjectInterface<T>.DefaultBindingFlags |= XBindingFlags.RWAllocate | XBindingFlags.RWAutoPropertyDirectRW;
            }

            return null;
        }
    }
}