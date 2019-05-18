using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
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

    /// <summary>
    /// 值暂存器。
    /// </summary>
    public sealed class ValueCopyer : IValueRW
    {
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
        /// 获取值暂存器的值。
        /// </summary>
        public object Value
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                switch (code)
                {
                    case ValueTypeCodes.Null:
                        return null;
                    case ValueTypeCodes.Boolean:
                        return value.Boolean;
                    case ValueTypeCodes.SByte:
                        return value.SByte;
                    case ValueTypeCodes.Int16:
                        return value.Int16;
                    case ValueTypeCodes.Int32:
                        return value.Int32;
                    case ValueTypeCodes.Int64:
                        return value.Int64;
                    case ValueTypeCodes.Byte:
                        return value.Byte;
                    case ValueTypeCodes.UInt16:
                        return value.UInt16;
                    case ValueTypeCodes.UInt32:
                        return value.UInt32;
                    case ValueTypeCodes.UInt64:
                        return value.UInt64;
                    case ValueTypeCodes.Single:
                        return value.Single;
                    case ValueTypeCodes.Double:
                        return value.Double;
                    case ValueTypeCodes.Decimal:
                        return value.Decimal;
                    case ValueTypeCodes.Char:
                        return value.Char;
                    case ValueTypeCodes.DateTime:
                        return value.DateTime;
                    case ValueTypeCodes.String:
                        return value.String;
                }

                return value.Object;
            }
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
                    return;
                case ValueTypeCodes.Boolean:
                    throw new InvalidCastException("Can't convert Boolean to Array.");
                case ValueTypeCodes.SByte:
                case ValueTypeCodes.Int16:
                case ValueTypeCodes.Int32:
                case ValueTypeCodes.Int64:
                case ValueTypeCodes.Byte:
                case ValueTypeCodes.UInt16:
                case ValueTypeCodes.UInt32:
                case ValueTypeCodes.UInt64:
                case ValueTypeCodes.Single:
                case ValueTypeCodes.Double:
                case ValueTypeCodes.Decimal:
                    throw new InvalidCastException("Can't convert Number to Array.");
                case ValueTypeCodes.Char:
                    throw new InvalidCastException("Can't convert Char to Array.");
                case ValueTypeCodes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Array.");
                case ValueTypeCodes.String:
                    throw new InvalidCastException("Can't convert String to Array.");
            }

            IDataReader<int> dataReader;

            if (code == ValueTypeCodes.Direct)
            {
                if (valueWriter is IDirectContent direct)
                {
                    direct.DirectContent = value.Object;

                    return;
                }

                dataReader = RWHelper.CreateReader(value.Object).As<int>();
            }
            else
            {
                dataReader = value.DataReader.As<int>();
            }

            valueWriter.Initialize(dataReader.Count);

            RWHelper.Copy(dataReader, valueWriter);
        }

        /// <summary>
        /// 读取一个 Boolean 值。
        /// </summary>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool ReadBoolean()
        {
            switch (code)
            {
                case ValueTypeCodes.Boolean:
                    return value.Boolean;
                case ValueTypeCodes.Null:
                    return false;
                case ValueTypeCodes.SByte:
                case ValueTypeCodes.Int16:
                case ValueTypeCodes.Int32:
                case ValueTypeCodes.Int64:
                case ValueTypeCodes.Byte:
                case ValueTypeCodes.UInt16:
                case ValueTypeCodes.UInt32:
                case ValueTypeCodes.UInt64:
                case ValueTypeCodes.Single:
                case ValueTypeCodes.Double:
                    return value.Int64 != 0;
                case ValueTypeCodes.Decimal:
                    return value.Decimal != 0;
                case ValueTypeCodes.Char:
                    throw new InvalidCastException("Can't convert Char to Boolean.");
                case ValueTypeCodes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Boolean.");
                case ValueTypeCodes.String:
                    return bool.Parse(value.String);
                case ValueTypeCodes.Direct:
                    return (bool)value.Object;
                case ValueTypeCodes.Array:
                    throw new InvalidCastException("Can't convert Array to Boolean.");
            }

            throw new InvalidCastException("Can't convert Object to Boolean.");
        }

        /// <summary>
        /// 获取值是否为空。
        /// </summary>
        /// <returns>返回一个 bool 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool IsEmptyValue()
        {
            switch (code)
            {
                case ValueTypeCodes.Boolean:
                    return value.Boolean == default;
                case ValueTypeCodes.SByte:
                    return value.SByte == default;
                case ValueTypeCodes.Int16:
                    return value.Int16 == default;
                case ValueTypeCodes.Int32:
                    return value.Int32 == default;
                case ValueTypeCodes.Int64:
                    return value.Int64 == default;
                case ValueTypeCodes.Byte:
                    return value.Byte == default;
                case ValueTypeCodes.UInt16:
                    return value.UInt16 == default;
                case ValueTypeCodes.UInt32:
                    return value.UInt32 == default;
                case ValueTypeCodes.UInt64:
                    return value.UInt64 == default;
                case ValueTypeCodes.Single:
                    return value.Single == default;
                case ValueTypeCodes.Double:
                    return value.Double == default;
                case ValueTypeCodes.Decimal:
                    return value.Decimal == default;
                case ValueTypeCodes.Char:
                    return value.Char == default;
                case ValueTypeCodes.DateTime:
                    return value.DateTime == default;
                case ValueTypeCodes.String:
                    return value.String == default;
                case ValueTypeCodes.Null:
                    return true;
                case ValueTypeCodes.Direct:
                    return TypeHelper.IsEmptyValue(value.Object);
            }

            if (value.DataReader is IDirectContent direct)
            {
                return TypeHelper.IsEmptyValue(direct.DirectContent);
            }

            return false;
        }

        /// <summary>
        /// 读取一个 Byte 值。
        /// </summary>
        /// <returns>返回一个 byte 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public byte ReadByte()
        {
            switch (code)
            {
                case ValueTypeCodes.Null:
                    return 0;
                case ValueTypeCodes.Boolean:
                case ValueTypeCodes.SByte:
                case ValueTypeCodes.Int16:
                case ValueTypeCodes.Int32:
                case ValueTypeCodes.Int64:
                case ValueTypeCodes.Byte:
                case ValueTypeCodes.UInt16:
                case ValueTypeCodes.UInt32:
                case ValueTypeCodes.UInt64:
                case ValueTypeCodes.Char:
                    return value.Byte;
                case ValueTypeCodes.Single:
                    return (byte)value.Single;
                case ValueTypeCodes.Double:
                    return (byte)value.Double;
                case ValueTypeCodes.Decimal:
                    return (byte)value.Decimal;
                case ValueTypeCodes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Byte.");
                case ValueTypeCodes.String:
                    return byte.Parse(value.String);
                case ValueTypeCodes.Direct:
                    return (byte)value.Object;
                case ValueTypeCodes.Array:
                    throw new InvalidCastException("Can't convert Array to Byte.");
            }

            throw new InvalidCastException("Can't convert Object to Byte.");
        }

        /// <summary>
        /// 读取一个 Char 值。
        /// </summary>
        /// <returns>返回一个 char 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public char ReadChar()
        {
            switch (code)
            {
                case ValueTypeCodes.Null:
                    throw new InvalidCastException("Can't convert Null to Char.");
                case ValueTypeCodes.Single:
                    return (char)value.Single;
                case ValueTypeCodes.Double:
                    return (char)value.Double;
                case ValueTypeCodes.Decimal:
                    return (char)value.Decimal;
                case ValueTypeCodes.Boolean:
                case ValueTypeCodes.SByte:
                case ValueTypeCodes.Int16:
                case ValueTypeCodes.Int32:
                case ValueTypeCodes.Int64:
                case ValueTypeCodes.Byte:
                case ValueTypeCodes.UInt16:
                case ValueTypeCodes.UInt32:
                case ValueTypeCodes.UInt64:
                case ValueTypeCodes.Char:
                    return value.Char;
                case ValueTypeCodes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Char.");
                case ValueTypeCodes.String:
                    return char.Parse(value.String);
                case ValueTypeCodes.Direct:
                    return (char)value.Object;
                case ValueTypeCodes.Array:
                    throw new InvalidCastException("Can't convert Array to Byte.");
            }

            throw new InvalidCastException("Can't convert Object to Byte.");
        }

        /// <summary>
        /// 读取一个 DateTime 值。
        /// </summary>
        /// <returns>返回一个 DateTime 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public DateTime ReadDateTime()
        {
            switch (code)
            {
                case ValueTypeCodes.Null:
                    throw new InvalidCastException("Can't convert Null to DateTime.");
                case ValueTypeCodes.Boolean:
                    throw new InvalidCastException("Can't convert Boolean to DateTime.");
                case ValueTypeCodes.UInt64:
                case ValueTypeCodes.Int64:
                    return new DateTime(value.Int64);
                case ValueTypeCodes.SByte:
                case ValueTypeCodes.Int16:
                case ValueTypeCodes.Int32:
                case ValueTypeCodes.Byte:
                case ValueTypeCodes.UInt16:
                case ValueTypeCodes.UInt32:
                case ValueTypeCodes.Single:
                case ValueTypeCodes.Double:
                case ValueTypeCodes.Decimal:
                    throw new InvalidCastException("Can't convert Number to DateTime.");
                case ValueTypeCodes.Char:
                    throw new InvalidCastException("Can't convert Char to DateTime.");
                case ValueTypeCodes.DateTime:
                    return value.DateTime;
                case ValueTypeCodes.String:
                    return DateTime.Parse(value.String);
                case ValueTypeCodes.Direct:
                    return (DateTime)value.Object;
                case ValueTypeCodes.Array:
                    throw new InvalidCastException("Can't convert Array to DateTime.");
            }

            throw new InvalidCastException("Can't convert Object to DateTime.");
        }

        /// <summary>
        /// 读取一个 Decimal 值。
        /// </summary>
        /// <returns>返回一个 decimal 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public decimal ReadDecimal()
        {
            switch (code)
            {
                case ValueTypeCodes.Null:
                    return 0;
                case ValueTypeCodes.Boolean:
                    return value.Byte;
                case ValueTypeCodes.SByte:
                    return value.SByte;
                case ValueTypeCodes.Int16:
                    return value.Int16;
                case ValueTypeCodes.Int32:
                    return value.Int32;
                case ValueTypeCodes.Int64:
                    return value.Int64;
                case ValueTypeCodes.Byte:
                    return value.Byte;
                case ValueTypeCodes.UInt16:
                    return value.UInt16;
                case ValueTypeCodes.UInt32:
                    return value.UInt32;
                case ValueTypeCodes.UInt64:
                    return value.UInt64;
                case ValueTypeCodes.Single:
                    return (decimal)value.Single;
                case ValueTypeCodes.Double:
                    return (decimal)value.Double;
                case ValueTypeCodes.Decimal:
                    return value.Decimal;
                case ValueTypeCodes.Char:
                    return value.Char;
                case ValueTypeCodes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Decimal.");
                case ValueTypeCodes.String:
                    return decimal.Parse(value.String);
                case ValueTypeCodes.Direct:
                    return (decimal)value.Object;
                case ValueTypeCodes.Array:
                    throw new InvalidCastException("Can't convert Array to Decimal.");
            }

            throw new InvalidCastException("Can't convert Object to Decimal.");
        }

        /// <summary>
        /// 读取一个未知类型的值。
        /// </summary>
        /// <returns>返回一个未知类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object DirectRead()
        {
            switch (code)
            {
                case ValueTypeCodes.Null:
                    return null;
                case ValueTypeCodes.Boolean:
                    return value.Boolean;
                case ValueTypeCodes.SByte:
                    return value.SByte;
                case ValueTypeCodes.Int16:
                    return value.Int16;
                case ValueTypeCodes.Int32:
                    return value.Int32;
                case ValueTypeCodes.Int64:
                    return value.Int64;
                case ValueTypeCodes.Byte:
                    return value.Byte;
                case ValueTypeCodes.UInt16:
                    return value.UInt16;
                case ValueTypeCodes.UInt32:
                    return value.UInt32;
                case ValueTypeCodes.UInt64:
                    return value.UInt64;
                case ValueTypeCodes.Single:
                    return value.Single;
                case ValueTypeCodes.Double:
                    return value.Double;
                case ValueTypeCodes.Decimal:
                    return value.Decimal;
                case ValueTypeCodes.Char:
                    return value.Char;
                case ValueTypeCodes.DateTime:
                    return value.DateTime;
                case ValueTypeCodes.String:
                    return value.String;
                case ValueTypeCodes.Direct:
                    return value.Object;
            }

            if (value.Object is IDirectContent direct)
            {
                return direct.DirectContent;
            }

            throw new NotSupportedException("Can't ReadDirect by Object/Array.");
        }

        /// <summary>
        /// 读取一个 Double 值。
        /// </summary>
        /// <returns>返回一个 double 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public double ReadDouble()
        {
            switch (code)
            {
                case ValueTypeCodes.Null:
                    return 0;
                case ValueTypeCodes.Boolean:
                    return value.Byte;
                case ValueTypeCodes.SByte:
                    return value.SByte;
                case ValueTypeCodes.Int16:
                    return value.Int16;
                case ValueTypeCodes.Int32:
                    return value.Int32;
                case ValueTypeCodes.Int64:
                    return value.Int64;
                case ValueTypeCodes.Byte:
                    return value.Byte;
                case ValueTypeCodes.UInt16:
                    return value.UInt16;
                case ValueTypeCodes.UInt32:
                    return value.UInt32;
                case ValueTypeCodes.UInt64:
                    return value.UInt64;
                case ValueTypeCodes.Single:
                    return value.Single;
                case ValueTypeCodes.Double:
                    return value.Double;
                case ValueTypeCodes.Decimal:
                    return (double)value.Decimal;
                case ValueTypeCodes.Char:
                    return value.Char;
                case ValueTypeCodes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Double.");
                case ValueTypeCodes.String:
                    return double.Parse(value.String);
                case ValueTypeCodes.Direct:
                    return (double)value.Object;
                case ValueTypeCodes.Array:
                    throw new InvalidCastException("Can't convert Array to Double.");
            }

            throw new InvalidCastException("Can't convert Object to Double.");
        }

        /// <summary>
        /// 读取一个 Int16 值。
        /// </summary>
        /// <returns>返回一个 short 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public short ReadInt16()
        {
            switch (code)
            {
                case ValueTypeCodes.Null:
                    return 0;
                case ValueTypeCodes.Int32:
                    return (short)value.Int32;
                case ValueTypeCodes.Int64:
                    return (short)value.Int64;
                case ValueTypeCodes.Boolean:
                case ValueTypeCodes.SByte:
                case ValueTypeCodes.Int16:
                case ValueTypeCodes.Byte:
                case ValueTypeCodes.UInt16:
                case ValueTypeCodes.UInt32:
                case ValueTypeCodes.UInt64:
                    return value.Int16;
                case ValueTypeCodes.Single:
                    return (short)value.Single;
                case ValueTypeCodes.Double:
                    return (short)value.Double;
                case ValueTypeCodes.Decimal:
                    return (short)value.Decimal;
                case ValueTypeCodes.Char:
                    return (short)value.Char;
                case ValueTypeCodes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Int16.");
                case ValueTypeCodes.String:
                    return short.Parse(value.String);
                case ValueTypeCodes.Direct:
                    return (short)value.Object;
                case ValueTypeCodes.Array:
                    throw new InvalidCastException("Can't convert Array to Int16.");
            }

            throw new InvalidCastException("Can't convert Object to Int16.");
        }

        /// <summary>
        /// 读取一个 Int32 值。
        /// </summary>
        /// <returns>返回一个 int 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ReadInt32()
        {
            switch (code)
            {
                case ValueTypeCodes.Null:
                    return 0;
                case ValueTypeCodes.Boolean:
                case ValueTypeCodes.SByte:
                case ValueTypeCodes.Int16:
                case ValueTypeCodes.Int32:
                case ValueTypeCodes.Byte:
                case ValueTypeCodes.UInt16:
                case ValueTypeCodes.UInt32:
                case ValueTypeCodes.UInt64:
                    return value.Int32;
                case ValueTypeCodes.Int64:
                    return (int)value.Int64;
                case ValueTypeCodes.Single:
                    return (int)value.Single;
                case ValueTypeCodes.Double:
                    return (int)value.Double;
                case ValueTypeCodes.Decimal:
                    return (int)value.Decimal;
                case ValueTypeCodes.Char:
                    return value.Char;
                case ValueTypeCodes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Int32.");
                case ValueTypeCodes.String:
                    return int.Parse(value.String);
                case ValueTypeCodes.Direct:
                    return (int)value.Object;
                case ValueTypeCodes.Array:
                    throw new InvalidCastException("Can't convert Array to Int32.");
            }

            throw new InvalidCastException("Can't convert Object to Int32.");
        }

        /// <summary>
        /// 读取一个 Int64 值。
        /// </summary>
        /// <returns>返回一个 long 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ReadInt64()
        {
            switch (code)
            {
                case ValueTypeCodes.Null:
                    return 0;
                case ValueTypeCodes.Boolean:
                case ValueTypeCodes.SByte:
                case ValueTypeCodes.Int16:
                case ValueTypeCodes.Int32:
                case ValueTypeCodes.Int64:
                case ValueTypeCodes.Byte:
                case ValueTypeCodes.UInt16:
                case ValueTypeCodes.UInt32:
                case ValueTypeCodes.UInt64:
                    return value.Int64;
                case ValueTypeCodes.Single:
                    return (long)value.Single;
                case ValueTypeCodes.Double:
                    return (long)value.Double;
                case ValueTypeCodes.Decimal:
                    return (long)value.Decimal;
                case ValueTypeCodes.Char:
                    return value.Char;
                case ValueTypeCodes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Int64.");
                case ValueTypeCodes.String:
                    return long.Parse(value.String);
                case ValueTypeCodes.Direct:
                    return (long)value.Object;
                case ValueTypeCodes.Array:
                    throw new InvalidCastException("Can't convert Array to Int64.");
            }

            throw new InvalidCastException("Can't convert Object to Int64.");
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
                    return;
                case ValueTypeCodes.Boolean:
                    throw new InvalidCastException("Can't convert Boolean to Object.");
                case ValueTypeCodes.SByte:
                case ValueTypeCodes.Int16:
                case ValueTypeCodes.Int32:
                case ValueTypeCodes.Int64:
                case ValueTypeCodes.Byte:
                case ValueTypeCodes.UInt16:
                case ValueTypeCodes.UInt32:
                case ValueTypeCodes.UInt64:
                case ValueTypeCodes.Single:
                case ValueTypeCodes.Double:
                case ValueTypeCodes.Decimal:
                    throw new InvalidCastException("Can't convert Number to Object.");
                case ValueTypeCodes.Char:
                    throw new InvalidCastException("Can't convert Char to Object.");
                case ValueTypeCodes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Object.");
                case ValueTypeCodes.String:
                    throw new InvalidCastException("Can't convert String to Object.");
            }

            IDataReader<string> dataReader;

            if (code == ValueTypeCodes.Direct)
            {
                if (valueWriter is IDirectContent direct)
                {
                    direct.DirectContent = value.Object;

                    return;
                }

                dataReader = RWHelper.CreateReader(value.Object).As<string>();
            }
            else
            {
                dataReader = value.DataReader.As<string>();
            }

            valueWriter.Initialize(dataReader.Count);

            RWHelper.Copy(dataReader, valueWriter);
        }

        /// <summary>
        /// 读取一个 SByte 值。
        /// </summary>
        /// <returns>返回一个 sbyte 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public sbyte ReadSByte()
        {
            switch (code)
            {
                case ValueTypeCodes.Null:
                    return 0;
                case ValueTypeCodes.Int16:
                    return (sbyte)value.Int16;
                case ValueTypeCodes.Int32:
                    return (sbyte)value.Int32;
                case ValueTypeCodes.Int64:
                    return (sbyte)value.Int64;
                case ValueTypeCodes.Boolean:
                case ValueTypeCodes.SByte:
                case ValueTypeCodes.Byte:
                case ValueTypeCodes.UInt16:
                case ValueTypeCodes.UInt32:
                case ValueTypeCodes.UInt64:
                case ValueTypeCodes.Char:
                    return value.SByte;
                case ValueTypeCodes.Single:
                    return (sbyte)value.Single;
                case ValueTypeCodes.Double:
                    return (sbyte)value.Double;
                case ValueTypeCodes.Decimal:
                    return (sbyte)value.Decimal;
                case ValueTypeCodes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to SByte.");
                case ValueTypeCodes.String:
                    return sbyte.Parse(value.String);
                case ValueTypeCodes.Direct:
                    return (sbyte)value.Object;
                case ValueTypeCodes.Array:
                    throw new InvalidCastException("Can't convert Array to SByte.");
            }

            throw new InvalidCastException("Can't convert Object to SByte.");
        }

        /// <summary>
        /// 读取一个 Single 值。
        /// </summary>
        /// <returns>返回一个 flaot 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public float ReadSingle()
        {
            switch (code)
            {
                case ValueTypeCodes.Null:
                    return 0;
                case ValueTypeCodes.Boolean:
                    return value.Byte;
                case ValueTypeCodes.SByte:
                    return value.SByte;
                case ValueTypeCodes.Int16:
                    return value.Int16;
                case ValueTypeCodes.Int32:
                    return value.Int32;
                case ValueTypeCodes.Int64:
                    return value.Int64;
                case ValueTypeCodes.Byte:
                    return value.Byte;
                case ValueTypeCodes.UInt16:
                    return value.UInt16;
                case ValueTypeCodes.UInt32:
                    return value.UInt32;
                case ValueTypeCodes.UInt64:
                    return value.UInt64;
                case ValueTypeCodes.Single:
                    return value.Single;
                case ValueTypeCodes.Double:
                    return (float)value.Double;
                case ValueTypeCodes.Decimal:
                    return (float)value.Decimal;
                case ValueTypeCodes.Char:
                    return value.Char;
                case ValueTypeCodes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to Single.");
                case ValueTypeCodes.String:
                    return float.Parse(value.String);
                case ValueTypeCodes.Direct:
                    return (float)value.Object;
                case ValueTypeCodes.Array:
                    throw new InvalidCastException("Can't convert Array to Single.");
            }

            throw new InvalidCastException("Can't convert Object to Single.");
        }

        /// <summary>
        /// 读取一个 String 值。
        /// </summary>
        /// <returns>返回一个 string 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string ReadString()
        {
            switch (code)
            {
                case ValueTypeCodes.Null:
                    return null;
                case ValueTypeCodes.Boolean:
                    return value.Boolean.ToString();
                case ValueTypeCodes.SByte:
                    return value.SByte.ToString();
                case ValueTypeCodes.Int16:
                    return value.Int16.ToString();
                case ValueTypeCodes.Int32:
                    return value.Int32.ToString();
                case ValueTypeCodes.Int64:
                    return value.Int64.ToString();
                case ValueTypeCodes.Byte:
                    return value.Byte.ToString();
                case ValueTypeCodes.UInt16:
                    return value.UInt16.ToString();
                case ValueTypeCodes.UInt32:
                    return value.UInt32.ToString();
                case ValueTypeCodes.UInt64:
                    return value.UInt64.ToString();
                case ValueTypeCodes.Single:
                    return value.Single.ToString();
                case ValueTypeCodes.Double:
                    return value.Double.ToString();
                case ValueTypeCodes.Decimal:
                    return value.Decimal.ToString();
                case ValueTypeCodes.Char:
                    return value.Char.ToString();
                case ValueTypeCodes.DateTime:
                    return value.DateTime.ToString();
                case ValueTypeCodes.String:
                    return value.String;
                case ValueTypeCodes.Direct:
                    return value.String;
                case ValueTypeCodes.Array:
                    throw new InvalidCastException("Can't convert Array to String.");
            }

            throw new InvalidCastException("Can't convert Object to String.");
        }

        /// <summary>
        /// 读取一个 UInt16 值。
        /// </summary>
        /// <returns>返回一个 ushort 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            switch (code)
            {
                case ValueTypeCodes.Null:
                    return 0;
                case ValueTypeCodes.Boolean:
                case ValueTypeCodes.SByte:
                case ValueTypeCodes.Int16:
                case ValueTypeCodes.Int32:
                case ValueTypeCodes.Int64:
                case ValueTypeCodes.Byte:
                case ValueTypeCodes.UInt16:
                case ValueTypeCodes.UInt32:
                case ValueTypeCodes.UInt64:
                    return value.UInt16;
                case ValueTypeCodes.Single:
                    return (ushort)value.Single;
                case ValueTypeCodes.Double:
                    return (ushort)value.Double;
                case ValueTypeCodes.Decimal:
                    return (ushort)value.Decimal;
                case ValueTypeCodes.Char:
                    return value.Char;
                case ValueTypeCodes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to UInt16.");
                case ValueTypeCodes.String:
                    return ushort.Parse(value.String);
                case ValueTypeCodes.Direct:
                    return (ushort)value.Object;
                case ValueTypeCodes.Array:
                    throw new InvalidCastException("Can't convert Array to UInt16.");
            }

            throw new InvalidCastException("Can't convert Object to UInt16.");
        }

        /// <summary>
        /// 读取一个 UInt32 值。
        /// </summary>
        /// <returns>返回一个 uint 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public uint ReadUInt32()
        {
            switch (code)
            {
                case ValueTypeCodes.Null:
                    return 0;
                case ValueTypeCodes.Boolean:
                case ValueTypeCodes.SByte:
                case ValueTypeCodes.Int16:
                case ValueTypeCodes.Int32:
                case ValueTypeCodes.Int64:
                case ValueTypeCodes.Byte:
                case ValueTypeCodes.UInt16:
                case ValueTypeCodes.UInt32:
                case ValueTypeCodes.UInt64:
                    return value.UInt32;
                case ValueTypeCodes.Single:
                    return (uint)value.Single;
                case ValueTypeCodes.Double:
                    return (uint)value.Double;
                case ValueTypeCodes.Decimal:
                    return (uint)value.Decimal;
                case ValueTypeCodes.Char:
                    return value.Char;
                case ValueTypeCodes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to UInt32.");
                case ValueTypeCodes.String:
                    return uint.Parse(value.String);
                case ValueTypeCodes.Direct:
                    return (uint)value.Object;
                case ValueTypeCodes.Array:
                    throw new InvalidCastException("Can't convert Array to UInt32.");
            }

            throw new InvalidCastException("Can't convert Object to UInt32.");
        }

        /// <summary>
        /// 读取一个 UInt64 值。
        /// </summary>
        /// <returns>返回一个 ulong 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            switch (code)
            {
                case ValueTypeCodes.Null:
                    return 0;
                case ValueTypeCodes.Boolean:
                case ValueTypeCodes.SByte:
                case ValueTypeCodes.Int16:
                case ValueTypeCodes.Int32:
                case ValueTypeCodes.Int64:
                case ValueTypeCodes.Byte:
                case ValueTypeCodes.UInt16:
                case ValueTypeCodes.UInt32:
                case ValueTypeCodes.UInt64:
                    return value.UInt64;
                case ValueTypeCodes.Single:
                    return (ulong)value.Single;
                case ValueTypeCodes.Double:
                    return (ulong)value.Double;
                case ValueTypeCodes.Decimal:
                    return (ulong)value.Decimal;
                case ValueTypeCodes.Char:
                    return value.Char;
                case ValueTypeCodes.DateTime:
                    throw new InvalidCastException("Can't convert DateTime to UInt64.");
                case ValueTypeCodes.String:
                    return ulong.Parse(value.String);
                case ValueTypeCodes.Direct:
                    return (ulong)value.Object;
                case ValueTypeCodes.Array:
                    throw new InvalidCastException("Can't convert Array to UInt64.");
            }

            throw new InvalidCastException("Can't convert Object to UInt64.");
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
            if (value == null)
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
            if (value == null)
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
        /// 基础类型枚举，此枚举不能按位合并值。
        /// </summary>
        enum ValueTypeCodes : byte
        {
            /// <summary>
            /// Boolean, bool
            /// </summary>
            Boolean = TypeCode.Boolean,
            /// <summary>
            /// SByte, sbyte
            /// </summary>
            SByte = TypeCode.SByte,
            /// <summary>
            /// Int16, short
            /// </summary>
            Int16 = TypeCode.Int16,
            /// <summary>
            /// Int32, int
            /// </summary>
            Int32 = TypeCode.Int32,
            /// <summary>
            /// Int64, long
            /// </summary>
            Int64 = TypeCode.Int64,
            /// <summary>
            /// Byte, byte
            /// </summary>
            Byte = TypeCode.Byte,
            /// <summary>
            /// UInt16, ushort
            /// </summary>
            UInt16 = TypeCode.UInt16,
            /// <summary>
            /// UInt32, uint
            /// </summary>
            UInt32 = TypeCode.UInt32,
            /// <summary>
            /// UInt64, ulong
            /// </summary>
            UInt64 = TypeCode.UInt64,
            /// <summary>
            /// Single, float
            /// </summary>
            Single = TypeCode.Single,
            /// <summary>
            /// Double, double
            /// </summary>
            Double = TypeCode.Double,
            /// <summary>
            /// Decimal, decimal
            /// </summary>
            Decimal = TypeCode.Decimal,
            /// <summary>
            /// Char, char
            /// </summary>
            Char = TypeCode.Char,
            /// <summary>
            /// DateTime
            /// </summary>
            DateTime = TypeCode.DateTime,
            /// <summary>
            /// String, string
            /// </summary>
            String = TypeCode.String,
            /// <summary>
            /// Direct
            /// 
            /// 表示可以直接读写值的类型。
            /// 通常是可以用字符串表示的值的类型。
            /// 
            /// Represents a type that can read and write value directly.
            /// is typically the type of a value that can be represented by a string.
            /// </summary>
            Direct = 100,
            /// <summary>
            /// Array
            /// </summary>
            Array = 101,
            /// <summary>
            /// Object
            /// 其他类型
            /// Other types
            /// </summary>
            Object = 102,
            /// <summary>
            /// Null, DBNull
            /// </summary>
            Null = TypeCode.Empty
        }
    }
}