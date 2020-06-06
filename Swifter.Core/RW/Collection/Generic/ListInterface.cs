

using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class ListInterface<T, TValue> : IValueInterface<T> where T : IList<TValue>
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> reader)
            {
                return reader.ReadValue();
            }
            else
            {
                var listRW = new ListRW<T, TValue>();

                valueReader.ReadArray(listRW);

                return listRW.content;
            }

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
                valueWriter.WriteArray(new ListRW<T, TValue> { content = value });
            }
        }
    }
}