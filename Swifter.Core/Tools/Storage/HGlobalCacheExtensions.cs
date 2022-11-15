using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text;

namespace Swifter.Tools
{
    /// <summary>
    /// HGlobalCache 扩展方法。
    /// </summary>
    public static partial class HGlobalCacheExtensions
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
        /// 读取文本读取器中的内容并缓存到全局缓存中。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="textReader">文本读取器</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<char> ReadFrom(this HGlobalCache<char> hGCache, TextReader textReader)
        {
            Loop:

            Grow(hGCache, 1218);

            int readCount = textReader.Read(
                hGCache.Context,
                hGCache.Offset + hGCache.Count,
                hGCache.Rest);

            hGCache.Count += readCount;

            if (readCount != 0)
            {
                goto Loop;
            }

            return hGCache;
        }

        /// <summary>
        /// 以指定编码读取流中的内容并缓存到全局缓存中。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<char> ReadFrom(this HGlobalCache<char> hGCache, Stream stream, Encoding encoding)
        {
            return hGCache.ReadFrom(new StreamReader(stream, encoding));
        }

        /// <summary>
        /// 以指定编码将数组中的内容缓存到全局缓存中。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="source">数组</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<char> ReadFrom(this HGlobalCache<char> hGCache, ArraySegment<byte> source, Encoding encoding)
        {
            if (source.Array is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            Grow(hGCache, encoding.GetMaxCharCount(source.Count));

            hGCache.Count += encoding.GetChars(
                source.Array,
                source.Offset,
                source.Count,
                hGCache.Context,
                hGCache.Offset + hGCache.Count);

            return hGCache;
        }

        /// <summary>
        /// 以指定编码将数组中的内容缓存到全局缓存中。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="source">数组</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<char> ReadFrom(this HGlobalCache<char> hGCache, byte[] source, Encoding encoding)
        {
            Grow(hGCache, encoding.GetMaxCharCount(source.Length));

            hGCache.Count += encoding.GetChars(
                source,
                0,
                source.Length,
                hGCache.Context,
                hGCache.Offset + hGCache.Count);

            return hGCache;
        }

        /// <summary>
        /// 以指定编码将字节全局缓存中的内容缓存到字符全局缓存中。
        /// </summary>
        /// <param name="hGCache">字符全局缓存</param>
        /// <param name="source">字节全局缓存</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<char> ReadFrom(this HGlobalCache<char> hGCache, HGlobalCache<byte> source, Encoding encoding)
        {
            Grow(hGCache, encoding.GetMaxCharCount(source.Count));

            hGCache.Count += encoding.GetChars(
                source.Context,
                source.Offset,
                source.Count,
                hGCache.Context,
                hGCache.Offset + hGCache.Count);

            return hGCache;
        }

        /// <summary>
        /// 读取流中的内容并缓存到全局缓存中。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="stream">流</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<byte> ReadFrom(this HGlobalCache<byte> hGCache, Stream stream)
        {
            Loop:

            Grow(hGCache, 1218);

            int readCount = stream.Read(
                hGCache.Context,
                hGCache.Offset + hGCache.Count,
                hGCache.Rest);

            hGCache.Count += readCount;

            if (readCount != 0)
            {
                goto Loop;
            }

            return hGCache;
        }

        /// <summary>
        /// 以指定编码读取文本读取器中的内容并缓存到全局缓存中。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="textReader">文本读取器</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<byte> ReadFrom(this HGlobalCache<byte> hGCache, TextReader textReader, Encoding encoding)
        {
            return hGCache.ReadFrom(CharsPool.Current().ReadFrom(textReader), encoding);
        }

        /// <summary>
        /// 以指定编码将字符串中的内容缓存到全局缓存中。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="str">字符串</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<byte> ReadFrom(this HGlobalCache<byte> hGCache, string str, Encoding encoding)
        {
            Grow(hGCache, encoding.GetMaxByteCount(str.Length));

            hGCache.Count += encoding.GetBytes(
                str,
                0,
                str.Length,
                hGCache.Context,
                hGCache.Offset + hGCache.Count);

            return hGCache;
        }

        /// <summary>
        /// 以指定编码将数组中的内容缓存到全局缓存中。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="str">数组</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<byte> ReadFrom(this HGlobalCache<byte> hGCache, char[] str, Encoding encoding)
        {
            Grow(hGCache, encoding.GetMaxByteCount(str.Length));

            hGCache.Count += encoding.GetBytes(
                str,
                0,
                str.Length,
                hGCache.Context,
                hGCache.Offset + hGCache.Count);

            return hGCache;
        }

        /// <summary>
        /// 以指定编码将数组中的内容缓存到全局缓存中。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="str">数组</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<byte> ReadFrom(this HGlobalCache<byte> hGCache, ArraySegment<char> str, Encoding encoding)
        {
            if (str.Array is null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            Grow(hGCache, encoding.GetMaxByteCount(str.Count));

            hGCache.Count += encoding.GetBytes(
                str.Array,
                str.Offset,
                str.Count,
                hGCache.Context,
                hGCache.Offset + hGCache.Count);

            return hGCache;
        }

        /// <summary>
        /// 以指定编码将字符全局缓存中的内容缓存到字节全局缓存中。
        /// </summary>
        /// <param name="hGCache">字节全局缓存</param>
        /// <param name="source">字符全局缓存</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<byte> ReadFrom(this HGlobalCache<byte> hGCache, HGlobalCache<char> source, Encoding encoding)
        {
            Grow(hGCache, encoding.GetMaxByteCount(source.Count));

            hGCache.Count += encoding.GetBytes(
                source.Context,
                source.Offset,
                source.Count,
                hGCache.Context,
                hGCache.Offset + hGCache.Count);

            return hGCache;
        }

        /// <summary>
        /// 将内存中的元素集合缓存到全局缓存中。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="source">第一个元素的引用</param>
        /// <param name="length">集合的长度</param>
        /// <returns>返回当前全局缓存</returns>
        public static unsafe HGlobalCache<T>  ReadFrom<T>(this HGlobalCache<T> hGCache, ref T source, int length) where T : unmanaged
        {
            Grow(hGCache, length);

            Unsafe.CopyBlock(
                ref Unsafe.As<T, byte>(ref *hGCache.Current),
                ref Unsafe.As<T, byte>(ref source),
                checked((uint)((long)length * sizeof(T)))
                );

            hGCache.Count += length;

            return hGCache;
        }

        /// <summary>
        /// 将内存中的元素集合缓存到全局缓存中。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="source">第一个元素的地址</param>
        /// <param name="length">集合的长度</param>
        /// <returns>返回当前全局缓存</returns>
        public static unsafe HGlobalCache<T> ReadFrom<T>(this HGlobalCache<T> hGCache, T* source, int length) where T : unmanaged
        {
            Grow(hGCache, length);

            Unsafe.CopyBlock(
                hGCache.Current,
                source,
                checked((uint)((long)length * sizeof(T)))
                );

            hGCache.Count += length;

            return hGCache;
        }

        /// <summary>
        /// 将数组中的元素缓存到全局缓存中。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="source">数组</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<T> ReadFrom<T>(this HGlobalCache<T> hGCache, ArraySegment<T> source) where T : unmanaged
        {
            if (source.Array != null && source.Count > 0)
            {
                hGCache.ReadFrom(ref source.Array[source.Offset], source.Count);
            }

            return hGCache;
        }

        /// <summary>
        /// 将数组中的元素缓存到全局缓存中。
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="source">数组</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<T> ReadFrom<T>(this HGlobalCache<T> hGCache, T[] source) where T : unmanaged
        {
            if (source != null && source.Length > 0)
            {
                hGCache.ReadFrom(ref source[0], source.Length);
            }

            return hGCache;
        }

        /// <summary>
        /// 将全局缓存中内容写入到指定的文本写入器中。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="textWriter">文本写入器</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<char> WriteTo(this HGlobalCache<char> hGCache, TextWriter textWriter)
        {
            textWriter.Write(hGCache.Context, hGCache.Offset, hGCache.Count);

            return hGCache;
        }

        /// <summary>
        /// 将全局缓存中内容写入到指定的流中。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="stream">流</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<byte> WriteTo(this HGlobalCache<byte> hGCache, Stream stream)
        {
            stream.Write(hGCache.Context, hGCache.Offset, hGCache.Count);

            return hGCache;
        }

        /// <summary>
        /// 将全局缓存中的内容以指定编码写入到流中。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<char> WriteTo(this HGlobalCache<char> hGCache, Stream stream, Encoding encoding)
        {
            BytesPool.Current().ReadFrom(hGCache, encoding).WriteTo(stream);

            return hGCache;
        }

        /// <summary>
        /// 将全局缓存中的内容写入到以指定编码文本写入器中。
        /// </summary>
        /// <param name="hGCache">全局缓存</param>
        /// <param name="textWriter">文本写入器</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回当前全局缓存</returns>
        public static HGlobalCache<byte> WriteTo(this HGlobalCache<byte> hGCache, TextWriter textWriter, Encoding encoding)
        {
            CharsPool.Current().ReadFrom(hGCache, encoding).WriteTo(textWriter);

            return hGCache;
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
        public static unsafe string ToStringEx(this HGlobalCache<char> hGCache)
        {
            return new string(hGCache.Context, hGCache.Offset, hGCache.Count);
        }

        /// <summary>
        /// 将字节缓存中的内容转换为 16 进制字符串。
        /// </summary>
        /// <param name="hGCache">字节缓存</param>
        /// <returns>返回一个字符串</returns>
        public static unsafe string ToHexString(this HGlobalCache<byte> hGCache)
        {
            return ToHexString(ref hGCache.Context[hGCache.Offset], hGCache.Count);
        }

        /// <summary>
        /// 将字节码转换为 16 进制字符串。
        /// </summary>
        /// <param name="firstByte">字节码</param>
        /// <param name="length">字节码长度</param>
        /// <returns>返回一个字符串</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe string ToHexString(ref byte firstByte, int length)
        {
            const int byte_len = 2;

            var str = StringHelper.MakeString(length * byte_len);

            fixed (char* chars = str)
            {
                NumberHelper.ToHexString(ref firstByte, length, chars);
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

            return string.Empty;
        }

        /// <summary>
        /// 将 16 进制字符串转换为字节码。
        /// </summary>
        /// <param name="hexString">16 进制字符串</param>
        /// <returns>返回字节码</returns>
        public static byte[] ToHexBytes(this string hexString)
        {
            const int byte_len = 2;

            if (hexString.Length == 0)
            {
                return new byte[0];
            }

            if (hexString.Length % byte_len != 0)
            {
                throw new FormatException(nameof(hexString));
            }

            var bytes = new byte[hexString.Length / byte_len];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = (byte)((NumberHelper.ToDigit(hexString[i * byte_len]) << 4) | (NumberHelper.ToDigit(hexString[i * byte_len + 1])));
            }

            return bytes;
        }

        /// <summary>
        /// 将字节码转换为 16 进制字符串。
        /// </summary>
        /// <param name="bytes">字节码</param>
        /// <returns>返回一个字符串</returns>
        public static string ToHexString(this ArraySegment<byte> bytes)
        {
            if (bytes.Array != null && bytes.Count > 0)
            {
                return ToHexString(ref bytes.Array[bytes.Offset], bytes.Count);
            }

            return string.Empty;
        }

        /// <summary>
        /// 在字符缓存的后面拼接一个字符串。
        /// </summary>
        /// <param name="hGCache">字符缓存</param>
        /// <param name="value">字符串</param>
        /// <returns>返回当前字符缓存</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe HGlobalCache<char> Append(this HGlobalCache<char> hGCache, string value)
        {
            Grow(hGCache, value.Length);

            Unsafe.CopyBlock(
                ref Unsafe.As<char, byte>(ref *hGCache.Current), 
                ref Unsafe.As<char, byte>(ref StringHelper.GetRawStringData(value)),
               ((uint)value.Length) * sizeof(char));

            hGCache.Count += value.Length;

            return hGCache;
        }

        /// <summary>
        /// 在字符缓存的后面拼接一个全局唯一标识符。
        /// </summary>
        /// <param name="hGCache">字符缓存</param>
        /// <param name="value">全局唯一标识符</param>
        /// <param name="withSeparator">是否包含分隔符</param>
        /// <returns>返回当前字符缓存</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe HGlobalCache<char> Append(this HGlobalCache<char> hGCache, Guid value, bool withSeparator)
        {
            Grow(hGCache, 40);

            hGCache.Count += GuidHelper.ToString(value, hGCache.Current, withSeparator);

            return hGCache;
        }

        /// <summary>
        /// 在字符缓存的后面拼接一个字符串。
        /// </summary>
        /// <param name="hGCache">字符缓存</param>
        /// <param name="value">字符串</param>
        /// <param name="offset">偏移量</param>
        /// <param name="count">字符数量</param>
        /// <returns>返回当前字符缓存</returns>
        public static unsafe HGlobalCache<char> Append(this HGlobalCache<char> hGCache, string value, int offset, int count)
        {
            if (offset >= 0 && count >= 0 && offset + count <= value.Length)
            {
                Grow(hGCache, count);

                Unsafe.CopyBlock(
                    ref Unsafe.As<char, byte>(ref *hGCache.Current),
                    ref Unsafe.As<char, byte>(ref Unsafe.Add(ref StringHelper.GetRawStringData(value), offset)),
                   ((uint)count) * sizeof(char));

                hGCache.Count += count;

                return hGCache;
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// 在字符缓存的后面拼接一个字符。
        /// </summary>
        /// <param name="hGCache">字符缓存</param>
        /// <param name="char">字符</param>
        /// <returns>返回当前字符缓存</returns>
        public static unsafe HGlobalCache<char> Append(this HGlobalCache<char> hGCache, char @char)
        {
            Grow(hGCache, 1);

            hGCache.First[hGCache.Count] = @char;

            ++hGCache.Count;

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
        public static unsafe HGlobalCache<char> Append(this HGlobalCache<char> hGCache, ulong value, byte radix = 10, byte fix = 0)
        {
            Grow(hGCache, 128);

            var nh = NumberHelper.GetOrCreateInstance(radix);

            if (fix > 0)
            {
                nh.ToString(value, fix, hGCache.Current);

                hGCache.Count += fix;
            }
            else
            {
                hGCache.Count += nh.ToString(value, hGCache.Current);
            }


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
        public static unsafe HGlobalCache<char> Append(this HGlobalCache<char> hGCache, long value, byte radix = 10)
        {
            Grow(hGCache, 128);

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
        public static unsafe HGlobalCache<char> Append(this HGlobalCache<char> hGCache, float value, byte radix = 10)
        {
            Grow(hGCache, 128);

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
        public static unsafe HGlobalCache<char> Append(this HGlobalCache<char> hGCache, double value, byte radix = 10)
        {
            Grow(hGCache, 128);

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
        public static unsafe HGlobalCache<char> Append(this HGlobalCache<char> hGCache, decimal value)
        {
            Grow(hGCache, NumberHelper.DecimalStringMaxLength);

            hGCache.Count += NumberHelper.ToString(value, hGCache.First + hGCache.Count);

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
                hGCache.Grow(count - hGCache.Rest);
            }

            return hGCache;
        }

        /// <summary>
        /// 对缓存进行 GZip 压缩。
        /// </summary>
        /// <param name="hGCache">缓存</param>
        /// <param name="ignoreLose">结果字节码的长度比原始字节码的长度长时将忽略操作</param>
        /// <returns>返回是否进行了操作</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe bool GZip(this HGlobalCache<byte> hGCache, bool ignoreLose = false)
        {
            using var memoryStream = new MemoryStream();
            using var gZipStream = new GZipStream(memoryStream, CompressionMode.Compress);

            hGCache.WriteTo(gZipStream);

            gZipStream.Flush();

            if (ignoreLose && memoryStream.Length >= hGCache.Count)
            {
                return false;
            }

            var length = checked((int)memoryStream.Length);

            hGCache.Count = length;

            Grow(hGCache, 0);

            Unsafe.CopyBlock(
                ref hGCache.First[0], 
                ref memoryStream.GetBuffer()[0], 
                (uint)length);

            return true;
        }

        /// <summary>
        /// 对缓存进行 GZip 解压。
        /// </summary>
        /// <param name="hGCache">缓存</param>
        public static unsafe void DeGZip(this HGlobalCache<byte> hGCache)
        {
            using var memoryStream = new MemoryStream(hGCache.Count);

            memoryStream.Write(hGCache.Context, hGCache.Offset, hGCache.Count);
            memoryStream.Position = 0;

            using var gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress);

            hGCache.Count = 0;

            Loop:

            var readCount = gZipStream.Read(hGCache.Context, hGCache.Count, hGCache.Rest);

            hGCache.Count += readCount;

            if (readCount > 0)
            {
                if (hGCache.Rest < 1)
                {
                    Grow(hGCache, 128);
                }

                goto Loop;
            }
        }
    }
}