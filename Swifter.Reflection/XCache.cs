using Swifter.Tools;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    internal sealed class XCache
    {
        static readonly XTypeInfo[] empty = new XTypeInfo[0];
        static readonly TypeCache<XCache> cache = new TypeCache<XCache>();
        
        public static XCache Get<T>() => Generic<T>.instance;
        
        public static XCache Get(Type type) => cache.GetValue(type) ?? TypeHelper.SlowGetValue<XCache>(typeof(Generic<>).MakeGenericType(type), nameof(Generic<object>.instance));

        Type type;
        XTypeInfo[] xTypeInfos;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        XTypeInfo Get(XBindingFlags flags)
        {
            foreach (var item in xTypeInfos)
            {
                if (item.flags == flags)
                {
                    return item;
                }
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        XTypeInfo LockGetOrCreate(XBindingFlags flags)
        {
            lock (this)
            {
                return Get(flags) ?? Create();
            }

            XTypeInfo Create()
            {
                var length = xTypeInfos.Length;

                Array.Resize(ref xTypeInfos, length + 1);

                return xTypeInfos[length] = new XTypeInfo(type, flags);
            }
        }

        public XTypeInfo this[XBindingFlags flags] => Get(flags) ?? LockGetOrCreate(flags);

        sealed class Generic<T>
        {
            public static readonly XCache instance;

            static Generic()
            {
                instance = new XCache() { type = typeof(T), xTypeInfos = empty };

                cache.DirectAdd(typeof(T), instance);
            }
        }
    }
}