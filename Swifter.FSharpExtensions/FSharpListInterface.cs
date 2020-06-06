using Microsoft.FSharp.Collections;
using Swifter.RW;

namespace Swifter.FSharpExtensions
{
    public sealed class FSharpListInterface<T> : IValueInterface<FSharpList<T>>
    {
        public FSharpList<T> ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<FSharpList<T>> reader)
            {
                return reader.ReadValue();
            }
            else
            {
                var rw = new FSharpListRW<T>();

                valueReader.ReadArray(rw);

                return rw.content;
            }
        }

        public void WriteValue(IValueWriter valueWriter, FSharpList<T> value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (valueWriter is IValueWriter<FSharpList<T>> writer)
            {
                writer.WriteValue(value);
            }
            else
            {
                valueWriter.WriteArray(new FSharpListRW<T> { content = value });
            }
        }
    }
}