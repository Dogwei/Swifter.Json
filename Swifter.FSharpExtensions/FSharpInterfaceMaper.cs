using Microsoft.FSharp.Collections;
using Swifter.RW;
using System;

namespace Swifter.FSharpExtensions
{
    sealed class FSharpInterfaceMaper : IValueInterfaceMaper
    {
        public IValueInterface<T> TryMap<T>()
        {
            if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(FSharpList<>))
            {
                var elementType = typeof(T).GetGenericArguments()[0];

                return (IValueInterface<T>)Activator.CreateInstance(typeof(FSharpListInterface<>).MakeGenericType(elementType));
            }

            return null;
        }
    }
}