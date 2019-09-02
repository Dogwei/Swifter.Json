


namespace Swifter.RW
{
    internal sealed class UInt32Interface : IValueInterface<uint>
    {
        public uint ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadUInt32();
        }

        public void WriteValue(IValueWriter valueWriter, uint value)
        {
            valueWriter.WriteUInt32(value);
        }
    }
}