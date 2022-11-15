using Swifter.Tools;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Swifter.RW
{
    internal sealed class MultiDimArrayRW<TArray, TElement> : IArrayRW, IFastArrayRW where TArray : class
    {
        public const int DefaultCapacity = 3;

        public static readonly int Rank = typeof(TArray).GetArrayRank();
        public static readonly int MaxDimension = Rank - 1;
        public static readonly ArrayAppendingInfo[] appendingInfos = new ArrayAppendingInfo[Rank];

        static MultiDimArrayRW()
        {
            for (int i = 0; i < Rank; i++)
            {
                appendingInfos[i].MostClosestMeanCommonlyUsedLength = DefaultCapacity;
            }
        }

        object content;

        public MultiDimArrayRW()
        {
            content = new FirstContent();
        }

        MultiDimArrayRW(object previous, int index)
        {
            if (previous is FollowContent followContent)
            {
                content = new FollowContent(followContent.First, followContent, followContent.Dimension + 1, index);
            }
            else
            {
                content = new FollowContent(Unsafe.As<FirstContent>(previous), previous, 1, index);
            }
        }

        public bool IsFirst => content is FirstContent;

        public bool IsLast => Dimension == MaxDimension;

        public int Dimension => content is FollowContent followContent ? followContent.Dimension : 0;

        public int Count
        {
            get
            {
                if (content is FollowContent followContent)
                {
                    return followContent.First.Counts[followContent.Dimension];
                }

                return Unsafe.As<FirstContent>(content).Counts[0];
            }
        }

        public Type? ContentType => IsFirst ? typeof(TArray) : null;

        public Type? ValueType => IsLast ? typeof(TElement) : null;

        public TArray? Content
        {
            get
            {
                if (content is FirstContent firstContent)
                {
                    if (firstContent.NeedResize())
                    {
                        firstContent.Resize();
                    }

                    for (int i = Rank - 1; i >= 0; i--)
                    {
                        appendingInfos[i].AddUsedLength(firstContent.Counts[i]);
                    }

                    return firstContent.Array;
                }

                throw new NotSupportedException();
            }
            set
            {
                if (content is FirstContent firstContent)
                {
                    firstContent.Array = (TArray?)value;

                    if (value is null)
                    {
                        firstContent.Counts.Initialize();
                        firstContent.Lengths.Initialize();
                    }
                    else
                    {
                        for (int i = Rank - 1; i >= 0; i--)
                        {
                            firstContent.Counts[i] = firstContent.Lengths[i] = Unsafe.As<Array>(value).GetLength(i);
                        }
                    }

                    return;
                }

                throw new NotSupportedException();
            }
        }

        ref TArray? Array
        {
            get
            {
                if (content is FollowContent followContent)
                {
                    return ref followContent.First.Array;
                }

                return ref Unsafe.As<FirstContent>(content).Array;
            }
        }

        public IValueRW this[int key] => IsLast ? new LastValueRW(this, key) : new ForwardValueRW(this, key);

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void CheckGrow()
        {
            if (content is FirstContent firstContent)
            {
                firstContent.CheckGrow();
            }
            else
            {
                Unsafe.As<FollowContent>(content).First.CheckGrow();
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public int GetOffset()
        {
            if (content is FollowContent followContent)
            {
                return followContent.GetOffset();
            }

            return 0;
        }

        public void Initialize()
        {
            Initialize(appendingInfos[Dimension].MostClosestMeanCommonlyUsedLength);
        }

        public void Initialize(int capacity)
        {
            if (content is FirstContent firstContent)
            {
                firstContent.Array = null;

                firstContent.Counts.Initialize();
                firstContent.Lengths.Initialize();

                firstContent.Lengths[0] = capacity;

                firstContent.NeedGrow = true;
            }
            else
            {
                var followContent = Unsafe.As<FollowContent>(content);

                if (capacity > followContent.First.Lengths[followContent.Dimension])
                {
                    followContent.First.Lengths[followContent.Dimension] = capacity;

                    followContent.First.NeedGrow = true;
                }
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, RWStopToken stopToken = default)
        {
            var count = Count;

            if (IsLast)
            {
                CheckGrow();

                if (Array is null)
                {
                    throw new NullReferenceException(nameof(content));
                }

                ref var elements = ref ArrayHelper.AddrOfArrayElement<TElement?>(Unsafe.As<Array>(Array), GetOffset());

                for (int i = 0; i < count; i++)
                {
                    ValueInterface.WriteValue(dataWriter[i], Unsafe.Add(ref elements, i));
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    dataWriter[i].WriteArray(new MultiDimArrayRW<TArray, TElement>(content, i));
                }
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            var count = Count;

            if (key < 0 || key >= count)
            {
                throw new IndexOutOfRangeException();
            }

            if (IsLast)
            {
                CheckGrow();

                if (Array is null)
                {
                    throw new NullReferenceException(nameof(content));
                }

                ValueInterface.WriteValue(valueWriter, ArrayHelper.AddrOfArrayElement<TElement?>(Unsafe.As<Array>(Array), GetOffset() + key));
            }
            else
            {
                valueWriter.WriteArray(new MultiDimArrayRW<TArray, TElement>(content, key));
            }
        }

        public void OnWriteAll(IDataReader<int> dataReader, RWStopToken stopToken = default)
        {
            var count = Count;

            if (IsLast)
            {
                CheckGrow();

                if (Array is null)
                {
                    throw new NullReferenceException(nameof(content));
                }

                ref var elements = ref ArrayHelper.AddrOfArrayElement<TElement?>(Unsafe.As<Array>(Array), GetOffset());

                for (int i = 0; i < count; i++)
                {
                    Unsafe.Add(ref elements, i) = ValueInterface<TElement>.ReadValue(dataReader[i]);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    dataReader[i].ReadArray(new MultiDimArrayRW<TArray, TElement>(content, i));
                }
            }
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            var firstContent = content as FirstContent ?? Unsafe.As<FollowContent>(content).First;

            ref var count = ref firstContent.Counts[Dimension];
            ref var length = ref firstContent.Lengths[Dimension];

            if (key < 0 || key > count)
            {
                throw new IndexOutOfRangeException();
            }

            if (key == count && count == length)
            {
                length += length + 1;

                firstContent.NeedGrow = true;
            }

            if (IsLast)
            {
                firstContent.CheckGrow();

                if (firstContent.Array is null)
                {
                    throw new NullReferenceException(nameof(content));
                }

                ArrayHelper.AddrOfArrayElement<TElement?>(Unsafe.As<Array>(firstContent.Array), GetOffset() + key) = ValueInterface.ReadValue<TElement>(valueReader);
            }
            else
            {
                valueReader.ReadArray(new MultiDimArrayRW<TArray, TElement>(content, key));
            }

            if (key == count)
            {
                ++count;
            }
        }

        public void WriteTo(IFastArrayValueWriter valueWriter)
        {
            if (!IsLast)
            {
                throw new NotSupportedException(/* 仅最后一维度支持快速写入到 */);
            }

            if (Array is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            valueWriter.WriteArray(
                ref ArrayHelper.AddrOfArrayElement<TElement?>(Unsafe.As<Array>(Array), GetOffset()),
                Count
                );
        }

        public void ReadFrom(IFastArrayValueReader valueReader)
        {
            if (!IsLast)
            {
                throw new NotSupportedException(/* 仅最后一维度支持快速读取自 */);
            }

            var firstContent = content as FirstContent ?? Unsafe.As<FollowContent>(content).First;

            ref var count = ref firstContent.Counts[Dimension];
            ref var length = ref firstContent.Lengths[Dimension];

            var array = valueReader.ReadArray<TElement>(length, out var size);

            if (size > count)
            {
                count = size;
            }

            if (count > length)
            {
                length = count;

                firstContent.NeedGrow = true;
            }

            firstContent.CheckGrow();

            if (firstContent.Array is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            ref var elements = ref ArrayHelper.AddrOfArrayElement<TElement?>(Unsafe.As<Array>(firstContent.Array), GetOffset());

            ArrayHelper.Memmove(ref elements, ref array[0], size);
        }

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        object? IDataRW.Content { get => Content; set => Content = (TArray?)value; }

        object? IDataReader.Content { get => Content; set => Content = (TArray?)value; }

        object? IDataWriter.Content { get => Content; set => Content = (TArray?)value; }

        sealed class FirstContent
        {
            public TArray? Array;
            public bool NeedGrow;
            public int Version;
            public readonly int[] Lengths;
            public readonly int[] Counts;

            public FirstContent()
            {
                Lengths = new int[Rank];
                Counts = new int[Rank];
            }

            public void CheckGrow()
            {
                if (NeedGrow)
                {
                    Grow();
                }
            }

            public void Grow()
            {
                ArrayHelper.Resize<TElement>(ref Unsafe.As<TArray?, Array?>(ref Array), Lengths);

                ++Version;

                NeedGrow = false;
            }

            public bool NeedResize()
            {
                if (Array is null && NeedGrow)
                {
                    return true;
                }

                for (int i = Rank - 1; i >= 0; i--)
                {
                    if (Counts[i] != Lengths[i])
                    {
                        return true;
                    }
                }

                return false;
            }

            public void Resize()
            {
                ArrayHelper.Resize<TElement>(ref Unsafe.As<TArray?, Array?>(ref Array), Counts);

                for (int i = Rank - 1; i >= 0; i--)
                {
                    Lengths[i] = Counts[i];
                }

                ++Version;

                NeedGrow = false;
            }
        }

        sealed class FollowContent
        {
            public readonly FirstContent First;
            public readonly object Previous;
            public readonly int Dimension;
            public readonly int Index;

            int offset;
            int offsetVersion;
            
            public FollowContent(FirstContent first, object previous, int dimension, int index)
            {
                First = first;
                Previous = previous;
                Dimension = dimension;
                Index = index;

                offset = -1;
            }

            public int GetOffset()
            {
                if (offset >= 0 && offsetVersion == First.Version)
                {
                    return offset;
                }

                offset = 0;
                offsetVersion = First.Version;

                var array = Unsafe.As<Array>(First.Array);

                if (array is null)
                {
                    throw new NullReferenceException();
                }

                var index = Index;
                var dimension = Dimension;
                var length = array.GetLength(dimension);

                if (dimension > 1)
                {
                    var followContent = Unsafe.As<FollowContent>(Previous);

                    do
                    {
                        offset += index * length;

                        index = followContent.Index;

                        --dimension;

                        length *= array.GetLength(dimension);

                        followContent = Unsafe.As<FollowContent>(followContent.Previous);

                    } while (dimension > 1);
                }

                offset += index * length;

                return offset;
            }
        }

        sealed class LastValueRW : BaseGenericRW<TElement>, IValueRW<TElement>
        {
            public readonly MultiDimArrayRW<TArray, TElement> BaseRW;
            public readonly int Index;

            public LastValueRW(MultiDimArrayRW<TArray, TElement> baseRW, int index)
            {
                BaseRW = baseRW;
                Index = index;
            }

            public override TElement? ReadValue()
            {
                var count = BaseRW.Count;

                if (Index < 0 || Index >= count)
                {
                    throw new IndexOutOfRangeException();
                }

                BaseRW.CheckGrow();

                if (BaseRW.Array is null)
                {
                    throw new NullReferenceException(nameof(content));
                }

                return ArrayHelper.AddrOfArrayElement<TElement?>(Unsafe.As<Array>(BaseRW.Array), BaseRW.GetOffset() + Index);
            }

            public override void WriteValue(TElement? value)
            {
                var firstContent = BaseRW.content as FirstContent ?? Unsafe.As<FollowContent>(BaseRW.content).First;

                ref var count = ref firstContent.Counts[BaseRW.Dimension];
                ref var length = ref firstContent.Lengths[BaseRW.Dimension];

                if (Index < 0 || Index > count)
                {
                    throw new IndexOutOfRangeException();
                }

                if (Index == count && count == length)
                {
                    length += length + 1;

                    firstContent.NeedGrow = true;
                }

                firstContent.CheckGrow();

                if (BaseRW.Array is null)
                {
                    throw new NullReferenceException(nameof(content));
                }

                ArrayHelper.AddrOfArrayElement<TElement?>(Unsafe.As<Array>(BaseRW.Array), BaseRW.GetOffset() + Index) = value;

                if (Index == count)
                {
                    ++count;
                }
            }
        }

        sealed class ForwardValueRW : BaseDirectRW
        {
            public readonly MultiDimArrayRW<TArray, TElement> BaseRW;
            public readonly int Index;

            public ForwardValueRW(MultiDimArrayRW<TArray, TElement> baseRW, int index)
            {
                BaseRW = baseRW;
                Index = index;
            }

            public override Type? ValueType => null;

            public override object? DirectRead()
            {
                var count = BaseRW.Count;

                if (Index < 0 || Index >= count)
                {
                    throw new IndexOutOfRangeException();
                }

                return new MultiDimArrayRW<TArray, TElement>(BaseRW.content, Index);
            }

            public override void DirectWrite(object? value)
            {
                var firstContent = BaseRW.content as FirstContent ?? Unsafe.As<FollowContent>(BaseRW.content).First;

                ref var count = ref firstContent.Counts[BaseRW.Dimension];

                if (Index < 0 || Index > count)
                {
                    throw new IndexOutOfRangeException();
                }

                if (value is null)
                {
                    throw new NullReferenceException(nameof(value));
                }

                var dataReader = RWHelper.CreateReader(value)?.As<int>();

                if (dataReader is null)
                {
                    throw new NotSupportedException();
                }

                var dataRW = new MultiDimArrayRW<TArray, TElement>(BaseRW.content, Index);

                var capacity = dataReader.Count;

                if (capacity < 0)
                {
                    capacity = DefaultCapacity;
                }

                dataRW.Initialize(capacity);

                dataReader.OnReadAll(dataRW);

                if (Index == count)
                {
                    ++count;
                }
            }

            public override void ReadArray(IDataWriter<int> dataWriter)
            {
                var count = BaseRW.Count;

                if (Index < 0 || Index >= count)
                {
                    throw new IndexOutOfRangeException();
                }

                var dataRW = new MultiDimArrayRW<TArray, TElement>(BaseRW.content, Index);

                dataWriter.Initialize(dataRW.Count);

                dataRW.OnReadAll(dataWriter);
            }

            public override void WriteArray(IDataReader<int> dataReader)
            {
                var firstContent = BaseRW.content as FirstContent ?? Unsafe.As<FollowContent>(BaseRW.content).First;

                ref var count = ref firstContent.Counts[BaseRW.Dimension];

                if (Index < 0 || Index > count)
                {
                    throw new IndexOutOfRangeException();
                }

                var dataRW = new MultiDimArrayRW<TArray, TElement>(BaseRW.content, Index);

                var capacity = dataReader.Count;

                if (capacity < 0)
                {
                    capacity = DefaultCapacity;
                }

                dataRW.Initialize(capacity);

                dataReader.OnReadAll(dataRW);

                if (Index == count)
                {
                    ++count;
                }
            }
        }
    }
}