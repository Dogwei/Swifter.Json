
#if NETCOREAPP && ! NETCOREAPP2_0

using System;
using System.Text;

namespace Swifter.Tools
{
    partial class HGlobalCacheExtensions
    {
        /// <summary>
        /// 将 source 的内容缓存到 HGlobalCache 中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="source">source</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回缓冲的长度</returns>
        public static void ReadFrom(this HGlobalCache<char> hGCache, ReadOnlySpan<byte> source, Encoding encoding)
        {
            var maxCharsCount = encoding.GetMaxCharCount(source.Length);

            if (maxCharsCount >= hGCache.Capacity)
            {
                hGCache.Expand(maxCharsCount);
            }

            hGCache.Count = encoding.GetChars(source, hGCache);
        }
    }
}

#endif