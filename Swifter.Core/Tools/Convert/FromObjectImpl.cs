using System;
using System.Collections.Generic;

namespace Swifter.Tools
{
    public static partial class XConvert
    {
        private abstract class FromObjectImpl
        {
            private static readonly Dictionary<IntPtr, FromObjectImpl> Impls = new Dictionary<IntPtr, FromObjectImpl>();

            public static TDestination FromObject<TDestination>(object value)
            {
                if (value is null) return Convert<object, TDestination>(null);
                if (Impls.TryGetValue(TypeHelper.GetMethodTablePointer(value), out var impl)) return impl.Convert<TDestination>(value);

                return InternalFromObject<TDestination>(value);
            }

            public static TDestination InternalFromObject<TDestination>(object value)
            {
                lock (Impls)
                {
                    var key = TypeHelper.GetMethodTablePointer(value);

                    if (!Impls.TryGetValue(key, out var impl))
                    {
                        var implType = typeof(Impl<>).MakeGenericType(value.GetType());

                        impl = (FromObjectImpl)Activator.CreateInstance(implType);

                        Impls.Add(key, impl);
                    }

                    return impl.Convert<TDestination>(value);
                }
            }

            public abstract TDestination Convert<TDestination>(object obj);

            private sealed class Impl<TSource> : FromObjectImpl
            {
                public override TDestination Convert<TDestination>(object value) => Convert<TSource, TDestination>((TSource)value);
            }
        }
    }
}