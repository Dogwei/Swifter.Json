#if Async

using Swifter.RW;
using Swifter.Tools;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

#if ValueTask

using Task = System.Threading.Tasks.ValueTask;

#endif

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
        public static async Task DeserializeToAsync<T>(this ITextFormatter textFormatter, TextReader textReader, T obj)
        {
            await textFormatter.DeserializeToAsync(textReader, RWHelper.CreateWriter(obj));
        }

        /// <summary>
        /// 异步将字节码内容反序列化到指定的对象中。
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="textFormatter">字节码反序列化器</param>
        /// <param name="stream">字节码内容读取器</param>
        /// <param name="obj">对象</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static async Task DeserializeToAsync<T>(this ITextFormatter textFormatter, Stream stream, T obj)
        {
            await textFormatter.DeserializeToAsync(stream, RWHelper.CreateWriter(obj));
        }
    }
}

#endif