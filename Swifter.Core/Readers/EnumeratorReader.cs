using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Swifter.Readers
{
    internal sealed class EnumeratorReader<T, TValue> : IDataReader<int>, IDirectContent, IInitialize<T> where T : IEnumerator<TValue>
    {
        internal T content;
        
        public IValueReader this[int key] => throw new NotSupportedException();

        public IEnumerable<int> Keys => throw new NotSupportedException();

        public int Count => 0;

        public object ReferenceToken => content;

        public T Content => content;

        public object DirectContent { get => content; set => content = (T)value; }

        public void Initialize(T obj)
        {
            content = obj;
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            int index = 0;

            while (content.MoveNext())
            {
                ValueInterface<TValue>.WriteValue(dataWriter[index], content.Current);

                ++index;
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int index = 0;

            var valueInfo = new ValueFilterInfo<int>();

            while (content.MoveNext())
            {
                ValueInterface<TValue>.WriteValue(valueInfo.ValueCopyer, content.Current);

                valueInfo.Key = index;
                valueInfo.Type = typeof(TValue);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[index]);
                }

                ++index;
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            throw new NotSupportedException();
        }
    }

    internal sealed class EnumeratorReader<T> : IDataReader<int>, IDirectContent, IInitialize<T> where T : IEnumerator
    {
        internal T content;

        public IValueReader this[int key] => throw new NotSupportedException();

        public IEnumerable<int> Keys => throw new NotSupportedException();

        public int Count => 0;

        public object ReferenceToken => content;

        public T Content => content;

        public object DirectContent { get => content; set => content = (T)value; }

        public void Initialize(T obj)
        {
            content = obj;
        }

        public void OnReadAll(IDataWriter<int> dataWriter)
        {
            int index = 0;

            while (content.MoveNext())
            {
                ValueInterface.WriteValue(dataWriter[index], content.Current);

                ++index;
            }
        }

        public void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int index = 0;

            var valueInfo = new ValueFilterInfo<int>();

            while (content.MoveNext())
            {
                ValueInterface.WriteValue(valueInfo.ValueCopyer, content.Current);

                valueInfo.Key = index;
                valueInfo.Type = typeof(object);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[index]);
                }

                ++index;
            }
        }

        public void OnReadValue(int key, IValueWriter valueWriter)
        {
            throw new NotSupportedException();
        }
    }

    internal sealed class EnumeratorInterface<T, TValue> : IValueInterface<T> where T : IEnumerator<TValue>
    {
        public T ReadValue(IValueReader valueReader)
        {
            throw new NotSupportedException();
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var enumeratorReader = new EnumeratorReader<T, TValue>();

            enumeratorReader.Initialize(value);

            valueWriter.WriteArray(enumeratorReader);
        }
    }

    internal sealed class EnumeratorInterface<T> : IValueInterface<T> where T : IEnumerator
    {
        public T ReadValue(IValueReader valueReader)
        {
            throw new NotSupportedException();
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var enumeratorReader = new EnumeratorReader<T>();

            enumeratorReader.Initialize(value);

            valueWriter.WriteArray(enumeratorReader);
        }
    }

    internal sealed class EnumerableInterface<T, TValue> : IValueInterface<T> where T : IEnumerable<TValue>
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (typeof(T).IsAssignableFrom(typeof(TValue[])))
            {
                return (T)(object)ValueInterface<TValue[]>.ReadValue(valueReader);
            }
            else if (typeof(T).IsAssignableFrom(typeof(List<TValue>)))
            {
                return (T)(object)ValueInterface<List<TValue>>.ReadValue(valueReader);
            }

            throw new NotSupportedException();
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            var enumerator = value?.GetEnumerator();

            if (enumerator == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var enumeratorReader = new EnumeratorReader<IEnumerator<TValue>, TValue>();

            enumeratorReader.Initialize(enumerator);

            valueWriter.WriteArray(enumeratorReader);
        }
    }

    internal sealed class EnumerableInterface<T> : IValueInterface<T> where T : IEnumerable
    {
        public T ReadValue(IValueReader valueReader)
        {
            throw new NotSupportedException();
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            var enumeratorReader = new EnumeratorReader<IEnumerator>();

            enumeratorReader.Initialize(value.GetEnumerator());

            valueWriter.WriteArray(enumeratorReader);
        }
    }

    internal sealed class EnumerableInterfaceMaper : IValueInterfaceMaper
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

            if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var genericArguments = item.GetGenericArguments();

                return (IValueInterface<T>)Activator.CreateInstance(typeof(EnumerableInterface<,>).MakeGenericType(type, genericArguments[0]));
            }

            if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IEnumerator<>))
            {
                var genericArguments = item.GetGenericArguments();

                return (IValueInterface<T>)Activator.CreateInstance(typeof(EnumeratorInterface<,>).MakeGenericType(type, genericArguments[0]));
            }

            if (index < interfaces.Length)
            {
                item = interfaces[index];

                ++index;

                goto Loop;
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(EnumerableInterface<>).MakeGenericType(type));
            }

            if (typeof(IEnumerator).IsAssignableFrom(type))
            {
                return (IValueInterface<T>)Activator.CreateInstance(typeof(EnumeratorInterface<>).MakeGenericType(type));
            }

            return null;
        }
    }
}