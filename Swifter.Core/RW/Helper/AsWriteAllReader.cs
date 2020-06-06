using Swifter.Tools;
using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    sealed class AsWriteAllReader<TIn, TOut> : IDataReader<TIn>
    {
        public readonly IDataReader<TOut> dataReader;

        public AsWriteAllReader(IDataReader<TOut> dataReader)
        {
            this.dataReader = dataReader;
        }

        public IValueReader this[TIn key] => dataReader[XConvert<TOut>.Convert(key)];

        public IEnumerable<TIn> Keys => ArrayHelper.CreateConvertIterator<TOut, TIn>(dataReader.Keys);

        public int Count => dataReader.Count;

        public object Content
        {
            get => dataReader.Content;
            set => dataReader.Content = value;
        }

        public Type ContentType => dataReader.ContentType;

        public void OnReadAll(IDataWriter<TIn> dataWriter) =>
            dataReader.OnReadAll(new AsReadAllWriter<TOut, TIn>(dataWriter));

        public void OnReadValue(TIn key, IValueWriter valueWriter) =>
            dataReader.OnReadValue(XConvert<TOut>.Convert(key), valueWriter);
    }
}