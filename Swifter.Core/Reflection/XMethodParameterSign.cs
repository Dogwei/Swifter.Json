using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace Swifter.Reflection
{
    /// <summary>
    /// 方法参数签名。
    /// 表示方法参数的类型集合。
    /// </summary>
    public readonly struct XMethodParameters : IEquatable<XMethodParameters>, IEquatable<Type[]>
    {
        readonly ParameterInfo[] Parameters;

        /// <summary>
        /// 创建方法签名集合。
        /// </summary>
        public XMethodParameters(ParameterInfo[] parameters)
        {
            Parameters = parameters;
        }

        /// <summary>
        /// 获取参数数量。
        /// </summary>
        public int Count => Parameters.Length;

        /// <summary>
        /// 获取指定索引处的参数信息。
        /// </summary>
        public ParameterInfo this[int index] => Parameters[index];

        /// <summary>
        /// 获取指定名称的参数信息。
        /// </summary>
        public ParameterInfo? this[string name]
        {
            get
            {
                foreach (var item in Parameters)
                {
                    if (string.Equals(name, item.Name))
                    {
                        return item;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// 获取当前签名的 HashCode。
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            int hashCode = 0x12181996;

            for (int i = 0; i < Parameters.Length; i++)
            {
                hashCode ^= (i + 1218) * Parameters[i].ParameterType.GetHashCode();
            }

            return hashCode;
        }

        /// <summary>
        /// 判断当前方法签名与指定实例是否相等。
        /// </summary>
        public override bool Equals(object? obj)
        {
            return obj is XMethodParameters other && Equals(other);
        }

        /// <summary>
        /// 判断当前方法签名与指定方法签名是否相等。
        /// </summary>
        public bool Equals(XMethodParameters other)
        {
            return Equals(other.Parameters);
        }

        /// <summary>
        /// 判断当前方法签名与指定类型集合是否相等。
        /// </summary>
        public bool Equals(Type[]? other)
        {
            if (other is null)
            {
                return false;
            }

            if (other.Length != Parameters.Length)
            {
                return false;
            }

            for (int i = 0; i < other.Length; i++)
            {
                if (Parameters[i].ParameterType != other[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取迭代器。
        /// </summary>
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        
        /// <summary>
        /// 迭代器。
        /// </summary>
        public struct Enumerator
        {
            readonly XMethodParameters Parameters;

            int offset;

            internal Enumerator(XMethodParameters parameters)
            {
                Parameters = parameters;

                offset = -1;
            }

            /// <summary>
            /// 获取当前元素。
            /// </summary>
            public ParameterInfo Current => Parameters[offset];

            /// <summary>
            /// 将索引移至下一个位置。
            /// </summary>
            /// <returns>返回是否有下一个元素</returns>
            public bool MoveNext() => ++offset < Parameters.Count;
        }
    }
}