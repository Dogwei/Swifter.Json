#if NETCOREAPP && !NETCOREAPP2_0
using System;

namespace Swifter.Tools
{
    unsafe partial class HGlobalCache<T>
    {
        /// <summary>
        /// 返回全局缓存中的空间 Span。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        public static implicit operator Span<T>(HGlobalCache<T> hGCache) => new Span<T>(hGCache.Pointer, hGCache.Capacity);

        /// <summary>
        /// 返回全局缓存中的内容 Span。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        public static implicit operator ReadOnlySpan<T>(HGlobalCache<T> hGCache) => new ReadOnlySpan<T>(hGCache.Pointer, hGCache.Count);

        /// <summary>
        /// 回全局缓存中的空间 Memory。
        /// </summary>
        /// <param name="hGCache"></param>
        public static implicit operator Memory<T>(HGlobalCache<T> hGCache) => new Memory<T>(hGCache.Context, 0, hGCache.Capacity);

        /// <summary>
        /// 回全局缓存中的内容 Memory。
        /// </summary>
        /// <param name="hGCache"></param>
        public static implicit operator ReadOnlyMemory<T>(HGlobalCache<T> hGCache) => new ReadOnlyMemory<T>(hGCache.Context, 0, hGCache.Count);
    }
}
#endif