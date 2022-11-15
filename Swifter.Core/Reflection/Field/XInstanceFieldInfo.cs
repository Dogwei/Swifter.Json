using Swifter.RW;
using Swifter.Tools;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Reflection
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public sealed class XInstanceFieldInfo<TValue> : XFieldInfo, IXFieldRW
    {
        private readonly int offset;
        private readonly Type declaringType;

        new private readonly IValueInterface<TValue>? valueInterface;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <param name="flags"></param>
        public XInstanceFieldInfo(FieldInfo fieldInfo, XBindingFlags flags)
            : base(fieldInfo, typeof(TValue), flags)
        {
            offset = TypeHelper.OffsetOf(fieldInfo);
            declaringType = fieldInfo.DeclaringType!;
        }

        private XInstanceFieldInfo(XInstanceFieldInfo<TValue> baseInfo, RWFieldAttribute attribute)
            : base(baseInfo, attribute)
        {
            offset = baseInfo.offset;
            declaringType = baseInfo.declaringType;

            attribute.GetBestMatchInterfaceMethod(typeof(TValue), out var firstArgument, out var readValueMethod, out var writeValueMethod);

            if (readValueMethod is not null || writeValueMethod is not null)
            {
                valueInterface = (IValueInterface<TValue>)XHelper.GetValueInterface(typeof(TValue), firstArgument, readValueMethod, writeValueMethod);
            }
        }

        /// <summary>
        /// [无检查] 获取该结构 (<see langword="struct"/>) 字段的值的引用。
        /// </summary>
        /// <param name="obj">结构对象的引用</param>
        /// <returns>返回字段的值的引用</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ref TValue? UnsafeGetReference(ref byte obj)
            => ref TypeHelper.AddByteOffset(ref Unsafe.As<byte, TValue?>(ref obj), offset);

        /// <summary>
        /// [无检查] 获取该字段的值的引用。
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>返回字段的值的引用</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ref TValue? UnsafeGetReference(object obj)
            => ref TypeHelper.AddByteOffset(ref TypeHelper.Unbox<TValue?>(obj), offset);

        /// <summary>
        /// 获取该字段的值的引用。
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>返回字段的值的引用</returns>
        /// <exception cref="ArgumentException">对象不是字段的定义类的类型</exception>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public ref TValue? GetReference(object obj)
        {
            if (obj.GetType() == declaringType || declaringType.IsInstanceOfType(obj))
            {
                return ref UnsafeGetReference(obj);
            }
            else
            {
                throw new ArgumentException(nameof(obj));
            }
        }

        private protected override object? InternalGetValue(object? obj)
        {
            if (obj is null)
            {
                throw new TargetException();
            }

            return GetReference(obj);
        }

        private protected override void InternalSetValue(object? obj, object? value)
        {
            if (obj is null)
            {
                throw new TargetException();
            }

            GetReference(obj) = (TValue?)value;
        }

        internal override XFieldInfo WithAttribute(RWFieldAttribute attribute)
        {
            return new XInstanceFieldInfo<TValue>(this, attribute);
        }

        void IXFieldRW.OnReadValue(object obj, IValueWriter valueWriter)
        {
            if (canRead)
            {
                if (valueInterface is not null)
                {
                    valueInterface.WriteValue(valueWriter, UnsafeGetReference(obj));
                }
                else
                {
                    ValueInterface.WriteValue(valueWriter, UnsafeGetReference(obj));
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
                    UnsafeGetReference(obj) = valueInterface.ReadValue(valueReader);
                }
                else
                {
                    UnsafeGetReference(obj) = ValueInterface.ReadValue<TValue>(valueReader);
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

            var value = UnsafeGetReference(obj);

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
                UnsafeGetReference(obj) = valueInterface.ReadValue(dataReader[name]);
            }
            else
            {
                UnsafeGetReference(obj) = ValueInterface.ReadValue<TValue>(dataReader[name]);
            }
        }

        IValueRW IXFieldRW.CreateValueRW(XObjectRW baseRW)
        {
            return new ValueRW(this, baseRW);
        }

        sealed class ValueRW : BaseGenericRW<TValue>, IValueRW<TValue>
        {
            readonly XInstanceFieldInfo<TValue> fieldInfo;
            readonly XObjectRW baseRW;

            public ValueRW(XInstanceFieldInfo<TValue> fieldInfo, XObjectRW baseRW)
            {
                this.fieldInfo = fieldInfo;
                this.baseRW = baseRW;
            }

            public override TValue? ReadValue()
            {
                if (baseRW.content is null)
                {
                    throw new NullReferenceException(nameof(baseRW.Content));
                }

                if (fieldInfo.canRead)
                {
                    return XHelper.CannotReadValue<TValue>(fieldInfo);
                }

                return fieldInfo.UnsafeGetReference(baseRW.content);
            }

            public override void WriteValue(TValue? value)
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
                    fieldInfo.UnsafeGetReference(baseRW.content) = value;
                }
            }
        }
    }
}