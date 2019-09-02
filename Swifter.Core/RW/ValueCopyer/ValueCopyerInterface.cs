namespace Swifter.RW
{
    sealed class ValueCopyerInterface : IValueInterface<ValueCopyer>
    {
        public ValueCopyer ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<ValueCopyer> reader)
            {
                return reader.ReadValue();
            }

            var valueCopyer = new ValueCopyer();
            var value = valueReader.DirectRead();

            ValueInterface.WriteValue(valueCopyer, value);

            return valueCopyer;
        }

        public void WriteValue(IValueWriter valueWriter, ValueCopyer value)
        {
            value.WriteTo(valueWriter);
        }
    }
}