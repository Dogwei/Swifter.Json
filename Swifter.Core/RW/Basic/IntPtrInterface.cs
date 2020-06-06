

using System;

namespace Swifter.RW
{
    internal sealed class IntPtrInterface : IValueInterface<IntPtr>
    {
        public IntPtr ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<IntPtr> intPtrReader)
            {
                return intPtrReader.ReadValue();
            }

            var value = valueReader.ReadNullable<long>();

            if (value is null)
            {
                return IntPtr.Zero;
            }

            return (IntPtr)value.Value;
        }

        public void WriteValue(IValueWriter valueWriter, IntPtr value)
        {
            if (valueWriter is IValueWriter<IntPtr> intPtrWriter)
            {
                intPtrWriter.WriteValue(value);
            }
            else
            {
                valueWriter.WriteInt64((long)value);
            }
        }
    }
}