using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace Swifter.Tools
{
    /// <summary>
    /// HGlobalCache 扩展方法。
    /// </summary>
    public static unsafe partial class HGlobalCacheExtensions
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

            hGCache.WriteTo(hGBytes, encoding);

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

            hGCache.ReadFrom(hGBytes, encoding);

            BytesPool.Return(hGBytes);
        }

        /// <summary>
        /// 将 HGlobalCache 中的内容写入到 destination 中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="destination">destination</param>
        /// <param name="encoding">编码</param>
        public static void WriteTo(this HGlobalCache<char> hGCache, HGlobalCache<byte> destination, Encoding encoding)
        {
            if (hGCache.Count <= 0)
            {
                return;
            }

            var maxBytesCount = encoding.GetMaxByteCount(hGCache.Count);

            if (maxBytesCount > destination.Capacity)
            {
                destination.Expand(maxBytesCount - destination.Capacity);
            }

            destination.Count = encoding.GetBytes(
                hGCache.GetPointer(),
                hGCache.Count,
                destination.GetPointer(),
                destination.Capacity);
        }

        /// <summary>
        /// 将 source 的内容缓存到 HGlobalCache 中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="source">source</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回缓冲的长度</returns>
        public static void ReadFrom(this HGlobalCache<char> hGCache, HGlobalCache<byte> source, Encoding encoding)
        {
            if (source.Count <= 0)
            {
                return;
            }

            var maxCharsCount = encoding.GetMaxCharCount(source.Count);

            if (maxCharsCount >= hGCache.Capacity)
            {
                hGCache.Expand(maxCharsCount);
            }

            hGCache.Count = encoding.GetChars(
                source.GetPointer(),
                source.Count,
                hGCache.GetPointer(),
                hGCache.Capacity);
        }

        /// <summary>
        /// 将 source 的内容缓存到 HGlobalCache 中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="source">source</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回缓冲的长度</returns>
        public static void ReadFrom(this HGlobalCache<char> hGCache, ArraySegment<byte> source, Encoding encoding)
        {
            if (source.Count <= 0)
            {
                return;
            }

            var maxCharsCount = encoding.GetMaxCharCount(source.Count);

            if (maxCharsCount >= hGCache.Capacity)
            {
                hGCache.Expand(maxCharsCount);
            }

            fixed (byte* pSource = &source.Array[source.Offset])
            {
                hGCache.Count = encoding.GetChars(
                    pSource,
                    source.Count,
                    hGCache.GetPointer(),
                    hGCache.Capacity);
            }
        }

        /// <summary>
        /// 获取 HGlobalCache&lt;byte&gt; 的 Byte 指针。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <returns>返回 Byte 指针</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static byte* GetPointer(this HGlobalCache<byte> hGCache) => (byte*)hGCache.Pointer;

        /// <summary>
        /// 获取 HGlobalCache&lt;char&gt; 的 Char 指针。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <returns>返回 Char 指针</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static char* GetPointer(this HGlobalCache<char> hGCache) => (char*)hGCache.Pointer;

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
        /// 将缓存中的内容复制到新的数组中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回一个新的数组</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static byte[] ToBytes(this HGlobalCache<char> hGCache, Encoding encoding)
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

            var bytes = hGBytes.ToBytes();

            BytesPool.Return(hGBytes);

            return bytes;
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

        /// <summary>
        /// 使用指定哈希算法类型计算字节缓存的哈希值。以十六进制字符串返回。
        /// </summary>
        /// <typeparam name="THashAlgorithm">哈希算法类型</typeparam>
        /// <param name="hGCache">字节缓存</param>
        /// <returns>返回 Hash 值的十六进制字符串</returns>
        public static string ComputeHash<THashAlgorithm>(this HGlobalCache<byte> hGCache) where THashAlgorithm : HashAlgorithm
        {
            const int byte_len = 2;

#if NETCOREAPP && !NETCOREAPP2_0

            var bytes = ((Span<byte>)hGCache).Slice(hGCache.Count);

            var instance = THashAlgorithmInstances<THashAlgorithm>.Instance;

            if (hGCache.Capacity - hGCache.Count < instance.HashSize)
            {
                hGCache.Expand(instance.HashSize);
            }

            if (instance.TryComputeHash(hGCache, bytes, out var written))
            {
                bytes = bytes.Slice(0, written);
            }
            else
            {
                bytes = instance.ComputeHash(hGCache.Context, 0, hGCache.Count);
            }
#else

            var bytes = THashAlgorithmInstances<THashAlgorithm>.Instance.ComputeHash(hGCache.Context, 0, hGCache.Count);
#endif

            var str = StringHelper.MakeString(bytes.Length * byte_len);

            fixed (char* pStr = str)
            {
                var pStr2 = pStr;

                foreach (var item in bytes)
                {
                    NumberHelper.Hex.ToString(item, byte_len, pStr2);

                    pStr2 += byte_len;
                }
            }

            return str;
        }

        /// <summary>
        /// 使用指定哈希算法类型和编码类型计算字符缓存的哈希值。以十六进制字符串返回。
        /// </summary>
        /// <typeparam name="THashAlgorithm">哈希算法类型</typeparam>
        /// <param name="hGCache">字符缓存</param>
        /// <param name="encoding">指定编码</param>
        /// <returns>返回 Hash 值的十六进制字符串</returns>
        public static string ComputeHash<THashAlgorithm>(this HGlobalCache<char> hGCache, Encoding encoding) where THashAlgorithm : HashAlgorithm
        {
            var hGBytes = BytesPool.Rent();

            hGCache.WriteTo(hGBytes, encoding);

            var result = ComputeHash<THashAlgorithm>(hGBytes);

            BytesPool.Return(hGBytes);

            return result;
        }

        /// <summary>
        /// 使用指定哈希算法类型并使用 UTF-8 编码计算字符缓存的哈希值。以十六进制字符串返回。
        /// </summary>
        /// <typeparam name="THashAlgorithm">哈希算法类型</typeparam>
        /// <param name="hGCache">字符缓存</param>
        /// <returns>返回 Hash 值的十六进制字符串</returns>
        public static string ComputeHash<THashAlgorithm>(this HGlobalCache<char> hGCache) where THashAlgorithm : HashAlgorithm
        {
            return ComputeHash<THashAlgorithm>(hGCache, Encoding.UTF8);
        }

        /// <summary>
        /// 在字符缓存的后面拼接一个字符串。
        /// </summary>
        /// <param name="hGCache">字符缓存</param>
        /// <param name="value">字符串</param>
        /// <returns>返回当前字符缓存</returns>
        public static HGlobalCache<char> Append(this HGlobalCache<char> hGCache, string value)
        {
            var length = value.Length;

            if (hGCache.Capacity - hGCache.Count <= length)
            {
                hGCache.Expand(length + 2);
            }

            var pointer = hGCache.GetPointer() + hGCache.Count;

            for (int i = 0; i < length; ++i)
            {
                pointer[i] = value[i];
            }

            hGCache.Count += length;

            return hGCache;
        }

        /// <summary>
        /// 清楚缓存中的内容
        /// </summary>
        /// <param name="hGCache">缓存</param>
        public static void Clear<T>(this HGlobalCache<T> hGCache) where T : struct
        {
            hGCache.Count = 0;
        }
    }
}