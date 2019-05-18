using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供泛型类为键的缓存类。
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    /// <typeparam name="TValue">值的类型</typeparam>
    public sealed class HashCache<TKey, TValue> : BaseCache<TKey, TValue>
    {
        readonly IEqualityComparer<TKey> equalityComparer;

        /// <summary>
        /// 初始化高效读取的缓存字典。
        /// </summary>
        /// <param name="equalityComparer">缓存键的比较器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public HashCache(IEqualityComparer<TKey> equalityComparer = null)
            : this(0, equalityComparer)
        {
        }

        /// <summary>
        /// 初始化高效读取的缓存字典。
        /// </summary>
        /// <param name="capacity">初始容量</param>
        /// <param name="equalityComparer">缓存键的比较器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public HashCache(int capacity, IEqualityComparer<TKey> equalityComparer = null)
            : base(capacity)
        {
            if (equalityComparer == null)
            {
                equalityComparer = EqualityComparer<TKey>.Default;
            }

            this.equalityComparer = equalityComparer;
        }

        /// <summary>
        /// 计算缓存键的 HashCode。
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>返回 HashCode</returns>
        protected override int ComputeHashCode(TKey key)
        {
            var hashCode = equalityComparer.GetHashCode(key);

            return hashCode ^ (hashCode >> 16);
        }

        /// <summary>
        /// 比较两个缓存键是否相等。
        /// </summary>
        /// <param name="key1">缓存键 1</param>
        /// <param name="key2">缓存键 2</param>
        /// <returns>返回是否相等</returns>
        protected override bool Equals(TKey key1, TKey key2)
        {
            return equalityComparer.Equals(key1, key2);
        }
    }
}