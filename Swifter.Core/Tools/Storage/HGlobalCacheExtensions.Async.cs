
#if NET45 || NET451 || NET47 || NET471 || NETSTANDARD || NETCOREAPP


using System;
using System.IO;
using System.Runtime.CompilerServices;
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
            // await VersionDifferences.WriteCharsAsync(textWriter, hGCache.Address, hGCache.Count);
        }

        /// <summary>
        /// 异步将缓存中内容写入到指定的流中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="stream">流</param>
        public static async Task WriteToAsync(this HGlobalCache<byte> hGCache, Stream stream)
        {
            await stream.WriteAsync(hGCache.Context, 0, hGCache.Count);
            // await VersionDifferences.WriteBytesAsync(stream, hGCache.Address, hGCache.Count);
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

            //IntPtr address;

            //unsafe
            //{
            //    address = (IntPtr)(hGCache.GetPointer() + offset);
            //}

            //int readCount = await VersionDifferences.ReadCharsAsync(
            //    textReader,
            //    address,
            //    hGCache.Capacity - offset);

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

            //IntPtr address;

            //unsafe
            //{
            //    address = (IntPtr)(hGCache.GetPointer() + offset);
            //}

            //int readCount = await VersionDifferences.ReadBytesAsync(
            //    stream,
            //    address,
            //    hGCache.Capacity - offset);

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
    }
}

#endif