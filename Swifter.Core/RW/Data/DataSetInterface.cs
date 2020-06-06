

using System.Data;

namespace Swifter.RW
{
    internal sealed class DataSetInterface<T> : IValueInterface<T> where T : DataSet
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }
            
            var dataReader = new DataSetRW<T>();

            valueReader.ReadArray(dataReader);

            return dataReader.dataset;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            if (valueWriter is IValueWriter<T> tWriter)
            {
                tWriter.WriteValue(value);

                return;
            }

            var dataReader = new DataSetRW<T>
            {
                dataset = value
            };

            valueWriter.WriteArray(dataReader);
        }
    }
}