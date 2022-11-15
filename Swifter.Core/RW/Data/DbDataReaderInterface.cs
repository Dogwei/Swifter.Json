using System;
using System.Diagnostics.CodeAnalysis;
using static Swifter.RW.DataTableRW;

namespace Swifter.RW
{
    sealed class DbDataReaderInterface<T> : IValueInterface<T> where T : class, System.Data.IDataReader
    {
        [return: MaybeNull]
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> reader)
            {
                return reader.ReadValue();
            }

            throw new NotSupportedException();
        }

        public void WriteValue(IValueWriter valueWriter, [AllowNull] T value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (valueWriter is IValueWriter<T> writer)
            {
                writer.WriteValue(value);
            }
            else
            {
                valueWriter.WriteArray(new DbDataReaderReader(
                    value,
                    valueWriter is ITargetableValueRW targetable && TargetableSetOptionsHelper<DataTableRWOptions>.TryGetOptions(targetable, out var options)
                    ? options
                    : DataTableRW.DefaultOptions
                    ));
            }
        }
    }
}