


namespace Swifter.RW
{
    internal sealed class UInt64Interface : IValueInterface<ulong>
    {
        public ulong ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadUInt64();
        }

        public void WriteValue(IValueWriter valueWriter, ulong value)
        {
            valueWriter.WriteUInt64(value);
        }
    }
}