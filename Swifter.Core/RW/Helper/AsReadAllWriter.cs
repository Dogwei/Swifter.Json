
using Swifter.Tools;
using System;

namespace Swifter.RW
{
    sealed class AsReadAllWriter<TIn, TOut> : IDataWriter<TIn> where TIn : notnull where TOut : notnull
    {
        public readonly IDataWriter<TOut> dataWriter;

        public AsReadAllWriter(IDataWriter<TOut> dataWriter)
        {
            this.dataWriter = dataWriter;
        }

        public IValueWriter this[TIn key] => dataWriter[XConvert.Convert<TIn, TOut>(key)!];

        public int Count => dataWriter.Count;

        public void Initialize()=> dataWriter.Initialize();

        public void Initialize(int capacity) => dataWriter.Initialize(capacity);

        public void OnWriteAll(IDataReader<TIn> dataReader, RWStopToken stopToken = default) =>
            dataWriter.OnWriteAll(new AsWriteAllReader<TOut, TIn>(dataReader), stopToken);

        public void OnWriteValue(TIn key, IValueReader valueReader)=>
            dataWriter.OnWriteValue(XConvert.Convert<TIn, TOut>(key)!, valueReader);

        public object? Content
        {
            get => dataWriter.Content;
            set => dataWriter.Content = value;
        }

        public Type? ContentType => dataWriter.ContentType;

        public Type? ValueType => dataWriter.ValueType;
    }
}