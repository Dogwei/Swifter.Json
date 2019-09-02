


namespace Swifter.RW
{
    internal sealed class SByteInterface : IValueInterface<sbyte>
    {
        public sbyte ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadSByte();
        }

        public void WriteValue(IValueWriter valueWriter, sbyte value)
        {
            valueWriter.WriteSByte(value);
        }
    }
}