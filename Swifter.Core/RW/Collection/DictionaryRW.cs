

using System;
using System.Collections;
using System.Collections.Generic;

namespace Swifter.RW
{

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
            if (typeof(T) == typeof(Hashtable) || typeof(T).IsAssignableFrom(typeof(Hashtable)))
            {
                content = (T)(object)new Hashtable(capacity);
            }
            else
            {
                // TODO: Capacity
                content = Activator.CreateInstance<T>();
            }
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
}