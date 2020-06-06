using System;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供指定值类型的全局内存缓存。
    /// </summary>
    /// <typeparam name="T">指定值类型</typeparam>
    public sealed unsafe partial class HGlobalCache<T> where T : unmanaged
    {
        /// <summary>
        /// 可以设置的最大缓存大小。
        /// </summary>
        public static readonly int AbsolutelyMaxSize = 1218 * 500000 / sizeof(T);

        /// <summary>
        /// 可以设置的最小缓存大小。
        /// </summary>
        public static readonly int AbsolutelyMinSize = 1218 * 70 / sizeof(T);

        private static int max_size = AbsolutelyMaxSize;
        private static int min_size = AbsolutelyMinSize;

        /// <summary>
        /// 读取或设置最大缓存大小。
        /// </summary>
        public static int MaxSize
        {
            get
            {
                return max_size;
            }
            set
            {
                if (value >= AbsolutelyMinSize && value <= AbsolutelyMaxSize)
                {
                    max_size = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }

        /// <summary>
        /// 读取或设置最小缓存大小。
        /// </summary>
        public static int MinSize
        {
            get
            {
                return min_size;
            }
            set
            {
                if (value >= AbsolutelyMinSize && value <= AbsolutelyMaxSize)
                {
                    min_size = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }

        T[] array;
        int offset;

        /// <summary>
        /// 获取元数组。
        /// </summary>
        public T[] Context => array;

        /// <summary>
        /// 可用的空间总长度。
        /// </summary>
        public int Available => array.Length - offset;

        /// <summary>
        /// 剩余可用的空间长度。
        /// </summary>
        public int Rest => Available - Count;
        

        /// <summary>
        /// 已使用的空间数量。
        /// </summary>
        public int Count;

        /// <summary>
        /// 创建全局缓存实例。
        /// </summary>
        public HGlobalCache()
        {
            ReAlloc(MinSize);
        }

        /// <summary>
        /// 第一个元素的地址。
        /// </summary>
        public T* First { get; private set; }

        /// <summary>
        /// 最后一个元素的地址。
        /// </summary>
        public T* Last { get; private set; }

        /// <summary>
        /// 当前元素的地址。
        /// </summary>
        public T* Current => First + Count;

        /// <summary>
        /// 获取或设置偏移量。
        /// </summary>
        public int Offset
        {
            get => offset;
            set
            {
                if (offset >= 0 && offset < array.Length)
                {
                    First = (T*)Underlying.AsPointer(ref array[value]);
                    Last = (T*)Underlying.AsPointer(ref array[Available - 1]);


                    Count = Math.Max(Count + offset - value, 0);

                    offset = value;
                }
                else
                {
                    throw new ArgumentException("Out of range.", nameof(Offset));
                }
            }
        }

        /// <summary>
        /// 扩展全局缓存内存。
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

            if (array.Length >= limit)
            {
                throw new OutOfMemoryException("HGlobal cache size exceeds limit.");
            }

            ReAlloc(Math.Min(array.Length * 2 + expandMinSize, limit));
        }


        void ReAlloc(int size)
        {
            if (array is null)
            {
                array = new T[size];
            }
            else
            {
                Array.Resize(ref array, size + 12);
            }

            Offset = Offset;
        }

        /// <summary>
        /// 返回全局缓存中的内容 ArraySegment。
        /// </summary>
        /// <param name="hGCache"></param>
        public static implicit operator ArraySegment<T>(HGlobalCache<T> hGCache) => new ArraySegment<T>(hGCache.Context, hGCache.Offset, hGCache.Count);

        /// <summary>
        /// 返回全局缓存中的内容段。
        /// </summary>
        /// <param name="hGCache"></param>
        public static implicit operator Ps<T>(HGlobalCache<T> hGCache) => new Ps<T>(hGCache.First, hGCache.Count);
    }
}