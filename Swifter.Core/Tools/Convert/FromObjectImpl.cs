using System;

namespace Swifter.Tools
{
    public static partial class XConvert
    {
        private abstract class FromObjectImpl
        {
            private static readonly MyCache Impls = new MyCache();

            public static TDestination FromObject<TDestination>(object obj)
            {
                if (obj == null)
                {
                    return Convert<object, TDestination>(null);
                }

                return Impls.GetOrCreate(obj).Convert<TDestination>(obj);
            }

            public abstract TDestination Convert<TDestination>(object obj);

            private sealed class MyCache : BaseCache<IntPtr, FromObjectImpl>, BaseCache<IntPtr, FromObjectImpl>.IGetOrCreate<object>
            {
                public MyCache() : base(0)
                {
                }

                public IntPtr AsKey(object token) => Unsafe.GetObjectTypeHandle(token);

                public FromObjectImpl AsValue(object token)=> (FromObjectImpl)Activator.CreateInstance(typeof(Internal<>).MakeGenericType(token.GetType()));

                public FromObjectImpl GetOrCreate(object token) => GetOrCreate(this, token);

                protected override int ComputeHashCode(IntPtr key) => key.GetHashCode();

                protected override bool Equals(IntPtr key1, IntPtr key2) => key1 == key2;
            }

            private sealed class Internal<TSource> : FromObjectImpl
            {
                public override TDestination Convert<TDestination>(object value) => XConvert<TDestination>.Convert((TSource)value);
            }
        }
    }
}