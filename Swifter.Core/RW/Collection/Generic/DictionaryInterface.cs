

using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class DictionaryInterface<T, TKey, TValue> : IValueInterface<T> where T : IDictionary<TKey, TValue>
    {
        public T ReadValue(IValueReader valueReader)
        {
            var dictionaryRW = new DictionaryRW<T, TKey, TValue>();

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

            var dictionaryRW = new DictionaryRW<T, TKey, TValue>
            {
                content = value
            };

            valueWriter.WriteObject(dictionaryRW.As<string>());
        }
    }
}