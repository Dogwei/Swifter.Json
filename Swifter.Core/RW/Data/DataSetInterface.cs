

using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.RW
{
    internal sealed class DataSetInterface<T> : IValueInterface<T> where T : DataSet
    {
        public T? ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }

            var dataReader = new DataSetRW<T>();

            valueReader.ReadArray(dataReader);

            return dataReader.content;
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
            else
            {
                valueWriter.WriteArray(new DataSetRW<T>
                {
                    content = value
                });
            }
        }
    }
}