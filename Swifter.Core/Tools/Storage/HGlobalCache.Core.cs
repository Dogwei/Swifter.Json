#if Span
using System;

namespace Swifter.Tools
{
    unsafe partial class HGlobalCache<T>
    {
        /// <summary>
        /// 返回全局缓存中的空间 Span。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        public static implicit operator Span<T>(HGlobalCache<T> hGCache) => new Span<T>(hGCache.First, hGCache.Available);

        /// <summary>
        /// 返回全局缓存中的内容 Span。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        public static implicit operator ReadOnlySpan<T>(HGlobalCache<T> hGCache) => new ReadOnlySpan<T>(hGCache.First, hGCache.Count);

        /// <summary>
        /// 返回全局缓存中的空间 Memory。
        /// </summary>
        /// <param name="hGCache"></param>
        public static implicit operator Memory<T>(HGlobalCache<T> hGCache) => new Memory<T>(hGCache.Context, hGCache.Offset, hGCache.Available);

        /// <summary>
        /// 返回全局缓存中的内容 Memory。
        /// </summary>
        /// <param name="hGCache"></param>
        public static implicit operator ReadOnlyMemory<T>(HGlobalCache<T> hGCache) => new ReadOnlyMemory<T>(hGCache.Context, hGCache.Offset, hGCache.Count);
    }
}
#endif