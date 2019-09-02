


namespace Swifter.RW
{
    internal sealed class DecimalInterface : IValueInterface<decimal>
    {
        public decimal ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadDecimal();
        }

        public void WriteValue(IValueWriter valueWriter, decimal value)
        {
            valueWriter.WriteDecimal(value);
        }
    }
}