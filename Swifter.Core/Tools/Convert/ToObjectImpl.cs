using System;
using System.Collections.Generic;

namespace Swifter.Tools
{
    public static partial class XConvert
    {
        private abstract class ToObjectImpl
        {
            private static readonly Dictionary<Type, ToObjectImpl> Impls = new Dictionary<Type, ToObjectImpl>();

            public static object ToObject<TSource>(TSource value, Type outType)
            {
                if (outType is null) throw new ArgumentNullException(nameof(outType));
                if (Impls.TryGetValue(outType, out var impl)) return impl.Convert(value);

                return InternalToObject(value, outType);
            }

            public static object InternalToObject<TSource>(TSource value, Type outType)
            {
                lock (Impls)
                {
                    if (!Impls.TryGetValue(outType, out var impl))
                    {
                        var implType = typeof(Impl<>).MakeGenericType(outType);

                        impl = (ToObjectImpl)Activator.CreateInstance(implType);

                        Impls.Add(outType, impl);
                    }

                    return impl.Convert(value);
                }
            }

            public abstract object Convert<TSource>(TSource value);


            private sealed class Impl<TDestination> : ToObjectImpl
            {
                public override object Convert<TSource>(TSource value) => Convert<TSource, TDestination>(value);
            }
        }
    }
}