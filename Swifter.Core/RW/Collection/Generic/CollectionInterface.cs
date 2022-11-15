

using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.RW
{
    internal sealed class CollectionInterface<T, TValue> : IValueInterface<T> where T : ICollection<TValue?>
    {
        static readonly bool ReadByXConvertFromList = typeof(T).GetConstructor(Type.EmptyTypes) is null && XConvert.IsEffectiveConvert(typeof(List<TValue?>), typeof(T));

        public T? ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> reader)
            {
                return reader.ReadValue();
            }

            if (ReadByXConvertFromList)
            {
                var listRW = new ListRW<List<TValue?>, TValue>();

                valueReader.ReadArray(listRW);

                return XConvert.Convert<List<TValue?>, T>(listRW.Content);
            }
            else
            {
                var collectionRW = new CollectionRW<T, TValue>();

                valueReader.ReadArray(collectionRW);

                return collectionRW.Content;
            }
        }

        public void WriteValue(IValueWriter valueWriter, T? value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (valueWriter is IValueWriter<T> writer)
            {
                writer.WriteValue(value);
            }
            else
            {
                valueWriter.WriteArray(new CollectionRW<T, TValue> { Content = value });
            }
        }
    }
}