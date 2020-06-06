
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    /// <summary>
    /// 数据写入器键类型转换的接口。
    /// </summary>
    public interface IAsDataWriter
    {
        /// <summary>
        /// 原始数据写入器。
        /// </summary>
        IDataWriter Original { get; }

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
    /// 数据写入器键类型转换的类型。
    /// </summary>
    /// <typeparam name="TIn">输入类型</typeparam>
    /// <typeparam name="TOut">输出类型</typeparam>
    public sealed class AsDataWriter<TIn, TOut> : IDataWriter<TOut>, IAsDataWriter
    {
        /// <summary>
        /// 原始数据写入器。
        /// </summary>
        public readonly IDataWriter<TIn> dataWriter;

        /// <summary>
        /// 创建数据写入器键类型转换类的实例。
        /// </summary>
        /// <param name="dataWriter">原始数据写入器</param>
        public AsDataWriter(IDataWriter<TIn> dataWriter)
        {
            this.dataWriter = dataWriter;
        }

        /// <summary>
        /// 转换键，并返回该键对应的值写入器。
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>返回值写入器</returns>
        public IValueWriter this[TOut key] => dataWriter[XConvert<TIn>.Convert(key)];

        /// <summary>
        /// 获取转换后的键集合。
        /// </summary>
        public IEnumerable<TOut> Keys => ArrayHelper.CreateConvertIterator<TIn, TOut>(dataWriter.Keys);

        /// <summary>
        /// 获取数据源键的数量。
        /// </summary>
        public int Count => dataWriter.Count;

        /// <summary>
        /// 获取原数据写入器。
        /// </summary>
        public IDataWriter Original => dataWriter;

        /// <summary>
        /// 获取或设置原数据写入器的数据源。
        /// </summary>
        public object Content
        {
            get => dataWriter.Content;
            set => dataWriter.Content = value;
        }

        /// <summary>
        /// 获取原数据写入器的数据源类型。
        /// </summary>
        public Type ContentType => dataWriter.ContentType;

        /// <summary>
        /// 初始化数据源。
        /// </summary>
        public void Initialize()
        {
            dataWriter.Initialize();
        }

        /// <summary>
        /// 初始化具有指定容量的数据源。
        /// </summary>
        /// <param name="capacity">指定容量</param>
        public void Initialize(int capacity)
        {
            dataWriter.Initialize(capacity);
        }

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

        /// <summary>
        /// 从值读取器中读取一个值设置到指定键的值中。
        /// </summary>
        /// <param name="key">指定键</param>
        /// <param name="valueReader">值读取器</param>
        public void OnWriteValue(TOut key, IValueReader valueReader)
        {
            dataWriter.OnWriteValue(XConvert<TIn>.Convert(key), valueReader);
        }

        /// <summary>
        /// 从数据读取器中读取所有数据源字段到数据源的值。
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        public void OnWriteAll(IDataReader<TOut> dataReader)
        {
            dataWriter.OnWriteAll(new AsWriteAllReader<TIn, TOut>(dataReader));
        }
    }
}