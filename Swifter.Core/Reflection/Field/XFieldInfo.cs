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
    /// 字段信息。
    /// </summary>
    public class XFieldInfo : IXFieldRW
    {
        /// <summary>
        /// 创建 XFieldInfo 字段信息。
        /// </summary>
        /// <param name="fieldInfo">.Net 自带的 FieldInfo 字段信息。</param>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回一个 XFieldInfo 字段信息。</returns>
        public static XFieldInfo Create(FieldInfo fieldInfo, XBindingFlags flags)
        {
            var fieldType
                = fieldInfo.FieldType.IsPointer ? typeof(IntPtr)
                : fieldInfo.FieldType;

            VersionDifferences.Assert(fieldType.CanBeGenericParameter());

            if (!fieldInfo.IsLiteral && !fieldInfo.IsStatic && fieldInfo.DeclaringType is not null)
            {
                try
                {
                    return (XFieldInfo)typeof(XInstanceFieldInfo<>).MakeGenericType(fieldType)
                        .GetConstructors()
                        .First()
                        .Invoke(new object?[] { fieldInfo, flags });
                }
                catch
                {
                }
            }

            return new XFieldInfo(fieldInfo, fieldType, flags);
        }

        private protected readonly FieldInfo fieldInfo;
        private protected readonly Type fieldType;
        private protected readonly XBindingFlags flags;
        private protected readonly string name;
        private protected readonly ValueInterface valueInterface;
        private protected readonly RWFieldAttribute? attribute;
        private protected readonly bool skipDefaultValue;
        private protected readonly bool canRead;
        private protected readonly bool canWrite;


        private protected XFieldInfo(FieldInfo fieldInfo, Type fieldType, XBindingFlags flags)
        {
            this.fieldInfo = fieldInfo;
            this.fieldType = fieldType;
            this.flags = flags;

            name = fieldInfo.Name;

            valueInterface = ValueInterface.GetInterface(fieldType);

            skipDefaultValue = flags.On(XBindingFlags.RWSkipDefaultValue);

            canRead = true;
            canWrite = true;
        }

        private protected XFieldInfo(XFieldInfo baseInfo, RWFieldAttribute attribute)
        {
            fieldInfo = baseInfo.fieldInfo;
            fieldType = baseInfo.fieldType;
            flags = baseInfo.flags;

            name = attribute.Name ?? baseInfo.name;

            attribute.GetBestMatchInterfaceMethod(fieldType, out var firstArgument, out var readValueMethod, out var writeValueMethod);

            if (readValueMethod is null && writeValueMethod is null)
            {
                valueInterface = baseInfo.valueInterface;
            }
            else
            {
                valueInterface = XHelper.MakeValueInterface(
                    XHelper.GetValueInterface(fieldType, firstArgument, readValueMethod, writeValueMethod)
                    );
            }

            this.attribute = attribute;

            skipDefaultValue
                = attribute.SkipDefaultValue is not RWBoolean.None
                ? attribute.SkipDefaultValue is RWBoolean.Yes
                : baseInfo.skipDefaultValue;

            canRead = attribute.Access.On(RWFieldAccess.ReadOnly);
            canWrite = attribute.Access.On(RWFieldAccess.WriteOnly);
        }

        /// <summary>
        /// 获取 .Net 自带的 FieldInfo 字段信息。
        /// </summary>
        public FieldInfo FieldInfo => fieldInfo;

        /// <summary>
        /// 获取此字段的名称。
        /// </summary>
        public string Name => name;

        /// <summary>
        /// 获取字段的类型。
        /// </summary>
        public Type FieldType => fieldType;

        /// <summary>
        /// 获取该字段的值。
        /// </summary>
        /// <param name="obj">实例。如果为静态字段，此参数则忽略</param>
        /// <returns>返回该字段的值</returns>
        /// <exception cref="TargetException">此字段非静态，并且 <paramref name="obj"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException">该字段即不由 <paramref name="obj"/> 的类声明也不由其继承。</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public object? GetValue(object? obj)
        {
            return InternalGetValue(obj);
        }

        /// <summary>
        /// 设置该字段的值。
        /// </summary>
        /// <param name="obj">实例。如果为静态字段，此参数则忽略</param>
        /// <param name="value">值</param>
        /// <exception cref="TargetException">此字段非静态，并且 <paramref name="obj"/> 为 <see langword="null"/>。</exception>
        /// <exception cref="ArgumentException">该字段即不由 <paramref name="obj"/> 的类声明也不由其继承。</exception>
        /// <exception cref="InvalidCastException"><paramref name="value"/> 不能存储在该字段中。</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void SetValue(object? obj, object? value)
        {
            InternalSetValue(obj, value);
        }

        internal virtual XFieldInfo WithAttribute(RWFieldAttribute attribute)
        {
            return new XFieldInfo(this, attribute);
        }

        private protected virtual object? InternalGetValue(object? obj)
        {
            return fieldInfo.GetValue(obj);
        }

        private protected virtual void InternalSetValue(object? obj, object? value)
        {
            fieldInfo.SetValue(obj, value);
        }

        bool IObjectField.CanRead => canRead;

        bool IObjectField.CanWrite => canWrite;

        bool IObjectField.IsPublic => fieldInfo.IsPublic;

        bool IObjectField.IsStatic => fieldInfo.IsStatic;

        int IObjectField.Order => attribute is not null ? attribute.Order : RWFieldAttribute.DefaultOrder;

        MemberInfo IObjectField.MemberInfo => fieldInfo;

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
                valueInterface.Write(valueWriter, fieldInfo.GetValue(obj));
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
                fieldInfo.SetValue(obj, valueInterface.Read(valueReader));
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

            var value = fieldInfo.GetValue(obj);

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

            fieldInfo.SetValue(obj, valueInterface.Read(dataReader[name]));
        }

        IValueRW IXFieldRW.CreateValueRW(XObjectRW baseRW)
        {
            return new ValueRW(this, baseRW);
        }

        sealed class ValueRW : BaseDirectRW
        {
            readonly XFieldInfo fieldInfo;
            readonly XObjectRW baseRW;

            public ValueRW(XFieldInfo fieldInfo, XObjectRW baseRW)
            {
                this.fieldInfo = fieldInfo;
                this.baseRW = baseRW;
            }

            public override Type ValueType => fieldInfo.fieldType;

            public override object? DirectRead()
            {
                if (baseRW.content is null)
                {
                    throw new NullReferenceException(nameof(baseRW.Content));
                }

                if (fieldInfo.canRead)
                {
                    return XHelper.CannotReadValue(fieldInfo);
                }

                return fieldInfo.fieldInfo.GetValue(baseRW.content);
            }

            public override void DirectWrite(object? value)
            {
                if (baseRW.content is null)
                {
                    throw new NullReferenceException(nameof(baseRW.Content));
                }

                if (fieldInfo.canWrite)
                {
                    XHelper.CannotWriteValue(fieldInfo);
                }
                else
                {
                    fieldInfo.fieldInfo.SetValue(baseRW.content, value);
                }
            }
        }
    }
}