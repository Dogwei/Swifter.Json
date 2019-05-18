using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    sealed class XStructPropertyInfo<TStruct, TValue> : XPropertyInfo, IXFieldRW where TStruct : struct
    {
        XStructGetValueHandler<TStruct, TValue> _get;
        XStructSetValueHandler<TStruct, TValue> _set;

        Type declaringType;

        private protected override void InitializeByValue(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.InitializeByValue(propertyInfo, flags);

            declaringType = propertyInfo.DeclaringType;

            if (_get == null)
            {
                var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);

                if (getMethod != null)
                {
                    _get = MethodHelper.CreateDelegate<XStructGetValueHandler<TStruct, TValue>>(getMethod, SignatureLevels.Cast);
                }
            }

            if (_set == null)
            {
                var setMethod = propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0);

                if (setMethod != null)
                {
                    _set = MethodHelper.CreateDelegate<XStructSetValueHandler<TStruct, TValue>>(setMethod, SignatureLevels.Cast);
                }
            }
        }

        private protected override void InitializeByRef(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.InitializeByRef(propertyInfo, flags);

            declaringType = propertyInfo.DeclaringType;

            if (_get == null || _set == null)
            {
                var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);

                if (getMethod != null)
                {
                    var _ref = MethodHelper.CreateDelegate<XStructRefValueHandler<TStruct, TValue>>(getMethod, SignatureLevels.Cast);

                    _get = (ref TStruct obj) =>
                    {
                        return _ref(ref obj);
                    };

                    _set = (ref TStruct obj, TValue value) =>
                    {
                        _ref(ref obj) = value;
                    };
                }
            }
        }

        public bool CanRead => _get != null;

        public bool CanWrite => _set != null;

        public int Order => RWFieldAttribute.DefaultOrder;

        public Type BeforeType => typeof(TValue);

        public Type AfterType => typeof(TValue);

        public bool IsPublic => (PropertyInfo.GetGetMethod(true) ?? PropertyInfo.GetSetMethod(true))?.IsPublic ?? false;

        public bool IsStatic => false;

        public object Original => PropertyInfo;
        
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        ref TStruct GetRef(object obj)
        {
            return ref TypeHelper.Unbox<TStruct>(obj);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        ref TStruct GetRefCheck(object obj)
        {
            if (obj.GetType() != declaringType)
            {
                throw new TargetException(nameof(obj));
            }

            return ref GetRef(obj);
        }

        public override object GetValue(object obj)
        {
            Assert(CanRead, "get");

            return _get(ref GetRefCheck(obj));
        }

        public override object GetValue(TypedReference typedRef)
        {
            Assert(CanRead, "get");

            if (__reftype(typedRef) != declaringType)
            {
                throw new TargetException(nameof(typedRef));
            }

            return _get(ref __refvalue(typedRef, TStruct));
        }
        public override void SetValue(object obj, object value)
        {
            Assert(CanWrite, "set");

            _set(ref GetRefCheck(obj), (TValue)value);
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            Assert(CanWrite, "set");

            if (__reftype(typedRef) != declaringType)
            {
                throw new TargetException(nameof(typedRef));
            }

            _set(ref __refvalue(typedRef, TStruct), (TValue)value);
        }

        public void OnReadValue(object obj, IValueWriter valueWriter)
        {
            Assert(CanRead, "get");

            ValueInterface<TValue>.WriteValue(valueWriter, _get(ref GetRef(obj)));
        }

        public void OnWriteValue(object obj, IValueReader valueReader)
        {
            Assert(CanWrite, "set");

            _set(ref GetRef(obj), ValueInterface<TValue>.ReadValue(valueReader));
        }

        public T ReadValue<T>(object obj)
        {
            Assert(CanRead, "get");

            var value = _get(ref GetRef(obj));

            if (typeof(T) == typeof(TValue))
            {
                return Unsafe.As<TValue, T>(ref value);
            }

            return XConvert<T>.Convert(value);
        }

        public void WriteValue<T>(object obj, T value)
        {
            Assert(CanWrite, "set");

            if (typeof(T) == typeof(TValue))
            {
                _set(ref GetRef(obj), (TValue)(object)value);

                return;
            }

            _set(ref GetRef(obj), XConvert<TValue>.Convert(value));
        }
    }
}