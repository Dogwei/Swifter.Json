using InlineIL;
using Swifter.RW;
using Swifter.Tools;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public sealed class XInstancePropertyInfo<TValue> : XPropertyInfo, IXFieldRW
    {
        delegate TValue? StructGetValueFunc<TStruct>(ref TStruct obj);
        delegate void StructSetValueAction<TStruct>(ref TStruct obj, TValue? value);
        delegate ref TValue? StructRefValueFunc<TStruct>(ref TStruct obj);

        delegate TValue? ClassGetValueFunc<TClass>(TClass obj);
        delegate void ClassSetValueAction<TClass>(TClass obj, TValue? value);
        delegate ref TValue? ClassRefValueFunc<TClass>(TClass obj);

        private readonly Type declaringType;
        private readonly Delegate? getValue;
        private readonly Delegate? setValue;
        private readonly bool isStruct;

        new private readonly IValueInterface<TValue>? valueInterface;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="flags"></param>
        public XInstancePropertyInfo(PropertyInfo propertyInfo, XBindingFlags flags)
            : base(propertyInfo, typeof(TValue), flags)
        {
            declaringType = propertyInfo.DeclaringType!;
            isStruct = declaringType.IsValueType;

            if (propertyInfo.PropertyType.IsByRef)
            {
                var getMethod = propertyInfo.GetGetMethod(true);

                if (getMethod is not null)
                {
                    if (isStruct)
                    {
                        var refValue = Delegate.CreateDelegate(typeof(StructRefValueFunc<>).MakeGenericType(typeof(TValue), declaringType), getMethod);

                        getValue = (StructGetValueFunc<byte>)GetValueImpl<byte>;
                        setValue = (StructSetValueAction<byte>)SetValueImpl<byte>;

                        TValue? GetValueImpl<TStruct>(ref TStruct obj) => Unsafe.As<StructRefValueFunc<TStruct>>(refValue)(ref obj);
                        void SetValueImpl<TStruct>(ref TStruct obj, TValue? value) => Unsafe.As<StructRefValueFunc<TStruct>>(refValue)(ref obj) = value;
                    }
                    else
                    {
                        var refValue = Delegate.CreateDelegate(typeof(ClassRefValueFunc<>).MakeGenericType(typeof(TValue), declaringType), getMethod);

                        getValue = (ClassGetValueFunc<object>)GetValueImpl<object>;
                        setValue = (ClassSetValueAction<object>)SetValueImpl<object>;

                        TValue? GetValueImpl<TClass>(TClass obj) => Unsafe.As<ClassRefValueFunc<TClass>>(refValue)(obj);
                        void SetValueImpl<TClass>(TClass obj, TValue? value) => Unsafe.As<ClassRefValueFunc<TClass>>(refValue)(obj) = value;
                    }
                }

                canRead = propertyInfo.GetGetMethod(flags.On(XBindingFlags.NonPublic)) is not null;
                canWrite = propertyInfo.GetGetMethod(flags.On(XBindingFlags.NonPublic)) is not null;
            }
            else if (TypeHelper.IsAutoProperty(propertyInfo, out var fieldInfo))
            {
                var success = false;

                try
                {
                    var offset = TypeHelper.OffsetOf(fieldInfo);

                    if (isStruct)
                    {
                        getValue = (StructGetValueFunc<byte>)GetValueImpl<byte>;
                        setValue = (StructSetValueAction<byte>)SetValueImpl<byte>;

                        TValue? GetValueImpl<TStruct>(ref TStruct obj) => TypeHelper.AddByteOffset(ref Unsafe.As<TStruct, TValue?>(ref obj), offset);
                        void SetValueImpl<TStruct>(ref TStruct obj, TValue? value) => TypeHelper.AddByteOffset(ref Unsafe.As<TStruct, TValue?>(ref obj), offset) = value;
                    }
                    else
                    {
                        getValue = (ClassGetValueFunc<object>)GetValueImpl<object>;
                        setValue = (ClassSetValueAction<object>)SetValueImpl<object>;

                        TValue? GetValueImpl<TClass>(TClass obj) => TypeHelper.AddByteOffset(ref TypeHelper.Unbox<TValue?>(obj!), offset);
                        void SetValueImpl<TClass>(TClass obj, TValue? value) => TypeHelper.AddByteOffset(ref TypeHelper.Unbox<TValue?>(obj!), offset) = value;
                    }

                    success = true;
                }
                catch
                {
                }

                if (success)
                {
                    if (flags.On(XBindingFlags.RWAutoPropertyDirectRW))
                    {
                        canRead = true;
                        canWrite = true;
                    }
                }
                else
                {
                    IL.Emit.Br("Else");
                }
            }
            else
            {
                IL.MarkLabel("Else");

                var getMethod = propertyInfo.GetGetMethod(true);
                var setMethod = propertyInfo.GetSetMethod(true);

                if (isStruct)
                {
                    if (getMethod is not null)
                    {
                        getValue = Delegate.CreateDelegate(typeof(StructGetValueFunc<>).MakeGenericType(typeof(TValue), declaringType), getMethod);
                    }

                    if (setMethod is not null)
                    {
                        setValue = Delegate.CreateDelegate(typeof(StructSetValueAction<>).MakeGenericType(typeof(TValue), declaringType), setMethod);
                    }
                }
                else
                {
                    if (getMethod is not null)
                    {
                        getValue = Delegate.CreateDelegate(typeof(ClassGetValueFunc<>).MakeGenericType(typeof(TValue), declaringType), getMethod);
                    }

                    if (setMethod is not null)
                    {
                        setValue = Delegate.CreateDelegate(typeof(ClassSetValueAction<>).MakeGenericType(typeof(TValue), declaringType), setMethod);
                    }
                }
            }
        }

        private XInstancePropertyInfo(XInstancePropertyInfo<TValue> baseInfo, RWFieldAttribute attribute)
            : base(baseInfo, attribute)
        {
            declaringType = baseInfo.declaringType;
            getValue = baseInfo.getValue;
            setValue = baseInfo.setValue;
            isStruct = baseInfo.isStruct;

            attribute.GetBestMatchInterfaceMethod(typeof(TValue), out var firstArgument, out var readValueMethod, out var writeValueMethod);

            if (readValueMethod is not null || writeValueMethod is not null)
            {
                valueInterface = (IValueInterface<TValue>)XHelper.GetValueInterface(typeof(TValue), firstArgument, readValueMethod, writeValueMethod);
            }

            canRead = getValue is not null && attribute.Access.On(RWFieldAccess.ReadOnly);
            canWrite = setValue is not null && attribute.Access.On(RWFieldAccess.WriteOnly);
        }

        /// <summary>
        /// [无检查] 获取该属性的值。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <returns>返回该字段的值</returns>
        /// <exception cref="MemberAccessException">此属性没有 <see langword="get"/> 方法或不能访问</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public TValue? UnsafeGetValue(object obj)
        {
            if (getValue is null)
            {
                throw new MemberAccessException();
            }

            if (isStruct)
            {
                return Unsafe.As<StructGetValueFunc<byte>>(getValue)(ref TypeHelper.Unbox<byte>(obj));
            }
            else
            {
                return Unsafe.As<ClassGetValueFunc<object>>(getValue)(obj);
            }
        }

        /// <summary>
        /// [无检查] 设置该属性的值。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <param name="value">值</param>
        /// <exception cref="MemberAccessException">此属性没有 <see langword="set"/> 方法或不能访问。</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void UnsafeSetValue(object obj, TValue? value)
        {
            if (setValue is null)
            {
                throw new MemberAccessException();
            }

            if (isStruct)
            {
                Unsafe.As<StructSetValueAction<byte>>(setValue)(ref TypeHelper.Unbox<byte>(obj), value);
            }
            else
            {
                Unsafe.As<ClassSetValueAction<object>>(setValue)(obj, value);
            }
        }

        /// <summary>
        /// [无检查] 获取该结构 (<see langword="struct"/>) 属性的值。
        /// </summary>
        /// <param name="obj">结构对象的引用</param>
        /// <returns>返回该字段的值</returns>
        /// <exception cref="MemberAccessException">此属性没有 <see langword="get"/> 方法或不能访问</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public TValue? UnsafeGetValue(ref byte obj)
        {
            if (getValue is null)
            {
                throw new MemberAccessException();
            }

            return Unsafe.As<StructGetValueFunc<byte>>(getValue)(ref obj);
        }

        /// <summary>
        /// [无检查] 设置该结构 (<see langword="struct"/>) 属性的值。
        /// </summary>
        /// <param name="obj">结构对象的引用</param>
        /// <param name="value">值</param>
        /// <exception cref="MemberAccessException">此属性没有 <see langword="set"/> 方法或不能访问。</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void UnsafeSetValue(ref byte obj, TValue? value)
        {
            if (setValue is null)
            {
                throw new MemberAccessException();
            }

            Unsafe.As<StructSetValueAction<byte>>(setValue)(ref obj, value);
        }

        /// <summary>
        /// 获取该属性的值。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <returns>返回该字段的值</returns>
        /// <exception cref="TargetException"><paramref name="obj"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException">该属性即不由 <paramref name="obj"/> 的类声明也不由其继承。</exception>
        /// <exception cref="MemberAccessException">此属性没有 <see langword="get"/> 方法或不能访问</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        new public TValue? GetValue(object? obj)
        {
            if (obj is null)
            {
                throw new TargetException();
            }

            if (obj.GetType() == declaringType || declaringType.IsInstanceOfType(obj))
            {
                return UnsafeGetValue(obj);
            }
            else
            {
                throw new ArgumentException(nameof(obj));
            }
        }

        /// <summary>
        /// 设置该属性的值。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <param name="value">值</param>
        /// <exception cref="TargetException"><paramref name="obj"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException">该属性即不由 <paramref name="obj"/> 的类声明也不由其继承。</exception>
        /// <exception cref="MemberAccessException">此属性没有 <see langword="set"/> 方法或不能访问。</exception>
        /// <exception cref="InvalidCastException"><paramref name="value"/> 不能存储在该属性中。</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SetValue(object? obj, TValue? value)
        {
            if (obj is null)
            {
                throw new TargetException();
            }

            if (obj.GetType() == declaringType || declaringType.IsInstanceOfType(obj))
            {
                UnsafeSetValue(obj, value);
            }
            else
            {
                throw new ArgumentException(nameof(obj));
            }
        }

        private protected override object? InternalGetValue(object? obj)
        {
            return GetValue(obj);
        }

        private protected override void InternalSetValue(object? obj, object? value)
        {
            SetValue(obj, (TValue?)value);
        }

        internal override XPropertyInfo WithAttribute(RWFieldAttribute attribute)
        {
            return new XInstancePropertyInfo<TValue>(this, attribute);
        }

        void IXFieldRW.OnReadValue(object obj, IValueWriter valueWriter)
        {
            if (canRead)
            {
                if (valueInterface is not null)
                {
                    valueInterface.WriteValue(valueWriter, UnsafeGetValue(obj));
                }
                else
                {
                    ValueInterface.WriteValue(valueWriter, UnsafeGetValue(obj));
                }
            }
            else
            {
                XHelper.OnCannotReadValue(this, valueWriter);
            }
        }

        void IXFieldRW.OnWriteValue(object obj, IValueReader valueReader)
        {
            if (canWrite)
            {
                if (valueInterface is not null)
                {
                    UnsafeSetValue(obj, valueInterface.ReadValue(valueReader));
                }
                else
                {
                    UnsafeSetValue(obj, ValueInterface.ReadValue<TValue>(valueReader));
                }
            }
            else
            {
                XHelper.OnCannotWriteValue(this, valueReader);
            }
        }

        void IXFieldRW.OnReadAll(object obj, IDataWriter<string> dataWriter)
        {
            if (!canRead)
            {
                return;
            }

            if (flags.On(XBindingFlags.RWMembersOptIn) && attribute is null)
            {
                return;
            }

            var value = UnsafeGetValue(obj);

            if (skipDefaultValue && TypeHelper.IsEmptyValue(value))
            {
                return;
            }

            if (valueInterface is not null)
            {
                valueInterface.WriteValue(dataWriter[name], value);
            }
            else
            {
                ValueInterface.WriteValue(dataWriter[name], value);
            }
        }

        void IXFieldRW.OnWriteAll(object obj, IDataReader<string> dataReader)
        {
            if (!canWrite)
            {
                return;
            }

            if (valueInterface is not null)
            {
                UnsafeSetValue(obj, valueInterface.ReadValue(dataReader[name]));
            }
            else
            {
                UnsafeSetValue(obj, ValueInterface.ReadValue<TValue>(dataReader[name]));
            }
        }

        IValueRW IXFieldRW.CreateValueRW(XObjectRW baseRW)
        {
            return new ValueRW(this, baseRW);
        }

        sealed class ValueRW : BaseGenericRW<TValue>, IValueRW<TValue>
        {
            readonly XInstancePropertyInfo<TValue> propertyInfo;
            readonly XObjectRW baseRW;

            public ValueRW(XInstancePropertyInfo<TValue> propertyInfo, XObjectRW baseRW)
            {
                this.propertyInfo = propertyInfo;
                this.baseRW = baseRW;
            }

            public override TValue? ReadValue()
            {
                if (baseRW.content is null)
                {
                    throw new NullReferenceException(nameof(baseRW.Content));
                }

                if (propertyInfo.canRead)
                {
                    return XHelper.CannotReadValue<TValue>(propertyInfo);
                }

                return propertyInfo.UnsafeGetValue(baseRW.content);
            }

            public override void WriteValue(TValue? value)
            {
                if (baseRW.content is null)
                {
                    throw new NullReferenceException(nameof(baseRW.Content));
                }

                if (propertyInfo.canWrite)
                {
                    XHelper.CannotWriteValue(propertyInfo);
                }
                else
                {
                    propertyInfo.UnsafeSetValue(baseRW.content, value);
                }
            }
        }
    }
}