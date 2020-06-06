using Swifter.RW;
using System;
using System.Runtime.Serialization;
using SystemConvert = System.Convert;
namespace Swifter.Reflection
{
    sealed class ValueInterfaceFormatterConverter : IFormatterConverter
    {
        public object Convert(object value, Type type)
            => ValueInterface.ReadValue(ValueCopyer.ValueOf(value), type);

        public object Convert(object value, TypeCode typeCode)
            => SystemConvert.ChangeType(value, typeCode);

        public bool ToBoolean(object value)
            => SystemConvert.ToBoolean(value);

        public byte ToByte(object value)
            => SystemConvert.ToByte(value);

        public char ToChar(object value)
            => SystemConvert.ToChar(value);

        public DateTime ToDateTime(object value)
            => SystemConvert.ToDateTime(value);

        public decimal ToDecimal(object value)
            => SystemConvert.ToDecimal(value);

        public double ToDouble(object value)
            => SystemConvert.ToDouble(value);

        public short ToInt16(object value)
            => SystemConvert.ToInt16(value);

        public int ToInt32(object value)
            => SystemConvert.ToInt32(value);

        public long ToInt64(object value)
            => SystemConvert.ToInt64(value);

        public sbyte ToSByte(object value)
            => SystemConvert.ToSByte(value);

        public float ToSingle(object value)
            => SystemConvert.ToSingle(value);

        public string ToString(object value)
            => SystemConvert.ToString(value);

        public ushort ToUInt16(object value)
            => SystemConvert.ToUInt16(value);

        public uint ToUInt32(object value)
            => SystemConvert.ToUInt32(value);

        public ulong ToUInt64(object value) 
            => SystemConvert.ToUInt64(value);
    }
}