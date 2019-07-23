using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace Swifter.Tools
{
    /// <summary>
    /// 一个重叠的指针类型集合。
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct OverlappedPointer
    {
        [FieldOffset(0)]
        public void* VoidPtr;
        [FieldOffset(0)]
        public byte* BytePtr;
        [FieldOffset(0)]
        public sbyte* SBytePtr;
        [FieldOffset(0)]
        public short* Int16Ptr;
        [FieldOffset(0)]
        public ushort* UInt16Ptr;
        [FieldOffset(0)]
        public int* Int32Ptr;
        [FieldOffset(0)]
        public uint* UInt32Ptr;
        [FieldOffset(0)]
        public long* Int64Ptr;
        [FieldOffset(0)]
        public ulong* UInt64Ptr;
        [FieldOffset(0)]
        public float* SinglePtr;
        [FieldOffset(0)]
        public double* DoublePtr;
        [FieldOffset(0)]
        public decimal* DecimalPtr;
        [FieldOffset(0)]
        public char* CharPtr;
        [FieldOffset(0)]
        public IntPtr IntPtr;
        [FieldOffset(0)]
        public UIntPtr UIntPtr;
    }
}
