using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class ListRW<T, TValue> : IDataRW<int>, IDirectContent, IInitialize<T> where T : IList<TValue>
    {
        public const int DefaultCapacity = 3;

        internal T content;

        public T Content => content;

        public ValueCopyer<int> this[int key]=> new ValueCopyer<int>(this, key);

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(Count);

        public int Count => content.Count;

        object IDirectContent.DirectContent { get => content; set => content = (T)value; }

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

            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<List<TValue>>.Int64TypeHandle)
            {
                goto List;
            }

            // TODO: Capacity
            content = Activator.CreateInstance<T>();

            return;

        List:

            content = (T)(object)new List<TValue>(capacity);
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            int length = content.Count;

            for (int i = 0; i < length; i++)
            {
                ValueInterface<TValue>.WriteValue(dataWriter[i], content[i]);
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.WriteValue(valueWriter, content[key]);
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (key >= content.Count)
            {
                content.Add(ValueInterface<TValue>.ReadValue(valueReader));
            }
            else
            {
                content[key] = ValueInterface<TValue>.ReadValue(valueReader);
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int length = content.Count;

            var valueInfo = new ValueFilterInfo<int>();

            for (int i = 0; i < length; i++)
            {
                var value = content[i];

                ValueInterface<TValue>.WriteValue(valueInfo.ValueCopyer, value);

                valueInfo.Key = i;
                valueInfo.Type = typeof(TValue);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        public void OnWriteAll(IDataReader<int> dataReader)
        {
            var length = Count;

            for (int i = 0; i < length; i++)
            {
                content[i] = ValueInterface<TValue>.ReadValue(dataReader[i]);
            }
        }
    }

    internal sealed class ListRW<T> : IDataRW<int>, IDirectContent where T : IList
    {
        public const int DefaultCapacity = 3;

        internal T content;

        public T Content => content;

        public ValueCopyer<int> this[int key] => new ValueCopyer<int>(this, key);

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        public IEnumerable<int> Keys => ArrayHelper.CreateLengthIterator(Count);

        public int Count => content.Count;

        object IDirectContent.DirectContent { get => content; set => content = (T)value; }

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
                    goto List;
                }

                // TODO: Others Interface initialize.
            }

            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<ArrayList>.Int64TypeHandle)
            {
                goto List;
            }

            // TODO: Capacity
            content = Activator.CreateInstance<T>();

            return;

            List:

            content = (T)(object)new ArrayList(capacity);
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
            if (key >= content.Count)
            {
                content.Add(ValueInterface<object>.ReadValue(valueReader));
            }
            else
            {
                content[key] = ValueInterface<object>.ReadValue(valueReader);
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int length = content.Count;

            var valueInfo = new ValueFilterInfo<int>();

            for (int i = 0; i < length; i++)
            {
                var value = content[i];

                ValueInterface.WriteValue(valueInfo.ValueCopyer, value);

                valueInfo.Key = i;
                valueInfo.Type = typeof(object);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
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

    internal sealed class ListInterfaceMaper : IValueInterfaceMaper
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

            if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IList<>))
            {
                var genericArguments = item.GetGenericArguments();

                return (IValueInterface<T>)Activator.CreateInstance(typeof(ListInterface<,>).MakeGenericType(type, genericArguments[0]));
            }

            if (index < interfaces.Length)
            {
                item = interfaces[index];

                ++index;

                goto Loop;
            }

            if (typeof(IList).IsAssignableFrom(type))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(ListInterface<>).MakeGenericType(type));
            }

            return null;
        }
    }

    internal sealed class ListInterface<T, TValue> : IValueInterface<T> where T : IList<TValue>
    {
        public T ReadValue(IValueReader valueReader)
        {
            var listRW = new ListRW<T, TValue>();
            
            valueReader.ReadArray(listRW);

            return listRW.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var listRW = new ListRW<T, TValue>();

            listRW.Initialize(value);

            valueWriter.WriteArray(listRW);
        }
    }

    internal sealed class ListInterface<T> : IValueInterface<T> where T : IList
    {
        public T ReadValue(IValueReader valueReader)
        {
            var listRW = new ListRW<T>();

            valueReader.ReadArray(listRW);

            return listRW.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var listRW = new ListRW<T>();

            listRW.Initialize(value);

            valueWriter.WriteArray(listRW);
        }
    }
}