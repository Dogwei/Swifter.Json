using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class DictionaryRW<T, TKey, TValue> : IDataRW<TKey>, IDirectContent, IInitialize<T> where T : IDictionary<TKey, TValue>
    {
        public const int DefaultCapacity = 3;

        internal T content;

        public T Content
        {
            get
            {
                return content;
            }
        }

        public ValueCopyer<TKey> this[TKey key]=> new ValueCopyer<TKey>(this, key);

        IValueWriter IDataWriter<TKey>.this[TKey key]=> this[key];

        IValueReader IDataReader<TKey>.this[TKey key]=> this[key];

        public IEnumerable<TKey> Keys => content.Keys;

        public int Count => content.Count;

        object IDirectContent.DirectContent
        {
            get
            {
                return content;
            }
            set
            {
                content = (T)value;
            }
        }

        public object ReferenceToken
        {
            get
            {
                return content;
            }
        }

        IValueRW IDataRW<TKey>.this[TKey key] => this[key];

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
                if (typeof(T).IsAssignableFrom(typeof(Dictionary<TKey, TValue>)))
                {
                    goto Dictionary;
                }

                // TODO: Others Interface initialize.
            }

            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<Dictionary<TKey, TValue>>.Int64TypeHandle)
            {
                goto Dictionary;
            }

            // TODO: Capacity
            content = Activator.CreateInstance<T>();

            return;

        Dictionary:

            content = (T)(object)new Dictionary<TKey, TValue>(capacity);
        }

        public void OnReadAll(IDataWriter<TKey> dataWriter)
        {
            foreach (var item in content)
            {
                ValueInterface<TValue>.WriteValue(dataWriter[item.Key], item.Value);
            }
        }

        public void OnReadValue(TKey key, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.WriteValue(valueWriter, content[key]);
        }

        public void OnWriteValue(TKey key, IValueReader valueReader)
        {
            content[key] = ValueInterface<TValue>.ReadValue(valueReader);
        }

        public void OnReadAll(IDataWriter<TKey> dataWriter, IValueFilter<TKey> valueFilter)
        {
            var valueInfo = new ValueFilterInfo<TKey>();

            foreach (var item in content)
            {
                ValueInterface<TValue>.WriteValue(valueInfo.ValueCopyer, item.Value);

                valueInfo.Key = item.Key;
                valueInfo.Type = typeof(TValue);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        public void OnWriteAll(IDataReader<TKey> dataReader)
        {
            var buckets = new KeyValuePair<TKey, TValue>[content.Count];

            var index = 0;

            foreach (var item in content.Keys)
            {
                buckets[index] = new KeyValuePair<TKey, TValue>(item, ValueInterface<TValue>.ReadValue(dataReader[item]));

                ++index;
            }

            content.Clear();

            foreach (var item in buckets)
            {
                content.Add(item);
            }
        }
    }

    internal sealed class DictionaryRW<T> : IDataRW<object>, IDirectContent where T : IDictionary
    {
        public const int DefaultCapacity = 3;

        internal T content;

        public T Content => content;

        public ValueCopyer<object> this[object key]=> new ValueCopyer<object>(this, key);

        IValueRW IDataRW<object>.this[object key] => this[key];

        IValueWriter IDataWriter<object>.this[object key]=> this[key];

        IValueReader IDataReader<object>.this[object key]=> this[key];

        public IEnumerable<object> Keys
        {
            get
            {
                // TODO:
                return (IEnumerable<object>)content.Keys;
            }
        }

        public int Count
        {
            get
            {
                return content.Count;
            }
        }

        object IDirectContent.DirectContent
        {
            get
            {
                return content;
            }
            set
            {
                content = (T)value;
            }
        }

        public object ReferenceToken
        {
            get
            {
                return content;
            }
        }

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
                if (typeof(T).IsAssignableFrom(typeof(Hashtable)))
                {
                    goto Hashtable;
                }

                // TODO: Others Interface initialize.
            }

            if (TypeInfo<T>.Int64TypeHandle == TypeInfo<Hashtable>.Int64TypeHandle)
            {
                goto Hashtable;
            }

            // TODO: Capacity
            content = Activator.CreateInstance<T>();

            return;

            Hashtable:

            content = (T)(object)new Hashtable(capacity);
        }

        public void OnReadAll(IDataWriter<object> dataWriter)
        {
            foreach (DictionaryEntry item in content)
            {
                ValueInterface.WriteValue(dataWriter[item.Key], item.Value);
            }
        }

        public void OnReadValue(object key, IValueWriter valueWriter)
        {
            ValueInterface.WriteValue(valueWriter, content[key]);
        }

        public void OnWriteValue(object key, IValueReader valueReader)
        {
            content[key] = ValueInterface<object>.ReadValue(valueReader);
        }

        public void OnReadAll(IDataWriter<object> dataWriter, IValueFilter<object> valueFilter)
        {
            var valueInfo = new ValueFilterInfo<object>();

            foreach (DictionaryEntry item in content)
            {
                ValueInterface.WriteValue(valueInfo.ValueCopyer, item.Value);

                valueInfo.Key = item.Key;
                valueInfo.Type = typeof(object);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        public void OnWriteAll(IDataReader<object> dataReader)
        {
            var buckets = new KeyValuePair<object, object>[content.Count];

            var index = 0;

            foreach (var item in content.Keys)
            {
                buckets[index] = new KeyValuePair<object, object>(item, ValueInterface<object>.ReadValue(dataReader[item]));

                ++index;
            }

            content.Clear();

            foreach (var item in buckets)
            {
                content.Add(item.Key, item.Value);
            }
        }
    }

    internal sealed class DictionaryInterfaceMaper : IValueInterfaceMaper
    {
        public IValueInterface<T> TryMap<T>()
        {
            var type = typeof(T);

            var item = type;

            var interfaces = type.GetInterfaces();

            var index = 0;

            Loop:

            if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            {
                var genericArguments = item.GetGenericArguments();

                return (IValueInterface<T>)Activator.CreateInstance(typeof(DictionaryInterface<,,>).MakeGenericType(type, genericArguments[0], genericArguments[1]));
            }

            if (index < interfaces.Length)
            {
                item = interfaces[index];

                ++index;

                goto Loop;
            }

            if (typeof(IDictionary).IsAssignableFrom(type))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(DictionaryInterface<>).MakeGenericType(type));
            }

            return null;
        }
    }

    internal sealed class DictionaryInterface<T, TKey, TValue> : IValueInterface<T> where T : IDictionary<TKey, TValue>
    {
        public T ReadValue(IValueReader valueReader)
        {
            var dictionaryRW = new DictionaryRW<T, TKey, TValue>();

            if (valueReader is IValueFiller<TKey> tFiller)
            {
                tFiller.FillValue(dictionaryRW);
            }

            valueReader.ReadObject(dictionaryRW.As<string>());

            return dictionaryRW.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var dictionaryRW = new DictionaryRW<T, TKey, TValue>();

            dictionaryRW.Initialize(value);

            valueWriter.WriteObject(dictionaryRW.As<string>());
        }
    }

    internal sealed class DictionaryInterface<T> : IValueInterface<T> where T : IDictionary
    {
        public T ReadValue(IValueReader valueReader)
        {
            var dictionaryRW = new DictionaryRW<T>();

            if (valueReader is IValueFiller<object> tFiller)
            {
                tFiller.FillValue(dictionaryRW);
            }
            else
            {
                valueReader.ReadObject(dictionaryRW.As<string>());
            }

            return dictionaryRW.Content;
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var dictionaryRW = new DictionaryRW<T>();

            dictionaryRW.Initialize(value);

            valueWriter.WriteObject(dictionaryRW.As<string>());
        }
    }
}