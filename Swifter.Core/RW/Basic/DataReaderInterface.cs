using Swifter.Tools;

namespace Swifter.RW
{
    internal sealed class DataReaderInterface<T, TKey> : IValueInterface<T> where T : IDataReader<TKey> where TKey : notnull
    {
        public T? ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }

            var value = valueReader.DirectRead();

            if (value is null)
            {
                return default;
            }

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

            return XConvert.Convert<T>(value);
        }

        public void WriteValue(IValueWriter valueWriter, T? value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (valueWriter is IValueWriter<T> tWriter)
            {
                tWriter.WriteValue(value);
            }
            else if (value is IArrayReader arrayReader)
            {
                valueWriter.WriteArray(arrayReader);
            }
            else
            {
                valueWriter.WriteObject(value.As<string>());
            }
        }
    }
}