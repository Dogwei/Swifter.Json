using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Swifter.RW
{
    /// <summary>
    /// 表示一个读写路径。
    /// </summary>
    public sealed class RWPath
    {
        /// <summary>
        /// 路径节点。
        /// </summary>
        public SinglyLinkedList<RWPathNode> Nodes;

        /// <summary>
        /// 向后添加一个节点。
        /// </summary>
        public void AddPathNode<TKey>(TKey key) where TKey : notnull
        {
            Nodes.AddLast(new RWPathConstantNode<TKey>(key));
        }

        /// <summary>
        /// 获取一个数据读取器该路径的值读取器。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <param name="rootDataReader">可指定根数据读取器</param>
        /// <returns>返回一个值读取器</returns>
        public IEnumerable<IValueReader> GetValueReader(IDataReader dataReader, IDataReader? rootDataReader = null)
        {
            rootDataReader ??= dataReader;

            var dataReaders = Enumerable.Repeat(dataReader, 1);

            for (var node = Nodes.FirstNode; node != null; node = node.Next)
            {
                if (ReferenceEquals(node, Nodes.LastNode))
                {
                    return node.Value.GetValueReader(dataReaders, rootDataReader);
                }
                else
                {
                    dataReaders = node.Value.GetDataReader(dataReaders, rootDataReader);
                }
            }

            return Enumerable.Repeat<IValueReader>(new DataRWContentValueRW(dataReader), 1);
        }

        /// <summary>
        /// 设置一个数据写入器该路径的值写入器。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="rootDataWriter">可指定根数据写入器</param>
        /// <returns>返回一个值写入器</returns>
        public IEnumerable<IValueWriter> GetValueWriter(IDataWriter dataWriter, IDataWriter? rootDataWriter = null)
        {
            rootDataWriter ??= dataWriter;

            var dataWriters = Enumerable.Repeat(dataWriter, 1);

            for (var node = Nodes.FirstNode; node != null; node = node.Next)
            {
                if (ReferenceEquals(node, Nodes.LastNode))
                {
                    return node.Value.GetValueWriter(dataWriters, rootDataWriter);
                }
                else
                {
                    dataWriters = node.Value.GetDataWriter(dataWriters, rootDataWriter);
                }
            }

            return Enumerable.Repeat<IValueWriter>(new DataRWContentValueRW(dataWriter), 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataReader"></param>
        /// <param name="rootDataReader"></param>
        /// <returns></returns>
        public IValueReader? GetFirstValueReader(IDataReader dataReader, IDataReader? rootDataReader = null)
        {
            return GetValueReader(dataReader, rootDataReader).FirstOrDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataWriter"></param>
        /// <param name="rootDataWriter"></param>
        /// <returns></returns>
        public IValueWriter? GetFirstValueWriter(IDataWriter dataWriter, IDataWriter? rootDataWriter = null)
        {
            return GetValueWriter(dataWriter, rootDataWriter).FirstOrDefault();
        }
    }
}