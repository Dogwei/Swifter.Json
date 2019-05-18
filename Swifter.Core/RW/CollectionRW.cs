using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class CollectionRW<T, TValue> : IDataRW<int>, IDirectContent, IInitialize<T> where T : ICollection<TValue>
    {
        public const int DefaultCapacity = 3;

        internal T content;

        public T Content => content;

        public ValueCopyer<int> this[int key]=> new ValueCopyer<int>(this, key);

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(Count);

        public int Count => content.Count;

        object IDirectContent.DirectContent
        {
            get => content;
            set => content = (T)value;
        }

        public object ReferenceToken => content;

        IValueRW IDataRW<int>.this[int key] => this[key];

        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        public void Initialize(T content)
        {
            this.content = content;
        }

        public void Initialize(int capacity)
        {
            if (TypeInfo<T>.IsInterface)
            {
                if (typeof(T).IsAssignableFrom(typeof(List<TValue>)))
                {
                    goto List;
                }

                // TODO: Others Interface initialize.
            }

            // TODO: Capacity
            content = Activator.CreateInstance<T>();

            return;

        List:

            content = (T)(object)new List<TValue>(capacity);
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            var index = 0;

            foreach (var item in content)
            {
                ValueInterface<TValue>.WriteValue(dataWriter[index], item);

                ++index;
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            if (content is IList<TValue> list)
            {
                ValueInterface<TValue>.WriteValue(valueWriter, list[key]);

                return;
            }

            throw new NotSupportedException();
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            var value = ValueInterface<TValue>.ReadValue(valueReader);

            if (key >= content.Count)
            {
                content.Add(value);

                return;
            }

            if (content is IList<TValue> list)
            {
                list[key] = value;

                return;
            }

            throw new NotSupportedException($"TODO: '{typeof(T)}' not supported set elements.");
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            var index = 0;

            var valueInfo = new ValueFilterInfo<int>();

            foreach (var item in content)
            {
                ValueInterface<TValue>.WriteValue(valueInfo.ValueCopyer, item);

                valueInfo.Key = index;
                valueInfo.Type = typeof(TValue);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
            if (content is IList<TValue> list)
            {
                var length = Count;

                for (int i = 0; i < length; i++)
                {
                    list[i] = ValueInterface<TValue>.ReadValue(dataReader[i]);
                }

                return;
            }

            throw new NotSupportedException($"TODO: '{typeof(T)}' not supported set elements.");
        }
    }

    internal sealed class CollectionRW<T> : IDataRW<int>, IDirectContent where T : ICollection
    {
        public const int DefaultCapacity = 3;

        internal T content;

        public T Content => content;

        public ValueCopyer<int> this[int key]=> throw new NotSupportedException();

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(content.Count);

        public int Count => content.Count;

        object IDirectContent.DirectContent
        {
            get => content;
            set => content = (T)value;
        }

        public object ReferenceToken => content;

        IValueRW IDataRW<int>.this[int key] => this[key];

        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        public void Initialize(T content)
        {
            this.content = content;
        }

        public void Initialize(int capacity)
        {
            if (TypeInfo<T>.IsInterface)
            {
                if (typeof(T).IsAssignableFrom(typeof(ArrayList)))
                {
                    goto Collection;
                }

                // TODO: Others Interface initialize.
            }

            // TODO: Capacity.
            content = Activator.CreateInstance<T>();

            return;

        Collection:

            content = (T)(object)new ArrayList(capacity);
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            var index = 0;

            foreach (var item in content)
            {
                ValueInterface.WriteValue(dataWriter[index], item);

                ++index;
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            if (content is IList list)
            {
                ValueInterface.WriteValue(valueWriter, list[key]);

                return;
            }

            throw new NotSupportedException();
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (content is IList list)
            {
                var value = ValueInterface<object>.ReadValue(valueReader);

                if (key < content.Count)
                {
                    list[key] = value;
                }
                else
                {
                    list.Add(value);
                }

                return;
            }

            throw new NotSupportedException();
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            var index = 0;

            var valueInfo = new ValueFilterInfo<int>();

            foreach (var item in content)
            {
                ValueInterface.WriteValue(valueInfo.ValueCopyer, item);

                valueInfo.Key = index;
                valueInfo.Type = typeof(object);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
            if (content is IList list)
            {
                var length = Count;

                for (int i = 0; i < length; i++)
                {
                    list[i] = ValueInterface<object>.ReadValue(dataReader[i]);
                }

                return;
            }

            throw new NotSupportedException($"TODO: '{typeof(T)}' not supported set elements.");
        }
    }

    internal sealed class CollectionInterfaceMaper : IValueInterfaceMaper
    {
        public IValueInterface<T> TryMap<T>()
        {
            var type = typeof(T);

            if (type.IsArray || typeof(Array).IsAssignableFrom(type))
            {
                return null;
            }

            var item = type;

            var interfaces = type.GetInterfaces();

            var index = 0;

        Loop:

            if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(ICollection<>))
            {
                var genericArguments = item.GetGenericArguments();

                return (IValueInterface<T>)Activator.CreateInstance(typeof(CollectionInterface<,>).MakeGenericType(type, genericArguments[0]));
            }

            if (index < interfaces.Length)
            {
                item = interfaces[index];

                ++index;

                goto Loop;
            }

            if (typeof(ICollection).IsAssignableFrom(type))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(CollectionInterface<>).MakeGenericType(type));
            }

            return null;
        }
    }

    internal sealed class CollectionInterface<T, TValue> : IValueInterface<T> where T : ICollection<TValue>
    {
        public T ReadValue(IValueReader valueReader)
        {
            var collectionRW = new CollectionRW<T, TValue>();

            valueReader.ReadArray(collectionRW);

            return collectionRW.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var collectionRW = new CollectionRW<T, TValue>();

            collectionRW.Initialize(value);

            valueWriter.WriteArray(collectionRW);
        }
    }

    internal sealed class CollectionInterface<T> : IValueInterface<T> where T : ICollection
    {
        public T ReadValue(IValueReader valueReader)
        {
            var collectionRW = new CollectionRW<T>();

            valueReader.ReadArray(collectionRW);

            return collectionRW.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var collectionRW = new CollectionRW<T>();

            collectionRW.Initialize(value);

            valueWriter.WriteArray(collectionRW);
        }
    }
}