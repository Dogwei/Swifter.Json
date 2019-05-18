using System;

namespace Swifter.Tools
{
    public static partial class XConvert
    {
        private abstract class ToObjectImpl
        {
            private static readonly MyCache Impls = new MyCache();

            public static object ToObject<TSource>(TSource value, Type outType) => Impls.GetOrCreate(outType).Convert(value);
            
            public abstract object Convert<TSource>(TSource value);

            private sealed class MyCache : BaseCache<Type, ToObjectImpl>, BaseCache<Type, ToObjectImpl>.IGetOrCreate<Type>
            {
                public MyCache() : base(0)
                {
                }

                public Type AsKey(Type token) => token;

                public ToObjectImpl AsValue(Type token) => (ToObjectImpl)Activator.CreateInstance(typeof(Internal<>).MakeGenericType(token));

                public ToObjectImpl GetOrCreate(Type token) => GetOrCreate<MyCache>(this, token);

                protected override int ComputeHashCode(Type key) => key.GetHashCode();

                protected override bool Equals(Type key1, Type key2) => key1 == key2;
            }

            private sealed class Internal<TDestination> : ToObjectImpl
            {
                public override object Convert<TSource>(TSource value) => XConvert<TDestination>.Convert(value);
            }
        }
    }
}