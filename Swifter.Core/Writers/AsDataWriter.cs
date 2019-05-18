using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Collections.Generic;

namespace Swifter.Writers
{
    /// <summary>
    /// 数据写入器键类型转换的接口。
    /// </summary>
    internal interface IAsDataWriter
    {
        IDataWriter Content { get; }
    }

    /// <summary>
    /// 数据写入器键类型转换的类型。
    /// </summary>
    /// <typeparam name="TIn">输入类型</typeparam>
    /// <typeparam name="TOut">输出类型</typeparam>
    internal sealed class AsDataWriter<TIn, TOut> : IDataWriter<TOut>, IAsDataWriter, IDirectContent
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
        public IEnumerable<TOut> Keys => ArrayHelper.CreateAsIterator<TIn, TOut>(dataWriter.Keys);

        /// <summary>
        /// 获取数据源键的数量。
        /// </summary>
        public int Count => dataWriter.Count;

        IDataWriter IAsDataWriter.Content => dataWriter;

        object IDirectContent.DirectContent
        {
            get
            {
                if (dataWriter is IDirectContent directContent)
                {
                    return directContent.DirectContent;
                }

                throw new NotSupportedException($"This data {"writer"} does not support direct {"get"} content.");
            }
            set
            {
                if (dataWriter is IDirectContent directContent)
                {
                    directContent.DirectContent = value;

                    return;
                }

                throw new NotSupportedException($"This data {"writer"} does not support direct {"set"} content.");
            }
        }

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
        /// 从值读取器中读取一个值设置到指定键的值中。
        /// </summary>
        /// <param name="key">指定键</param>
        /// <param name="valueReader">值读取器</param>
        public void OnWriteValue(TOut key, IValueReader valueReader)
        {
            dataWriter.OnWriteValue(XConvert<TIn>.Convert(key), valueReader);
        }

        public void OnWriteAll(IDataReader<TOut> dataReader)
        {
            dataWriter.OnWriteAll(new AsWriteAllReader<TIn, TOut>(dataReader));
        }
    }
}