
using Swifter.RW;

using System;
using System.Collections.Generic;

using ArrayType = System.Collections.Generic.List<object>;
using ObjectType = System.Collections.Generic.Dictionary<string, object>;

namespace Swifter.Json
{
    /// <summary>
    /// 表示一个 Json 值。
    /// </summary>
    public sealed partial class JsonValue
    {
        private readonly object value;

        private JsonValue(object value)
        {
            this.value = value;
        }

        /// <summary>
        /// 获取这个 Json 值是否为数组。
        /// </summary>
        public bool IsArray => value is ArrayType;

        /// <summary>
        /// 获取这个 Json 值是否为对象。
        /// </summary>
        public bool IsObject => value is ObjectType;

        /// <summary>
        /// 获取这个 Json 值是否是一个基础值（数字，字符串，布尔，Null）。
        /// </summary>
        public bool IsValue => !IsArray && !IsObject;

        private ObjectType Object 
            => value as ObjectType ?? throw new InvalidOperationException("this value is not a object.");

        private ArrayType Array 
            => value as ArrayType ?? throw new InvalidOperationException("this value is not a array.");

        /// <summary>
        /// 获取这个 Json 对象的字段数量。
        /// </summary>
        public int ObjectKeysCount => Object.Count;

        /// <summary>
        /// 获取这个 Json 对象的字段集合。
        /// </summary>
        public IEnumerable<string> ObjectKeys => Object.Keys;

        /// <summary>
        /// 获取这个 Json 对象的键值对集合。
        /// </summary>
        public IEnumerable<KeyValuePair<string, object>> ObjectItems => Object;

        /// <summary>
        /// 获取这个 Json 数组的长度。
        /// </summary>
        public int ArrayLength => Array.Count;

        /// <summary>
        /// 获取这个基础值。
        /// </summary>
        public object Value => IsValue ? value : throw new InvalidOperationException("this value is not a basic simple value.");

        /// <summary>
        /// 获取这个 Json 对象中指定字段名称的 Json 值，没有该字段则返回 Null。
        /// </summary>
        /// <param name="name">指定字段名称</param>
        /// <returns>返回一个 Json 值</returns>
        public JsonValue this[string name] => Object.TryGetValue(name, out var value) ? new JsonValue(value) : null;

        /// <summary>
        /// 获取这个 Json 数组中指定索引处的 Json 值。超出索引将发生异常。
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <returns>返回一个 Json 值</returns>
        public JsonValue this[int index] => new JsonValue(Array[index]);

        /// <summary>
        /// 获取这个 Json 值的布尔形式值。
        /// </summary>
        public bool BooleanValue => Convert.ToBoolean(Value);

        /// <summary>
        /// 获取这个 Json 值的字符形式值。
        /// </summary>
        public char CharValue => Convert.ToChar(Value);

        /// <summary>
        /// 获取这个 Json 值的有符号字节形式值。
        /// </summary>
        public sbyte SByteValue => Convert.ToSByte(Value);

        /// <summary>
        /// 获取这个 Json 值的无符号字节形式值。
        /// </summary>
        public byte ByteValue => Convert.ToByte(Value);

        /// <summary>
        /// 获取这个 Json 值的 16 位有符号整数形式值。
        /// </summary>
        public short Int16Value => Convert.ToInt16(Value);

        /// <summary>
        /// 获取这个 Json 值的 16 位无符号整数形式值。
        /// </summary>
        public ushort UInt16Value => Convert.ToUInt16(Value);

        /// <summary>
        /// 获取这个 Json 值的 32 位有符号整数形式值。
        /// </summary>
        public int Int32Value => Convert.ToInt32(Value);

        /// <summary>
        /// 获取这个 Json 值的 32 位无符号整数形式值。
        /// </summary>
        public uint UInt32Value => Convert.ToUInt32(Value);

        /// <summary>
        /// 获取这个 Json 值的 64 位有符号整数形式值。
        /// </summary>
        public long Int64Value => Convert.ToInt64(Value);

        /// <summary>
        /// 获取这个 Json 值的 64 位无符号整数形式值。
        /// </summary>
        public ulong UInt64Value => Convert.ToUInt64(Value);

        /// <summary>
        /// 获取这个 Json 值的单精度浮点数形式值。
        /// </summary>
        public float SingleValue => Convert.ToSingle(Value);

        /// <summary>
        /// 获取这个 Json 值的双精度浮点数形式值。
        /// </summary>
        public double DoubleValue => Convert.ToDouble(Value);

        /// <summary>
        /// 获取这个 Json 值的十进制数字形式值。
        /// </summary>
        public decimal DecimalValue => Convert.ToDecimal(Value);

        /// <summary>
        /// 获取这个 Json 值的日期形式值。
        /// </summary>
        public DateTime DateTimeValue => Convert.ToDateTime(Value);

        /// <summary>
        /// 获取这个 Json 值的字符串形式值。
        /// </summary>
        public string StringValue => Convert.ToString(Value);

        /// <summary>
        /// 获取这个值的 Json 字符串。
        /// </summary>
        /// <returns></returns>
        public override string ToString() => JsonFormatter.SerializeObject(value);


        sealed class ValueInterface : IValueInterface<JsonValue>
        {
            public JsonValue ReadValue(IValueReader valueReader)
            {
                return new JsonValue(valueReader.DirectRead());
            }

            public void WriteValue(IValueWriter valueWriter, JsonValue value)
            {
                RW.ValueInterface.WriteValue(valueWriter, value.value);
            }
        }
    }
}