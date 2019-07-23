using Swifter.Readers;
using Swifter.Writers;
using System;

namespace Swifter.RW
{
    internal sealed class DataReaderInterface<T, TKey> : IValueInterface<T> where T : IDataReader<TKey>
    {
        static readonly bool IsArray;

        static DataReaderInterface()
        {
            var tKey = typeof(TKey);

            switch (Type.GetTypeCode(tKey))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    IsArray = true && !tKey.IsEnum;
                    break;
            }
        }

        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }

            var value = valueReader.DirectRead();

            if (value is T tValue)
            {
                return tValue;
            }

            var reader = RWHelper.CreateReader(value);

            if (reader is T tResult)
            {
                return tResult;
            }

            throw new NotSupportedException($"Cannot read a '{typeof(T).Name}', It is a data {"reader"}.");
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (valueWriter is IValueWriter<T> tWriter)
            {
                tWriter.WriteValue(value);

                return;
            }

            if (valueWriter is IValueWriter<IDataReader> iWriter)
            {
                iWriter.WriteValue(value);

                return;
            }

            if (IsArray)
            {
                valueWriter.WriteArray(value.As<int>());
            }
            else
            {
                valueWriter.WriteObject(value.As<string>());
            }
        }
    }
}