
using System;
using static Swifter.RW.DataTableRW;

namespace Swifter.RW
{
    sealed class DbDataReaderInterface<T> : IValueInterface<T> where T : class, System.Data.IDataReader
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> reader)
            {
                return reader.ReadValue();
            }

            throw new NotSupportedException();
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            var reader = new DbDataReaderReader(value, GetDataTableRWOptions(valueWriter));

            valueWriter.WriteArray(reader);
        }
    }
}