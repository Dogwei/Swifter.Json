using System;
using System.Collections.Generic;
using System.Linq;

namespace Swifter.RW
{
    /// <summary>
    /// 读写路径节点。
    /// </summary>
    public abstract class RWPathNode : IEquatable<RWPathNode>
    {
        /// <summary>
        /// 获取数据读取器该路径的值读取器。
        /// </summary>
        /// <param name="dataReaders">数据读取器集合</param>
        /// <param name="rootDataReader">根数据读取器</param>
        /// <returns>返回值读取器集合</returns>
        public abstract IEnumerable<IValueReader> GetValueReader(IEnumerable<IDataReader> dataReaders, IDataReader rootDataReader);

        /// <summary>
        /// 设置数据写入器该路径的值写入器。
        /// </summary>
        /// <param name="dataWriters">数据写入器集合</param>
        /// <param name="rootDataWriter">根数据写入器</param>
        /// <returns>返回值写入器集合</returns>
        public abstract IEnumerable<IValueWriter> GetValueWriter(IEnumerable<IDataWriter> dataWriters, IDataWriter rootDataWriter);

        /// <summary>
        /// 获取数据读取器的值的数据读取器。
        /// </summary>
        /// <param name="dataReaders">数据读取器集合</param>
        /// <param name="rootDataReader">根数据读取器</param>
        /// <returns>返回数据读取器集合</returns>
        public abstract IEnumerable<IDataReader> GetDataReader(IEnumerable<IDataReader> dataReaders, IDataReader rootDataReader);

        /// <summary>
        /// 获取数据写入器的值的数据写入器。
        /// </summary>
        /// <param name="dataWriters">数据写入器集合</param>
        /// <param name="rootDataWriter">根数据写入器</param>
        /// <returns>返回数据写入器集合</returns>
        public abstract IEnumerable<IDataWriter> GetDataWriter(IEnumerable<IDataWriter> dataWriters, IDataWriter rootDataWriter);

        /// <summary>
        /// 调用此节点的访问方法。
        /// </summary>
        /// <param name="visitor">路径节点访问器</param>
        public abstract void Accept(IRWPathNodeVisitor visitor);

        /// <summary>
        /// 对当前路径节点和指定路径节点进行比较。
        /// </summary>
        /// <param name="other">指定路径节点</param>
        /// <returns>返回是否相等</returns>
        public abstract bool Equals(RWPathNode? other);

        /// <summary>
        /// 获取当前路径节点的哈希值。
        /// </summary>
        /// <returns></returns>
        public override abstract int GetHashCode();

        /// <summary>
        /// 对当前路径节点和指定对象进行比较。
        /// </summary>
        /// <param name="obj">指定对象</param>
        /// <returns>返回是否相等</returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as RWPathNode);
        }
    }
}