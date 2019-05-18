using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;

namespace Swifter.Formatters
{
    sealed class AuxiliaryWriter<Key> : IDataWriter<Key>, IDirectContent
    {
        [ThreadStatic]
        public static IDataWriter<Key> ThreadDataWriter;

        static AuxiliaryWriter()
        {
            ValueInterface<AuxiliaryWriter<Key>>.SetInterface(new AuxiliaryInterface<Key>());
        }

        public readonly IDataWriter<Key> dataWriter;

        public AuxiliaryWriter()
        {
            dataWriter = ThreadDataWriter;
        }

        public IValueWriter this[Key key] => dataWriter[key];

        public int Count => dataWriter.Count;

        public IEnumerable<Key> Keys => dataWriter.Keys;

        public object DirectContent
        {
            get
            {
                if (dataWriter is IDirectContent directContent)
                {
                    return directContent.DirectContent;
                }

                throw new NotSupportedException($"This data {"writer"} does not support direct {"get"} content.");
            }
            set
            {
                if (dataWriter is IDirectContent directContent)
                {
                    directContent.DirectContent = value;

                    return;
                }

                throw new NotSupportedException($"This data {"writer"} does not support direct {"set"} content.");
            }
        }

        public void Initialize()
        {
        }

        public void Initialize(int capacity)
        {
        }

        public void OnWriteValue(Key key, IValueReader valueReader) => dataWriter.OnWriteValue(key, valueReader);

        public void OnWriteAll(IDataReader<Key> dataReader) => dataWriter.OnWriteAll(dataReader);
    }
}