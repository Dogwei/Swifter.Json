

using System.Collections;

namespace Swifter.RW
{
    internal sealed class ListInterface<T> : IValueInterface<T> where T : IList
    {
        public T ReadValue(IValueReader valueReader)
        {
            var listRW = new ListRW<T>();

            valueReader.ReadArray(listRW);

            return listRW.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var listRW = new ListRW<T>();

            listRW.Initialize(value);

            valueWriter.WriteArray(listRW);
        }
    }
}