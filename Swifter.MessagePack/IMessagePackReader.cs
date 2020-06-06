
using Swifter.RW;
using System;

namespace Swifter.MessagePack
{
    /// <summary>
    /// MessagePack Reader.
    /// 注意：此接口中包含 Read，Get，Skip 开头的方法，
    /// Read 开头表示此方法会读取当前值，并将下标移至下一个值；
    /// Get 开头表示此方法会读取当前值，而不会移动下标；
    /// Skip 开头表示此方法不会读取当前值，直接将下标移至下一个值。
    /// </summary>
    public interface IMessagePackReader : IValueReader
    {
        /// <summary>
        /// 获取已读取的字节数。
        /// </summary>
        int Offset { get; }
        /// <summary>
        /// 读取八个字节。
        /// </summary>
        /// <returns>返回一个 ulong 值</returns>
        ulong Read8();

        /// <summary>
        /// 读取四个字节。
        /// </summary>
        /// <returns>返回一个 uint 值</returns>
        uint Read4();

        /// <summary>
        /// 读取两个字节。
        /// </summary>
        /// <returns>返回一个 ushort 值</returns>
        ushort Read2();

        /// <summary>
        /// 读取一个字节。
        /// </summary>
        /// <returns>返回一个 byte 值</returns>
        byte Read();

        /// <summary>
        /// 获取当前值的 Token。
        /// </summary>
        /// <returns></returns>
        MessagePackToken GetToken();

        /// <summary>
        /// 读取 Guid 值。
        /// </summary>
        Guid ReadGuid();

        /// <summary>
        /// 读取 DateTimeOffset 值。
        /// </summary>
        DateTimeOffset ReadDateTimeOffset();

        /// <summary>
        /// 跳过一个 MessagePack 任意值。
        /// </summary>
        void SkipValue();

        /// <summary>
        /// 尝试读取对象的开始标识。
        /// </summary>
        /// <returns>返回当前 MessagePack 标识是否为对象开始</returns>
        int TryReadMapHead();

        /// <summary>
        /// 尝试读取数组的开始标识。
        /// </summary>
        /// <returns>返回当前 MessagePack 标识是否为数组开始</returns>
        int TryReadArrayHead();
    }
}