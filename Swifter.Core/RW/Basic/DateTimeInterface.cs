

using System;

namespace Swifter.RW
{
    internal sealed class DateTimeInterface : IValueInterface<DateTime>
    {
        public DateTime ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadDateTime();
        }

        public void WriteValue(IValueWriter valueWriter, DateTime value)
        {
            valueWriter.WriteDateTime(value);
        }
    }
}