
using Swifter.Tools;

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Swifter.RW
{
    /// <summary>
    /// 值暂存器。
    /// </summary>
    public sealed class ValueCopyer : IValueRW
    {
        /// <summary>
        /// 创建一个具有指定初始值的值暂存器。
        /// </summary>
        /// <typeparam name="T">初始值类型</typeparam>
        /// <param name="value">初始值</param>
        /// <returns>返回一个值暂存器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ValueCopyer ValueOf<T>(T? value)
        {
            var valueCopyer = new ValueCopyer();

            valueCopyer.InternalValueOf(value);

            return valueCopyer;
        }

        /// <summary>
        /// 创建一个具有指定初始值的值暂存器。
        /// </summary>
        /// <param name="value">初始值</param>
        /// <returns>返回一个值暂存器</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ValueCopyer ValueOf(object? value)
        {
            var valueCopyer = new ValueCopyer();

            valueCopyer.InternalValueOf(value);

            return valueCopyer;
        }

        private RWOverlappedValue value;
        private RWTypeCode code;

        /// <summary>
        /// 初始化值暂存器。
        /// </summary>
        public ValueCopyer()
        {
            code = RWTypeCode.Null;
        }

        /// <summary>
        /// 获取值的 <see cref="RWTypeCode"/>。
        /// </summary>
        public RWTypeCode TypeCode => code;

        /// <summary>
        /// 获取值暂存器的内部对象。
        /// </summary>
        public object? InternalValue
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                switch (code)
                {
                    case RWTypeCode.Null: return null;
                    case RWTypeCode.Boolean: return value.Boolean;
                    case RWTypeCode.Byte: return value.Byte;
                    case RWTypeCode.SByte: return value.SByte;
                    case RWTypeCode.Char: return value.Char;
                    case RWTypeCode.Int16: return value.Int16;
                    case RWTypeCode.UInt16: return value.UInt16;
                    case RWTypeCode.Int32: return value.Int32;
                    case RWTypeCode.UInt32: return value.UInt32;
                    case RWTypeCode.Int64: return value.Int64;
                    case RWTypeCode.UInt64: return value.UInt64;
                    case RWTypeCode.Single: return value.Single;
                    case RWTypeCode.Double: return value.Double;
                    case RWTypeCode.Decimal: return value.Decimal;
                    case RWTypeCode.DateTime: return value.DateTime;
                    case RWTypeCode.DateTimeOffset: return value.DateTimeOffset;
                    case RWTypeCode.TimeSpan: return value.TimeSpan;
                    case RWTypeCode.Guid: return value.Guid;
                    case RWTypeCode.Enum: return value.EnumInterface.Box(value.UInt64);
                    case RWTypeCode.String: return value.String;
                    case RWTypeCode.Object: return value.ObjectReader;
                    case RWTypeCode.Array: return value.ArrayReader;
                    default: return value.Other;
                }
            }
        }

        /// <summary>
        /// 获取值暂存器里的值的类型。
        /// </summary>
        public Type? ValueType
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                switch (code)
                {
                    case RWTypeCode.Null: return null;
                    case RWTypeCode.Boolean: return typeof(Boolean);
                    case RWTypeCode.Byte: return typeof(Byte);
                    case RWTypeCode.SByte: return typeof(SByte);
                    case RWTypeCode.Char: return typeof(Char);
                    case RWTypeCode.Int16: return typeof(Int16);
                    case RWTypeCode.UInt16: return typeof(UInt16);
                    case RWTypeCode.Int32: return typeof(Int32);
                    case RWTypeCode.UInt32: return typeof(UInt32);
                    case RWTypeCode.Int64: return typeof(Int64);
                    case RWTypeCode.UInt64: return typeof(UInt64);
                    case RWTypeCode.Single: return typeof(Single);
                    case RWTypeCode.Double: return typeof(Double);
                    case RWTypeCode.Decimal: return typeof(Decimal);
                    case RWTypeCode.DateTime: return typeof(DateTime);
                    case RWTypeCode.DateTimeOffset: return typeof(DateTimeOffset);
                    case RWTypeCode.TimeSpan: return typeof(TimeSpan);
                    case RWTypeCode.Guid: return typeof(Guid);
                    case RWTypeCode.String: return typeof(String);
                    case RWTypeCode.Enum: return value.EnumInterface.EnumType;
                    case RWTypeCode.Object: return value.ObjectReader.ContentType;
                    case RWTypeCode.Array: return value.ArrayReader.ContentType;
                    default: return value.Other.GetType();
                }
            }
        }

        /// <summary>
        /// 获取值是否为空。
        /// </summary>
        /// <returns>返回一个 bool 值。</returns>
        public bool IsEmptyValue
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                switch (code)
                {
                    case RWTypeCode.Null: return true;
                    case RWTypeCode.Boolean: return value.Boolean == default;
                    case RWTypeCode.Byte: return value.Byte == default;
                    case RWTypeCode.SByte: return value.SByte == default;
                    case RWTypeCode.Char: return value.Char == default;
                    case RWTypeCode.Int16: return value.Int16 == default;
                    case RWTypeCode.UInt16: return value.UInt16 == default;
                    case RWTypeCode.Int32: return value.Int32 == default;
                    case RWTypeCode.UInt32: return value.UInt32 == default;
                    case RWTypeCode.Int64: return value.Int64 == default;
                    case RWTypeCode.UInt64: return value.UInt64 == default;
                    case RWTypeCode.Single: return value.Single == default;
                    case RWTypeCode.Double: return value.Double == default;
                    case RWTypeCode.Decimal: return value.Decimal == default;
                    case RWTypeCode.DateTime: return value.DateTime == default;
                    case RWTypeCode.DateTimeOffset: return value.DateTimeOffset == default;
                    case RWTypeCode.TimeSpan: return value.TimeSpan == default;
                    case RWTypeCode.Guid: return value.Guid == default;
                    case RWTypeCode.String: return value.String == default;
                    case RWTypeCode.Enum: return value.UInt64 == default;
                    case RWTypeCode.Object: return TypeHelper.IsEmptyValue(value.ObjectReader.Content);
                    case RWTypeCode.Array: return TypeHelper.IsEmptyValue(value.ArrayReader.Content);
                    default: return TypeHelper.IsEmptyValue(value.Other);
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void InternalValueOf(object? val)
        {
            if (val is null or DBNull) return;

            if (val is Enum enumVal)
            {
                code = RWTypeCode.Enum;

                value.EnumInterface = EnumInterface.GetInstance(enumVal.GetType());
                value.UInt64 = value.EnumInterface.UnBox(enumVal);

                return;
            }

            if (val is IConvertible)
            {
                switch ((Unsafe.As<IConvertible>(val).GetTypeCode()))
                {
                    case System.TypeCode.Boolean:
                        code = RWTypeCode.Boolean;
                        UnboxAndStore(ref value.Boolean, val);
                        return;
                    case System.TypeCode.Char:
                        code = RWTypeCode.Char;
                        UnboxAndStore(ref value.Char, val);
                        return;
                    case System.TypeCode.SByte:
                        code = RWTypeCode.SByte;
                        UnboxAndStore(ref value.SByte, val);
                        return;
                    case System.TypeCode.Byte:
                        code = RWTypeCode.Byte;
                        UnboxAndStore(ref value.Byte, val);
                        return;
                    case System.TypeCode.Int16:
                        code = RWTypeCode.Int16;
                        UnboxAndStore(ref value.Int16, val);
                        return;
                    case System.TypeCode.UInt16:
                        code = RWTypeCode.UInt16;
                        UnboxAndStore(ref value.UInt16, val);
                        return;
                    case System.TypeCode.Int32:
                        code = RWTypeCode.Int32;
                        UnboxAndStore(ref value.Int32, val);
                        return;
                    case System.TypeCode.UInt32:
                        code = RWTypeCode.UInt32;
                        UnboxAndStore(ref value.UInt32, val);
                        return;
                    case System.TypeCode.Int64:
                        code = RWTypeCode.Int64;
                        UnboxAndStore(ref value.Int64, val);
                        return;
                    case System.TypeCode.UInt64:
                        code = RWTypeCode.UInt64;
                        UnboxAndStore(ref value.UInt64, val);
                        return;
                    case System.TypeCode.Single:
                        code = RWTypeCode.Single;
                        UnboxAndStore(ref value.Single, val);
                        return;
                    case System.TypeCode.Double:
                        code = RWTypeCode.Double;
                        UnboxAndStore(ref value.Double, val);
                        return;
                    case System.TypeCode.Decimal:
                        code = RWTypeCode.Decimal;
                        UnboxAndStore(ref value.Decimal, val);
                        return;
                    case System.TypeCode.DateTime:
                        code = RWTypeCode.DateTime;
                        UnboxAndStore(ref value.DateTime, val);
                        return;
                    case System.TypeCode.String:
                        code = RWTypeCode.String;
                        value.String = Unsafe.As<string>(val);
                        return;
                }
            }

            if (val is Guid)
            {
                code = RWTypeCode.Guid;
                UnboxAndStore(ref value.Guid, val);
                return;
            }

            if (val is DateTimeOffset)
            {
                code = RWTypeCode.DateTimeOffset;
                UnboxAndStore(ref value.DateTimeOffset, val);
                return;
            }

            if (val is TimeSpan)
            {
                code = RWTypeCode.TimeSpan;
                UnboxAndStore(ref value.TimeSpan, val);
                return;
            }

            code = RWTypeCode.Other;
            value.Other = val;

            static void UnboxAndStore<TTo>(ref TTo destination, object value) where TTo : struct
                => destination = TypeHelper.Unbox<TTo>(value);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void InternalValueOf<T>(T? val)
        {
            if (val is null or DBNull) return;

            if (val is Enum enumVal)
            {
                // TODO: 优化性能

                code = RWTypeCode.Enum;

                value.EnumInterface = EnumInterface.GetInstance(enumVal.GetType());
                value.UInt64 = value.EnumInterface.UnBox(enumVal);

                return;
            }

            switch (Type.GetTypeCode(typeof(T)))
            {
                case System.TypeCode.Boolean:
                    code = RWTypeCode.Boolean;
                    As(ref value.Boolean) = val;
                    return;
                case System.TypeCode.Char:
                    code = RWTypeCode.Char;
                    As(ref value.Char) = val;
                    return;
                case System.TypeCode.SByte:
                    code = RWTypeCode.SByte;
                    As(ref value.SByte) = val;
                    return;
                case System.TypeCode.Byte:
                    code = RWTypeCode.Byte;
                    As(ref value.Byte) = val;
                    return;
                case System.TypeCode.Int16:
                    code = RWTypeCode.Int16;
                    As(ref value.Int16) = val;
                    return;
                case System.TypeCode.UInt16:
                    code = RWTypeCode.UInt16;
                    As(ref value.UInt16) = val;
                    return;
                case System.TypeCode.Int32:
                    code = RWTypeCode.Int32;
                    As(ref value.Int32) = val;
                    return;
                case System.TypeCode.UInt32:
                    code = RWTypeCode.UInt32;
                    As(ref value.UInt32) = val;
                    return;
                case System.TypeCode.Int64:
                    code = RWTypeCode.Int64;
                    As(ref value.Int64) = val;
                    return;
                case System.TypeCode.UInt64:
                    code = RWTypeCode.UInt64;
                    As(ref value.UInt64) = val;
                    return;
                case System.TypeCode.Single:
                    code = RWTypeCode.Single;
                    As(ref value.Single) = val;
                    return;
                case System.TypeCode.Double:
                    code = RWTypeCode.Double;
                    As(ref value.Double) = val;
                    return;
                case System.TypeCode.Decimal:
                    code = RWTypeCode.Decimal;
                    As(ref value.Decimal) = val;
                    return;
                case System.TypeCode.DateTime:
                    code = RWTypeCode.DateTime;
                    As(ref value.DateTime) = val;
                    return;
                case System.TypeCode.String:
                    code = RWTypeCode.String;
                    As(ref value.String) = val;
                    return;
            }

            if (typeof(T) == typeof(Guid))
            {
                code = RWTypeCode.Guid;
                As(ref value.Guid) = val;
                return;
            }

            if (typeof(T) == typeof(DateTimeOffset))
            {
                code = RWTypeCode.DateTimeOffset;
                As(ref value.DateTimeOffset) = val;
                return;
            }

            if (typeof(T) == typeof(TimeSpan))
            {
                code = RWTypeCode.TimeSpan;
                As(ref value.TimeSpan) = val;
                return;
            }

            code = RWTypeCode.Other;
            value.Other = val;

            static ref T As<TInput>(ref TInput input)
                => ref Unsafe.As<TInput, T>(ref input);
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ReadArray(IDataWriter<int> dataWriter)
        {
            switch (code)
            {
                case RWTypeCode.Null:

                    dataWriter.Content = null;

                    return;
                case RWTypeCode.Object:
                case RWTypeCode.Array:

                    var dataReader = value.DataReader;

                    if (dataWriter.TryCopyFromContent(dataReader))
                    {
                        return;
                    }

                    DataReader:

                    var count = dataReader.Count;

                    if (count >= 0)
                    {
                        dataWriter.Initialize(count);
                    }
                    else
                    {
                        dataWriter.Initialize();
                    }

                    dataReader.As<int>().OnReadAll(dataWriter);

                    return;
                case RWTypeCode.Other:

                    if (dataWriter.TrySetContent(value.Other))
                    {
                        return;
                    }

                    if (RWHelper.CreateReader(value.Other) is IDataReader otherDataReader)
                    {
                        dataReader = otherDataReader;

                        goto DataReader;
                    }

                    break;
            }

            if (dataWriter.ContentType is Type contentType)
            {
                dataWriter.Content = XConvert.Convert(InternalValue, contentType);
            }
            else
            {
                throw new NotSupportedException(/* TODO: 此数据写入器不支持设置数据源。 */);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void ReadObject(IDataWriter<string> dataWriter)
        {
            switch (code)
            {
                case RWTypeCode.Null:

                    dataWriter.Content = null;

                    return;
                case RWTypeCode.Object:
                case RWTypeCode.Array:

                    var dataReader = value.DataReader;

                    if (dataWriter.TryCopyFromContent(dataReader))
                    {
                        return;
                    }

                    DataReader:

                    var count = dataReader.Count;

                    if (count >= 0)
                    {
                        dataWriter.Initialize(count);
                    }
                    else
                    {
                        dataWriter.Initialize();
                    }

                    dataReader.As<string>().OnReadAll(dataWriter);

                    return;
                case RWTypeCode.Other:

                    if (dataWriter.TrySetContent(value.Other))
                    {
                        return;
                    }

                    if (RWHelper.CreateReader(value.Other) is IDataReader otherDataReader)
                    {
                        dataReader = otherDataReader;

                        goto DataReader;
                    }

                    break;
            }

            if (dataWriter.ContentType is Type contentType)
            {
                dataWriter.Content = XConvert.Convert(InternalValue, contentType);
            }
            else
            {
                throw new NotSupportedException(/* TODO: 此数据写入器不支持设置数据源。 */);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public bool ReadBoolean()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return Convert.ToBoolean(value.Boolean);
                case RWTypeCode.Byte: return Convert.ToBoolean(value.Byte);
                case RWTypeCode.SByte: return Convert.ToBoolean(value.SByte);
                case RWTypeCode.Char: return Convert.ToBoolean(value.Char);
                case RWTypeCode.Int16: return Convert.ToBoolean(value.Int16);
                case RWTypeCode.UInt16: return Convert.ToBoolean(value.UInt16);
                case RWTypeCode.Int32: return Convert.ToBoolean(value.Int32);
                case RWTypeCode.UInt32: return Convert.ToBoolean(value.UInt32);
                case RWTypeCode.Int64: return Convert.ToBoolean(value.Int64);
                case RWTypeCode.UInt64: return Convert.ToBoolean(value.UInt64);
                case RWTypeCode.Single: return Convert.ToBoolean(value.Single);
                case RWTypeCode.Double: return Convert.ToBoolean(value.Double);
                case RWTypeCode.Decimal: return Convert.ToBoolean(value.Decimal);
                case RWTypeCode.DateTime: return Convert.ToBoolean(value.DateTime);
                case RWTypeCode.DateTimeOffset: return Convert.ToBoolean(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return Convert.ToBoolean(value.TimeSpan);
                case RWTypeCode.Guid: return Convert.ToBoolean(value.Guid);
                case RWTypeCode.Enum: return Convert.ToBoolean(value.EnumInterface.Box(value.UInt64));
                case RWTypeCode.String: return Convert.ToBoolean(value.String);
                case RWTypeCode.Object: return Convert.ToBoolean(value.ObjectReader.Content);
                case RWTypeCode.Array: return Convert.ToBoolean(value.ArrayReader.Content);
                default: return Convert.ToBoolean(value.Other);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public byte ReadByte()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return Convert.ToByte(value.Boolean);
                case RWTypeCode.Byte: return Convert.ToByte(value.Byte);
                case RWTypeCode.SByte: return Convert.ToByte(value.SByte);
                case RWTypeCode.Char: return Convert.ToByte(value.Char);
                case RWTypeCode.Int16: return Convert.ToByte(value.Int16);
                case RWTypeCode.UInt16: return Convert.ToByte(value.UInt16);
                case RWTypeCode.Int32: return Convert.ToByte(value.Int32);
                case RWTypeCode.UInt32: return Convert.ToByte(value.UInt32);
                case RWTypeCode.Int64: return Convert.ToByte(value.Int64);
                case RWTypeCode.UInt64: return Convert.ToByte(value.UInt64);
                case RWTypeCode.Single: return Convert.ToByte(value.Single);
                case RWTypeCode.Double: return Convert.ToByte(value.Double);
                case RWTypeCode.Decimal: return Convert.ToByte(value.Decimal);
                case RWTypeCode.DateTime: return Convert.ToByte(value.DateTime);
                case RWTypeCode.DateTimeOffset: return Convert.ToByte(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return Convert.ToByte(value.TimeSpan);
                case RWTypeCode.Guid: return Convert.ToByte(value.Guid);
                case RWTypeCode.Enum: return Convert.ToByte(value.UInt64);
                case RWTypeCode.String: return Convert.ToByte(value.String);
                case RWTypeCode.Object: return Convert.ToByte(value.ObjectReader.Content);
                case RWTypeCode.Array: return Convert.ToByte(value.ArrayReader.Content);
                default: return Convert.ToByte(value.Other);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public char ReadChar()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return Convert.ToChar(value.Boolean);
                case RWTypeCode.Byte: return Convert.ToChar(value.Byte);
                case RWTypeCode.SByte: return Convert.ToChar(value.SByte);
                case RWTypeCode.Char: return Convert.ToChar(value.Char);
                case RWTypeCode.Int16: return Convert.ToChar(value.Int16);
                case RWTypeCode.UInt16: return Convert.ToChar(value.UInt16);
                case RWTypeCode.Int32: return Convert.ToChar(value.Int32);
                case RWTypeCode.UInt32: return Convert.ToChar(value.UInt32);
                case RWTypeCode.Int64: return Convert.ToChar(value.Int64);
                case RWTypeCode.UInt64: return Convert.ToChar(value.UInt64);
                case RWTypeCode.Single: return Convert.ToChar(value.Single);
                case RWTypeCode.Double: return Convert.ToChar(value.Double);
                case RWTypeCode.Decimal: return Convert.ToChar(value.Decimal);
                case RWTypeCode.DateTime: return Convert.ToChar(value.DateTime);
                case RWTypeCode.DateTimeOffset: return Convert.ToChar(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return Convert.ToChar(value.TimeSpan);
                case RWTypeCode.Guid: return Convert.ToChar(value.Guid);
                case RWTypeCode.Enum: return Convert.ToChar(value.EnumInterface.Box(value.UInt64));
                case RWTypeCode.String: return Convert.ToChar(value.String);
                case RWTypeCode.Object: return Convert.ToChar(value.ObjectReader.Content);
                case RWTypeCode.Array: return Convert.ToChar(value.ArrayReader.Content);
                default: return Convert.ToChar(value.Other);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public DateTime ReadDateTime()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return Convert.ToDateTime(value.Boolean);
                case RWTypeCode.Byte: return Convert.ToDateTime(value.Byte);
                case RWTypeCode.SByte: return Convert.ToDateTime(value.SByte);
                case RWTypeCode.Char: return Convert.ToDateTime(value.Char);
                case RWTypeCode.Int16: return Convert.ToDateTime(value.Int16);
                case RWTypeCode.UInt16: return Convert.ToDateTime(value.UInt16);
                case RWTypeCode.Int32: return Convert.ToDateTime(value.Int32);
                case RWTypeCode.UInt32: return Convert.ToDateTime(value.UInt32);
                case RWTypeCode.Int64: return Convert.ToDateTime(value.Int64);
                case RWTypeCode.UInt64: return Convert.ToDateTime(value.UInt64);
                case RWTypeCode.Single: return Convert.ToDateTime(value.Single);
                case RWTypeCode.Double: return Convert.ToDateTime(value.Double);
                case RWTypeCode.Decimal: return Convert.ToDateTime(value.Decimal);
                case RWTypeCode.DateTime: return Convert.ToDateTime(value.DateTime);
                case RWTypeCode.DateTimeOffset: return Convert.ToDateTime(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return Convert.ToDateTime(value.TimeSpan);
                case RWTypeCode.Guid: return Convert.ToDateTime(value.Guid);
                case RWTypeCode.Enum: return Convert.ToDateTime(value.EnumInterface.Box(value.UInt64));
                case RWTypeCode.String: return Convert.ToDateTime(value.String);
                case RWTypeCode.Object: return Convert.ToDateTime(value.ObjectReader.Content);
                case RWTypeCode.Array: return Convert.ToDateTime(value.ArrayReader.Content);
                default: return Convert.ToDateTime(value.Other);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public decimal ReadDecimal()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return Convert.ToDecimal(value.Boolean);
                case RWTypeCode.Byte: return Convert.ToDecimal(value.Byte);
                case RWTypeCode.SByte: return Convert.ToDecimal(value.SByte);
                case RWTypeCode.Char: return Convert.ToDecimal(value.Char);
                case RWTypeCode.Int16: return Convert.ToDecimal(value.Int16);
                case RWTypeCode.UInt16: return Convert.ToDecimal(value.UInt16);
                case RWTypeCode.Int32: return Convert.ToDecimal(value.Int32);
                case RWTypeCode.UInt32: return Convert.ToDecimal(value.UInt32);
                case RWTypeCode.Int64: return Convert.ToDecimal(value.Int64);
                case RWTypeCode.UInt64: return Convert.ToDecimal(value.UInt64);
                case RWTypeCode.Single: return Convert.ToDecimal(value.Single);
                case RWTypeCode.Double: return Convert.ToDecimal(value.Double);
                case RWTypeCode.Decimal: return Convert.ToDecimal(value.Decimal);
                case RWTypeCode.DateTime: return Convert.ToDecimal(value.DateTime);
                case RWTypeCode.DateTimeOffset: return Convert.ToDecimal(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return Convert.ToDecimal(value.TimeSpan);
                case RWTypeCode.Guid: return Convert.ToDecimal(value.Guid);
                case RWTypeCode.Enum: return Convert.ToDecimal(value.UInt64);
                case RWTypeCode.String: return Convert.ToDecimal(value.String);
                case RWTypeCode.Object: return Convert.ToDecimal(value.ObjectReader.Content);
                case RWTypeCode.Array: return Convert.ToDecimal(value.ArrayReader.Content);
                default: return Convert.ToDecimal(value.Other);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object? DirectRead()
        {
            switch (code)
            {
                case RWTypeCode.Null: return null;
                case RWTypeCode.Boolean: return value.Boolean;
                case RWTypeCode.Byte: return value.Byte;
                case RWTypeCode.SByte: return value.SByte;
                case RWTypeCode.Char: return value.Char;
                case RWTypeCode.Int16: return value.Int16;
                case RWTypeCode.UInt16: return value.UInt16;
                case RWTypeCode.Int32: return value.Int32;
                case RWTypeCode.UInt32: return value.UInt32;
                case RWTypeCode.Int64: return value.Int64;
                case RWTypeCode.UInt64: return value.UInt64;
                case RWTypeCode.Single: return value.Single;
                case RWTypeCode.Double: return value.Double;
                case RWTypeCode.Decimal: return value.Decimal;
                case RWTypeCode.DateTime: return value.DateTime;
                case RWTypeCode.DateTimeOffset: return value.DateTimeOffset;
                case RWTypeCode.TimeSpan: return value.TimeSpan;
                case RWTypeCode.Guid: return value.Guid;
                case RWTypeCode.Enum: return value.EnumInterface.Box(value.UInt64);
                case RWTypeCode.String: return value.String;
                case RWTypeCode.Object: return value.ObjectReader.Content;
                case RWTypeCode.Array: return value.ArrayReader.Content;
                default: return value.Other;
            }
        }

        /// <summary>
        /// 不做任何操作。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Pop()
        {

        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public double ReadDouble()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return Convert.ToDouble(value.Boolean);
                case RWTypeCode.Byte: return Convert.ToDouble(value.Byte);
                case RWTypeCode.SByte: return Convert.ToDouble(value.SByte);
                case RWTypeCode.Char: return Convert.ToDouble(value.Char);
                case RWTypeCode.Int16: return Convert.ToDouble(value.Int16);
                case RWTypeCode.UInt16: return Convert.ToDouble(value.UInt16);
                case RWTypeCode.Int32: return Convert.ToDouble(value.Int32);
                case RWTypeCode.UInt32: return Convert.ToDouble(value.UInt32);
                case RWTypeCode.Int64: return Convert.ToDouble(value.Int64);
                case RWTypeCode.UInt64: return Convert.ToDouble(value.UInt64);
                case RWTypeCode.Single: return Convert.ToDouble(value.Single);
                case RWTypeCode.Double: return Convert.ToDouble(value.Double);
                case RWTypeCode.Decimal: return Convert.ToDouble(value.Decimal);
                case RWTypeCode.DateTime: return Convert.ToDouble(value.DateTime);
                case RWTypeCode.DateTimeOffset: return Convert.ToDouble(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return Convert.ToDouble(value.TimeSpan);
                case RWTypeCode.Guid: return Convert.ToDouble(value.Guid);
                case RWTypeCode.Enum: return Convert.ToDouble(value.EnumInterface.Box(value.UInt64));
                case RWTypeCode.String: return Convert.ToDouble(value.String);
                case RWTypeCode.Object: return Convert.ToDouble(value.ObjectReader.Content);
                case RWTypeCode.Array: return Convert.ToDouble(value.ArrayReader.Content);
                default: return Convert.ToDouble(value.Other);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public short ReadInt16()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return Convert.ToInt16(value.Boolean);
                case RWTypeCode.Byte: return Convert.ToInt16(value.Byte);
                case RWTypeCode.SByte: return Convert.ToInt16(value.SByte);
                case RWTypeCode.Char: return Convert.ToInt16(value.Char);
                case RWTypeCode.Int16: return Convert.ToInt16(value.Int16);
                case RWTypeCode.UInt16: return Convert.ToInt16(value.UInt16);
                case RWTypeCode.Int32: return Convert.ToInt16(value.Int32);
                case RWTypeCode.UInt32: return Convert.ToInt16(value.UInt32);
                case RWTypeCode.Int64: return Convert.ToInt16(value.Int64);
                case RWTypeCode.UInt64: return Convert.ToInt16(value.UInt64);
                case RWTypeCode.Single: return Convert.ToInt16(value.Single);
                case RWTypeCode.Double: return Convert.ToInt16(value.Double);
                case RWTypeCode.Decimal: return Convert.ToInt16(value.Decimal);
                case RWTypeCode.DateTime: return Convert.ToInt16(value.DateTime);
                case RWTypeCode.DateTimeOffset: return Convert.ToInt16(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return Convert.ToInt16(value.TimeSpan);
                case RWTypeCode.Guid: return Convert.ToInt16(value.Guid);
                case RWTypeCode.Enum: return Convert.ToInt16(value.UInt64);
                case RWTypeCode.String: return Convert.ToInt16(value.String);
                case RWTypeCode.Object: return Convert.ToInt16(value.ObjectReader.Content);
                case RWTypeCode.Array: return Convert.ToInt16(value.ArrayReader.Content);
                default: return Convert.ToInt16(value.Other);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int ReadInt32()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return Convert.ToInt32(value.Boolean);
                case RWTypeCode.Byte: return Convert.ToInt32(value.Byte);
                case RWTypeCode.SByte: return Convert.ToInt32(value.SByte);
                case RWTypeCode.Char: return Convert.ToInt32(value.Char);
                case RWTypeCode.Int16: return Convert.ToInt32(value.Int16);
                case RWTypeCode.UInt16: return Convert.ToInt32(value.UInt16);
                case RWTypeCode.Int32: return Convert.ToInt32(value.Int32);
                case RWTypeCode.UInt32: return Convert.ToInt32(value.UInt32);
                case RWTypeCode.Int64: return Convert.ToInt32(value.Int64);
                case RWTypeCode.UInt64: return Convert.ToInt32(value.UInt64);
                case RWTypeCode.Single: return Convert.ToInt32(value.Single);
                case RWTypeCode.Double: return Convert.ToInt32(value.Double);
                case RWTypeCode.Decimal: return Convert.ToInt32(value.Decimal);
                case RWTypeCode.DateTime: return Convert.ToInt32(value.DateTime);
                case RWTypeCode.DateTimeOffset: return Convert.ToInt32(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return Convert.ToInt32(value.TimeSpan);
                case RWTypeCode.Guid: return Convert.ToInt32(value.Guid);
                case RWTypeCode.Enum: return Convert.ToInt32(value.UInt64);
                case RWTypeCode.String: return Convert.ToInt32(value.String);
                case RWTypeCode.Object: return Convert.ToInt32(value.ObjectReader.Content);
                case RWTypeCode.Array: return Convert.ToInt32(value.ArrayReader.Content);
                default: return Convert.ToInt32(value.Other);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public long ReadInt64()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return Convert.ToInt64(value.Boolean);
                case RWTypeCode.Byte: return Convert.ToInt64(value.Byte);
                case RWTypeCode.SByte: return Convert.ToInt64(value.SByte);
                case RWTypeCode.Char: return Convert.ToInt64(value.Char);
                case RWTypeCode.Int16: return Convert.ToInt64(value.Int16);
                case RWTypeCode.UInt16: return Convert.ToInt64(value.UInt16);
                case RWTypeCode.Int32: return Convert.ToInt64(value.Int32);
                case RWTypeCode.UInt32: return Convert.ToInt64(value.UInt32);
                case RWTypeCode.Int64: return Convert.ToInt64(value.Int64);
                case RWTypeCode.UInt64: return Convert.ToInt64(value.UInt64);
                case RWTypeCode.Single: return Convert.ToInt64(value.Single);
                case RWTypeCode.Double: return Convert.ToInt64(value.Double);
                case RWTypeCode.Decimal: return Convert.ToInt64(value.Decimal);
                case RWTypeCode.DateTime: return Convert.ToInt64(value.DateTime);
                case RWTypeCode.DateTimeOffset: return Convert.ToInt64(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return Convert.ToInt64(value.TimeSpan);
                case RWTypeCode.Guid: return Convert.ToInt64(value.Guid);
                case RWTypeCode.Enum: return Convert.ToInt64(value.UInt64);
                case RWTypeCode.String: return Convert.ToInt64(value.String);
                case RWTypeCode.Object: return Convert.ToInt64(value.ObjectReader.Content);
                case RWTypeCode.Array: return Convert.ToInt64(value.ArrayReader.Content);
                default: return Convert.ToInt64(value.Other);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public sbyte ReadSByte()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return Convert.ToSByte(value.Boolean);
                case RWTypeCode.Byte: return Convert.ToSByte(value.Byte);
                case RWTypeCode.SByte: return Convert.ToSByte(value.SByte);
                case RWTypeCode.Char: return Convert.ToSByte(value.Char);
                case RWTypeCode.Int16: return Convert.ToSByte(value.Int16);
                case RWTypeCode.UInt16: return Convert.ToSByte(value.UInt16);
                case RWTypeCode.Int32: return Convert.ToSByte(value.Int32);
                case RWTypeCode.UInt32: return Convert.ToSByte(value.UInt32);
                case RWTypeCode.Int64: return Convert.ToSByte(value.Int64);
                case RWTypeCode.UInt64: return Convert.ToSByte(value.UInt64);
                case RWTypeCode.Single: return Convert.ToSByte(value.Single);
                case RWTypeCode.Double: return Convert.ToSByte(value.Double);
                case RWTypeCode.Decimal: return Convert.ToSByte(value.Decimal);
                case RWTypeCode.DateTime: return Convert.ToSByte(value.DateTime);
                case RWTypeCode.DateTimeOffset: return Convert.ToSByte(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return Convert.ToSByte(value.TimeSpan);
                case RWTypeCode.Guid: return Convert.ToSByte(value.Guid);
                case RWTypeCode.Enum: return Convert.ToSByte(value.UInt64);
                case RWTypeCode.String: return Convert.ToSByte(value.String);
                case RWTypeCode.Object: return Convert.ToSByte(value.ObjectReader.Content);
                case RWTypeCode.Array: return Convert.ToSByte(value.ArrayReader.Content);
                default: return Convert.ToSByte(value.Other);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public float ReadSingle()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return Convert.ToSingle(value.Boolean);
                case RWTypeCode.Byte: return Convert.ToSingle(value.Byte);
                case RWTypeCode.SByte: return Convert.ToSingle(value.SByte);
                case RWTypeCode.Char: return Convert.ToSingle(value.Char);
                case RWTypeCode.Int16: return Convert.ToSingle(value.Int16);
                case RWTypeCode.UInt16: return Convert.ToSingle(value.UInt16);
                case RWTypeCode.Int32: return Convert.ToSingle(value.Int32);
                case RWTypeCode.UInt32: return Convert.ToSingle(value.UInt32);
                case RWTypeCode.Int64: return Convert.ToSingle(value.Int64);
                case RWTypeCode.UInt64: return Convert.ToSingle(value.UInt64);
                case RWTypeCode.Single: return Convert.ToSingle(value.Single);
                case RWTypeCode.Double: return Convert.ToSingle(value.Double);
                case RWTypeCode.Decimal: return Convert.ToSingle(value.Decimal);
                case RWTypeCode.DateTime: return Convert.ToSingle(value.DateTime);
                case RWTypeCode.DateTimeOffset: return Convert.ToSingle(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return Convert.ToSingle(value.TimeSpan);
                case RWTypeCode.Guid: return Convert.ToSingle(value.Guid);
                case RWTypeCode.Enum: return Convert.ToSingle(value.EnumInterface.Box(value.UInt64));
                case RWTypeCode.String: return Convert.ToSingle(value.String);
                case RWTypeCode.Object: return Convert.ToSingle(value.ObjectReader.Content);
                case RWTypeCode.Array: return Convert.ToSingle(value.ArrayReader.Content);
                default: return Convert.ToSingle(value.Other);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public string? ReadString()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return Convert.ToString(value.Boolean);
                case RWTypeCode.Byte: return Convert.ToString(value.Byte);
                case RWTypeCode.SByte: return Convert.ToString(value.SByte);
                case RWTypeCode.Char: return Convert.ToString(value.Char);
                case RWTypeCode.Int16: return Convert.ToString(value.Int16);
                case RWTypeCode.UInt16: return Convert.ToString(value.UInt16);
                case RWTypeCode.Int32: return Convert.ToString(value.Int32);
                case RWTypeCode.UInt32: return Convert.ToString(value.UInt32);
                case RWTypeCode.Int64: return Convert.ToString(value.Int64);
                case RWTypeCode.UInt64: return Convert.ToString(value.UInt64);
                case RWTypeCode.Single: return Convert.ToString(value.Single);
                case RWTypeCode.Double: return Convert.ToString(value.Double);
                case RWTypeCode.Decimal: return Convert.ToString(value.Decimal);
                case RWTypeCode.DateTime: return Convert.ToString(value.DateTime);
                case RWTypeCode.DateTimeOffset: return Convert.ToString(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return Convert.ToString(value.TimeSpan);
                case RWTypeCode.Guid: return Convert.ToString(value.Guid);
                case RWTypeCode.Enum: return Convert.ToString(value.EnumInterface.Box(value.UInt64));
                case RWTypeCode.String: return Convert.ToString(value.String);
                case RWTypeCode.Object: return Convert.ToString(value.ObjectReader.Content);
                case RWTypeCode.Array: return Convert.ToString(value.ArrayReader.Content);
                default: return Convert.ToString(value.Other);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ushort ReadUInt16()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return Convert.ToUInt16(value.Boolean);
                case RWTypeCode.Byte: return Convert.ToUInt16(value.Byte);
                case RWTypeCode.SByte: return Convert.ToUInt16(value.SByte);
                case RWTypeCode.Char: return Convert.ToUInt16(value.Char);
                case RWTypeCode.Int16: return Convert.ToUInt16(value.Int16);
                case RWTypeCode.UInt16: return Convert.ToUInt16(value.UInt16);
                case RWTypeCode.Int32: return Convert.ToUInt16(value.Int32);
                case RWTypeCode.UInt32: return Convert.ToUInt16(value.UInt32);
                case RWTypeCode.Int64: return Convert.ToUInt16(value.Int64);
                case RWTypeCode.UInt64: return Convert.ToUInt16(value.UInt64);
                case RWTypeCode.Single: return Convert.ToUInt16(value.Single);
                case RWTypeCode.Double: return Convert.ToUInt16(value.Double);
                case RWTypeCode.Decimal: return Convert.ToUInt16(value.Decimal);
                case RWTypeCode.DateTime: return Convert.ToUInt16(value.DateTime);
                case RWTypeCode.DateTimeOffset: return Convert.ToUInt16(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return Convert.ToUInt16(value.TimeSpan);
                case RWTypeCode.Guid: return Convert.ToUInt16(value.Guid);
                case RWTypeCode.Enum: return Convert.ToUInt16(value.UInt64);
                case RWTypeCode.String: return Convert.ToUInt16(value.String);
                case RWTypeCode.Object: return Convert.ToUInt16(value.ObjectReader.Content);
                case RWTypeCode.Array: return Convert.ToUInt16(value.ArrayReader.Content);
                default: return Convert.ToUInt16(value.Other);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public uint ReadUInt32()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return Convert.ToUInt32(value.Boolean);
                case RWTypeCode.Byte: return Convert.ToUInt32(value.Byte);
                case RWTypeCode.SByte: return Convert.ToUInt32(value.SByte);
                case RWTypeCode.Char: return Convert.ToUInt32(value.Char);
                case RWTypeCode.Int16: return Convert.ToUInt32(value.Int16);
                case RWTypeCode.UInt16: return Convert.ToUInt32(value.UInt16);
                case RWTypeCode.Int32: return Convert.ToUInt32(value.Int32);
                case RWTypeCode.UInt32: return Convert.ToUInt32(value.UInt32);
                case RWTypeCode.Int64: return Convert.ToUInt32(value.Int64);
                case RWTypeCode.UInt64: return Convert.ToUInt32(value.UInt64);
                case RWTypeCode.Single: return Convert.ToUInt32(value.Single);
                case RWTypeCode.Double: return Convert.ToUInt32(value.Double);
                case RWTypeCode.Decimal: return Convert.ToUInt32(value.Decimal);
                case RWTypeCode.DateTime: return Convert.ToUInt32(value.DateTime);
                case RWTypeCode.DateTimeOffset: return Convert.ToUInt32(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return Convert.ToUInt32(value.TimeSpan);
                case RWTypeCode.Guid: return Convert.ToUInt32(value.Guid);
                case RWTypeCode.Enum: return Convert.ToUInt32(value.UInt64);
                case RWTypeCode.String: return Convert.ToUInt32(value.String);
                case RWTypeCode.Object: return Convert.ToUInt32(value.ObjectReader.Content);
                case RWTypeCode.Array: return Convert.ToUInt32(value.ArrayReader.Content);
                default: return Convert.ToUInt32(value.Other);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ulong ReadUInt64()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return Convert.ToUInt64(value.Boolean);
                case RWTypeCode.Byte: return Convert.ToUInt64(value.Byte);
                case RWTypeCode.SByte: return Convert.ToUInt64(value.SByte);
                case RWTypeCode.Char: return Convert.ToUInt64(value.Char);
                case RWTypeCode.Int16: return Convert.ToUInt64(value.Int16);
                case RWTypeCode.UInt16: return Convert.ToUInt64(value.UInt16);
                case RWTypeCode.Int32: return Convert.ToUInt64(value.Int32);
                case RWTypeCode.UInt32: return Convert.ToUInt64(value.UInt32);
                case RWTypeCode.Int64: return Convert.ToUInt64(value.Int64);
                case RWTypeCode.UInt64: return Convert.ToUInt64(value.UInt64);
                case RWTypeCode.Single: return Convert.ToUInt64(value.Single);
                case RWTypeCode.Double: return Convert.ToUInt64(value.Double);
                case RWTypeCode.Decimal: return Convert.ToUInt64(value.Decimal);
                case RWTypeCode.DateTime: return Convert.ToUInt64(value.DateTime);
                case RWTypeCode.DateTimeOffset: return Convert.ToUInt64(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return Convert.ToUInt64(value.TimeSpan);
                case RWTypeCode.Guid: return Convert.ToUInt64(value.Guid);
                case RWTypeCode.Enum: return Convert.ToUInt64(value.UInt64);
                case RWTypeCode.String: return Convert.ToUInt64(value.String);
                case RWTypeCode.Object: return Convert.ToUInt64(value.ObjectReader.Content);
                case RWTypeCode.Array: return Convert.ToUInt64(value.ArrayReader.Content);
                default: return Convert.ToUInt64(value.Other);
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T ReadEnum<T>() where T : struct, Enum
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Byte: return EnumHelper.AsEnum<T>((ulong)value.Byte);
                case RWTypeCode.SByte: return EnumHelper.AsEnum<T>((ulong)value.SByte);
                case RWTypeCode.Char: return EnumHelper.AsEnum<T>((ulong)value.Char);
                case RWTypeCode.Int16: return EnumHelper.AsEnum<T>((ulong)value.Int16);
                case RWTypeCode.UInt16: return EnumHelper.AsEnum<T>((ulong)value.UInt16);
                case RWTypeCode.Int32: return EnumHelper.AsEnum<T>((ulong)value.Int32);
                case RWTypeCode.UInt32: return EnumHelper.AsEnum<T>((ulong)value.UInt32);
                case RWTypeCode.Int64: return EnumHelper.AsEnum<T>((ulong)value.Int64);
                case RWTypeCode.UInt64: return EnumHelper.AsEnum<T>((ulong)value.UInt64);
                case RWTypeCode.Enum: return EnumHelper.AsEnum<T>((ulong)value.UInt64);
                case RWTypeCode.String: return EnumHelper.TryParseEnum<T>(value.String, out var ret) ? ret : (T)Enum.Parse(typeof(T), value.String);
                case RWTypeCode.Single: return XConvert(value.Single);
                case RWTypeCode.Double: return XConvert(value.Double);
                case RWTypeCode.Decimal: return XConvert(value.Decimal);
                case RWTypeCode.DateTime: return XConvert(value.DateTime);
                case RWTypeCode.DateTimeOffset: return XConvert(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return XConvert(value.TimeSpan);
                case RWTypeCode.Guid: return XConvert(value.Guid);
                case RWTypeCode.Object: return XConvert(value.ObjectReader.Content);
                case RWTypeCode.Array: return XConvert(value.ArrayReader.Content);
                default: return value.Other is T t ? t : XConvert(value.Other);
            }

            static T XConvert<TInput>(TInput input)
                => Tools.XConvert.Convert<TInput, T>(input);
        }

        /// <inheritdoc/>
        public Guid ReadGuid()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return XConvert(value.Boolean);
                case RWTypeCode.Byte: return XConvert(value.Byte);
                case RWTypeCode.SByte: return XConvert(value.SByte);
                case RWTypeCode.Char: return XConvert(value.Char);
                case RWTypeCode.Int16: return XConvert(value.Int16);
                case RWTypeCode.UInt16: return XConvert(value.UInt16);
                case RWTypeCode.Int32: return XConvert(value.Int32);
                case RWTypeCode.UInt32: return XConvert(value.UInt32);
                case RWTypeCode.Int64: return XConvert(value.Int64);
                case RWTypeCode.UInt64: return XConvert(value.UInt64);
                case RWTypeCode.Single: return XConvert(value.Single);
                case RWTypeCode.Double: return XConvert(value.Double);
                case RWTypeCode.Decimal: return XConvert(value.Decimal);
                case RWTypeCode.DateTime: return XConvert(value.DateTime);
                case RWTypeCode.DateTimeOffset: return XConvert(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return XConvert(value.TimeSpan);
                case RWTypeCode.Guid: return value.Guid;
                case RWTypeCode.Enum: return XConvert(value.EnumInterface.Box(value.UInt64));
                case RWTypeCode.String: return GuidHelper.TryParseGuid(value.String, out var ret) ? ret : new Guid(value.String);
                case RWTypeCode.Object: return XConvert(value.ObjectReader.Content);
                case RWTypeCode.Array: return XConvert(value.ArrayReader.Content);
                default: return XConvert(value.Other);
            }

            static Guid XConvert<TInput>(TInput input)
                => Tools.XConvert.Convert<TInput, Guid>(input);
        }

        /// <inheritdoc/>
        public DateTimeOffset ReadDateTimeOffset()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return XConvert(value.Boolean);
                case RWTypeCode.Byte: return XConvert(value.Byte);
                case RWTypeCode.SByte: return XConvert(value.SByte);
                case RWTypeCode.Char: return XConvert(value.Char);
                case RWTypeCode.Int16: return XConvert(value.Int16);
                case RWTypeCode.UInt16: return XConvert(value.UInt16);
                case RWTypeCode.Int32: return XConvert(value.Int32);
                case RWTypeCode.UInt32: return XConvert(value.UInt32);
                case RWTypeCode.Int64: return XConvert(value.Int64);
                case RWTypeCode.UInt64: return XConvert(value.UInt64);
                case RWTypeCode.Single: return XConvert(value.Single);
                case RWTypeCode.Double: return XConvert(value.Double);
                case RWTypeCode.Decimal: return XConvert(value.Decimal);
                case RWTypeCode.DateTime: return value.DateTime;
                case RWTypeCode.DateTimeOffset: return value.DateTimeOffset;
                case RWTypeCode.TimeSpan: return XConvert(value.TimeSpan);
                case RWTypeCode.Guid: return XConvert(value.Guid);
                case RWTypeCode.Enum: return XConvert(value.EnumInterface.Box(value.UInt64));
                case RWTypeCode.String: return DateTimeOffset.Parse(value.String);
                case RWTypeCode.Object: return XConvert(value.ObjectReader.Content);
                case RWTypeCode.Array: return XConvert(value.ArrayReader.Content);
                default: return XConvert(value.Other);
            }

            static DateTimeOffset XConvert<TInput>(TInput input)
                => Tools.XConvert.Convert<TInput, DateTimeOffset>(input);
        }

        /// <inheritdoc/>
        public TimeSpan ReadTimeSpan()
        {
            switch (code)
            {
                case RWTypeCode.Null: return default;
                case RWTypeCode.Boolean: return XConvert(value.Boolean);
                case RWTypeCode.Byte: return XConvert(value.Byte);
                case RWTypeCode.SByte: return XConvert(value.SByte);
                case RWTypeCode.Char: return XConvert(value.Char);
                case RWTypeCode.Int16: return XConvert(value.Int16);
                case RWTypeCode.UInt16: return XConvert(value.UInt16);
                case RWTypeCode.Int32: return XConvert(value.Int32);
                case RWTypeCode.UInt32: return XConvert(value.UInt32);
                case RWTypeCode.Int64: return XConvert(value.Int64);
                case RWTypeCode.UInt64: return XConvert(value.UInt64);
                case RWTypeCode.Single: return XConvert(value.Single);
                case RWTypeCode.Double: return XConvert(value.Double);
                case RWTypeCode.Decimal: return XConvert(value.Decimal);
                case RWTypeCode.DateTime: return XConvert(value.DateTime);
                case RWTypeCode.DateTimeOffset: return XConvert(value.DateTimeOffset);
                case RWTypeCode.TimeSpan: return value.TimeSpan;
                case RWTypeCode.Guid: return XConvert(value.Guid);
                case RWTypeCode.Enum: return XConvert(value.EnumInterface.Box(value.UInt64));
                case RWTypeCode.String: return TimeSpan.Parse(value.String);
                case RWTypeCode.Object: return XConvert(value.ObjectReader.Content);
                case RWTypeCode.Array: return XConvert(value.ArrayReader.Content);
                default: return XConvert(value.Other);
            }

            static TimeSpan XConvert<TInput>(TInput input)
                => Tools.XConvert.Convert<TInput, TimeSpan>(input);
        }

        /// <summary>
        /// 读取一个已知类型的值。
        /// </summary>
        public T? ReadValue<T>()
        {
            if (typeof(T) == typeof(int)) return As(ReadInt32());
            if (typeof(T) == typeof(string)) return As(ReadString());
            if (typeof(T) == typeof(double)) return As(ReadDouble());
            if (typeof(T) == typeof(bool)) return As(ReadBoolean());
            if (typeof(T) == typeof(byte)) return As(ReadByte());
            if (typeof(T) == typeof(sbyte)) return As(ReadSByte());
            if (typeof(T) == typeof(short)) return As(ReadInt16());
            if (typeof(T) == typeof(ushort)) return As(ReadUInt16());
            if (typeof(T) == typeof(uint)) return As(ReadUInt32());
            if (typeof(T) == typeof(long)) return As(ReadInt64());
            if (typeof(T) == typeof(ulong)) return As(ReadUInt64());
            if (typeof(T) == typeof(float)) return As(ReadSingle());
            if (typeof(T) == typeof(char)) return As(ReadChar());
            if (typeof(T) == typeof(decimal)) return As(ReadDecimal());
            if (typeof(T) == typeof(DateTime)) return As(ReadDateTime());
            if (typeof(T) == typeof(Guid)) return As(ReadGuid());
            if (typeof(T) == typeof(DateTimeOffset)) return As(ReadDateTimeOffset());
            if (typeof(T) == typeof(TimeSpan)) return As(ReadTimeSpan());
            if (typeof(T) == typeof(object)) return As(DirectRead());

            // TODO: Enum

            return ValueInterface<T>.ReadValue(this);

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            static T As<TInput>(TInput input)
                => Unsafe.As<TInput, T>(ref input);
        }

        /// <inheritdoc/>
        public T? ReadNullable<T>() where T : struct
        {
            if (code is RWTypeCode.Null)
            {
                return null;
            }

            return ReadValue<T>();
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
                case RWTypeCode.Null:
                    valueWriter.DirectWrite(null);
                    break;
                case RWTypeCode.Boolean:
                    valueWriter.WriteBoolean(value.Boolean);
                    break;
                case RWTypeCode.Byte:
                    valueWriter.WriteByte(value.Byte);
                    break;
                case RWTypeCode.SByte:
                    valueWriter.WriteSByte(value.SByte);
                    break;
                case RWTypeCode.Char:
                    valueWriter.WriteChar(value.Char);
                    break;
                case RWTypeCode.Int16:
                    valueWriter.WriteInt16(value.Int16);
                    break;
                case RWTypeCode.UInt16:
                    valueWriter.WriteUInt16(value.UInt16);
                    break;
                case RWTypeCode.Int32:
                    valueWriter.WriteInt32(value.Int32);
                    break;
                case RWTypeCode.UInt32:
                    valueWriter.WriteUInt32(value.UInt32);
                    break;
                case RWTypeCode.Int64:
                    valueWriter.WriteInt64(value.Int64);
                    break;
                case RWTypeCode.UInt64:
                    valueWriter.WriteUInt64(value.UInt64);
                    break;
                case RWTypeCode.Single:
                    valueWriter.WriteSingle(value.Single);
                    break;
                case RWTypeCode.Double:
                    valueWriter.WriteDouble(value.Double);
                    break;
                case RWTypeCode.Decimal:
                    valueWriter.WriteDecimal(value.Decimal);
                    break;
                case RWTypeCode.DateTime:
                    valueWriter.WriteDateTime(value.DateTime);
                    break;
                case RWTypeCode.DateTimeOffset:
                    valueWriter.WriteDateTimeOffset(value.DateTimeOffset);
                    break;
                case RWTypeCode.TimeSpan:
                    valueWriter.WriteTimeSpan(value.TimeSpan);
                    break;
                case RWTypeCode.Guid:
                    valueWriter.WriteGuid(value.Guid);
                    break;
                case RWTypeCode.String:
                    valueWriter.WriteString(value.String);
                    break;
                case RWTypeCode.Enum:
                    value.EnumInterface.WriteEnum(valueWriter, value.UInt64);
                    break;
                case RWTypeCode.Object:
                    valueWriter.WriteObject(value.ObjectReader);
                    break;
                case RWTypeCode.Array:
                    valueWriter.WriteArray(value.ArrayReader);
                    break;
                default:
                    valueWriter.DirectWrite(value.Other);
                    break;
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteArray(IDataReader<int> dataReader)
        {
            code = RWTypeCode.Array;

            value.ArrayReader = dataReader;
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteBoolean(bool value)
        {
            code = RWTypeCode.Boolean;

            this.value.Boolean = value;
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteByte(byte value)
        {
            code = RWTypeCode.Byte;

            this.value.Byte = value;
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteChar(char value)
        {
            code = RWTypeCode.Char;

            this.value.Char = value;
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteDateTime(DateTime value)
        {
            code = RWTypeCode.DateTime;

            this.value.DateTime = value;
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteDecimal(decimal value)
        {
            code = RWTypeCode.Decimal;

            this.value.Decimal = value;
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void DirectWrite(object? value)
        {
            if (value is null)
            {
                code = RWTypeCode.Null;

                this.value.Other = null!;
            }
            else
            {
                code = RWTypeCode.Other;

                this.value.Other = value;
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteDouble(double value)
        {
            code = RWTypeCode.Double;

            this.value.Double = value;
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt16(short value)
        {
            code = RWTypeCode.Int16;

            this.value.Int16 = value;
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt32(int value)
        {
            code = RWTypeCode.Int32;

            this.value.Int32 = value;
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteInt64(long value)
        {
            code = RWTypeCode.Int64;

            this.value.Int64 = value;
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteObject(IDataReader<string> dataReader)
        {
            code = RWTypeCode.Object;

            value.ObjectReader = dataReader;
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteSByte(sbyte value)
        {
            code = RWTypeCode.SByte;

            this.value.SByte = value;
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteSingle(float value)
        {
            code = RWTypeCode.Single;

            this.value.Single = value;
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteString(string? value)
        {
            if (value is null)
            {
                code = RWTypeCode.Null;

                this.value.String = null!;
            }
            else
            {
                code = RWTypeCode.String;

                this.value.String = value;
            }
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt16(ushort value)
        {
            code = RWTypeCode.UInt16;

            this.value.UInt16 = value;
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt32(uint value)
        {
            code = RWTypeCode.UInt32;

            this.value.UInt32 = value;
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteUInt64(ulong value)
        {
            code = RWTypeCode.UInt64;

            this.value.UInt64 = value;
        }

        /// <inheritdoc/>
        public void WriteDateTimeOffset(DateTimeOffset value)
        {
            code = RWTypeCode.DateTimeOffset;

            this.value.DateTimeOffset = value;
        }

        /// <inheritdoc/>
        public void WriteTimeSpan(TimeSpan value)
        {
            code = RWTypeCode.TimeSpan;

            this.value.TimeSpan = value;
        }

        /// <inheritdoc/>
        public void WriteGuid(Guid value)
        {
            code = RWTypeCode.Guid;

            this.value.Guid = value;
        }

        /// <inheritdoc/>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void WriteEnum<T>(T value) where T : struct, Enum
        {
            code = RWTypeCode.Enum;

            this.value.UInt64 = EnumHelper.AsUInt64(value);
            this.value.EnumInterface = EnumInterface.GetInstance<T>();
        }

        sealed class ValueInterface : IValueInterface<ValueCopyer>
        {
            public ValueCopyer? ReadValue(IValueReader valueReader)
            {
                if (valueReader is IValueReader<ValueCopyer> reader)
                {
                    return reader.ReadValue();
                }

                return ValueCopyer.ValueOf(valueReader.DirectRead());
            }

            public void WriteValue(IValueWriter valueWriter, ValueCopyer? value)
            {
                if (value is null)
                {
                    valueWriter.DirectWrite(null);
                }
                else
                {
                    value.WriteTo(valueWriter);
                }
            }
        }

        [StructLayout(LayoutKind.Explicit)]
        struct RWOverlappedValue
        {
            [FieldOffset(0)]
            public bool Boolean;
            [FieldOffset(0)]
            public sbyte SByte;
            [FieldOffset(0)]
            public short Int16;
            [FieldOffset(0)]
            public int Int32;
            [FieldOffset(0)]
            public long Int64;
            [FieldOffset(0)]
            public byte Byte;
            [FieldOffset(0)]
            public ushort UInt16;
            [FieldOffset(0)]
            public uint UInt32;
            [FieldOffset(0)]
            public ulong UInt64;
            [FieldOffset(0)]
            public float Single;
            [FieldOffset(0)]
            public double Double;
            [FieldOffset(0)]
            public decimal Decimal;
            [FieldOffset(0)]
            public char Char;
            [FieldOffset(0)]
            public DateTime DateTime;
            [FieldOffset(0)]
            public DateTimeOffset DateTimeOffset;
            [FieldOffset(0)]
            public TimeSpan TimeSpan;
            [FieldOffset(0)]
            public Guid Guid;
            [FieldOffset(16)]
            public object Other;
            [FieldOffset(16)]
            public string String;
            [FieldOffset(16)]
            public IDataReader<string> ObjectReader;
            [FieldOffset(16)]
            public IDataReader<int> ArrayReader;
            [FieldOffset(16)]
            public IDataReader DataReader;
            [FieldOffset(16)]
            public EnumInterface EnumInterface;
        }
    }
}