using System.Data;
using static Swifter.RW.DataTableRW;

namespace Swifter.RW
{
    internal sealed class DataTableInterface<T> : IValueInterface<T> where T : DataTable
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> reader)
            {
                return reader.ReadValue();
            }

            var writer = new DataTableRW<T>(GetDataTableRWOptions(valueReader));

            valueReader.ReadArray(writer);

            return writer.datatable;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            if (valueWriter is IValueWriter<T> weiter)
            {
                weiter.WriteValue(value);

                return;
            }

            var reader = new DataTableRW<T>(GetDataTableRWOptions(valueWriter))
            {
                datatable = value
            };

            valueWriter.WriteArray(reader);
        }
    }
}