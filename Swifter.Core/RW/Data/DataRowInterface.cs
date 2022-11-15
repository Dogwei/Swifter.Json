

using System.Data;

namespace Swifter.RW
{
    internal sealed class DataRowInterface<T> : IValueInterface<T> where T : DataRow
    {
        public T? ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }

            var dataWriter = new DataRowRW<T>(new DataTable());

            valueReader.ReadObject(dataWriter);

            return dataWriter.content;
        }

        public void WriteValue(IValueWriter valueWriter, T? value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if(valueWriter is IValueWriter<T> writer)
            {
                writer.WriteValue(value);
            }
            else
            {
                var rw = new DataRowRW<T>(value.Table, value);

                rw.Initialize();

                valueWriter.WriteObject(rw);
            }
        }
    }
}