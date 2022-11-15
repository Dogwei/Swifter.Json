
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
        internal sealed class FastField : BaseField
        {
            public FieldInfo Field => (FieldInfo)Member;

            public FastField(FieldInfo field, RWFieldAttribute? attribute)
                : base(field, attribute)
            {
            }

            public override string Name => Attribute?.Name ?? Field.Name;

            public override bool CanRead => Attribute != null ? Attribute.Access.On(RWFieldAccess.ReadOnly) : Field.IsPublic;

            public override bool CanWrite => Attribute != null ? Attribute.Access.On(RWFieldAccess.WriteOnly) : Field.IsPublic;

            public override bool IsPublicGet => true;

            public override bool IsPublicSet => true;

            public override Type FieldType => Field.FieldType.IsPointer ? typeof(IntPtr) : Field.FieldType;

            public override Type ReadType => ReadValueMethod?.ReturnType ?? FieldType;

            public override Type WriteType => WriteValueMethod?.GetParameters().First().ParameterType ?? FieldType;

            public override bool IsStatic => Field.IsStatic;

            public override bool SkipDefaultValue => Attribute != null && Attribute.SkipDefaultValue != RWBoolean.None ? (Attribute.SkipDefaultValue == RWBoolean.Yes) : Options.On(FastObjectRWOptions.SkipDefaultValue);

            public override bool CannotGetException => Attribute != null && Attribute.CannotGetException != RWBoolean.None ? (Attribute.CannotGetException == RWBoolean.Yes) : Options.On(FastObjectRWOptions.CannotGetException);

            public override bool CannotSetException => Attribute != null && Attribute.CannotSetException != RWBoolean.None ? (Attribute.CannotSetException == RWBoolean.Yes) : Options.On(FastObjectRWOptions.CannotSetException);

            public override void GetValueAfter(ILGenerator ilGen)
            {
                if (Field.IsExternalVisible() || DynamicAssembly.CanAccessNonPublicMembers || IsVisibleTo)
                {
                    ilGen.LoadField(Field);
                }
                else
                {
                    ilGen.UnsafeLoadField(Field);
                }
            }

            public override void SetValueAfter(ILGenerator ilGen)
            {
                if (Field.IsExternalVisible() || DynamicAssembly.CanAccessNonPublicMembers || IsVisibleTo)
                {
                    ilGen.StoreField(Field);
                }
                else
                {
                    ilGen.UnsafeStoreField(Field);
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

                    return;
                }
            }

            public override void ReadValueAfter(ILGenerator ilGen)
            {
                if (ReadValueMethod is not null)
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