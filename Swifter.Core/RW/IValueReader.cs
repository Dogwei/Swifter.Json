
using System;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.RW
{
    /// <summary>
    /// 提供基础类型的值的读取器
    /// </summary>
    public interface IValueReader
    {
        /// <summary>
        /// 读取 <see cref="Int64"/> 值。
        /// </summary>
        long ReadInt64();

        /// <summary>
        /// 读取 <see cref="Double"/> 值。
        /// </summary>
        double ReadDouble();

        /// <summary>
        /// 读取 <see cref="String"/> 值。
        /// </summary>
        string? ReadString();

        /// <summary>
        /// 读取 <see cref="Boolean"/> 值。
        /// </summary>
        bool ReadBoolean();

        /// <summary>
        /// 读取 <see cref="Byte"/> 值。
        /// </summary>
        byte ReadByte();

        /// <summary>
        /// 读取 <see cref="Char"/> 值。
        /// </summary>
        char ReadChar();

        /// <summary>
        /// 读取 <see cref="DateTime"/> 值。
        /// </summary>
        DateTime ReadDateTime();

        /// <summary>
        /// 读取一个 <see cref="DateTimeOffset"/> 值。
        /// </summary>
        DateTimeOffset ReadDateTimeOffset();

        /// <summary>
        /// 读取一个 <see cref="TimeSpan"/> 值。
        /// </summary>
        TimeSpan ReadTimeSpan();

        /// <summary>
        /// 读取一个 <see cref="Guid"/> 值。
        /// </summary>
        Guid ReadGuid();

        /// <summary>
        /// 读取 <see cref="Decimal"/> 值。
        /// </summary>
        decimal ReadDecimal();

        /// <summary>
        /// 读取 <see cref="Int16"/> 值。
        /// </summary>
        short ReadInt16();

        /// <summary>
        /// 读取 <see cref="Int32"/> 值。
        /// </summary>
        int ReadInt32();

        /// <summary>
        /// 读取 <see cref="SByte"/> 值。
        /// </summary>
        sbyte ReadSByte();

        /// <summary>
        /// 读取 <see cref="Single"/> 值。
        /// </summary>
        float ReadSingle();

        /// <summary>
        /// 读取 <see cref="UInt16"/> 值。
        /// </summary>
        ushort ReadUInt16();

        /// <summary>
        /// 读取 <see cref="UInt32"/> 值。
        /// </summary>
        uint ReadUInt32();

        /// <summary>
        /// 读取 <see cref="UInt64"/> 值。
        /// </summary>
        ulong ReadUInt64();

        /// <summary>
        /// 读取一个对象数据结构。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        void ReadObject(IDataWriter<string> dataWriter);

        /// <summary>
        /// 读取一个数组数据结构。
        /// </summary>
        /// <param name="dataWriter">数组结构的写入器</param>
        void ReadArray(IDataWriter<int> dataWriter);

        /// <summary>
        /// 直接获取该值。
        /// </summary>
        object? DirectRead();

        /// <summary>
        /// 舍弃当前值。
        /// </summary>
        void Pop();

        /// <summary>
        /// 读取一个可空类型。注意：不可依赖此方法读取非空类型的值。
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        T? ReadNullable<T>() where T : struct;

        /// <summary>
        /// 读取一个枚举。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        T ReadEnum<T>() where T : struct, Enum;

        /// <summary>
        /// 获取值的类型。
        /// <see langword="null"/> 表示未知类型。
        /// </summary>
        Type? ValueType { get; }
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
        T? ReadValue();
    }
}