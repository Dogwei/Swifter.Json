#if NET20 || NET35 || NET40 || NET45 || NET47 || NETSTANDARD2_0 || NETCOREAPP2_0

using Swifter;
using System.Runtime.CompilerServices;

#pragma warning disable 1591

namespace System.Buffers.Binary
{
    public static class BinaryPrimitives
    {
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ushort ReverseEndianness(ushort value) => (ushort)((value >> 8) + (value << 8));

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static uint ReverseEndianness(uint value) => RotateRight(value & 0x00FF00FFu, 8) + RotateLeft(value & 0xFF00FF00u, 8);

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ulong ReverseEndianness(ulong value) => ((ulong)ReverseEndianness((uint)value) << 32) + ReverseEndianness((uint)(value >> 32));

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static uint RotateRight(uint value, int offset) => (value >> offset) | (value << (32 - offset));

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static uint RotateLeft(uint value, int offset) => (value << offset) | (value >> (32 - offset));
    }
}

#endif