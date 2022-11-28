﻿


namespace Swifter.RW
{
    internal sealed class DoubleInterface : IValueInterface<double>, IDefaultBehaviorValueInterface
    {
        public double ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadDouble();
        }

        public void WriteValue(IValueWriter valueWriter, double value)
        {
            valueWriter.WriteDouble(value);
        }
    }
}