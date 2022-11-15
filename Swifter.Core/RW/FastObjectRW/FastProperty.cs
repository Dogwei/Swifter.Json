
using InlineIL;
using Swifter.Tools;

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

            readonly FieldInfo? AutoFieldInfo;

            public FastProperty(PropertyInfo property, RWFieldAttribute? attribute)
                : base(property, attribute)
            {
                if (TypeHelper.IsAutoProperty(property, out var fieldInfo) && fieldInfo != null)
                {
                    AutoFieldInfo = fieldInfo;
                }
            }
            public PropertyInfo Property => (PropertyInfo)Member;

            public bool IsByRef => Property.PropertyType.IsByRef;

            public bool IsPointer => Property.PropertyType.IsPointer;

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
                    // 没有读取方式。
                    if (GetMethod is null && AutoFieldInfo is null)
                    {
                        return false;
                    }

                    // 特性指定。
                    if (Attribute != null)
                    {
                        return Attribute.Access.On(RWFieldAccess.ReadOnly);
                    }

                    // ref struct
                    if (FieldType.IsByRefLike())
                    {
                        return false;
                    }

                    // 公开的 Get 方法。
                    if (GetMethod?.IsPublic == true)
                    {
                        return true;
                    }

                    // 自动属性 Set 方法可访问。
                    if (Options.On(FastObjectRWOptions.AutoPropertyDirectRW))
                    {
                        return SetMethod?.IsPublic == true;
                    }

                    return false;
                }
            }

            public override bool CanWrite
            {
                get
                {
                    // ByRef 属性判断 Get 方法是否可读。
                    if (IsByRef)
                    {
                        return CanRead;
                    }

                    // 没有设置方式。
                    if (SetMethod is null && AutoFieldInfo is null)
                    {
                        return false;
                    }

                    // 特性指定。
                    if (Attribute != null)
                    {
                        return Attribute.Access.On(RWFieldAccess.WriteOnly);
                    }

                    // ref struct
                    if (FieldType.IsByRefLike())
                    {
                        return false;
                    }

                    // 公开的 Set 方法。
                    if (SetMethod?.IsPublic == true)
                    {
                        return true;
                    }

                    // 自动属性 Get 方法可访问。
                    if (Options.On(FastObjectRWOptions.AutoPropertyDirectRW))
                    {
                        return GetMethod?.IsPublic == true;
                    }

                    return false;
                }
            }

            public MethodInfo? GetMethod => Property.GetGetMethod(true);

            public MethodInfo? SetMethod => Property.GetSetMethod(true);

            public override bool IsPublicGet => true;

            public override bool IsPublicSet => true;

            public override Type FieldType =>
                IsByRef ? Property.PropertyType.GetElementType()! :
                IsPointer ? typeof(IntPtr) :
                Property.PropertyType;

            public override Type ReadType => ReadValueMethod?.ReturnType ?? FieldType;

            public override Type WriteType => WriteValueMethod?.GetParameters().First().ParameterType ?? FieldType;

            public override bool IsStatic => (GetMethod != null && GetMethod.IsStatic) || (SetMethod != null && SetMethod.IsStatic);

            public override bool SkipDefaultValue => Attribute != null && Attribute.SkipDefaultValue != RWBoolean.None ? (Attribute.SkipDefaultValue == RWBoolean.Yes) : Options.On(FastObjectRWOptions.SkipDefaultValue);

            public override bool CannotGetException => Attribute != null && Attribute.CannotGetException != RWBoolean.None ? (Attribute.CannotGetException == RWBoolean.Yes) : Options.On(FastObjectRWOptions.CannotGetException);

            public override bool CannotSetException => Attribute != null && Attribute.CannotSetException != RWBoolean.None ? (Attribute.CannotSetException == RWBoolean.Yes) : Options.On(FastObjectRWOptions.CannotSetException);


            public override void GetValueAfter(ILGenerator ilGen)
            {
                if (IsByRef)
                {
                    ilGen.AutoCall(GetMethod!);

                    ilGen.LoadValue(FieldType);
                }
                else
                {
                    if (GetMethod != null)
                    {
                        if (GetMethod.IsExternalVisible() == true || DynamicAssembly.CanAccessNonPublicMembers || IsVisibleTo)
                        {
                            ilGen.AutoCall(GetMethod);
                        }
                        else
                        {
                            ilGen.UnsafeCall(GetMethod);
                        }
                    }
                    else if (AutoFieldInfo != null)
                    {
                        if (AutoFieldInfo.IsExternalVisible())
                        {
                            ilGen.LoadField(AutoFieldInfo);
                        }
                        else
                        {
                            ilGen.UnsafeLoadField(AutoFieldInfo);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }
            }

            public override void SetValueAfter(ILGenerator ilGen)
            {
                if (IsByRef)
                {
                    ilGen.StoreValue(FieldType);
                }
                else
                {
                    if (SetMethod != null)
                    {
                        if (SetMethod.IsExternalVisible() || DynamicAssembly.CanAccessNonPublicMembers || IsVisibleTo)
                        {
                            ilGen.AutoCall(SetMethod);
                        }
                        else
                        {
                            ilGen.UnsafeCall(SetMethod);
                        }
                    }
                    else if (AutoFieldInfo != null)
                    {
                        if (AutoFieldInfo.IsExternalVisible())
                        {
                            ilGen.StoreField(AutoFieldInfo);
                        }
                        else
                        {
                            ilGen.UnsafeStoreField(AutoFieldInfo);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
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
                    ilGen.AutoCall(GetMethod!);
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

                    ilGen.LoadConstant(Array.IndexOf(Fields, this));

                    ilGen.AutoCall(GetGetValueInterfaceInstanceMethod());
                }
            }

            public override void ReadValueAfter(ILGenerator ilGen)
            {
                if (ReadValueMethod != null)
                {
                    ilGen.AutoCall(ReadValueMethod);

                    if (FieldType != ReadType)
                    {
                        if (ReadType.IsValueType)
                        {
                            ilGen.Box(ReadType);
                        }

                        ilGen.CastClass(FieldType);

                        if (FieldType.IsValueType)
                        {
                            ilGen.UnboxAny(FieldType);
                        }
                    }
                }
                else if (Options.On(FastObjectRWOptions.BasicTypeDirectCallMethod) && GetReadValueMethod(FieldType) is MethodInfo method)
                {
                    ilGen.AutoCall(method);
                }
                else
                {
                    ilGen.AutoCall(ValueInterfaceReadValueMethod.MakeGenericMethod(FieldType));
                }
            }

            public override void WriteValueBefore(ILGenerator ilGen)
            {
                if (WriteValueMethod != null && !WriteValueMethod.IsStatic)
                {
                    ilGen.LoadConstant(Array.IndexOf(Fields, this));

                    ilGen.AutoCall(GetGetValueInterfaceInstanceMethod());
                }
            }

            public override void WriteValueAfter(ILGenerator ilGen)
            {
                if (WriteValueMethod != null)
                {
                    if (FieldType != WriteType)
                    {
                        if (FieldType.IsValueType)
                        {
                            ilGen.Box(FieldType);
                        }

                        ilGen.CastClass(WriteType);

                        if (WriteType.IsValueType)
                        {
                            ilGen.UnboxAny(WriteType);
                        }
                    }

                    ilGen.AutoCall(WriteValueMethod);
                }
                else if (Options.On(FastObjectRWOptions.BasicTypeDirectCallMethod) && GetWriteValueMethod(FieldType) is MethodInfo method)
                {
                    ilGen.AutoCall(method);
                }
                else
                {
                    ilGen.AutoCall(ValueInterfaceWriteValueMethod.MakeGenericMethod(FieldType));
                }
            }
        }
    }
}