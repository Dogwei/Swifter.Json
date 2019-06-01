using Swifter.Readers;
using Swifter.Writers;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    internal sealed class MultiRankArrayRW<TArray, TValue> : ArrayRW<TArray> where TArray : class
    {
        private static readonly int maxRankIndex;
        private static readonly int rank;

        static MultiRankArrayRW()
        {
            rank = typeof(TArray).GetArrayRank();

            maxRankIndex = rank - 1;
        }




        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void InternalCopyValues(TArray source, int[] indices, TArray destination, int[] counts, int rankIndex)
        {
            int length = counts[rankIndex];

            if (rankIndex == maxRankIndex)
            {
                while (--length >= 0)
                {
                    indices[rankIndex] = length;

                    SetValue(destination, indices, GetValue(source, indices));
                }
            }
            else
            {
                while (--length >= 0)
                {
                    indices[rankIndex] = length;

                    InternalCopyValues(source, indices, destination, counts, rankIndex + 1);
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void InternalCopyValues(TValue[,,] source, TValue[,,] destination, int[] counts)
        {
            for (int a = counts[0] - 1; a >= 0; --a)
            {
                for (int b = counts[1] - 1; b >= 0; --b)
                {
                    for (int c = counts[2] - 1; c >= 0; --c)
                    {
                        destination[a, b, c] = source[a, b, c];
                    }
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void InternalCopyValues(TValue[,,,] source, TValue[,,,] destination, int[] counts)
        {
            for (int a = counts[0] - 1; a >= 0; --a)
            {
                for (int b = counts[1] - 1; b >= 0; --b)
                {
                    for (int c = counts[2] - 1; c >= 0; --c)
                    {
                        for (int d = counts[3] - 1; d >= 0; --d)
                        {
                            destination[a, b, c, d] = source[a, b, c, d];
                        }
                    }
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void InternalCopyValues(TValue[,,,,] source, TValue[,,,,] destination, int[] counts)
        {
            for (int a = counts[0] - 1; a >= 0; --a)
            {
                for (int b = counts[1] - 1; b >= 0; --b)
                {
                    for (int c = counts[2] - 1; c >= 0; --c)
                    {
                        for (int d = counts[3] - 1; d >= 0; --d)
                        {
                            for (int e = counts[4] - 1; e >= 0; --e)
                            {
                                destination[a, b, c, d, e] = source[a, b, c, d, e];
                            }
                        }
                    }
                }
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void InternalCopyValues(TValue[,,,,,] source, TValue[,,,,,] destination, int[] counts)
        {
            for (int a = counts[0] - 1; a >= 0; --a)
            {
                for (int b = counts[1] - 1; b >= 0; --b)
                {
                    for (int c = counts[2] - 1; c >= 0; --c)
                    {
                        for (int d = counts[3] - 1; d >= 0; --d)
                        {
                            for (int e = counts[4] - 1; e >= 0; --e)
                            {
                                for (int f = counts[5] - 1; f >= 0; --f)
                                {
                                    destination[a, b, c, d, e, f] = source[a, b, c, d, e, f];
                                }
                            }
                        }
                    }
                }
            }
        }
        

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void CopyValues(TArray source, TArray destination, int[] counts)
        {
            if (typeof(TArray) == typeof(TValue[,,]))
            {
                InternalCopyValues(Unsafe.As<TValue[,,]>(source), Unsafe.As<TValue[,,]>(destination), counts);
            }
            else if (typeof(TArray) == typeof(TValue[,,,]))
            {
                InternalCopyValues(Unsafe.As<TValue[,,,]>(source), Unsafe.As<TValue[,,,]>(destination), counts);
            }
            else if (typeof(TArray) == typeof(TValue[,,,,]))
            {
                InternalCopyValues(Unsafe.As<TValue[,,,,]>(source), Unsafe.As<TValue[,,,,]>(destination), counts);
            }
            else if (typeof(TArray) == typeof(TValue[,,,,,]))
            {
                InternalCopyValues(Unsafe.As<TValue[,,,,,]>(source), Unsafe.As<TValue[,,,,,]>(destination), counts);
            }
            else
            {
                InternalCopyValues(source, new int[rank], destination, counts, 0);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static TArray NewInstance(int[] lengths)
        {
            if (typeof(TArray) == typeof(TValue[,,]))
            {
                return Unsafe.As<TArray>(new TValue[lengths[0], lengths[1], lengths[2]]);
            }
            else if (typeof(TArray) == typeof(TValue[,,,]))
            {
                return Unsafe.As<TArray>(new TValue[lengths[0], lengths[1], lengths[2], lengths[3]]);
            }
            else if (typeof(TArray) == typeof(TValue[,,,,]))
            {
                return Unsafe.As<TArray>(new TValue[lengths[0], lengths[1], lengths[2], lengths[3], lengths[4]]);
            }
            else if (typeof(TArray) == typeof(TValue[,,,,,]))
            {
                return Unsafe.As<TArray>(new TValue[lengths[0], lengths[1], lengths[2], lengths[3], lengths[4], lengths[5]]);
            }
            else
            {
                return Unsafe.As<TArray>(Array.CreateInstance(typeof(TValue), lengths));
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static int GetLength(TArray array, int rank)
        {
            return Unsafe.As<Array>(array).GetLength(rank);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static TValue GetValue(TArray array, int[] indices)
        {
            if (typeof(TArray) == typeof(TValue[,,]))
            {
                return Unsafe.As<TValue[,,]>(array)[indices[0], indices[1], indices[2]];
            }
            else if (typeof(TArray) == typeof(TValue[,,,]))
            {
                return Unsafe.As<TValue[,,,]>(array)[indices[0], indices[1], indices[2], indices[3]];
            }
            else if (typeof(TArray) == typeof(TValue[,,,,]))
            {
                return Unsafe.As<TValue[,,,,]>(array)[indices[0], indices[1], indices[2], indices[3], indices[4]];
            }
            else if (typeof(TArray) == typeof(TValue[,,,,,]))
            {
                return Unsafe.As<TValue[,,,,,]>(array)[indices[0], indices[1], indices[2], indices[3], indices[4], indices[5]];
            }
            else
            {
                return (TValue)Unsafe.As<Array>(array).GetValue(indices);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static void SetValue(TArray array, int[] indices, TValue value)
        {
            if (typeof(TArray) == typeof(TValue[,,]))
            {
                Unsafe.As<TValue[,,]>(array)[indices[0], indices[1], indices[2]] = value;
            }
            else if (typeof(TArray) == typeof(TValue[,,,]))
            {
                Unsafe.As<TValue[,,,]>(array)[indices[0], indices[1], indices[2], indices[3]] = value;
            }
            else if (typeof(TArray) == typeof(TValue[,,,,]))
            {
                Unsafe.As<TValue[,,,,]>(array)[indices[0], indices[1], indices[2], indices[3], indices[4]] = value;
            }
            else if (typeof(TArray) == typeof(TValue[,,,,,]))
            {
                Unsafe.As<TValue[,,,,,]>(array)[indices[0], indices[1], indices[2], indices[3], indices[4], indices[5]] = value;
            }
            else
            {
                Unsafe.As<Array>(array).SetValue(value, indices);
            }
        }






        private readonly int currentRankIndex;
        private readonly int baseIndex;

        private readonly int[] counts;
        private readonly int[] indices;

        private readonly MultiRankArrayRW<TArray, TValue> rootRank;
        private readonly MultiRankArrayRW<TArray, TValue> lastRank;

        public MultiRankArrayRW()
        {
            rootRank = this;

            counts = new int[rank];
            indices = new int[rank];
        }

        internal MultiRankArrayRW(int currentRankIndex, int maxRankIndex, int baseIndex, MultiRankArrayRW<TArray, TValue> lastRank)
        {
            this.currentRankIndex = currentRankIndex;
            this.baseIndex = baseIndex;
            this.lastRank = lastRank;

            rootRank = lastRank.rootRank;
        }

        public override TArray Content
        {
            get
            {
                var content = rootRank.content;

                if (content == null)
                {
                    return default;
                }

                int[] counts = rootRank.counts;

                for (int i = maxRankIndex; i >= 0; --i)
                {
                    if (GetLength(content, i) != counts[i])
                    {
                        goto Resize;
                    }
                }

                return rootRank.content;

            Resize:

                Resize(counts);

                return rootRank.content;
            }
        }

        public override int Count => rootRank.counts[currentRankIndex];

        public override object ReferenceToken
        {
            get
            {
                return baseIndex == 0 ? content : null;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void Resize(int[] lengths)
        {
            var temp = rootRank.content;

            var content = NewInstance(lengths);

            CopyValues(temp, content, rootRank.counts);

            rootRank.content = content;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void CheckExpansion(int index)
        {
            var content = rootRank.content;

            int length;

            /* 仅在新增时检查扩容 */
            if (index >= rootRank.counts[currentRankIndex] && index >= (length = GetLength(content, currentRankIndex)))
            {
                var lengths = new int[maxRankIndex + 1];

                for (int i = maxRankIndex; i >= 0; --i)
                {
                    lengths[i] = GetLength(content, i);
                }

                lengths[currentRankIndex] = length + index + 1;

                Resize(lengths);
            }
        }

        public override void Initialize(TArray content)
        {
            count = 0;

            rootRank.counts[currentRankIndex] = 0;
            
            rootRank.content = content;

            for (int i = maxRankIndex; i >= 0; --i)
            {
                rootRank.counts[i] = GetLength(content, i);
            }
        }

        public override void Initialize(int capacity)
        {
            count = 0;

            rootRank.counts[currentRankIndex] = 0;

            if (content == null || GetLength(content, 0) < capacity)
            {
                if (this == rootRank)
                {
                    int[] lengths = new int[maxRankIndex + 1];

                    for (int i = maxRankIndex; i >= 0; --i)
                    {
                        lengths[i] = DefaultInitializeSize;
                    }

                    lengths[currentRankIndex] = capacity;

                    content = NewInstance(lengths);

                    return;
                }

                CheckExpansion(capacity - 1);
            }
        }

        public override void OnReadAll(IDataWriter<int> dataWriter)
        {
            int length = rootRank.counts[currentRankIndex];

            if (currentRankIndex == maxRankIndex)
            {
                var indices = GetIndices();

                var content = rootRank.content;

                for (int i = 0; i < length; ++i)
                {
                    indices[currentRankIndex] = i;

                    var value = GetValue(content, indices);

                    ValueInterface<TValue>.WriteValue(dataWriter[i], value);
                }
            }
            else
            {
                for (int i = 0; i < length; ++i)
                {
                    dataWriter[i].WriteArray(new MultiRankArrayRW<TArray, TValue>(currentRankIndex + 1, maxRankIndex, i, this));
                }
            }
        }

        public override void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int length = rootRank.counts[currentRankIndex];

            var valueInfo = new ValueFilterInfo<int>();

            if (currentRankIndex == maxRankIndex)
            {
                var indices = GetIndices();

                var content = rootRank.content;

                for (int i = 0; i < length; ++i)
                {
                    valueInfo.Key = i;
                    valueInfo.Type = typeof(TValue);

                    indices[currentRankIndex] = i;

                    var value = GetValue(content, indices);

                    ValueInterface<TValue>.WriteValue(valueInfo.ValueCopyer, value);

                    if (valueFilter.Filter(valueInfo))
                    {
                        valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < length; ++i)
                {
                    valueInfo.Key = i;
                    valueInfo.Type = typeof(Array);

                    valueInfo.ValueCopyer.WriteArray(new MultiRankArrayRW<TArray, TValue>(currentRankIndex + 1, maxRankIndex, i, this));

                    if (valueFilter.Filter(valueInfo))
                    {
                        valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                    }
                }
            }
        }

        public override void OnWriteAll(IDataReader<int> dataReader)
        {
            var length = Count;

            for (int i = 0; i < length; i++)
            {
                OnWriteValue(i, dataReader[i]);
            }
        }

        public override void OnReadValue(int key, IValueWriter valueWriter)
        {
            if (currentRankIndex == maxRankIndex)
            {
                var indices = GetIndices();

                indices[currentRankIndex] = key;

                var value = GetValue(rootRank.content, indices);

                ValueInterface<TValue>.WriteValue(valueWriter, value);

                return;
            }

            valueWriter.WriteArray(new MultiRankArrayRW<TArray, TValue>(currentRankIndex + 1, maxRankIndex, key, this));
        }

        public override void OnWriteValue(int key, IValueReader valueReader)
        {
            CheckExpansion(key);

            rootRank.counts[currentRankIndex] = Math.Max(key + 1, rootRank.counts[currentRankIndex]);

            if (currentRankIndex == maxRankIndex)
            {
                var value = ValueInterface<TValue>.ReadValue(valueReader);

                var indices = GetIndices();

                indices[currentRankIndex] = key;

                SetValue(rootRank.content, indices, value);
            }
            else
            {
                valueReader.ReadArray(new MultiRankArrayRW<TArray, TValue>(currentRankIndex + 1, maxRankIndex, key, this));
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal int[] GetIndices()
        {
            var indices = rootRank.indices;

            var rank = this;

            while (rank.lastRank != null)
            {
                var index = rank.baseIndex;

                rank = rank.lastRank;

                indices[rank.currentRankIndex] = index;
            }

            return indices;
        }
    }

    internal sealed class MultiRankArrayRWCreater<TArray, TValue> : IArrayRWCreater<TArray> where TArray : class
    {
        public ArrayRW<TArray> Create()
        {
            return new MultiRankArrayRW<TArray, TValue>();
        }
    }
}