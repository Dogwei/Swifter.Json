
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    internal sealed class CollectionRW<T> : IArrayRW where T : ICollection
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

        public Type ValueType => typeof(object);

        public void Initialize()
        {
            Initialize(DefaultCapacity);
        }

        public void Initialize(int capacity)
        {
            if (typeof(T).IsAssignableFrom(typeof(ArrayList)))
            {
                Unsafe.As<T?, ArrayList?>(ref content) = new ArrayList(capacity);
            }
            else
            {
                // TODO: Capacity
                content = Activator.CreateInstance<T>();
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, RWStopToken stopToken = default)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            int index = 0;

            if (stopToken.CanBeStopped)
            {
                IEnumerator enumerator;

                if (stopToken.PopState() is ValueTuple<IEnumerator, int> state)
                {
                    enumerator = state.Item1;
                    index = state.Item2;
                }
                else
                {
                    enumerator = content.GetEnumerator();
                }

                while (enumerator.MoveNext())
                {
                    ValueInterface.WriteValue(dataWriter[index], enumerator.Current);

                    ++index;

                    if (stopToken.IsStopRequested)
                    {
                        stopToken.SetState((enumerator, index));

                        return;
                    }
                }
            }
            else
            {
                foreach (var item in content)
                {
                    ValueInterface.WriteValue(dataWriter[index], item);

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

            if (content is IList list)
            {
                ValueInterface.WriteValue(valueWriter, list[key]);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public void OnWriteValue(int key, IValueReader valueReader)
        {
            if (content is null)
            {
                throw new NullReferenceException(nameof(Content));
            }

            if (content is IList list)
            {
                var value = ValueInterface<object>.ReadValue(valueReader);

                if (key == Count)
                {
                    list.Add(value);
                }
                else
                {
                    list[key] = value;
                }
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

            if (content is IList list)
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

                        list[i] = ValueInterface<object>.ReadValue(dataReader[i]);
                    }
                }
                else
                {
                    for (; i < length; i++)
                    {
                        list[i] = ValueInterface<object>.ReadValue(dataReader[i]);
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