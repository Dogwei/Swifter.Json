
using Swifter.Tools;

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    internal abstract class ArrayRW<TArray> : IDataRW<int>, IDirectContent, IInitialize<TArray> where TArray : class
    {
        public const int DefaultCapacity = 3;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ArrayRW<TArray> Create(TArray array)
        {
            var rWer = StaticArrayRW<TArray>.Creater.Create();

            rWer.Initialize(array);

            return rWer;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static ArrayRW<TArray> CreateAppend()
        {
            return StaticArrayRW<TArray>.Creater.CreateArrayBuilder();
        }

        internal protected TArray content;

        public abstract TArray Content { get; }

        public abstract TArray GetCopy();

        public abstract int Count { get; }

        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        public abstract void Initialize(TArray content);

        public abstract void Initialize(int capacity);

        public abstract void OnWriteValue(int key, IValueReader valueReader);

        public abstract void OnReadValue(int key, IValueWriter valueWriter);

        public virtual void OnReadAll(IDataWriter<int> dataWriter)
        {
            var length = Count;

            for (int i = 0; i < length; i++)
            {
                OnReadValue(i, dataWriter[i]);
            }
        }

        public virtual void OnWriteAll(IDataReader<int> dataReader)
        {
            var length = Count;

            for (int i = 0; i < length; i++)
            {
                OnWriteValue(i, dataReader[i]);
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter) => OnReadAll(new DataFilterWriter<int>(dataWriter, valueFilter));

        public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(Count);


        object IDirectContent.DirectContent
        {
            get => Content;
            set => Initialize((TArray)value);
        }

        public object ReferenceToken => content;

        public abstract IValueRW this[int key] { get; }

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueWriter IDataWriter<int>.this[int key] => this[key];
    }
}