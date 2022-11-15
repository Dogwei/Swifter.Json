
using System;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.RW
{
    /// <summary>
    /// 基础类型的值写入器。
    /// </summary>
    public interface IValueWriter
    {
        /// <summary>
        /// 写入一个 <see cref="Boolean"/> 值
        /// </summary>
        void WriteBoolean(bool value);

        /// <summary>
        /// 写入一个 <see cref="Byte"/> 值
        /// </summary>
        void WriteByte(byte value);

        /// <summary>
        /// 写入一个 <see cref="SByte"/> 值
        /// </summary>
        void WriteSByte(sbyte value);

        /// <summary>
        /// 写入一个 <see cref="Int16"/> 值
        /// </summary>
        void WriteInt16(short value);

        /// <summary>
        /// 写入一个 <see cref="Char"/> 值
        /// </summary>
        void WriteChar(char value);

        /// <summary>
        /// 写入一个 <see cref="UInt16"/> 值
        /// </summary>
        void WriteUInt16(ushort value);

        /// <summary>
        /// 写入一个 <see cref="Int32"/> 值
        /// </summary>
        void WriteInt32(int value);

        /// <summary>
        /// 写入一个 <see cref="Boolean"/> 值
        /// </summary>
        void WriteSingle(float value);

        /// <summary>
        /// 写入一个 <see cref="UInt32"/> 值
        /// </summary>
        void WriteUInt32(uint value);

        /// <summary>
        /// 写入一个 <see cref="Int64"/> 值
        /// </summary>
        void WriteInt64(long value);

        /// <summary>
        /// 写入一个 <see cref="Double"/> 值
        /// </summary>
        void WriteDouble(double value);

        /// <summary>
        /// 写入一个 <see cref="UInt64"/> 值
        /// </summary>
        void WriteUInt64(ulong value);

        /// <summary>
        /// 写入一个 <see cref="String"/> 值
        /// </summary>
        void WriteString(string? value);

        /// <summary>
        /// 写入一个 <see cref="DateTime"/> 值
        /// </summary>
        void WriteDateTime(DateTime value);

        /// <summary>
        /// 写入一个 <see cref="DateTimeOffset"/> 值
        /// </summary>
        void WriteDateTimeOffset(DateTimeOffset value);

        /// <summary>
        /// 写入一个 <see cref="TimeSpan"/> 值
        /// </summary>
        void WriteTimeSpan(TimeSpan value);

        /// <summary>
        /// 写入一个 <see cref="Guid"/> 值
        /// </summary>
        void WriteGuid(Guid value);

        /// <summary>
        /// 写入一个 <see cref="Decimal"/> 值
        /// </summary>
        void WriteDecimal(decimal value);

        /// <summary>
        /// 写入一个对象结构的值
        /// </summary>
        /// <param name="dataReader">对象结构读取器</param>
        void WriteObject(IDataReader<string> dataReader);
        /// <summary>
        /// 写入一个数组结构的值
        /// </summary>
        /// <param name="dataReader">数组结构读取器</param>
        void WriteArray(IDataReader<int> dataReader);

        /// <summary>
        /// 写入一个枚举值。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        void WriteEnum<T>(T value) where T : struct, Enum;

        /// <summary>
        /// 直接写入一个值。
        /// </summary>
        void DirectWrite(object? value);

        /// <summary>
        /// 获取值的类型。
        /// <see langword="null"/> 表示未知类型。
        /// </summary>
        Type? ValueType { get; }
    }

    /// <summary>
    /// 指定类型的值写入器
    /// </summary>
    /// <typeparam name="T">指定类型</typeparam>
    public interface IValueWriter<T>
    {
        /// <summary>
        /// 写入该类型的值
        /// </summary>
        /// <param name="value">值</param>
        void WriteValue(T? value);
    }
}