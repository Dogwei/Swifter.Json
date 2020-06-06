using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#nullable enable

namespace Swifter.Tools
{
    /// <summary>
    /// 一个键值对缓存集合。
    /// </summary>
    /// <typeparam name="TKey">缓存键</typeparam>
    /// <typeparam name="TValue">缓存值</typeparam>
    [Serializable]
    public class Cache<TKey, TValue> where TKey : notnull
    {
        private const int DefaultCapacity = 1;
        private const int MaxCapacity = 0x40000000;

        /// <summary>
        /// 缓存实体。
        /// </summary>
        [Serializable]
        public struct Entry
        {
            internal int next;

            internal uint hashCode;

            /// <summary>
            /// 该缓存的键，注意：修改此值将导致缓存集合不稳定。
            /// </summary>
            public TKey Key;

            /// <summary>
            /// 该缓存的值。
            /// </summary>
            public TValue Value;
        }

        private int _count;
        private int[]? _buckets;
        private Entry[]? _entries;
        private readonly IEqualityComparer<TKey>? _comparer;

        /// <summary>
        /// 获取缓存集合当前的缓存数量。
        /// </summary>
        public int Count => _count!;

        /// <summary>
        /// 读取或设置缓存集合的容量。
        /// </summary>
        public int Capacity
        {
            get => _entries?.Length ?? 0;
            set => SetCapacity(value);
        }

        /// <summary>
        /// 获取键集合。
        /// </summary>
        public IEnumerable<TKey> Keys
        {
            get
            {
                for (int i = 0; i < _count; i++)
                {
                    yield return _entries![i].Key;
                }
            }
        }

        /// <summary>
        /// 获取值集合。
        /// </summary>
        public IEnumerable<TValue> Values
        {
            get
            {
                for (int i = 0; i < _count; i++)
                {
                    yield return _entries![i].Value;
                }
            }
        }

        /// <summary>
        /// 使用默认比较器初始化实例。
        /// </summary>
        public Cache()
        {

        }

        /// <summary>
        /// 使用指定比较器初始化实例。
        /// </summary>
        /// <param name="comparer">指定比较器</param>
        public Cache(IEqualityComparer<TKey> comparer)
        {
            _comparer = comparer;
        }

        private void Initialize(int capacity)
        {
            int size = 1;

            while (capacity > size) size <<= 1;

            _buckets = new int[size];
            _entries = new Entry[capacity];
        }

        private void Grow()
        {
            if (_count >= MaxCapacity)
                throw new ArgumentException("too big.");

            int size = 2;

            while (_count >= size) size <<= 1;

            _buckets = new int[size];

            var entries = new Entry[size];

            if (_count > 0)
            {
                Array.Copy(_entries!, entries, _count);

                for (int i = 0; i < _count; ++i)
                {
                    ref int bucket = ref GetBucket(entries[i].hashCode);

                    entries[i].next = bucket - 1;

                    bucket = i + 1;
                }
            }

            _entries = entries;
        }

        private void SetCapacity(int capacity)
        {
            if (capacity < _count)
                throw new ArgumentException("too small.");

            if (capacity > MaxCapacity)
                throw new ArgumentException("too big.");

            if (capacity == _count)
                return;

            int size = 1;

            while (capacity > size) size <<= 1;

            if (_buckets != null && _buckets.Length == size)
            {
                Array.Resize(ref _entries, capacity);

                return;
            }

            _buckets = new int[size];

            var entries = new Entry[capacity];

            if (_count > 0)
            {
                Array.Copy(_entries!, entries, _count);

                for (int i = 0; i < _count; ++i)
                {
                    ref int bucket = ref GetBucket(entries[i].hashCode);

                    entries[i].next = bucket - 1;

                    bucket = i + 1;
                }
            }

            _entries = entries;
        }

