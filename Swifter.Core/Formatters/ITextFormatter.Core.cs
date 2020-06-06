#if Span


using Swifter.RW;
using System;

namespace Swifter.Formatters
{
    partial interface ITextFormatter
    {
        /// <summary>
        /// 将文档字符串反序列化为指定类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="text">文档字符串</param>
        /// <returns>指定类型的值</returns>
        T Deserialize<T>(ReadOnlySpan<char> text);

        /// <summary>
        /// 将文档字符串反序列化到指定的数据写入器中。
        /// </summary>
        /// <param name="text">文档字符串</param>
        /// <param name="dataWriter">数据写入器</param>
        void DeserializeTo(ReadOnlySpan<char> text, IDataWriter dataWriter);

        /// <summary>
        /// 将文档字符串反序列化为指定类型的值。
        /// </summary>
        /// <param name="text">文档字符串</param>
        /// <param name="type">指定类型</param>
        /// <returns>指定类型的值</returns>
        object Deserialize(ReadOnlySpan<char> text, Type type);
    }
}

#endif