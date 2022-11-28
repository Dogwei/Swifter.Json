
using System;

namespace Swifter.RW
{
    internal sealed class GuidInterface : IValueInterface<Guid>, IDefaultBehaviorValueInterface
    {
        public Guid ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadGuid();
        }

        public void WriteValue(IValueWriter valueWriter, Guid value)
        {
            valueWriter.WriteGuid(value);
        }
    }
}