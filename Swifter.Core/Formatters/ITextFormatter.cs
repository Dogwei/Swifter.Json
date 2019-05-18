using System;
using System.IO;

namespace Swifter.Formatters
{
    /// <summary>
    /// 提供字符类文档的序列化和反序列化接口
    /// </summary>
    public partial interface ITextFormatter
    {
        /// <summary>
        /// 将文档字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="text">文档字符串</param>
        /// <returns>指定类型的值</returns>
        T Deserialize<T>(string text);

        /// <summary>
        /// 将文档字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="textReader">文档字符串读取器</param>
        /// <returns>指定类型的值</returns>
        T Deserialize<T>(TextReader textReader);

        /// <summary>
        /// 将文档字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="text">文档字符串</param>
        /// <param name="type">指定类型</param>
        /// <returns>指定类型的值</returns>
        object Deserialize(string text, Type type);

        /// <summary>
        /// 将文档字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="textReader">文档字符串读取器</param>
        /// <param name="type">指定类型</param>
        /// <returns>指定类型的值</returns>
        object Deserialize(TextReader textReader, Type type);

        /// <summary>
        /// 将指定类型的实例序列化为文档字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的值</param>
        /// <returns>返回当前文档字符串</returns>
        string Serialize<T>(T value);

        /// <summary>
        /// 将指定类型的实例序列化为文档字符串。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的值</param>
        /// <param name="textWriter">文档字符串写入器</param>
        void Serialize<T>(T value, TextWriter textWriter);

    }
}
