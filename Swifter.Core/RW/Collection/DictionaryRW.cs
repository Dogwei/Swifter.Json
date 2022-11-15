using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    internal sealed class DictionaryRW<T> : IDataRW<object> where T : IDictionary
    {
        public const int DefaultCapacity = 3;

        T? content;

        public IValueRW this[object key] => new ValueRW(this, key);

        public int Count => content?.Count ?? -1;

        public T? Content
        {
            get => content;
            set => content = value;
        }
        public Type ContentType => typeof(T);

        public Type ValueType => typeof(object);

        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        public void Initialize(int capacity)
        {
            if (typeof(T).IsAssignableFrom(typeof(Hashtable)))
            {
                Unsafe.As<T?, Hashtable?>(ref content) = new Hashtable(capacity);
            }
            else
            {
                // TODO: Capacity
                content = Activator.CreateInstance<T>();
            }
        }

        public void OnReadAll(IDataWriter<object> dataWriter, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(content));
            }

            IDictionaryEnumerator enumerator;

            if (stopToken.CanBeStopped)
            {
                if (stopToken.PopState() is IDictionaryEnumerator state)
                {
                    enumerator = state;
                }
                else
                {
                    enumerator = content.GetEnumerator();
                }

                while (enumerator.MoveNext())
                {
                    ValueInterface.WriteValue(dataWriter[enumerator.Key], enumerator.Value);

                    if (stopToken.IsStopRequested)
                    {
                        stopToken.SetState(enumerator);

                        return;
                    }
                }
            }
            else
            {
                for (enumerator = content.GetEnumerator(); enumerator.MoveNext();)
                {
                    ValueInterface.WriteValue(dataWriter[enumerator.Key], enumerator.Value);
                }
            }
        }

        public void OnReadValue(object key, IValueWriter valueWriter)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(content));
            }

            ValueInterface.WriteValue(valueWriter, content[key]);
        }

        public void OnWriteValue(object key, IValueReader valueReader)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(content));
            }

            content[key] = ValueInterface<object>.ReadValue(valueReader);
        }

        public void OnWriteAll(IDataReader<object> dataReader, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(content));
            }

            var canBeStopped = stopToken.CanBeStopped;

            object[] keys;
            int i = 0;

            if (canBeStopped && stopToken.PopState() is ValueTuple<object[], int> state)
            {
                keys = state.Item1;
                i = state.Item2;
            }
            else
            {
                var count = content.Count;

                if (count is 0)
                {
                    return;
                }

#if ARRAY_POOL
                keys = ArrayPool<object>.Shared.Rent(count);
#else
                keys = new object[count];
#endif
                content.Keys.CopyTo(keys, 0);
            }

            if (canBeStopped)
            {
                for (; i < keys.Length; i++)
                {
                    if (canBeStopped && stopToken.IsStopRequested)
                    {
                        stopToken.SetState((keys, i));

                        return;
                    }

                    content[keys[i]] = ValueInterface<object>.ReadValue(dataReader[keys[i]]);
                }
            }
            else
            {
                for (; i < keys.Length; i++)
                {
                    content[keys[i]] = ValueInterface<object>.ReadValue(dataReader[keys[i]]);
                }
            }

#if ARRAY_POOL
            ArrayPool<object>.Shared.Return(keys, true);
#endif
        }

        IValueWriter IDataWriter<object>.this[object key] => this[key];

        IValueReader IDataReader<object>.this[object key] => this[key];

        object? IDataRW.Content { get => Content; set => Content = (T?)value; }

        object? IDataReader.Content { get => Content; set => Content = (T?)value; }

        object? IDataWriter.Content { get => Content; set => Content = (T?)value; }

        sealed class ValueRW : BaseDirectRW
        {
            readonly DictionaryRW<T> DictionaryRW;
            readonly object Key;

            public ValueRW(DictionaryRW<T> dictionaryRW, object key)
            {
                DictionaryRW = dictionaryRW;
                Key = key;
            }

            public override Type ValueType => typeof(object);

            public override object? DirectRead()
            {
                if (DictionaryRW.content is null)
                {
                    throw new NullReferenceException(nameof(Content));
                }

                return DictionaryRW.content[Key];
            }

            public override void DirectWrite(object? value)
            {
                if (DictionaryRW.content is null)
                {
                    throw new NullReferenceException(nameof(Content));
                }

                DictionaryRW.content[Key] = value;
            }
        }
    }
}