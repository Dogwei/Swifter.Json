
using Swifter.RW;
using Swifter.Tools;

using System;

namespace Swifter.Reflection
{
    /// <summary>
    /// 表示静态字段的信息。
    /// </summary>
    /// <typeparam name="TValue">字段类型</typeparam>
    public sealed class XStaticFieldInfo<TValue> : XFieldInfo, IXFieldRW
    {
        private unsafe void* address;
        private int offset;

        unsafe void* Address => address == null
            ? ((byte*)TypeHelper.GetStaticsBasePointer(fieldInfo) + offset) 
            : address;

        /// <summary>
        /// 获取字段值的引用。
        /// </summary>
        public unsafe ref TValue Value 
            => ref Underlying.AsRef<TValue>(Address);

        XStaticFieldInfo()
        {

        }

        private protected override unsafe void Initialize(System.Reflection.FieldInfo fieldInfo, XBindingFlags flags)
        {
            base.Initialize(fieldInfo, flags);

            offset = TypeHelper.OffsetOf(fieldInfo);

            switch (TypeHelper.GetStaticBaseBlock(fieldInfo))
            {
                case StaticsBaseBlock.GC:
                case StaticsBaseBlock.NonGC:
                    address = (byte*)TypeHelper.GetStaticsBasePointer(fieldInfo) + offset;
                    break;
            }
        }

        /// <summary>
        /// 获取字段的值。
        /// </summary>
        /// <returns>返回字段的值</returns>
        public override object GetValue()
        {
            return Value;
        }

        /// <summary>
        /// 设置字段的值。
        /// </summary>
        /// <param name="value">值</param>
        public override void SetValue(object value)
        {
            Value = (TValue)value;
        }


        bool IObjectField.CanRead => true;

        bool IObjectField.CanWrite => true;

        int IObjectField.Order => RWFieldAttribute.DefaultOrder;

        Type IObjectField.BeforeType => typeof(TValue);

        Type IObjectField.AfterType => typeof(TValue);

        bool IObjectField.IsPublic => FieldInfo.IsPublic;

        bool IObjectField.IsStatic => true;

        object IObjectField.Original => FieldInfo;

        bool IObjectField.SkipDefaultValue => (flags & XBindingFlags.RWSkipDefaultValue) != 0;

        bool IObjectField.CannotGetException => (flags & XBindingFlags.RWCannotGetException) != 0;

        bool IObjectField.CannotSetException => (flags & XBindingFlags.RWCannotSetException) != 0;


        void IXFieldRW.OnReadValue(object obj, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.WriteValue(valueWriter, Value);
        }

        void IXFieldRW.OnWriteValue(object obj, IValueReader valueReader)
        {
            Value = ValueInterface<TValue>.ReadValue(valueReader);
        }

        T IXFieldRW.ReadValue<T>(object obj)
        {
            return XConvert<T>.Convert(Value);
        }

        void IXFieldRW.WriteValue<T>(object obj, T value)
        {
            Value = XConvert<TValue>.Convert(value);
        }
    }
}