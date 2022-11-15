using Swifter.Tools;
using System;

namespace Swifter.RW
{
    sealed class AsWriteAllReader<TIn, TOut> : IDataReader<TIn> where TIn: notnull where TOut: notnull
    {
        public readonly IDataReader<TOut> dataReader;

        public AsWriteAllReader(IDataReader<TOut> dataReader)
        {
            this.dataReader = dataReader;
        }

        public IValueReader this[TIn key] => dataReader[XConvert.Convert<TIn, TOut>(key)!];

        public int Count => dataReader.Count;

        public object? Content
        {
            get => dataReader.Content;
            set => dataReader.Content = value;
        }

        public Type? ContentType => dataReader.ContentType;

        public Type? ValueType => dataReader.ValueType;

        public void OnReadAll(IDataWriter<TIn> dataWriter, RWStopToken stopToken = default) =>
            dataReader.OnReadAll(new AsReadAllWriter<TOut, TIn>(dataWriter), stopToken);

        public void OnReadValue(TIn key, IValueWriter valueWriter) =>
            dataReader.OnReadValue(XConvert.Convert<TIn, TOut>(key)!, valueWriter);
    }
}