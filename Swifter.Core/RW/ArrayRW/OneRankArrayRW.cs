using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    internal sealed class OneRankArrayRW<TMode, TElement> : ArrayRW<TElement[]> where TMode : struct
    {
        public int index;

        public override TElement[] Content
        {
            get
            {
                if (typeof(TMode) == typeof(ArrayRWModes.Builder) && content != null && content.Length != index)
                {
                    Array.Resize(ref content, index);
                }

                return content;
            }
        }


        public override TElement[] GetCopy()
        {
            var result = content;

            Array.Resize(ref result, index);

            return result;
        }

        public override int Count
        {
            get
            {
                if (typeof(TMode) == typeof(ArrayRWModes.Builder))
                {
                    return index;
                }
                else
                {
                    return content.Length;
                }
            }
        }

        public override IValueRW this[int key] => new ValueCopyer<int>(this, key);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Extend(int size)
        {
            if (content == null)
            {
                content = new TElement[size];
            }
            else if (size > content.Length)
            {
                Array.Resize(ref content, size);
            }
        }

        public override void Initialize(TElement[] content)
        {
            if (typeof(TMode) == typeof(ArrayRWModes.Builder))
            {
                throw new NotSupportedException($"{nameof(ArrayRWModes.Builder)} mode does not support setting up Content.");
            }
            else
            {
                this.content = content;

                index = 0;
            }
        }

        public override void Initialize(int capacity)
        {
            if (typeof(TMode) == typeof(ArrayRWModes.Builder))
            {
                Extend(capacity);

                index = 0;
            }
            else
            {
                throw new NotSupportedException($"{nameof(ArrayRWModes.Fixed)} mode does not support setting up Capacity.");
            }
        }

        public override void OnReadValue(int key, IValueWriter valueWriter)
        {
            ValueInterface<TElement>.WriteValue(valueWriter, content[key]);
        }

        public override void OnWriteValue(int key, IValueReader valueReader)
        {
            if (typeof(TMode) == typeof(ArrayRWModes.Builder))
            {
                if (index >= content.Length)
                {
                    Extend(index * 2 + 1);
                }

                if (ValueInterface<TElement>.IsNotModified)
                {
                    if (typeof(TElement) == typeof(string))
                    {
                        Unsafe.As<string[]>(content)[index] = valueReader.ReadString();
                    }
                    else if (typeof(TElement) == typeof(int))
                    {
                        Unsafe.As<int[]>(content)[index] = valueReader.ReadInt32();
                    }
                    else if (typeof(TElement) == typeof(bool))
                    {
                        Unsafe.As<bool[]>(content)[index] = valueReader.ReadBoolean();
                    }
                    else if (typeof(TElement) == typeof(double))
                    {
                        Unsafe.As<double[]>(content)[index] = valueReader.ReadDouble();
                    }
                    else if (typeof(TElement) == typeof(long))
                    {
                        Unsafe.As<long[]>(content)[index] = valueReader.ReadInt64();
                    }
                    else if (typeof(TElement) == typeof(DateTime))
                    {
                        Unsafe.As<DateTime[]>(content)[index] = valueReader.ReadDateTime();
                    }
                    else if (typeof(TElement) == typeof(decimal))
                    {
                        Unsafe.As<decimal[]>(content)[index] = valueReader.ReadDecimal();
                    }
                    else
                    {
                        content[index] = ValueInterface<TElement>.ReadValue(valueReader);
                    }
                }
                else
                {
                    content[index] = ValueInterface<TElement>.ReadValue(valueReader);
                }
                
                ++index;
            }
            else
            {
                content[key] = ValueInterface<TElement>.ReadValue(valueReader);
            }
        }

        public override void OnReadAll(IDataWriter<int> dataWriter)
        {
            var length = Count;

            // 对常用类型进行优化。
            if (ValueInterface<TElement>.IsNotModified)
            {
                if (typeof(TElement) == typeof(string))
                {
                    for (int i = 0; i < length; i++)
                    {
                        dataWriter[i].WriteString(Unsafe.As<string[]>(content)[i]);
                    }

                    return;
                }

                if (typeof(TElement) == typeof(int))
                {
                    for (int i = 0; i < length; i++)
                    {
                        dataWriter[i].WriteInt32(Unsafe.As<int[]>(content)[i]);
                    }

                    return;
                }

                if (typeof(TElement) == typeof(bool))
                {
                    for (int i = 0; i < length; i++)
                    {
                        dataWriter[i].WriteBoolean(Unsafe.As<bool[]>(content)[i]);
                    }

                    return;
                }

                if (typeof(TElement) == typeof(double))
                {
                    for (int i = 0; i < length; i++)
                    {
                        dataWriter[i].WriteDouble(Unsafe.As<double[]>(content)[i]);
                    }

                    return;
                }

                if (typeof(TElement) == typeof(long))
                {
                    for (int i = 0; i < length; i++)
                    {
                        dataWriter[i].WriteInt64(Unsafe.As<long[]>(content)[i]);
                    }

                    return;
                }

                if (typeof(TElement) == typeof(DateTime))
                {
                    for (int i = 0; i < length; i++)
                    {
                        dataWriter[i].WriteDateTime(Unsafe.As<DateTime[]>(content)[i]);
                    }

                    return;
                }

                if (typeof(TElement) == typeof(decimal))
                {
                    for (int i = 0; i < length; i++)
                    {
                        dataWriter[i].WriteDecimal(Unsafe.As<decimal[]>(content)[i]);
                    }

                    return;
                }
            }

            for (int i = 0; i < length; i++)
            {
                ValueInterface<TElement>.WriteValue(dataWriter[i], content[i]);
            }
        }
    }

    internal sealed class OneRankArrayRWCreater<T> : IArrayRWCreater<T[]>
    {
        public ArrayRW<T[]> CreateArrayBuilder()
        {
            return new OneRankArrayRW<ArrayRWModes.Builder, T>();
        }

        public ArrayRW<T[]> Create()
        {
            return new OneRankArrayRW<ArrayRWModes.Fixed, T>();
        }
    }
}