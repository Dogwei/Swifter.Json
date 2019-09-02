


namespace Swifter.RW
{
    internal sealed class Int64Interface : IValueInterface<long>
    {
        public long ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadInt64();
        }

        public void WriteValue(IValueWriter valueWriter, long value)
        {
            valueWriter.WriteInt64(value);
        }
    }
}