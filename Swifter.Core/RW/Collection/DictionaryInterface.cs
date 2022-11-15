using System.Collections;

namespace Swifter.RW
{
    internal sealed class DictionaryInterface<T> : IValueInterface<T> where T : IDictionary
    {
        public T? ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> reader)
            {
                return reader.ReadValue();
            }

            var dictionaryRW = new DictionaryRW<T>();

            valueReader.ReadObject(dictionaryRW.As<string>());

            return dictionaryRW.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T? value)
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
                valueWriter.WriteObject(new DictionaryRW<T> { Content = value }.As<string>());
            }
        }
    }
}