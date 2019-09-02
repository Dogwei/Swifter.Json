


namespace Swifter.RW
{
    internal sealed class BooleanInterface : IValueInterface<bool>
    {
        public bool ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadBoolean();
        }

        public void WriteValue(IValueWriter valueWriter, bool value)
        {
            valueWriter.WriteBoolean(value);
        }
    }
}