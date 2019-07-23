using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.IO;
using System.Text;

namespace Swifter.Json
{
    /// <summary>
    /// 提供 Json 格式化工具的扩展方法。
    /// </summary>
    public static unsafe partial class JsonExtensions
    {
        static readonly HGlobalCachePool<byte> BytesPool = new HGlobalCachePool<byte>();

        /// <summary>
        /// 将 HGlobalCache 中的内容写入到流中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码</param>
        public static void WriteTo(this HGlobalCache<char> hGCache, Stream stream, Encoding encoding)
        {
            var hGBytes = BytesPool.Rent();

            var maxBytesCount = encoding.GetMaxByteCount(hGCache.Count);

            if (maxBytesCount > hGBytes.Capacity)
            {
                hGBytes.Expand(maxBytesCount - hGBytes.Capacity);
            }

            hGBytes.Count = encoding.GetBytes(
                hGCache.GetPointer(),
                hGCache.Count,
                hGBytes.GetPointer(),
                hGBytes.Capacity);

            hGBytes.WriteTo(stream);

            BytesPool.Return(hGBytes);
        }

        /// <summary>
        /// 将 Stream 的内容缓存到 HGlobalCache 中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="stream">Stream</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回缓冲的长度</returns>
        public static void ReadFrom(this HGlobalCache<char> hGCache, Stream stream, Encoding encoding)
        {
            var hGBytes = BytesPool.Rent();

            hGBytes.ReadFrom(stream);

            var maxCharsCount = encoding.GetMaxCharCount(hGBytes.Count);

            if (maxCharsCount >= hGCache.Capacity)
            {
                hGCache.Expand(maxCharsCount);
            }

            hGCache.Count = encoding.GetChars(
                hGBytes.GetPointer(),
                hGBytes.Count,
                hGCache.GetPointer(),
                hGCache.Capacity);

            BytesPool.Return(hGBytes);
        }
    }
}