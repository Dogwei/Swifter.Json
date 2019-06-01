using Swifter.Readers;
using Swifter.Writers;
using System;

namespace Swifter.RW
{
    internal sealed class TypeInfoInterface<T> : IValueInterface<T> where T : Type
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }

            if (valueReader is IValueReader<Type> typeReader)
            {
                return (T)typeReader.ReadValue();
            }

            var value = valueReader.DirectRead();

            if (value == null)
            {
                return null;
            }

            if (value is T tValue)
            {
                return tValue;
            }

            if (value is string sValue)
            {
                return (T)Type.GetType(sValue);
            }

            throw new NotSupportedException($"Cannot Read a 'TypeInfo' by '{value}'.");
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            if (valueWriter is IValueWriter<T> tWriter)
            {
                tWriter.WriteValue(value);

                return;
            }

            if (valueWriter is IValueWriter<Type> typeWriter)
            {
                typeWriter.WriteValue(value);

                return;
            }

            valueWriter.WriteString(value.AssemblyQualifiedName);
        }
    }
}