

using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    /// <summary>
    /// 数据读写器
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    public interface IDataRW<TKey> : IDataRW, IDataReader<TKey>, IDataWriter<TKey>
    {
        /// <summary>
        /// 获取键集合。
        /// </summary>
        new IEnumerable<TKey> Keys { get; }
        /// <summary>
        /// 获取指定键的值读写器实例。
        /// </summary>
        /// <param name="key">指定键</param>
        /// <returns>返回值读写器实例</returns>
        new IValueRW this[TKey key] { get; }
    }


    /// <summary>
    /// 表示数据读写器
    /// </summary>
    public interface IDataRW : IDataReader, IDataWriter
    {
        /// <summary>
        /// 获取读写器的键的数量。
        /// </summary>
        new int Count { get; }

        /// <summary>
        /// 获取数据源的类型。
        /// </summary>
        new Type ContentType { get; }

        /// <summary>
        /// 获取或设置数据源。
        /// </summary>
        new object Content { get; set; }
    }
}