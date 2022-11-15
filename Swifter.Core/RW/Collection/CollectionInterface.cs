

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.RW
{
    internal sealed class CollectionInterface<T> : IValueInterface<T> where T : ICollection
    {
        public T? ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> reader)
            {
                return reader.ReadValue();
            }

            var collectionRW = new CollectionRW<T>();

            valueReader.ReadArray(collectionRW);

            return collectionRW.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T? value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (value is IValueWriter<T> writer)
            {
                writer.WriteValue(value);
            }
            else
            {
                valueWriter.WriteArray(new CollectionRW<T> { Content = value });
            }
        }
    }
}