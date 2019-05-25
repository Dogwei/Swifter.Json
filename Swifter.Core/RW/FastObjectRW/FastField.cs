using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Reflection;
using System.Reflection.Emit;

using static Swifter.RW.StaticFastObjectRW;

namespace Swifter.RW
{

    internal static partial class StaticFastObjectRW<T>
    {
        internal sealed class FastField : BaseField
        {
            public FieldInfo Field => (FieldInfo)Member;


            public FastField(FieldInfo field, RWFieldAttribute attribute)
                : base(field, attribute)
            {
            }

            public override string Name => Attribute?.Name ?? Field.Name;

            public override bool CanRead => Attribute != null ? (Attribute.Access & RWFieldAccess.ReadOnly) != 0 : Field.IsPublic;

            public override bool CanWrite => Attribute != null ? (Attribute.Access & RWFieldAccess.WriteOnly) != 0 : Field.IsPublic;

            public override bool IsPublic => Field.IsPublic;

            public override Type BeforeType => (Field.FieldType.IsPointer || Field.FieldType.IsByRef) ? typeof(IntPtr) : Field.FieldType;

            public override Type AfterType => ReadValueMethod?.ReturnType ?? BeforeType;

            public override bool IsStatic => Field.IsStatic;

            public override void GetValueAfter(ILGenerator ilGen)
            {
                ilGen.LoadField(Field);
            }

            public override void SetValueAfter(ILGenerator ilGen)
            {
                ilGen.StoreField(Field);
            }

            public override void GetValueBefore(ILGenerator ilGen)
            {
                if (!IsStatic)
                {
                    LoadContent(ilGen);
                }
            }

            public override void SetValueBefore(ILGenerator ilGen)
            {
                if (!IsStatic)
                {
                    LoadContent(ilGen);
                }
            }

            public override void ReadValueBefore(ILGenerator ilGen)
            {
                if (ReadValueMethod != null)
                {
                    if (ReadValueMethod.IsStatic)
                    {
                        return;
                    }

                    var index = Array.IndexOf(Fields, this);

                    ilGen.LoadConstant(index);

                    ilGen.Call(GetValueInterfaceInstanceMethod);

                    return;
                }
            }

            public override void ReadValueAfter(ILGenerator ilGen)
            {
                if (ReadValueMethod != null)
                {
                    ilGen.Call(ReadValueMethod);

                    if (BeforeType != AfterType)
                    {
                        if (AfterType.IsValueType)
                        {
                            ilGen.Box(AfterType);
                        }

                        ilGen.CastClass(BeforeType);

                        if (BeforeType.IsValueType)
                        {
                            ilGen.UnboxAny(BeforeType);
                        }
                    }

                    return;
                }

                var methodName = GetReadValueMethodName(BeforeType);
                
                if (methodName != null)
                {
                    ilGen.Call(typeof(IValueReader).GetMethod(methodName));

                    return;
                }

                var valueInterfaceType = typeof(ValueInterface<>).MakeGenericType(BeforeType);

                var valueInterfaceReadValueMethod = valueInterfaceType.GetMethod(nameof(ValueInterface<object>.ReadValue), StaticDeclaredOnly);

                ilGen.Call(valueInterfaceReadValueMethod);
            }

            public override void WriteValueBefore(ILGenerator ilGen)
            {
                if (WriteValueMethod != null)
                {
                    if (ReadValueMethod.IsStatic)
                    {
                        return;
                    }

                    var index = Array.IndexOf(Fields, this);

                    ilGen.LoadConstant(index);

                    ilGen.Call(GetValueInterfaceInstanceMethod);

                    return;
                }
            }

            public override void WriteValueAfter(ILGenerator ilGen)
            {
                if (WriteValueMethod != null)
                {
                    if (BeforeType != AfterType)
                    {
                        if (BeforeType.IsValueType)
                        {
                            ilGen.Box(BeforeType);
                        }

                        ilGen.CastClass(AfterType);

                        if (AfterType.IsValueType)
                        {
                            ilGen.UnboxAny(AfterType);
                        }
                    }

                    ilGen.Call(WriteValueMethod);

                    return;
                }

                var methodName = GetWriteValueMethodName(BeforeType);

                if (methodName != null)
                {
                    ilGen.Call(typeof(IValueWriter).GetMethod(methodName));

                    return;
                }

                var valueInterfaceType = typeof(ValueInterface<>).MakeGenericType(BeforeType);

                var valueInterfaceWriteValueMethod = valueInterfaceType.GetMethod(nameof(ValueInterface<object>.WriteValue), StaticDeclaredOnly);

                ilGen.Call(valueInterfaceWriteValueMethod);
            }
        }
    }
}