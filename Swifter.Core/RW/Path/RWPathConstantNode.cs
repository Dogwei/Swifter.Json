using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Swifter.RW
{
    /// <summary>
    /// 表示读写路径常量节点。
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public sealed class RWPathConstantNode<TKey> : RWPathNode, IEquatable<RWPathConstantNode<TKey>> where TKey : notnull
    {
        /// <summary>
        /// 常量键。
        /// </summary>
        public readonly TKey Key;

        /// <summary>
        /// 初始化读写路径常量节点。
        /// </summary>
        /// <param name="key">常量键</param>
        public RWPathConstantNode(TKey key)
        {
            Key = key;
        }

        /// <inheritdoc/>
        public override void Accept(IRWPathNodeVisitor visitor)
        {
            visitor.VisitConstant<TKey>(this);
        }

        /// <inheritdoc/>
        public override bool Equals(RWPathNode? other)
        {
            return Equals(other as RWPathConstantNode<TKey>);
        }

        /// <summary>
        /// 对当前常量节点与指定常量节点进行比较。
        /// </summary>
        /// <param name="other">指定常量节点</param>
        /// <returns>返回是否相等</returns>
        public bool Equals(RWPathConstantNode<TKey>? other)
        {
            return other is not null && EqualityComparer<TKey>.Default.Equals(Key, other.Key);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return EqualityComparer<TKey>.Default.GetHashCode(Key);
        }

        /// <inheritdoc/>
        public override IEnumerable<IDataReader> GetDataReader(IEnumerable<IDataReader> dataReaders, IDataReader rootDataReader)
        {
            return dataReaders
                .Select(x => RWHelper.CreateItemReader(x.As<TKey>(), Key))
                .Where(x => x != null)!;
        }

        /// <inheritdoc/>
        public override IEnumerable<IDataWriter> GetDataWriter(IEnumerable<IDataWriter> dataWriters, IDataWriter rootDataWriter)
        {
            return dataWriters
                .Select(x => RWHelper.CreateItemWriter(x.As<TKey>(), Key))
                .Where(x => x != null)!;
        }

        /// <inheritdoc/>
        public override IEnumerable<IValueReader> GetValueReader(IEnumerable<IDataReader> dataReaders, IDataReader rootDataReader)
        {
            return dataReaders
                .Select(x => x.As<TKey>()[Key])
                .Where(x => x != null)!;
        }

        /// <inheritdoc/>
        public override IEnumerable<IValueWriter> GetValueWriter(IEnumerable<IDataWriter> dataWriters, IDataWriter rootDataWriter)
        {
            return dataWriters
                .Select(x => x.As<TKey>()[Key])
                .Where(x => x != null)!;
        }
    }
}