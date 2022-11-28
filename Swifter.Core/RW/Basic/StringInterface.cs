


namespace Swifter.RW
{
    internal sealed class StringInterface : IValueInterface<string>, IDefaultBehaviorValueInterface
    {
        public string? ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadString();
        }

        public void WriteValue(IValueWriter valueWriter, string? value)
        {
            valueWriter.WriteString(value);
        }
    }
}