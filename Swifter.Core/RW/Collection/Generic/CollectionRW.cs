using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    internal sealed class CollectionRW<T, TValue> : IArrayRW where T : ICollection<TValue?>
    {
        public const int DefaultCapacity = 3;

        T? content;

        public IValueRW this[int key] => throw new NotSupportedException();

        public int Count => content?.Count ?? -1;

        public T? Content
        {
            get => content;
            set => content = value;
        }

        public Type ContentType => typeof(T);

        public Type ValueType => typeof(TValue);

        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        [MemberNotNull(nameof(content))]
        public void Initialize(int capacity)
        {
            if (typeof(T).IsAssignableFrom(typeof(List<TValue?>)))
            {
                content = TypeHelper.As<List<TValue?>, T>(new List<TValue?>(capacity));
            }
#if NET35_OR_GREATER || NET || NETCOREAPP || NETSTANDARD
            else if (typeof(T).IsAssignableFrom(typeof(HashSet<TValue?>)))
            {
                content = TypeHelper.As<HashSet<TValue?>, T>(new HashSet<TValue?>(
#if NET48_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                    capacity
#endif
                    ));
            }
#endif
            else
            {
                // TODO: Capacity
                content = Activator.CreateInstance<T>();

#if NET48_OR_GREATER || NET5_0_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER

                if (content is HashSet<TValue?> hashSet)
                {
                    hashSet.EnsureCapacity(capacity);
                }
#endif
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            var index = 0;

            if (stopToken.CanBeStopped)
            {
                IEnumerator<TValue?>? enumerator;

                if (stopToken.PopState() is ValueTuple<IEnumerator<TValue?>, int> state)
                {
                    enumerator = state.Item1;
                    index = state.Item2;
                }
                else
                {
                    enumerator = content.GetEnumerator();
                }

                try
                {
                    while (enumerator.MoveNext())
                    {
                        ValueInterface<TValue>.WriteValue(dataWriter[index], enumerator.Current);

                        ++index;

                        if (stopToken.IsStopRequested)
                        {
                            stopToken.SetState((enumerator, index));

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
                foreach (var item in content)
                {
                    ValueInterface<TValue>.WriteValue(dataWriter[index], item);

                    ++index;
                }
            }

        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            if (content is IList<TValue?> list)
            {
                ValueInterface<TValue>.WriteValue(valueWriter, list[key]);

                return;
            }

            throw new NotSupportedException();
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            var value = ValueInterface<TValue>.ReadValue(valueReader);

            if (key == Count)
            {
                content.Add(value);
            }
            else if (content is IList<TValue?> list)
            {
                list[key] = value;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void OnWriteAll(IDataReader<int> dataReader, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            if (content is IList<TValue?> list)
            {
                int length = Count;
                int i = 0;

                if (stopToken.CanBeStopped)
                {
                    if (stopToken.PopState() is int index)
                    {
                        i = index;
                    }

                    for (; i < length; i++)
                    {
                        if (stopToken.IsStopRequested)
                        {
                            stopToken.SetState(i);

                            return;
                        }

                        list[i] = ValueInterface<TValue>.ReadValue(dataReader[i]);
                    }
                }
                else
                {
                    for (; i < length; i++)
                    {
                        list[i] = ValueInterface<TValue>.ReadValue(dataReader[i]);
                    }
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        object? IDataRW.Content { get => Content; set => Content = (T?)value; }

        object? IDataReader.Content { get => Content; set => Content = (T?)value; }

        object? IDataWriter.Content { get => Content; set => Content = (T?)value; }
    }
}