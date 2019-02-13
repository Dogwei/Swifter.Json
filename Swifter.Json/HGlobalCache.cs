using System;
using System.Runtime.InteropServices;

namespace Swifter.Json
{
    /// <summary>
    /// 提供字符串的缓存
    /// </summary>
    public sealed unsafe class HGlobalCache
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
        private static HGlobalCache threadInstance;

        /// <summary>
        /// 当前线程的实例
        /// </summary>
        internal static HGlobalCache ThreadInstance
        {
            get
            {
                var value = threadInstance;

                if (value == null)
                {
                    value = new HGlobalCache();

                    threadInstance = value;
                }

                return value;
            }
        }

        /// <summary>
        /// 全局字符串内存地址
        /// </summary>
        internal char* chars;
        /// <summary>
        /// 字符串长度
        /// </summary>
        internal int count;

        /// <summary>
        /// 释放全局内存。
        /// </summary>
        ~HGlobalCache()
        {
            if (chars != null)
            {
                Marshal.FreeHGlobal((IntPtr)chars);
            }
        }

        /// <summary>
        /// 扩展字符串长度。
        /// </summary>
        /// <param name="expandMinSize">最小扩展长度</param>
        internal void Expand(int expandMinSize)
        {
            var limit = MaxSize;
            
            if (expandMinSize >= limit)
            {
                throw new OutOfMemoryException("HGlobal cache expand size exceeds limit.");
            }

            if (count >= limit)
            {
                throw new OutOfMemoryException("HGlobal cache size exceeds limit.");
            }

            count = count * 3 + expandMinSize;

            if (count > limit)
            {
                count = limit;
            }

            if (chars == null)
            {
                chars = (char*)Marshal.AllocHGlobal(count * sizeof(char));
            }
            else
            {
                chars = (char*)Marshal.ReAllocHGlobal((IntPtr)chars, (IntPtr)(count * sizeof(char)));
            }
        }
    }
}