using System;

namespace Swifter.RW
{
    sealed class NonGenericValueInterface : ValueInterface
    {
        public override Type Type { get; }

        public NonGenericValueInterface(Type type)
        {
            Type = type;
        }

        public override bool IsDefaultBehaviorInternal => true;

        public override object Interface => throw new NotSupportedException();

        public override object? Read(IValueReader valueReader)
        {
            throw new NotSupportedException();
        }

        public override void Write(IValueWriter valueWriter, object? value)
        {
            throw new NotSupportedException();
        }
    }
}