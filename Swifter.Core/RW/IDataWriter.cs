
using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    /// <summary>
    /// 提供数据的写入方法。
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    public interface IDataWriter<TKey> : IDataWriter
    {
        /// <summary>
        /// 获取键的集合。
        /// </summary>
        IEnumerable<TKey> Keys { get; }
        /// <summary>
        /// 从值读取器中读取一个值设置到指定键的值中。
        /// </summary>
        /// <param name="key">指定键</param>
        /// <param name="valueReader">值读取器</param>
        void OnWriteValue(TKey key, IValueReader valueReader);
        
        /// <summary>
        /// 从数据读取器中读取所有数据源字段到数据源的值。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        void OnWriteAll(IDataReader<TKey> dataReader);

        /// <summary>
        /// 获取指定键的值写入器实例。
        /// </summary>
        /// <param name="key">指定键</param>
        /// <returns>返回值写入器实例</returns>
        IValueWriter this[TKey key] { get; }
    }

    /// <summary>
    /// 表示一个数据写入器。
    /// </summary>
    public interface IDataWriter
    {
        /// <summary>
        /// 初始化数据源。
        /// </summary>
        void Initialize();

        /// <summary>
        /// 初始化具有指定容量的数据源。
        /// </summary>
        /// <param name="capacity">指定容量</param>
        void Initialize(int capacity);

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