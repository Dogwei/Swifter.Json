using Swifter.Tools;
using System;

namespace Swifter.RW
{
    /// <summary>
    /// 提供直接读写器的基本实现。
    /// </summary>
    public abstract class BaseDirectRW :
        IValueReader,
        IValueWriter,
        IValueReader<Guid>,
        IValueWriter<Guid>,
        IValueReader<DateTimeOffset>,
        IValueWriter<DateTimeOffset>,
        IValueReader<TimeSpan>,
        IValueWriter<TimeSpan>
    {
        /// <summary>
        /// 子类需要实现的直接读取。
        /// </summary>
        /// <returns></returns>
        public abstract object DirectRead();

        /// <summary>
        /// 子类需要实现的直接写入。
        /// </summary>
        /// <param name="value"></param>
        public abstract void DirectWrite(object value);

        /// <summary>
        /// 读取一个数组。
        /// </summary>
        /// <param name="valueWriter">数据写入器</param>
        public virtual void ReadArray(IDataWriter<int> valueWriter)
        {
            var obj = DirectRead();

            if (obj != null)
            {
                if (valueWriter.ContentType?.IsInstanceOfType(obj) == true)
                {
                    valueWriter.Content = obj;
                }
                else
                {
                    ValueCopyer.ValueOf(obj).ReadArray(valueWriter);
                }
            }
        }

        /// <summary>
        /// 读取一个对象。
        /// </summary>
        /// <param name="valueWriter">对象写入器</param>
        public virtual void ReadObject(IDataWriter<string> valueWriter)
        {
            var obj = DirectRead();

            if (obj != null)
            {
                if (valueWriter.ContentType?.IsInstanceOfType(obj) == true)
                {
                    valueWriter.Content = obj;
                }
                else
                {
                    ValueCopyer.ValueOf(obj).ReadObject(valueWriter);
                }
            }
        }

        /// <summary>
        /// 读取一个布尔值。
        /// </summary>
        /// <returns>返回一个 <see cref="bool"/> 值</returns>
        public bool ReadBoolean()
        {
            return Convert.ToBoolean(DirectRead());
        }

        /// <summary>
        /// 读取一个单字节无符号整数。
        /// </summary>
        /// <returns>返回一个 <see cref="byte"/> 值</returns>
        public byte ReadByte()
        {
            return Convert.ToByte(DirectRead());
        }

        /// <summary>
        /// 读取一个字符。
        /// </summary>
        /// <returns>返回一个 <see cref="char"/> 值</returns>
        public char ReadChar()
        {
            return Convert.ToChar(DirectRead());
        }

        /// <summary>
        /// 读取一个日期和时间。
        /// </summary>
        /// <returns>返回一个 <see cref="DateTime"/> 值</returns>
        public DateTime ReadDateTime()
        {
            return Convert.ToDateTime(DirectRead());
        }

        /// <summary>
        /// 读取一个十进制数字。
        /// </summary>
        /// <returns>返回一个 <see cref="decimal"/> 值</returns>
        public decimal ReadDecimal()
        {
            return Convert.ToDecimal(DirectRead());
        }

        /// <summary>
        /// 读取一个双精度浮点数。
        /// </summary>
        /// <returns>返回一个 <see cref="double"/> 值</returns>
        public double ReadDouble()
        {
            return Convert.ToDouble(DirectRead());
        }

        /// <summary>
        /// 读取一个双字节有符号整数。
        /// </summary>
        /// <returns>返回一个 <see cref="short"/> 值</returns>
        public short ReadInt16()
        {
            return Convert.ToInt16(DirectRead());
        }

        /// <summary>
        /// 读取一个四字节有符号整数。
        /// </summary>
        /// <returns>返回一个 <see cref="int"/> 值</returns>
        public int ReadInt32()
        {
            return Convert.ToInt32(DirectRead());
        }

        /// <summary>
        /// 读取一个八字节有符号整数。
        /// </summary>
        /// <returns>返回一个 <see cref="long"/> 值</returns>
        public long ReadInt64()
        {
            return Convert.ToInt64(DirectRead());
        }

        /// <summary>
        /// 读取一个单字节有符号整数。
        /// </summary>
        /// <returns>返回一个 <see cref="sbyte"/> 值</returns>
        public sbyte ReadSByte()
        {
            return Convert.ToSByte(DirectRead());
        }

        /// <summary>
        /// 读取一个单精度浮点数。
        /// </summary>
        /// <returns>返回一个 <see cref="float"/> 值</returns>
        public float ReadSingle()
        {
            return Convert.ToSingle(DirectRead());
        }

        /// <summary>
        /// 读取一个字符串。
        /// </summary>
        /// <returns>返回一个 <see cref="string"/> 值</returns>
        public string ReadString()
        {
            var value = DirectRead();

            if (value == null)
            {
                return null;
            }

            if (value is string str)
            {
                return str;
            }

            return Convert.ToString(value);
        }

        /// <summary>
        /// 读取一个双字节无符号整数。
        /// </summary>
        /// <returns>返回一个 <see cref="ushort"/> 值</returns>
        public ushort ReadUInt16()
        {
            return Convert.ToUInt16(DirectRead());
        }

        /// <summary>
        /// 读取一个四字节无符号整数。
        /// </summary>
        /// <returns>返回一个 <see cref="uint"/> 值</returns>
        public uint ReadUInt32()
        {
            return Convert.ToUInt32(DirectRead());
        }

        /// <summary>
        /// 读取一个八字节无符号整数。
        /// </summary>
        /// <returns>返回一个 <see cref="ulong"/> 值</returns>
        public ulong ReadUInt64()
        {
            return Convert.ToUInt64(DirectRead());
        }

        /// <summary>
        /// 读取一个枚举。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>返回该枚举类型的值</returns>
        public T ReadEnum<T>() where T : struct, Enum
        {
            var value = DirectRead();

            if (value is T t)
            {
                return t;
            }

            if (value is string str)
            {
                return (T)Enum.Parse(typeof(T), str);
            }

            if (value is null)
            {
                return default;
            }

            return (T)Enum.ToObject(typeof(T), DirectRead());
        }

        /// <summary>
        /// 读取一个全局唯一标识符。
        /// </summary>
        /// <returns>返回一个 <see cref="Guid"/> 值</returns>
        public Guid ReadGuid()
        {
            var value = DirectRead();

            if (value is Guid guid)
            {
                return guid;
            }

            if (value is string str)
            {
                return new Guid(str);
            }

            if (value is null)
            {
                return default;
            }

            return XConvert<Guid>.FromObject(value);
        }

        /// <summary>
        /// 读取一个包含偏移量的日期和时间。
        /// </summary>
        /// <returns>返回一个 <see cref="DateTimeOffset"/> 值</returns>
        public DateTimeOffset ReadDateTimeOffset()
        {
            var value = DirectRead();

            if (value is DateTimeOffset dto)
            {
                return dto;
            }

            if (value is string str)
            {
                return DateTimeOffset.Parse(str);
            }

            if (value is null)
            {
                return default;
            }

            return XConvert<DateTimeOffset>.FromObject(value);
        }

        /// <summary>
        /// 读取一个时间间隔。
        /// </summary>
        /// <returns>返回一个 <see cref="TimeSpan"/> 值</returns>
        public TimeSpan ReadTimeSpan()
        {
            var value = DirectRead();

            if (value is TimeSpan ts)
            {
                return ts;
            }

            if (value is string str)
            {
                return TimeSpan.Parse(str);
            }

            if (value is null)
            {
                return default;
            }

            return XConvert<TimeSpan>.FromObject(value);
        }

        /// <summary>
        /// 读取一个可空值。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <returns>该类型的值或 Null</returns>
        public T? ReadNullable<T>() where T : struct
        {
            var value = DirectRead();

            if (value is T t)
            {
                return t;
            }

            if (value == null)
            {
                return null;
            }

            return XConvert<T?>.FromObject(value);
        }



        /// <summary>
        /// 写入一个布尔值。
        /// </summary>
        /// <param name="value"></param>
        public void WriteBoolean(bool value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteByte(byte value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteSByte(sbyte value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteInt16(short value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteChar(char value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteUInt16(ushort value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteInt32(int value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteSingle(float value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteUInt32(uint value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteInt64(long value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteDouble(double value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteUInt64(ulong value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteString(string value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteDateTime(DateTime value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteDecimal(decimal value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataReader"></param>
        public void WriteObject(IDataReader<string> dataReader)
        {
            DirectWrite(dataReader.Content);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataReader"></param>
        public void WriteArray(IDataReader<int> dataReader)
        {
            DirectWrite(dataReader.Content);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        public void WriteEnum<T>(T value) where T : struct, Enum
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteGuid(Guid value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteDateTimeOffset(DateTimeOffset value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public void WriteTimeSpan(TimeSpan value)
        {
            DirectWrite(value);
        }

        Guid IValueReader<Guid>.ReadValue() => ReadGuid();

        DateTimeOffset IValueReader<DateTimeOffset>.ReadValue() => ReadDateTimeOffset();

        TimeSpan IValueReader<TimeSpan>.ReadValue() => ReadTimeSpan();

        void IValueWriter<Guid>.WriteValue(Guid value)
        {
            throw new NotImplementedException();
        }

        void IValueWriter<DateTimeOffset>.WriteValue(DateTimeOffset value)
        {
            throw new NotImplementedException();
        }

        void IValueWriter<TimeSpan>.WriteValue(TimeSpan value)
        {
            throw new NotImplementedException();
        }
    }
}