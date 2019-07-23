using System.Runtime.CompilerServices;
using System.Threading;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供对象池的基类。
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    public abstract class BaseObjectPool<T> where T : class
    {
        /*核心思想：拖延租借，尽早归还。*/

        volatile Node first;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private T RentNull()
        {
            Node node = null;

            if (node == null)
            {
                Yield();

                node = first;
            }

            if (node == null)
            {
                return CreateInstance();
            }

            if (Interlocked.CompareExchange(ref first, node.Next, node) == node)
            {
                return node.Value;
            }

            return RentLoop();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private T RentLoop()
        {
            Yield();

            return Rent();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private void Yield()
        {
#if NET20 || NET30 || NET35
            lock (this) Thread.Sleep(1);
#else
            lock (this) Thread.Yield();
#endif
        }

        /// <summary>
        /// 借出一个实例。（借出的实例不一定要归还，平衡选择，如果归还成本大于实例本身，可以选择不归还实例。）
        /// </summary>
        /// <returns>返回一个实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T Rent()
        {
            var node = first;

            if (node == null) return RentNull();
            
            if (Interlocked.CompareExchange(ref first, node.Next, node) == node)
            {
                return node.Value;
            }

            return RentLoop();
        }

        /// <summary>
        /// 归还一个实例。
        /// </summary>
        /// <param name="obj">实例</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Return(T obj)
        {
            var node = new Node(obj, first);

            while (Interlocked.CompareExchange(ref first, node, node.Next) != node.Next) node.Next = first;
        }

        /// <summary>
        /// 由派生类重写的创建实例方法。
        /// </summary>
        /// <returns>返回一个实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        protected abstract T CreateInstance();

        sealed class Node
        {
            public readonly T Value;

            public volatile Node Next;

            public Node(T value, Node next)
            {
                Value = value;
                Next = next;
            }
        }
    }

    /// <summary>
    /// 全局缓存的对象池。
    /// </summary>
    /// <typeparam name="T">缓存类型</typeparam>
    public sealed class HGlobalCachePool<T> : BaseObjectPool<HGlobalCache<T>> where T : struct
    {
        /// <summary>
        /// 创建全局缓存实例。
        /// </summary>
        /// <returns>返回一个实例</returns>
        protected override HGlobalCache<T> CreateInstance()
        {
            return new HGlobalCache<T>();
        }
    }
}