using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Swifter.RW
{
    internal sealed class ListInterface<T, TValue> : IValueInterface<T> where T : IList<TValue?>
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
                var listRW = new ListRW<T, TValue>();

                valueReader.ReadArray(listRW);

                return listRW.Content;
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
                valueWriter.WriteArray(new ListRW<T, TValue> { Content = value });
            }
        }
    }
}