        private ref int GetBucket(uint hashCode)
        {
            var buckets = _buckets!;

            return ref buckets![hashCode & (buckets.Length - 1)];
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private uint GetHashCode(TKey key)
        {
            if (_comparer != null)
            {
                return (uint)_comparer.GetHashCode(key);
            }

            return (uint)key.GetHashCode();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private bool Equals(TKey x, TKey y)
        {
            if (_comparer != null)
            {
                return _comparer.Equals(x, y);
            }

            return EqualityComparer<TKey>.Default.Equals(x, y);
        }

        /// <summary>
        /// 获取指定索引处的缓存。
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <returns>返回缓存的引用。</returns>
        public ref readonly Entry this[int index]
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                if (index >= 0 && index < _count)
                {
                    return ref _entries![index];
                }

                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// 设置指定索引处缓存的值。
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <param name="value">新的值</param>
        public void SetValue(int index, TValue value)
        {
            if (index >= 0 && index < _count)
            {
                _entries![index].Value = value;
            }
            else
            {
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
            if (_buckets is null) Initialize(DefaultCapacity);

            if (_count == _entries!.Length) Grow();

            var hashCode = GetHashCode(key);

            ref var bucket = ref GetBucket(hashCode);

            ref var entry = ref _entries[_count];

            entry.hashCode = hashCode;
            entry.next = bucket - 1;
            entry.Key = key;
            entry.Value = value;

            ++_count;

            bucket = _count;
        }

        /// <summary>
        /// 修改或添加一个缓存。
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        public void SetOrAdd(TKey key, TValue value)
        {
            if (_buckets is null) Initialize(0);

            var hashCode = GetHashCode(key);

            ref var bucket = ref GetBucket(hashCode);

            if (bucket > 0)
            {
                int index = bucket - 1;

                do
                {
                    if (_entries![index].hashCode == hashCode && Equals(_entries[index].Key, key))
                    {
                        _entries[index].Value = value;

                        return;
                    }

                    index = _entries[index].next;

                } while (index >= 0);
            }

            if (_count == _entries!.Length) Grow();

            ref var entry = ref _entries[_count];

            entry.hashCode = hashCode;
            entry.next = bucket - 1;
            entry.Key = key;
            entry.Value = value;

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
                var entries = _entries!;

                var hashCode = GetHashCode(key);

                var bucket = GetBucket(hashCode);

                if (bucket > 0)
                {
                    int index = bucket - 1;

                    do
                    {
                        ref var entry = ref entries[index];

                        if (entry.hashCode == hashCode && Equals(entry.Key, key))
                        {
                            return index;
                        }

                        index = entry.next;

                    } while (index >= 0);
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
                var entries = _entries!;

                ref var entry = ref entries![index];

                index = entry.next;

                while (index >= 0)
                {
                    if (entries[index].hashCode == entry.hashCode && Equals(entries[index].Key, entry.Key))
                    {
                        return index;
                    }

                    index = entries[index].next;
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
                ref var entry = ref _entries![index];
                ref var bucket = ref GetBucket(entry.hashCode);

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
                    bucket = ref GetBucket(entry.hashCode);

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

                entry = default;

                --_count;

            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// 尝试获取第一个（先进后出）缓存的值。
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <returns>返回是否获取成功</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            var index = FindIndex(key);

            if (index >= 0)
            {
                value = _entries![index].Value;

                return true;
            }

            value = default!;

            return false;
        }

        /// <summary>
        /// 压缩缓存集合。
        /// </summary>
        public void Compress()
        {
            SetCapacity(_count);
        }

        /// <summary>
        /// 获取第一个（先进后出）缓存的值。
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <exception cref="KeyNotFoundException">当缓存键不存在时引发此异常</exception>
        /// <returns>返回缓存的值</returns>
        public TValue GetFirstValue(TKey key)
        {
            var index = FindIndex(key);

            if (index >= 0)
            {
                return _entries![index].Value;
            }

            throw new KeyNotFoundException();
        }
    }
}
