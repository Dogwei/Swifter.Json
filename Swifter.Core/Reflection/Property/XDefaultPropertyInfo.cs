
using Swifter.RW;

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    sealed class XDefaultPropertyInfo : XPropertyInfo, IXFieldRW
    {
        MethodInfo _get;
        MethodInfo _set;

        ValueInterface @interface;

        internal XDefaultPropertyInfo()
        {

        }

        private protected override void InitializeByRef(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.InitializeByRef(propertyInfo, flags);

            _get = null;
            _set = null;

            @interface = ValueInterface.GetInterface(propertyInfo.PropertyType.GetElementType());
        }

        private protected override void InitializeByValue(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            base.InitializeByValue(propertyInfo, flags);

            if (_get is null)
            {
                _get = propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0);
            }

            if (_set is null)
            {
                _set = propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0);
            }

            @interface = ValueInterface.GetInterface(propertyInfo.PropertyType);
        }

        public bool CanRead
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => _get != null;
        }

        public bool CanWrite
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get => _set != null;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override object GetValue(object obj)
        {
            Assert(CanRead, "get");

            return _get.Invoke(obj, null);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override object GetValue()
        {
            Assert(CanRead, "get");

            return _get.Invoke(null, null);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override void SetValue(object obj, object value)
        {
            Assert(CanWrite, "set");

            _set.Invoke(obj, new object[] { value });
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override void SetValue(object value)
        {
            Assert(CanWrite, "set");

            _set.Invoke(null, new object[] { value });
        }

        Type IObjectField.BeforeType => propertyInfo.PropertyType;

        Type IObjectField.AfterType => propertyInfo.PropertyType;

        bool IObjectField.IsPublic => (PropertyInfo.GetGetMethod(true) ?? PropertyInfo.GetSetMethod(true))?.IsPublic ?? false;

        bool IObjectField.IsStatic => (PropertyInfo.GetGetMethod(true) ?? PropertyInfo.GetSetMethod(true))?.IsStatic ?? false;

        int IObjectField.Order => RWFieldAttribute.DefaultOrder;

        object IObjectField.Original => PropertyInfo;

        bool IObjectField.SkipDefaultValue => (flags & XBindingFlags.RWSkipDefaultValue) != 0;

        bool IObjectField.CannotGetException => (flags & XBindingFlags.RWCannotGetException) != 0;

        bool IObjectField.CannotSetException => (flags & XBindingFlags.RWCannotSetException) != 0;

        void IXFieldRW.OnReadValue(object obj, IValueWriter valueWriter)
        {
            // TODO: If static
            @interface.Write(valueWriter, GetValue(obj));
        }

        void IXFieldRW.OnWriteValue(object obj, IValueReader valueReader)
        {
            // TODO: If static
            SetValue(obj, @interface.Read(valueReader));
        }

        T IXFieldRW.ReadValue<T>(object obj)
        {
            // TODO: If static
            return @interface.XConvertTo<T>(GetValue(obj));
        }

        void IXFieldRW.WriteValue<T>(object obj, T value)
        {
            // TODO: If static
            SetValue(obj, @interface.XConvertFrom(value));
        }
    }
}