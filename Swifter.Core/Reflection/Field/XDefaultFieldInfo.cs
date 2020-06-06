
using Swifter.RW;

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    sealed class XDefaultFieldInfo : XFieldInfo, IXFieldRW
    {
        ValueInterface @interface;

        internal XDefaultFieldInfo()
        {

        }

        private protected override void Initialize(FieldInfo fieldInfo, XBindingFlags flags)
        {
            @interface = ValueInterface.GetInterface(fieldInfo.FieldType);

            base.Initialize(fieldInfo, flags);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override object GetValue(object obj)
        {
            return fieldInfo.GetValue(obj);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override object GetValue()
        {
            return fieldInfo.GetValue(null);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override void SetValue(object obj, object value)
        {
            fieldInfo.SetValue(obj, value);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public override void SetValue(object value)
        {
            fieldInfo.SetValue(null, value);
        }

        Type IObjectField.BeforeType => fieldInfo.FieldType;

        Type IObjectField.AfterType => fieldInfo.FieldType;

        bool IObjectField.CanRead => true;

        bool IObjectField.CanWrite => true;

        bool IObjectField.IsPublic => fieldInfo.IsPublic;

        bool IObjectField.IsStatic => fieldInfo.IsStatic;

        int IObjectField.Order => RWFieldAttribute.DefaultOrder;

        object IObjectField.Original => fieldInfo;

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