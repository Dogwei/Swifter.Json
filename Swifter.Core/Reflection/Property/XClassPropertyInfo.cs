
using Swifter.RW;
using Swifter.Tools;

using System;
using System.Reflection;

namespace Swifter.Reflection
{
    sealed class XClassPropertyInfo<TValue> : XPropertyInfo, IXFieldRW
    {
        XClassGetValueHandler<TValue> _get;
        XClassSetValueHandler<TValue> _set;

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
                    _get = MethodHelper.CreateDelegate<XClassGetValueHandler<TValue>>(getMethod, false);
                }
            }

            if (_set == null)
            {
                var setMethod = propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0);

                if (setMethod != null)
                {
                    _set = MethodHelper.CreateDelegate<XClassSetValueHandler<TValue>>(setMethod, false);
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
                    var _ref = MethodHelper.CreateDelegate<XClassRefValueHandler<TValue>>(getMethod, false);

                    _get = (obj) =>
                    {
                        return _ref(obj);
                    };

                    _set = (obj, value) =>
                    {
                        _ref(obj) = value;
                    };
                }
            }
        }
        
        public bool CanRead => _get != null;
        
        public bool CanWrite => _set != null;

        public int Order => RWFieldAttribute.DefaultOrder;
        
        public Type BeforeType => propertyInfo.PropertyType;

        public Type AfterType => typeof(TValue);

        public bool IsPublic => (PropertyInfo.GetGetMethod(true) ?? PropertyInfo.GetSetMethod(true))?.IsPublic ?? false;

        public bool IsStatic => false;

        public object Original => PropertyInfo;

        public override object GetValue(object obj)
        {
            Assert(CanRead, "get");

            if (!declaringType.IsInstanceOfType(obj))
            {
                throw new TargetException(nameof(obj));
            }

            return _get(obj);
        }

        public override void SetValue(object obj, object value)
        {
            Assert(CanWrite, "set");

            if (!declaringType.IsInstanceOfType(obj))
            {
                throw new TargetException(nameof(obj));
            }

            _set(obj, (TValue)value);
        }

        public void OnReadValue(object obj, IValueWriter valueWriter)
        {
            Assert(CanRead, "get");

            ValueInterface<TValue>.WriteValue(valueWriter, _get(obj));
        }

        public void OnWriteValue(object obj, IValueReader valueReader)
        {
            Assert(CanWrite, "set");

            _set(obj, ValueInterface<TValue>.ReadValue(valueReader));
        }

        public T ReadValue<T>(object obj)
        {
            Assert(CanRead, "get");

            var value = _get(obj);

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
                _set(obj, Unsafe.As<T, TValue>(ref value));

                return;
            }

            _set(obj, XConvert<TValue>.Convert(value));
        }
    }
}