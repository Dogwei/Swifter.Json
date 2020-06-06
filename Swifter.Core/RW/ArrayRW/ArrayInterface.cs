namespace Swifter.RW
{
    internal sealed class ArrayInterface<TElement> : IValueInterface<TElement[]>
    {
        public TElement[] ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<TElement[]> reader)
            {
                return reader.ReadValue();
            }
            else
            {
                var rw = new ArrayRW<TElement>();

                valueReader.ReadArray(rw);

                return rw.GetContent();
            }
        }

        public void WriteValue(IValueWriter valueWriter, TElement[] value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (valueWriter is IValueWriter<TElement[]> writer)
            {
                writer.WriteValue(value);
            }
            else
            {
                valueWriter.WriteArray(new ArrayRW<TElement>(value));
            }
        }
    }
}