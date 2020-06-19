using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
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
        /// <summary>
        /// 字节缓存池。
        /// </summary>
        public static readonly HGlobalCachePool<byte> BytesPool = new HGlobalCachePool<byte>();

        /// <summary>
        /// 字符缓存池。
        /// </summary>
        public static readonly HGlobalCachePool<char> CharsPool = new HGlobalCachePool<char>();

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
            destination.Grow(encoding.GetMaxByteCount(hGCache.Count));

            destination.Count += encoding.GetBytes(
                hGCache.First,
                hGCache.Count,
                destination.Current,
                destination.Rest);
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
            hGCache.Grow(encoding.GetMaxCharCount(source.Count));

            hGCache.Count += encoding.GetChars(
                source.First,
                source.Count,
                hGCache.Current,
                hGCache.Rest);
        }

        /// <summary>
        /// 将 source 的内容缓存到 HGlobalCache 中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="source">source</param>
        /// <param name="encoding">编码</param>
        public static void ReadFrom(this HGlobalCache<char> hGCache, ArraySegment<byte> source, Encoding encoding)
        {
            hGCache.Grow(encoding.GetMaxCharCount(source.Count));

            fixed (byte* pSource = &source.Array[source.Offset])
            {
                hGCache.Count += encoding.GetChars(
                    pSource,
                    source.Count,
                    hGCache.Current,
                    hGCache.Rest);
            }
        }

        /// <summary>
        /// 将 source 的内容缓存到 HGlobalCache 中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="source">source</param>
        /// <param name="encoding">编码</param>
        public static void ReadFrom(this HGlobalCache<char> hGCache, byte[] source, Encoding encoding)
        {
            hGCache.Grow(encoding.GetMaxCharCount(source.Length));

            fixed (byte* pSource = source)
            {
                hGCache.Count += encoding.GetChars(
                    pSource,
                    source.Length,
                    hGCache.Current,
                    hGCache.Rest);
            }
        }

        /// <summary>
        /// 将缓存中的内容复制到新的数组中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <returns>返回一个新的数组</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T[] ToArray<T>(this HGlobalCache<T> hGCache) where T: unmanaged
        {
            var ret = new T[hGCache.Count];

            Array.Copy(hGCache.Context, hGCache.Offset, ret, 0, hGCache.Count);

            return ret;
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
            return encoding.GetBytes(hGCache.Context, hGCache.Offset, hGCache.Count);
        }

        /// <summary>
        /// 将缓存中的内容复制到新的数组中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回一个新的数组</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static char[] ToChars(this HGlobalCache<byte> hGCache, Encoding encoding)
        {
            return encoding.GetChars(hGCache.Context, hGCache.Offset, hGCache.Count);
        }

        /// <summary>
        /// 将缓存中的内容复制到新的字符串中。(Ex: 避免冲突)
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <returns>返回一个新的字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string ToStringEx(this HGlobalCache<char> hGCache)
        {
            return StringHelper.ToString(hGCache.First, hGCache.Count);
        }

        /// <summary>
        /// 将字节缓存中的内容转换为 16 进制字符串。
        /// </summary>
        /// <param name="hGCache">字节缓存</param>
        /// <returns>返回一个字符串</returns>
        public static string ToHexString(this HGlobalCache<byte> hGCache)
        {
            return ToHexString(ref *hGCache.First, hGCache.Count);
        }

        /// <summary>
        /// 将缓存中内容写入到指定的文本写入器中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="textWriter">文本写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void WriteTo(this HGlobalCache<char> hGCache, TextWriter textWriter)
        {
            textWriter.Write(hGCache.Context, hGCache.Offset, hGCache.Count);
        }

        /// <summary>
        /// 将缓存中内容写入到指定的流中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="stream">流</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void WriteTo(this HGlobalCache<byte> hGCache, Stream stream)
        {
            stream.Write(hGCache.Context, hGCache.Offset, hGCache.Count);
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
        Loop:

            hGCache.Grow(1218);

            int readCount = textReader.Read(
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
        /// 缓冲 Stream 的内容到 HGlobalCache 中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="stream">Stream</param>
        /// <returns>返回缓冲的长度</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe void ReadFrom(this HGlobalCache<byte> hGCache, Stream stream)
        {
        Loop:

            hGCache.Grow(1218);

            int readCount = stream.Read(
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
        /// 缓冲 字符串 的内容到 HGlobalCache 中。
        /// </summary>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="str">字符串</param>
        /// <param name="encoding">编码</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void ReadFrom(this HGlobalCache<byte> hGCache, string str, Encoding encoding)
        {
            hGCache.Grow(encoding.GetMaxByteCount(str.Length));

            hGCache.Count += encoding.GetBytes(
                str, 
                0, 
                str.Length, 
                hGCache.Context, 
                hGCache.Offset + hGCache.Count);
        }

        /// <summary>
        /// 将 Source 中的内容缓存到 HGlobalCache 中。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="source">Source</param>
        /// <param name="length">Source 长度</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void ReadFrom<T>(this HGlobalCache<T> hGCache, ref T source, int length) where T : unmanaged
        {
            hGCache.Grow(length);

            Underlying.CopyBlock(
                ref Underlying.As<T, byte>(ref *hGCache.Current),
                ref Underlying.As<T, byte>(ref source),
                checked((uint)((long)length * sizeof(T)))
                );

            hGCache.Count += length;
        }

        /// <summary>
        /// 将 Source 中的内容缓存到 HGlobalCache 中。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="hGCache">HGlobalCache</param>
        /// <param name="source">Source</param>
        /// <param name="length">Source 长度</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void ReadFrom<T>(this HGlobalCache<T> hGCache, T* source, int length) where T : unmanaged
        {
            hGCache.Grow(length);

            Underlying.CopyBlock(
                hGCache.Current,
                source,
                checked((uint)((long)length * sizeof(T)))
                );

            hGCache.Count += length;
        }

        /// <summary>
        /// 将字节码转换为 16 进制字符串。
        /// </summary>
        /// <param name="bytes">字节码</param>
        /// <param name="length">字节码长度</param>
        /// <returns>返回一个字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string ToHexString(ref byte bytes, int length)
        {
            const int byte_len = 2;

            var str = StringHelper.MakeString(length * byte_len);

            ref var raw = ref StringHelper.GetRawStringData(str);

            for (int i = 0; i < length; i++)
            {
                NumberHelper.Hex.AppendD2(ref Underlying.Add(ref raw, i * byte_len), Underlying.Add(ref bytes, i));
            }

            return str;
        }

        /// <summary>
        /// 将字节码转换为 16 进制字符串。
        /// </summary>
        /// <param name="bytes">字节码</param>
        /// <returns>返回一个字符串</returns>
        public static string ToHexString(this byte[] bytes)
        {
            if (bytes != null && bytes.Length > 0)
            {
                return ToHexString(ref bytes[0], bytes.Length);
            }

            return "";
        }

        /// <summary>
        /// 将字节码转换为 16 进制字符串。
        /// </summary>
        /// <param name="bytes">字节码</param>
        /// <returns>返回一个字符串</returns>
        public static string ToHexString(this ArraySegment<byte> bytes)
        {
            return ToHexString(ref bytes.Array[bytes.Offset], bytes.Count);
        }

        /// <summary>
        /// 使用指定哈希算法类型计算字节缓存的哈希值。以十六进制字符串返回。
        /// </summary>
        /// <typeparam name="THashAlgorithm">哈希算法类型</typeparam>
        /// <param name="hGCache">字节缓存</param>
        /// <returns>返回 Hash 值的十六进制字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string ComputeHash<THashAlgorithm>(this HGlobalCache<byte> hGCache) where THashAlgorithm : HashAlgorithm
        {

            var instance = THashAlgorithmInstances<THashAlgorithm>.Instance;

#if Span
            hGCache.Grow(instance.HashSize);

            var bytes = ((Span<byte>)hGCache).Slice(hGCache.Count);

            if (instance.TryComputeHash(hGCache, bytes, out var written))
            {
                bytes = bytes.Slice(0, written);
            }
            else
            {
                bytes = instance.ComputeHash(hGCache.Context, hGCache.Offset, hGCache.Count);
            }
#else

            var bytes = instance.ComputeHash(hGCache.Context, hGCache.Offset, hGCache.Count);
#endif

            return ToHexString(ref bytes[0], bytes.Length);
        }

        /// <summary>
        /// 使用指定哈希算法类型和编码类型计算字符缓存的哈希值。以十六进制字符串返回。
        /// </summary>
        /// <typeparam name="THashAlgorithm">哈希算法类型</typeparam>
        /// <param name="hGCache">字符缓存</param>
        /// <param name="encoding">指定编码</param>
        /// <returns>返回 Hash 值的十六进制字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
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
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string ComputeHash<THashAlgorithm>(this HGlobalCache<char> hGCache) where THashAlgorithm : HashAlgorithm
        {
            return ComputeHash<THashAlgorithm>(hGCache, Encoding.UTF8);
        }

        /// <summary>
        /// 使用指定哈希算法类型并使用 UTF-8 编码计算字节数组的哈希值。以十六进制字符串返回。
        /// </summary>
        /// <typeparam name="THashAlgorithm">哈希算法类型</typeparam>
        /// <param name="data">字节数组</param>
        /// <returns>返回 Hash 值的十六进制字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string ComputeHash<THashAlgorithm>(this byte[] data) where THashAlgorithm : HashAlgorithm
        {
            const int byte_len = 2;

            var bytes = THashAlgorithmInstances<THashAlgorithm>.Instance.ComputeHash(data);

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
        /// 使用指定哈希算法类型和编码类型计算字符串的哈希值。以十六进制字符串返回。
        /// </summary>
        /// <typeparam name="THashAlgorithm">哈希算法类型</typeparam>
        /// <param name="str">字符串</param>
        /// <param name="encoding">指定编码</param>
        /// <returns>返回 Hash 值的十六进制字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string ComputeHash<THashAlgorithm>(this string str, Encoding encoding) where THashAlgorithm : HashAlgorithm
        {
            var hGBytes = BytesPool.Rent();

            hGBytes.ReadFrom(str, encoding);

            var result = ComputeHash<THashAlgorithm>(hGBytes);

            BytesPool.Return(hGBytes);

            return result;
        }

        /// <summary>
        /// 使用指定哈希算法类型并使用 UTF-8 编码计算字符串的哈希值。以十六进制字符串返回。
        /// </summary>
        /// <typeparam name="THashAlgorithm">哈希算法类型</typeparam>
        /// <param name="str">字符串</param>
        /// <returns>返回 Hash 值的十六进制字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string ComputeHash<THashAlgorithm>(this string str) where THashAlgorithm : HashAlgorithm
        {
            return ComputeHash<THashAlgorithm>(str, Encoding.UTF8);
        }

        /// <summary>
        /// 在字符缓存的后面拼接一个字符串。
        /// </summary>
        /// <param name="hGCache">字符缓存</param>
        /// <param name="value">字符串</param>
        /// <returns>返回当前字符缓存</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static HGlobalCache<char> Append(this HGlobalCache<char> hGCache, string value)
        {
            hGCache.Grow(value.Length);

            Underlying.CopyBlock(
                ref Underlying.As<char, byte>(ref *hGCache.Current), 
                ref Underlying.As<char, byte>(ref StringHelper.GetRawStringData(value)),
               ((uint)value.Length) * sizeof(char));

            hGCache.Count += value.Length;

            return hGCache;
        }

        /// <summary>
        /// 在字符缓存的后面拼接一个字符。
        /// </summary>
        /// <param name="hGCache">字符缓存</param>
        /// <param name="char">字符</param>
        /// <returns>返回当前字符缓存</returns>
        public static HGlobalCache<char> Append(this HGlobalCache<char> hGCache, char @char)
        {
            hGCache.Grow(1);

            hGCache.First[hGCache.Count] = @char;

            return hGCache;
        }

        /// <summary>
        /// 在字符缓存的后面拼接一个数字格式。
        /// </summary>
        /// <param name="hGCache">字符缓存</param>
        /// <param name="value">数字</param>
        /// <param name="radix">进制数</param>
        /// <param name="fix">固定数字位数</param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static HGlobalCache<char> Append(this HGlobalCache<char> hGCache, ulong value, byte radix = 10, byte fix = 0)
        {
            hGCache.Grow(128);

            var nh = NumberHelper.GetOrCreateInstance(radix);

            if (fix > 0)
            {
                nh.ToString(value, fix, hGCache.First + hGCache.Count);

                hGCache.Count += fix;
            }
            else
            {
                hGCache.Count += nh.ToString(value, hGCache.First + hGCache.Count);
            }


            return hGCache;
        }

        /// <summary>
        /// 在字符缓存的后面拼接一个数字格式。
        /// </summary>
        /// <param name="hGCache">字符缓存</param>
        /// <param name="value">数字</param>
        /// <param name="radix">进制数</param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static HGlobalCache<char> Append(this HGlobalCache<char> hGCache, float value, byte radix = 10)
        {
            hGCache.Grow(128);

            hGCache.Count += NumberHelper.GetOrCreateInstance(radix).ToString(value, hGCache.First + hGCache.Count);

            return hGCache;
        }

        /// <summary>
        /// 在字符缓存的后面拼接一个数字格式。
        /// </summary>
        /// <param name="hGCache">字符缓存</param>
        /// <param name="value">数字</param>
        /// <param name="radix">进制数</param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static HGlobalCache<char> Append(this HGlobalCache<char> hGCache, double value, byte radix = 10)
        {
            hGCache.Grow(128);

            hGCache.Count += NumberHelper.GetOrCreateInstance(radix).ToString(value, hGCache.First + hGCache.Count);

            return hGCache;
        }

        /// <summary>
        /// 在字符缓存的后面拼接一个数字格式。
        /// </summary>
        /// <param name="hGCache">字符缓存</param>
        /// <param name="value">数字</param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static HGlobalCache<char> Append(this HGlobalCache<char> hGCache, decimal value)
        {
            hGCache.Grow(128);

            hGCache.Count += NumberHelper.ToString(value, hGCache.First + hGCache.Count);

            return hGCache;
        }

        /// <summary>
        /// 在字符缓存的后面拼接一个数字格式。
        /// </summary>
        /// <param name="hGCache">字符缓存</param>
        /// <param name="value">数字</param>
        /// <param name="radix">进制数</param>
        /// <returns>返回当前缓存</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static HGlobalCache<char> Append(this HGlobalCache<char> hGCache, long value, byte radix = 10)
        {
            hGCache.Grow(128);

            hGCache.Count += NumberHelper.GetOrCreateInstance(radix).ToString(value, hGCache.First + hGCache.Count);

            return hGCache;
        }

        /// <summary>
        /// 清除缓存中的内容
        /// </summary>
        /// <typeparam name="T">缓存类型</typeparam>
        /// <param name="hGCache">缓存</param>
        /// <returns>返回当前缓存</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static HGlobalCache<T> Clear<T>(this HGlobalCache<T> hGCache) where T : unmanaged
        {
            hGCache.Count = 0;

            return hGCache;
        }

        /// <summary>
        /// 当缓存中剩余的空间小于指定数量时进行扩容。
        /// </summary>
        /// <typeparam name="T">缓存类型</typeparam>
        /// <param name="hGCache">缓存</param>
        /// <param name="count">指定数量</param>
        /// <returns>返回当前缓存</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static HGlobalCache<T> Grow<T>(this HGlobalCache<T> hGCache, int count) where T: unmanaged
        {
            if (hGCache.Rest <= count)
            {
                hGCache.Expand(count - hGCache.Rest);
            }

            return hGCache;
        }

        /// <summary>
        /// 对缓存进行 GZip 操作。
        /// </summary>
        /// <param name="hGCache">缓存</param>
        /// <param name="mode">操作</param>
        /// <param name="ignoreLose">结果字节码的长度比原始字节码的长度长时将忽略操作</param>
        /// <returns>返回时候进行了操作</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool GZip(this HGlobalCache<byte> hGCache, CompressionMode mode, bool ignoreLose = false)
        {
            using var memoryStream = new MemoryStream();
            using var gZipStream = new GZipStream(memoryStream, mode);

            hGCache.WriteTo(gZipStream);

            gZipStream.Flush();

            if (ignoreLose && memoryStream.Length >= hGCache.Count)
            {
                return false;
            }

            var length = checked((int)memoryStream.Length);

            hGCache.Count = length;

            hGCache.Grow(0);

            Underlying.CopyBlock(
                ref hGCache.First[0], 
                ref memoryStream.GetBuffer()[0], 
                (uint)length);

            return true;
        }


        /// <summary>
        /// 读取流到结尾。
        /// </summary>
        /// <param name="stream">流</param>
        /// <returns>返回一个数组段</returns>
        public static ArraySegment<byte> ReadToEnd(this Stream stream)
        {
            if (stream is MemoryStream ms)
            {
                return InternalReadToEnd(ms);
            }

            if (stream.CanSeek)
            {
                return InternalReadToEnd(stream);
            }

            return SteadyReadToEnd(stream);
        }

        private static ArraySegment<byte> SteadyReadToEnd(Stream stream)
        {
            var hGCache = BytesPool.Rent();

            hGCache.ReadFrom(stream);

            var bytes = hGCache.ToArray();

            BytesPool.Return(hGCache);

            return new ArraySegment<byte>(bytes);
        }

        private static ArraySegment<byte> InternalReadToEnd(Stream stream)
        {
            var length = stream.GetCheckedLength();

            var bytes = new byte[length];

            for (int i = 0; i < length;)
            {
                i += stream.Read(bytes, i, length - i);
            }

            return new ArraySegment<byte>(bytes);
        }

        private static ArraySegment<byte> InternalReadToEnd(MemoryStream ms)
        {
            return new ArraySegment<byte>(ms.GetBuffer(), 0, ms.GetCheckedLength());
        }

        private static int GetCheckedLength(this Stream stream)
        {
            return checked((int)stream.Length);
        }
    }
}