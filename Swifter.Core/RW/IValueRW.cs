


using System;

namespace Swifter.RW
{
    /// <summary>
    /// 基础类型的值读写器
    /// </summary>
    public interface IValueRW : IValueReader, IValueWriter
    {

        /// <summary>
        /// 获取值的类型。
        /// <see langword="null"/> 表示未知类型。
        /// </summary>
        new Type? ValueType { get; }
    }

    /// <summary>
    /// 自定义类型的值读写器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IValueRW<T> :IValueReader<T>, IValueWriter<T>
    {
    }
}