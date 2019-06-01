using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    /// <summary>
    /// 成员集合。
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct XMemberCollection<T> : IEnumerable<T>
    {
        private readonly T[] array;

        private XMemberCollection(T[] array)
        {
            this.array = array;
        }

        /// <summary>
        /// 获取成员集合元素数量。
        /// </summary>
        public int Count => array.Length;

        /// <summary>
        /// 获取指定索引处的成员。
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <returns>返回成员</returns>
        public T this[int index] => array[index];

        /// <summary>
        /// 获取成员集合迭代器。
        /// </summary>
        /// <returns>返回成员集合迭代器。</returns>
        public IEnumerator<T> GetEnumerator() => ((IList<T>)array).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// 隐式构建成员集合。
        /// </summary>
        /// <param name="array">原始成员集合</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static implicit operator XMemberCollection<T>(T[] array) => new XMemberCollection<T>(array);
    }
}