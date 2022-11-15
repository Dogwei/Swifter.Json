using Swifter.RW;
using Swifter.Tools;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    sealed class SerializationArrayReader<TElement> : IArrayReader
    {
        readonly Array array;
        readonly int count;

        public SerializationArrayReader(Array array)
        {
            this.array = array;

            count = array.Length;

            if (count > 0)
            {
                ref var elements = ref ArrayHelper.AddrOfArrayElement<TElement>(array, 0);

                do
                {
                    --count;

                    if (!TypeHelper.IsEmptyValue(Unsafe.Add(ref elements, count)))
                    {
                        ++count;

                        break;
                    }

                } while (count > 0);
            }
        }

        public IValueReader this[int key] => throw new NotImplementedException();

        public int Count => count;

        public Type? ContentType => null;

        public Type? ValueType => null;

        public object? Content
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public void OnReadAll(IDataWriter<int> dataWriter, RWStopToken stopToken = default)
        {
            if (count > 0)
            {
                ref var elements = ref ArrayHelper.AddrOfArrayElement<TElement>(array, 0);

                int i = 0;

                do
                {
                    SerializationHelper.WriteValue(dataWriter[i], Unsafe.Add(ref elements, i));

                    ++i;

                } while (i < count);
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            if (key >= 0 && key < count)
            {
                SerializationHelper.WriteValue(valueWriter, ArrayHelper.AddrOfArrayElement<TElement>(array, key));
            }
            else
            {
                throw new IndexOutOfRangeException();
            }
        }
    }
}