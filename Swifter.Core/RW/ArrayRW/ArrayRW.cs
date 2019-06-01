using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
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
}