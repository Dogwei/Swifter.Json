using Swifter.Tools;
using System;

namespace Swifter.RW
{
    /// <summary>
    /// 提供直接值读写器的基本实现。
    /// </summary>
    public abstract class BaseDirectRW : IValueRW
    {
        /// <summary>
        /// 子类需要实现的直接读取。
        /// </summary>
        /// <returns></returns>
        public abstract object? DirectRead();

        /// <summary>
        /// 子类需要实现的直接写入。
        /// </summary>
        /// <param name="value"></param>
        public abstract void DirectWrite(object? value);

        /// <inheritdoc/>
        public virtual void Pop()
        {
            DirectRead();
        }

        /// <summary>
        /// 获取值的类型。
        /// <see langword="null"/> 表示未知类型。
        /// </summary>
        public abstract Type? ValueType { get; }

        /// <summary>
        /// 读取一个数组。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        public virtual void ReadArray(IDataWriter<int> dataWriter)
        {
            if (ValueType is Type valueType && dataWriter.ContentType is Type contentType && XConvert.IsEffectiveConvert(valueType, contentType))
            {
                dataWriter.Content = XConvert.Convert(DirectRead(), contentType);
            }
            else
            {
                var valueCopyer = new ValueCopyer();

                valueCopyer.DirectWrite(DirectRead());

                valueCopyer.ReadArray(dataWriter);
            }
        }

        /// <summary>
        /// 读取一个对象。
        /// </summary>
        /// <param name="dataWriter">对象写入器</param>
        public virtual void ReadObject(IDataWriter<string> dataWriter)
        {
            if (ValueType is Type valueType && dataWriter.ContentType is Type contentType && XConvert.IsEffectiveConvert(valueType, contentType))
            {
                dataWriter.Content = XConvert.Convert(DirectRead(), contentType);
            }
            else
            {
                var valueCopyer = new ValueCopyer();

                valueCopyer.DirectWrite(DirectRead());

                valueCopyer.ReadObject(dataWriter);
            }
        }

        /// <summary>
        /// 写入一个对象。
        /// </summary>
        /// <param name="dataReader">对象读取器</param>
        public virtual void WriteObject(IDataReader<string> dataReader)
        {
            if (ValueType is Type valueType && dataReader.ContentType is Type contentType && XConvert.IsEffectiveConvert(contentType, valueType))
            {
                DirectWrite(XConvert.Convert(dataReader.Content, valueType));
            }
            else
            {
                var valueCopyer = new ValueCopyer();

                valueCopyer.WriteObject(dataReader);

                DirectWrite(valueCopyer.DirectRead());
            }
        }

        /// <summary>
        /// 写入一个数组。
        /// </summary>
        /// <param name="dataReader">数组读取器</param>
        public virtual void WriteArray(IDataReader<int> dataReader)
        {
            if (ValueType is Type valueType && dataReader.ContentType is Type contentType && XConvert.IsEffectiveConvert(contentType, valueType))
            {
                DirectWrite(XConvert.Convert(dataReader.Content, valueType));
            }
            else
            {
                var valueCopyer = new ValueCopyer();

                valueCopyer.WriteArray(dataReader);

                DirectWrite(valueCopyer.DirectRead());
            }
        }

        /// <summary>
        /// 读取一个布尔值。
        /// </summary>
        public bool ReadBoolean()
        {
            return Convert.ToBoolean(DirectRead());
        }

        /// <summary>
        /// 读取一个单字节无符号整数。
        /// </summary>
        public byte ReadByte()
        {
            return Convert.ToByte(DirectRead());
        }

        /// <summary>
        /// 读取一个字符。
        /// </summary>
        public char ReadChar()
        {
            return Convert.ToChar(DirectRead());
        }

        /// <summary>
        /// 读取一个日期和时间。
        /// </summary>
        public DateTime ReadDateTime()
        {
            return Convert.ToDateTime(DirectRead());
        }

        /// <summary>
        /// 读取一个十进制数字。
        /// </summary>
        public decimal ReadDecimal()
        {
            return Convert.ToDecimal(DirectRead());
        }

        /// <summary>
        /// 读取一个双精度浮点数。
        /// </summary>
        public double ReadDouble()
        {
            return Convert.ToDouble(DirectRead());
        }

