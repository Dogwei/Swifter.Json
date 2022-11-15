using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 一个开放的键值对字典，允许更直接的操作该字典。
    /// </summary>
    /// <typeparam name="TKey">缓存键</typeparam>
    /// <typeparam name="TValue">缓存值</typeparam>
    [Serializable]
    public class OpenDictionary<TKey, TValue> where TKey : notnull
    {
        private const int MaxCapacity = 0x40000000;

        private static readonly Entry[] EmptyEntries = new Entry[1];

        /// <summary>
        /// 缓存实体。
        /// </summary>
        [StructLayout(LayoutKind.Auto)]
        public struct Entry
        {
            internal int bucket;
            internal int next;
            internal int hashCode;
            internal TKey key;

            /// <summary>
            /// 该缓存的值。
            /// </summary>
            public TValue Value;

            /// <summary>
            /// 该缓存的键。
            /// </summary>
            public TKey Key => key;
        }

        private int _count;
        private Entry[] _entries;
        private readonly IEqualityComparer<TKey> _comparer;

        /// <summary>
        /// 获取缓存集合当前的缓存数量。
        /// </summary>
        public int Count => _count;

        /// <summary>
        /// 使用默认比较器初始化实例。
        /// </summary>
        public OpenDictionary() : this(EqualityComparer<TKey>.Default)
        {
        }

        /// <summary>
        /// 使用指定比较器初始化实例。
        /// </summary>
        /// <param name="comparer">比较器</param>
        public OpenDictionary(IEqualityComparer<TKey> comparer)
        {
            _entries = EmptyEntries;
            _comparer = comparer;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void Grow()
        {
            if (_count >= MaxCapacity)
                throw new InvalidOperationException("Capacity overflow");

            var entries = new Entry[_entries.Length << 1];

            for (int i = 0; i < _count; ++i)
            {
                var oldEntry = _entries[i];
                ref var newEntry = ref entries[i];

                newEntry.hashCode = oldEntry.hashCode;
                newEntry.key = oldEntry.key;
                newEntry.Value = oldEntry.Value;

                ref var bucket = ref entries[newEntry.hashCode & (entries.Length - 1)].bucket;

                newEntry.next = bucket - 1;

                bucket = i + 1;
            }

            _entries = entries;
        }

        /// <summary>
        /// 初始化缓存。
        /// </summary>
        /// <param name="capacity">缓存的容量</param>
        public void Initialize(int capacity)
        {
            if (capacity < 0 || capacity > MaxCapacity)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            if ((capacity & (capacity - 1)) != 0)
            {
                int i = 2;

                while (i < capacity)
                {
                    i <<= 1;
                }

                capacity = i;
            }

            _entries
                = capacity is 0
                ? EmptyEntries
                : new Entry[capacity];

            _count = 0;
        }

        /// <summary>
        /// 清空缓存。
        /// </summary>
        public void Clear()
        {
            Array.Clear(_entries, 0, _count);

            _count = 0;
        }

        /// <summary>
        /// 获取指定索引处的缓存。
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <returns>返回缓存的引用。</returns>
        public ref Entry this[int index]
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                if (index >= 0 && index < _count)
                {
                    return ref _entries[index];
                }

                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// 直接添加一个缓存。
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        public void Add(TKey key, TValue value)
        {
            if (_entries.Length == _count || _entries.Length == 1) Grow();

            var hashCode = _comparer.GetHashCode(key);

            ref var bucket = ref _entries[hashCode & (_entries.Length - 1)].bucket;
            ref var entry = ref _entries[_count];

            entry.hashCode = hashCode;
            entry.key = key;
            entry.Value = value;
            entry.next = bucket - 1;

            ++_count;

            bucket = _count;
        }

        /// <summary>
        /// 找到第一个（先进后出）缓存所在的索引。
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>返回索引，没找到则返回 -1</returns>
        public int FindIndex(TKey key)
        {
            if (_count > 0)
            {
                var hashCode = _comparer.GetHashCode(key);

                var index = _entries[hashCode & (_entries.Length - 1)].bucket - 1;

                while (index >= 0)
                {
                    var entry = _entries[index];

                    if (entry.hashCode == hashCode && _comparer.Equals(key, entry.key))
                    {
                        return index;
                    }

                    index = entry.next;
                }
            }

            return -1;
        }

        /// <summary>
        /// 找到下一个（先进后出）缓存所在的索引。
        /// </summary>
        /// <param name="index">当前缓存索引</param>
        /// <returns>返回索引，没找到则返回 -1</returns>
        public int NextIndex(int index)
        {
            if (index >= 0 && index < _count)
            {
                var entry = _entries[index];

                index = entry.next;

                while (index >= 0)
                {
                    var item = _entries[index];

                    if (item.hashCode == entry.hashCode && _comparer.Equals(item.key, entry.key))
                    {
                        return index;
                    }

                    index = item.next;
                }
            }

            return -1;
        }

        /// <summary>
        /// 找到下一个（先进后出）缓存所在的索引。
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="index">当前缓存索引</param>
        /// <returns>返回索引，没找到则返回 -1</returns>
        public int NextIndex(TKey key, int index)
        {
            if (index >= 0 && index < _count)
            {
                var hashCode = _comparer.GetHashCode(key);

                index = _entries[index].next;

                while (index >= 0)
                {
                    var item = _entries[index];

                    if (item.hashCode == hashCode && _comparer.Equals(item.key, key))
                    {
                        return index;
                    }

                    index = item.next;
                }
            }

            return -1;
        }

        /// <summary>
        /// 移除指定索引处的缓存。
        /// </summary>
        /// <param name="index">指定索引</param>
        public void RemoveAt(int index)
        {
            if (index >= 0 && index < _count)
            {
                ref var entry = ref _entries[index];
                ref var bucket = ref _entries[entry.hashCode & (_entries.Length - 1)].bucket;

                var i = bucket - 1;

                if (i == index)
                {
                    bucket = entry.next + 1;
                }
                else
                {
                    // 一定有指向 index 处的缓存实体。
                    while (_entries[i].next != index) i = _entries[i].next;

                    _entries[i].next = entry.next;
                }

                while (++index < _count)
                {
                    entry = ref _entries[index];
                    bucket = ref _entries[entry.hashCode & (_entries.Length - 1)].bucket;

                    i = bucket - 1;

                    if (i == index)
                    {
                        --bucket;
                    }
                    else
                    {
                        // 一定有指向 index 处的缓存实体。
                        while (_entries[i].next != index) i = _entries[i].next;

                        --_entries[i].next;
                    }

                    _entries[index - 1] = entry;
                }

                entry.key = default!;
                entry.Value = default!;

                --_count;

            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }
    }
}