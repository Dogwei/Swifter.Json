using System;
using System.Diagnostics;

namespace Swifter.RW
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITargetableValueRWSource
    {
        /// <summary>
        /// 
        /// </summary>
        public TargetableValueInterfaceMapSource ValueInterfaceMapSource { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValueReader"></typeparam>
    /// <typeparam name="TValueWriter"></typeparam>
    public interface ITargetableValueRWSource<TValueReader, TValueWriter> : ITargetableValueRWSource 
        where TValueReader : IValueReader 
        where TValueWriter : IValueWriter
    {

    }

    sealed class TargetableValueInterface<TValueReader, TValueWriter, TValue> : IValueInterface<TValue>
        where TValueReader : IValueReader
        where TValueWriter : IValueWriter
    {
        public readonly Func<TValueReader, TValue?>? ReadValueFunc;
        public readonly Action<TValueWriter, TValue?>? WriteValueFunc;

        public TargetableValueInterface(Func<TValueReader, TValue?>? readValueFunc, Action<TValueWriter, TValue?>? writeValueFunc)
        {
            this.ReadValueFunc = readValueFunc;
            this.WriteValueFunc = writeValueFunc;
        }

        public TValue? ReadValue(IValueReader valueReader)
        {
            if (ReadValueFunc != null)
            {
                return ReadValueFunc((TValueReader)valueReader);
            }

            var previousValueInterface = TargetableImplIValueInterface<TValue>.Instance.PreviousValueInterface;

            VersionDifferences.Assert(previousValueInterface is not null);

            return previousValueInterface.ReadValue(valueReader);
        }

        public void WriteValue(IValueWriter valueWriter, TValue? value)
        {
            if (WriteValueFunc != null)
            {
                WriteValueFunc((TValueWriter)valueWriter, value);

                return;
            }

            var previousValueInterface = TargetableImplIValueInterface<TValue>.Instance.PreviousValueInterface;

            VersionDifferences.Assert(previousValueInterface is not null);

            previousValueInterface.WriteValue(valueWriter, value);
        }
    }
}