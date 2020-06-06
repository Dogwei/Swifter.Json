

using System.Data;

namespace Swifter.RW
{
    internal sealed class DataRowInterface<T> : IValueInterface<T> where T : DataRow
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }

            var dataWriter = new DataRowRW<T>();

            valueReader.ReadObject(dataWriter);

            return dataWriter.datarow;
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

            var dataReader = new DataRowRW<T>
            {
                datarow = value
            };

            valueWriter.WriteObject(dataReader);
        }
    }
}