#if NET20 || NET35 || NET40

using System.Diagnostics.CodeAnalysis;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Versioning;
using System.Security;

#pragma warning disable 1591

namespace System.Threading
{
    public static class Volatile
    {
        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static bool Read(ref bool location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static sbyte Read(ref sbyte location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static byte Read(ref byte location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static short Read(ref short location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static ushort Read(ref ushort location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static int Read(ref int location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static uint Read(ref uint location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static long Read(ref long location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static ulong Read(ref ulong location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static IntPtr Read(ref IntPtr location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static UIntPtr Read(ref UIntPtr location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static float Read(ref float location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static double Read(ref double location)
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]

        [return: NotNullIfNotNull("location")]
        public static T? Read<T>(ref T? location) where T : class
        {
            var value = location;
            Thread.MemoryBarrier();
            return value;
        }




        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void Write(ref bool location, bool value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void Write(ref sbyte location, sbyte value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void Write(ref byte location, byte value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void Write(ref short location, short value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void Write(ref ushort location, ushort value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void Write(ref int location, int value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void Write(ref uint location, uint value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void Write(ref long location, long value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void Write(ref ulong location, ulong value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void Write(ref IntPtr location, IntPtr value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void Write(ref UIntPtr location, UIntPtr value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void Write(ref float location, float value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void Write(ref double location, double value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        [ResourceExposure(ResourceScope.None)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        public static void Write<T>(ref T? location, [NotNullIfNotNull("location")] T? value) where T : class
        {
            Thread.MemoryBarrier();
            location = value;
        }
    }
}

#endif