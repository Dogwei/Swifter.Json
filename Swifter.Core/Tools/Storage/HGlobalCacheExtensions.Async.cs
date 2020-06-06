#if Async


using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

#if ValueTask

using Task = System.Threading.Tasks.ValueTask;
using ReadToEndTask = System.Threading.Tasks.ValueTask<System.ArraySegment<byte>>;
using ReadToEndCharsTask = System.Threading.Tasks.ValueTask<System.ArraySegment<char>>;

#else

using Task = System.Threading.Tasks.Task;
using ReadToEndTask = System.Threading.Tasks.Task<System.ArraySegment<byte>>;
using ReadToEndCharsTask = System.Threading.Tasks.Task<System.ArraySegment<char>>;

#endif

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
            await textWriter.WriteAsync(hGCache.Context, hGCache.Offset, hGCache.Count);
        }

        /// <summary>
        /// 异步将缓存中内容写入到指定的流中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="stream">流</param>
        public static async Task WriteToAsync(this HGlobalCache<byte> hGCache, Stream stream)
        {
            await stream.WriteAsync(hGCache.Context, hGCache.Offset, hGCache.Count);
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
        Loop:

            hGCache.Grow(1218);

            var readCount = await textReader.ReadAsync(
                hGCache.Context,
                hGCache.Offset + hGCache.Count,
                hGCache.Rest);

            hGCache.Count += readCount;

            if (readCount != 0)
            {
                goto Loop;
            }
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
        Loop:

            hGCache.Grow(1218);

            var readCount = await stream.ReadAsync(
                hGCache.Context,
                hGCache.Offset + hGCache.Count,
                hGCache.Rest);

            hGCache.Count += readCount;

            if (readCount != 0)
            {
                goto Loop;
            }
        }

        /// <summary>
        /// 异步将 HGlobalCache 中的内容写入到流中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码</param>
        public static async Task WriteToAsync(this HGlobalCache<char> hGCache, Stream stream, Encoding encoding)
        {
            var writer = new StreamWriter(stream, encoding);

            await hGCache.WriteToAsync(writer);

            writer.Flush();
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
            await hGCache.ReadFromAsync(new StreamReader(stream, encoding));
        }


        static ArrayAppendingInfo BytesAppendingInfo = new ArrayAppendingInfo { MostClosestMeanCommonlyUsedLength = 4096 };

        /// <summary>
        /// 异步读取流到结尾。
        /// </summary>
        /// <param name="stream">流</param>
        /// <returns>返回一个数组段</returns>
        public static async ReadToEndTask ReadToEndAsync(this Stream stream)
        {
            if (stream is MemoryStream ms)
            {
                return InternalReadToEnd(ms);
            }

            if (stream.CanSeek)
            {
                return await InternalReadToEndAsync(stream);
            }

            try
            {
                return await InternalReadToEndAsync(stream);
            }
            catch
            {
            }

            return await SteadyReadToEndAsync(stream);
        }

        private static async ReadToEndTask InternalReadToEndAsync(Stream stream)
        {
            var length = stream.GetCheckedLength();

            var bytes = new byte[length];

            for (int i = 0; i < length;)
            {
                i += await stream.ReadAsync(bytes, i, length - i);
            }

            return new ArraySegment<byte>(bytes);
        }

        private static async ReadToEndTask SteadyReadToEndAsync(Stream stream)
        {
            const int extra = 512;
            const int expand = 4096;

            var bytes = new byte[BytesAppendingInfo.MostClosestMeanCommonlyUsedLength + extra];

            var offset = 0;
            var count = bytes.Length;

            Loop:

            var readCount = await stream.ReadAsync(bytes, offset, count);

            offset += readCount;
            count -= readCount;

            if (readCount <= 0)
            {
                BytesAppendingInfo.AddUsedLength(offset);

                if (bytes.Length > offset * 2)
                {
                    Array.Resize(ref bytes, offset);
                }

                return new ArraySegment<byte>(bytes, 0, offset);
            }

            if (count == 0)
            {
                var newSize = checked((int)(unchecked((bytes.Length + expand) * 9L) / 5));

                if (BytesAppendingInfo.FirstCommonlyUsedLength < newSize && BytesAppendingInfo.FirstCommonlyUsedLength > bytes.Length)
                {
                    newSize = BytesAppendingInfo.FirstCommonlyUsedLength + extra;
                }

                if (BytesAppendingInfo.SecondCommonlyUsedLength < newSize && BytesAppendingInfo.SecondCommonlyUsedLength > bytes.Length)
                {
                    newSize = BytesAppendingInfo.SecondCommonlyUsedLength + extra;
                }

                if (BytesAppendingInfo.ThirdCommonlyUsedLength < newSize && BytesAppendingInfo.ThirdCommonlyUsedLength > bytes.Length)
                {
                    newSize = BytesAppendingInfo.ThirdCommonlyUsedLength + extra;
                }

                count += newSize - bytes.Length;

                Array.Resize(ref bytes, newSize);
            }

            goto Loop;
        }

    }
}

#endif