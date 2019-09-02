
using System;

namespace Swifter.RW
{
    /// <summary>
    /// 基础类型枚举，此枚举不能按位合并值。
    /// </summary>
    public enum ValueTypeCodes : byte
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