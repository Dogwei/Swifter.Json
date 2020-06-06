
using System;

namespace Swifter.RW
{
    /// <summary>
    /// 提供基础类型的值的读取器
    /// </summary>
    public interface IValueReader
    {
        /// <summary>
        /// 读取 Int64 值。
        /// </summary>
        /// <returns>返回一个 long 值。</returns>
        long ReadInt64();

        /// <summary>
        /// 读取 Double 值。
        /// </summary>
        /// <returns>返回一个 double 值。</returns>
        double ReadDouble();

        /// <summary>
        /// 读取 String 值。
        /// </summary>
        /// <returns>返回一个 string 值。</returns>
        string ReadString();

        /// <summary>
        /// 读取 Boolean 值。
        /// </summary>
        /// <returns>返回一个 bool 值。</returns>
        bool ReadBoolean();

        /// <summary>
        /// 读取 Byte 值。
        /// </summary>
        /// <returns>返回一个 byte 值。</returns>
        byte ReadByte();

        /// <summary>
        /// 读取 Char 值。
        /// </summary>
        /// <returns>返回一个 char 值。</returns>
        char ReadChar();

        /// <summary>
        /// 读取 DateTime 值。
        /// </summary>
        /// <returns>返回一个 DateTime 值。</returns>
        DateTime ReadDateTime();

        /// <summary>
        /// 读取 Decimal 值。
        /// </summary>
        /// <returns>返回一个 decimal 值。</returns>
        decimal ReadDecimal();

        /// <summary>
        /// 读取 Int16 值。
        /// </summary>
        /// <returns>返回一个 short 值。</returns>
        short ReadInt16();

        /// <summary>
        /// 读取 Int32 值。
        /// </summary>
        /// <returns>返回一个 int 值。</returns>
        int ReadInt32();

        /// <summary>
        /// 读取 SByte 值。
        /// </summary>
        /// <returns>返回一个 sbyte 值。</returns>
        sbyte ReadSByte();

        /// <summary>
        /// 读取 Single 值。
        /// </summary>
        /// <returns>返回一个 float 值。</returns>
        float ReadSingle();

        /// <summary>
        /// 读取 UInt16 值。
        /// </summary>
        /// <returns>返回一个 ushort 值。</returns>
        ushort ReadUInt16();

        /// <summary>
        /// 读取 UInt32 值。
        /// </summary>
        /// <returns>返回一个 uint 值。</returns>
        uint ReadUInt32();

        /// <summary>
        /// 读取 UInt64 值。
        /// </summary>
        /// <returns>返回一个 ulong 值。</returns>
        ulong ReadUInt64();

        /// <summary>
        /// 读取一个对象数据结构。
        /// </summary>
        /// <param name="valueWriter">数据写入器</param>
        void ReadObject(IDataWriter<string> valueWriter);

        /// <summary>
        /// 读取一个数组数据结构。
        /// </summary>
        /// <param name="valueWriter">数据写入器</param>
        void ReadArray(IDataWriter<int> valueWriter);

        /// <summary>
        /// 直接读取一个值。
        /// </summary>
        /// <returns>返回一个未知类型的值。</returns>
        object DirectRead();

        /// <summary>
        /// 读取一个可空类型。注意：不可依赖此方法读取非空类型的值。
        /// </summary>
        /// <typeparam name="T">一个值类型</typeparam>
        /// <returns>返回一个可空类型的值</returns>
        T? ReadNullable<T>() where T : struct;

        /// <summary>
        /// 读取一个枚举。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>返回一个枚举值</returns>
        T ReadEnum<T>() where T : struct, Enum;
    }

    /// <summary>
    /// 提供具体类型的值的读取方法。
    /// </summary>
    /// <typeparam name="T">具体类型</typeparam>
    public interface IValueReader<T>
    {
        /// <summary>
        /// 读取该类型的值。
        /// </summary>
        /// <returns>返回该类型的值</returns>
        T ReadValue();
    }
}