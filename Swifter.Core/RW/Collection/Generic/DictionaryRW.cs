using Swifter.Tools;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    internal sealed class DictionaryRW<T, TKey, TValue> : IDataRW<TKey> where T : IDictionary<TKey, TValue?> where TKey : notnull
    {
        public const int DefaultCapacity = 3;

        static readonly bool IsAssignableFromDictionary = typeof(T).IsAssignableFrom(typeof(Dictionary<TKey, TValue?>));

        static ArrayAppendingInfo appendingInfo = new ArrayAppendingInfo() { MostClosestMeanCommonlyUsedLength = DefaultCapacity };

        T? content;

        public IValueRW this[TKey key] => new ValueRW(this, key);

        public int Count => content?.Count ?? -1;

        public T? Content
        {
            get
            {
                if (content is not null)
                {
                    appendingInfo.AddUsedLength(content.Count);
                }

                return content;
            }
            set => content = value;
        }

        public Type ContentType => typeof(T);

        public Type ValueType => typeof(TValue);

        [MemberNotNull(nameof(content))]
        public void Initialize()
        {
            Initialize(appendingInfo.MostClosestMeanCommonlyUsedLength);
        }

        [MemberNotNull(nameof(content))]
        public void Initialize(int capacity)
        {
            if (IsAssignableFromDictionary)
            {
                content = TypeHelper.As<Dictionary<TKey, TValue?>, T>(new Dictionary<TKey, TValue?>(capacity));
            }
            else
            {
                content = Activator.CreateInstance<T>();

#if NETCOREAPP2_1_OR_GREATER || NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                if (content is Dictionary<TKey, TValue?> dictionary)
                {
                    dictionary.EnsureCapacity(capacity);
                }
#endif
                // TODO: Other Capacity
            }
        }

        public void OnReadAll(IDataWriter<TKey> dataWriter, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            if (stopToken.CanBeStopped)
            {
                IEnumerator<KeyValuePair<TKey, TValue?>>? enumerator;

                if (stopToken.PopState() is IEnumerator<KeyValuePair<TKey, TValue?>> state)
                {
                    enumerator = state;
                }
                else
                {
                    enumerator = content.GetEnumerator();
                }

                try
                {
                    while (enumerator.MoveNext())
                    {
                        var item = enumerator.Current;

                        ValueInterface.WriteValue<TValue>(dataWriter[item.Key], item.Value);

                        if (stopToken.IsStopRequested)
                        {
                            stopToken.SetState(enumerator);

                            enumerator = null;

                            return;
                        }
                    }
                }
                finally
                {
                    enumerator?.Dispose();
                }
            }
            else
            {
                if (content is Dictionary<TKey, TValue?> dictionary)
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
        }

        public void OnReadValue(TKey key, IValueWriter valueWriter)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            ValueInterface<TValue>.WriteValue(valueWriter, content[key]);
        }

        public void OnWriteValue(TKey key, IValueReader valueReader)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            content[key] = ValueInterface<TValue>.ReadValue(valueReader)!;
        }

        public void OnWriteAll(IDataReader<TKey> dataReader, RWStopToken stopToken)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            var canBeStopped = stopToken.CanBeStopped;

            TKey[] keys;
            int i = 0;

            if (canBeStopped && stopToken.PopState() is ValueTuple<TKey[], int> state)
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
                keys = ArrayPool<TKey>.Shared.Rent(count);
#else
                keys = new TKey[count];
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

                    content[keys[i]] = ValueInterface<TValue>.ReadValue(dataReader[keys[i]]);
                }
            }
            else
            {
                for (; i < keys.Length; i++)
                {
                    content[keys[i]] = ValueInterface<TValue>.ReadValue(dataReader[keys[i]]);
                }
            }

#if ARRAY_POOL
            ArrayPool<TKey>.Shared.Return(keys, true);
#endif
        }

        IValueWriter IDataWriter<TKey>.this[TKey key] => this[key];

        IValueReader IDataReader<TKey>.this[TKey key] => this[key];

        object? IDataRW.Content { get => Content; set => Content = (T?)value; }

        object? IDataReader.Content { get => Content; set => Content = (T?)value; }

        object? IDataWriter.Content { get => Content; set => Content = (T?)value; }

        sealed class ValueRW : BaseGenericRW<TValue>, IValueRW<TValue>
        {
            public readonly DictionaryRW<T, TKey, TValue> DictionaryRW;
            public readonly TKey Key;

            public ValueRW(DictionaryRW<T, TKey, TValue> dictionaryRW, TKey key)
            {
                DictionaryRW = dictionaryRW;
                Key = key;
            }

            public override TValue? ReadValue()
            {
                if (DictionaryRW.content is null)
                {
                    throw new NullReferenceException(nameof(Content));
                }

                return DictionaryRW.content[Key];
            }

            public override void WriteValue(TValue? value)
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