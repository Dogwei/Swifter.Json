
using Swifter.RW;
using Swifter.Tools;

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    sealed class XLiteralFieldInfo<TValue> : XFieldInfo, IXFieldRW
    {
        TValue value;

        internal override void Initialize(FieldInfo fieldInfo, XBindingFlags flags)
        {
            base.Initialize(fieldInfo, flags);

            value = (TValue)FieldInfo.GetRawConstantValue();
        }

        public bool CanRead => true;

        public bool CanWrite => false;

        public int Order => RWFieldAttribute.DefaultOrder;

        public Type BeforeType => fieldInfo.FieldType;

        public Type AfterType => typeof(TValue);
        
        public bool IsPublic => FieldInfo.IsPublic;

        public bool IsStatic => true;

        public object Original => FieldInfo;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        void ThrowCannotSetException()
        {
            throw new InvalidOperationException($"Cannot set value of the const field '{FieldInfo.DeclaringType.Name}.{FieldInfo.Name}'.");
        }

        public override object GetValue()
        {
            return value;
        }

        public override void SetValue(object value)
        {
            ThrowCannotSetException();
        }

        public void OnReadValue(object obj, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.WriteValue(valueWriter, value);
        }

        public void OnWriteValue(object obj, IValueReader valueReader)
        {
            ThrowCannotSetException();
        }

        public T ReadValue<T>(object obj)
        {
            if (typeof(T) == typeof(TValue))
            {
                return Unsafe.As<TValue, T>(ref value);
            }

            return XConvert<T>.Convert(value);
        }

        public void WriteValue<T>(object obj, T value)
        {
            ThrowCannotSetException();
        }
    }
}