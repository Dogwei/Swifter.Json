#if Span

using Swifter.RW;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.Formatters
{
    partial class FormatterHelper
    {
        /// <summary>
        /// 将文档字符串反序列化到指定的对象中。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="textFormatter">文档反序列化器</param>
        /// <param name="text">文档字符串</param>
        /// <param name="obj">对象</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void DeserializeTo<T>(this ITextFormatter textFormatter, ReadOnlySpan<char> text, T obj)
        {
            textFormatter.DeserializeTo(text, RWHelper.CreateWriter(obj));
        }

        /// <summary>
        /// 将字节码内容反序列化到指定的对象中。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="binaryFromatter">字节码反序列化器</param>
        /// <param name="text">文档字符串</param>
        /// <param name="obj">对象</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void DeserializeTo<T>(this IBinaryFormatter binaryFromatter, ReadOnlySpan<byte> text, T obj)
        {
            binaryFromatter.DeserializeTo(text, RWHelper.CreateWriter(obj));
        }
    }
}
#endif