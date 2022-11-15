#if !(NET45_OR_GREATER || NETSTANDARD2_0_OR_GREATER || NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER)

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