using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Reflection;

namespace Swifter.Reflection
{
    sealed class XStaticPropertyInfo<TValue> : XPropertyInfo, IXFieldRW
    {
        XStaticGetValueHandler<TValue> _get;
        XStaticSetValueHandler<TValue> _set;

        private protected override void InitializeByValue(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.InitializeByValue(propertyInfo, flags);

            if (_get == null)
            {
                var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);

                if (getMethod != null)
                {
                    _get = MethodHelper.CreateDelegate<XStaticGetValueHandler<TValue>>(getMethod, SignatureLevels.Cast);
                }
            }

            if (_set == null)
            {
                var setMethod = propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0);

                if (setMethod != null)
                {
                    _set = MethodHelper.CreateDelegate<XStaticSetValueHandler<TValue>>(setMethod, SignatureLevels.Cast);
                }
            }
        }

        private protected override void InitializeByRef(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.InitializeByRef(propertyInfo, flags);

            if (_get == null || _set == null)
            {
                var getMethod = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);

                if (getMethod != null)
                {
                    var _ref = MethodHelper.CreateDelegate<XStaticRefValueHandler<TValue>>(getMethod, SignatureLevels.Cast);

                    _get = () =>
                    {
                        return _ref();
                    };

                    _set = (value) =>
                    {
                        _ref() = value;
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

        public bool IsStatic => true;

        public object Original => PropertyInfo;

        public override object GetValue(object obj)
        {
            Assert(CanRead, "get");

            return _get();
        }

        public override object GetValue(TypedReference typedRef)
        {
            Assert(CanRead, "get");

            return _get();
        }

        public override void SetValue(object obj, object value)
        {
            Assert(CanWrite, "set");

            _set((TValue)value);
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            Assert(CanWrite, "set");

            _set((TValue)value);
        }

        public void OnReadValue(object obj, IValueWriter valueWriter)
        {
            Assert(CanRead, "get");

            ValueInterface<TValue>.WriteValue(valueWriter, _get());
        }

        public void OnWriteValue(object obj, IValueReader valueReader)
        {
            Assert(CanWrite, "set");

            _set(ValueInterface<TValue>.ReadValue(valueReader));
        }

        public T ReadValue<T>(object obj)
        {
            Assert(CanRead, "get");

            var value = _get();

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
                _set(Unsafe.As<T, TValue>(ref value));

                return;
            }

            _set(XConvert<TValue>.Convert(value));
        }
    }
}