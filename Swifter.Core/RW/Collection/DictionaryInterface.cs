

using System.Collections;

namespace Swifter.RW
{
    internal sealed class DictionaryInterface<T> : IValueInterface<T> where T : IDictionary
    {
        public T ReadValue(IValueReader valueReader)
        {
            var dictionaryRW = new DictionaryRW<T>();

            valueReader.ReadObject(dictionaryRW.As<string>());

            return dictionaryRW.content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var dictionaryRW = new DictionaryRW<T>
            {
                content = value
            };

            valueWriter.WriteObject(dictionaryRW.As<string>());
        }
    }
}