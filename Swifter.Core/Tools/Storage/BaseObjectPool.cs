using System;
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
        [ThreadStatic]
        private protected static T ThreadStatic;


        /* 核心思想：拖延租借，尽早归还。*/

        volatile Node first;

        /// <summary>
        /// 借出一个实例。（借出的实例不一定要归还，平衡选择，如果归还成本大于实例本身，可以选择不归还实例。）
        /// </summary>
        /// <returns>返回一个实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public T Rent()
        {
            ref var thread_static = ref ThreadStatic;

            if (thread_static != null)
            {
                var r = thread_static;

                thread_static = null;

                return r;
            }

            return LockedRent();
        }

        /// <summary>
        /// 归还一个实例。
        /// </summary>
        /// <param name="obj">实例</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Return(T obj)
        {
            ref var thread_static = ref ThreadStatic;

            if (thread_static is null)
            {
                thread_static = obj;
            }
            else
            {
                LockedReturn(obj);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void LockedReturn(T obj)
        {
            var node = new Node(obj, first);

            while (Interlocked.CompareExchange(ref first, node, node.Next) != node.Next) node.Next = first;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private T LockedRent()
        {
            if (first is Node node)
            {
                if (Interlocked.CompareExchange(ref first, node.Next, node) == node)
                {
                    return node.Value;
                }
            }

            return CreateInstance();
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
}