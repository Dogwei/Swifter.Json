


namespace Swifter.RW
{
    internal sealed class Int16Interface : IValueInterface<short>
    {
        public short ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadInt16();
        }

        public void WriteValue(IValueWriter valueWriter, short value)
        {
            valueWriter.WriteInt16(value);
        }
    }
}