
using Swifter.Tools;

using System;

namespace Swifter.RW
{
    internal sealed class TimeSpanInterface : IValueInterface<TimeSpan>
    {
        public TimeSpan ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadTimeSpan();
        }

        public void WriteValue(IValueWriter valueWriter, TimeSpan value)
        {
            valueWriter.WriteTimeSpan(value);
        }
    }
}