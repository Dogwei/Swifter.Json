
using Swifter.RW;

using System;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    /// <summary>
    /// 提供 XObjectRW 的字段读写器。
    /// </summary>
    public sealed class XFieldValueRW : IValueRW
    {
        internal readonly object obj;
        internal readonly IXFieldRW fieldRW;

        internal XFieldValueRW(object obj, IXFieldRW fieldRW)
        {
            this.obj = obj;
            this.fieldRW = fieldRW;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal T ReadValue<T>()
        {
            return fieldRW.ReadValue<T>(obj);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void WriteValue<T>(T value)
        {
            fieldRW.WriteValue(obj, value);
        }

        /// <summary>
        /// 直接读取值。
        /// </summary>
        /// <returns>值</returns>
        public object DirectRead()
        {
            return ReadValue<object>();
        }

        /// <summary>
        /// 直接写入值。
        /// </summary>
        /// <param name="value">值</param>
        public void DirectWrite(object value)
        {
            WriteValue(value);
        }

        /// <summary>
        /// 读取一个数组结构。
        /// </summary>
        /// <param name="valueWriter">数组结构写入器</param>
        public void ReadArray(IDataWriter<int> valueWriter)
        {
            var valueCopyer = new ValueCopyer();

            fieldRW.OnReadValue(obj, valueCopyer);

            valueCopyer.ReadArray(valueWriter);
        }

        /// <summary>
        /// 读取一个 Boolean 值。
        /// </summary>
        /// <returns>返回 bool 值。</returns>
        public bool ReadBoolean()
        {
            return ReadValue<bool>();
        }

        /// <summary>
        /// 读取一个 Byte 值。
        /// </summary>
        /// <returns>返回 byte 值。</returns>
        public byte ReadByte()
        {
            return ReadValue<byte>();
        }

        /// <summary>
        /// 读取一个 Char 值。
        /// </summary>
        /// <returns>返回 char 值。</returns>
        public char ReadChar()
        {
            return ReadValue<char>();
        }

        /// <summary>
        /// 读取一个 DateTime 值。
        /// </summary>
        /// <returns>返回 DateTime 值。</returns>
        public DateTime ReadDateTime()
        {
            return ReadValue<DateTime>();
        }

        /// <summary>
        /// 读取一个 Decimal 值。
        /// </summary>
        /// <returns>返回 decimal 值。</returns>
        public decimal ReadDecimal()
        {
            return ReadValue<decimal>();
        }

        /// <summary>
        /// 读取一个 Double 值。
        /// </summary>
        /// <returns>返回 double 值。</returns>
        public double ReadDouble()
        {
            return ReadValue<double>();
        }

        /// <summary>
        /// 读取一个 Int16 值。
        /// </summary>
        /// <returns>返回 short 值。</returns>
        public short ReadInt16()
        {
            return ReadValue<short>();
        }

        /// <summary>
        /// 读取一个 Int32 值。
        /// </summary>
        /// <returns>返回 int 值。</returns>
        public int ReadInt32()
        {
            return ReadValue<int>();
        }

        /// <summary>
        /// 读取一个 Int64 值。
        /// </summary>
        /// <returns>返回 long 值。</returns>
        public long ReadInt64()
        {
            return ReadValue<long>();
        }

        /// <summary>
        /// 读取一个对象结构数据。
        /// </summary>
        /// <param name="valueWriter">对象结构数据写入器</param>
        public void ReadObject(IDataWriter<string> valueWriter)
        {
            var valueCopyer = new ValueCopyer();

            fieldRW.OnReadValue(obj, valueCopyer);

            valueCopyer.ReadObject(valueWriter);
        }

        /// <summary>
        /// 读取一个 SByte 值。
        /// </summary>
        /// <returns>返回 sbyte 值。</returns>
        public sbyte ReadSByte()
        {
            return ReadValue<sbyte>();
        }

        /// <summary>
        /// 读取一个 Single 值。
        /// </summary>
        /// <returns>返回 float 值。</returns>
        public float ReadSingle()
        {
            return ReadValue<float>();
        }

        /// <summary>
        /// 读取一个 String 值。
        /// </summary>
        /// <returns>返回 string 值。</returns>
        public string ReadString()
        {
            return ReadValue<string>();
        }

        /// <summary>
        /// 读取一个 UInt16 值。
        /// </summary>
        /// <returns>返回 ushort 值。</returns>
        public ushort ReadUInt16()
        {
            return ReadValue<ushort>();
        }

        /// <summary>
        /// 读取一个 UInt32 值。
        /// </summary>
        /// <returns>返回 uint 值。</returns>
        public uint ReadUInt32()
        {
            return ReadValue<uint>();
        }

        /// <summary>
        /// 读取一个 UInt64 值。
        /// </summary>
        /// <returns>返回 ulong 值。</returns>
        public ulong ReadUInt64()
        {
            return ReadValue<ulong>();
        }

        /// <summary>
        /// 写入一个数组结构数据。
        /// </summary>
        /// <param name="dataReader">数组结构数据读取器</param>
        public void WriteArray(IDataReader<int> dataReader)
        {
            var valueCopyer = new ValueCopyer();

            valueCopyer.WriteArray(dataReader);

            fieldRW.OnWriteValue(obj, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Boolean 值。
        /// </summary>
        /// <param name="value">bool 值</param>
        public void WriteBoolean(bool value)
        {
            WriteValue(value);
        }

        /// <summary>
        /// 写入一个 Byte 值。
        /// </summary>
        /// <param name="value">byte 值</param>
        public void WriteByte(byte value)
        {
            WriteValue(value);
        }

        /// <summary>
        /// 写入一个 Char 值。
        /// </summary>
        /// <param name="value">char 值</param>
        public void WriteChar(char value)
        {
            WriteValue(value);
        }

        /// <summary>
        /// 写入一个 DateTime 值。
        /// </summary>
        /// <param name="value">DateTime 值</param>
        public void WriteDateTime(DateTime value)
        {
            WriteValue(value);
        }

        /// <summary>
        /// 写入一个 Decimal 值。
        /// </summary>
        /// <param name="value">decimal 值</param>
        public void WriteDecimal(decimal value)
        {
            WriteValue(value);
        }

        /// <summary>
        /// 写入一个 Double 值。
        /// </summary>
        /// <param name="value">double 值</param>
        public void WriteDouble(double value)
        {
            WriteValue(value);
        }

        /// <summary>
        /// 写入一个 Int16 值。
        /// </summary>
        /// <param name="value">short 值</param>
        public void WriteInt16(short value)
        {
            WriteValue(value);
        }

        /// <summary>
        /// 写入一个 Int32 值。
        /// </summary>
        /// <param name="value">int 值</param>
        public void WriteInt32(int value)
        {
            WriteValue(value);
        }

        /// <summary>
        /// 写入一个 Int64 值。
        /// </summary>
        /// <param name="value">long 值</param>
        public void WriteInt64(long value)
        {
            WriteValue(value);
        }

        /// <summary>
        /// 写入一个对象结构数据。
        /// </summary>
        /// <param name="dataReader">对象结构数据读取器</param>
        public void WriteObject(IDataReader<string> dataReader)
        {
            var valueCopyer = new ValueCopyer();

            valueCopyer.WriteObject(dataReader);

            fieldRW.OnWriteValue(obj, valueCopyer);
        }

        /// <summary>
        /// 写入一个 SByte 值。
        /// </summary>
        /// <param name="value">sbyte 值</param>
        public void WriteSByte(sbyte value)
        {
            WriteValue(value);
        }

        /// <summary>
        /// 写入一个 Single 值。
        /// </summary>
        /// <param name="value">float 值</param>
        public void WriteSingle(float value)
        {
            WriteValue(value);
        }

        /// <summary>
        /// 写入一个 String 值。
        /// </summary>
        /// <param name="value">bool 值</param>
        public void WriteString(string value)
        {
            WriteValue(value);
        }

        /// <summary>
        /// 写入一个 UInt16 值。
        /// </summary>
        /// <param name="value">ushort 值</param>
        public void WriteUInt16(ushort value)
        {
            WriteValue(value);
        }

        /// <summary>
        /// 写入一个 UInt32 值。
        /// </summary>
        /// <param name="value">uint 值</param>
        public void WriteUInt32(uint value)
        {
            WriteValue(value);
        }

        /// <summary>
        /// 写入一个 UInt64 值。
        /// </summary>
        /// <param name="value">ulong 值</param>
        public void WriteUInt64(ulong value)
        {
            WriteValue(value);
        }

        /// <summary>
        /// 获取一个可空类型的值。
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <returns>获取 Null 或该值类型的值。</returns>
        public T? ReadNullable<T>() where T : struct
        {
            return ReadValue<T?>();
        }

        /// <summary>
        /// 获取一个枚举值。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>返回一个枚举值</returns>
        public T ReadEnum<T>() where T : struct, Enum
        {
            return ReadValue<T>();
        }

        /// <summary>
        /// 写入一个枚举值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">枚举值</param>
        public void WriteEnum<T>(T value) where T : struct, Enum
        {
            WriteValue(value);
        }
    }
}