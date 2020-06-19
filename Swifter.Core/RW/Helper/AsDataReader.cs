using Swifter.RW;
using Swifter.Tools;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Swifter.RW
{
    /// <summary>
    /// 数据读取器键类型转换的接口。
    /// </summary>
    public interface IAsDataReader
    {
        /// <summary>
        /// 原始数据读取器。
        /// </summary>
        IDataReader Original { get; }

        /// <summary>
        /// 执行输入类型。
        /// </summary>
        /// <param name="invoker">泛型执行器</param>
        void InvokeTIn(IGenericInvoker invoker);

        /// <summary>
        /// 执行输出类型方法。
        /// </summary>
        /// <param name="invoker">泛型执行器</param>
        void InvokeTOut(IGenericInvoker invoker);
    }

    /// <summary>
    /// 数据读取器键类型转换的类型。
    /// </summary>
    /// <typeparam name="TIn">输入类型</typeparam>
    /// <typeparam name="TOut">输出类型</typeparam>
    public sealed class AsDataReader<TIn, TOut> : IDataReader<TOut>, IAsDataReader
    {
        /// <summary>
        /// 原始数据读取器。
        /// </summary>
        public readonly IDataReader<TIn> dataReader;

        /// <summary>
        /// 创建数据读取器键类型转换类的实例。
        /// </summary>
        /// <param name="dataReader">原始数据读取器</param>
        public AsDataReader(IDataReader<TIn> dataReader)
        {
            this.dataReader = dataReader;
        }

        /// <summary>
        /// 转换键，并返回该键对应的值读取器。
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回值读取器</returns>
        public IValueReader this[TOut key] => dataReader[XConvert<TIn>.Convert(key)];

        /// <summary>
        /// 获取转换后的键集合。
        /// </summary>
        public IEnumerable<TOut> Keys => dataReader.Keys.Select(key => XConvert.Convert<TIn, TOut>(key));

        /// <summary>
        /// 获取数据源键的数量。
        /// </summary>
        public int Count => dataReader.Count;

        /// <summary>
        /// 获取原始数据读取器。
        /// </summary>
        public IDataReader Original => dataReader;

        /// <summary>
        /// 获取原数据读取器的数据源类型。
        /// </summary>
        public Type ContentType => dataReader.ContentType;

        /// <summary>
        /// 获取或设置原数据读取器的数据源。
        /// </summary>
        public object Content
        {
            get => dataReader.Content;
            set => dataReader.Content = value;
        }

        /// <summary>
        /// 将数据中的所有转换后的键与值写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        public void OnReadAll(IDataWriter<TOut> dataWriter) =>
            dataReader.OnReadAll(new AsReadAllWriter<TIn, TOut>(dataWriter));

        /// <summary>
        /// 转换键，并将该键对应的值写入到值写入器中。
        /// </summary>
        /// <param name="key">指定键</param>
        /// <param name="valueWriter">值写入器</param>
        public void OnReadValue(TOut key, IValueWriter valueWriter) =>
            dataReader.OnReadValue(XConvert<TIn>.Convert(key), valueWriter);

        /// <summary>
        /// 执行输入类型方法。
        /// </summary>
        /// <param name="invoker">泛型执行器</param>
        public void InvokeTIn(IGenericInvoker invoker)
        {
            invoker.Invoke<TIn>();
        }

        /// <summary>
        /// 执行输出类型方法。
        /// </summary>
        /// <param name="invoker">泛型执行器</param>
        public void InvokeTOut(IGenericInvoker invoker)
        {
            invoker.Invoke<TOut>();
        }
    }
}