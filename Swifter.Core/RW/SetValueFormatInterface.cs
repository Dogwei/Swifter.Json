using System;
using System.Globalization;

namespace Swifter.RW
{
    internal sealed class SetValueFormatInterface<T> : IValueInterface<T> where T : IFormattable
    {
        public readonly IValueInterface<T> DefaultInterface;
        public readonly string Format;
        public readonly IFormatProvider? FormatProvider;

        public SetValueFormatInterface(IValueInterface<T> defaultInterface, string format, IFormatProvider? formatProvider)
        {
            DefaultInterface = defaultInterface;
            Format = format;
            FormatProvider = formatProvider;
        }

        public T? ReadValue(IValueReader valueReader)
        {
            return DefaultInterface.ReadValue(valueReader);
        }

        public void WriteValue(IValueWriter valueWriter, T? value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else
            {
                valueWriter.WriteString(value.ToString(Format, FormatProvider ?? CultureInfo.CurrentCulture));
            }
        }
    }
}