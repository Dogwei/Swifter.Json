using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;

namespace Swifter.RW
{
    internal abstract class BaseField : IObjectField, IComparable<BaseField>
    {
        public readonly MemberInfo Member;

        public readonly RWFieldAttribute? Attribute;

        public readonly MethodInfo? ReadValueMethod;
        public readonly MethodInfo? WriteValueMethod;

        public readonly object? InterfaceInstance;


        public BaseField(MemberInfo member, RWFieldAttribute? attribute)
        {
            Attribute = attribute;

            Member = member;

            attribute?.GetBestMatchInterfaceMethod(FieldType, out InterfaceInstance, out ReadValueMethod, out WriteValueMethod);
        }

        public abstract string Name { get; }

        public abstract Type FieldType { get; }

        public abstract Type ReadType { get; }

        public abstract Type WriteType { get; }

        public abstract bool CanRead { get; }

        public abstract bool CanWrite { get; }

        public abstract bool SkipDefaultValue { get; }

        public abstract bool CannotGetException { get; }

        public abstract bool CannotSetException { get; }

        public abstract bool IsPublicGet { get; }

        public abstract bool IsPublicSet { get; }

        public bool IsPublic => IsPublicGet && IsPublicSet;

        public abstract bool IsStatic { get; }

        public int Order => Attribute?.Order ?? RWFieldAttribute.DefaultOrder;

        public MemberInfo MemberInfo => Member;

        public abstract void GetValueBefore(ILGenerator ilGen);

        public abstract void GetValueAfter(ILGenerator ilGen);

        public abstract void SetValueBefore(ILGenerator ilGen);

        public abstract void SetValueAfter(ILGenerator ilGen);

        public abstract void ReadValueBefore(ILGenerator ilGen);

        public abstract void ReadValueAfter(ILGenerator ilGen);

        public abstract void WriteValueBefore(ILGenerator ilGen);

        public abstract void WriteValueAfter(ILGenerator ilGen);

        public int CompareTo([AllowNull] BaseField other)
        {
            if (other is null)
            {
                return -1;
            }

            var comparison = Order.CompareTo(other.Order);

            if (comparison is not 0)
            {
                return comparison;
            }

            return Name.CompareTo(other.Name);
        }
    }
}