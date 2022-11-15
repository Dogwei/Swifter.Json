
#if NET20 || NET35 || NET40

#pragma warning disable 1591

using InlineIL;
using Swifter;
using System;
using System.Diagnostics.CodeAnalysis;
using static InlineIL.IL;
using static InlineIL.IL.Emit;

namespace System.Runtime.CompilerServices
{
    public static unsafe class Unsafe
    {
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T Read<T>(void* source)
        {
            Ldarg_0();
            Ldobj<T>();

            return Return<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T ReadUnaligned<T>(void* source)
        {
            Ldarg_0();
            Unaligned(1);
            Ldobj<T>();

            return Return<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T ReadUnaligned<T>(ref byte source)
        {
            Ldarg_0();
            Unaligned(1);
            Ldobj<T>();

            return Return<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Write<T>(void* source, T value)
        {
            Ldarg_0();
            Ldarg_1();
            Stobj<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void WriteUnaligned<T>(void* source, T value)
        {
            Ldarg_0();
            Ldarg_1();
            Unaligned(1);
            Stobj<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void WriteUnaligned<T>(ref byte source, T value)
        {
            Ldarg_0();
            Ldarg_1();
            Unaligned(1);
            Stobj<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy<T>(void* destination, ref T source)
        {
            Ldarg_0();
            Ldarg_1();
            Ldobj<T>();
            Stobj<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void Copy<T>(ref T destination, void* source)
        {
            Ldarg_0();
            Ldarg_1();
            Ldobj<T>();
            Stobj<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void* AsPointer<T>(ref T value)
        {
            Ldarg_0();
            Conv_U();

            return ReturnPointer();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int SizeOf<T>()
        {
            Emit.Sizeof<T>();

            return Return<int>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void CopyBlock(ref byte destination, ref byte source, uint byteCount)
        {
            Ldarg_0();
            Ldarg_1();
            Ldarg_2();
            Cpblk();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void CopyBlock(void* destination, void* source, uint byteCount)
        {
            Ldarg_0();
            Ldarg_1();
            Ldarg_2();
            Cpblk();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void CopyBlockUnaligned(ref byte destination, ref byte source, uint byteCount)
        {
            Ldarg_0();
            Ldarg_1();
            Ldarg_2();
            Unaligned(1);
            Cpblk();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void CopyBlockUnaligned(void* destination, void* source, uint byteCount)
        {
            Ldarg_0();
            Ldarg_1();
            Ldarg_2();
            Unaligned(1);
            Cpblk();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void InitBlock(void* startAddress, byte value, uint byteCount)
        {
            Ldarg_0();
            Ldarg_1();
            Ldarg_2();
            Initblk();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void InitBlock(ref byte startAddress, byte value, uint byteCount)
        {
            Ldarg_0();
            Ldarg_1();
            Ldarg_2();
            Initblk();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void InitBlockUnaligned(ref byte startAddress, byte value, uint byteCount)
        {
            Ldarg_0();
            Ldarg_1();
            Ldarg_2();
            Unaligned(1);
            Initblk();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void InitBlockUnaligned(void* startAddress, byte value, uint byteCount)
        {
            Ldarg_0();
            Ldarg_1();
            Ldarg_2();
            Unaligned(1);
            Initblk();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref TTo As<TFrom, TTo>(ref TFrom source)
        {
            Ldarg_0();

            return ref ReturnRef<TTo>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        [return: NotNullIfNotNull("o")]
        public static T? As<T>(object? o) where T : class
        {
            Ldarg_0();

            return Return<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T AsRef<T>(in T source)
        {
            Ldarg_0();

            return ref ReturnRef<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T AsRef<T>(void* source)
        {
            DeclareLocals(false, new LocalVar(typeof(int).MakeByRefType()));

            Ldarg_0();
            Stloc_0();
            Ldloc_0();

            return ref ReturnRef<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T Unbox<T>(object box) where T : struct
        {
            Ldarg_0();
            Emit.Unbox<T>();

            return ref ReturnRef<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void* Add<T>(void* source, int elementOffset)
        {
            Ldarg_0();
            Ldarg_1();
            Emit.Sizeof<T>();
            Conv_I();
            Mul();
            Emit.Add();

            return ReturnPointer();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T Add<T>(ref T source, int elementOffset)
        {
            Ldarg_0();
            Ldarg_1();
            Emit.Sizeof<T>();
            Conv_I();
            Mul();
            Emit.Add();

            return ref ReturnRef<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T Add<T>(ref T source, IntPtr elementOffset)
        {
            Ldarg_0();
            Ldarg_1();
            Emit.Sizeof<T>();
            Mul();
            Emit.Add();

            return ref ReturnRef<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T AddByteOffset<T>(ref T source, IntPtr byteOffset)
        {
            Ldarg_0();
            Ldarg_1();
            Emit.Add();

            return ref ReturnRef<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void* Subtract<T>(void* source, int elementOffset)
        {
            Ldarg_0();
            Ldarg_1();
            Emit.Sizeof<T>();
            Conv_I();
            Mul();
            Sub();

            return ReturnPointer();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T Subtract<T>(ref T source, int elementOffset)
        {
            Ldarg_0();
            Ldarg_1();
            Emit.Sizeof<T>();
            Conv_I();
            Mul();
            Sub();

            return ref ReturnRef<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T Subtract<T>(ref T source, IntPtr elementOffset)
        {
            Ldarg_0();
            Ldarg_1();
            Emit.Sizeof<T>();
            Mul();
            Sub();

            return ref ReturnRef<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T SubtractByteOffset<T>(ref T source, IntPtr byteOffset)
        {
            Ldarg_0();
            Ldarg_1();
            Sub();

            return ref ReturnRef<T>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr ByteOffset<T>([AllowNull] ref T origin, [AllowNull] ref T target)
        {
            Ldarg_1();
            Ldarg_0();
            Sub();

            return Return<IntPtr>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool AreSame<T>([AllowNull] ref T left, [AllowNull] ref T right)
        {
            Ldarg_0();
            Ldarg_1();
            Ceq();

            return Return<bool>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IsAddressGreaterThan<T>([AllowNull] ref T left, [AllowNull] ref T right)
        {
            Ldarg_0();
            Ldarg_1();
            Cgt_Un();

            return Return<bool>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IsAddressLessThan<T>([AllowNull] ref T left, [AllowNull] ref T right)
        {
            Ldarg_0();
            Ldarg_1();
            Clt_Un();

            return Return<bool>();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SkipInit<T>(out T value)
        {
            Ret();
            throw IL.Unreachable();
        }
    }
}

#endif