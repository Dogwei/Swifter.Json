using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 表示一个元素连续内存信息。
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    public readonly unsafe struct Ps<T> : IEquatable<Ps<T>> where T : unmanaged
    {
        /// <summary>
        /// 第一个元素的指针。
        /// </summary>
        public readonly T* Pointer;
        /// <summary>
        /// 元素数量。
        /// </summary>
        public readonly int Length;

        /// <summary>
        /// 初始化一个元素连续内存信息。
        /// </summary>
        /// <param name="pointer">第一个元素的指针</param>
        /// <param name="length">元素数量</param>
        public Ps(T* pointer, int length)
        {
            Pointer = pointer;
            Length = length;
        }

        /// <summary>
        /// 比较两个连续内存是否完全相等。
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(Ps<T> other)
        {
            return other.Length == Length && other.Pointer == Pointer;
        }

        /// <summary>
        /// 比较两个连续内存是否完全相等。
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return obj is Ps<T> other && Equals(other);
        }

        /// <summary>
        /// 获取连续内存的 Hash 值。
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return ((IntPtr)Pointer).GetHashCode() ^ Length.GetHashCode();
        }


        /// <summary>
        /// 分割连续内存信息。
        /// </summary>
        /// <param name="separator">分隔符</param>
        /// <returns>返回一个结果迭代器</returns>
        public SplitEnumerator Split(T separator) => new SplitEnumerator(this, separator);

        /// <summary>
        /// 分割连续内存信息的迭代器。
        /// </summary>
        public ref struct SplitEnumerator
        {
            T* pointer;
            int length;

            readonly T separator;

            internal SplitEnumerator(Ps<T> ps, T separator)
            {
                pointer = ps.Pointer;
                length = ps.Length;

                this.separator = separator;

                Current = default;
            }

            /// <summary>
            /// 获取当前段落。
            /// </summary>
            public Ps<T> Current;

            /// <summary>
            /// 移动至下个段落。
            /// </summary>
            /// <returns></returns>
            public bool MoveNext()
            {
                if (length > 0)
                {
                    var count = 0;

                    while (count < length && !Equals(pointer[count], separator))
                    {
                        ++count;
                    }

                    Current = new Ps<T>(pointer, count);

                    ++count;

                    pointer += count;
                    length -= count;

                    return true;
                }

                return false;
            }

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            static bool Equals(T x, T y)
            {
                if (typeof(T) == typeof(char)) return Underlying.As<T, char>(ref x) == Underlying.As<T, char>(ref y);
                else if (typeof(T) == typeof(byte) || typeof(T) == typeof(Utf8Byte)) return Underlying.As<T, byte>(ref x) == Underlying.As<T, byte>(ref y);
                else return EqualityComparer<T>.Default.Equals(x, y);
            }
        }



#if Span

        /// <summary>
        /// 将一个连续内存信息转换为内存段。
        /// </summary>
        /// <param name="ps">连续内存信息</param>
        public static implicit operator Span<T>(Ps<T> ps) => new Span<T>(ps.Pointer, ps.Length);

        /// <summary>
        /// 将一个连续内存信息转换为只读内存段。
        /// </summary>
        /// <param name="ps">连续内存信息</param>
        public static implicit operator ReadOnlySpan<T>(Ps<T> ps) => new Span<T>(ps.Pointer, ps.Length);

#endif
    }
}