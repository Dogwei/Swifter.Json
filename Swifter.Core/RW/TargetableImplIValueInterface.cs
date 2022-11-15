namespace Swifter.RW
{
    sealed class TargetableImplIValueInterface<T> : ChainedValueInterfaceBase<T>
    {
        public static readonly TargetableImplIValueInterface<T> Instance = new();

        private static int targetCounting;

        public static void IncrementTargetCount()
        {
            lock (Instance)
            {
                if (targetCounting == 0)
                {
                    ValueInterface<T>.AddChainedInterface(Instance);
                }

                ++targetCounting;
            }
        }

        public static void DecrementTargetCount()
        {
            lock (Instance)
            {
                --targetCounting;

                if (targetCounting == 0)
                {
                    ValueInterface<T>.RemoveChainedInterface(Instance);
                }
            }
        }

        private TargetableImplIValueInterface()
        {

        }

        public override T? ReadValue(IValueReader valueReader)
        {
            if (valueReader is ITargetableValueRW targetableValueRW)
            {
                var valueInterface = targetableValueRW.GetValueInterface<T>();

                if (valueInterface != null)
                {
                    return valueInterface.ReadValue(valueReader);
                }
            }

            VersionDifferences.Assert(PreviousValueInterface is not null);

            return PreviousValueInterface.ReadValue(valueReader);
        }

        public override void WriteValue(IValueWriter valueWriter, T? value)
        {
            if (valueWriter is ITargetableValueRW targetableValueRW)
            {
                var valueInterface = targetableValueRW.GetValueInterface<T>();

                if (valueInterface != null)
                {
                    valueInterface.WriteValue(valueWriter, value);

                    return;
                }
            }

            VersionDifferences.Assert(PreviousValueInterface is not null);

            PreviousValueInterface.WriteValue(valueWriter, value);
        }
    }
}