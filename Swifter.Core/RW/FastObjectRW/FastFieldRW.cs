using Swifter.Readers;
using Swifter.Writers;
using System;

namespace Swifter.RW
{

    public abstract partial class FastObjectRW<T>
    {
        /// <summary>
        /// 字段值读写器。
        /// </summary>
        public sealed class FastFieldRW : IValueRW
        {
            private readonly FastObjectRW<T> dataRW;

            private readonly long key;

            private readonly ValueCopyer valueCopyer;

            /// <summary>
            /// 初始化值暂存器。
            /// </summary>
            /// <param name="dataRW">数据读写器</param>
            /// <param name="key">键</param>
            internal FastFieldRW(FastObjectRW<T> dataRW, long key)
            {
                this.dataRW = dataRW;
                this.key = key;

                valueCopyer = new ValueCopyer();
            }

            /// <summary>
            /// 读取一个数组结构数据。
            /// </summary>
            /// <param name="valueWriter">数据写入器</param>
            public void ReadArray(IDataWriter<int> valueWriter)
            {
                dataRW.OnReadValue(key, valueCopyer);
                valueCopyer.ReadArray(valueWriter);
            }

            /// <summary>
            /// 读取一个 Boolean 值。
            /// </summary>
            /// <returns>返回一个 bool 值</returns>
            public bool ReadBoolean()
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.ReadBoolean();
            }

            /// <summary>
            /// 读取一个 Byte 值。
            /// </summary>
            /// <returns>返回一个 byte 值</returns>
            public byte ReadByte()
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.ReadByte();
            }

