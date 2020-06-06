
using Swifter.Tools;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Swifter.RW
{

    internal sealed class ListRW<T> : IDataRW<int> where T : IList
    {
        public const int DefaultCapacity = 3;

        static readonly bool IsAssignableFromArrayList = typeof(T).IsAssignableFrom(typeof(ArrayList));

        internal T content;

        public ValueCopyer<int> this[int key] => new ValueCopyer<int>(this, key);

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(Count);

        public int Count => content.Count;

        public object Content
        {
            get => content;
            set => content = (T)value;
        }

        public Type ContentType => typeof(T);

        IValueRW IDataRW<int>.this[int key] => this[key];

        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        public void Initialize(int capacity)
        {
            if (IsAssignableFromArrayList)
            {
                Underlying.As<T, ArrayList>(ref content) = new ArrayList(capacity);
            }
            else
            {
                // TODO: Capacity
                content = Activator.CreateInstance<T>();
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            int length = content.Count;

            for (int i = 0; i < length; i++)
            {
                ValueInterface.WriteValue(dataWriter[i], content[i]);
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            ValueInterface.WriteValue(valueWriter, content[key]);
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (key == Count)
            {
                content.Add(ValueInterface<object>.ReadValue(valueReader));
            }
            else
            {
                content[key] = ValueInterface<object>.ReadValue(valueReader);
            }
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
            var length = Count;

            for (int i = 0; i < length; i++)
            {
                content[i] = ValueInterface<object>.ReadValue(dataReader[i]);
            }
        }
    }
}