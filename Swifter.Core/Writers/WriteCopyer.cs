using Swifter.Readers;
using Swifter.RW;
using System;

namespace Swifter.Writers
{
    /// <summary>
    /// 值写入暂存器。
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    public sealed class WriteCopyer<TKey> : IValueWriter
    {
        private readonly IDataWriter<TKey> dataWriter;

        private readonly TKey key;

        private readonly ValueCopyer valueCopyer;

        /// <summary>
        /// 初始化值写入暂存器。
        /// </summary>
        /// <param name="dataWriter">数据读写器</param>
        /// <param name="key">键</param>
        public WriteCopyer(IDataWriter<TKey> dataWriter, TKey key)
        {
            this.dataWriter = dataWriter;
            this.key = key;

            valueCopyer = new ValueCopyer();
        }

        /// <summary>
        /// 写入一个数组结构数据。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        public void WriteArray(IDataReader<int> dataReader)
        {
            valueCopyer.WriteArray(dataReader);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Boolean 值。
        /// </summary>
        /// <param name="value">bool 值</param>
        public void WriteBoolean(bool value)
        {
            valueCopyer.WriteBoolean(value);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Byte 值。
        /// </summary>
        /// <param name="value">byte 值</param>
        public void WriteByte(byte value)
        {
            valueCopyer.WriteByte(value);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Char 值。
        /// </summary>
        /// <param name="value">char 值</param>
        public void WriteChar(char value)
        {
            valueCopyer.WriteChar(value);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 DateTime 值。
        /// </summary>
        /// <param name="value">DateTime 值</param>
        public void WriteDateTime(DateTime value)
        {
            valueCopyer.WriteDateTime(value);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Decimal 值。
        /// </summary>
        /// <param name="value">decimal 值</param>
        public void WriteDecimal(decimal value)
        {
            valueCopyer.WriteDecimal(value);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个未知类型的值。
        /// </summary>
        /// <param name="value">未知类型的值</param>
        public void DirectWrite(object value)
        {
            valueCopyer.DirectWrite(value);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Double 值。
        /// </summary>
        /// <param name="value">double 值</param>
        public void WriteDouble(double value)
        {
            valueCopyer.WriteDouble(value);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Int16 值。
        /// </summary>
        /// <param name="value">short 值</param>
        public void WriteInt16(short value)
        {
            valueCopyer.WriteInt16(value);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Int32 值。
        /// </summary>
        /// <param name="value">int 值</param>
        public void WriteInt32(int value)
        {
            valueCopyer.WriteInt32(value);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Int64 值。
        /// </summary>
        /// <param name="value">long 值</param>
        public void WriteInt64(long value)
        {
            valueCopyer.WriteInt64(value);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个对象结构数据。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        public void WriteObject(IDataReader<string> dataReader)
        {
            valueCopyer.WriteObject(dataReader);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 SByte 值。
        /// </summary>
        /// <param name="value">sbyte 值</param>
        public void WriteSByte(sbyte value)
        {
            valueCopyer.WriteSByte(value);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 Single 值。
        /// </summary>
        /// <param name="value">float 值</param>
        public void WriteSingle(float value)
        {
            valueCopyer.WriteSingle(value);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 String 值。
        /// </summary>
        /// <param name="value">string 值</param>
        public void WriteString(string value)
        {
            valueCopyer.WriteString(value);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 UInt16 值。
        /// </summary>
        /// <param name="value">ushort 值</param>
        public void WriteUInt16(ushort value)
        {
            valueCopyer.WriteUInt16(value);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 UInt32 值。
        /// </summary>
        /// <param name="value">uint 值</param>
        public void WriteUInt32(uint value)
        {
            valueCopyer.WriteUInt32(value);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 写入一个 UInt64 值。
        /// </summary>
        /// <param name="value">ulong 值</param>
        public void WriteUInt64(ulong value)
        {
            valueCopyer.WriteUInt64(value);
            dataWriter.OnWriteValue(key, valueCopyer);
        }

        /// <summary>
        /// 获取值读写器的名称。
        /// </summary>
        /// <returns>返回一个名称</returns>
        public override string ToString()
        {
            return dataWriter.ToString() + "[\"" + key + "\"]";
        }
    }
}