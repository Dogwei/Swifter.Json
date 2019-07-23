using Swifter.Tools;
using Swifter.Writers;
using System;

namespace Swifter.Json
{
    /// <summary>
    /// JSON Writer.
    /// </summary>
    public interface IJsonWriter : IValueWriter
    {
        /// <summary>
        /// 获取 HGCache 缓存。
        /// </summary>
        HGlobalCache<char> HGCache { get; }

        /// <summary>
        /// 设置已写入的 JSON 内容长度到 HGCache 的内容数量中。
        /// </summary>
        void Flush();

        /// <summary>
        /// 重置 HGCache 的内容数量和 JSON 内容长度。
        /// </summary>
        void Clear();

        /// <summary>
        /// 写入 Guid 值。
        /// </summary>
        /// <param name="value">Guid</param>
        void WriteGuid(Guid value);

        /// <summary>
        /// 写入 DateTimeOffset 值。
        /// </summary>
        /// <param name="value">DateTimeOffset</param>
        void WriteDateTimeOffset(DateTimeOffset value);

        /// <summary>
        /// 写入 JSON 对象开始标识符。
        /// </summary>
        void WriteBeginObject();

        /// <summary>
        /// 写入 JSON 对象结束标识符。
        /// </summary>
        void WriteEndObject();

        /// <summary>
        /// 写入 JSON 数组开始标识符。
        /// </summary>
        void WriteBeginArray();

        /// <summary>
        /// 写入 JSON 数组结束标识符。
        /// </summary>
        void WriteEndArray();

        /// <summary>
        /// 写入 JSON 对象中的字段名称。
        /// </summary>
        /// <param name="name">字段名称</param>
        void WritePropertyName(string name);
    }
}