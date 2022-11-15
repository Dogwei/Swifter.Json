

using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.RW
{
    internal sealed class DictionaryInterface<T, TKey, TValue> : IValueInterface<T> where T : IDictionary<TKey, TValue?> where TKey : notnull
    {
        static readonly bool ReadByXConvertFromList = typeof(T).GetConstructor(Type.EmptyTypes) is null && XConvert.IsEffectiveConvert(typeof(Dictionary<TKey, TValue?>), typeof(T));

        public T? ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> reader)
            {
                return reader.ReadValue();
            }

            if (ReadByXConvertFromList)
            {
                var dictionaryRW = new DictionaryRW<Dictionary<TKey, TValue?>, TKey, TValue>();

                valueReader.ReadObject(dictionaryRW.As<string>());

                return XConvert.Convert<Dictionary<TKey, TValue?>, T>(dictionaryRW.Content);
            }
            else
            {
                var dictionaryRW = new DictionaryRW<T, TKey, TValue>();

                valueReader.ReadObject(dictionaryRW.As<string>());

                return dictionaryRW.Content;
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
                valueWriter.WriteObject(new DictionaryRW<T, TKey, TValue> { Content = value }.As<string>());
            }
        }
    }
}