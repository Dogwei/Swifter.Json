
using Swifter.Tools;

using System;

namespace Swifter.RW
{
    internal sealed class DateTimeOffsetInterface : IValueInterface<DateTimeOffset>
    {
        public DateTimeOffset ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<DateTimeOffset> dateTimeOffsetReader)
            {
                return dateTimeOffsetReader.ReadValue();
            }

            object directValue = valueReader.DirectRead();

            if (directValue is DateTimeOffset)
            {
                return (DateTimeOffset)directValue;
            }

            if (directValue is string)
            {
                return DateTimeOffset.Parse((string)directValue);
            }

            return XConvert.FromObject<DateTimeOffset>(directValue);
        }

        public void WriteValue(IValueWriter valueWriter, DateTimeOffset value)
        {
            if (valueWriter is IValueWriter<DateTimeOffset> dateTimeOffsetWriter)
            {
                dateTimeOffsetWriter.WriteValue(value);

                return;
            }

            valueWriter.DirectWrite(value);
        }
    }
}