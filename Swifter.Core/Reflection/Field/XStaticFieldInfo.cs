
using Swifter.RW;
using Swifter.Tools;

using System;

namespace Swifter.Reflection
{
    unsafe sealed class XStaticFieldInfo<TValue> : XFieldInfo, IXFieldRW
    {
        private void* address;

        private ref TValue Value => ref Unsafe.AsRef<TValue>(address);

        public bool CanRead => true;

        public bool CanWrite => true;

        public int Order => RWFieldAttribute.DefaultOrder;

        public Type BeforeType => fieldInfo.FieldType;

        public Type AfterType => typeof(TValue);

        public bool IsPublic => FieldInfo.IsPublic;

        public bool IsStatic => true;

        public object Original => FieldInfo;

        internal override void Initialize(System.Reflection.FieldInfo fieldInfo, XBindingFlags flags)
        {
            base.Initialize(fieldInfo, flags);

            address = (byte*)TypeHelper.GetTypeStaticMemoryAddress(fieldInfo.DeclaringType) + TypeHelper.OffsetOf(fieldInfo);
        }

        public override object GetValue()
        {
            return Value;
        }

        public override void SetValue(object value)
        {
            Value = (TValue)value;
        }

        public void OnReadValue(object obj, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.WriteValue(valueWriter, Value);
        }

        public void OnWriteValue(object obj, IValueReader valueReader)
        {
            Value = ValueInterface<TValue>.ReadValue(valueReader);
        }

        public T ReadValue<T>(object obj)
        {
            if (typeof(T) == typeof(TValue))
            {
                return Unsafe.Read<T>(address);
            }

            return XConvert<T>.Convert(Value);
        }

        public void WriteValue<T>(object obj, T value)
        {
            if (typeof(T) == typeof(TValue))
            {
                Unsafe.Write(address, value);

                return;
            }

            Value = XConvert<TValue>.Convert(value);
        }
    }
}