        /// <summary>
        /// 读取一个双字节有符号整数。
        /// </summary>
        public short ReadInt16()
        {
            return Convert.ToInt16(DirectRead());
        }

        /// <summary>
        /// 读取一个四字节有符号整数。
        /// </summary>
        public int ReadInt32()
        {
            return Convert.ToInt32(DirectRead());
        }

        /// <summary>
        /// 读取一个八字节有符号整数。
        /// </summary>
        public long ReadInt64()
        {
            return Convert.ToInt64(DirectRead());
        }

        /// <summary>
        /// 读取一个单字节有符号整数。
        /// </summary>
        public sbyte ReadSByte()
        {
            return Convert.ToSByte(DirectRead());
        }

        /// <summary>
        /// 读取一个单精度浮点数。
        /// </summary>
        public float ReadSingle()
        {
            return Convert.ToSingle(DirectRead());
        }

        /// <summary>
        /// 读取一个字符串。
        /// </summary>
        public string? ReadString()
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
        public ushort ReadUInt16()
        {
            return Convert.ToUInt16(DirectRead());
        }

        /// <summary>
        /// 读取一个四字节无符号整数。
        /// </summary>
        public uint ReadUInt32()
        {
            return Convert.ToUInt32(DirectRead());
        }

        /// <summary>
        /// 读取一个八字节无符号整数。
        /// </summary>
        public ulong ReadUInt64()
        {
            return Convert.ToUInt64(DirectRead());
        }

        /// <summary>
        /// 读取一个枚举值。
        /// </summary>
        /// <typeparam name="T">枚举的类型</typeparam>
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

            return (T)Enum.ToObject(typeof(T), value);
        }

        /// <summary>
        /// 读取一个全局唯一标识符。
        /// </summary>
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

            return XConvert.Convert<Guid>(value);
        }

        /// <summary>
        /// 读取一个包含偏移量的日期和时间。
        /// </summary>
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

            return XConvert.Convert<DateTimeOffset>(value);
        }

        /// <summary>
        /// 读取一个时间间隔。
        /// </summary>
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

            return XConvert.Convert<TimeSpan>(value);
        }

        /// <summary>
        /// 读取一个可空值。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
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

            return XConvert.Convert<T?>(value);
        }

        /// <summary>
        /// 写入一个布尔值。
        /// </summary>
        public void WriteBoolean(bool value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个单字节无符号整数。
        /// </summary>
        public void WriteByte(byte value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个单字节有符号整数。
        /// </summary>
        public void WriteSByte(sbyte value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个双字节有符号整数。
        /// </summary>
        public void WriteInt16(short value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个字符。
        /// </summary>
        public void WriteChar(char value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个双字节无符号整数。
        /// </summary>
        public void WriteUInt16(ushort value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个四字节有符号整数。
        /// </summary>
        public void WriteInt32(int value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个单精度浮点数。
        /// </summary>
        public void WriteSingle(float value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个四字节无符号整数。
        /// </summary>
        public void WriteUInt32(uint value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个八字节有符号整数。
        /// </summary>
        public void WriteInt64(long value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个双精度浮点数。
        /// </summary>
        public void WriteDouble(double value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个八字节无符号整数。
        /// </summary>
        public void WriteUInt64(ulong value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个字符串。
        /// </summary>
        public void WriteString(string? value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个日期和时间。
        /// </summary>
        public void WriteDateTime(DateTime value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 读取一个十进制数字。
        /// </summary>
        public void WriteDecimal(decimal value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个枚举值。
        /// </summary>
        /// <typeparam name="T">枚举的类型</typeparam>
        public void WriteEnum<T>(T value) where T : struct, Enum
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个全局唯一标识符。
        /// </summary>
        public void WriteGuid(Guid value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个包含偏移量的日期和时间。
        /// </summary>
        public void WriteDateTimeOffset(DateTimeOffset value)
        {
            DirectWrite(value);
        }

        /// <summary>
        /// 写入一个时间间隔。
        /// </summary>
        public void WriteTimeSpan(TimeSpan value)
        {
            DirectWrite(value);
        }
    }
}