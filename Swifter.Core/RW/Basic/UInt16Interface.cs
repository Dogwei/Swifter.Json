


namespace Swifter.RW
{
    internal sealed class UInt16Interface : IValueInterface<ushort>
    {
        public ushort ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadUInt16();
        }

        public void WriteValue(IValueWriter valueWriter, ushort value)
        {
            valueWriter.WriteUInt16(value);
        }
    }
}