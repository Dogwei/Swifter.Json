
using Swifter.Tools;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// 值暂存器。
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    public sealed class ValueCopyer<TKey> : IValueRW
    {

        private readonly IDataRW<TKey> dataRW;

        private readonly TKey key;

        private readonly ValueCopyer valueCopyer;

        /// <summary>
        /// 初始化值暂存器。
        /// </summary>
        /// <param name="dataRW">数据读写器</param>
        /// <param name="key">键</param>
        public ValueCopyer(IDataRW<TKey> dataRW, TKey key)
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
        /// <typeparam name="T">值类型</typeparam>
        /// <returns>返回 Null 或该值类型的值</returns>
        public T? ReadNullable<T>() where T : struct
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadNullable<T>();
        }

        /// <summary>
        /// 读取一个枚举值。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>返回枚举值</returns>
        public T ReadEnum<T>() where T : struct, Enum
        {
            dataRW.OnReadValue(key, valueCopyer);
            return valueCopyer.ReadEnum<T>();
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

        /// <summary>
        /// 写入一个枚举值。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">枚举值</param>
        public void WriteEnum<T>(T value) where T : struct, Enum
        {
            valueCopyer.WriteEnum(value);
            dataRW.OnWriteValue(key, valueCopyer);
        }
    }

    /// <summary>
    /// 值暂存器。
    /// </summary>
    public sealed partial class ValueCopyer : IValueRW
    {
        /// <summary>
        /// 默认容量。
        /// </summary>
        const int DefaultCapacity = 3;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static void SetValue<T>(ValueCopyer valueCopyer, T value)
        {
            if (value is null || value is DBNull)
            {
                valueCopyer.code = ValueTypeCodes.Null;

                return;
            }

            var code = Type.GetTypeCode(typeof(T));

            switch (code)
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                case TypeCode.DateTime:

                    Underlying.As<OverlappedValue, T>(ref valueCopyer.value) = value;
                    valueCopyer.code = (ValueTypeCodes)code;

                    return;
            }

            valueCopyer.value.Object = value;
            valueCopyer.code = ValueTypeCodes.Direct;
        }

        /// <summary>
        /// 创建一个具有指定初始值的值暂存器。
        /// </summary>
        /// <typeparam name="T">初始值类型</typeparam>
        /// <param name="value">初始值</param>
        /// <returns>返回一个值暂存器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ValueCopyer ValueOf<T>(T value)
        {
            var valueCopyer = new ValueCopyer();

            SetValue(valueCopyer, value);

            return valueCopyer;
        }

        private OverlappedValue value;

        private ValueTypeCodes code;

        /// <summary>
        /// 初始化值暂存器。
        /// </summary>
        public ValueCopyer()
        {
            code = ValueTypeCodes.Null;
        }

        /// <summary>
        /// 获取值的 TypeCode。
        /// </summary>
        public TypeCode TypeCode => code < ValueTypeCodes.Direct ? (TypeCode)code : TypeCode.Object;

        /// <summary>
        /// 获取值暂存器的内部对象。
        /// </summary>
        public object InternalObject
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                return code switch
                {
                    ValueTypeCodes.Null => null,
                    ValueTypeCodes.Boolean => value.Boolean,
                    ValueTypeCodes.SByte => value.SByte,
                    ValueTypeCodes.Int16 => value.Int16,
                    ValueTypeCodes.Int32 => value.Int32,
                    ValueTypeCodes.Int64 => value.Int64,
                    ValueTypeCodes.Byte => value.Byte,
                    ValueTypeCodes.UInt16 => value.UInt16,
                    ValueTypeCodes.UInt32 => value.UInt32,
                    ValueTypeCodes.UInt64 => value.UInt64,
                    ValueTypeCodes.Single => value.Single,
                    ValueTypeCodes.Double => value.Double,
                    ValueTypeCodes.Decimal => value.Decimal,
                    ValueTypeCodes.Char => value.Char,
                    ValueTypeCodes.DateTime => value.DateTime,
                    ValueTypeCodes.String => value.String,

                    _ => value.Object,
                };
            }
        }

        private static bool TryStoreContent(object direct, IDataWriter dataWriter)
        {
            var destType = dataWriter.ContentType;
            var sourType = direct?.GetType();

            if (sourType != null && destType != null && (XConvert.IsImplicitConvert(sourType, destType) || XConvert.IsBasicConvert(sourType, destType)))
            {
                dataWriter.Content = XConvert.Cast(direct, destType);

                return true;
            }

            return false;
        }

        private static bool TryStoreContent(IDataReader dataReader, IDataWriter dataWriter)
        {
            var destType = dataWriter.ContentType;
            var sourType = dataReader.ContentType;

            if (sourType != null && destType != null && (XConvert.IsImplicitConvert(sourType, destType) || XConvert.IsBasicConvert(sourType, destType)))
            {
                dataWriter.Content = XConvert.Cast(dataReader.Content, destType);

                return true;
            }

            return false;
        }

        /// <summary>
        /// 读取一个数组结构数据。
        /// </summary>
        /// <param name="valueWriter">数据写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ReadArray(IDataWriter<int> valueWriter)
        {
            switch (code)
            {
                case ValueTypeCodes.Null:

                    valueWriter.Content = null;

                    return;

                case ValueTypeCodes.Array:
                case ValueTypeCodes.Object:

                    var dataReader = value.DataReader.As<int>();

                    RW:

                    if (TryStoreContent(dataReader, valueWriter))
                    {
                        return;
                    }

                    valueWriter.Initialize(Math.Max(dataReader.Count, DefaultCapacity));

                    dataReader.OnReadAll(valueWriter);

                    return;

                case ValueTypeCodes.Direct:

                    if (TryStoreContent(value.Object, valueWriter))
                    {
                        return;
                    }

                    if (RWHelper.CreateReader(value.Object, false) is var objDataReader)
                    {
                        dataReader = objDataReader.As<int>();

                        goto RW;
                    }

                    break;
            }

            valueWriter.Content = XConvert.Cast(InternalObject, valueWriter.ContentType);
        }

        /// <summary>
        /// 读取一个 Boolean 值。
        /// </summary>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool ReadBoolean()
        {
            return code switch
            {
                ValueTypeCodes.Null => default,
                ValueTypeCodes.Boolean => Convert.ToBoolean(value.Byte),
                ValueTypeCodes.SByte => Convert.ToBoolean(value.SByte),
                ValueTypeCodes.Int16 => Convert.ToBoolean(value.Int16),
                ValueTypeCodes.Int32 => Convert.ToBoolean(value.Int32),
                ValueTypeCodes.Int64 => Convert.ToBoolean(value.Int64),
                ValueTypeCodes.Byte => Convert.ToBoolean(value.Byte),
                ValueTypeCodes.UInt16 => Convert.ToBoolean(value.UInt16),
                ValueTypeCodes.UInt32 => Convert.ToBoolean(value.UInt32),
                ValueTypeCodes.UInt64 => Convert.ToBoolean(value.UInt64),
                ValueTypeCodes.Single => Convert.ToBoolean(value.Single),
                ValueTypeCodes.Double => Convert.ToBoolean(value.Double),
                ValueTypeCodes.Decimal => Convert.ToBoolean(value.Decimal),
                ValueTypeCodes.Char => Convert.ToBoolean(value.Char),
                ValueTypeCodes.DateTime => Convert.ToBoolean(value.DateTime),
                ValueTypeCodes.String => Convert.ToBoolean(value.String),
                ValueTypeCodes.Direct => Convert.ToBoolean(value.Object),
                _ => Convert.ToBoolean(value.DataReader.Content)
            };
        }

        /// <summary>
        /// 获取值是否为空。
        /// </summary>
        /// <returns>返回一个 bool 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool IsEmptyValue()
        {
            return code switch
            {
                ValueTypeCodes.Null => true,
                ValueTypeCodes.Boolean => value.Boolean == default,
                ValueTypeCodes.SByte => value.SByte == default,
                ValueTypeCodes.Int16 => value.Int16 == default,
                ValueTypeCodes.Int32 => value.Int32 == default,
                ValueTypeCodes.Int64 => value.Int64 == default,
                ValueTypeCodes.Byte => value.Byte == default,
                ValueTypeCodes.UInt16 => value.UInt16 == default,
                ValueTypeCodes.UInt32 => value.UInt32 == default,
                ValueTypeCodes.UInt64 => value.UInt64 == default,
                ValueTypeCodes.Single => value.Single == default,
                ValueTypeCodes.Double => value.Double == default,
                ValueTypeCodes.Decimal => value.Decimal == default,
                ValueTypeCodes.Char => value.Char == default,
                ValueTypeCodes.DateTime => value.DateTime == default,
                ValueTypeCodes.String => value.String == default,
                ValueTypeCodes.Direct => TypeHelper.IsEmptyValue(value.Object),
                _ => TypeHelper.IsEmptyValue(value.DataReader.Content),
            };
        }

        /// <summary>
        /// 读取一个 Byte 值。
        /// </summary>
        /// <returns>返回一个 byte 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public byte ReadByte()
        {
            return code switch
            {
                ValueTypeCodes.Null => default,
                ValueTypeCodes.Boolean => Convert.ToByte(value.Byte),
                ValueTypeCodes.SByte => Convert.ToByte(value.SByte),
                ValueTypeCodes.Int16 => Convert.ToByte(value.Int16),
                ValueTypeCodes.Int32 => Convert.ToByte(value.Int32),
                ValueTypeCodes.Int64 => Convert.ToByte(value.Int64),
                ValueTypeCodes.Byte => Convert.ToByte(value.Byte),
                ValueTypeCodes.UInt16 => Convert.ToByte(value.UInt16),
                ValueTypeCodes.UInt32 => Convert.ToByte(value.UInt32),
                ValueTypeCodes.UInt64 => Convert.ToByte(value.UInt64),
                ValueTypeCodes.Single => Convert.ToByte(value.Single),
                ValueTypeCodes.Double => Convert.ToByte(value.Double),
                ValueTypeCodes.Decimal => Convert.ToByte(value.Decimal),
                ValueTypeCodes.Char => Convert.ToByte(value.Char),
                ValueTypeCodes.DateTime => Convert.ToByte(value.DateTime),
                ValueTypeCodes.String => Convert.ToByte(value.String),
                ValueTypeCodes.Direct => Convert.ToByte(value.Object),
                _ => Convert.ToByte(value.DataReader.Content)
            };
        }

        /// <summary>
        /// 读取一个 Char 值。
        /// </summary>
        /// <returns>返回一个 char 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public char ReadChar()
        {
            return code switch
            {
                ValueTypeCodes.Null => default,
                ValueTypeCodes.Boolean => Convert.ToChar(value.Byte),
                ValueTypeCodes.SByte => Convert.ToChar(value.SByte),
                ValueTypeCodes.Int16 => Convert.ToChar(value.Int16),
                ValueTypeCodes.Int32 => Convert.ToChar(value.Int32),
                ValueTypeCodes.Int64 => Convert.ToChar(value.Int64),
                ValueTypeCodes.Byte => Convert.ToChar(value.Byte),
                ValueTypeCodes.UInt16 => Convert.ToChar(value.UInt16),
                ValueTypeCodes.UInt32 => Convert.ToChar(value.UInt32),
                ValueTypeCodes.UInt64 => Convert.ToChar(value.UInt64),
                ValueTypeCodes.Single => Convert.ToChar(value.Single),
                ValueTypeCodes.Double => Convert.ToChar(value.Double),
                ValueTypeCodes.Decimal => Convert.ToChar(value.Decimal),
                ValueTypeCodes.Char => Convert.ToChar(value.Char),
                ValueTypeCodes.DateTime => Convert.ToChar(value.DateTime),
                ValueTypeCodes.String => Convert.ToChar(value.String),
                ValueTypeCodes.Direct => Convert.ToChar(value.Object),
                _ => Convert.ToChar(value.DataReader.Content)
            };
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static InvalidCastException CreateInvalidCastException(ValueTypeCodes source, ValueTypeCodes destination)
        {
            return new InvalidCastException($"Can't convert {Enum.GetName(typeof(ValueTypeCodes), source)} to {Enum.GetName(typeof(ValueTypeCodes), destination)}.");
        }

        /// <summary>
        /// 读取一个 DateTime 值。
        /// </summary>
        /// <returns>返回一个 DateTime 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public DateTime ReadDateTime()
        {
            return code switch
            {
                ValueTypeCodes.Null => default,
                ValueTypeCodes.Boolean => Convert.ToDateTime(value.Byte),
                ValueTypeCodes.SByte => Convert.ToDateTime(value.SByte),
                ValueTypeCodes.Int16 => Convert.ToDateTime(value.Int16),
                ValueTypeCodes.Int32 => Convert.ToDateTime(value.Int32),
                ValueTypeCodes.Int64 => Convert.ToDateTime(value.Int64),
                ValueTypeCodes.Byte => Convert.ToDateTime(value.Byte),
                ValueTypeCodes.UInt16 => Convert.ToDateTime(value.UInt16),
                ValueTypeCodes.UInt32 => Convert.ToDateTime(value.UInt32),
                ValueTypeCodes.UInt64 => Convert.ToDateTime(value.UInt64),
                ValueTypeCodes.Single => Convert.ToDateTime(value.Single),
                ValueTypeCodes.Double => Convert.ToDateTime(value.Double),
                ValueTypeCodes.Decimal => Convert.ToDateTime(value.Decimal),
                ValueTypeCodes.Char => Convert.ToDateTime(value.Char),
                ValueTypeCodes.DateTime => Convert.ToDateTime(value.DateTime),
                ValueTypeCodes.String => Convert.ToDateTime(value.String),
                ValueTypeCodes.Direct => Convert.ToDateTime(value.Object),
                _ => Convert.ToDateTime(value.DataReader.Content)
            };
        }

        /// <summary>
        /// 读取一个 Decimal 值。
        /// </summary>
        /// <returns>返回一个 decimal 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public decimal ReadDecimal()
        {
            return code switch
            {
                ValueTypeCodes.Null => default,
                ValueTypeCodes.Boolean => Convert.ToDecimal(value.Byte),
                ValueTypeCodes.SByte => Convert.ToDecimal(value.SByte),
                ValueTypeCodes.Int16 => Convert.ToDecimal(value.Int16),
                ValueTypeCodes.Int32 => Convert.ToDecimal(value.Int32),
                ValueTypeCodes.Int64 => Convert.ToDecimal(value.Int64),
                ValueTypeCodes.Byte => Convert.ToDecimal(value.Byte),
                ValueTypeCodes.UInt16 => Convert.ToDecimal(value.UInt16),
                ValueTypeCodes.UInt32 => Convert.ToDecimal(value.UInt32),
                ValueTypeCodes.UInt64 => Convert.ToDecimal(value.UInt64),
                ValueTypeCodes.Single => Convert.ToDecimal(value.Single),
                ValueTypeCodes.Double => Convert.ToDecimal(value.Double),
                ValueTypeCodes.Decimal => Convert.ToDecimal(value.Decimal),
                ValueTypeCodes.Char => Convert.ToDecimal(value.Char),
                ValueTypeCodes.DateTime => Convert.ToDecimal(value.DateTime),
                ValueTypeCodes.String => Convert.ToDecimal(value.String),
                ValueTypeCodes.Direct => Convert.ToDecimal(value.Object),
                _ => Convert.ToDecimal(value.DataReader.Content)
            };
        }

        /// <summary>
        /// 读取一个未知类型的值。
        /// </summary>
        /// <returns>返回一个未知类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object DirectRead()
        {
            return code switch
            {
                ValueTypeCodes.Null => default,
                ValueTypeCodes.Boolean => value.Boolean,
                ValueTypeCodes.SByte => value.SByte,
                ValueTypeCodes.Int16 => value.Int16,
                ValueTypeCodes.Int32 => value.Int32,
                ValueTypeCodes.Int64 => value.Int64,
                ValueTypeCodes.Byte => value.Byte,
                ValueTypeCodes.UInt16 => value.UInt16,
                ValueTypeCodes.UInt32 => value.UInt32,
                ValueTypeCodes.UInt64 => value.UInt64,
                ValueTypeCodes.Single => value.Single,
                ValueTypeCodes.Double => value.Double,
                ValueTypeCodes.Decimal => value.Decimal,
                ValueTypeCodes.Char => value.Char,
                ValueTypeCodes.DateTime => value.DateTime,
                ValueTypeCodes.String => value.String,
                ValueTypeCodes.Direct => value.Object,
                _ => ReadFromRW(),
            };

            object ReadFromRW()
            {
                if (value.DataReader.ContentType != null)
                {
                    return value.DataReader.Content;
                }

                //if ( is IDataReader<int>)
                //{
                //    return this.ReadList<object>();
                //}

                //if (value.DataReader is IDataReader<string>)
                //{
                //    return this.ReadDictionary<string, object>();
                //}

                //return this.ReadDictionary<object, object>();

                return value.DataReader;
            }
        }

        /// <summary>
        /// 读取一个 Double 值。
        /// </summary>
        /// <returns>返回一个 double 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public double ReadDouble()
        {
            return code switch
            {
                ValueTypeCodes.Null => default,
                ValueTypeCodes.Boolean => Convert.ToDouble(value.Byte),
                ValueTypeCodes.SByte => Convert.ToDouble(value.SByte),
                ValueTypeCodes.Int16 => Convert.ToDouble(value.Int16),
                ValueTypeCodes.Int32 => Convert.ToDouble(value.Int32),
                ValueTypeCodes.Int64 => Convert.ToDouble(value.Int64),
                ValueTypeCodes.Byte => Convert.ToDouble(value.Byte),
                ValueTypeCodes.UInt16 => Convert.ToDouble(value.UInt16),
                ValueTypeCodes.UInt32 => Convert.ToDouble(value.UInt32),
                ValueTypeCodes.UInt64 => Convert.ToDouble(value.UInt64),
                ValueTypeCodes.Single => Convert.ToDouble(value.Single),
                ValueTypeCodes.Double => Convert.ToDouble(value.Double),
                ValueTypeCodes.Decimal => Convert.ToDouble(value.Decimal),
                ValueTypeCodes.Char => Convert.ToDouble(value.Char),
                ValueTypeCodes.DateTime => Convert.ToDouble(value.DateTime),
                ValueTypeCodes.String => Convert.ToDouble(value.String),
                ValueTypeCodes.Direct => Convert.ToDouble(value.Object),
                _ => Convert.ToDouble(value.DataReader.Content)
            };
        }

        /// <summary>
        /// 读取一个 Int16 值。
        /// </summary>
        /// <returns>返回一个 short 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public short ReadInt16()
        {
            return code switch
            {
                ValueTypeCodes.Null => default,
                ValueTypeCodes.Boolean => Convert.ToInt16(value.Byte),
                ValueTypeCodes.SByte => Convert.ToInt16(value.SByte),
                ValueTypeCodes.Int16 => Convert.ToInt16(value.Int16),
                ValueTypeCodes.Int32 => Convert.ToInt16(value.Int32),
                ValueTypeCodes.Int64 => Convert.ToInt16(value.Int64),
                ValueTypeCodes.Byte => Convert.ToInt16(value.Byte),
                ValueTypeCodes.UInt16 => Convert.ToInt16(value.UInt16),
                ValueTypeCodes.UInt32 => Convert.ToInt16(value.UInt32),
                ValueTypeCodes.UInt64 => Convert.ToInt16(value.UInt64),
                ValueTypeCodes.Single => Convert.ToInt16(value.Single),
                ValueTypeCodes.Double => Convert.ToInt16(value.Double),
                ValueTypeCodes.Decimal => Convert.ToInt16(value.Decimal),
                ValueTypeCodes.Char => Convert.ToInt16(value.Char),
                ValueTypeCodes.DateTime => Convert.ToInt16(value.DateTime),
                ValueTypeCodes.String => Convert.ToInt16(value.String),
                ValueTypeCodes.Direct => Convert.ToInt16(value.Object),
                _ => Convert.ToInt16(value.DataReader.Content)
            };
        }

        /// <summary>
        /// 读取一个 Int32 值。
        /// </summary>
        /// <returns>返回一个 int 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ReadInt32()
        {
            return code switch
            {
                ValueTypeCodes.Null => default,
                ValueTypeCodes.Boolean => Convert.ToInt32(value.Byte),
                ValueTypeCodes.SByte => Convert.ToInt32(value.SByte),
                ValueTypeCodes.Int16 => Convert.ToInt32(value.Int16),
                ValueTypeCodes.Int32 => Convert.ToInt32(value.Int32),
                ValueTypeCodes.Int64 => Convert.ToInt32(value.Int64),
                ValueTypeCodes.Byte => Convert.ToInt32(value.Byte),
                ValueTypeCodes.UInt16 => Convert.ToInt32(value.UInt16),
                ValueTypeCodes.UInt32 => Convert.ToInt32(value.UInt32),
                ValueTypeCodes.UInt64 => Convert.ToInt32(value.UInt64),
                ValueTypeCodes.Single => Convert.ToInt32(value.Single),
                ValueTypeCodes.Double => Convert.ToInt32(value.Double),
                ValueTypeCodes.Decimal => Convert.ToInt32(value.Decimal),
                ValueTypeCodes.Char => Convert.ToInt32(value.Char),
                ValueTypeCodes.DateTime => Convert.ToInt32(value.DateTime),
                ValueTypeCodes.String => Convert.ToInt32(value.String),
                ValueTypeCodes.Direct => Convert.ToInt32(value.Object),
                _ => Convert.ToInt32(value.DataReader.Content)
            };
        }

        /// <summary>
        /// 读取一个 Int64 值。
        /// </summary>
        /// <returns>返回一个 long 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ReadInt64()
        {
            return code switch
            {
                ValueTypeCodes.Null => default,
                ValueTypeCodes.Boolean => Convert.ToInt64(value.Byte),
                ValueTypeCodes.SByte => Convert.ToInt64(value.SByte),
                ValueTypeCodes.Int16 => Convert.ToInt64(value.Int16),
                ValueTypeCodes.Int32 => Convert.ToInt64(value.Int32),
                ValueTypeCodes.Int64 => Convert.ToInt64(value.Int64),
                ValueTypeCodes.Byte => Convert.ToInt64(value.Byte),
                ValueTypeCodes.UInt16 => Convert.ToInt64(value.UInt16),
                ValueTypeCodes.UInt32 => Convert.ToInt64(value.UInt32),
                ValueTypeCodes.UInt64 => Convert.ToInt64(value.UInt64),
                ValueTypeCodes.Single => Convert.ToInt64(value.Single),
                ValueTypeCodes.Double => Convert.ToInt64(value.Double),
                ValueTypeCodes.Decimal => Convert.ToInt64(value.Decimal),
                ValueTypeCodes.Char => Convert.ToInt64(value.Char),
                ValueTypeCodes.DateTime => Convert.ToInt64(value.DateTime),
                ValueTypeCodes.String => Convert.ToInt64(value.String),
                ValueTypeCodes.Direct => Convert.ToInt64(value.Object),
                _ => Convert.ToInt64(value.DataReader.Content)
            };
        }

        /// <summary>
        /// 读取一个对象结构数据。
        /// </summary>
        /// <param name="valueWriter">数据写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ReadObject(IDataWriter<string> valueWriter)
        {
            switch (code)
            {
                case ValueTypeCodes.Null:

                    valueWriter.Content = null;

                    return;

                case ValueTypeCodes.Array:
                case ValueTypeCodes.Object:

                    var dataReader = value.DataReader.As<string>();

                    RW:

                    if (TryStoreContent(dataReader, valueWriter))
                    {
                        return;
                    }

                    valueWriter.Initialize(Math.Max(dataReader.Count, DefaultCapacity));

                    dataReader.OnReadAll(valueWriter);

                    return;

                case ValueTypeCodes.Direct:

                    if (TryStoreContent(value.Object, valueWriter))
                    {
                        return;
                    }

                    if (RWHelper.CreateReader(value.Object) is var objDataReader)
                    {
                        dataReader = objDataReader.As<string>();

                        goto RW;
                    }

                    break;
            }

            valueWriter.Content = XConvert.Cast(InternalObject, valueWriter.ContentType);
        }

        /// <summary>
        /// 读取一个 SByte 值。
        /// </summary>
        /// <returns>返回一个 sbyte 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public sbyte ReadSByte()
        {
            return code switch
            {
                ValueTypeCodes.Null => default,
                ValueTypeCodes.Boolean => Convert.ToSByte(value.Byte),
                ValueTypeCodes.SByte => Convert.ToSByte(value.SByte),
                ValueTypeCodes.Int16 => Convert.ToSByte(value.Int16),
                ValueTypeCodes.Int32 => Convert.ToSByte(value.Int32),
                ValueTypeCodes.Int64 => Convert.ToSByte(value.Int64),
                ValueTypeCodes.Byte => Convert.ToSByte(value.Byte),
                ValueTypeCodes.UInt16 => Convert.ToSByte(value.UInt16),
                ValueTypeCodes.UInt32 => Convert.ToSByte(value.UInt32),
                ValueTypeCodes.UInt64 => Convert.ToSByte(value.UInt64),
                ValueTypeCodes.Single => Convert.ToSByte(value.Single),
                ValueTypeCodes.Double => Convert.ToSByte(value.Double),
                ValueTypeCodes.Decimal => Convert.ToSByte(value.Decimal),
                ValueTypeCodes.Char => Convert.ToSByte(value.Char),
                ValueTypeCodes.DateTime => Convert.ToSByte(value.DateTime),
                ValueTypeCodes.String => Convert.ToSByte(value.String),
                ValueTypeCodes.Direct => Convert.ToSByte(value.Object),
                _ => Convert.ToSByte(value.DataReader.Content)
            };
        }

        /// <summary>
        /// 读取一个 Single 值。
        /// </summary>
        /// <returns>返回一个 flaot 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public float ReadSingle()
        {
            return code switch
            {
                ValueTypeCodes.Null => default,
                ValueTypeCodes.Boolean => Convert.ToSingle(value.Byte),
                ValueTypeCodes.SByte => Convert.ToSingle(value.SByte),
                ValueTypeCodes.Int16 => Convert.ToSingle(value.Int16),
                ValueTypeCodes.Int32 => Convert.ToSingle(value.Int32),
                ValueTypeCodes.Int64 => Convert.ToSingle(value.Int64),
                ValueTypeCodes.Byte => Convert.ToSingle(value.Byte),
                ValueTypeCodes.UInt16 => Convert.ToSingle(value.UInt16),
                ValueTypeCodes.UInt32 => Convert.ToSingle(value.UInt32),
                ValueTypeCodes.UInt64 => Convert.ToSingle(value.UInt64),
                ValueTypeCodes.Single => Convert.ToSingle(value.Single),
                ValueTypeCodes.Double => Convert.ToSingle(value.Double),
                ValueTypeCodes.Decimal => Convert.ToSingle(value.Decimal),
                ValueTypeCodes.Char => Convert.ToSingle(value.Char),
                ValueTypeCodes.DateTime => Convert.ToSingle(value.DateTime),
                ValueTypeCodes.String => Convert.ToSingle(value.String),
                ValueTypeCodes.Direct => Convert.ToSingle(value.Object),
                _ => Convert.ToSingle(value.DataReader.Content)
            };
        }

        /// <summary>
        /// 读取一个 String 值。
        /// </summary>
        /// <returns>返回一个 string 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string ReadString()
        {
            return code switch
            {
                ValueTypeCodes.Null => default,
                ValueTypeCodes.Boolean => Convert.ToString(value.Byte),
                ValueTypeCodes.SByte => Convert.ToString(value.SByte),
                ValueTypeCodes.Int16 => Convert.ToString(value.Int16),
                ValueTypeCodes.Int32 => Convert.ToString(value.Int32),
                ValueTypeCodes.Int64 => Convert.ToString(value.Int64),
                ValueTypeCodes.Byte => Convert.ToString(value.Byte),
                ValueTypeCodes.UInt16 => Convert.ToString(value.UInt16),
                ValueTypeCodes.UInt32 => Convert.ToString(value.UInt32),
                ValueTypeCodes.UInt64 => Convert.ToString(value.UInt64),
                ValueTypeCodes.Single => Convert.ToString(value.Single),
                ValueTypeCodes.Double => Convert.ToString(value.Double),
                ValueTypeCodes.Decimal => Convert.ToString(value.Decimal),
                ValueTypeCodes.Char => Convert.ToString(value.Char),
                ValueTypeCodes.DateTime => Convert.ToString(value.DateTime),
                ValueTypeCodes.String => Convert.ToString(value.String),
                ValueTypeCodes.Direct => Convert.ToString(value.Object),
                _ => Convert.ToString(value.DataReader.Content)
            };
        }

        /// <summary>
        /// 读取一个 UInt16 值。
        /// </summary>
        /// <returns>返回一个 ushort 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            return code switch
            {
                ValueTypeCodes.Null => default,
                ValueTypeCodes.Boolean => Convert.ToUInt16(value.Byte),
                ValueTypeCodes.SByte => Convert.ToUInt16(value.SByte),
                ValueTypeCodes.Int16 => Convert.ToUInt16(value.Int16),
                ValueTypeCodes.Int32 => Convert.ToUInt16(value.Int32),
                ValueTypeCodes.Int64 => Convert.ToUInt16(value.Int64),
                ValueTypeCodes.Byte => Convert.ToUInt16(value.Byte),
                ValueTypeCodes.UInt16 => Convert.ToUInt16(value.UInt16),
                ValueTypeCodes.UInt32 => Convert.ToUInt16(value.UInt32),
                ValueTypeCodes.UInt64 => Convert.ToUInt16(value.UInt64),
                ValueTypeCodes.Single => Convert.ToUInt16(value.Single),
                ValueTypeCodes.Double => Convert.ToUInt16(value.Double),
                ValueTypeCodes.Decimal => Convert.ToUInt16(value.Decimal),
                ValueTypeCodes.Char => Convert.ToUInt16(value.Char),
                ValueTypeCodes.DateTime => Convert.ToUInt16(value.DateTime),
                ValueTypeCodes.String => Convert.ToUInt16(value.String),
                ValueTypeCodes.Direct => Convert.ToUInt16(value.Object),
                _ => Convert.ToUInt16(value.DataReader.Content)
            };
        }

        /// <summary>
        /// 读取一个 UInt32 值。
        /// </summary>
        /// <returns>返回一个 uint 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public uint ReadUInt32()
        {
            return code switch
            {
                ValueTypeCodes.Null => default,
                ValueTypeCodes.Boolean => Convert.ToUInt32(value.Byte),
                ValueTypeCodes.SByte => Convert.ToUInt32(value.SByte),
                ValueTypeCodes.Int16 => Convert.ToUInt32(value.Int16),
                ValueTypeCodes.Int32 => Convert.ToUInt32(value.Int32),
                ValueTypeCodes.Int64 => Convert.ToUInt32(value.Int64),
                ValueTypeCodes.Byte => Convert.ToUInt32(value.Byte),
                ValueTypeCodes.UInt16 => Convert.ToUInt32(value.UInt16),
                ValueTypeCodes.UInt32 => Convert.ToUInt32(value.UInt32),
                ValueTypeCodes.UInt64 => Convert.ToUInt32(value.UInt64),
                ValueTypeCodes.Single => Convert.ToUInt32(value.Single),
                ValueTypeCodes.Double => Convert.ToUInt32(value.Double),
                ValueTypeCodes.Decimal => Convert.ToUInt32(value.Decimal),
                ValueTypeCodes.Char => Convert.ToUInt32(value.Char),
                ValueTypeCodes.DateTime => Convert.ToUInt32(value.DateTime),
                ValueTypeCodes.String => Convert.ToUInt32(value.String),
                ValueTypeCodes.Direct => Convert.ToUInt32(value.Object),
                _ => Convert.ToUInt32(value.DataReader.Content)
            };
        }

        /// <summary>
        /// 读取一个 UInt64 值。
        /// </summary>
        /// <returns>返回一个 ulong 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            return code switch
            {
                ValueTypeCodes.Null => default,
                ValueTypeCodes.Boolean => Convert.ToUInt64(value.Byte),
                ValueTypeCodes.SByte => Convert.ToUInt64(value.SByte),
                ValueTypeCodes.Int16 => Convert.ToUInt64(value.Int16),
                ValueTypeCodes.Int32 => Convert.ToUInt64(value.Int32),
                ValueTypeCodes.Int64 => Convert.ToUInt64(value.Int64),
                ValueTypeCodes.Byte => Convert.ToUInt64(value.Byte),
                ValueTypeCodes.UInt16 => Convert.ToUInt64(value.UInt16),
                ValueTypeCodes.UInt32 => Convert.ToUInt64(value.UInt32),
                ValueTypeCodes.UInt64 => Convert.ToUInt64(value.UInt64),
                ValueTypeCodes.Single => Convert.ToUInt64(value.Single),
                ValueTypeCodes.Double => Convert.ToUInt64(value.Double),
                ValueTypeCodes.Decimal => Convert.ToUInt64(value.Decimal),
                ValueTypeCodes.Char => Convert.ToUInt64(value.Char),
                ValueTypeCodes.DateTime => Convert.ToUInt64(value.DateTime),
                ValueTypeCodes.String => Convert.ToUInt64(value.String),
                ValueTypeCodes.Direct => Convert.ToUInt64(value.Object),
                _ => Convert.ToUInt64(value.DataReader.Content)
            };
        }

        /// <summary>
        /// 读取一个枚举值。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>返回枚举值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T ReadEnum<T>() where T : struct, Enum
        {
            switch (code)
            {
                case ValueTypeCodes.SByte:
                case ValueTypeCodes.Int16:
                case ValueTypeCodes.Int32:
                case ValueTypeCodes.Int64:
                case ValueTypeCodes.Byte:
                case ValueTypeCodes.UInt16:
                case ValueTypeCodes.UInt32:
                case ValueTypeCodes.UInt64:
                    return EnumHelper.AsEnum<T>(value.UInt64);
                case ValueTypeCodes.String:

                    unsafe
                    {
                        fixed(char* ptr = value.String)
                        {
                            if (EnumHelper.TryParseEnum(new Ps<char>(ptr, value.String.Length), out T ret))
                            {
                                return ret;
                            }
                        }
                    }

                    return (T)Enum.Parse(typeof(T), value.String);
                case ValueTypeCodes.Null:
                    return default;
                default:
                    return (T)DirectRead();
            }
        }

        /// <summary>
        /// 读取一个可空类型的值。
        /// </summary>
        /// <typeparam name="T">值类型</typeparam>
        /// <returns>返回 Null 或该值类型的值</returns>
        public T? ReadNullable<T>() where T : struct
        {
            if (code == ValueTypeCodes.Null)
            {
                return null;
            }

            return ValueInterface<T>.ReadValue(this);
        }

        /// <summary>
        /// 将值写入到值写入器中。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteTo(IValueWriter valueWriter)
        {
            switch (code)
            {
                case ValueTypeCodes.Null:
                    valueWriter.DirectWrite(null);
                    break;
                case ValueTypeCodes.Boolean:
                    valueWriter.WriteBoolean(value.Boolean);
                    break;
                case ValueTypeCodes.SByte:
                    valueWriter.WriteSByte(value.SByte);
                    break;
                case ValueTypeCodes.Int16:
                    valueWriter.WriteInt16(value.Int16);
                    break;
                case ValueTypeCodes.Int32:
                    valueWriter.WriteInt32(value.Int32);
                    break;
                case ValueTypeCodes.Int64:
                    valueWriter.WriteInt64(value.Int64);
                    break;
                case ValueTypeCodes.Byte:
                    valueWriter.WriteByte(value.Byte);
                    break;
                case ValueTypeCodes.UInt16:
                    valueWriter.WriteUInt16(value.UInt16);
                    break;
                case ValueTypeCodes.UInt32:
                    valueWriter.WriteUInt32(value.UInt32);
                    break;
                case ValueTypeCodes.UInt64:
                    valueWriter.WriteUInt64(value.UInt64);
                    break;
                case ValueTypeCodes.Single:
                    valueWriter.WriteSingle(value.Single);
                    break;
                case ValueTypeCodes.Double:
                    valueWriter.WriteDouble(value.Double);
                    break;
                case ValueTypeCodes.Decimal:
                    valueWriter.WriteDecimal(value.Decimal);
                    break;
                case ValueTypeCodes.Char:
                    valueWriter.WriteChar(value.Char);
                    break;
                case ValueTypeCodes.DateTime:
                    valueWriter.WriteDateTime(value.DateTime);
                    break;
                case ValueTypeCodes.String:
                    valueWriter.WriteString(value.String);
                    break;
                case ValueTypeCodes.Direct:
                    valueWriter.DirectWrite(value.Object);
                    break;
                case ValueTypeCodes.Array:
                    valueWriter.WriteArray(value.DataReader.As<int>());
                    break;
                case ValueTypeCodes.Object:
                    valueWriter.WriteObject(value.DataReader.As<string>());
                    break;
            }
        }

        /// <summary>
        /// 写入一个数组结构数据。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteArray(IDataReader<int> dataReader)
        {
            code = ValueTypeCodes.Array;

            value.DataReader = dataReader;
        }

        /// <summary>
        /// 写入一个 Boolean 值。
        /// </summary>
        /// <param name="value">bool 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteBoolean(bool value)
        {
            code = ValueTypeCodes.Boolean;

            this.value.Boolean = value;
        }

        /// <summary>
        /// 写入一个 Byte 值。
        /// </summary>
        /// <param name="value">byte 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            code = ValueTypeCodes.Byte;

            this.value.Byte = value;
        }

        /// <summary>
        /// 写入一个 Char 值。
        /// </summary>
        /// <param name="value">char 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteChar(char value)
        {
            code = ValueTypeCodes.Char;

            this.value.Char = value;
        }

        /// <summary>
        /// 写入一个 DateTime 值。
        /// </summary>
        /// <param name="value">DateTime 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteDateTime(DateTime value)
        {
            code = ValueTypeCodes.DateTime;

            this.value.DateTime = value;
        }

        /// <summary>
        /// 写入一个 Decimal 值。
        /// </summary>
        /// <param name="value">decimal 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteDecimal(decimal value)
        {
            code = ValueTypeCodes.Decimal;

            this.value.Decimal = value;
        }

        /// <summary>
        /// 写入一个未知类型的值。
        /// </summary>
        /// <param name="value">未知类型的值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void DirectWrite(object value)
        {
            if (value is null)
            {
                code = ValueTypeCodes.Null;

                this.value.Object = null;

                return;
            }

            code = ValueTypeCodes.Direct;

            this.value.Object = value;
        }

        /// <summary>
        /// 写入一个 Double 值。
        /// </summary>
        /// <param name="value">double 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteDouble(double value)
        {
            code = ValueTypeCodes.Double;

            this.value.Double = value;
        }

        /// <summary>
        /// 写入一个 Int16 值。
        /// </summary>
        /// <param name="value">short 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt16(short value)
        {
            code = ValueTypeCodes.Int16;

            this.value.Int16 = value;
        }

        /// <summary>
        /// 写入一个 Int32 值。
        /// </summary>
        /// <param name="value">int 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt32(int value)
        {
            code = ValueTypeCodes.Int32;

            this.value.Int32 = value;
        }

        /// <summary>
        /// 写入一个 Int64 值。
        /// </summary>
        /// <param name="value">long 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt64(long value)
        {
            code = ValueTypeCodes.Int64;

            this.value.Int64 = value;
        }

        /// <summary>
        /// 写入一个对象结构数据。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteObject(IDataReader<string> dataReader)
        {
            code = ValueTypeCodes.Object;

            value.DataReader = dataReader;
        }

        /// <summary>
        /// 写入一个 SByte 值。
        /// </summary>
        /// <param name="value">sbyte 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteSByte(sbyte value)
        {
            code = ValueTypeCodes.SByte;

            this.value.SByte = value;
        }

        /// <summary>
        /// 写入一个 Single 值。
        /// </summary>
        /// <param name="value">float 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteSingle(float value)
        {
            code = ValueTypeCodes.Single;

            this.value.Single = value;
        }

        /// <summary>
        /// 写入一个 String 值。
        /// </summary>
        /// <param name="value">string 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteString(string value)
        {
            if (value is null)
            {
                code = ValueTypeCodes.Null;

                this.value.String = null;

                return;
            }

            code = ValueTypeCodes.String;

            this.value.String = value;
        }

        /// <summary>
        /// 写入一个 UInt16 值。
        /// </summary>
        /// <param name="value">ushort 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt16(ushort value)
        {
            code = ValueTypeCodes.UInt16;

            this.value.UInt16 = value;
        }

        /// <summary>
        /// 写入一个 UInt32 值。
        /// </summary>
        /// <param name="value">uint 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt32(uint value)
        {
            code = ValueTypeCodes.UInt32;

            this.value.UInt32 = value;
        }

        /// <summary>
        /// 写入一个 UInt64 值。
        /// </summary>
        /// <param name="value">ulong 值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt64(ulong value)
        {
            code = ValueTypeCodes.UInt64;

            this.value.UInt64 = value;
        }

        /// <summary>
        /// 写入一个枚举值。
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <param name="value">枚举值</param>
        public void WriteEnum<T>(T value) where T : struct, Enum
        {
            code = (ValueTypeCodes)EnumInterface<T>.TypeCode;

            this.value.Int64 = 0;

            Underlying.As<long, T>(ref this.value.Int64) = value;
        }


        sealed class ValueInterface : IValueInterface<ValueCopyer>
        {
            public ValueCopyer ReadValue(IValueReader valueReader)
            {
                if (valueReader is IValueReader<ValueCopyer> reader)
                {
                    return reader.ReadValue();
                }

                var valueCopyer = new ValueCopyer();
                var value = valueReader.DirectRead();

                RW.ValueInterface.WriteValue(valueCopyer, value);

                return valueCopyer;
            }

            public void WriteValue(IValueWriter valueWriter, ValueCopyer value)
            {
                value.WriteTo(valueWriter);
            }
        }
    }
}