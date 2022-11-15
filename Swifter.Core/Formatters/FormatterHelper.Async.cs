#if Async

using Swifter.RW;
using System.IO;
using System.Runtime.CompilerServices;
using System;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.Formatters
{
    partial class FormatterHelper
    {

        /// <summary>
        /// 异步将文档字符串反序列化到指定的对象中。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="textFormatter">文档反序列化器</param>
        /// <param name="textReader">文档字符串读取器</param>
        /// <param name="obj">对象</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async ValueTask DeserializeToAsync<T>(this ITextFormatter textFormatter, TextReader textReader, [DisallowNull]T obj)
        {
            var writer = RWHelper.CreateWriter(obj);

            if (writer is null)
            {
                throw new NotSupportedException(typeof(T).FullName);
            }

            await textFormatter.DeserializeToAsync(textReader, writer);
        }

        /// <summary>
        /// 异步将字节码内容反序列化到指定的对象中。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="binaryFormatter">字节码反序列化器</param>
        /// <param name="stream">字节码内容读取器</param>
        /// <param name="obj">对象</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async ValueTask DeserializeToAsync<T>(this IBinaryFormatter binaryFormatter, Stream stream, [DisallowNull]T obj)
        {
            var writer = RWHelper.CreateWriter(obj);

            if (writer is null)
            {
                throw new NotSupportedException(typeof(T).FullName);
            }

            await binaryFormatter.DeserializeToAsync(stream, writer);
        }
    }
}

#endif