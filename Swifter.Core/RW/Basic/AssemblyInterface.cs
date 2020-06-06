using Swifter.Tools;
using System.Reflection;

namespace Swifter.RW
{
    internal sealed class AssemblyInterface<T> : IValueInterface<T> where T : Assembly
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }

            if (valueReader is IValueReader<Assembly> assemblyReader)
            {
                return (T)assemblyReader.ReadValue();
            }

            var value = valueReader.DirectRead();

            if (value is string sssemblyString && Assembly.Load(sssemblyString) is T result)
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
            else if (valueWriter is IValueWriter<Assembly> assemblyWriter)
            {
                assemblyWriter.WriteValue(value);
            }
            else
            {
                valueWriter.WriteString(value.FullName);
            }
        }
    }
}