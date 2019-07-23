using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Swifter.Tools
{
    /// <summary>
    /// HGlobalCache 扩展方法。
    /// </summary>
    public static unsafe partial class HGlobalCacheExtensions
    {
        /// <summary>
        /// 获取 HGlobalCache&lt;byte&gt; 的 Byte 指针。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <returns>返回 Byte 指针</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static byte* GetPointer(this HGlobalCache<byte> hGCache) => (byte*)hGCache.Address;

        /// <summary>
        /// 获取 HGlobalCache&lt;char&gt; 的 Char 指针。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <returns>返回 Char 指针</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static char* GetPointer(this HGlobalCache<char> hGCache) => (char*)hGCache.Address;

        /// <summary>
        /// 将缓存中的内容复制到新的数组中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <returns>返回一个新的数组</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static byte[] ToBytes(this HGlobalCache<byte> hGCache)
        {
            var destinetion = new byte[hGCache.Count];

            Unsafe.CopyBlock(ref destinetion[0], ref hGCache.GetPointer()[0], (uint)hGCache.Count);

            return destinetion;
        }

        /// <summary>
        /// 将缓存中的内容复制到新的字符串中。(Ex: 避免冲突)
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <returns>返回一个新的字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string ToStringEx(this HGlobalCache<char> hGCache)
        {
            return new string(hGCache.GetPointer(), 0, hGCache.Count);
        }

        /// <summary>
        /// 将缓存中内容写入到指定的文本写入器中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="textWriter">文本写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void WriteTo(this HGlobalCache<char> hGCache, TextWriter textWriter)
        {
            textWriter.Write(hGCache.Context, 0, hGCache.Count);
            // VersionDifferences.WriteChars(textWriter, hGCache.GetPointer(), hGCache.Count);
        }

        /// <summary>
        /// 将缓存中内容写入到指定的流中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="stream">流</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void WriteTo(this HGlobalCache<byte> hGCache, Stream stream)
        {
            stream.Write(hGCache.Context, 0, hGCache.Count);
            // VersionDifferences.WriteBytes(stream, hGCache.GetPointer(), hGCache.Count);
        }

        /// <summary>
        /// 缓冲 TextReader 的内容到 HGlobalCache 中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="textReader">TextReader</param>
        /// <returns>返回缓冲的长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe void ReadFrom(this HGlobalCache<char> hGCache, TextReader textReader)
        {
            int offset = 0;

        Loop:

            if (offset >= hGCache.Capacity)
            {
                hGCache.Expand(1218);
            }

            int readCount = textReader.Read(hGCache.Context,
                offset,
                hGCache.Capacity - offset);

            //int readCount = VersionDifferences.ReadChars(
            //    textReader,
            //    hGCache.GetPointer() + offset,
            //    hGCache.Capacity - offset);

            offset += readCount;

            if (offset == hGCache.Capacity)
            {
                goto Loop;
            }

            hGCache.Count = offset;
        }

        /// <summary>
        /// 缓冲 Stream 的内容到 HGlobalCache 中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="stream">Stream</param>
        /// <returns>返回缓冲的长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe void ReadFrom(this HGlobalCache<byte> hGCache, Stream stream)
        {
            int offset = 0;

        Loop:

            if (offset >= hGCache.Capacity)
            {
                hGCache.Expand(1218);
            }

            int readCount = stream.Read(hGCache.Context,
                offset,
                hGCache.Capacity - offset);

            //int readCount = VersionDifferences.ReadBytes(
            //    stream,
            //    hGCache.GetPointer() + offset,
            //    hGCache.Capacity - offset);

            offset += readCount;

            if (offset == hGCache.Capacity)
            {
                goto Loop;
            }

            hGCache.Count = offset;
        }
    }
}