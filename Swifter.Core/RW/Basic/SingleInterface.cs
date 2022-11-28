


namespace Swifter.RW
{
    internal sealed class SingleInterface : IValueInterface<float>, IDefaultBehaviorValueInterface
    {
        public float ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadSingle();
        }

        public void WriteValue(IValueWriter valueWriter, float value)
        {
            valueWriter.WriteSingle(value);
        }
    }
}