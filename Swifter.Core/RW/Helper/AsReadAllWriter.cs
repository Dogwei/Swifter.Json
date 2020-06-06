
using Swifter.Tools;
using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    sealed class AsReadAllWriter<TIn, TOut> : IDataWriter<TIn>
    {
        public readonly IDataWriter<TOut> dataWriter;

        public AsReadAllWriter(IDataWriter<TOut> dataWriter)
        {
            this.dataWriter = dataWriter;
        }

        public IValueWriter this[TIn key] => dataWriter[XConvert<TOut>.Convert(key)];

        public IEnumerable<TIn> Keys => ArrayHelper.CreateConvertIterator<TOut, TIn>(dataWriter.Keys);

        public int Count => dataWriter.Count;

        public void Initialize()=> dataWriter.Initialize();

        public void Initialize(int capacity) => dataWriter.Initialize(capacity);

        public void OnWriteAll(IDataReader<TIn> dataReader) =>
            dataWriter.OnWriteAll(new AsWriteAllReader<TOut, TIn>(dataReader));

        public void OnWriteValue(TIn key, IValueReader valueReader)=>
            dataWriter.OnWriteValue(XConvert<TOut>.Convert(key), valueReader);

        public object Content
        {
            get => dataWriter.Content;
            set => dataWriter.Content = value;
        }

        public Type ContentType => dataWriter.ContentType;
    }
}