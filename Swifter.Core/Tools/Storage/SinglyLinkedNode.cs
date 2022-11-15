using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Swifter.Tools
{

    /// <summary>
    /// 表示一个单向链的节点。
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    public sealed class SinglyLinkedNode<T>
    {
        /// <summary>
        /// 值。
        /// </summary>
        public T Value;

        /// <summary>
        /// 下一个节点。
        /// </summary>
        public SinglyLinkedNode<T>? Next;

        /// <summary>
        /// 初始化单向链的节点。
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="next">下一个节点</param>
        public SinglyLinkedNode(T value, SinglyLinkedNode<T>? next = null)
        {
            Value = value;
            Next = next;
        }
    }

    /// <summary>
    /// 一个简单的单向链列表。
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    public struct SinglyLinkedList<T>
    {
        /// <summary>
        /// 第一个节点。
        /// </summary>
        public SinglyLinkedNode<T>? FirstNode;

        /// <summary>
        /// 最后一个节点。
        /// </summary>
        public SinglyLinkedNode<T>? LastNode;

        /// <summary>
        /// 获取第一个元素。
        /// </summary>
        public T? First
        {
            get
            {
                var _first = Volatile.Read(ref FirstNode);

                if (_first is null)
                {
                    return default;
                }

                return _first.Value;
            }
        }

        /// <summary>
        /// 获取最后一个元素。
        /// </summary>
        public T? Last
        {
            get
            {
                var _last = Volatile.Read(ref LastNode);

                if (_last is null)
                {
                    return default;
                }

                return _last.Value;
            }
        }

        /// <summary>
        /// 判断当前单向链列表是否为空的。
        /// </summary>
        public bool IsEmpty 
            => Volatile.Read(ref FirstNode) is null;

        /// <summary>
        /// 在单向链列表的尾部添加一个元素。
        /// </summary>
        /// <param name="value">元素</param>
        public void AddLast(T value)
        {
            var node = new SinglyLinkedNode<T>(value);

        Loop:

            if (Volatile.Read(ref FirstNode) is null)
            {
                if (Interlocked.CompareExchange(ref FirstNode, node, null) is null)
                {
                    LastNode = node;
                }
                else
                {
                    goto Loop;
                }
            }
            else
            {
                var _last = Volatile.Read(ref LastNode);

                if (_last is null)
                {
                    goto Loop;
                }

                if (ReferenceEquals(Interlocked.CompareExchange(ref LastNode, node, _last), _last))
                {
                    _last.Next = node;
                }
                else
                {
                    goto Loop;
                }
            }
        }

        /// <summary>
        /// 在单向链列表的头部添加一个元素。
        /// </summary>
        /// <param name="value">元素</param>
        public void AddFirst(T value)
        {
            var node = new SinglyLinkedNode<T>(value);

        Loop:

            var _first = Volatile.Read(ref FirstNode);

            if (_first is null)
            {
                if (Interlocked.CompareExchange(ref FirstNode, node, null) is null)
                {
                    LastNode = node;
                }
                else
                {
                    goto Loop;
                }
            }
            else
            {
                node.Next = _first;

                if (ReferenceEquals(Interlocked.CompareExchange(ref FirstNode, node, _first), _first))
                {
                }
                else
                {
                    node.Next = null;

                    goto Loop;
                }
            }
        }

        /// <summary>
        /// 移除并返回单向链列表中第一个元素。
        /// </summary>
        /// <param name="value">返回被移除元素</param>
        /// <returns>返回在移除前是否至少有一个元素</returns>
        public bool RemoveFirst([MaybeNullWhen(false)] out T value)
        {
        Loop:

            var _first = Volatile.Read(ref FirstNode);

            if (_first is null)
            {
                value = default;

                return false;
            }

            if (ReferenceEquals(Interlocked.CompareExchange(ref FirstNode, _first.Next, _first), _first))
            {
                if (_first.Next is null)
                {
                    Volatile.Write(ref LastNode, null);
                }

                value = _first.Value;

                return true;
            }

            goto Loop;
        }
    }
}