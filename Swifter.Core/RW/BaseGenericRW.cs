using Swifter.Tools;
using System;

namespace Swifter.RW
{
    /// <summary>
    /// 提供一个通用值读写器的基本实现。
    /// </summary>
    /// <typeparam name="TValue">值的类型</typeparam>
    public abstract class BaseGenericRW<TValue> : IValueRW
    {
        /// <summary>
        /// 子类需要实现的读取方法。
        /// </summary>
        public abstract TValue? ReadValue();

        /// <summary>
        /// 子类需要实现的写入方法。
        /// </summary>
        public abstract void WriteValue(TValue? value);

        /// <inheritdoc/>
        public virtual void Pop()
        {
            ReadValue();
        }

        /// <summary>
        /// 获取值的类型。
        /// <see langword="null"/> 表示未知类型。
        /// </summary>
        public Type ValueType => typeof(TValue);

        /// <summary>
        /// 直接读取一个值。
        /// </summary>
        /// <returns></returns>
        public object? DirectRead() => ReadValue();

        /// <summary>
        /// 直接写入一个值。
        /// </summary>
        /// <param name="value"></param>
        public void DirectWrite(object? value) => WriteValue((TValue?)value);

        /// <summary>
        /// 读取一个数组。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        public virtual void ReadArray(IDataWriter<int> dataWriter)
        {
            if (dataWriter.ContentType is Type contentType && XConvert.IsEffectiveConvert(typeof(TValue), contentType))
            {
                dataWriter.Content = XConvert.Convert(ReadValue(), contentType);
            }
            else
            {
                ValueCopyer.ValueOf(ReadValue()).ReadArray(dataWriter);
            }
        }

        /// <summary>
        /// 读取一个对象。
        /// </summary>
        /// <param name="dataWriter">对象写入器</param>
        public virtual void ReadObject(IDataWriter<string> dataWriter)
        {
            if (dataWriter.ContentType is Type contentType && XConvert.IsEffectiveConvert(typeof(TValue), contentType))
            {
                dataWriter.Content = XConvert.Convert(ReadValue(), contentType);
            }
            else
            {
                ValueCopyer.ValueOf(ReadValue()).ReadObject(dataWriter);
            }
        }

        /// <summary>
        /// 写入一个对象。
        /// </summary>
        /// <param name="dataReader">对象读取器</param>
        public virtual void WriteObject(IDataReader<string> dataReader)
        {
            if (dataReader.ContentType is Type contentType && XConvert.IsEffectiveConvert(contentType, typeof(TValue)))
            {
                WriteValue(XConvert.Convert<TValue>(dataReader.Content));
            }
            else
            {
                var valueCopyer = new ValueCopyer();

                valueCopyer.WriteObject(dataReader);

                WriteValue(ValueInterface<TValue>.ReadValue(valueCopyer));
            }
        }

        /// <summary>
        /// 写入一个数组。
        /// </summary>
        /// <param name="dataReader">数组读取器</param>
        public virtual void WriteArray(IDataReader<int> dataReader)
        {
            if (dataReader.ContentType is Type contentType && XConvert.IsEffectiveConvert(contentType, typeof(TValue)))
            {
                WriteValue(XConvert.Convert<TValue>(dataReader.Content));
            }
            else
            {
                var valueCopyer = new ValueCopyer();

                valueCopyer.WriteArray(dataReader);

                WriteValue(ValueInterface<TValue>.ReadValue(valueCopyer));
            }
        }

        /// <summary>
        /// 读取一个布尔值。
        /// </summary>
        public bool ReadBoolean() => XConvert.Convert<TValue, bool>(ReadValue());

        /// <summary>
        /// 读取一个单字节无符号整数。
        /// </summary>
        public byte ReadByte() => XConvert.Convert<TValue, byte>(ReadValue());

        /// <summary>
        /// 读取一个字符。
        /// </summary>
        public char ReadChar() => XConvert.Convert<TValue, char>(ReadValue());

        /// <summary>
        /// 读取一个日期和时间。
        /// </summary>
        public DateTime ReadDateTime() => XConvert.Convert<TValue, DateTime>(ReadValue());

        /// <summary>
        /// 读取一个十进制数字。
        /// </summary>
        public decimal ReadDecimal() => XConvert.Convert<TValue, decimal>(ReadValue());

        /// <summary>
        /// 读取一个双精度浮点数。
        /// </summary>
        public double ReadDouble() => XConvert.Convert<TValue, double>(ReadValue());

        /// <summary>
        /// 读取一个双字节有符号整数。
        /// </summary>
        public short ReadInt16() => XConvert.Convert<TValue, short>(ReadValue());

        /// <summary>
        /// 读取一个四字节有符号整数。
        /// </summary>
        public int ReadInt32() => XConvert.Convert<TValue, int>(ReadValue());

        /// <summary>
        /// 读取一个八字节有符号整数。
        /// </summary>
        public long ReadInt64() => XConvert.Convert<TValue, long>(ReadValue());

        /// <summary>
        /// 读取一个单字节有符号整数。
        /// </summary>
        public sbyte ReadSByte() => XConvert.Convert<TValue, sbyte>(ReadValue());

        /// <summary>
        /// 读取一个单精度浮点数。
        /// </summary>
        public float ReadSingle() => XConvert.Convert<TValue, float>(ReadValue());

