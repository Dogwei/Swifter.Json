using Swifter.Readers;
using System;
using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 一个重叠的基础类型集合。
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal struct OverlappedValue
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
        [FieldOffset(16)]
        public object Object;
        [FieldOffset(16)]
        public string String;
        [FieldOffset(16)]
        public IDataReader DataReader;
    }
}
