using Microsoft.FSharp.Core;
using Swifter.RW;
using System;

namespace Swifter.FSharpExtensions
{
    public sealed class FSharpOptionInterface<T> : IValueInterface<FSharpOption<T>>
    {
        static readonly ValueInterface NullableInterface;

        static FSharpOptionInterface()
        {
            if (default(T) != null)
            {
                NullableInterface = ValueInterface.GetInterface(typeof(Nullable<>).MakeGenericType(typeof(T)));
            }
        }

        public FSharpOption<T> ReadValue(IValueReader valueReader)
        {
            if (default(T) != null)
            {
                var obj = NullableInterface.Read(valueReader);

                if (obj is null)
                {
                    return FSharpOption<T>.None;
                }

                return (T)obj;
            }
            else
            {
                var val = ValueInterface<T>.ReadValue(valueReader);

                if (val == null)
                {
                    return FSharpOption<T>.None;
                }

                return val;
            }
        }

        public void WriteValue(IValueWriter valueWriter, FSharpOption<T> value)
        {
            if (value is null)
            {
                valueWriter.DirectWrite(null);
            }
            else if (value == FSharpOption<T>.None)
            {
                valueWriter.DirectWrite(null);
            }
            else
            {
                ValueInterface.WriteValue(valueWriter, value.Value);
            }
        }
    }
}