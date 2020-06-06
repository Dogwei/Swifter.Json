using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    public static partial class XConvert
    {
        internal abstract class CastImpl
        {
            private static readonly Dictionary<KeyInfo, CastImpl> Impls = new Dictionary<KeyInfo, CastImpl>();

            public readonly bool IsImplicitConvert;
            public readonly bool IsExplicitConvert;
            public readonly bool IsCustomConvert;
            public readonly bool IsBasicConvert;

            public CastImpl(Type sourceType, Type destinationType)
            {
                IsImplicitConvert = InternalConvert.IsImplicitConvert(sourceType, destinationType);
                IsExplicitConvert = InternalConvert.IsExplicitConvert(sourceType, destinationType);
                IsCustomConvert = InternalConvert.IsCustomConvert(sourceType, destinationType);
                IsBasicConvert = InternalConvert.IsBasicConvert(sourceType, destinationType);
            }

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public static object Cast(object value, Type outType)
            {
                if (outType is null) throw new ArgumentNullException(nameof(outType));
                if (value is null) return ToObject<object>(null, outType);
                if (Impls.TryGetValue(new KeyInfo(value, outType), out var impl)) return impl.Convert(value);

                return InternalCast(value, outType);
            }

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public static CastImpl GetImpl(Type sourceType, Type destinationType)
            {
                if (Impls.TryGetValue(new KeyInfo(sourceType, destinationType), out var impl)) return impl;

                return InternalGetImpl(sourceType, destinationType);
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static CastImpl InternalGetImpl(Type sourceType, Type destinationType)
            {
                lock (Impls)
                {
                    var key = new KeyInfo(sourceType, destinationType);

                    if (!Impls.TryGetValue(key, out var impl))
                    {
                        var implType = typeof(Impl<,>).MakeGenericType(sourceType, destinationType);

                        impl = (CastImpl)Activator.CreateInstance(implType);

                        Impls.Add(key, impl);
                    }

                    return impl;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public static object InternalCast(object value, Type outType)
            {
                lock (Impls)
                {
                    var key = new KeyInfo(value, outType);

                    if (!Impls.TryGetValue(key, out var impl))
                    {
                        var implType = typeof(Impl<,>).MakeGenericType(value.GetType(), outType);

                        impl = (CastImpl)Activator.CreateInstance(implType);

                        Impls.Add(key, impl);
                    }

                    return impl.Convert(value);
                }
            }

            public abstract object Convert(object value);

            private readonly struct KeyInfo : IEquatable<KeyInfo>
            {
                public readonly IntPtr tSource;

                public readonly IntPtr tDestination;

                public KeyInfo(object source, Type destination)
                {
                    tSource = TypeHelper.GetTypeHandle(source);
                    tDestination = TypeHelper.GetTypeHandle(destination);
                }

                public KeyInfo(Type source, Type destination)
                {
                    tSource = TypeHelper.GetTypeHandle(source);
                    tDestination = TypeHelper.GetTypeHandle(destination);
                }

                public bool Equals(KeyInfo other)
                {
                    return tSource == other.tSource && tDestination == other.tDestination;
                }

                public override int GetHashCode()
                {
                    return tSource.GetHashCode() ^ tDestination.GetHashCode();
                }
            }

            private sealed class Impl<TSource, TDestination> : CastImpl
            {
                public Impl() : base(typeof(TSource), typeof(TDestination))
                {
                }

                public override object Convert(object value) => Convert<TSource, TDestination>((TSource)value);
            }
        }
    }
}