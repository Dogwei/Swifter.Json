﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.RW
{
    sealed class TupleInterface : IValueInterface<ValueTuple>
    {
        public ValueTuple ReadValue(IValueReader valueReader)
        {
            return default;
        }

        public void WriteValue(IValueWriter valueWriter, ValueTuple value)
        {
        }
    }

    sealed class TupleInterface<T1> : IValueInterface<ValueTuple<T1?>>
    {
        public ValueTuple<T1?> ReadValue(IValueReader valueReader)
        {
            return new ValueTuple<T1?>(ValueInterface<T1>.ReadValue(valueReader));
        }

        public void WriteValue(IValueWriter valueWriter, ValueTuple<T1?> value)
        {
            ValueInterface.WriteValue(valueWriter, value.Item1);
        }
    }

    sealed class TupleInterface<T1, T2> : IValueInterface<ValueTuple<T1?, T2?>>
    {
        
        public ValueTuple<T1?, T2?> ReadValue(IValueReader valueReader)
        {
            return (
                ValueInterface<T1>.ReadValue(valueReader),
                ValueInterface<T2>.ReadValue(valueReader)
                );
        }

        public void WriteValue(IValueWriter valueWriter, ValueTuple<T1?, T2?> value)
        {
            ValueInterface.WriteValue(valueWriter, value.Item1);
            ValueInterface.WriteValue(valueWriter, value.Item2);
        }
    }

    sealed class TupleInterface<T1, T2, T3> : IValueInterface<ValueTuple<T1?, T2?, T3?>>
    {
        public ValueTuple<T1?, T2?, T3?> ReadValue(IValueReader valueReader)
        {
            return (
                ValueInterface<T1>.ReadValue(valueReader),
                ValueInterface<T2>.ReadValue(valueReader),
                ValueInterface<T3>.ReadValue(valueReader)
                );
        }

        public void WriteValue(IValueWriter valueWriter, ValueTuple<T1?, T2?, T3?> value)
        {
            ValueInterface.WriteValue(valueWriter, value.Item1);
            ValueInterface.WriteValue(valueWriter, value.Item2);
            ValueInterface.WriteValue(valueWriter, value.Item3);
        }
    }

    sealed class TupleInterface<T1, T2, T3, T4> : IValueInterface<ValueTuple<T1?, T2?, T3?, T4?>>
    {
        public ValueTuple<T1?, T2?, T3?, T4?> ReadValue(IValueReader valueReader)
        {
            return (
                ValueInterface<T1>.ReadValue(valueReader),
                ValueInterface<T2>.ReadValue(valueReader),
                ValueInterface<T3>.ReadValue(valueReader),
                ValueInterface<T4>.ReadValue(valueReader)
                );
        }

        public void WriteValue(IValueWriter valueWriter, ValueTuple<T1?, T2?, T3?, T4?> value)
        {
            ValueInterface.WriteValue(valueWriter, value.Item1);
            ValueInterface.WriteValue(valueWriter, value.Item2);
            ValueInterface.WriteValue(valueWriter, value.Item3);
            ValueInterface.WriteValue(valueWriter, value.Item4);
        }
    }

    sealed class TupleInterface<T1, T2, T3, T4, T5> : IValueInterface<ValueTuple<T1?, T2?, T3?, T4?, T5?>>
    {
        public ValueTuple<T1?, T2?, T3?, T4?, T5?> ReadValue(IValueReader valueReader)
        {
            return (
                ValueInterface<T1>.ReadValue(valueReader),
                ValueInterface<T2>.ReadValue(valueReader),
                ValueInterface<T3>.ReadValue(valueReader),
                ValueInterface<T4>.ReadValue(valueReader),
                ValueInterface<T5>.ReadValue(valueReader)
                );
        }

        public void WriteValue(IValueWriter valueWriter, ValueTuple<T1?, T2?, T3?, T4?, T5?> value)
        {
            ValueInterface.WriteValue(valueWriter, value.Item1);
            ValueInterface.WriteValue(valueWriter, value.Item2);
            ValueInterface.WriteValue(valueWriter, value.Item3);
            ValueInterface.WriteValue(valueWriter, value.Item4);
            ValueInterface.WriteValue(valueWriter, value.Item5);
        }
    }

    sealed class TupleInterface<T1, T2, T3, T4, T5, T6> : IValueInterface<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?>>
    {
        public ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> ReadValue(IValueReader valueReader)
        {
            return (
                ValueInterface<T1>.ReadValue(valueReader),
                ValueInterface<T2>.ReadValue(valueReader),
                ValueInterface<T3>.ReadValue(valueReader),
                ValueInterface<T4>.ReadValue(valueReader),
                ValueInterface<T5>.ReadValue(valueReader),
                ValueInterface<T6>.ReadValue(valueReader)
                );
        }

        public void WriteValue(IValueWriter valueWriter, ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?> value)
        {
            ValueInterface.WriteValue(valueWriter, value.Item1);
            ValueInterface.WriteValue(valueWriter, value.Item2);
            ValueInterface.WriteValue(valueWriter, value.Item3);
            ValueInterface.WriteValue(valueWriter, value.Item4);
            ValueInterface.WriteValue(valueWriter, value.Item5);
            ValueInterface.WriteValue(valueWriter, value.Item6);
        }
    }

    sealed class TupleInterface<T1, T2, T3, T4, T5, T6, T7> : IValueInterface<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?>>
    {
        public ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> ReadValue(IValueReader valueReader)
        {
            return (
                ValueInterface<T1>.ReadValue(valueReader),
                ValueInterface<T2>.ReadValue(valueReader),
                ValueInterface<T3>.ReadValue(valueReader),
                ValueInterface<T4>.ReadValue(valueReader),
                ValueInterface<T5>.ReadValue(valueReader),
                ValueInterface<T6>.ReadValue(valueReader),
                ValueInterface<T7>.ReadValue(valueReader)
                );
        }

        public void WriteValue(IValueWriter valueWriter, ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?> value)
        {
            ValueInterface.WriteValue(valueWriter, value.Item1);
            ValueInterface.WriteValue(valueWriter, value.Item2);
            ValueInterface.WriteValue(valueWriter, value.Item3);
            ValueInterface.WriteValue(valueWriter, value.Item4);
            ValueInterface.WriteValue(valueWriter, value.Item5);
            ValueInterface.WriteValue(valueWriter, value.Item6);
            ValueInterface.WriteValue(valueWriter, value.Item7);
        }
    }

    sealed class TupleInterface<T1, T2, T3, T4, T5, T6, T7, TRest> : IValueInterface<ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>> where TRest : struct
    {
        public ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> ReadValue(IValueReader valueReader)
        {
            return new ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest>(
                ValueInterface<T1>.ReadValue(valueReader),
                ValueInterface<T2>.ReadValue(valueReader),
                ValueInterface<T3>.ReadValue(valueReader),
                ValueInterface<T4>.ReadValue(valueReader),
                ValueInterface<T5>.ReadValue(valueReader),
                ValueInterface<T6>.ReadValue(valueReader),
                ValueInterface<T7>.ReadValue(valueReader),
                ValueInterface<TRest>.ReadValue(valueReader)
                );
        }

        public void WriteValue(IValueWriter valueWriter, ValueTuple<T1?, T2?, T3?, T4?, T5?, T6?, T7?, TRest> value)
        {
            ValueInterface.WriteValue(valueWriter, value.Item1);
            ValueInterface.WriteValue(valueWriter, value.Item2);
            ValueInterface.WriteValue(valueWriter, value.Item3);
            ValueInterface.WriteValue(valueWriter, value.Item4);
            ValueInterface.WriteValue(valueWriter, value.Item5);
            ValueInterface.WriteValue(valueWriter, value.Item6);
            ValueInterface.WriteValue(valueWriter, value.Item7);
            ValueInterface.WriteValue(valueWriter, value.Rest);
        }
    }
}