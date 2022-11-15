using System;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// 提供数据读写器的内容值读写器。
    /// </summary>
    public sealed class DataRWContentValueRW : BaseDirectRW
    {
        readonly object rw;

        /// <summary>
        /// 初始化数据读取器的内容值读写器。
        /// </summary>
        /// <param name="reader">数据读取器</param>
        public DataRWContentValueRW(IDataReader reader)
        {
            rw = reader;
        }

        /// <summary>
        /// 初始化数据读取器的内容值读写器。
        /// </summary>
        /// <param name="writer">数据写入器</param>
        public DataRWContentValueRW(IDataWriter writer)
        {
            rw = writer;
        }

        /// <summary>
        /// 初始化数据读取器的内容值读写器。
        /// </summary>
        /// <param name="rw">数据读写器</param>
        public DataRWContentValueRW(IDataRW rw)
        {
            this.rw = rw;
        }

        /// <summary>
        /// 获取此数据读写器的内容类型。
        /// </summary>
        public override Type? ValueType
        {
            get
            {
                if (rw is IDataReader reader)
                {
                    return reader.ContentType;
                }
                else
                {
                    return Unsafe.As<IDataWriter>(rw).ContentType;
                }
            }
        }

        /// <summary>
        /// 获取此数据读写器的内容。
        /// </summary>
        public override object? DirectRead()
        {
            if (rw is IDataReader reader)
            {
                return reader.Content;
            }
            else
            {
                return Unsafe.As<IDataWriter>(rw).Content;
            }
        }

        /// <summary>
        /// 设置此数据读写器的内容。
        /// </summary>
        public override void DirectWrite(object? value)
        {
            if (rw is IDataReader reader)
            {
                reader.Content = value;
            }
            else
            {
                Unsafe.As<IDataWriter>(rw).Content = value;
            }
        }
    }
}