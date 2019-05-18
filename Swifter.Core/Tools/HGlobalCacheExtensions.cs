using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// HGlobalCache 扩展方法。
    /// </summary>
    public static class HGlobalCacheExtensions
    {
        /// <summary>
        /// 获取 HGlobalCache&lt;byte&gt; 的 Byte 指针。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <returns>返回 Byte 指针</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe byte* GetPointer(this HGlobalCache<byte> hGCache) => (byte*)hGCache.Pointer;

        /// <summary>
        /// 获取 HGlobalCache&lt;char&gt; 的 Char 指针。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <returns>返回 Char 指针</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe char* GetPointer(this HGlobalCache<char> hGCache) => (char*)hGCache.Pointer;
    }
}