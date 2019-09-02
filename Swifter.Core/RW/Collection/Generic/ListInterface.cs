

using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class ListInterface<T, TValue> : IValueInterface<T> where T : IList<TValue>
    {
        public T ReadValue(IValueReader valueReader)
        {
            var listRW = new ListRW<T, TValue>();
            
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

            var listRW = new ListRW<T, TValue>();

            listRW.Initialize(value);

            valueWriter.WriteArray(listRW);
        }
    }
}