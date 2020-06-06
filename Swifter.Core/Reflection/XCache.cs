using Swifter.Tools;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    sealed class XCache
    {
        static readonly XTypeInfo[] EmptyXTypeInfos = new XTypeInfo[0];
        static readonly Cache<Type, XCache> TypeCache = new Cache<Type, XCache>();

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static XCache Get<T>() => Generic<T>.Instance;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static XCache Get(Type type)
        {
            if (TypeCache.TryGetValue(type, out var xCache))
            {
                return xCache;
            }

            return TypeHelper.SlowGetValue<XCache>(typeof(Generic<>).MakeGenericType(type), nameof(Generic<object>.Instance));
        }

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
            public static readonly XCache Instance;

            static Generic()
            {
                lock (typeof(Generic<T>))
                {
                    if (Instance == null)
                    {
                        Instance = new XCache() { type = typeof(T), xTypeInfos = EmptyXTypeInfos };

                        TypeCache.Add(typeof(T), Instance);
                    }
                }
            }
        }
    }
}