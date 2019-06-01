using Swifter.Readers;
using Swifter.RW;
using Swifter.Tools;
using Swifter.Writers;
using System;

namespace Swifter.Reflection
{
    sealed class XDefaultFieldInfo : XFieldInfo, IXFieldRW
    {
        public Type BeforeType => fieldInfo.FieldType;

        public Type AfterType => fieldInfo.FieldType;

        public bool CanRead => true;

        public bool CanWrite => true;

        public bool IsPublic => fieldInfo.IsPublic;

        public bool IsStatic => fieldInfo.IsStatic;

        public int Order => RWFieldAttribute.DefaultOrder;

        public object Original => fieldInfo;

        public override object GetValue(object obj)
        {
            return fieldInfo.GetValue(obj);
        }

        public override object GetValue()
        {
            return fieldInfo.GetValue(null);
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
            return XConvert.FromObject<T>(GetValue(obj));
        }

        public override void SetValue(object obj, object value)
        {
            fieldInfo.SetValue(obj, value);
        }

        public override void SetValue(object value)
        {
            fieldInfo.SetValue(null, value);
        }

        public void WriteValue<T>(object obj, T value)
        {
            SetValue(obj, XConvert.ToObject(value, BeforeType));
        }
    }
}