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
    /// 属性信息。
    /// </summary>
    public class XPropertyInfo : IXFieldRW
    {
        /// <summary>
        /// 创建 XPropertyInfo 属性信息。
        /// </summary>
        /// <param name="propertyInfo">.Net 自带的 PropertyInfo 属性信息</param>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回 XPropertyInfo 属性信息。</returns>
        public static XPropertyInfo Create(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            var propertyType
                = propertyInfo.PropertyType.IsPointer ? typeof(IntPtr)
                : propertyInfo.PropertyType.IsByRef ? propertyInfo.PropertyType.GetElementType()!
                : propertyInfo.PropertyType;

            VersionDifferences.Assert(propertyType.CanBeGenericParameter());

            if (!propertyInfo.IsStatic() && !propertyType.IsByRefLike() && propertyInfo.DeclaringType is not null)
            {
                try
                {
                    return (XPropertyInfo)typeof(XInstancePropertyInfo<>).MakeGenericType(propertyType)
                        .GetConstructors()
                        .First()
                        .Invoke(new object?[] { propertyInfo, flags });
                }
                catch
                {
                }
            }

            return new XPropertyInfo(propertyInfo, propertyType, flags);
        }

        private protected readonly PropertyInfo propertyInfo;
        private protected readonly Type propertyType;
        private protected readonly XBindingFlags flags;
        private protected readonly string name;
        private protected readonly ValueInterface valueInterface;
        private protected readonly RWFieldAttribute? attribute;
        private protected readonly bool skipDefaultValue;

        private protected bool canRead;
        private protected bool canWrite;

        private protected XPropertyInfo(PropertyInfo propertyInfo, Type propertyType, XBindingFlags flags)
        {
            this.propertyInfo = propertyInfo;
            this.propertyType = propertyType;
            this.flags = flags;

            name = propertyInfo.Name;

            valueInterface = ValueInterface.GetInterface(propertyType);

            skipDefaultValue = flags.On(XBindingFlags.RWSkipDefaultValue);

            canRead = propertyInfo.GetGetMethod(flags.On(XBindingFlags.NonPublic)) is not null;
            canWrite = propertyInfo.GetSetMethod(flags.On(XBindingFlags.NonPublic)) is not null;
        }

        private protected XPropertyInfo(XPropertyInfo baseInfo, RWFieldAttribute attribute)
        {
            propertyInfo = baseInfo.propertyInfo;
            propertyType = baseInfo.propertyType;
            flags = baseInfo.flags;

            name = attribute.Name ?? baseInfo.name;

            attribute.GetBestMatchInterfaceMethod(propertyType, out var firstArgument, out var readValueMethod, out var writeValueMethod);

            if (readValueMethod is null && writeValueMethod is null)
            {
                valueInterface = baseInfo.valueInterface;
            }
            else
            {
                valueInterface = XHelper.MakeValueInterface(
                    XHelper.GetValueInterface(propertyType, firstArgument, readValueMethod, writeValueMethod)
                    );
            }

            this.attribute = attribute;

            skipDefaultValue
                = attribute.SkipDefaultValue is not RWBoolean.None
                ? attribute.SkipDefaultValue is RWBoolean.Yes
                : baseInfo.skipDefaultValue;

            canRead = propertyInfo.GetGetMethod(true) is not null && attribute.Access.On(RWFieldAccess.ReadOnly);
            canWrite = propertyInfo.GetSetMethod(true) is not null && attribute.Access.On(RWFieldAccess.WriteOnly);
        }

        /// <summary>
        /// 获取 .Net 自带的 PropertyInfo 属性信息。
        /// </summary>
        public PropertyInfo PropertyInfo => propertyInfo;

        /// <summary>
        /// 获取此属性的名称。
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 表示该属性能否读取。
        /// </summary>
        public bool CanRead => canRead;

        /// <summary>
        /// 表示该属性能否写入。
        /// </summary>
        public bool CanWrite => canWrite;

        /// <summary>
        /// 获取该属性的值。
        /// </summary>
        /// <param name="obj">实例。如果为静态属性，此参数则忽略</param>
        /// <returns>返回该字段的值</returns>
        /// <exception cref="TargetException">此属性非静态，并且 <paramref name="obj"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException">该属性即不由 <paramref name="obj"/> 的类声明也不由其继承。</exception>
        /// <exception cref="MemberAccessException">此属性没有 <see langword="get"/> 方法或不能访问</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object? GetValue(object? obj)
        {
            return InternalGetValue(obj);
        }

        /// <summary>
        /// 设置该属性的值。
        /// </summary>
        /// <param name="obj">实例。如果为静态属性，此参数则忽略</param>
        /// <param name="value">值</param>
        /// <exception cref="TargetException">此属性非静态，并且 <paramref name="obj"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException">该属性即不由 <paramref name="obj"/> 的类声明也不由其继承。</exception>
        /// <exception cref="MemberAccessException">此属性没有 <see langword="set"/> 方法或不能访问。</exception>
        /// <exception cref="InvalidCastException"><paramref name="value"/> 不能存储在该属性中。</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SetValue(object? obj, object? value)
        {
            InternalSetValue(obj, value);
        }

        private protected virtual object? InternalGetValue(object? obj)
        {
            return propertyInfo.GetValue(obj, null);
        }

        private protected virtual void InternalSetValue(object? obj, object? value)
        {
            propertyInfo.SetValue(obj, value, null);
        }

        internal virtual XPropertyInfo WithAttribute(RWFieldAttribute attribute)
        {
            return new XPropertyInfo(this, attribute);
        }

        Type IObjectField.FieldType => propertyType;

        bool IObjectField.IsPublic => propertyInfo.IsPublic();

        bool IObjectField.IsStatic => propertyInfo.IsStatic();

        int IObjectField.Order => attribute is not null ? attribute.Order : RWFieldAttribute.DefaultOrder;

        MemberInfo IObjectField.MemberInfo => propertyInfo;

        bool IObjectField.SkipDefaultValue
            => skipDefaultValue;

        bool IObjectField.CannotGetException
            => attribute is not null && attribute.CannotGetException is not RWBoolean.None
            ? attribute.CannotGetException is RWBoolean.Yes
            : flags.On(XBindingFlags.RWCannotGetException);

        bool IObjectField.CannotSetException
            => attribute is not null && attribute.CannotSetException is not RWBoolean.None
            ? attribute.CannotSetException is RWBoolean.Yes
            : flags.On(XBindingFlags.RWCannotSetException);

        void IXFieldRW.OnReadValue(object obj, IValueWriter valueWriter)
        {
            if (canRead)
            {
                valueInterface.Write(valueWriter, propertyInfo.GetValue(obj, null));
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
                propertyInfo.SetValue(obj, valueInterface.Read(valueReader), null);
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

            var value = propertyInfo.GetValue(obj, null);

            if (skipDefaultValue && TypeHelper.IsEmptyValue(value))
            {
                return;
            }

            valueInterface.Write(dataWriter[name], value);
        }

        void IXFieldRW.OnWriteAll(object obj, IDataReader<string> dataReader)
        {
            if (!canWrite)
            {
                return;
            }

            propertyInfo.SetValue(obj, valueInterface.Read(dataReader[name]), null);
        }

        IValueRW IXFieldRW.CreateValueRW(XObjectRW baseRW)
        {
            return new ValueRW(this, baseRW);
        }

        sealed class ValueRW : BaseDirectRW
        {
            readonly XPropertyInfo propertyInfo;
            readonly XObjectRW baseRW;

            public ValueRW(XPropertyInfo propertyInfo, XObjectRW baseRW)
            {
                this.propertyInfo = propertyInfo;
                this.baseRW = baseRW;
            }

            public override Type ValueType => propertyInfo.propertyType;

            public override object? DirectRead()
            {
                if (baseRW.content is null)
                {
                    throw new NullReferenceException(nameof(baseRW.Content));
                }

                if (propertyInfo.canRead)
                {
                    return XHelper.CannotReadValue(propertyInfo);
                }

                return propertyInfo.propertyInfo.GetValue(baseRW.content, null);
            }

            public override void DirectWrite(object? value)
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
                    propertyInfo.propertyInfo.SetValue(baseRW.content, value, null);
                }
            }
        }
    }
}