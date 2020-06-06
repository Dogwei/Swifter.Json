using Swifter.Tools;
using System;
using System.Linq;
using System.Reflection;

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

            if (value is string typeName && Type.GetType(typeName) is T result)
            {
                return result;
            }

            return XConvert<T>.FromObject(value);
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (valueWriter is IValueWriter<T> tWriter)
            {
                tWriter.WriteValue(value);
            }
            else if (valueWriter is IValueWriter<Type> typeWriter)
            {
                typeWriter.WriteValue(value);
            }
            else
            {
                if (value.Assembly == typeof(object).Assembly)
                {
                    valueWriter.WriteString(value.FullName);
                }
                else
                {
                    valueWriter.WriteString(value.AssemblyQualifiedName);
                }
            }
        }
    }
}