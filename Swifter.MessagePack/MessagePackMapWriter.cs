using Swifter.Readers;
using Swifter.RW;
using Swifter.Writers;
using System.Collections.Generic;

namespace Swifter.MessagePack
{
    sealed class MessagePackMapWriter<TKey> : IDataWriter<TKey>
    {
        private readonly MessagePackSerializer serializer;

        public MessagePackMapWriter(MessagePackSerializer serializer)
        {
            this.serializer = serializer;
        }

        public IValueWriter this[TKey key]
        {
            get
            {
                ValueInterface<TKey>.WriteValue(serializer, key);

                return serializer;
            }
        }

        public IEnumerable<TKey> Keys => null;

        public int Count => 0;

        public void Initialize()
        {
        }

        public void Initialize(int capacity)
        {
        }

        public void OnWriteAll(IDataReader<TKey> dataReader)
        {
        }

        public void OnWriteValue(TKey key, IValueReader valueReader)
        {
            this[key].DirectWrite(valueReader.DirectRead());
        }
    }
}