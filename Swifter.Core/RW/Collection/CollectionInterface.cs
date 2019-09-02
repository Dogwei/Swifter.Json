

using System.Collections;

namespace Swifter.RW
{
    internal sealed class CollectionInterface<T> : IValueInterface<T> where T : ICollection
    {
        public T ReadValue(IValueReader valueReader)
        {
            var collectionRW = new CollectionRW<T>();

            valueReader.ReadArray(collectionRW);

            return collectionRW.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var collectionRW = new CollectionRW<T>();

            collectionRW.Initialize(value);

            valueWriter.WriteArray(collectionRW);
        }
    }
}