
using Swifter.RW;
using System;
using System.Collections.Generic;

namespace Swifter.Json
{
    /// <summary>
    /// JSON Reader.
    /// </summary>
    public interface IJsonReader : IValueReader
    {
        /// <summary>
        /// 当前 JSON 值是否为 Object。
        /// </summary>
        bool IsObject { get; }

        /// <summary>
        /// 当前 JSON 值是否为 Array。
        /// </summary>
        bool IsArray { get; }

        /// <summary>
        /// 当前 JSON 值是否为 字符串。
        /// </summary>
        bool IsString { get; }

        /// <summary>
        /// 当前 JSON 值是否为 数字。
        /// </summary>
        bool IsNumber { get; }

        /// <summary>
        /// 当前 JSON 值是否为 Null。
        /// </summary>
        bool IsNull { get; }

        /// <summary>
        /// 当前 JSON 值是否为 True/False。
        /// </summary>
        bool IsBoolean { get; }

        /// <summary>
        /// 当前 JSON 值是否为 文本值（没有引号）。
        /// </summary>
        bool IsValue { get; }

        /// <summary>
        /// 读取 Guid 值。
        /// </summary>
        Guid ReadGuid();

        /// <summary>
        /// 读取 DateTimeOffset 值。
        /// </summary>
        DateTimeOffset ReadDateTimeOffset();

        /// <summary>
        /// 读取 字符串 值。
        /// </summary>
        /// <param name="length">返回字符串长度</param>
        /// <returns>返回字符串第一个字符的引用</returns>
        ref readonly char ReadString(out int length);

        /// <summary>
        /// 跳过一个 JSON 任意值。
        /// </summary>
        void SkipValue();

        /// <summary>
        /// 跳过一个 JSON 对象中字段的名称。
        /// </summary>
        void SkipPropertyName();

        /// <summary>
        /// 获取当前 JSON 对象的遍历器。注意：在读取器中，每个值都必须读且只读一次！
        /// </summary>
        /// <returns>返回一个键值对遍历器</returns>
        IEnumerable<KeyValuePair<string, IJsonReader>> ReadObject();

        /// <summary>
        /// 获取当前 JSON 数组的遍历器。注意：在读取器中，每个值都必须读且只读一次！
        /// </summary>
        /// <returns>返回一个值遍历器</returns>
        IEnumerable<IJsonReader> ReadArray();

        /// <summary>
        /// 读取 JSON 对象中字段的名称。
        /// </summary>
        /// <returns>返回一个字符串</returns>
        string ReadPropertyName();

        /// <summary>
        /// 读取 JSON 对象中字段的名称。
        /// </summary>
        /// <param name="length">返回字符串长度</param>
        /// <returns>返回字符串第一个字符的引用</returns>
        ref readonly char ReadPropertyName(out int length);

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