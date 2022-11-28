﻿


namespace Swifter.RW
{
    internal sealed class CharInterface : IValueInterface<char>, IDefaultBehaviorValueInterface
    {
        public char ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadChar();
        }

        public void WriteValue(IValueWriter valueWriter, char value)
        {
            valueWriter.WriteChar(value);
        }
    }
}