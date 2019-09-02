


namespace Swifter.RW
{
    internal sealed class Int32Interface : IValueInterface<int>
    {
        public int ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadInt32();
        }

        public void WriteValue(IValueWriter valueWriter, int value)
        {
            valueWriter.WriteInt32(value);
        }
    }
}