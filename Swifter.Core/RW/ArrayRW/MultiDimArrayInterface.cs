namespace Swifter.RW
{
    internal sealed class MultiDimArrayInterface<TArray, TElement> : IValueInterface<TArray> where TArray : class
    {
        public TArray ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<TArray> reader)
            {
                return reader.ReadValue();
            }

            var rw = new MultiDimArray<TArray, TElement>.FirstRW();

            valueReader.ReadArray(rw);

            return rw.GetContent();
        }

        public void WriteValue(IValueWriter valueWriter, TArray value)
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
                var rw = new MultiDimArray<TArray, TElement>.FirstRW();

                rw.Initialize(value);

                valueWriter.WriteArray(rw);
            }
        }
    }
}