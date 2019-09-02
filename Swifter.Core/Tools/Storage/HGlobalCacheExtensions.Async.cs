
#if NET45 || NET451 || NET47 || NET471 || NETSTANDARD || NETCOREAPP


using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Swifter.Tools
{
    partial class HGlobalCacheExtensions
    {
        /// <summary>
        /// 异步将缓存中内容写入到指定的文本写入器中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="textWriter">文本写入器</param>
        public static async Task WriteToAsync(this HGlobalCache<char> hGCache, TextWriter textWriter)
        {
            await textWriter.WriteAsync(hGCache.Context, 0, hGCache.Count);
        }

        /// <summary>
        /// 异步将缓存中内容写入到指定的流中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="stream">流</param>
        public static async Task WriteToAsync(this HGlobalCache<byte> hGCache, Stream stream)
        {
            await stream.WriteAsync(hGCache.Context, 0, hGCache.Count);
        }

        /// <summary>
        /// 异步将 TextReader 的内容缓冲到 HGlobalCache 中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="textReader">TextReader</param>
        /// <returns>返回缓冲的长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async Task ReadFromAsync(this HGlobalCache<char> hGCache, TextReader textReader)
        {
            int offset = 0;

        Loop:

            if (offset >= hGCache.Capacity)
            {
                hGCache.Expand(1218);
            }

            int readCount = await textReader.ReadAsync(
                hGCache.Context,
                offset,
                hGCache.Capacity - offset);

            offset += readCount;

            if (offset == hGCache.Capacity)
            {
                goto Loop;
            }

            hGCache.Count = offset;
        }

        /// <summary>
        /// 异步将 Stream 的内容缓冲到 HGlobalCache 中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="stream">Stream</param>
        /// <returns>返回缓冲的长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async Task ReadFromAsync(this HGlobalCache<byte> hGCache, Stream stream)
        {
            int offset = 0;

        Loop:

            if (offset >= hGCache.Capacity)
            {
                hGCache.Expand(1218);
            }

            int readCount = await stream.ReadAsync(
                hGCache.Context,
                offset,
                hGCache.Capacity - offset);

            offset += readCount;

            if (offset == hGCache.Capacity)
            {
                goto Loop;
            }

            hGCache.Count = offset;
        }


        /// <summary>
        /// 异步将 HGlobalCache 中的内容写入到流中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码</param>
        public static async Task WriteToAsync(this HGlobalCache<char> hGCache, Stream stream, Encoding encoding)
        {
            var hGBytes = BytesPool.Rent();

            var maxBytesCount = encoding.GetMaxByteCount(hGCache.Count);

            if (maxBytesCount > hGBytes.Capacity)
            {
                hGBytes.Expand(maxBytesCount - hGBytes.Capacity);
            }

            unsafe
            {
                hGBytes.Count = encoding.GetBytes(
                    hGCache.GetPointer(),
                    hGCache.Count,
                    hGBytes.GetPointer(),
                    hGBytes.Capacity);
            }

            await hGBytes.WriteToAsync(stream);

            BytesPool.Return(hGBytes);
        }

        /// <summary>
        /// 异步将 Stream 的内容缓存到 HGlobalCache 中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="stream">Stream</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回缓冲的长度</returns>
        public static async Task ReadFromAsync(this HGlobalCache<char> hGCache, Stream stream, Encoding encoding)
        {
            var hGBytes = BytesPool.Rent();

            await hGBytes.ReadFromAsync(stream);

            var maxCharsCount = encoding.GetMaxCharCount(hGBytes.Count);

            if (maxCharsCount > hGCache.Capacity)
            {
                hGCache.Expand(maxCharsCount - hGCache.Capacity);
            }

            unsafe
            {
                hGCache.Count = encoding.GetChars(
                    hGBytes.GetPointer(),
                    hGBytes.Count,
                    hGCache.GetPointer(),
                    hGCache.Capacity);
            }

            BytesPool.Return(hGBytes);
        }
    }
}

#endif