using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Reflection;

namespace Swifter.Reflection
{
    sealed class XDefaultPropertyInfo : XPropertyInfo, IXFieldRW
    {
        MethodInfo _get;
        MethodInfo _set;

        private protected override void InitializeByRef(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.InitializeByRef(propertyInfo, flags);

            if (_get == null)
            {
                _get = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);
            }

            if (_set == null)
            {
                _set = propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0);
            }
        }

        private protected override void InitializeByValue(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.InitializeByValue(propertyInfo, flags);

            if (_get == null)
            {
                _get = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);
            }

            if (_set == null)
            {
                _set = propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0);
            }
        }

        public Type BeforeType => propertyInfo.PropertyType;

        public Type AfterType => propertyInfo.PropertyType;

        public bool CanRead => _get != null && propertyInfo.CanRead;

        public bool CanWrite => _set != null && propertyInfo.CanWrite;

        public bool IsPublic => (PropertyInfo.GetGetMethod(true) ?? PropertyInfo.GetSetMethod(true))?.IsPublic ?? false;

        public bool IsStatic => (PropertyInfo.GetGetMethod(true) ?? PropertyInfo.GetSetMethod(true))?.IsStatic ?? false;

        public int Order => RWFieldAttribute.DefaultOrder;

        public object Original => PropertyInfo;

        public override object GetValue(object obj)
        {
            Assert(CanRead, "get");

            return _get.Invoke(obj, null);
        }

        public override object GetValue(TypedReference typedRef)
        {
            Assert(CanRead, "get");

            return _get.Invoke(Unsafe.AsRef<object>(typedRef), null);
        }

        public void OnReadValue(object obj, IValueWriter valueWriter)
        {
            ValueInterface.WriteValue(valueWriter, GetValue(obj));
        }

        public void OnWriteValue(object obj, IValueReader valueReader)
        {
            SetValue(obj, ValueInterface.GetInterface(AfterType).Read(valueReader));
        }

        public T ReadValue<T>(object obj)
        {
            return XConvert<T>.FromObject(GetValue(obj));
        }

        public override void SetValue(object obj, object value)
        {
            Assert(CanWrite, "set");

            _set.Invoke(obj, new object[] { value });
        }

        public override void SetValue(TypedReference typedRef, object value)
        {
            Assert(CanWrite, "set");

            _set.Invoke(Unsafe.AsRef<object>(typedRef), new object[] { value });
        }

        public void WriteValue<T>(object obj, T value)
        {
            SetValue(obj, XConvert.ToObject(value, BeforeType));
        }
    }
}