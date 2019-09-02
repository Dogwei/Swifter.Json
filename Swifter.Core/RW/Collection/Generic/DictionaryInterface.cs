

using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class DictionaryInterface<T, TKey, TValue> : IValueInterface<T> where T : IDictionary<TKey, TValue>
    {
        public T ReadValue(IValueReader valueReader)
        {
            var dictionaryRW = new DictionaryRW<T, TKey, TValue>();

            if (typeof(TKey) == typeof(string))
            {
                valueReader.ReadObject(Unsafe.As<IDataWriter<string>>(dictionaryRW));
            }
            else
            {
                if (valueReader is IMapValueReader mapReader)
                {
                    mapReader.ReadMap(dictionaryRW);
                }
                else
                {
                    valueReader.ReadObject(dictionaryRW.As<string>());
                }
            }

            return dictionaryRW.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var dictionaryRW = new DictionaryRW<T, TKey, TValue>();

            dictionaryRW.Initialize(value);

            if (typeof(TKey) == typeof(string))
            {
                valueWriter.WriteObject(Unsafe.As<IDataReader<string>>(dictionaryRW));
            }
            else
            {
                if (valueWriter is IMapValueWriter mapWriter)
                {
                    mapWriter.WriteMap(dictionaryRW);
                }
                else
                {
                    valueWriter.WriteObject(dictionaryRW.As<string>());
                }
            }

        }
    }
}