            /// <summary>
            /// 读取一个 Char 值。
            /// </summary>
            /// <returns>返回一个 char 值</returns>
            public char ReadChar()
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.ReadChar();
            }

            /// <summary>
            /// 读取一个 DateTime 值。
            /// </summary>
            /// <returns>返回一个 DateTime 值</returns>
            public DateTime ReadDateTime()
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.ReadDateTime();
            }

            /// <summary>
            /// 读取一个 Decimal 值。
            /// </summary>
            /// <returns>返回一个 decimal 值</returns>
            public decimal ReadDecimal()
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.ReadDecimal();
            }

            /// <summary>
            /// 读取一个未知类型的值。
            /// </summary>
            /// <returns>返回一个未知类型的值</returns>
            public object DirectRead()
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.DirectRead();
            }

            /// <summary>
            /// 读取一个 Double 值。
            /// </summary>
            /// <returns>返回一个 double 值</returns>
            public double ReadDouble()
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.ReadDouble();
            }

            /// <summary>
            /// 读取一个 Int16 值。
            /// </summary>
            /// <returns>返回一个 short 值</returns>
            public short ReadInt16()
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.ReadInt16();
            }

            /// <summary>
            /// 读取一个 Int32 值。
            /// </summary>
            /// <returns>返回一个 int 值</returns>
            public int ReadInt32()
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.ReadInt32();
            }

            /// <summary>
            /// 读取一个 Int64 值。
            /// </summary>
            /// <returns>返回一个 long 值</returns>
            public long ReadInt64()
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.ReadInt64();
            }

            /// <summary>
            /// 读取一个对象结构数据。
            /// </summary>
            /// <param name="valueWriter">数据写入器</param>
            public void ReadObject(IDataWriter<string> valueWriter)
            {
                dataRW.OnReadValue(key, valueCopyer);
                valueCopyer.ReadObject(valueWriter);
            }

            /// <summary>
            /// 读取一个 SByte 值。
            /// </summary>
            /// <returns>返回一个 sbyte 值</returns>
            public sbyte ReadSByte()
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.ReadSByte();
            }

            /// <summary>
            /// 读取一个 Single 值。
            /// </summary>
            /// <returns>返回一个 flaot 值</returns>
            public float ReadSingle()
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.ReadSingle();
            }

            /// <summary>
            /// 读取一个 String 值。
            /// </summary>
            /// <returns>返回一个 string 值</returns>
            public string ReadString()
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.ReadString();
            }

            /// <summary>
            /// 读取一个 UInt16 值。
            /// </summary>
            /// <returns>返回一个 ushort 值</returns>
            public ushort ReadUInt16()
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.ReadUInt16();
            }

            /// <summary>
            /// 读取一个 UInt32 值。
            /// </summary>
            /// <returns>返回一个 uint 值</returns>
            public uint ReadUInt32()
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.ReadUInt32();
            }

            /// <summary>
            /// 读取一个 UInt64 值。
            /// </summary>
            /// <returns>返回一个 ulong 值</returns>
            public ulong ReadUInt64()
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.ReadUInt64();
            }

            /// <summary>
            /// 读取一个可空类型的值。
            /// </summary>
            /// <typeparam name="TValue">值类型</typeparam>
            /// <returns>返回 Null 或该值类型的值</returns>
            public TValue? ReadNullable<TValue>() where TValue : struct
            {
                dataRW.OnReadValue(key, valueCopyer);
                return valueCopyer.ReadNullable<TValue>();
            }

            /// <summary>
            /// 写入一个数组结构数据。
            /// </summary>
            /// <param name="dataReader">数据读取器</param>
            public void WriteArray(IDataReader<int> dataReader)
            {
                valueCopyer.WriteArray(dataReader);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个 Boolean 值。
            /// </summary>
            /// <param name="value">bool 值</param>
            public void WriteBoolean(bool value)
            {
                valueCopyer.WriteBoolean(value);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个 Byte 值。
            /// </summary>
            /// <param name="value">byte 值</param>
            public void WriteByte(byte value)
            {
                valueCopyer.WriteByte(value);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个 Char 值。
            /// </summary>
            /// <param name="value">char 值</param>
            public void WriteChar(char value)
            {
                valueCopyer.WriteChar(value);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个 DateTime 值。
            /// </summary>
            /// <param name="value">DateTime 值</param>
            public void WriteDateTime(DateTime value)
            {
                valueCopyer.WriteDateTime(value);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个 Decimal 值。
            /// </summary>
            /// <param name="value">decimal 值</param>
            public void WriteDecimal(decimal value)
            {
                valueCopyer.WriteDecimal(value);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个未知类型的值。
            /// </summary>
            /// <param name="value">未知类型的值</param>
            public void DirectWrite(object value)
            {
                valueCopyer.DirectWrite(value);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个 Double 值。
            /// </summary>
            /// <param name="value">double 值</param>
            public void WriteDouble(double value)
            {
                valueCopyer.WriteDouble(value);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个 Int16 值。
            /// </summary>
            /// <param name="value">short 值</param>
            public void WriteInt16(short value)
            {
                valueCopyer.WriteInt16(value);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个 Int32 值。
            /// </summary>
            /// <param name="value">int 值</param>
            public void WriteInt32(int value)
            {
                valueCopyer.WriteInt32(value);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个 Int64 值。
            /// </summary>
            /// <param name="value">long 值</param>
            public void WriteInt64(long value)
            {
                valueCopyer.WriteInt64(value);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个对象结构数据。
            /// </summary>
            /// <param name="dataReader">数据读取器</param>
            public void WriteObject(IDataReader<string> dataReader)
            {
                valueCopyer.WriteObject(dataReader);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个 SByte 值。
            /// </summary>
            /// <param name="value">sbyte 值</param>
            public void WriteSByte(sbyte value)
            {
                valueCopyer.WriteSByte(value);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个 Single 值。
            /// </summary>
            /// <param name="value">float 值</param>
            public void WriteSingle(float value)
            {
                valueCopyer.WriteSingle(value);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个 String 值。
            /// </summary>
            /// <param name="value">string 值</param>
            public void WriteString(string value)
            {
                valueCopyer.WriteString(value);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个 UInt16 值。
            /// </summary>
            /// <param name="value">ushort 值</param>
            public void WriteUInt16(ushort value)
            {
                valueCopyer.WriteUInt16(value);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个 UInt32 值。
            /// </summary>
            /// <param name="value">uint 值</param>
            public void WriteUInt32(uint value)
            {
                valueCopyer.WriteUInt32(value);
                dataRW.OnWriteValue(key, valueCopyer);
            }

            /// <summary>
            /// 写入一个 UInt64 值。
            /// </summary>
            /// <param name="value">ulong 值</param>
            public void WriteUInt64(ulong value)
            {
                valueCopyer.WriteUInt64(value);
                dataRW.OnWriteValue(key, valueCopyer);
            }
        }
    }
}