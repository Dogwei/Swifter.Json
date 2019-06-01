using Swifter.Readers;
using Swifter.Writers;
using System;

namespace Swifter.RW
{
    internal sealed class OneRankArrayRW<T> : ArrayRW<T[]>
    {
        public override void Initialize(int capacity)
        {
            if (content == null || content.Length < capacity)
            {
                content = new T[capacity];
            }

            count = 0;
        }

        public override void Initialize(T[] content)
        {
            this.content = content;

            count = content.Length;
        }

        public override void OnWriteValue(int key, IValueReader valueReader)
        {
            if (key >= content.Length)
            {
                Array.Resize(ref content, (key * 3) + 1);
            }

            count = Math.Max(key + 1, count);

            content[key] = ValueInterface<T>.ReadValue(valueReader);
        }

        public override T[] Content
        {
            get
            {
                if (content == null)
                {
                    return null;
                }

                if (count != content.Length)
                {
                    Array.Resize(ref content, count);
                }

                return content;
            }
        }

        public override void OnReadValue(int key, IValueWriter valueWriter)
        {
            ValueInterface<T>.WriteValue(valueWriter, content[key]);
        }

        public override void OnReadAll(IDataWriter<int> dataWriter)
        {
            int length = Count;

            for (int i = 0; i < length; ++i)
            {
                ValueInterface<T>.WriteValue(dataWriter[i], content[i]);
            }
        }

        public override void OnReadAll(IDataWriter<int> dataWriter, IValueFilter<int> valueFilter)
        {
            int length = Count;

            var valueInfo = new ValueFilterInfo<int>();

            for (int i = 0; i < length; ++i)
            {
                var value = content[i];

                ValueInterface<T>.WriteValue(valueInfo.ValueCopyer, value);

                valueInfo.Key = i;
                valueInfo.Type = typeof(T);

                if (valueFilter.Filter(valueInfo))
                {
                    valueInfo.ValueCopyer.WriteTo(dataWriter[valueInfo.Key]);
                }
            }
        }

        public override void OnWriteAll(IDataReader<int> dataReader)
        {
            var length = Count;

            for (int i = 0; i < length; i++)
            {
                OnWriteValue(i, dataReader[i]);
            }
        }
    }
}