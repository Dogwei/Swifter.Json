
using Swifter.Tools;

using System;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    internal sealed class MultRankArrayRW<TMode, TArray, TElement> : ArrayRW<TArray> where TArray : class
    {
        public static readonly int Rank = typeof(TArray).GetArrayRank();
        public static readonly int MaxDimension = Rank - 1;

        public readonly int[] indices;
        public int dimension;

        public MultRankArrayRW()
        {
            indices = new int[Rank];

            dimension = 0;
        }

        public Array ArrayInstance => Unsafe.As<Array>(content);

        public override TArray Content
        {
            get
            {
                if (typeof(TMode) == typeof(ArrayRWModes.Builder) && content != null)
                {
                    ArrayHelper.Resize<TArray, TElement>(ref content, indices);
                }

                return content;
            }
        }

        public override TArray GetCopy()
        {
            var result = content;

            ArrayHelper.Resize<TArray, TElement>(ref result, indices);

            return result;
        }

        public override int Count
        {
            get
            {
                if (typeof(TMode) == typeof(ArrayRWModes.Builder))
                {
                    return indices[dimension];
                }
                else
                {
                    return ArrayInstance.GetLength(dimension);
                }
            }
        }

        public override IValueRW this[int key] => new ValueRW(this, dimension, key);

        public override void Initialize(TArray content)
        {
            if (typeof(TMode) == typeof(ArrayRWModes.Builder))
            {
                throw new NotSupportedException($"{nameof(ArrayRWModes.Builder)} mode does not support setting up Content.");
            }
            else
            {
                this.content = content;

                indices[dimension] = 0;
            }
        }

        public override void Initialize(int capacity)
        {
            if (typeof(TMode) == typeof(ArrayRWModes.Builder))
            {
                Extend(capacity);

                indices[dimension] = 0;
            }
            else
            {
                throw new NotSupportedException($"{nameof(ArrayRWModes.Fixed)} mode does not support setting up Capacity.");
            }
        }

        public override void OnReadAll(IDataWriter<int> dataWriter)
        {
            var length = Count;

            if (dimension == MaxDimension)
            {
                ref int i = ref indices[dimension];

                for (i = 0; i < length; i++)
                {
                    if (indices.Length == 2)
                    {
                        ValueInterface<TElement>.WriteValue(dataWriter[i], Unsafe.As<TElement[,]>(content)[indices[0], indices[1]]);
                    }
                    else
                    {
                        ValueInterface<TElement>.WriteValue(dataWriter[i], ArrayHelper.Ref<TArray, TElement>(content, indices));
                    }
                }
            }
            else
            {
                ref int i = ref indices[dimension];

                ++dimension;

                for (i = 0; i < length; i++)
                {
                    dataWriter[i].WriteArray(this);
                }

                --dimension;
            }
        }

        public override void OnReadValue(int key, IValueWriter valueWriter)
        {
            indices[dimension] = key;

            if (dimension == MaxDimension)
            {
                ValueInterface<TElement>.WriteValue(valueWriter, ArrayHelper.Ref<TArray, TElement>(content, indices));
            }
            else
            {
                ++dimension;

                valueWriter.WriteArray(this);

                --dimension;
            }
        }

        public override void OnWriteValue(int key, IValueReader valueReader)
        {
            if (typeof(TMode) == typeof(ArrayRWModes.Builder))
            {
                if (indices[dimension] >= ArrayInstance.GetLength(dimension))
                {
                    Extend(indices[dimension] * 2 + 1);
                }
            }
            else
            {
                indices[dimension] = key;
            }

            if (dimension == MaxDimension)
            {
                if (indices.Length == 2)
                {
                    Unsafe.As<TElement[,]>(content)[indices[0], indices[1]] = ValueInterface<TElement>.ReadValue(valueReader);
                }
                else
                {
                    ArrayHelper.Ref<TArray, TElement>(content, indices) = ValueInterface<TElement>.ReadValue(valueReader);
                }
            }
            else
            {
                ++dimension;

                valueReader.ReadArray(this);

                --dimension;
            }


            if (typeof(TMode) == typeof(ArrayRWModes.Builder))
            {
                ++indices[dimension];
            }
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Extend(int capacity)
        {
            if (content != null && capacity <= ArrayInstance.GetLength(dimension))
            {
                return;
            }

            var lengths = new int[Rank];

            for (int i = 0; i < lengths.Length; i++)
            {
                lengths[i] = ArrayInstance?.GetLength(i) ?? DefaultCapacity;
            }

            lengths[dimension] = capacity;

            if (content == null)
            {
                content = ArrayHelper.CreateInstance<TArray, TElement>(lengths);
            }
            else
            {
                ArrayHelper.Resize<TArray, TElement>(ref content, lengths);
            }

        }

        sealed class ValueRW : IValueRW
        {
            readonly MultRankArrayRW<TMode, TArray, TElement> data;
            readonly int dimension;
            readonly int index;
            readonly ValueCopyer copyer;

            public ValueRW(MultRankArrayRW<TMode, TArray, TElement> data, int dimension, int index)
            {
                this.data = data;
                this.dimension = dimension;
                this.index = index;

                copyer = new ValueCopyer();
            }

            void OnReadValue()
            {
                var old = data.dimension;

                data.dimension = dimension;

                data.OnReadValue(index, copyer);

                data.dimension = old;
            }

            void OnWriteValue()
            {
                var old = data.dimension;

                data.dimension = dimension;

                data.OnWriteValue(index, copyer);

                data.dimension = old;
            }

            public object DirectRead()
            {
                OnReadValue();

                return copyer.DirectRead();
            }

            public void DirectWrite(object value)
            {
                copyer.DirectWrite(value);

                OnWriteValue();
            }

            public void ReadArray(IDataWriter<int> valueWriter)
            {
                OnReadValue();

                copyer.ReadArray(valueWriter);
            }

            public bool ReadBoolean()
            {
                OnReadValue();

                return copyer.ReadBoolean();
            }

            public byte ReadByte()
            {
                OnReadValue();

                return copyer.ReadByte();
            }

            public char ReadChar()
            {
                OnReadValue();

                return copyer.ReadChar();
            }

            public DateTime ReadDateTime()
            {
                OnReadValue();

                return copyer.ReadDateTime();
            }

            public decimal ReadDecimal()
            {
                OnReadValue();

                return copyer.ReadDecimal();
            }

            public double ReadDouble()
            {
                OnReadValue();

                return copyer.ReadDouble();
            }

            public short ReadInt16()
            {
                OnReadValue();

                return copyer.ReadInt16();
            }

            public int ReadInt32()
            {
                OnReadValue();

                return copyer.ReadInt32();
            }

            public long ReadInt64()
            {
                OnReadValue();

                return copyer.ReadInt64();
            }

            public T? ReadNullable<T>() where T : struct
            {
                OnReadValue();

                return copyer.ReadNullable<T>();
            }

            public void ReadObject(IDataWriter<string> valueWriter)
            {
                OnReadValue();

                copyer.ReadObject(valueWriter);
            }

            public sbyte ReadSByte()
            {
                OnReadValue();

                return copyer.ReadSByte();
            }

            public float ReadSingle()
            {
                OnReadValue();

                return copyer.ReadSingle();
            }

            public string ReadString()
            {
                OnReadValue();

                return copyer.ReadString();
            }

            public ushort ReadUInt16()
            {
                OnReadValue();

                return copyer.ReadUInt16();
            }

            public uint ReadUInt32()
            {
                OnReadValue();

                return copyer.ReadUInt32();
            }

            public ulong ReadUInt64()
            {
                OnReadValue();

                return copyer.ReadUInt64();
            }

            public void WriteArray(IDataReader<int> dataReader)
            {
                copyer.WriteArray(dataReader);

                OnWriteValue();
            }

            public void WriteBoolean(bool value)
            {
                copyer.WriteBoolean(value);

                OnWriteValue();
            }

            public void WriteByte(byte value)
            {
                copyer.WriteByte(value);

                OnWriteValue();
            }

            public void WriteChar(char value)
            {
                copyer.WriteChar(value);

                OnWriteValue();
            }

            public void WriteDateTime(DateTime value)
            {
                copyer.WriteDateTime(value);

                OnWriteValue();
            }

            public void WriteDecimal(decimal value)
            {
                copyer.WriteDecimal(value);

                OnWriteValue();
            }

            public void WriteDouble(double value)
            {
                copyer.WriteDouble(value);

                OnWriteValue();
            }

            public void WriteInt16(short value)
            {
                copyer.WriteInt16(value);

                OnWriteValue();
            }

            public void WriteInt32(int value)
            {
                copyer.WriteInt32(value);

                OnWriteValue();
            }

            public void WriteInt64(long value)
            {
                copyer.WriteInt64(value);

                OnWriteValue();
            }

            public void WriteObject(IDataReader<string> dataReader)
            {
                copyer.WriteObject(dataReader);

                OnWriteValue();
            }

            public void WriteSByte(sbyte value)
            {
                copyer.WriteSByte(value);

                OnWriteValue();
            }

            public void WriteSingle(float value)
            {
                copyer.WriteSingle(value);

                OnWriteValue();
            }

            public void WriteString(string value)
            {
                copyer.WriteString(value);

                OnWriteValue();
            }

            public void WriteUInt16(ushort value)
            {
                copyer.WriteUInt16(value);

                OnWriteValue();
            }

            public void WriteUInt32(uint value)
            {
                copyer.WriteUInt32(value);

                OnWriteValue();
            }

            public void WriteUInt64(ulong value)
            {
                copyer.WriteUInt64(value);

                OnWriteValue();
            }
        }
    }

    internal sealed class MultRankArrayRWCreater<TArray, TElement> : IArrayRWCreater<TArray> where TArray : class
    {
        public ArrayRW<TArray> CreateArrayBuilder()
        {
            return new MultRankArrayRW<ArrayRWModes.Builder, TArray, TElement>();
        }

        public ArrayRW<TArray> Create()
        {
            return new MultRankArrayRW<ArrayRWModes.Fixed, TArray, TElement>();
        }
    }
}