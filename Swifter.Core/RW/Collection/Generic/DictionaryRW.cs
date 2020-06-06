using System;
using System.Collections.Generic;

namespace Swifter.RW
{
    internal sealed class DictionaryRW<T, TKey, TValue> : IDataRW<TKey> where T : IDictionary<TKey, TValue>
    {
        public const int DefaultCapacity = 3;

        static readonly bool IsAssignableFromDictionary = typeof(T).IsAssignableFrom(typeof(Dictionary<TKey, TValue>));

        internal T content;

        public ValueCopyer<TKey> this[TKey key] => new ValueCopyer<TKey>(this, key);

        IValueWriter IDataWriter<TKey>.this[TKey key] => this[key];

        IValueReader IDataReader<TKey>.this[TKey key] => this[key];

        public IEnumerable<TKey> Keys => content.Keys;

        public int Count => content.Count;

        public object Content
        {
            get => content;
            set => content = (T)value;
        }

        public Type ContentType => typeof(T);

        IValueRW IDataRW<TKey>.this[TKey key] => this[key];

        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        public void Initialize(int capacity)
        {
            if (IsAssignableFromDictionary)
            {
                Underlying.As<T, Dictionary<TKey, TValue>>(ref content) = new Dictionary<TKey, TValue>(capacity);
            }
            else
            {
                content = Activator.CreateInstance<T>();
            }
        }

        public void OnReadAll(IDataWriter<TKey> dataWriter)
        {
            if (content is Dictionary<TKey, TValue> dictionary)
            {
                foreach (var item in dictionary)
                {
                    ValueInterface<TValue>.WriteValue(dataWriter[item.Key], item.Value);
                }
            }
            else
            {
                foreach (var item in content)
                {
                    ValueInterface<TValue>.WriteValue(dataWriter[item.Key], item.Value);
                }
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

        public void OnWriteAll(IDataReader<TKey> dataReader)
        {
            // TODO: Fast

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