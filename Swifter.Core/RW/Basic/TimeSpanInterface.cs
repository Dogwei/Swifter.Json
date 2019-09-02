
using Swifter.Tools;

using System;

namespace Swifter.RW
{
    internal sealed class TimeSpanInterface : IValueInterface<TimeSpan>
    {
        public TimeSpan ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<TimeSpan> timeSpanReader)
            {
                return timeSpanReader.ReadValue();
            }

            object directValue = valueReader.DirectRead();

            if (directValue is TimeSpan)
            {
                return (TimeSpan)directValue;
            }

            if (directValue is string)
            {
                return TimeSpan.Parse((string)directValue);
            }

            return XConvert.FromObject<TimeSpan>(directValue);
        }

        public void WriteValue(IValueWriter valueWriter, TimeSpan value)
        {
            if (valueWriter is IValueWriter<TimeSpan> timeSpanWriter)
            {
                timeSpanWriter.WriteValue(value);

                return;
            }

            valueWriter.DirectWrite(value);
        }
    }
}