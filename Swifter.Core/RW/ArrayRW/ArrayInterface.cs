
using Swifter.Tools;


namespace Swifter.RW
{
    internal sealed class ArrayInterface<T> : BaseObjectPool<ArrayRW<T>>, IValueInterface<T> where T : class
    {
        public T ReadValue(IValueReader valueReader)
        {
            var writer = Rent();

            valueReader.ReadArray(writer);

            if (valueReader is IUsePool)
            {
                var content = writer.GetCopy();

                Return(writer);

                return content;
            }

            return writer.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);
            }
            else
            {
                valueWriter.WriteArray(ArrayRW<T>.Create(value));
            }
        }

        protected override ArrayRW<T> CreateInstance()
        {
            return ArrayRW<T>.CreateAppend();
        }
    }
}