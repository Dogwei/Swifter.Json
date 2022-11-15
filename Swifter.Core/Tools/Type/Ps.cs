using System;

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
        public override bool Equals(object? obj)
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
        public SplitEnumerator<T> Split(T separator) => new SplitEnumerator<T>(Pointer, Length, separator);



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
        public static implicit operator ReadOnlySpan<T>(Ps<T> ps) => new ReadOnlySpan<T>(ps.Pointer, ps.Length);

        /// <summary>
        /// 将一个连续内存信息转换为内存段。
        /// </summary>
        public Span<T> AsSpan() => this;

#endif
    }
}