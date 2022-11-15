namespace Swifter.RW
{
    internal sealed class MultiDimArrayInterface<TArray, TElement> : IValueInterface<TArray> where TArray : class
    {
        public TArray? ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<TArray> reader)
            {
                return reader.ReadValue();
            }

            var rw = new MultiDimArrayRW<TArray, TElement>();

            valueReader.ReadArray(rw);

            return rw.Content;
        }

        public void WriteValue(IValueWriter valueWriter, TArray? value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (valueWriter is IValueWriter<TArray> writer)
            {
                writer.WriteValue(value);
            }
            else
            {
                valueWriter.WriteArray(new MultiDimArrayRW<TArray, TElement>
                {
                    Content = value
                });
            }
        }
    }
}