namespace Swifter.Reflection
{
    sealed class SerializationReferenceInfo
    {
        public readonly int TargetIndex;

        public SerializationReferenceInfo(int targetIndex)
        {
            TargetIndex = targetIndex;
        }
    }
}