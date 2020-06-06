
using Swifter.RW;

using System;

namespace Swifter.Reflection
{
    sealed class XInterfaceAttributedFieldRW<T> : XAttributedFieldRW, IObjectField
    {
        internal readonly IValueInterface<T> valueInterface;

        public XInterfaceAttributedFieldRW(IXFieldRW fieldRW, RWFieldAttribute attribute, XBindingFlags flags, IValueInterface<T> valueInterface) 
            : base(fieldRW, attribute, flags)
        {
            this.valueInterface = valueInterface;
        }

        public override void OnReadValue(object obj, IValueWriter valueWriter)
        {
            valueInterface.WriteValue(valueWriter, ReadValue<T>(obj));
        }

        public override void OnWriteValue(object obj, IValueReader valueReader)
        {
            WriteValue(obj, valueInterface.ReadValue(valueReader));
        }

        Type IObjectField.AfterType => typeof(T);
    }
}