using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Swifter.RW
{

    internal sealed class DictionaryRW<T> : IDataRW<object> where T : IDictionary
    {
        public const int DefaultCapacity = 3;

        internal T content;

        public IValueRW this[object key] => new ValueCopyer<object>(this, key);

        IValueWriter IDataWriter<object>.this[object key]=> this[key];

        IValueReader IDataReader<object>.this[object key]=> this[key];

        public IEnumerable<object> Keys => content.Keys.Cast<object>();

        public int Count => content.Count;

        public object Content
        {
            get => content;
            set => content = (T)value;
        }

        public Type ContentType => typeof(T);

        public void Initialize()
        {
            Initialize(DefaultCapacity);
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

        public void OnWriteAll(IDataReader<object> dataReader)
        {
            // TODO: Fast
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