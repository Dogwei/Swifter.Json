#if NET45 || NET41 || NET47 || NET471 || NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1

using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.IO;
using System.Runtime.CompilerServices;
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
            using (var hGCache = HGlobalCache<char>.OccupancyInstance())
            {
                var length = await hGCache.BufferAsync(textReader);

                unsafe
                {
                    return Deserialize<T>(hGCache.GetPointer(), length);
                }
            }
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
            using (var hGCache = HGlobalCache<char>.OccupancyInstance())
            {
                var length = await hGCache.BufferAsync(textReader);

                unsafe
                {
                    return Deserialize(hGCache.GetPointer(), length, type);
                }
            }
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
            using (var jsonSerializer = CreateJsonSerializer())
            {
                if (id != 0)
                {
                    jsonSerializer.jsonFormatter = this;
                }

                ValueInterface<T>.WriteValue((IValueWriter)jsonSerializer, value);

                await VersionDifferences.WriteCharsAsync(
                    textWriter,
                    jsonSerializer.hGlobal.Address,
                    jsonSerializer.StringLength);
            }
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
            using (var hGCache = HGlobalCache<char>.OccupancyInstance())
            {
                var length = await hGCache.BufferAsync(textReader);

                unsafe
                {
                    return DeserializeObject<T>(hGCache.GetPointer(), length);
                }
            }
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
            using (var hGCache = HGlobalCache<char>.OccupancyInstance())
            {
                var length = await hGCache.BufferAsync(textReader);

                unsafe
                {
                    return DeserializeObject(hGCache.GetPointer(), length, type);
                }
            }
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
            using (var hGCache = HGlobalCache<char>.OccupancyInstance())
            {
                var length = await hGCache.BufferAsync(textReader);

                unsafe
                {
                    return DeserializeObject<T>(hGCache.GetPointer(), length, options);
                }
            }
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
            using (var hGCache = HGlobalCache<char>.OccupancyInstance())
            {
                var length = await hGCache.BufferAsync(textReader);

                unsafe
                {
                    return DeserializeObject(hGCache.GetPointer(), length, type, options);
                }
            }
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
            using (var jsonSerializer = new JsonDefaultSerializer(DefaultMaxDepth) { textWriter = textWriter })
            {
                ValueInterface<T>.WriteValue(jsonSerializer, value);

                await VersionDifferences.WriteCharsAsync(
                    textWriter,
                    jsonSerializer.hGlobal.Address,
                    jsonSerializer.StringLength);
            }
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
            using (var jsonSerializer = CreateJsonSerializer(options))
            {
                ValueInterface<T>.WriteValue((IValueWriter)jsonSerializer, value);

                await VersionDifferences.WriteCharsAsync(
                    textWriter,
                    jsonSerializer.hGlobal.Address,
                    jsonSerializer.StringLength);
            }
        }
    }
}

#endif