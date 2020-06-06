
using Swifter.RW;
using System;

namespace Swifter.Json
{
    /// <summary>
    /// JSON Reader.
    /// 注意：此接口中包含 Read，Get，Skip 开头的方法，
    /// Read 开头表示此方法会读取当前值，并将下标移至下一个值；
    /// Get 开头表示此方法会读取当前值，而不会移动下标；
    /// Skip 开头表示此方法不会读取当前值，直接将下标移至下一个值。
    /// </summary>
    public interface IJsonReader : IValueReader
    {
        /// <summary>
        /// 获取已读取的字符数。
        /// </summary>
        int Offset { get; }
        /// <summary>
        /// 获取当前值的 Token。
        /// </summary>
        /// <returns></returns>
        JsonToken GetToken();

        /// <summary>
        /// 读取 Guid 值。
        /// </summary>
        Guid ReadGuid();

        /// <summary>
        /// 读取 DateTimeOffset 值。
        /// </summary>
        DateTimeOffset ReadDateTimeOffset();

        /// <summary>
        /// 跳过一个 JSON 任意值。
        /// </summary>
        void SkipValue();

        /// <summary>
        /// 读取 JSON 对象中字段的名称。
        /// </summary>
        /// <returns>返回一个字符串</returns>
        string ReadPropertyName();

        /// <summary>
        /// 尝试读取对象的开始标识。
        /// </summary>
        /// <returns>返回当前 JSON 标识是否为对象开始</returns>
        bool TryReadBeginObject();

        /// <summary>
        /// 尝试读取对象的结束标识。
        /// </summary>
        /// <returns>返回当前 JSON 标识是否为对象结束</returns>
        bool TryReadEndObject();

        /// <summary>
        /// 尝试读取数组的开始标识。
        /// </summary>
        /// <returns>返回当前 JSON 标识是否为数组开始</returns>
        bool TryReadBeginArray();

        /// <summary>
        /// 尝试读取数组的结束标识。
        /// </summary>
        /// <returns>返回当前 JSON 标识是否为数组结束</returns>
        bool TryReadEndArray();
    }
}