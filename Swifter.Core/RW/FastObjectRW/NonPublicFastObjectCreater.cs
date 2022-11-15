using Swifter.Tools;

namespace Swifter.RW
{
    sealed class NonPublicFastObjectCreater<T> : IFastObjectRWCreater<T>, IValueInterface<T>
    {
        public FastObjectRW<T> Create()
        {
            return new NonPublicFastObjectRW<T>();
        }

        public T? ReadValue(IValueReader valueReader)
        {
            var writer = new NonPublicFastObjectRW<T>();

            valueReader.ReadObject(writer);

            return writer.content;
        }

        public void WriteValue(IValueWriter valueWriter, T? value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (!ValueInterface<T>.IsFinalType && value.GetType() != typeof(T))
            {
                /* 父类引用，子类实例时使用 Type 获取写入器。 */
                ValueInterface.GetInterface(value).Write(valueWriter, value);
            }
            else
            {
                valueWriter.WriteObject(new NonPublicFastObjectRW<T>
                {
                    content = value
                });
            }
        }
    }
}