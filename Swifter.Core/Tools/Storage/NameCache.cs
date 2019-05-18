using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供 String 为 Id 的缓存类。
    /// </summary>
    /// <typeparam name="TValue">值的类型</typeparam>
    public sealed class NameCache<TValue> : BaseCache<string, TValue>
    {
        /// <summary>
        /// 初始化高效读取的缓存字典。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public NameCache()
            : base(0)
        {
        }

        /// <summary>
        /// 初始化高效读取的缓存字典。
        /// </summary>
        /// <param name="capacity">初始容量</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public NameCache(int capacity)
            : base(capacity)
        {
        }

        /// <summary>
        /// 计算缓存键的 HashCode。
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>返回 HashCode</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        protected override int ComputeHashCode(string key)
        {
            return key.GetHashCode();
        }

        /// <summary>
        /// 比较两个缓存键是否相等。
        /// </summary>
        /// <param name="key1">缓存键 1</param>
        /// <param name="key2">缓存键 2</param>
        /// <returns>返回是否相等</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        protected override bool Equals(string key1, string key2)
        {
            return key1 == key2;
        }
    }
}