
using Swifter.Tools;

using System;

namespace Swifter.RW
{
    internal sealed class DateTimeOffsetInterface : IValueInterface<DateTimeOffset>, IDefaultBehaviorValueInterface
    {
        public DateTimeOffset ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadDateTimeOffset();
        }

        public void WriteValue(IValueWriter valueWriter, DateTimeOffset value)
        {
            valueWriter.WriteDateTimeOffset(value);
        }
    }
}