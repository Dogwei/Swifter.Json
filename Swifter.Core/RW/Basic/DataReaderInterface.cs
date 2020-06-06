using Swifter.Tools;

namespace Swifter.RW
{
    internal sealed class DataReaderInterface<T, TKey> : IValueInterface<T> where T : IDataReader<TKey>
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }

            var value = valueReader.DirectRead();

            if (value is T tValue)
            {
                return tValue;
            }

            var reader = RWHelper.CreateReader(value);

            if (reader is T tResult)
            {
                return tResult;
            }

            if (reader != null && reader.As<TKey>() is T tResult2)
            {
                return tResult2;
            }

            return XConvert<T>.FromObject(value);
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (valueWriter is IValueWriter<T> tWriter)
            {
                tWriter.WriteValue(value);
            }
            else if (valueWriter is IValueWriter<IDataReader<TKey>> tWriter2)
            {
                tWriter2.WriteValue(value);
            }
            else if (valueWriter is IValueWriter<IDataReader> tWriter3)
            {
                tWriter3.WriteValue(value);
            }
            else if (RWHelper.IsArrayKey<TKey>())
            {
                valueWriter.WriteArray(value.As<int>());
            }
            else
            {
                valueWriter.WriteObject(value.As<string>());
            }
        }
    }
}