
#if Span

using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Swifter.Tools
{
    unsafe partial class HGlobalCacheExtensions
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
            Grow(hGCache, encoding.GetMaxCharCount(source.Length));

#if NativeSpan

            hGCache.Count += encoding.GetChars(source, new Span<char>(hGCache.Current, hGCache.Rest));

#else
            fixed(byte* bytes = source)
            {
                hGCache.Count += encoding.GetChars(bytes, source.Length, hGCache.Current, hGCache.Rest);
            }
#endif
        }

        /// <summary>
        /// 将字节码转换为 16 进制字符串。
        /// </summary>
        /// <param name="bytes">字节码</param>
        /// <returns>返回一个字符串</returns>
        public static string ToHexString(this ReadOnlySpan<byte> bytes)
        {
            return ToHexString(ref Unsafe.AsRef(bytes[0]), bytes.Length);
        }
    }
}

#endif