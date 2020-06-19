using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    internal static class MultiDimArray<TArray, TElement> where TArray : class
    {
        public const int DefaultCapacity = 3;

        public static readonly int Rank = typeof(TArray).GetArrayRank();

        public static ArrayAppendingInfo[] appendingInfos = new ArrayAppendingInfo[Rank];

        static MultiDimArray()
        {
            for (int i = 0; i < Rank; i++)
            {
                appendingInfos[i].MostClosestMeanCommonlyUsedLength = DefaultCapacity;
            }
        }

        internal abstract class BaseRW : IDataRW<int>
        {
            public IValueRW this[int key] => new ValueCopyer<int>(this, key);

            IValueReader IDataReader<int>.this[int key] => this[key];

            IValueWriter IDataWriter<int>.this[int key] => this[key];

            public IEnumerable<int> Keys => Enumerable.Range(0, Count);

            public abstract int Count { get; }

            public abstract Type ContentType { get; }

            public abstract object Content { get; set; }

            public abstract void Initialize();

            public abstract void Initialize(int capacity);

            public abstract void OnReadAll(IDataWriter<int> dataWriter);

            public abstract void OnReadValue(int key, IValueWriter valueWriter);

            public abstract void OnWriteAll(IDataReader<int> dataReader);

            public abstract void OnWriteValue(int key, IValueReader valueReader);
        }

        internal sealed class FirstRW : BaseRW
        {
            public readonly int[] counts;
            public readonly int[] lengths;

            public TArray content;
            public bool needGrow;

            public FirstRW()
            {
                counts = new int[Rank];
                lengths = new int[Rank];
            }

            public ref Array Array
            {
                [MethodImpl(VersionDifferences.AggressiveInlining)]
                get => ref Underlying.As<TArray, Array>(ref content);
            }

            public override int Count => counts[0];

            public override Type ContentType => typeof(TArray);

            public override object Content
            {
                get => GetContent();
                set => Initialize((TArray)value);
            }

            public override void Initialize()
            {
                Initialize(appendingInfos[0].MostClosestMeanCommonlyUsedLength);
            }

            public override void Initialize(int capacity)
            {
                content = null;

                counts.Initialize();
                lengths.Initialize();

                lengths[0] = capacity;

                needGrow = true;
            }

            public void Initialize(TArray array)
            {
                content = array;

                for (int i = 0; i < Rank; i++)
                {
                    lengths[i] = counts[i] = Array.GetLength(i);
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public void Grow()
            {
                Array = ArrayHelper.Resize<TElement>(Array, lengths);

                needGrow = false;
            }

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public void CheckGrow()
            {
                if (needGrow)
                {
                    Grow();
                }
            }

            public override void OnReadAll(IDataWriter<int> dataWriter)
            {
                var length = Count;

                if (1 == Rank)
                {
                    CheckGrow();

                    ref var elements = ref ArrayHelper.AddrOfArrayElement<TElement>(Array, 0);

                    for (int i = 0; i < length; i++)
                    {
                        ValueInterface.WriteValue(dataWriter[i], Underlying.Add(ref elements, i));
                    }
                }
                else if (2 == Rank)
                {
                    for (int i = 0; i < length; i++)
                    {
                        dataWriter[i].WriteArray(new LastRW(this, i));
                    }
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        dataWriter[i].WriteArray(new MiddleRW(this, i));
                    }
                }
            }

            public override void OnReadValue(int key, IValueWriter valueWriter)
            {
                if (key >= 0 && key < Count)
                {
                    if (1 == Rank)
                    {
                        CheckGrow();

                        ValueInterface.WriteValue(valueWriter, ArrayHelper.AddrOfArrayElement<TElement>(Array, key));
                    }
                    else if (2 == Rank)
                    {
                        valueWriter.WriteArray(new LastRW(this, key));
                    }
                    else
                    {
                        valueWriter.WriteArray(new MiddleRW(this, key));
                    }
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }

            public override void OnWriteAll(IDataReader<int> dataReader)
            {
                var length = Count;

                if (1 == Rank)
                {
                    CheckGrow();

                    ref var elements = ref ArrayHelper.AddrOfArrayElement<TElement>(Array, 0);

                    for (int i = 0; i < length; i++)
                    {
                        Underlying.Add(ref elements, i) = ValueInterface.ReadValue<TElement>(dataReader[i]);
                    }
                }
                else if (2 == Rank)
                {
                    for (int i = 0; i < length; i++)
                    {
                        dataReader[i].ReadArray(new LastRW(this, i));
                    }
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        dataReader[i].ReadArray(new MiddleRW(this, i));
                    }
                }
            }

            public override void OnWriteValue(int key, IValueReader valueReader)
            {
                ref var count = ref counts[0];
                ref var length = ref lengths[0];

                if (key >= 0 && key <= count)
                {
                    if (key == count && key == length)
                    {
                        length += length + 1;

                        needGrow = true;
                    }

                    if (1 == Rank)
                    {
                        CheckGrow();

                        ArrayHelper.AddrOfArrayElement<TElement>(Array, key) = ValueInterface.ReadValue<TElement>(valueReader);
                    }
                    else if (2 == Rank)
                    {
                        valueReader.ReadArray(new LastRW(this, key));
                    }
                    else
                    {
                        valueReader.ReadArray(new MiddleRW(this, key));
                    }

                    if (key == count)
                    {
                        ++count;
                    }
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public bool IsNeedResize()
            {
                if (content is null && needGrow)
                {
                    return true;
                }

                for (int i = 0; i < Rank; i++)
                {
                    if (counts[i] != lengths[i])
                    {
                        return true;
                    }
                }

                return false;
            }

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public TArray GetContent()
            {
                if (IsNeedResize())
                {
                    Array = ArrayHelper.Resize<TElement>(Array, counts);

                    needGrow = true;
                }

                for (int i = 0; i < Rank; i++)
                {
                    appendingInfos[i].AddUsedLength(counts[i]);
                }

                return content;
            }
        }

        internal sealed class MiddleRW : BaseRW
        {
            public readonly FirstRW first;
            public readonly MiddleRW parent;
            public readonly int dimension;
            public readonly int index;

            public MiddleRW(FirstRW parent, int index)
            {
                first = parent;

                dimension = 1;

                this.index = index;
            }

            public MiddleRW(MiddleRW parent, int index)
            {
                first = parent.first;
                this.parent = parent;

                dimension = parent.dimension + 1;
                this.index = index;
            }

            public override int Count => first.counts[dimension];

            public override Type ContentType => null;

            public override object Content
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Initialize()
            {
                Initialize(appendingInfos[dimension].MostClosestMeanCommonlyUsedLength);
            }

            public override void Initialize(int capacity)
            {
                ref var length = ref first.lengths[dimension];

                if (length < capacity)
                {
                    length = capacity;

                    first.needGrow = true;
                }
            }

            public override void OnReadAll(IDataWriter<int> dataWriter)
            {
                var length = Count;

                if (dimension + 2 == Rank)
                {
                    for (int i = 0; i < length; i++)
                    {
                        dataWriter[i].WriteArray(new LastRW(this, i));
                    }
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        dataWriter[i].WriteArray(new MiddleRW(this, i));
                    }
                }
            }

            public override void OnReadValue(int key, IValueWriter valueWriter)
            {
                if (key >= 0 && key < Count)
                {
                    if (dimension + 2 == Rank)
                    {
                        valueWriter.WriteArray(new LastRW(this, key));
                    }
                    else
                    {
                        valueWriter.WriteArray(new MiddleRW(this, key));
                    }
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }

            public override void OnWriteAll(IDataReader<int> dataReader)
            {
                var length = Count;

                if (dimension + 2 == Rank)
                {
                    for (int i = 0; i < length; i++)
                    {
                        dataReader[i].ReadArray(new LastRW(this, i));
                    }
                }
                else
                {
                    for (int i = 0; i < length; i++)
                    {
                        dataReader[i].ReadArray(new MiddleRW(this, i));
                    }
                }
            }

            public override void OnWriteValue(int key, IValueReader valueReader)
            {
                ref var count = ref first.counts[dimension];
                ref var length = ref first.lengths[dimension];

                if (key >= 0 && key <= count)
                {
                    if (key == count && key == length)
                    {
                        length += length + 1;

                        first.needGrow = true;
                    }

                    if (dimension + 2 == Rank)
                    {
                        valueReader.ReadArray(new LastRW(this, key));
                    }
                    else
                    {
                        valueReader.ReadArray(new MiddleRW(this, key));
                    }

                    if (key == count)
                    {
                        ++count;
                    }
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        internal sealed class LastRW : BaseRW, IArrayCollectionRW
        {
            public readonly FirstRW first;
            public readonly MiddleRW parent;
            public readonly int dimension;
            public readonly int index;
            public int offset;

            public LastRW(FirstRW parent, int index)
            {
                first = parent;

                dimension = 1;

                this.index = index;

                ComputeOffset();
            }

            public LastRW(MiddleRW parent, int index)
            {
                first = parent.first;
                this.parent = parent;

                dimension = parent.dimension + 1;
                this.index = index;

                ComputeOffset();
            }

            public void ComputeOffset()
            {
                if (first.content !=null)
                {
                    offset = 0;

                    var index = this.index;

                    var dim = dimension;
                    var len = first.Array.GetLength(dim);

                    if (dim >= 2)
                    {
                        var parent = this.parent;

                        do
                        {
                            offset += index * len;

                            --dim;

                            len *= first.Array.GetLength(dim);

                            index = parent.index;

                            parent = parent.parent;
                        } while (dim >= 2);
                    }

                    offset += index * len;
                }
            }

            public override int Count => first.counts[dimension];

            public override Type ContentType => null;

            public override object Content
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Initialize()
            {
                Initialize(appendingInfos[dimension].MostClosestMeanCommonlyUsedLength);
            }

            public override void Initialize(int capacity)
            {
                ref var length = ref first.lengths[dimension];

                if (length < capacity)
                {
                    length = capacity;

                    first.needGrow = true;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public void Grow()
            {
                first.Array = ArrayHelper.Resize<TElement>(first.Array, first.lengths);

                first.needGrow = false;

                ComputeOffset();
            }

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public void CheckGrow()
            {
                if (first.needGrow)
                {
                    Grow();
                }
            }

            public override void OnReadAll(IDataWriter<int> dataWriter)
            {
                CheckGrow();

                var length = Count;

                ref var elements = ref ArrayHelper.AddrOfArrayElement<TElement>(first.Array, offset);

                for (int i = 0; i < length; i++)
                {
                    ValueInterface.WriteValue(dataWriter[i], Underlying.Add(ref elements, i));
                }
            }

            public override void OnReadValue(int key, IValueWriter valueWriter)
            {
                CheckGrow();

                if (key >= 0 && key < Count)
                {
                    ValueInterface.WriteValue(valueWriter, ArrayHelper.AddrOfArrayElement<TElement>(first.Array, offset + key));
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }

            public override void OnWriteAll(IDataReader<int> dataReader)
            {
                CheckGrow();

                var length = Count;

                ref var elements = ref ArrayHelper.AddrOfArrayElement<TElement>(first.Array, offset);

                for (int i = 0; i < length; i++)
                {
                    Underlying.Add(ref elements, i) = ValueInterface.ReadValue<TElement>(dataReader[i]);
                }
            }

            public override void OnWriteValue(int key, IValueReader valueReader)
            {
                ref var count = ref first.counts[dimension];
                ref var length = ref first.lengths[dimension];

                if (key >= 0 && key <= count)
                {
                    if (key == count && key == length)
                    {
                        length += length + 1;

                        first.needGrow = true;
                    }

                    CheckGrow();

                    ArrayHelper.AddrOfArrayElement<TElement>(first.Array, offset + key) = ValueInterface.ReadValue<TElement>(valueReader);

                    if (key == count)
                    {
                        ++count;
                    }
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }

            public void InvokeElementType(IGenericInvoker invoker)
            {
                invoker.Invoke<TElement>();
            }
        }
    }
}