        /// <summary>
        /// 读取一个字符串。
        /// </summary>
        public string? ReadString() => XConvert.Convert<TValue, string>(ReadValue());

        /// <summary>
        /// 读取一个双字节无符号整数。
        /// </summary>
        public ushort ReadUInt16() => XConvert.Convert<TValue, ushort>(ReadValue());

        /// <summary>
        /// 读取一个四字节无符号整数。
        /// </summary>
        public uint ReadUInt32() => XConvert.Convert<TValue, uint>(ReadValue());

        /// <summary>
        /// 读取一个八字节无符号整数。
        /// </summary>
        public ulong ReadUInt64() => XConvert.Convert<TValue, ulong>(ReadValue());

        /// <summary>
        /// 读取一个枚举值。
        /// </summary>
        /// <typeparam name="T">枚举的类型</typeparam>
        public T ReadEnum<T>() where T : struct, Enum => XConvert.Convert<TValue, T>(ReadValue());

        /// <summary>
        /// 读取一个全局唯一标识符。
        /// </summary>
        public Guid ReadGuid() => XConvert.Convert<TValue, Guid>(ReadValue());

        /// <summary>
        /// 读取一个包含偏移量的日期和时间。
        /// </summary>
        public DateTimeOffset ReadDateTimeOffset() => XConvert.Convert<TValue, DateTimeOffset>(ReadValue());

        /// <summary>
        /// 读取一个时间间隔。
        /// </summary>
        public TimeSpan ReadTimeSpan() => XConvert.Convert<TValue, TimeSpan>(ReadValue());

        /// <summary>
        /// 读取一个可空值。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        public T? ReadNullable<T>() where T : struct => XConvert.Convert<TValue, T?>(ReadValue());

        /// <summary>
        /// 写入一个布尔值。
        /// </summary>
        public void WriteBoolean(bool value) => WriteValue(XConvert.Convert<bool, TValue>(value));

        /// <summary>
        /// 写入一个单字节无符号整数。
        /// </summary>
        public void WriteByte(byte value) => WriteValue(XConvert.Convert<byte, TValue>(value));

        /// <summary>
        /// 写入一个单字节有符号整数。
        /// </summary>
        public void WriteSByte(sbyte value) => WriteValue(XConvert.Convert<sbyte, TValue>(value));

        /// <summary>
        /// 写入一个双字节有符号整数。
        /// </summary>
        public void WriteInt16(short value) => WriteValue(XConvert.Convert<short, TValue>(value));

        /// <summary>
        /// 写入一个字符。
        /// </summary>
        public void WriteChar(char value) => WriteValue(XConvert.Convert<char, TValue>(value));

        /// <summary>
        /// 写入一个双字节无符号整数。
        /// </summary>
        public void WriteUInt16(ushort value) => WriteValue(XConvert.Convert<ushort, TValue>(value));

        /// <summary>
        /// 写入一个四字节有符号整数。
        /// </summary>
        public void WriteInt32(int value) => WriteValue(XConvert.Convert<int, TValue>(value));

        /// <summary>
        /// 写入一个单精度浮点数。
        /// </summary>
        public void WriteSingle(float value) => WriteValue(XConvert.Convert<float, TValue>(value));

        /// <summary>
        /// 写入一个四字节无符号整数。
        /// </summary>
        public void WriteUInt32(uint value) => WriteValue(XConvert.Convert<uint, TValue>(value));

        /// <summary>
        /// 写入一个八字节有符号整数。
        /// </summary>
        public void WriteInt64(long value) => WriteValue(XConvert.Convert<long, TValue>(value));

        /// <summary>
        /// 写入一个双精度浮点数。
        /// </summary>
        public void WriteDouble(double value) => WriteValue(XConvert.Convert<double, TValue>(value));

        /// <summary>
        /// 写入一个八字节无符号整数。
        /// </summary>
        public void WriteUInt64(ulong value) => WriteValue(XConvert.Convert<ulong, TValue>(value));

        /// <summary>
        /// 写入一个字符串。
        /// </summary>
        public void WriteString(string? value) => WriteValue(XConvert.Convert<string?, TValue>(value));

        /// <summary>
        /// 写入一个日期和时间。
        /// </summary>
        public void WriteDateTime(DateTime value) => WriteValue(XConvert.Convert<DateTime, TValue>(value));

        /// <summary>
        /// 读取一个十进制数字。
        /// </summary>
        public void WriteDecimal(decimal value) => WriteValue(XConvert.Convert<decimal, TValue>(value));

        /// <summary>
        /// 写入一个枚举值。
        /// </summary>
        /// <typeparam name="T">枚举的类型</typeparam>
        public void WriteEnum<T>(T value) where T : struct, Enum => WriteValue(XConvert.Convert<T, TValue>(value));

        /// <summary>
        /// 写入一个全局唯一标识符。
        /// </summary>
        public void WriteGuid(Guid value) => WriteValue(XConvert.Convert<Guid, TValue>(value));

        /// <summary>
        /// 写入一个包含偏移量的日期和时间。
        /// </summary>
        public void WriteDateTimeOffset(DateTimeOffset value) => WriteValue(XConvert.Convert<DateTimeOffset, TValue>(value));

        /// <summary>
        /// 写入一个时间间隔。
        /// </summary>
        public void WriteTimeSpan(TimeSpan value) => WriteValue(XConvert.Convert<TimeSpan, TValue>(value));
    }
}