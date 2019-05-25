using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    sealed class XClassFieldInfo<TValue> : XFieldInfo, IXFieldRW
    {
        private int offset;
        private Type declaringType;

        public int Order => RWFieldAttribute.DefaultOrder;

        public bool CanRead => true;

        public bool CanWrite => true;

        public Type BeforeType => fieldInfo.FieldType;

        public Type AfterType => typeof(TValue);

        public bool IsPublic => FieldInfo.IsPublic;

        public bool IsStatic => false;

        public object Original => FieldInfo;

        internal override void Initialize(System.Reflection.FieldInfo fieldInfo, XBindingFlags flags)
        {
            offset = TypeHelper.OffsetOf(fieldInfo);
            declaringType = fieldInfo.DeclaringType;

            base.Initialize(fieldInfo, flags);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        ref byte GetAddress(object obj) => ref Unsafe.AddByteOffset(ref TypeHelper.Unbox<byte>(obj), offset);

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        ref byte GetAddressCheck(object obj)
        {
            if (!declaringType.IsInstanceOfType(obj))
            {
                throw new System.Reflection.TargetException(nameof(obj));
            }

            return ref GetAddress(obj);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        ref byte GetAddress(TypedReference typedRef)
        {
            if (declaringType != __reftype(typedRef))
            {
                throw new System.Reflection.TargetException(nameof(typedRef));
            }

            return ref GetAddress(Unsafe.AsRef<object>(typedRef));
        }
        
        public override object GetValue(object obj)
        {
            return Unsafe.Read<TValue>(ref GetAddressCheck(obj));
        }

        public override unsafe object GetValue(TypedReference typedRef)
        {
            return Unsafe.Read<TValue>(ref GetAddress(typedRef));
        }

        public void OnReadValue(object obj, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.WriteValue(valueWriter, Unsafe.Read<TValue>(ref GetAddress(obj)));
        }

        public void OnWriteValue(object obj, IValueReader valueReader)
        {
            Unsafe.Write(ref GetAddress(obj), ValueInterface<TValue>.ReadValue(valueReader));
        }

        public T ReadValue<T>(object obj)
        {
            if (typeof(T) == typeof(TValue))
            {
                return Unsafe.Read<T>(ref GetAddress(obj));
            }

            return XConvert<T>.Convert(Unsafe.Read<TValue>(ref GetAddress(obj)));
        }

        public override void SetValue(object obj, object value)
        {
            Unsafe.Write(ref GetAddressCheck(obj), (TValue)value);
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            Unsafe.Write(ref GetAddress(typedRef), (TValue)value);
        }

        public void WriteValue<T>(object obj, T value)
        {
            if (typeof(T) == typeof(TValue))
            {
                Unsafe.Write(ref GetAddress(obj), value);

                return;
            }

            Unsafe.Write(ref GetAddress(obj), XConvert<TValue>.Convert(value));
        }
    }
}