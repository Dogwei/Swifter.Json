using System;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 
    /// </summary>
    public static partial class XConvert
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TDestination Convert<TSource, TDestination>(TSource value)
        {
            if (typeof(TSource) == typeof(TDestination))
            {
                return Unsafe.As<TSource, TDestination>(ref value);
            }

            return InternalConvert<TSource, TDestination>.Instance.Convert(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="value"></param>
        /// <param name="outType"></param>
        /// <returns></returns>
        public static object ToObject<TSource>(TSource value, Type outType) => ToObjectImpl.ToObject(value, outType);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static TDestination FromObject<TDestination>(object value) => FromObjectImpl.FromObject<TDestination>(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="outType"></param>
        /// <returns></returns>
        public static object Cast(object value, Type outType) => CastImpl.Cast(value, outType);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TDestination"></typeparam>
    public static class XConvert<TDestination>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TDestination Convert<TSource>(TSource value) => XConvert.Convert<TSource, TDestination>(value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TDestination FromObject(object obj) => XConvert.FromObject<TDestination>(obj);
    }
}