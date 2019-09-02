

using System.Collections;

namespace Swifter.RW
{
    internal sealed class DictionaryInterface<T> : IValueInterface<T> where T : IDictionary
    {
        public T ReadValue(IValueReader valueReader)
        {
            var dictionaryRW = new DictionaryRW<T>();

            if (valueReader is IMapValueReader mapReader)
            {
                mapReader.ReadMap(dictionaryRW);
            }
            else
            {
                valueReader.ReadObject(dictionaryRW.As<string>());
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

            var dictionaryRW = new DictionaryRW<T>();

            dictionaryRW.Initialize(value);

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