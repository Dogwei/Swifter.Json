using Swifter.RW;

namespace Swifter.FSharpExtensions
{
    public sealed class FSharpUnionInterface<T> : IValueInterface<T>
    {
        public T ReadValue(IValueReader valueReader)
        {
            var rw = new FSharpUnionRW<T>();

            valueReader.ReadArray(rw);

            return rw.GetContent();
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            var rw = new FSharpUnionRW<T> { Content = value };

            var (caceInfo, values) = rw.GetUnionFields();

            if (values != null && values.Length > 0)
            {
                valueWriter.WriteArray(rw);
            }
            else
            {
                valueWriter.WriteString(caceInfo.Name);
            }
        }
    }
}