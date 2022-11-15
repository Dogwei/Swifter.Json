using Swifter.RW;
using Swifter.Tools;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    sealed class SerializationArrayWriter<TElement> : IArrayWriter
    {
        readonly Array array;

        public SerializationArrayWriter(Array array)
        {
            this.array = array;
        }

        public IValueWriter this[int key] => throw new NotImplementedException();

        public int Count => array.Length;

        public Type? ContentType => null;

        public Type? ValueType => null;

        public object? Content
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public void Initialize()
        {
        }

        public void Initialize(int capacity)
        {
        }

        public void OnWriteAll(IDataReader<int> dataReader, RWStopToken stopToken = default)
        {
            ref var elements = ref ArrayHelper.AddrOfArrayElement<TElement>(array, 0);

            for (int i = 0; i < array.Length; i++)
            {
                Unsafe.Add(ref elements, i) = SerializationHelper.ReadValue<TElement>(dataReader[i]);
            }
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (key >= 0 && key < array.Length)
            {
                ArrayHelper.AddrOfArrayElement<TElement>(array, key) = SerializationHelper.ReadValue<TElement>(valueReader);
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }
    }
}