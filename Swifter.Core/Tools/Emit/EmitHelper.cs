using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Swifter.Tools.MethodHelper;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供 Emit 帮助方法。
    /// </summary>
    public static partial class EmitHelper
    {
        /// <summary>
        /// Emit 内部使用函数。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TValue LoadFieldValue<TValue>(object obj, int offset)
        {
            return Underlying.As<byte, TValue>(ref Underlying.Add(ref TypeHelper.Unbox<byte>(obj), offset));
        }

        /// <summary>
        /// Emit 内部使用函数。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void StoreFieldValue<TValue>(object obj, TValue value, int offset)
        {
            Underlying.As<byte, TValue>(ref Underlying.Add(ref TypeHelper.Unbox<byte>(obj), offset)) = value;
        }

        /// <summary>
        /// Emit 内部使用函数。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TValue LoadStructFieldValue<TValue>(ref byte obj , int offset)
        {
            return Underlying.As<byte, TValue>(ref Underlying.Add(ref obj, offset));
        }

        /// <summary>
        /// Emit 内部使用函数。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void StoreStructFieldValue<TValue>(ref byte obj, TValue value, int offset)
        {
            Underlying.As<byte, TValue>(ref Underlying.Add(ref obj, offset)) = value;
        }

        /// <summary>
        /// Emit 内部使用函数。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe TValue LoadStaticFieldValue<TValue>(IntPtr address)
        {
            return Underlying.AsRef<TValue>((void*)address);
        }

        /// <summary>
        /// Emit 内部使用函数。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe void StoreStaticFieldValue<TValue>(TValue value, IntPtr address)
        {
            Underlying.AsRef<TValue>((void*)address) = value;
        }


        /// <summary>
        /// 加载字段值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="fieldInfo">字段信息</param>
        /// <param name="normal">是否只使用默认的方式</param>
        public static ILGenerator LoadField(this ILGenerator ilGen, FieldInfo fieldInfo, bool normal = false)
        {
            if (fieldInfo is FieldBuilder)
            {
                normal = true;
            }

            if (fieldInfo.IsStatic)
            {
                if (fieldInfo.IsPublic || normal || fieldInfo.IsThreadStatic())
                {
                    ilGen.Emit(OpCodes.Ldsfld, fieldInfo);
                }
                else
                {
                    ilGen.LoadConstant((long)TypeHelper.GetStaticsBasePointer(fieldInfo) + TypeHelper.OffsetOf(fieldInfo));
                    ilGen.ConvertPointer();

                    if (fieldInfo.FieldType.IsValueType)
                    {
                        ilGen.Call(typeof(EmitHelper).GetMethod(nameof(LoadStaticFieldValue)).MakeGenericMethod(fieldInfo.FieldType));
                    }
                    else
                    {
                        ilGen.Call(MethodOf<IntPtr, object>(LoadStaticFieldValue<object>));
                    }
                }
            }
            else
            {
                if (fieldInfo.IsPublic || normal)
                {
                    ilGen.Emit(OpCodes.Ldfld, fieldInfo);
                }
                else
                {
                    ilGen.LoadConstant(TypeHelper.OffsetOf(fieldInfo));

                    if (fieldInfo.DeclaringType.IsValueType && fieldInfo.FieldType.IsValueType)
                    {
                        ilGen.Call(typeof(EmitHelper).GetMethod(nameof(LoadStructFieldValue)).MakeGenericMethod(fieldInfo.FieldType));
                    }
                    else if (fieldInfo.DeclaringType.IsValueType)
                    {
                        ilGen.Call(typeof(EmitHelper).GetMethod(nameof(LoadStructFieldValue)).MakeGenericMethod(typeof(object)));
                    }
                    else if (fieldInfo.FieldType.IsValueType)
                    {
                        ilGen.Call(typeof(EmitHelper).GetMethod(nameof(LoadFieldValue)).MakeGenericMethod(fieldInfo.FieldType));
                    }
                    else
                    {
                        ilGen.Call(MethodOf<object, int, object>(LoadFieldValue<object>));
                    }
                }
            }

            return ilGen;
        }

        /// <summary>
        /// 设置实例字段值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="fieldInfo">字段信息</param>
        /// <param name="normal">是否只使用默认的方式</param>
        public static ILGenerator StoreField(this ILGenerator ilGen, FieldInfo fieldInfo, bool normal = false)
        {
            if (fieldInfo is FieldBuilder)
            {
                normal = true;
            }

            if (fieldInfo.IsStatic)
            {
                if (fieldInfo.IsPublic || normal || fieldInfo.IsThreadStatic())
                {
                    ilGen.Emit(OpCodes.Stsfld, fieldInfo);
                }
                else
                {
                    ilGen.LoadConstant((long)TypeHelper.GetStaticsBasePointer(fieldInfo) + TypeHelper.OffsetOf(fieldInfo));
                    ilGen.ConvertPointer();

                    if (fieldInfo.FieldType.IsValueType)
                    {
                        ilGen.Call(typeof(EmitHelper).GetMethod(nameof(StoreStaticFieldValue)).MakeGenericMethod(fieldInfo.FieldType));
                    }
                    else
                    {
                        ilGen.Call(MethodOf<object, IntPtr>(StoreStaticFieldValue));
                    }
                }
            }
            else
            {
                if (fieldInfo.IsPublic || normal)
                {
                    ilGen.Emit(OpCodes.Stfld, fieldInfo);
                }
                else
                {
                    ilGen.LoadConstant(TypeHelper.OffsetOf(fieldInfo));

                    if (fieldInfo.DeclaringType.IsValueType && fieldInfo.FieldType.IsValueType)
                    {
                        ilGen.Call(typeof(EmitHelper).GetMethod(nameof(StoreStructFieldValue)).MakeGenericMethod(fieldInfo.FieldType));
                    }
                    else if (fieldInfo.DeclaringType.IsValueType)
                    {
                        ilGen.Call(typeof(EmitHelper).GetMethod(nameof(StoreStructFieldValue)).MakeGenericMethod(typeof(object)));
                    }
                    else if (fieldInfo.FieldType.IsValueType)
                    {
                        ilGen.Call(typeof(EmitHelper).GetMethod(nameof(StoreFieldValue)).MakeGenericMethod(fieldInfo.FieldType));
                    }
                    else
                    {
                        ilGen.Call(MethodOf<object, object, int>(StoreFieldValue));
                    }
                }
            }

            return ilGen;
        }

        /// <summary>
        /// 加载字段地址。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="fieldInfo">字段信息</param>
        public static void LoadFieldAddress(this ILGenerator ilGen, FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic)
            {
                ilGen.Emit(OpCodes.Ldsflda, fieldInfo);
            }
            else
            {
                ilGen.Emit(OpCodes.Ldflda, fieldInfo);
            }
        }

        /// <summary>
        /// 加载类型的元数据元素。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">类型信息</param>
        public static void LoadToken(this ILGenerator ilGen, Type type)
        {
            ilGen.Emit(OpCodes.Ldtoken, type);
        }

        /// <summary>
        /// 加载类型 Type 到栈顶。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">类型信息</param>
        public static void LoadType(this ILGenerator ilGen, Type type)
        {
            ilGen.LoadToken(type);
            ilGen.Call(MethodOf<RuntimeTypeHandle, Type>(Type.GetTypeFromHandle));
        }

        /// <summary>
        /// 加载本地变量值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="localBuilder">变量信息</param>
        public static void LoadLocal(this ILGenerator ilGen, LocalBuilder localBuilder)
        {
            ilGen.Emit(OpCodes.Ldloc, localBuilder);
        }

        /// <summary>
        /// 设置本地变量值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="localBuilder">本地变量信息</param>
        public static void StoreLocal(this ILGenerator ilGen, LocalBuilder localBuilder)
        {
            ilGen.Emit(OpCodes.Stloc, localBuilder);
        }

        /// <summary>
        /// 加载本地变量地址。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="localBuilder">变量信息</param>
        public static void LoadLocalAddress(this ILGenerator ilGen, LocalBuilder localBuilder)
        {
            ilGen.Emit(OpCodes.Ldloca, localBuilder);
        }

        /// <summary>
        /// 加载 Boolean 常量。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="value">值</param>
        public static ILGenerator LoadConstant(this ILGenerator ilGen, bool value)
        {
            if (value)
            {
                ilGen.Emit(OpCodes.Ldc_I4_1);
            }
            else
            {
                ilGen.Emit(OpCodes.Ldc_I4_0);
            }

            return ilGen;
        }

        /// <summary>
        /// 加载 Int32 常量。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="value">值</param>
        public static ILGenerator LoadConstant(this ILGenerator ilGen, int value)
        {
            switch (value)
            {
                case -1:
                    ilGen.Emit(OpCodes.Ldc_I4_M1);
                    break;
                case 0:
                    ilGen.Emit(OpCodes.Ldc_I4_0);
                    break;
                case 1:
                    ilGen.Emit(OpCodes.Ldc_I4_1);
                    break;
                case 2:
                    ilGen.Emit(OpCodes.Ldc_I4_2);
                    break;
                case 3:
                    ilGen.Emit(OpCodes.Ldc_I4_3);
                    break;
                case 4:
                    ilGen.Emit(OpCodes.Ldc_I4_4);
                    break;
                case 5:
                    ilGen.Emit(OpCodes.Ldc_I4_5);
                    break;
                case 6:
                    ilGen.Emit(OpCodes.Ldc_I4_6);
                    break;
                case 7:
                    ilGen.Emit(OpCodes.Ldc_I4_7);
                    break;
                case 8:
                    ilGen.Emit(OpCodes.Ldc_I4_8);
                    break;
                default:
                    if (value >= -0x80 && value <= 0x7F)
                    {
                        ilGen.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    }
                    else
                    {
                        ilGen.Emit(OpCodes.Ldc_I4, value);
                    }
                    break;
            }

            return ilGen;
        }

        /// <summary>
        /// 加载 Int64 常量。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="value">值</param>
        public static void LoadConstant(this ILGenerator ilGen, long value)
        {
            ilGen.Emit(OpCodes.Ldc_I8, value);
        }

        /// <summary>
        /// 加载常量地址。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="value">值</param>
        public static void LoadConstant(this ILGenerator ilGen, IntPtr value)
        {
            if (IntPtr.Size == 4)
            {
                ilGen.LoadConstant((int)value);
                ilGen.ConvertPointer();
            }
            else
            {
                ilGen.LoadConstant((long)value);
                ilGen.ConvertPointer();
            }
        }

        /// <summary>
        /// 加载 String 常量。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="value">值</param>
        public static void LoadString(this ILGenerator ilGen, string value)
        {
            ilGen.Emit(OpCodes.Ldstr, value);
        }

        /// <summary>
        /// 设置类型已提供的值到提供的内存上。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">值的类型</param>
        public static void StoreValue(this ILGenerator ilGen, Type type)
        {
            if (type.IsValueType)
            {
                if (type == typeof(IntPtr) || type == typeof(UIntPtr) || type.IsPointer || type.IsPointer)
                {
                    ilGen.Emit(OpCodes.Stind_I);

                    return;
                }

                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        ilGen.Emit(OpCodes.Stind_I1);
                        break;
                    case TypeCode.Char:
                        ilGen.Emit(OpCodes.Stind_I2);
                        break;
                    case TypeCode.SByte:
                        ilGen.Emit(OpCodes.Stind_I1);
                        break;
                    case TypeCode.Byte:
                        ilGen.Emit(OpCodes.Stind_I1);
                        break;
                    case TypeCode.Int16:
                        ilGen.Emit(OpCodes.Stind_I2);
                        break;
                    case TypeCode.UInt16:
                        ilGen.Emit(OpCodes.Stind_I2);
                        break;
                    case TypeCode.Int32:
                        ilGen.Emit(OpCodes.Stind_I4);
                        break;
                    case TypeCode.UInt32:
                        ilGen.Emit(OpCodes.Stind_I4);
                        break;
                    case TypeCode.Int64:
                        ilGen.Emit(OpCodes.Stind_I8);
                        break;
                    case TypeCode.UInt64:
                        ilGen.Emit(OpCodes.Stind_I8);
                        break;
                    case TypeCode.Single:
                        ilGen.Emit(OpCodes.Stind_R4);
                        break;
                    case TypeCode.Double:
                        ilGen.Emit(OpCodes.Stind_R8);
                        break;
                    default:
                        ilGen.Emit(OpCodes.Stobj, type);
                        break;
                }
            }
            else
            {
                ilGen.Emit(OpCodes.Stind_Ref);
            }
        }

        /// <summary>
        /// 在提供的内存上加载一个类型的值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">值的类型</param>
        public static void LoadValue(this ILGenerator ilGen, Type type)
        {
            if (type.IsValueType)
            {
                if (type == typeof(IntPtr) || type == typeof(UIntPtr) || type.IsPointer || type.IsPointer)
                {
                    ilGen.Emit(OpCodes.Ldind_I);

                    return;
                }

                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean:
                        ilGen.Emit(OpCodes.Ldind_I1);
                        break;
                    case TypeCode.Char:
                        ilGen.Emit(OpCodes.Ldind_U2);
                        break;
                    case TypeCode.SByte:
                        ilGen.Emit(OpCodes.Ldind_I1);
                        break;
                    case TypeCode.Byte:
                        ilGen.Emit(OpCodes.Ldind_U1);
                        break;
                    case TypeCode.Int16:
                        ilGen.Emit(OpCodes.Ldind_I2);
                        break;
                    case TypeCode.UInt16:
                        ilGen.Emit(OpCodes.Ldind_U2);
                        break;
                    case TypeCode.Int32:
                        ilGen.Emit(OpCodes.Ldind_I4);
                        break;
                    case TypeCode.UInt32:
                        ilGen.Emit(OpCodes.Ldind_U4);
                        break;
                    case TypeCode.Int64:
                        ilGen.Emit(OpCodes.Ldind_I8);
                        break;
                    case TypeCode.UInt64:
                        ilGen.Emit(OpCodes.Ldind_I8);
                        break;
                    case TypeCode.Single:
                        ilGen.Emit(OpCodes.Ldind_R4);
                        break;
                    case TypeCode.Double:
                        ilGen.Emit(OpCodes.Ldind_R8);
                        break;
                    default:
                        ilGen.Emit(OpCodes.Ldobj, type);
                        break;
                }
            }
            else
            {
                ilGen.Emit(OpCodes.Ldind_Ref);
            }
        }

        /// <summary>
        /// 加载参数值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="index">参数序号</param>
        public static ILGenerator LoadArgument(this ILGenerator ilGen, int index)
        {
            switch (index)
            {
                case 0:
                    ilGen.Emit(OpCodes.Ldarg_0);
                    break;
                case 1:
                    ilGen.Emit(OpCodes.Ldarg_1);
                    break;
                case 2:
                    ilGen.Emit(OpCodes.Ldarg_2);
                    break;
                case 3:
                    ilGen.Emit(OpCodes.Ldarg_3);
                    break;
                default:
                    if (index >= 4 && index <= 255)
                    {
                        ilGen.Emit(OpCodes.Ldarg_S, (byte)index);
                    }
                    else
                    {
                        ilGen.Emit(OpCodes.Ldarg, index);
                    }
                    break;
            }

            return ilGen;
        }

        /// <summary>
        /// 设置参数值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="index">参数序号</param>
        public static void StoreArgument(this ILGenerator ilGen, int index)
        {
            if (index >= 0 && index <= 255)
            {
                ilGen.Emit(OpCodes.Starg_S, (byte)index);
            }
            else
            {
                ilGen.Emit(OpCodes.Starg, index);
            }
        }

        /// <summary>
        /// 加载参数地址。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="index">参数序号</param>
        public static void LoadArgumentAddress(this ILGenerator ilGen, int index)
        {
            if (index >= 0 && index <= 255)
            {
                ilGen.Emit(OpCodes.Ldarga_S, (byte)index);
            }
            else
            {
                ilGen.Emit(OpCodes.Ldarga, index);
            }
        }

        /// <summary>
        /// 加载提供的数组位于提供索引出的元素的地址。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="elementType">元素的类型</param>
        public static void LoadElementAddress(this ILGenerator ilGen, Type elementType)
        {
            ilGen.Emit(OpCodes.Ldelema, elementType);
        }

        /// <summary>
        /// 加载提供的数组位于提供索引出的元素。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="elementType">元素的类型</param>
        public static void LoadElement(this ILGenerator ilGen, Type elementType)
        {
            switch (Type.GetTypeCode(elementType))
            {
                case TypeCode.Boolean:
                    ilGen.Emit(OpCodes.Ldelem_U1);
                    return;
                case TypeCode.Char:
                    ilGen.Emit(OpCodes.Ldelem_U2);
                    return;
                case TypeCode.SByte:
                    ilGen.Emit(OpCodes.Ldelem_I1);
                    return;
                case TypeCode.Byte:
                    ilGen.Emit(OpCodes.Ldelem_U1);
                    return;
                case TypeCode.Int16:
                    ilGen.Emit(OpCodes.Ldelem_I2);
                    return;
                case TypeCode.UInt16:
                    ilGen.Emit(OpCodes.Ldelem_U2);
                    return;
                case TypeCode.Int32:
                    ilGen.Emit(OpCodes.Ldelem_I4);
                    return;
                case TypeCode.UInt32:
                    ilGen.Emit(OpCodes.Ldelem_U4);
                    return;
                case TypeCode.Int64:
                    ilGen.Emit(OpCodes.Ldelem_I8);
                    return;
                case TypeCode.UInt64:
                    ilGen.Emit(OpCodes.Ldelem_I8);
                    ilGen.Emit(OpCodes.Conv_U8);
                    return;
                case TypeCode.Single:
                    ilGen.Emit(OpCodes.Ldelem_R4);
                    return;
                case TypeCode.Double:
                    ilGen.Emit(OpCodes.Ldelem_R8);
                    return;
            }

            if (elementType.IsPointer || elementType.IsByRef)
            {
                ilGen.Emit(OpCodes.Ldelem_I);
                return;
            }

            if (elementType.IsValueType)
            {
                ilGen.Emit(OpCodes.Ldelem, elementType);
                return;
            }

            ilGen.Emit(OpCodes.Ldelem_Ref);
        }

        /// <summary>
        /// 加载引用类型元素数组的元素。
        /// </summary>
        /// <param name="ilGen"></param>
        public static void LoadReferenceElement(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Ldelem_Ref);
        }

        /// <summary>
        /// 加载类型值的大小。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">类型</param>
        public static void SizeOf(this ILGenerator ilGen, Type type)
        {
            ilGen.Emit(OpCodes.Sizeof, type);
        }

        /// <summary>
        /// 加载一个 Null 值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void LoadNull(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Ldnull);
        }

        /// <summary>
        /// 当提供的值为 False 时跳到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static void BranchFalse(this ILGenerator ilGen, Label label)
        {
            ilGen.Emit(OpCodes.Brfalse, label);
        }

        /// <summary>
        /// 当提供的值为 True 时跳到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static void BranchTrue(this ILGenerator ilGen, Label label)
        {
            ilGen.Emit(OpCodes.Brtrue, label);
        }

        /// <summary>
        /// 无条件跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static void Branch(this ILGenerator ilGen, Label label)
        {
            ilGen.Emit(OpCodes.Br, label);
        }

        /// <summary>
        /// 当栈顶第一个值小于第二个值时跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static void BranchIfLess(this ILGenerator ilGen, Label label)
        {
            ilGen.Emit(OpCodes.Blt, label);
        }

        /// <summary>
        /// 当栈顶第一个值小于或等于第二个值时跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static void BranchIfLessOrEqual(this ILGenerator ilGen, Label label)
        {
            ilGen.Emit(OpCodes.Ble, label);
        }

        /// <summary>
        /// 当栈顶第一个值等于第二个值时跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static void BranchIfEqual(this ILGenerator ilGen, Label label)
        {
            ilGen.Emit(OpCodes.Beq, label);
        }

        /// <summary>
        /// 当栈顶第一个值大于第二个值时跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static void BranchIfGreater(this ILGenerator ilGen, Label label)
        {
            ilGen.Emit(OpCodes.Bgt, label);
        }

        /// <summary>
        /// 当栈顶第一个值大于或等于第二个值时跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static void BranchIfGreaterOrEqual(this ILGenerator ilGen, Label label)
        {
            ilGen.Emit(OpCodes.Bge, label);
        }

        /// <summary>
        /// 将当前值类型转换为对象引用 (类型 O)。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">值类型</param>
        public static void Box(this ILGenerator ilGen, Type type)
        {
            ilGen.Emit(OpCodes.Box, type);
        }

        /// <summary>
        /// 将指令中指定类型的已装箱的表示形式转换成未装箱形式。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">类型</param>
        public static void UnboxAny(this ILGenerator ilGen, Type type)
        {
            ilGen.Emit(OpCodes.Unbox_Any, type);
        }

        /// <summary>
        /// 将值类型的已装箱的表示形式转换为其未装箱的形式。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">类型</param>
        public static void Unbox(this ILGenerator ilGen, Type type)
        {
            ilGen.Emit(OpCodes.Unbox, type);
        }

        /// <summary>
        /// 将栈顶的值尝试转化为指定的类型。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">类型</param>
        public static void CastClass(this ILGenerator ilGen, Type type)
        {
            ilGen.Emit(OpCodes.Castclass, type);
        }

        /// <summary>
        /// 创建一个新的对象或值类型，并将对象引用的新实例 (类型 O) 到计算堆栈上。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="constructor">对象的构造函数</param>
        public static ILGenerator NewObject(this ILGenerator ilGen, ConstructorInfo constructor)
        {
            ilGen.Emit(OpCodes.Newobj, constructor);

            return ilGen;
        }

        /// <summary>
        /// 当指定的本地变量值为该类型的默认值时跳到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="local">本地变量</param>
        /// <param name="label">代码块</param>
        public static void BranchDefaultValue(this ILGenerator ilGen, LocalBuilder local, Label label)
        {
            var type = local.LocalType;

            if (type.IsClass ||
                type.IsInterface ||
                type.IsPointer ||
                type.IsByRef ||
                type == typeof(IntPtr) ||
                type == typeof(UIntPtr))
            {
                ilGen.LoadLocal(local);
                ilGen.BranchFalse(label);

                return;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Single:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Double:
                    ilGen.LoadLocal(local);
                    ilGen.BranchFalse(label);
                    return;
            }

            var size = TypeHelper.SizeOf(type);

            var labNotEmpty = ilGen.DefineLabel();

            while (size >= 4)
            {
                size -= 4;
                ilGen.LoadLocalAddress(local);

                if (size != 0)
                {
                    ilGen.LoadConstant(size);
                    ilGen.Emit(OpCodes.Add);
                }

                ilGen.Emit(OpCodes.Ldind_I4);
                ilGen.BranchTrue(labNotEmpty);
            }

            if (size >= 2)
            {
                size -= 2;
                ilGen.LoadLocalAddress(local);

                if (size != 0)
                {
                    ilGen.LoadConstant(size);
                    ilGen.Emit(OpCodes.Add);
                }

                ilGen.Emit(OpCodes.Ldind_I2);
                ilGen.BranchTrue(labNotEmpty);
            }

            if (size >= 1)
            {
                ilGen.LoadLocalAddress(local);
                ilGen.Emit(OpCodes.Ldind_I1);
                ilGen.BranchTrue(labNotEmpty);
            }

            ilGen.Branch(label);

            ilGen.MarkLabel(labNotEmpty);
        }

        /// <summary>
        /// 调用方法。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="methodBase">方法信息</param>
        public static void Call(this ILGenerator ilGen, MethodBase methodBase)
        {
            if (methodBase is ConstructorInfo constructor)
            {
                ilGen.Emit(OpCodes.Call, constructor);
            }
            else if (methodBase.IsVirtual && !methodBase.IsFinal)
            {
                ilGen.Emit(OpCodes.Callvirt, (MethodInfo)methodBase);
            }
            else
            {
                ilGen.Emit(OpCodes.Call, (MethodInfo)methodBase);
            }
        }

        /// <summary>
        /// 以托管代码默认约定调用方法指针。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="functionPointer">指针底值</param>
        /// <param name="parameterTypes">调用参数签名</param>
        /// <param name="returnType">调用返回值前面</param>
        public static ILGenerator Calli(this ILGenerator ilGen, IntPtr functionPointer, Type returnType, Type[] parameterTypes)
        {
            ilGen.LoadConstant(functionPointer);

            ilGen.EmitCalli(OpCodes.Calli, CallingConventions.Standard, returnType, parameterTypes, null);

            return ilGen;
        }

        /// <summary>
        /// 以托管代码默认约定调用动态方法。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="dynamicMethod">动态方法</param>
        public static ILGenerator Calli(this ILGenerator ilGen, DynamicMethod dynamicMethod)
        {
            GCHandle.Alloc(dynamicMethod);

            return Calli(ilGen, dynamicMethod.GetFunctionPointer(), dynamicMethod.ReturnType, dynamicMethod.GetParameters().Select(item => item.ParameterType).ToArray());
        }

        /// <summary>
        /// 将值转换为指针类型。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void ConvertPointer(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Conv_I);
        }

        /// <summary>
        /// 将值转换为 Int32 类型。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void ConvertInt32(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Conv_I4);
        }

        /// <summary>
        /// 将值转换为 Int8 类型。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void ConvertInt8(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Conv_I1);
        }

        /// <summary>
        /// 将值转换为 Int16 类型。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void ConvertInt16(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Conv_I2);
        }

        /// <summary>
        /// 将值转换为 Int64 类型。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void ConvertInt64(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Conv_I8);
        }

        /// <summary>
        /// 对栈顶的两个值进行 加法运算，返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void Add(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Add);
        }

        /// <summary>
        /// 对栈顶的两个值进行 减法运算，返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void Subtract(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Sub);
        }

        /// <summary>
        /// 对栈顶的两个值进行 乘法运算，返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void Multiply(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Mul);
        }

        /// <summary>
        /// 对栈顶的两个值进行 除法运算，返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void Division(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Div);
        }

        /// <summary>
        /// 对栈顶的两个值进行 求余运算，返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void Rem(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Rem);
        }

        /// <summary>
        /// 对栈顶的两个值进行 按位异或，返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void Xor(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Xor);
        }

        /// <summary>
        /// 对栈顶的两个值进行 按位或，返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void Or(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Or);
        }

        /// <summary>
        /// 对栈顶的两个值进行 按位与，返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void And(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.And);
        }

        /// <summary>
        /// 对栈顶的两个值进行 左移。返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void ShiftLeft(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Shl);
        }

        /// <summary>
        /// 对栈顶的两个值进行 右移。返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void ShiftRight(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Shr);
        }

        /// <summary>
        /// 对栈顶的两个值进行 无符号右移。返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void UnsignedShiftRight(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Shr_Un);
        }

        /// <summary>
        /// 抛出位于栈顶的异常。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void Throw(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Throw);
        }

        /// <summary>
        /// 移除位于栈顶的一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void Pop(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Pop);
        }

        /// <summary>
        /// 复制位于栈顶的一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void Duplicate(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Dup);
        }

        /// <summary>
        /// 判断栈顶的对象是否为指定类型的实例，如果为是则返回该实例。如果为否则为返回 Null。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">指定类型</param>
        public static void IsInstance(this ILGenerator ilGen, Type type)
        {
            ilGen.Emit(OpCodes.Isinst, type);
        }

        /// <summary>
        /// 分配本地内存。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void LocalAllocate(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Localloc);
        }

        /// <summary>
        /// 方法返回。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void Return(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Ret);
        }
        /// <summary>
        /// 定义自动完成的属性。
        /// </summary>
        /// <param name="typeBuilder">类型生成器</param>
        /// <param name="attributes">属性的属性</param>
        /// <param name="name">属性的名称</param>
        /// <param name="type">属性的类型</param>
        /// <param name="fieldAttributes">字段的属性</param>
        /// <param name="methodAttributes">get 和 set 方法的属性</param>
        /// <returns>返回当前类型生成器</returns>
        public static TypeBuilder DefineAutoProperty(
            this TypeBuilder typeBuilder,
            string name,
            Type type,
            PropertyAttributes attributes = PropertyAttributes.HasDefault,
            FieldAttributes fieldAttributes = FieldAttributes.Private | FieldAttributes.SpecialName,
            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName)
        {
            var fieldBuilder = typeBuilder.DefineField(
                $"_{name}_{Guid.NewGuid().ToString("N")}",
                type,
                fieldAttributes
                );

            return typeBuilder.DefineProperty(name, attributes, type,
                (propertyBuilder, methodBuilder, ilGen) =>
                {
                    ilGen.LoadArgument(0);
                    ilGen.LoadField(fieldBuilder);
                    ilGen.Return();
                }, (propertyBuilder, methodBuilder, ilGen) =>
                {

                    ilGen.LoadArgument(0);
                    ilGen.LoadArgument(1);
                    ilGen.StoreField(fieldBuilder);
                    ilGen.Return();
                }, methodAttributes);
        }

        /// <summary>
        /// 定义属性。
        /// </summary>
        /// <param name="typeBuilder">类型生成器</param>
        /// <param name="name">属性名</param>
        /// <param name="attributes">属性的属性</param>
        /// <param name="type">属性的类型</param>
        /// <param name="getCallback">get 方法的回调</param>
        /// <param name="setCallback">set 方法的回调</param>
        /// <param name="methodAttributes">get 和 set 方法的属性</param>
        /// <returns>返回当前类生成器</returns>
        public static TypeBuilder DefineProperty(
            this TypeBuilder typeBuilder, 
            string name, 
            PropertyAttributes attributes,
            Type type, 
            Action<PropertyBuilder, MethodBuilder, ILGenerator> getCallback, 
            Action<PropertyBuilder, MethodBuilder, ILGenerator> setCallback, 
            MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName)
        {
            var propertyBuilder = typeBuilder.DefineProperty(
                name,
                attributes,
                type,
                Type.EmptyTypes);

            if (getCallback != null)
            {
                typeBuilder.DefineMethod(
                    $"{"get"}_{name}_{Guid.NewGuid().ToString("N")}",
                    methodAttributes,
                    type,
                    Type.EmptyTypes,
                    (methodBuilder, ilGen) =>
                    {
                        getCallback(propertyBuilder, methodBuilder, ilGen);

                        propertyBuilder.SetGetMethod(methodBuilder);
                    });
            }

            if (setCallback != null)
            {
                typeBuilder.DefineMethod(
                    $"{"set"}_{name}_{Guid.NewGuid().ToString("N")}",
                    methodAttributes,
                    type,
                    Type.EmptyTypes,
                    (methodBuilder, ilGen) =>
                    {
                        setCallback(propertyBuilder, methodBuilder, ilGen);

                        propertyBuilder.SetGetMethod(methodBuilder);
                    });
            }

            return typeBuilder;
        }

        /// <summary>
        /// 定义方法。
        /// </summary>
        /// <param name="typeBuilder">类型生成器</param>
        /// <param name="name">方法名</param>
        /// <param name="attributes">方法的属性</param>
        /// <param name="returnType">返回值类型</param>
        /// <param name="parameterTypes">参数类型集合</param>
        /// <param name="callback">方法生成器回调</param>
        /// <returns>返回当前类型生成器</returns>
        public static TypeBuilder DefineMethod(this TypeBuilder typeBuilder, string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes, Action<MethodBuilder, ILGenerator> callback)
        {
            var methodBuilder = typeBuilder.DefineMethod(name, attributes, returnType, parameterTypes);

            callback(methodBuilder, methodBuilder.GetILGenerator());

            return typeBuilder;
        }

        /// <summary>
        /// 将特性转换为特性生成器。
        /// </summary>
        /// <typeparam name="TAttribute">特性类型</typeparam>
        /// <param name="attribute">特性实例</param>
        /// <returns>返回一个将特性转换为特性生成器</returns>
        public static CustomAttributeBuilder ToCustomAttributeBuilder<TAttribute>(this TAttribute attribute) where TAttribute : Attribute, new()
        {
            var emptyValues = new object[0];

            var constructor = typeof(TAttribute).GetConstructor(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static,
                Type.DefaultBinder,
                Type.EmptyTypes,
                null);

            var constructorArgs = emptyValues;

            var namedFields = new List<FieldInfo>();
            var fieldValues = new List<object>();
            var namedProperties = new List<PropertyInfo>();
            var propertyValues = new List<object>();

            foreach (var item in typeof(TAttribute).GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (item.CanRead && item.CanWrite && item.GetValue(attribute, emptyValues) is object value)
                {
                    namedProperties.Add(item);
                    propertyValues.Add(value);
                }
            }

            foreach (var item in typeof(TAttribute).GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (!item.IsInitOnly && item.GetValue(attribute) is object value)
                {
                    namedFields.Add(item);
                    fieldValues.Add(value);
                }
            }

            return new CustomAttributeBuilder(
                constructor, constructorArgs,
                namedProperties.ToArray(), propertyValues.ToArray(),
                namedFields.ToArray(), fieldValues.ToArray());
        }
    }
}