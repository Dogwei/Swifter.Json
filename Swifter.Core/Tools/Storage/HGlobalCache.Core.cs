#if NETCOREAPP && !NETCOREAPP2_0
using System;

namespace Swifter.Tools
{
    unsafe partial class HGlobalCache<T>
    {
        /// <summary>
        /// 返回全局缓存中的内容 Span。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        public static implicit operator Span<T>(HGlobalCache<T> hGCache)=> new Span<T>(hGCache.Pointer, hGCache.Count);

        /// <summary>
        /// 返回全局缓存中的内容 Span。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        public static implicit operator ReadOnlySpan<T>(HGlobalCache<T> hGCache) => new ReadOnlySpan<T>(hGCache.Pointer, hGCache.Count);
    }
}
#endif