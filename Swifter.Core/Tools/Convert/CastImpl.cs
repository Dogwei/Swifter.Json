using System;

namespace Swifter.Tools
{
    public static partial class XConvert
    {
        private abstract class CastImpl
        {
            private static readonly MyCache Impls = new MyCache();

            public static object Cast(object value, Type outType)
            {
                if (value == null)
                {
                    return ToObject<object>(null, outType);
                }

                return Impls.GetOrCreate(new KeyInfo() { tSource = value.GetType(), tDestination = outType }).Convert(value);
            }

            public abstract object Convert(object value);

            private sealed class MyCache : BaseCache<KeyInfo, CastImpl>, BaseCache<KeyInfo, CastImpl>.IGetOrCreate<KeyInfo>
            {
                public MyCache() : base(0)
                {
                }

                public KeyInfo AsKey(KeyInfo token) => token;

                public CastImpl AsValue(KeyInfo token) => (CastImpl)Activator.CreateInstance(typeof(Internal<,>).MakeGenericType(token.tSource, token.tDestination));

                public CastImpl GetOrCreate(KeyInfo token) => GetOrCreate<MyCache>(this, token);

                protected override int ComputeHashCode(KeyInfo key) => key.tSource.GetHashCode() ^ key.tDestination.GetHashCode();

                protected override bool Equals(KeyInfo key1, KeyInfo key2) => key1.tSource == key2.tSource && key1.tDestination == key2.tDestination;
            }

            private sealed class Internal<TSource, TDestination> : CastImpl
            {
                public override object Convert(object value) => XConvert<TDestination>.Convert((TSource)value);
            }

            private struct KeyInfo
            {
                public Type tSource;

                public Type tDestination;
            }
        }
    }
}