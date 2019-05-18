#if NET45 || NET41 || NET47 || NET471 || NETSTANDARD2_0 || NETCOREAPP2_0 || NETCOREAPP2_1


using System;
using System.IO;
using System.Threading.Tasks;

namespace Swifter.Formatters
{
    partial interface ITextFormatter
    {
        /// <summary>
        /// 异步将文档字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="textReader">文档字符串读取器</param>
        /// <returns>指定类型的值</returns>
        Task<T> DeserializeAsync<T>(TextReader textReader);


        /// <summary>
        /// 异步将文档字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="textReader">文档字符串读取器</param>
        /// <param name="type">指定类型</param>
        /// <returns>指定类型的值</returns>
        Task<object> DeserializeAsync(TextReader textReader, Type type);


        /// <summary>
        /// 异步将指定类型的实例序列化为文档字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的值</param>
        /// <param name="textWriter">文档字符串写入器</param>
        Task SerializeAsync<T>(T value, TextWriter textWriter);

    }
}

#endif