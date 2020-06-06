

using System;

namespace Swifter.RW
{
    internal sealed class UriInterface : IValueInterface<Uri>
    {
        public Uri ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<Uri> reader)
            {
                return reader.ReadValue();
            }

            return new Uri(valueReader.ReadString());
        }

        public void WriteValue(IValueWriter valueWriter, Uri value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            if (valueWriter is IValueWriter<Uri> writer)
            {
                writer.WriteValue(value);

                return;
            }

            valueWriter.WriteString(value.ToString());
        }
    }
}
