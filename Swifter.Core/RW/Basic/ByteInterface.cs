


namespace Swifter.RW
{
    internal sealed class ByteInterface : IValueInterface<byte>
    {
        public byte ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadByte();
        }

        public void WriteValue(IValueWriter valueWriter, byte value)
        {
            valueWriter.WriteByte(value);
        }
    }
}