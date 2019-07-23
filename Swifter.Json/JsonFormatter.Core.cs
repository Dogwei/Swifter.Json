#if NETCOREAPP && !NETCOREAPP2_0

using Swifter.Writers;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.Json
{
    unsafe partial class JsonFormatter
    {
        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="text">JSON 字符串</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(ReadOnlySpan<char> text)
        {
            fixed (char* chars = text)
            {
                return DeserializeObject<T>(chars, text.Length);
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="text">JSON 字符串</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T DeserializeObject<T>(ReadOnlySpan<char> text, JsonFormatterOptions options)
        {
            fixed (char* chars = text)
            {
                return DeserializeObject<T>(chars, text.Length, options);
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="text">JSON 字符串</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(ReadOnlySpan<char> text, Type type)
        {
            fixed (char* chars = text)
            {
                return DeserializeObject(chars, text.Length, type);
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="text">JSON 字符串</param>
        /// <param name="type">指定类型</param>
        /// <param name="options">反序列化配置项，可设置 MultiReferencingReference</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object DeserializeObject(ReadOnlySpan<char> text, Type type, JsonFormatterOptions options)
        {
            fixed (char* chars = text)
            {
                return DeserializeObject(chars, text.Length, type, options);
            }
        }


        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="text">JSON 字符串</param>
        /// <returns>返回指定类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T Deserialize<T>(ReadOnlySpan<char> text)
        {
            fixed (char* chars = text)
            {
                return Deserialize<T>(chars, text.Length);
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="text">JSON 字符串</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回指定类型的实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object Deserialize(ReadOnlySpan<char> text, Type type)
        {
            fixed (char* chars = text)
            {
                return Deserialize(chars, text.Length, type);
            }
        }

        /// <summary>
        /// 将 JSON 字符串反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="text">JSON 字符串</param>
        /// <param name="dataWriter">数据写入器</param>
        public void DeserializeTo(ReadOnlySpan<char> text, IDataWriter dataWriter)
        {
            fixed (char* chars = text)
            {
                DeserializeTo(chars, text.Length, dataWriter);
            }
        }
    }
}

#endif