using System;
using System.Globalization;

namespace Swifter.RW
{
    internal sealed class SetValueFormatInterface<T> : IValueInterface<T> where T : IFormattable
    {
        public readonly IValueInterface<T> DefaultInterface;
        public readonly string Format;

        public SetValueFormatInterface(IValueInterface<T> defaultInterface, string format)
        {
            DefaultInterface = defaultInterface;
            Format = format;
        }

        public T ReadValue(IValueReader valueReader)
        {
            return DefaultInterface.ReadValue(valueReader);
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            valueWriter.WriteString(value.ToString(Format, CultureInfo.CurrentCulture));
        }
    }
}