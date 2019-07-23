using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Reflection;
using System.Reflection.Emit;

using static Swifter.RW.StaticFastObjectRW;

namespace Swifter.RW
{
    partial class StaticFastObjectRW<T>
    {
        internal sealed class FastProperty : BaseField
        {
            public PropertyInfo Property => (PropertyInfo)Member;

            public FastProperty(PropertyInfo property, RWFieldAttribute attribute)
                : base(property, attribute)
            {
            }

            public override string Name
            {
                get
                {
                    if (Attribute != null && Attribute.Name != null)
                    {
                        return Attribute.Name;
                    }

                    return Property.Name;
                }
            }

            public override bool CanRead
            {
                get
                {
                    if (!Property.CanRead)
                    {
                        return false;
                    }

                    var method = GetMethod;

                    if (method == null)
                    {
                        return false;
                    }

                    if (Attribute != null)
                    {
                        return Attribute.Access == RWFieldAccess.RW || Attribute.Access == RWFieldAccess.ReadOnly;
                    }

                    return method.IsPublic;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    MethodInfo method;

                    if (IsByRef)
                    {
                        if (!Property.CanRead)
                        {
                            return false;
                        }

                        method = GetMethod;
                    }
                    else
                    {
                        if (!Property.CanWrite)
                        {
                            return false;
                        }

                        method = SetMethod;
                    }

                    if (method == null)
                    {
                        return false;
                    }

                    if (Attribute != null)
                    {
                        return Attribute.Access == RWFieldAccess.RW || Attribute.Access == RWFieldAccess.WriteOnly;
                    }

                    return method.IsPublic;
                }
            }
            
            public MethodInfo GetMethod => Property.GetGetMethod(true);

            public MethodInfo SetMethod => Property.GetSetMethod(true);

            public override bool IsPublic => (!CanRead || GetMethod.IsPublic) && (!CanWrite || SetMethod.IsPublic);

            public override Type BeforeType => IsByRef ? Property.PropertyType.GetElementType() : Property.PropertyType.IsPointer ? typeof(IntPtr) : Property.PropertyType;

            public override Type AfterType => ReadValueMethod?.ReturnType ?? BeforeType;

            public override bool IsStatic => (GetMethod != null && GetMethod.IsStatic) || (SetMethod != null && SetMethod.IsStatic);

            public bool IsByRef => Property.PropertyType.IsByRef;

            public override void GetValueAfter(ILGenerator ilGen)
            {
                if (IsByRef)
                {
                    ilGen.Call(GetMethod);

                    ilGen.LoadValue(BeforeType);
                }
                else
                {
                    ilGen.Call(GetMethod);
                }
            }

            public override void SetValueAfter(ILGenerator ilGen)
            {
                if (IsByRef)
                {
                    ilGen.StoreValue(BeforeType);
                }
                else
                {
                    ilGen.Call(SetMethod);
                }
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

                if (IsByRef)
                {
                    ilGen.Call(GetMethod);
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

                        ilGen.CastClass( BeforeType);

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
                    var method = typeof(IValueReader).GetMethod(methodName);

                    if (methodName == nameof(IValueReader.ReadNullable) && method.IsGenericMethodDefinition)
                    {
                        method = method.MakeGenericMethod(Nullable.GetUnderlyingType(BeforeType));
                    }

                    ilGen.Call(method);

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