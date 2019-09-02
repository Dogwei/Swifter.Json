

using System;
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

        public ValueCopyer<TKey> this[TKey key] => new ValueCopyer<TKey>(this, key);

        IValueWriter IDataWriter<TKey>.this[TKey key] => this[key];

        IValueReader IDataReader<TKey>.this[TKey key] => this[key];

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
            if (typeof(T) == typeof(Dictionary<TKey, TValue>) || typeof(T).IsAssignableFrom(typeof(Dictionary<TKey, TValue>)))
            {
                content = (T)(object)new Dictionary<TKey, TValue>(capacity);
            }
            else
            {
                content = Activator.CreateInstance<T>();
            }
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
}