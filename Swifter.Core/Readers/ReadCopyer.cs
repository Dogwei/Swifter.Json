using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;

namespace Swifter.Readers
{
    /// <summary>
    /// 值读取暂存器。
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    public sealed class ReadCopyer<TKey> : IValueReader
    {
        private readonly IDataReader<TKey> dataReader;

        private readonly TKey key;

        private readonly ValueCopyer valueCopyer;

        /// <summary>
        /// 初始化值读取暂存器。
        /// </summary>
        /// <param name="dataReader">数据读写器</param>
        /// <param name="key">键</param>
        public ReadCopyer(IDataReader<TKey> dataReader, TKey key)
        {
            this.dataReader = dataReader;
            this.key = key;

            valueCopyer = new ValueCopyer();
        }

        /// <summary>
        /// 读取一个数组结构数据。
        /// </summary>
        /// <param name="valueWriter">数据写入器</param>
        public void ReadArray(IDataWriter<int> valueWriter)
        {
            dataReader.OnReadValue(key, valueCopyer);
            valueCopyer.ReadArray(valueWriter);
        }

        /// <summary>
        /// 读取一个 Boolean 值。
        /// </summary>
        /// <returns>返回一个 bool 值</returns>
        public bool ReadBoolean()
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadBoolean();
        }

        /// <summary>
        /// 读取一个 Byte 值。
        /// </summary>
        /// <returns>返回一个 byte 值</returns>
        public byte ReadByte()
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadByte();
        }

        /// <summary>
        /// 读取一个 Char 值。
        /// </summary>
        /// <returns>返回一个 char 值</returns>
        public char ReadChar()
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadChar();
        }

        /// <summary>
        /// 读取一个 DateTime 值。
        /// </summary>
        /// <returns>返回一个 DateTime 值</returns>
        public DateTime ReadDateTime()
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadDateTime();
        }

        /// <summary>
        /// 读取一个 Decimal 值。
        /// </summary>
        /// <returns>返回一个 decimal 值</returns>
        public decimal ReadDecimal()
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadDecimal();
        }

        /// <summary>
        /// 读取一个未知类型的值。
        /// </summary>
        /// <returns>返回一个未知类型的值</returns>
        public object DirectRead()
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.DirectRead();
        }

        /// <summary>
        /// 读取一个 Double 值。
        /// </summary>
        /// <returns>返回一个 double 值</returns>
        public double ReadDouble()
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadDouble();
        }

        /// <summary>
        /// 读取一个 Int16 值。
        /// </summary>
        /// <returns>返回一个 short 值</returns>
        public short ReadInt16()
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadInt16();
        }

        /// <summary>
        /// 读取一个 Int32 值。
        /// </summary>
        /// <returns>返回一个 int 值</returns>
        public int ReadInt32()
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadInt32();
        }

        /// <summary>
        /// 读取一个 Int64 值。
        /// </summary>
        /// <returns>返回一个 long 值</returns>
        public long ReadInt64()
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadInt64();
        }

        /// <summary>
        /// 读取一个对象结构数据。
        /// </summary>
        /// <param name="valueWriter">数据写入器</param>
        public void ReadObject(IDataWriter<string> valueWriter)
        {
            dataReader.OnReadValue(key, valueCopyer);
            valueCopyer.ReadObject(valueWriter);
        }

        /// <summary>
        /// 读取一个 SByte 值。
        /// </summary>
        /// <returns>返回一个 sbyte 值</returns>
        public sbyte ReadSByte()
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadSByte();
        }

        /// <summary>
        /// 读取一个 Single 值。
        /// </summary>
        /// <returns>返回一个 flaot 值</returns>
        public float ReadSingle()
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadSingle();
        }

        /// <summary>
        /// 读取一个 String 值。
        /// </summary>
        /// <returns>返回一个 string 值</returns>
        public string ReadString()
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadString();
        }

        /// <summary>
        /// 读取一个 UInt16 值。
        /// </summary>
        /// <returns>返回一个 ushort 值</returns>
        public ushort ReadUInt16()
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadUInt16();
        }

        /// <summary>
        /// 读取一个 UInt32 值。
        /// </summary>
        /// <returns>返回一个 uint 值</returns>
        public uint ReadUInt32()
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadUInt32();
        }

        /// <summary>
        /// 读取一个 UInt64 值。
        /// </summary>
        /// <returns>返回一个 ulong 值</returns>
        public ulong ReadUInt64()
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadUInt64();
        }

        /// <summary>
        /// 读取一个可空类型的值。
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <returns>返回 Null 或该值类型的值</returns>
        public T? ReadNullable<T>() where T : struct
        {
            dataReader.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadNullable<T>();
        }
    }
}
