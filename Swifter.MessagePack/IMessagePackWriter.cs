using Swifter.RW;

using System;

namespace Swifter.MessagePack
{
    /// <summary>
    /// MessagePack Writer.
    /// </summary>
    public interface IMessagePackWriter : IValueWriter
    {
        /// <summary>
        /// 获取已写入的字节数。
        /// </summary>
        int Offset { get; }
        /// <summary>
        /// 设置已写入的 MessagePack 内容长度到 HGCache 的内容数量中。
        /// </summary>
        void Flush();

        /// <summary>
        /// 重置 MessagePack 写入位置。
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
        /// 写入 MessagePack 对象开始标识符。
        /// </summary>
        void WriteMapHead(int size);

        /// <summary>
        /// 写入 MessagePack 数组开始标识符。
        /// </summary>
        void WriteArrayHead(int size);

        /// <summary>
        /// 将当前内容转换为字节数组。
        /// </summary>
        /// <returns>返回一个字节数组</returns>
        byte[] ToBytes();
    }
}