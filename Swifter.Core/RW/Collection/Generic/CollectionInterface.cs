

using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class CollectionInterface<T, TValue> : IValueInterface<T> where T : ICollection<TValue>
    {
        public T ReadValue(IValueReader valueReader)
        {
            var collectionRW = new CollectionRW<T, TValue>();

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

            var collectionRW = new CollectionRW<T, TValue>();

            collectionRW.Initialize(value);

            valueWriter.WriteArray(collectionRW);
        }
    }
}