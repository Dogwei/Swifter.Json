using Swifter.Tools;
using Swifter.Writers;
using System.Collections.Generic;

namespace Swifter.Readers
{
    sealed class AsWriteAllReader<TIn, TOut> : IDataReader<TIn>
    {
        public readonly IDataReader<TOut> dataReader;

        public AsWriteAllReader(IDataReader<TOut> dataReader)
        {
            this.dataReader = dataReader;
        }

        public IValueReader this[TIn key] => dataReader[XConvert<TOut>.Convert(key)];

        public IEnumerable<TIn> Keys => ArrayHelper.CreateAsIterator<TOut, TIn>(dataReader.Keys);

        public int Count => dataReader.Count;

        public object ReferenceToken => dataReader.ReferenceToken;

        public void OnReadAll(IDataWriter<TIn> dataWriter) =>
            dataReader.OnReadAll(new AsReadAllWriter<TOut, TIn>(dataWriter));

        public void OnReadAll(IDataWriter<TIn> dataWriter, IValueFilter<TIn> valueFilter) => 
            dataReader.OnReadAll(new AsReadAllWriter<TOut, TIn>(dataWriter), new AsReadAllFilter<TOut, TIn>(valueFilter));

        public void OnReadValue(TIn key, IValueWriter valueWriter) =>
            dataReader.OnReadValue(XConvert<TOut>.Convert(key), valueWriter);
    }
}