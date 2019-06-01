using Swifter.Readers;
using Swifter.Writers;
using System;

namespace Swifter.RW
{
    internal sealed class VersionInterface : IValueInterface<Version>
    {
        public Version ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<Version> versionReader)
            {
                return versionReader.ReadValue();
            }

            var versionText = valueReader.ReadString();

            if (versionText == null)
            {
                return null;
            }

            return new Version(versionText);
        }

        public void WriteValue(IValueWriter valueWriter, Version value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            if (valueWriter is IValueWriter<Version> versionReader)
            {
                versionReader.WriteValue(value);
            }
            else
            {
                valueWriter.WriteString(value.ToString());
            }
        }
    }
}