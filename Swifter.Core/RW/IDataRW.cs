

using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    /// <summary>
    /// 数据读写器
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    public interface IDataRW<TKey> : IDataRW, IDataReader<TKey>, IDataWriter<TKey> where TKey : notnull
    {
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
        /// -1 表示未知数量。
        /// </summary>
        new int Count { get; }

        /// <summary>
        /// 获取数据源的类型。
        /// <see langword="null"/> 表示无数据源或不可读。
        /// </summary>
        new Type? ContentType { get; }

        /// <summary>
        /// 获取值的类型。
        /// <see langword="null"/> 标识值的类型不固定或未知。
        /// </summary>
        new Type? ValueType { get; }

        /// <summary>
        /// 获取或设置数据源。
        /// </summary>
        new object? Content { get; set; }
    }

    /// <summary>
    /// 表示一个数组读写器。
    /// </summary>
    public interface IArrayRW : IDataRW<int>, IArrayReader, IArrayWriter
    {

    }

    /// <summary>
    /// 表示一个有键集合的数据读写器。
    /// </summary>
    /// <typeparam name="TKey">键的类型</typeparam>
    public interface IHasKeysDataRW<TKey> : IDataRW<TKey>, IHasKeysDataReader<TKey>, IHasKeysDataWriter<TKey> where TKey : notnull
    {
        /// <summary>
        /// 键的集合。
        /// </summary>
        new IEnumerable<TKey> Keys { get; }
    }

    /// <summary>
    /// 表示一个对象读写器。
    /// </summary>
    public interface IObjectRW : IHasKeysDataRW<string>, IObjectReader, IObjectWriter
    {

    }
}