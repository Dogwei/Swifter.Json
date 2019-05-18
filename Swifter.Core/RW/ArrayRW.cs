using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    internal abstract class ArrayRW<T> : IDataRW<int>, IDirectContent, IInitialize<T> where T : class
    {
        public const int DefaultInitializeSize = 3;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ArrayRW<T> Create()
        {
            return StaticArrayRW<T>.Creater.Create();
        }

        internal protected T content;
        internal protected int count;

        public abstract T Content { get; }

        public void Initialize()
        {
            Initialize(DefaultInitializeSize);
        }

        public abstract void Initialize(T content);

        public abstract void Initialize(int capacity);

        public abstract void OnWriteValue(int key, IValueReader valueReader);

        public abstract void OnReadValue(int key, IValueWriter valueWriter);

        public abstract void OnReadAll(IDataWriter<int> dataWriter);

        public abstract void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter);

        public abstract void OnWriteAll(IDataReader<int> dataReader);

        public ValueCopyer<int> this[int key] => new ValueCopyer<int>(this, key);

        public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(Count);

        public virtual int Count => count;

        object IDirectContent.DirectContent
        {
            get => Content;
            set => Initialize((T)value);
        }

        public virtual object ReferenceToken => content;

        IValueRW IDataRW<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueWriter IDataWriter<int>.this[int key] => this[key];
    }

    internal interface IArrayRWCreater<TArray> where TArray : class
    {
        ArrayRW<TArray> Create();
    }

    internal static class StaticArrayRW<TArray> where TArray : class
    {
        public static readonly IArrayRWCreater<TArray> Creater;

        static StaticArrayRW()
        {
            var type = typeof(TArray);

            if (type.IsArray)
            {
                int rank = type.GetArrayRank();

                var elementType = type.GetElementType();

                Type internalType;

                switch (rank)
                {
                    case 1:
                        internalType = typeof(OneRankArrayRWCreater<>).MakeGenericType(elementType);
                        break;
                    case 2:
                        internalType = typeof(TwoRankArrayRWCreater<>).MakeGenericType(elementType);
                        break;
                    default:
                        internalType = typeof(MultiRankArrayRWCreater<,>).MakeGenericType(type, elementType);
                        break;
                }

                Creater = (IArrayRWCreater<TArray>)Activator.CreateInstance(internalType);
            }
            else
            {
                throw new ArgumentException($"'{typeof(TArray).FullName}' is not a Array type.");
            }
        }
    }

    internal sealed class OneRankArrayRW<T> : ArrayRW<T[]>
    {
        public override void Initialize(int capacity)
        {
            if (content == null || content.Length < capacity)
            {
                content = new T[capacity];
            }

            count = 0;
        }

        public override void Initialize(T[] content)
        {
            this.content = content;

            count = content.Length;
        }

        public override void OnWriteValue(int key, IValueReader valueReader)
        {
            if (key >= content.Length)
            {
                Array.Resize(ref content, (key * 3) + 1);
            }

            count = Math.Max(key + 1, count);

            content[key] = ValueInterface<T>.ReadValue(valueReader);
        }

        public override T[] Content
        {
            get
            {
                if (content == null)
                {
                    return null;
                }

                if (count != content.Length)
                {
                    Array.Resize(ref content, count);
                }

                return content;
            }
        }

        public override void OnReadValue(int key, IValueWriter valueWriter)
        {
            ValueInterface<T>.WriteValue(valueWriter, content[key]);
        }

        public override void OnReadAll(IDataWriter<int> dataWriter)
        {
            int length = Count;

            for (int i = 0; i < length; ++i)
            {
                ValueInterface<T>.WriteValue(dataWriter[i], content[i]);
            }
        }

        public override void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int length = Count;

            var valueInfo = new ValueFilterInfo<int>();

            for (int i = 0; i < length; ++i)
            {
                var value = content[i];

                ValueInterface<T>.WriteValue(valueInfo.ValueCopyer, value);

                valueInfo.Key = i;
                valueInfo.Type = typeof(T);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
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
    }

    internal sealed class OneRankArrayRWCreater<T> : IArrayRWCreater<T[]>
    {
        public ArrayRW<T[]> Create()
        {
            return new OneRankArrayRW<T>();
        }
    }

    internal sealed class TwoRankArrayRW<T> : ArrayRW<T[,]>
    {
        private int rank2Count;

        public override void Initialize(T[,] content)
        {
            this.content = content;

            count = content.GetLength(0);

            rank2Count = content.GetLength(1);
        }

        public override T[,] Content
        {
            get
            {
                if (content == null)
                {
                    return null;
                }

                var old1 = content.GetLength(0);
                var old2 = content.GetLength(1);

                if (old1 != count || old2 != rank2Count)
                {
                    Resize(count, rank2Count);
                }

                return content;
            }
        }

        public override void Initialize(int capacity)
        {
            if (content == null || content.GetLength(0) < capacity)
            {
                content = new T[capacity, DefaultInitializeSize];
            }

            count = 0;
        }

        public override void OnWriteValue(int key, IValueReader valueReader)
        {
            valueReader.ReadArray(new ChildrenRW(this, key));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal void Resize(int len1, int len2)
        {
            var temp = content;

            content = new T[len1, len2];

            for (int i = 0; i < count; i++)
            {
                for (int j = 0; j < rank2Count; j++)
                {
                    content[i, j] = temp[i, j];
                }
            }
        }

        public override void OnReadValue(int key, IValueWriter valueWriter)
        {
            valueWriter.WriteArray(new ChildrenRW(this, key));
        }

        public override void OnReadAll(IDataWriter<int> dataWriter)
        {
            int length = Count;

            for (int i = 0; i < length; ++i)
            {
                dataWriter[i].WriteArray(new ChildrenRW(this, i));
            }
        }

        public override void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int length = Count;

            var valueInfo = new ValueFilterInfo<int>();

            for (int i = 0; i < length; ++i)
            {
                valueInfo.ValueCopyer.WriteArray(new ChildrenRW(this, i));

                valueInfo.Key = i;
                valueInfo.Type = typeof(T[]);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
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

        private sealed class ChildrenRW : IDataRW<int>
        {
            private readonly TwoRankArrayRW<T> content;
            private readonly int baseIndex;

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public ChildrenRW(TwoRankArrayRW<T> content, int baseIndex)
            {
                this.content = content;
                this.baseIndex = baseIndex;
            }

            public void Initialize()
            {
                CheckExpansion(DefaultInitializeSize - 1);
            }

            public void Initialize(int capacity)
            {
                // Last Index
                CheckExpansion(capacity - 1);
            }

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            private void CheckExpansion(int index)
            {
                /* 仅在新增时检查扩容 */
                if (baseIndex >= content.count || index >= content.rank2Count)
                {
                    int len1 = content.content.GetLength(0);
                    int len2 = content.content.GetLength(1);

                    if (baseIndex >= len1)
                    {
                        if (index >= len2)
                        {
                            content.Resize(baseIndex + len1 + 1, index + len2 + 1);
                        }
                        else
                        {
                            content.Resize(baseIndex + len1 + 1, len2);
                        }
                    }
                    else if (index >= len2)
                    {
                        content.Resize(len1, index + len2 + 1);
                    }
                }
            }

            public void OnWriteValue(int key, IValueReader valueReader)
            {
                CheckExpansion(key);

                content.content[baseIndex, key] = ValueInterface<T>.ReadValue(valueReader);

                content.count = Math.Max(baseIndex + 1, content.count);

                content.rank2Count = Math.Max(key + 1, content.rank2Count);
            }

            public ValueCopyer<int> this[int key] => new ValueCopyer<int>(this, key);

            public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(Count);

            public int Count => content.rank2Count;

            public object ReferenceToken => null;

            IValueRW IDataRW<int>.this[int key] => this[key];

            IValueWriter IDataWriter<int>.this[int key] => this[key];

            IValueReader IDataReader<int>.this[int key] => this[key];

            public void OnReadValue(int key, IValueWriter valueWriter)
            {
                ValueInterface<T>.WriteValue(valueWriter, content.content[baseIndex, key]);
            }

            public void OnReadAll(IDataWriter<int> dataWriter)
            {
                int length = Count;

                for (int i = 0; i < length; ++i)
                {
                    ValueInterface<T>.WriteValue(dataWriter[i], content.content[baseIndex, i]);
                }
            }

            public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
            {
                int length = Count;

                var valueInfo = new ValueFilterInfo<int>();

                for (int i = 0; i < length; ++i)
                {
                    var value = content.content[baseIndex, i];

                    ValueInterface<T>.WriteValue(valueInfo.ValueCopyer, value);

                    valueInfo.Key = i;
                    valueInfo.Type = typeof(T);

                    if (valueFilter.Filter(valueInfo))
                    {
                        valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                    }
                }
            }

            public void OnWriteAll(IDataReader<int> dataReader)
            {
                int length = Count;

                for (int i = 0; i < length; ++i)
                {
                    content.content[baseIndex, i] = ValueInterface<T>.ReadValue(dataReader[i]);
                }
            }
        }
    }

    internal sealed class TwoRankArrayRWCreater<T> : IArrayRWCreater<T[,]>
    {
        public ArrayRW<T[,]> Create()
        {
            return new TwoRankArrayRW<T>();
        }
    }
}