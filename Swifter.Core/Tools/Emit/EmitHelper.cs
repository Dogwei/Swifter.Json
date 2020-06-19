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
        public static TValue LoadClassField<TValue>(object obj, int offset)
        {
            return Underlying.AddByteOffset(ref TypeHelper.Unbox<TValue>(obj), offset);
        }

        /// <summary>
        /// Emit 内部使用函数。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref TValue LoadClassFieldAddress<TValue>(object obj, int offset)
        {
            return ref Underlying.AddByteOffset(ref TypeHelper.Unbox<TValue>(obj), offset);
        }

        /// <summary>
        /// Emit 内部使用函数。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void StoreClassField<TValue>(object obj, TValue value, int offset)
        {
            Underlying.AddByteOffset(ref TypeHelper.Unbox<TValue>(obj), offset) = value;
        }

        /// <summary>
        /// Emit 内部使用函数。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TValue LoadStructField<TValue>(ref TValue obj, int offset)
        {
            return Underlying.AddByteOffset(ref obj, offset);
        }

        /// <summary>
        /// Emit 内部使用函数。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref TValue LoadStructFieldAddress<TValue>(ref TValue obj, int offset)
        {
            return ref Underlying.AddByteOffset(ref obj, offset);
        }

        /// <summary>
        /// Emit 内部使用函数。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void StoreStructField<TValue>(ref TValue obj, TValue value, int offset)
        {
            Underlying.AddByteOffset(ref obj, offset) = value;
        }

        /// <summary>
        /// Emit 内部使用函数。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TValue LoadStaticField<TValue>(ref TValue address)
        {
            return address;
        }

        /// <summary>
        /// Emit 内部使用函数。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref TValue LoadStaticFieldAddress<TValue>(ref TValue address)
        {
            return ref address;
        }

        /// <summary>
        /// Emit 内部使用函数。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void StoreStaticField<TValue>(TValue value, ref TValue address)
        {
            address = value;
        }

        /// <summary>
        /// 加载字段值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="fieldInfo">字段信息</param>
        public static ILGenerator LoadField(this ILGenerator ilGen, FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic)
            {
                ilGen.Emit(OpCodes.Ldsfld, fieldInfo);
            }
            else
            {
                ilGen.Emit(OpCodes.Ldfld, fieldInfo);
            }

            return ilGen;
        }

        /// <summary>
        /// 跳过字段访问检查加载字段值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="fieldInfo">字段信息</param>
        public static ILGenerator UnsafeLoadField(this ILGenerator ilGen, FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic)
            {
                throw new NotSupportedException("Static fields are not supported at this time.");
            }
            else if(fieldInfo.DeclaringType.IsValueType)
            {
                ilGen.LoadConstant(TypeHelper.OffsetOf(fieldInfo));

                ilGen.Call(typeof(EmitHelper).GetMethod(nameof(LoadStructField)).MakeGenericMethod(fieldInfo.FieldType));
            }
            else
            {
                ilGen.LoadConstant(TypeHelper.OffsetOf(fieldInfo));

                ilGen.Call(typeof(EmitHelper).GetMethod(nameof(LoadClassField)).MakeGenericMethod(fieldInfo.FieldType));
            }

            return ilGen;
        }

        /// <summary>
        /// 设置字段值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="fieldInfo">字段信息</param>
        public static ILGenerator StoreField(this ILGenerator ilGen, FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic)
            {
                ilGen.Emit(OpCodes.Stsfld, fieldInfo);
            }
            else
            {
                ilGen.Emit(OpCodes.Stfld, fieldInfo);
            }

            return ilGen;
        }

        /// <summary>
        /// 跳过字段访问检查设置字段值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="fieldInfo">字段信息</param>
        public static ILGenerator UnsafeStoreField(this ILGenerator ilGen, FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic)
            {
                throw new NotSupportedException("Static fields are not supported at this time.");
            }
            else if (fieldInfo.DeclaringType.IsValueType)
            {
                ilGen.LoadConstant(TypeHelper.OffsetOf(fieldInfo));

                ilGen.Call(typeof(EmitHelper).GetMethod(nameof(StoreStructField)).MakeGenericMethod(fieldInfo.FieldType));
            }
            else
            {
                ilGen.LoadConstant(TypeHelper.OffsetOf(fieldInfo));

                ilGen.Call(typeof(EmitHelper).GetMethod(nameof(StoreClassField)).MakeGenericMethod(fieldInfo.FieldType));
            }

            return ilGen;
        }

        /// <summary>
        /// 加载字段地址。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="fieldInfo">字段信息</param>
        public static ILGenerator LoadFieldAddress(this ILGenerator ilGen, FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic)
            {
                ilGen.Emit(OpCodes.Ldsflda, fieldInfo);
            }
            else
            {
                ilGen.Emit(OpCodes.Ldflda, fieldInfo);
            }

            return ilGen;
        }

        /// <summary>
        /// 跳过字段访问检查加载字段地址。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="fieldInfo">字段信息</param>
        public static ILGenerator UnsafeLoadFieldAddress(this ILGenerator ilGen, FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic)
            {
                throw new NotSupportedException("Static fields are not supported at this time.");
            }
            else if (fieldInfo.DeclaringType.IsValueType)
            {
                ilGen.LoadConstant(TypeHelper.OffsetOf(fieldInfo));

                ilGen.Call(typeof(EmitHelper).GetMethod(nameof(LoadClassFieldAddress)).MakeGenericMethod(typeof(byte)));
            }
            else
            {
                ilGen.LoadConstant(TypeHelper.OffsetOf(fieldInfo));

                ilGen.Call(typeof(EmitHelper).GetMethod(nameof(LoadStructFieldAddress)).MakeGenericMethod(typeof(byte)));
            }

            return ilGen;
        }

        /// <summary>
        /// 加载类型的元数据元素。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">类型信息</param>
        public static ILGenerator LoadToken(this ILGenerator ilGen, Type type)
        {
            ilGen.Emit(OpCodes.Ldtoken, type);

            return ilGen;
        }

        /// <summary>
        /// 加载类型 Type 到栈顶。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">类型信息</param>
        public static ILGenerator LoadType(this ILGenerator ilGen, Type type)
        {
            ilGen.LoadToken(type);

            ilGen.Call(MethodOf<RuntimeTypeHandle, Type>(Type.GetTypeFromHandle));

            return ilGen;
        }

        /// <summary>
        /// 加载本地变量值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="localBuilder">变量信息</param>
        public static ILGenerator LoadLocal(this ILGenerator ilGen, LocalBuilder localBuilder)
        {
            ilGen.Emit(OpCodes.Ldloc, localBuilder);

            return ilGen;
        }

        /// <summary>
        /// 设置本地变量值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="localBuilder">本地变量信息</param>
        public static ILGenerator StoreLocal(this ILGenerator ilGen, LocalBuilder localBuilder)
        {
            ilGen.Emit(OpCodes.Stloc, localBuilder);

            return ilGen;
        }

        /// <summary>
        /// 加载本地变量地址。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="localBuilder">变量信息</param>
        public static ILGenerator LoadLocalAddress(this ILGenerator ilGen, LocalBuilder localBuilder)
        {
            ilGen.Emit(OpCodes.Ldloca, localBuilder);

            return ilGen;
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
        public static ILGenerator LoadConstant(this ILGenerator ilGen, long value)
        {
            ilGen.Emit(OpCodes.Ldc_I8, value);

            return ilGen;
        }

        /// <summary>
        /// 加载常量地址。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="value">值</param>
        public static ILGenerator LoadConstant(this ILGenerator ilGen, IntPtr value)
        {
            if ((int)(long)value == (long)value)
            {
                ilGen.LoadConstant((int)value);
                ilGen.ConvertPointer();
            }
            else
            {
                ilGen.LoadConstant((long)value);
                ilGen.ConvertPointer();
            }

            return ilGen;
        }

        /// <summary>
        /// 加载 String 常量。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="value">值</param>
        public static ILGenerator LoadString(this ILGenerator ilGen, string value)
        {
            ilGen.Emit(OpCodes.Ldstr, value);

            return ilGen;
        }

        /// <summary>
        /// 设置类型已提供的值到提供的内存上。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">值的类型</param>
        public static ILGenerator StoreValue(this ILGenerator ilGen, Type type)
        {
            if (type.IsValueType)
            {
                if (type.IsByRef ||
                    type.IsPointer ||
                    type == typeof(IntPtr) ||
                    type == typeof(UIntPtr))
                {
                    ilGen.Emit(OpCodes.Stind_I);
                }
                else
                {
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
            }
            else
            {
                ilGen.Emit(OpCodes.Stind_Ref);
            }

            return ilGen;
        }

        /// <summary>
        /// 在提供的内存上加载一个类型的值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">值的类型</param>
        public static ILGenerator LoadValue(this ILGenerator ilGen, Type type)
        {
            if (type.IsValueType)
            {
                if (type.IsByRef ||
                    type.IsPointer ||
                    type == typeof(IntPtr) ||
                    type == typeof(UIntPtr))
                {
                    ilGen.Emit(OpCodes.Ldind_I);
                }
                else
                {
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
                            ilGen.Emit(OpCodes.Conv_U8);
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
            }
            else
            {
                ilGen.Emit(OpCodes.Ldind_Ref);
            }

            return ilGen;
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
        public static ILGenerator StoreArgument(this ILGenerator ilGen, int index)
        {
            if (index >= 0 && index <= 255)
            {
                ilGen.Emit(OpCodes.Starg_S, (byte)index);
            }
            else
            {
                ilGen.Emit(OpCodes.Starg, index);
            }

            return ilGen;
        }

        /// <summary>
        /// 加载参数地址。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="index">参数序号</param>
        public static ILGenerator LoadArgumentAddress(this ILGenerator ilGen, int index)
        {
            if (index >= 0 && index <= 255)
            {
                ilGen.Emit(OpCodes.Ldarga_S, (byte)index);
            }
            else
            {
                ilGen.Emit(OpCodes.Ldarga, index);
            }

            return ilGen;
        }

        /// <summary>
        /// 加载提供的数组位于提供索引处的元素的地址。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="elementType">元素的类型</param>
        public static ILGenerator LoadElementAddress(this ILGenerator ilGen, Type elementType)
        {
            ilGen.Emit(OpCodes.Ldelema, elementType);

            return ilGen;
        }

        /// <summary>
        /// 加载提供的数组位于提供索引处的元素。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="elementType">元素的类型</param>
        public static ILGenerator LoadElement(this ILGenerator ilGen, Type elementType)
        {
            switch (Type.GetTypeCode(elementType))
            {
                case TypeCode.Boolean:
                    ilGen.Emit(OpCodes.Ldelem_U1);
                    break;
                case TypeCode.Char:
                    ilGen.Emit(OpCodes.Ldelem_U2);
                    break;
                case TypeCode.SByte:
                    ilGen.Emit(OpCodes.Ldelem_I1);
                    break;
                case TypeCode.Byte:
                    ilGen.Emit(OpCodes.Ldelem_U1);
                    break;
                case TypeCode.Int16:
                    ilGen.Emit(OpCodes.Ldelem_I2);
                    break;
                case TypeCode.UInt16:
                    ilGen.Emit(OpCodes.Ldelem_U2);
                    break;
                case TypeCode.Int32:
                    ilGen.Emit(OpCodes.Ldelem_I4);
                    break;
                case TypeCode.UInt32:
                    ilGen.Emit(OpCodes.Ldelem_U4);
                    break;
                case TypeCode.Int64:
                    ilGen.Emit(OpCodes.Ldelem_I8);
                    break;
                case TypeCode.UInt64:
                    ilGen.Emit(OpCodes.Ldelem_I8);
                    ilGen.Emit(OpCodes.Conv_U8);
                    break;
                case TypeCode.Single:
                    ilGen.Emit(OpCodes.Ldelem_R4);
                    break;
                case TypeCode.Double:
                    ilGen.Emit(OpCodes.Ldelem_R8);
                    break;
                default:
                    if (elementType.IsPointer || elementType.IsByRef)
                    {
                        ilGen.Emit(OpCodes.Ldelem_I);
                    }
                    else if(elementType.IsValueType)
                    {
                        ilGen.Emit(OpCodes.Ldelem, elementType);
                    }
                    else
                    {
                        ilGen.Emit(OpCodes.Ldelem_Ref);
                    }
                    break;
            }

            return ilGen;
        }

        /// <summary>
        /// 加载引用类型元素数组的元素。
        /// </summary>
        /// <param name="ilGen"></param>
        public static ILGenerator LoadReferenceElement(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Ldelem_Ref);

            return ilGen;
        }

        /// <summary>
        /// 加载类型值的大小。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">类型</param>
        public static ILGenerator SizeOf(this ILGenerator ilGen, Type type)
        {
            ilGen.Emit(OpCodes.Sizeof, type);

            return ilGen;
        }

        /// <summary>
        /// 加载一个 Null 值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator LoadNull(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Ldnull);

            return ilGen;
        }

        /// <summary>
        /// 当提供的值为 False 时跳到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static ILGenerator BranchFalse(this ILGenerator ilGen, Label label)
        {
            ilGen.Emit(OpCodes.Brfalse, label);

            return ilGen;
        }

        /// <summary>
        /// 当提供的值为 True 时跳到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static ILGenerator BranchTrue(this ILGenerator ilGen, Label label)
        {
            ilGen.Emit(OpCodes.Brtrue, label);

            return ilGen;
        }

        /// <summary>
        /// 无条件跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static ILGenerator Branch(this ILGenerator ilGen, Label label)
        {
            // TODO: Use Br_S

            ilGen.Emit(OpCodes.Br, label);

            return ilGen;
        }

        /// <summary>
        /// 当栈顶第一个值小于第二个值时跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static ILGenerator BranchIfLess(this ILGenerator ilGen, Label label)
        {
            // TODO: Use Blt_S

            ilGen.Emit(OpCodes.Blt, label);

            return ilGen;
        }

        /// <summary>
        /// 当栈顶第一个值小于或等于第二个值时跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static ILGenerator BranchIfLessOrEqual(this ILGenerator ilGen, Label label)
        {
            // TODO: Use Ble_S

            ilGen.Emit(OpCodes.Ble, label);

            return ilGen;
        }

        /// <summary>
        /// 当栈顶第一个值小于第二个值时跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static ILGenerator BranchIfLessUnsigned(this ILGenerator ilGen, Label label)
        {
            // TODO: Use Blt_Un_S

            ilGen.Emit(OpCodes.Blt_Un, label);

            return ilGen;
        }

        /// <summary>
        /// 当栈顶第一个值小于或等于第二个值时跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static ILGenerator BranchIfLessOrEqualUnsigned(this ILGenerator ilGen, Label label)
        {
            // TODO: Use Ble_Un_S

            ilGen.Emit(OpCodes.Ble_Un, label);

            return ilGen;
        }

        /// <summary>
        /// 当栈顶第一个值等于第二个值时跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static ILGenerator BranchIfEqual(this ILGenerator ilGen, Label label)
        {
            // TODO: Use Beq_S

            ilGen.Emit(OpCodes.Beq, label);

            return ilGen;
        }

        /// <summary>
        /// 当栈顶第一个值不等于第二个值时跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static ILGenerator BranchIfNotEqualUnsigned(this ILGenerator ilGen, Label label)
        {
            // TODO: Use Bne_Un_S

            ilGen.Emit(OpCodes.Bne_Un, label);

            return ilGen;
        }

        /// <summary>
        /// 当栈顶第一个值大于第二个值时跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static ILGenerator BranchIfGreater(this ILGenerator ilGen, Label label)
        {
            // TODO: Use Bgt_S

            ilGen.Emit(OpCodes.Bgt, label);

            return ilGen;
        }

        /// <summary>
        /// 当栈顶第一个值大于或等于第二个值时跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static ILGenerator BranchIfGreaterOrEqual(this ILGenerator ilGen, Label label)
        {
            // TODO: Use Bge_S

            ilGen.Emit(OpCodes.Bge, label);

            return ilGen;
        }

        /// <summary>
        /// 当栈顶第一个值大于第二个值时跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static ILGenerator BranchIfGreaterUnsigned(this ILGenerator ilGen, Label label)
        {
            // TODO: Use Bgt_Un_S

            ilGen.Emit(OpCodes.Bgt_Un, label);

            return ilGen;
        }

        /// <summary>
        /// 当栈顶第一个值大于或等于第二个值时跳转到指定块。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="label">代码块</param>
        public static ILGenerator BranchIfGreaterOrEqualUnsigned(this ILGenerator ilGen, Label label)
        {
            // TODO: Use Bge_Un_S

            ilGen.Emit(OpCodes.Bge_Un, label);

            return ilGen;
        }

        /// <summary>
        /// 将当前值类型转换为对象引用 (类型 O)。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">值类型</param>
        public static ILGenerator Box(this ILGenerator ilGen, Type type)
        {
            ilGen.Emit(OpCodes.Box, type);

            return ilGen;
        }

        /// <summary>
        /// 将指令中指定类型的已装箱的表示形式转换成未装箱形式。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">类型</param>
        public static ILGenerator UnboxAny(this ILGenerator ilGen, Type type)
        {
            ilGen.Emit(OpCodes.Unbox_Any, type);

            return ilGen;
        }

        /// <summary>
        /// 将值类型的已装箱的表示形式转换为其未装箱的形式。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">类型</param>
        public static ILGenerator Unbox(this ILGenerator ilGen, Type type)
        {
            ilGen.Emit(OpCodes.Unbox, type);

            return ilGen;
        }

        /// <summary>
        /// 将栈顶的值尝试转化为指定的类型。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">类型</param>
        public static ILGenerator CastClass(this ILGenerator ilGen, Type type)
        {
            ilGen.Emit(OpCodes.Castclass, type);

            return ilGen;
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
        public static ILGenerator BranchDefaultValue(this ILGenerator ilGen, LocalBuilder local, Label label)
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
            }
            else
            {
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
                        break;
                    default:
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
                        break;
                }
            }

            return ilGen;
        }

        /// <summary>
        /// 调用方法。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="methodBase">方法信息</param>
        public static ILGenerator Call(this ILGenerator ilGen, MethodBase methodBase)
        {
            if (methodBase is ConstructorInfo constructor)
            {
                ilGen.Emit(OpCodes.Call, constructor);
            }
            else if (methodBase.IsVirtual && !methodBase.IsFinal)
            {
                // TODO: 检查反射类是否为最终类。

                ilGen.Emit(OpCodes.Callvirt, (MethodInfo)methodBase);
            }
            else
            {
                ilGen.Emit(OpCodes.Call, (MethodInfo)methodBase);
            }

            return ilGen;
        }

        /// <summary>
        /// 跳过方法访问检查调用方法。
        /// </summary>
        /// <param name="ilGen"></param>
        /// <param name="methodBase"></param>
        /// <returns></returns>
        public static ILGenerator UnsafeCall(this ILGenerator ilGen, MethodBase methodBase)
        {
            var functionPointer = methodBase.GetFunctionPointer();

            GetParametersTypes(methodBase, out var parameterTypes, out var returnType);

            return ilGen.Calli(functionPointer, returnType, parameterTypes);
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
            // 保存引用。
            GCHandle.Alloc(dynamicMethod);

            return ilGen.Calli(
                dynamicMethod.GetFunctionPointer(), 
                dynamicMethod.ReturnType, 
                dynamicMethod.GetParameters().Map(item => item.ParameterType));
        }

        /// <summary>
        /// 将值转换为指针类型。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator ConvertPointer(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Conv_I);

            return ilGen;
        }

        /// <summary>
        /// 将值转换为 Int32 类型。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator ConvertInt32(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Conv_I4);

            return ilGen;
        }

        /// <summary>
        /// 将值转换为 Int8 类型。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator ConvertInt8(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Conv_I1);

            return ilGen;
        }

        /// <summary>
        /// 将值转换为 Int16 类型。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator ConvertInt16(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Conv_I2);

            return ilGen;
        }

        /// <summary>
        /// 将值转换为 Int64 类型。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator ConvertInt64(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Conv_I8);

            return ilGen;
        }

        /// <summary>
        /// 从栈中弹出 2 个值进行加法运算，将结果推到栈中。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator Add(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Add);

            return ilGen;
        }

        /// <summary>
        /// 从栈中弹出 2 个进整数行加法运算，并执行溢出检查，将结果推到栈中。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator AddOverflow(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Add_Ovf);

            return ilGen;
        }

        /// <summary>
        /// 从栈中弹出 2 个无符号整数进行加法运算，并执行溢出检查，将结果推到栈中。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator AddOverflowUnsigned(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Add_Ovf_Un);

            return ilGen;
        }

        /// <summary>
        /// 对栈顶的两个值进行 减法运算，返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator Subtract(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Sub);

            return ilGen;
        }

        /// <summary>
        /// 对栈顶的两个值进行 乘法运算，返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator Multiply(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Mul);

            return ilGen;
        }

        /// <summary>
        /// 对栈顶的两个值进行 除法运算，返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator Division(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Div);

            return ilGen;
        }

        /// <summary>
        /// 对栈顶的两个值进行 求余运算，返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator Rem(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Rem);

            return ilGen;
        }

        /// <summary>
        /// 对栈顶的两个值进行 按位异或，返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator Xor(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Xor);

            return ilGen;
        }

        /// <summary>
        /// 对栈顶的两个值进行 按位或，返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator Or(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Or);

            return ilGen;
        }

        /// <summary>
        /// 对栈顶的两个值进行 按位与，返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator And(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.And);

            return ilGen;
        }

        /// <summary>
        /// 对栈顶的两个值进行 左移。返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator ShiftLeft(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Shl);

            return ilGen;
        }

        /// <summary>
        /// 对栈顶的两个值进行 右移。返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator ShiftRight(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Shr);

            return ilGen;
        }

        /// <summary>
        /// 对栈顶的两个值进行 无符号右移。返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator ShiftRightUnsigned(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Shr_Un);

            return ilGen;
        }

        /// <summary>
        /// 抛出位于栈顶的异常。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator Throw(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Throw);

            return ilGen;
        }

        /// <summary>
        /// 再次引发当前异常。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator ReThrow(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Rethrow);

            return ilGen;
        }

        /// <summary>
        /// 移除位于栈顶的一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator Pop(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Pop);

            return ilGen;
        }

        /// <summary>
        /// 复制位于栈顶的一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator Duplicate(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Dup);

            return ilGen;
        }

        /// <summary>
        /// 判断栈顶的对象是否为指定类型的实例，如果为是则返回该实例。如果为否则为返回 Null。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="type">指定类型</param>
        public static ILGenerator IsInstance(this ILGenerator ilGen, Type type)
        {
            ilGen.Emit(OpCodes.Isinst, type);

            return ilGen;
        }

        /// <summary>
        /// 分配本地内存。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator LocalAllocate(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Localloc);

            return ilGen;
        }

        /// <summary>
        /// 方法返回。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static ILGenerator Return(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Ret);

            return ilGen;
        }

        /// <summary>
        /// 实现跳转表。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="labels">标签集合</param>
        public static ILGenerator Switch(this ILGenerator ilGen, Label[] labels)
        {
            ilGen.Emit(OpCodes.Switch, labels);

            return ilGen;
        }
    }
}