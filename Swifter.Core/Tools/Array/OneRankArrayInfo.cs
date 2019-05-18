using System;
using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 值类型一维数组的内部储存结构。
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    [StructLayout(LayoutKind.Sequential)]
    public struct OneRankValueArrayInfo<T> where T : struct
    {
        /// <summary>
        /// 该元素类型的 0 长度一维数组实例。
        /// </summary>
        public static readonly T[] Empty = new T[0];

        /// <summary>
        /// 该元素一维数组的 ObjectTypeHandle。
        /// </summary>
        public static readonly IntPtr ObjectTypeHandle = TypeHelper.GetObjectTypeHandle(Empty);

        /// <summary>
        /// 表示该结构是否可用。
        /// </summary>
        public static readonly bool Available = IsAvailable(Empty) && IsAvailable(new T[2]);

        /// <summary>
        /// 表示一维数组的元素便宜量。
        /// </summary>
        public static readonly int ElementOffset = GetElementOffset(new T[1]);

        private static bool IsAvailable(T[] ts)
        {
            ref var rts = ref Unsafe.AsRef<OneRankValueArrayInfo<T>>(ts);

            return rts.TypeHandle == ObjectTypeHandle && (long)rts.Length == ts.Length && (ts.Length == 0 || Unsafe.AreSame(ref rts.FirstElement, ref ts[0]));
        }

        private static unsafe int GetElementOffset(T[] ts)
        {
            return (int)((byte*)Unsafe.AsIntPtr(ref ts[0]) - (byte*)Unsafe.AsIntPtr(ref Unsafe.AsRef<IntPtr>(ts)));
        }

        /// <summary>
        /// 数组的 ObjectTypeHandle。
        /// </summary>
        public IntPtr TypeHandle;

        /// <summary>
        /// 数组的长度。
        /// </summary>
        public IntPtr Length;

        /// <summary>
        /// 数组的第一个元素。
        /// </summary>
        public T FirstElement;
    }
}