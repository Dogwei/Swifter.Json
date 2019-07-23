using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供指定值类型的全局内存缓存。
    /// </summary>
    /// <typeparam name="T">指定值类型</typeparam>
    public sealed partial class HGlobalCache<T> where T : struct
    {
        static readonly T[] empty = new T[0];

        /// <summary>
        /// 可以设置的最大缓存大小。
        /// </summary>
        public const int AbsolutelyMaxSize = 400000000;
        /// <summary>
        /// 可以设置的最小缓存大小。
        /// </summary>
        public const int AbsolutelyMinSize = 102400;

        private static int maxSize = AbsolutelyMaxSize;

        /// <summary>
        /// 读取或设置最大缓存大小。
        /// </summary>
        public static int MaxSize
        {
            get
            {
                return Math.Max(Math.Min(maxSize, AbsolutelyMaxSize), AbsolutelyMinSize);
            }
            set
            {
                if (value > AbsolutelyMaxSize || value < AbsolutelyMinSize)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                maxSize = value;
            }
        }

        /// <summary>
        /// 获取 T 的 Size。
        /// </summary>
        public static int TSize => Unsafe.SizeOf<T>();

        GCHandle gc;
        T[] array;

        /// <summary>
        /// 获取数组。
        /// </summary>
        public T[] Context => array;

        /// <summary>
        /// 创建全局缓存实例。
        /// </summary>
        public HGlobalCache()
        {
            array = empty;
        }

        /// <summary>
        /// 全局字符串内存地址。
        /// </summary>
        public IntPtr Address { get; private set; }

        /// <summary>
        /// 全局字符串内存地址。
        /// </summary>
        public unsafe void* Pointer => (void*)Address;

        /// <summary>
        /// 获取指定位置的 T 元素。
        /// </summary>
        /// <param name="index">指定位置</param>
        /// <returns>返回 T 值</returns>
        public ref T this[int index] => ref array[index];

        /// <summary>
        /// T 元素数量。
        /// </summary>
        public int Capacity => array.Length;

        /// <summary>
        /// T 元素的内容数量。
        /// </summary>
        public int Count;

        /// <summary>
        /// 首个 T 元素引用。
        /// </summary>
        public unsafe ref T First => ref Unsafe.AsRef<T>(Address);

        /// <summary>
        /// 总 Byte 数量。
        /// </summary>
        public int ByteCount => Capacity * TSize;

        /// <summary>
        /// 释放全局内存。
        /// </summary>
        ~HGlobalCache()
        {
            if (gc.IsAllocated)
            {
                gc.Free();

                array = empty;
            }
        }

        /// <summary>
        /// 扩展字符串长度。
        /// </summary>
        /// <param name="expandMinSize">最小扩展长度</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Expand(int expandMinSize)
        {
            var limit = MaxSize;

            if (expandMinSize >= limit)
            {
                throw new OutOfMemoryException("HGlobal cache expand size exceeds limit.");
            }

            if (Capacity >= limit)
            {
                throw new OutOfMemoryException("HGlobal cache size exceeds limit.");
            }

            var size = Capacity * 2 + expandMinSize;

            if (gc.IsAllocated)
            {
                gc.Free();
            }

            Array.Resize(ref array, size);

            gc = GCHandle.Alloc(array, GCHandleType.Pinned);

            Address = Unsafe.AsIntPtr(ref array[0]);
        }
    }
}