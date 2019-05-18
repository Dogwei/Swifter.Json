using Swifter.Readers;
using System;

namespace Swifter.Writers
{
    /// <summary>
    /// 基础类型的值写入器。
    /// </summary>
    public interface IValueWriter
    {
        /// <summary>
        /// 写入一个 Boolean 值
        /// </summary>
        /// <param name="value">bool 值</param>
        void WriteBoolean(bool value);

        /// <summary>
        /// 写入一个 Byte 值
        /// </summary>
        /// <param name="value">bool 值</param>
        void WriteByte(byte value);

        /// <summary>
        /// 写入一个 SByte 值
        /// </summary>
        /// <param name="value">bool 值</param>
        void WriteSByte(sbyte value);

        /// <summary>
        /// 写入一个 Int16 值
        /// </summary>
        /// <param name="value">bool 值</param>
        void WriteInt16(short value);

        /// <summary>
        /// 写入一个 Char 值
        /// </summary>
        /// <param name="value">bool 值</param>
        void WriteChar(char value);

        /// <summary>
        /// 写入一个 UInt16 值
        /// </summary>
        /// <param name="value">bool 值</param>
        void WriteUInt16(ushort value);

        /// <summary>
        /// 写入一个 Int32 值
        /// </summary>
        /// <param name="value">bool 值</param>
        void WriteInt32(int value);

        /// <summary>
        /// 写入一个 Boolean 值
        /// </summary>
        /// <param name="value">bool 值</param>
        void WriteSingle(float value);

        /// <summary>
        /// 写入一个 UInt32 值
        /// </summary>
        /// <param name="value">bool 值</param>
        void WriteUInt32(uint value);

        /// <summary>
        /// 写入一个 Int64 值
        /// </summary>
        /// <param name="value">bool 值</param>
        void WriteInt64(long value);

        /// <summary>
        /// 写入一个 Double 值
        /// </summary>
        /// <param name="value">bool 值</param>
        void WriteDouble(double value);

        /// <summary>
        /// 写入一个 UInt64 值
        /// </summary>
        /// <param name="value">bool 值</param>
        void WriteUInt64(ulong value);

        /// <summary>
        /// 写入一个 String 值
        /// </summary>
        /// <param name="value">bool 值</param>
        void WriteString(string value);

        /// <summary>
        /// 写入一个 DateTime 值
        /// </summary>
        /// <param name="value">bool 值</param>
        void WriteDateTime(DateTime value);

        /// <summary>
        /// 写入一个 Decimal 值
        /// </summary>
        /// <param name="value">bool 值</param>
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
        /// 直接写入一个对象。
        /// </summary>
        /// <param name="value">对象 值</param>
        void DirectWrite(object value);
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
        void WriteValue(T value);
    }
}