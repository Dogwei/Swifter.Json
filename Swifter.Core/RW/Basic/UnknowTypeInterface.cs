﻿
using Swifter.Tools;

using System;

namespace Swifter.RW
{
    internal sealed class UnknowTypeInterface<T> : IValueInterface<T>
    {
        public T? ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> reader)
            {
                return reader.ReadValue();
            }

            var value = valueReader.DirectRead();

            if (value is T ret)
            {
                return ret;
            }

            if (value is null)
            {
                return default;
            }

            return XConvert.Convert<T>(value);
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
            else if (value is IFormattable)
            {
                valueWriter.DirectWrite(value);
            }
            else if (value is string str)
            {
                valueWriter.WriteString(str);
            }
            else if (value.GetType() != typeof(T))
            {
                ValueInterface.GetInterface(value).Write(valueWriter, value);
            }
            else
            {
                valueWriter.DirectWrite(value);
            }
        }
    }
}