

using System.Collections;

namespace Swifter.RW
{
    internal sealed class ListInterface<T> : IValueInterface<T> where T : IList
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> reader)
            {
                return reader.ReadValue();
            }

            var listRW = new ListRW<T>();

            valueReader.ReadArray(listRW);

            return listRW.content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (valueWriter is IValueWriter<T> writer)
            {
                writer.WriteValue(value);
            }
            else
            {
                valueWriter.WriteArray(new ListRW<T> { content = value });
            }
        }
    }
}