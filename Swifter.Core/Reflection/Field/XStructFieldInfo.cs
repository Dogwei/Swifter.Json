
using Swifter.RW;
using Swifter.Tools;

using System;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    /// <summary>
    /// 表示一个结构类型中的实例字段信息
    /// </summary>
    /// <typeparam name="TStruct">结构类型</typeparam>
    /// <typeparam name="TValue">字段类型</typeparam>
    public sealed class XStructFieldInfo<TStruct, TValue> : XFieldInfo, IXFieldRW where TStruct : struct
    {
        private int offset;

        XStructFieldInfo()
        {

        }

        private protected override void Initialize(System.Reflection.FieldInfo fieldInfo, XBindingFlags flags)
        {
            offset = TypeHelper.OffsetOf(fieldInfo);

            base.Initialize(fieldInfo, flags);
        }

        /// <summary>
        /// 获取字段的值的引用。
        /// </summary>
        /// <param name="obj">对象的引用</param>
        /// <returns>返回字段的值的引用</returns>
        public ref TValue GetReference(ref TStruct obj)
            => ref Underlying.AddByteOffset(ref Underlying.As<TStruct, TValue>(ref obj), offset);

        /// <summary>
        /// 获取字段的值的引用。注意：此方法不会检查对象的类型，请确保对象是该字段的定义类的类型。
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>返回字段的值的引用</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ref TValue GetReference(object obj)
            => ref Underlying.AddByteOffset(ref TypeHelper.Unbox<TValue>(obj), offset);

        /// <summary>
        /// 获取字段的值的引用。
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>返回字段的值的引用</returns>
        /// <exception cref="ArgumentException">对象不是字段的定义类的类型</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ref TValue GetReferenceCheck(object obj)
        {
            if (!(obj is TStruct))
            {
                throw new System.Reflection.TargetException(nameof(obj));
            }

            return ref GetReference(obj);
        }

        /// <summary>
        /// 获取字段的值。
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>返回字段的值</returns>
        /// <exception cref="ArgumentException">对象不是字段的定义类的类型</exception>
        public override object GetValue(object obj)
        {
            return GetReferenceCheck(obj);
        }

        /// <summary>
        /// 设置字段的值。
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="value">值</param>
        /// <exception cref="ArgumentException">对象不是字段的定义类的类型</exception>
        public override void SetValue(object obj, object value)
        {
            GetReferenceCheck(obj) = (TValue)value;
        }



        int IObjectField.Order => RWFieldAttribute.DefaultOrder;

        bool IObjectField.CanRead => true;

        bool IObjectField.CanWrite => true;

        Type IObjectField.BeforeType => typeof(TValue);

        Type IObjectField.AfterType => typeof(TValue);

        bool IObjectField.IsPublic => FieldInfo.IsPublic;

        bool IObjectField.IsStatic => false;

        object IObjectField.Original => FieldInfo;

        bool IObjectField.SkipDefaultValue => (flags & XBindingFlags.RWSkipDefaultValue) != 0;

        bool IObjectField.CannotGetException => (flags & XBindingFlags.RWCannotGetException) != 0;

        bool IObjectField.CannotSetException => (flags & XBindingFlags.RWCannotSetException) != 0;


        T IXFieldRW.ReadValue<T>(object obj)
        {
            return XConvert<T>.Convert(GetReference(obj));
        }

        void IXFieldRW.WriteValue<T>(object obj, T value)
        {
            GetReference(obj) = XConvert<TValue>.Convert(value);
        }

        void IXFieldRW.OnReadValue(object obj, IValueWriter valueWriter)
        {
            ValueInterface<TValue>.WriteValue(valueWriter, GetReference(obj));
        }

        void IXFieldRW.OnWriteValue(object obj, IValueReader valueReader)
        {
            GetReference(obj) = ValueInterface<TValue>.ReadValue(valueReader);
        }
    }
}