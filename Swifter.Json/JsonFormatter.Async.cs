#if NET45 || NET451 || NET47 || NET471 || NETSTANDARD || NETCOREAPP

using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Swifter.Json
{
    partial class JsonFormatter
    {
        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public async Task<T> DeserializeAsync<T>(TextReader textReader)
        {
            var hGCache = CharsPool.Rent();

            await hGCache.ReadFromAsync(textReader);

            var value = Deserialize<T>(hGCache);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 异步将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public async Task<object> DeserializeAsync(TextReader textReader, Type type)
        {
            var hGCache = CharsPool.Rent();

            await hGCache.ReadFromAsync(textReader);

            var value = Deserialize(hGCache, type);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 异步将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="textWriter">JSON 字符串写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public async Task SerializeAsync<T>(T value, TextWriter textWriter)
        {
            var hGCache = CharsPool.Rent();

            Serialize(value, hGCache);

            await hGCache.WriteToAsync(textWriter);

            CharsPool.Return(hGCache);
        }
        
        /// <summary>
        /// 将 JSON 字符串流的内容反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="stream">JSON 字符串流</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public async Task<T> DeserializeAsync<T>(Stream stream, Encoding encoding)
        {
            var hGCache = CharsPool.Rent();

            await hGCache.ReadFromAsync(stream, encoding);

            var value = Deserialize<T>(hGCache);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串流的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="stream">JSON 字符串流</param>
        /// <param name="encoding">编码</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public async Task<object> DeserializeAsync(Stream stream, Encoding encoding, Type type)
        {
            var hGCache = CharsPool.Rent();

            await hGCache.ReadFromAsync(stream, encoding);

            var value = Deserialize(hGCache, type);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 异步将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public async Task SerializeAsync<T>(T value, Stream stream, Encoding encoding)
        {
            var hGCache = CharsPool.Rent();

            Serialize(value, hGCache);

            await hGCache.WriteToAsync(stream, encoding);

            CharsPool.Return(hGCache);
        }

        /// <summary>
        /// 异步将 JSON 字符串反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="dataWriter">数据写入器</param>
        public async Task DeserializeToAsync(TextReader textReader, IDataWriter dataWriter)
        {
            var hGCache = CharsPool.Rent();

            await hGCache.ReadFromAsync(textReader);

            DeserializeTo(hGCache, dataWriter);

            CharsPool.Return(hGCache);
        }

        /// <summary>
        /// 将 JSON 字符串流的内容反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="stream">JSON 字符串流</param>
        /// <param name="encoding">编码</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async Task<T> DeserializeObjectAsync<T>(Stream stream, Encoding encoding)
        {
            var hGCache = CharsPool.Rent();

            await hGCache.ReadFromAsync(stream, encoding);

            var value = DeserializeObject<T>(hGCache);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async Task<T> DeserializeObjectAsync<T>(TextReader textReader)
        {
            var hGCache = CharsPool.Rent();

            await hGCache.ReadFromAsync(textReader);

            var value = DeserializeObject<T>(hGCache);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串流的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="stream">JSON 字符串流</param>
        /// <param name="encoding">编码</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async Task<object> DeserializeObjectAsync(Stream stream, Encoding encoding, Type type)
        {
            var hGCache = CharsPool.Rent();

            await hGCache.ReadFromAsync(stream, encoding);

            var value = DeserializeObject(hGCache, type);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async Task<object> DeserializeObjectAsync(TextReader textReader, Type type)
        {
            var hGCache = CharsPool.Rent();

            await hGCache.ReadFromAsync(textReader);

            var value = DeserializeObject(hGCache, type);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async Task<T> DeserializeObjectAsync<T>(TextReader textReader, JsonFormatterOptions options)
        {
            var hGCache = CharsPool.Rent();

            await hGCache.ReadFromAsync(textReader);

            var value = DeserializeObject<T>(hGCache, options);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串读取器的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="textReader">JSON 字符串读取器</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async Task<object> DeserializeObjectAsync(TextReader textReader, Type type, JsonFormatterOptions options)
        {
            var hGCache = CharsPool.Rent();

            await hGCache.ReadFromAsync(textReader);

            var value = DeserializeObject(hGCache, type, options);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串流的内容反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="stream">JSON 字符串流</param>
        /// <param name="encoding">编码</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async Task<T> DeserializeObjectAsync<T>(Stream stream, Encoding encoding, JsonFormatterOptions options)
        {
            var hGCache = CharsPool.Rent();

            await hGCache.ReadFromAsync(stream, encoding);

            var value = DeserializeObject<T>(hGCache);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将 JSON 字符串流的内容反序列化为指定类型的值。
        /// </summary>
        /// <param name="stream">JSON 字符串流</param>
        /// <param name="encoding">编码</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async Task<object> DeserializeObjectAsync(Stream stream, Encoding encoding, Type type, JsonFormatterOptions options)
        {
            var hGCache = CharsPool.Rent();

            await hGCache.ReadFromAsync(stream, encoding);

            var value = DeserializeObject(hGCache, type, options);

            CharsPool.Return(hGCache);

            return value;
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="textWriter">JSON 字符串写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async Task SerializeObjectAsync<T>(T value, TextWriter textWriter)
        {
            var hGCache = CharsPool.Rent();

            SerializeObject(value, hGCache);

            await hGCache.WriteToAsync(textWriter);

            CharsPool.Return(hGCache);
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="textWriter">JSON 字符串写入器</param>
        /// <param name="options">序列化配置</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async Task SerializeObjectAsync<T>(T value, TextWriter textWriter, JsonFormatterOptions options)
        {
            var hGCache = CharsPool.Rent();

            SerializeObject(value, hGCache, options);

            await hGCache.WriteToAsync(textWriter);

            CharsPool.Return(hGCache);
        }

        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async Task SerializeObjectAsync<T>(T value, Stream stream, Encoding encoding)
        {
            var hGCache = CharsPool.Rent();

            SerializeObject(value, hGCache);

            await hGCache.WriteToAsync(stream, encoding);

            CharsPool.Return(hGCache);
        }
        
        /// <summary>
        /// 将指定类型的实例序列化为 JSON 字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的实例</param>
        /// <param name="stream">流</param>
        /// <param name="encoding">编码</param>
        /// <param name="options">序列化配置</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async Task SerializeObjectAsync<T>(T value, Stream stream, Encoding encoding, JsonFormatterOptions options)
        {
            var hGCache = CharsPool.Rent();

            SerializeObject(value, hGCache, options);

            await hGCache.WriteToAsync(stream, encoding);

            CharsPool.Return(hGCache);
        }
    }
}

#endif