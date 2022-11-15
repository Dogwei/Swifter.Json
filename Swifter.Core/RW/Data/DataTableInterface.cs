using System.Data;
using static Swifter.RW.DataTableRW;

namespace Swifter.RW
{
    internal sealed class DataTableInterface<T> : IValueInterface<T?> where T : DataTable
    {
        public T? ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> reader)
            {
                return reader.ReadValue();
            }

            var writer = new DataTableRW<T>(
                valueReader is ITargetableValueRW targetable && TargetableSetOptionsHelper<DataTableRWOptions>.TryGetOptions(targetable, out var options)
                ? options
                : DataTableRW.DefaultOptions
                );

            valueReader.ReadArray(writer);

            return writer.content;
        }

        public void WriteValue(IValueWriter valueWriter, T? value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (valueWriter is IValueWriter<T> weiter)
            {
                weiter.WriteValue(value);
            }
            else
            {
                valueWriter.WriteArray(new DataTableRW<T>(
                    valueWriter is ITargetableValueRW targetable && TargetableSetOptionsHelper<DataTableRWOptions>.TryGetOptions(targetable, out var options)
                    ? options
                    : DataTableRW.DefaultOptions
                    )
                {
                    content = value
                });
            }
        }
    }
}