using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供指定值类型的全局内存缓存。
    /// </summary>
    /// <typeparam name="T">指定值类型</typeparam>
    public sealed class HGlobalCache<T> : IDisposable where T : struct
    {
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

        [ThreadStatic]
        private static HGlobalCache<T> threadInstance;

        /// <summary>
        /// 占用当前线程的实例。
        /// </summary>
        /// <returns>返回一个实例</returns>
        public static HGlobalCache<T> OccupancyInstance()
        {
            var value = threadInstance;

            if (value == null)
            {
                value = new HGlobalCache<T>();

                threadInstance = value;
            }

            var nested = 0;

        Loop:

            if (value.available)
            {
                value.available = false;

                return value;
            }

            if (nested > 5)
            {
                throw new NotSupportedException("Too many nested!");
            }

            value = value.next ?? (value.next = new HGlobalCache<T>());

            ++nested;

            goto Loop;
        }

        /// <summary>
        /// 全局字符串内存地址
        /// </summary>
        public IntPtr Address { get; private set; }

        /// <summary>
        /// T 元素数量。
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 内存指针。
        /// </summary>
        public unsafe void* Pointer => (void*)Address;

        /// <summary>
        /// 首个 T 元素引用。
        /// </summary>
        public unsafe ref T First => ref Unsafe.AsRef<T>(Pointer);

        /// <summary>
        /// 总 Byte 数量。
        /// </summary>
        public int ByteCount => Count * Unsafe.SizeOf<T>();

        /// <summary>
        /// 是否可用。
        /// </summary>
        private bool available;

        /// <summary>
        /// 下一个缓存。
        /// </summary>
        private HGlobalCache<T> next;

        private HGlobalCache()
        {
            available = true;
        }

        /// <summary>
        /// 释放全局内存。
        /// </summary>
        ~HGlobalCache()
        {
            if (Address != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(Address);
            }
        }

        /// <summary>
        /// 扩展字符串长度。
        /// </summary>
        /// <param name="expandMinSize">最小扩展长度</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Expand(int expandMinSize)
        {
            var limit = MaxSize;

            if (expandMinSize >= limit)
            {
                throw new OutOfMemoryException("HGlobal cache expand size exceeds limit.");
            }

            if (Count >= limit)
            {
                throw new OutOfMemoryException("HGlobal cache size exceeds limit.");
            }

            Count = Count * 3 + expandMinSize;

            if (Count > limit)
            {
                Count = limit;
            }

            GC.RemoveMemoryPressure(ByteCount);

            var byteCount = Count * Unsafe.SizeOf<T>();

            if (Address == IntPtr.Zero)
            {
                Address = Marshal.AllocHGlobal(byteCount);
            }
            else
            {
                Address = Marshal.ReAllocHGlobal(Address, (IntPtr)byteCount);
            }

            GC.AddMemoryPressure(ByteCount);
        }

        /// <summary>
        /// 取消占用当前线程的实例。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Dispose()
        {
            available = true;
        }
    }
}