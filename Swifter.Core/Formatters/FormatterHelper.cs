using Swifter.Reflection;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Swifter.Formatters
{
    /// <summary>
    /// 提供字符类文档的格式的扩展方法。
    /// </summary>
    public static unsafe partial class FormatterHelper
    {
        /// <summary>
        /// 将文档字符串反序列化到指定的对象中。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="textFormatter">文档反序列化器</param>
        /// <param name="text">文档字符串</param>
        /// <param name="obj">对象</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void DeserializeTo<T>(this ITextFormatter textFormatter, string text, T obj)
        {
            textFormatter.DeserializeTo(text, RWHelper.CreateWriter(obj));
        }

        /// <summary>
        /// 将文档字符串反序列化到指定的对象中。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="textFormatter">文档反序列化器</param>
        /// <param name="textReader">文档字符串读取器</param>
        /// <param name="obj">对象</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void DeserializeTo<T>(this ITextFormatter textFormatter, TextReader textReader, T obj)
        {
            textFormatter.DeserializeTo(textReader, RWHelper.CreateWriter(obj));
        }

        /// <summary>
        /// 将文档字符串反序列化到指定的对象中。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="textFormatter">文档反序列化器</param>
        /// <param name="hGCache">文档字符串缓存</param>
        /// <param name="obj">对象</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void DeserializeTo<T>(this ITextFormatter textFormatter, HGlobalCache<char> hGCache, T obj)
        {
            textFormatter.DeserializeTo(hGCache, RWHelper.CreateWriter(obj));
        }

        /// <summary>
        /// 将字节码内容反序列化到指定的对象中。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="binaryFormatter">字节码反序列化器</param>
        /// <param name="bytes">字节码内容</param>
        /// <param name="obj">对象</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void DeserializeTo<T>(this IBinaryFormatter binaryFormatter, byte[] bytes, T obj)
        {
            binaryFormatter.DeserializeTo(bytes, RWHelper.CreateWriter(obj));
        }

        /// <summary>
        /// 将字节码内容反序列化到指定的对象中。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="binaryFormatter">字节码反序列化器</param>
        /// <param name="stream">字节码读取器</param>
        /// <param name="obj">对象</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void DeserializeTo<T>(this IBinaryFormatter binaryFormatter, Stream stream, T obj)
        {
            binaryFormatter.DeserializeTo(stream, RWHelper.CreateWriter(obj));
        }

        /// <summary>
        /// 将字节码内容反序列化到指定的对象中。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="binaryFormatter">字节码序列化器</param>
        /// <param name="hGCache">字节码缓存</param>
        /// <param name="obj">对象</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void DeserializeTo<T>(this IBinaryFormatter binaryFormatter, HGlobalCache<byte> hGCache, T obj)
        {
            binaryFormatter.DeserializeTo(hGCache, RWHelper.CreateWriter(obj));
        }
    }
}