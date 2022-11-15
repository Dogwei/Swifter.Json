

using System;

namespace Swifter.RW
{
    internal sealed class UriInterface : IValueInterface<Uri>
    {
        public Uri? ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<Uri> reader)
            {
                return reader.ReadValue();
            }

            var uriTest = valueReader.ReadString();

            if (uriTest is null)
            {
                return null;
            }

            return new Uri(uriTest);
        }

        public void WriteValue(IValueWriter valueWriter, Uri? value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (valueWriter is IValueWriter<Uri> writer)
            {
                writer.WriteValue(value);
            }
            else
            {
                valueWriter.WriteString(value.ToString());
            }
        }
    }
}