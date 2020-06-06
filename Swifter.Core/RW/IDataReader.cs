
using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    /// <summary>
    /// 提供数据的读取方法。
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    public interface IDataReader<TKey> : IDataReader
    {
        /// <summary>
        /// 获取键的集合。
        /// </summary>
        IEnumerable<TKey> Keys { get; }

        /// <summary>
        /// 获取指定键的值读取器实例。
        /// </summary>
        /// <param name="key">指定键</param>
        /// <returns>返回值读取器实例</returns>
        IValueReader this[TKey key] { get; }

        /// <summary>
        /// 将指定键对应的值写入到值写入器中。
        /// </summary>
        /// <param name="key">指定键</param>
        /// <param name="valueWriter">值写入器</param>
        void OnReadValue(TKey key, IValueWriter valueWriter);

        /// <summary>
        /// 将数据中的所有键与值写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        void OnReadAll(IDataWriter<TKey> dataWriter);
    }

    /// <summary>
    /// 表示一个数据读取器。
    /// </summary>
    public interface IDataReader
    {
        /// <summary>
        /// 获取数据源键的数量。
        /// -1 表示未知数量。
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 获取数据源的类型。
        /// </summary>
        Type ContentType { get; }

        /// <summary>
        /// 获取或设置数据源。
        /// </summary>
        object Content { get; set; }
    }
}