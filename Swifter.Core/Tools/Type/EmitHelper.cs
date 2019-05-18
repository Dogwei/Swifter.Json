using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供 Emit 帮助方法。
    /// </summary>
    public static class EmitHelper
    {
        private const int DifferenceSwitchMaxDepth = 2;
        private static readonly MethodInfo Method_GetTypeFromHandle = typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), new Type[] { typeof(RuntimeTypeHandle) });

        /// <summary>
        /// 加载字段值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="fieldInfo">字段信息</param>
        public static void LoadField(this ILGenerator ilGen, FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic)
            {
                ilGen.Emit(OpCodes.Ldsfld, fieldInfo);
            }
            else
            {
                ilGen.Emit(OpCodes.Ldfld, fieldInfo);
            }
        }

        /// <summary>
        /// 设置实例字段值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="fieldInfo">字段信息</param>
        public static void StoreField(this ILGenerator ilGen, FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic)
            {
                ilGen.Emit(OpCodes.Stsfld, fieldInfo);
            }
            else
            {
                ilGen.Emit(OpCodes.Stfld, fieldInfo);
            }
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
            ilGen.Call(Method_GetTypeFromHandle);
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
        /// 加载 Int32 常量。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="value">值</param>
        public static void LoadConstant(this ILGenerator ilGen, int value)
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
                    if ((uint)value <= 255)
                    {
                        ilGen.Emit(OpCodes.Ldc_I4_S, (byte)value);
                    }
                    else
                    {
                        ilGen.Emit(OpCodes.Ldc_I4, value);
                    }
                    break;
            }
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
        public static void LoadArgument(this ILGenerator ilGen, int index)
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
                    if ((uint)index <= 255)
                    {
                        ilGen.Emit(OpCodes.Ldarg_S, (byte)index);
                    }
                    else
                    {
                        ilGen.Emit(OpCodes.Ldarg, index);
                    }
                    break;
            }
        }

        /// <summary>
        /// 设置参数值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        /// <param name="index">参数序号</param>
        public static void StoreArgument(this ILGenerator ilGen, int index)
        {
            if ((uint)index <= 255)
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
            if ((uint)index <= 255)
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
        public static void NewObject(this ILGenerator ilGen, ConstructorInfo constructor)
        {
            ilGen.Emit(OpCodes.Newobj, constructor);
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
            else if(methodBase.IsVirtual && !methodBase.IsFinal)
            {
                ilGen.Emit(OpCodes.Callvirt, (MethodInfo)methodBase);
            }
            else
            {
                ilGen.Emit(OpCodes.Call, (MethodInfo)methodBase);
            }
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
        /// 对栈顶的两个值进行 按位异或，返回一个值。
        /// </summary>
        /// <param name="ilGen">ilGen</param>
        public static void Xor(this ILGenerator ilGen)
        {
            ilGen.Emit(OpCodes.Xor);
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
        /// 生成 Switch(String) 代码块。
        /// </summary>
        /// <param name="iLGen">ILGenerator IL 指令生成器</param>
        /// <param name="emitLdcValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        public static void Switch(this ILGenerator iLGen,
            Action<ILGenerator> emitLdcValue,
            CaseInfo<string>[] cases,
            Label defaultLabel, bool ignoreCase = false)
        {
            try
            {
                DifferenceSwitch(iLGen, emitLdcValue, cases, defaultLabel, ignoreCase);
            }
            catch (Exception)
            {
                HashSwitch(iLGen, emitLdcValue, cases, defaultLabel, ignoreCase);
            }
        }

        /// <summary>
        /// 生成 Switch(String) 代码块。
        /// </summary>
        /// <param name="iLGen">ILGenerator IL 指令生成器</param>
        /// <param name="emitLdcValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        private static void HashSwitch(this ILGenerator iLGen,
            Action<ILGenerator> emitLdcValue,
            CaseInfo<string>[] cases,
            Label defaultLabel, bool ignoreCase = false)
        {
            Func<string, int> funcGetHashCode;
            Func<string, string, bool> funcEquals;

            if (ignoreCase)
            {
                funcGetHashCode = StringHelper.GetUpperedHashCode;
                funcEquals = StringHelper.IgnoreCaseEqualsByUpper;

                cases = (CaseInfo<string>[])cases.Clone();

                for (int i = 0; i < cases.Length; i++)
                {
                    cases[i] = new CaseInfo<string>(cases[i].Value.ToUpper(), cases[i].Label);
                }
            }
            else
            {
                funcGetHashCode = StringHelper.GetHashCode;
                funcEquals = StringHelper.Equals;
            }

            foreach (var item in cases)
            {
                item.HashCode = funcGetHashCode(item.Value);
            }

            Switch(
                iLGen,
                emitLdcValue,
                funcGetHashCode.Method,
                funcEquals.Method,
                cases,
                defaultLabel,
                (tILGen, value) => tILGen.Emit(OpCodes.Ldstr, value));
        }

        /// <summary>
        /// 生成 Switch(int) 代码块。
        /// </summary>
        /// <param name="ILGen">ILGenerator IL 指令生成器</param>
        /// <param name="ldValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        public static void Switch(this ILGenerator ILGen,
            Action<ILGenerator> ldValue,
            CaseInfo<int>[] cases,
            Label defaultLabel)
        {
            cases = (CaseInfo<int>[])cases.Clone();

            Array.Sort(cases, (Before, After)=> { return Before.Value - After.Value; });

            SwitchNumber(ILGen, ldValue, cases, defaultLabel, (tILGen, value) => { tILGen.LoadConstant(value); }, 0, (cases.Length - 1) / 2, cases.Length - 1);
        }

        /// <summary>
        /// 生成 Switch(int) 代码块。
        /// </summary>
        /// <param name="ILGen">ILGenerator IL 指令生成器</param>
        /// <param name="loadValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        public static void Switch(this ILGenerator ILGen,
            Action<ILGenerator> loadValue,
            CaseInfo<IntPtr>[] cases,
            Label defaultLabel)
        {
            cases = TypeHelper.Clone(cases);

            Array.Sort(cases, (Before, After) => ((long)Before.Value).CompareTo((long)After.Value));

            SwitchNumber(
                ILGen,
                loadValue, 
                cases,
                defaultLabel,
                (tILGen, value) => { tILGen.LoadConstant((long)value); tILGen.ConvertPointer(); },
                0, 
                (cases.Length - 1) / 2,
                cases.Length - 1);
        }

        /// <summary>
        /// 生成 Switch(long) 代码块。
        /// </summary>
        /// <param name="ILGen">ILGenerator IL 指令生成器</param>
        /// <param name="loadValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        public static void Switch(this ILGenerator ILGen,
            Action<ILGenerator> loadValue,
            CaseInfo<long>[] cases,
            Label defaultLabel)
        {
            cases = (CaseInfo<long>[])cases.Clone();

            Array.Sort(cases, (before, after) => before.Value.CompareTo(after.Value));

            SwitchNumber(ILGen, loadValue, cases, defaultLabel, (tILGen, value) => { tILGen.LoadConstant(value); }, 0, (cases.Length - 1) / 2, cases.Length - 1);
        }

        /// <summary>
        /// 生成 Switch(String) 代码块。字符串差异位置比较，通常情况下这比 Hash 比较要快。
        /// </summary>
        /// <param name="ilGen">ILGenerator IL 指令生成器</param>
        /// <param name="loadValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        /// <param name="ignoreCase">是否忽略大小写</param>
        private static void DifferenceSwitch(this ILGenerator ilGen,
            Action<ILGenerator> loadValue,
            CaseInfo<string>[] cases,
            Label defaultLabel, bool ignoreCase = false)
        {
            if (ignoreCase)
            {
                cases = TypeHelper.Clone(cases);

                for (int i = 0; i < cases.Length; i++)
                {
                    cases[i] = new CaseInfo<string>(cases[i].Value.ToUpper(), cases[i].Label);
                }

                DifferenceSwitch(
                    ilGen,
                    loadValue,
                    ((Func<string, string, bool>)StringHelper.IgnoreCaseEqualsByUpper).Method,
                    StringUpperCharArMethod,
                    cases,
                    defaultLabel,
                    ItemLdcValue);
            }
            else
            {
                DifferenceSwitch(
                    ilGen,
                    loadValue,
                    ((Func<string, string, bool>)StringHelper.Equals).Method,
                    StringCharAtMethod,
                    cases,
                    defaultLabel,
                    ItemLdcValue);
            }

            void ItemLdcValue(ILGenerator il, string value)
            {
                il.Emit(OpCodes.Ldstr, value);
            }
        }

        private static readonly MethodInfo StringCharAtMethod = ArrayHelper.Filter(
            typeof(string).GetProperties(),
            (item) => { var indexParameters = item.GetIndexParameters(); return indexParameters.Length == 1 && indexParameters[0].ParameterType == typeof(int); },
            item => item)
            [0].GetGetMethod();

        private static readonly MethodInfo StringUpperCharArMethod = typeof(StringHelper).GetMethod(nameof(StringHelper.UpperCharAt), BindingFlags.Public | BindingFlags.Static);

        private static readonly MethodInfo StringGetLengthMethod = typeof(string).GetProperty(nameof(string.Length)).GetGetMethod();

        private static void DifferenceCasesProcess(
            ILGenerator iLGen, 
            Action<ILGenerator> emitLdcValue, 
            MethodInfo stringEqualsMethod,
            MethodInfo stringCharAtMethod,
            Action<ILGenerator, string> itemLdcValue,
            Label defaultLabel, 
            CaseInfo<int>[] differenceCases)
        {
            foreach (var item in differenceCases)
            {
                iLGen.MarkLabel(item.Label);

                if (item.Tag is StringSingleGroup<CaseInfo<string>> singleGroup)
                {
                    emitLdcValue(iLGen);
                    itemLdcValue(iLGen, singleGroup.Value.Value);
                    iLGen.Call(stringEqualsMethod);
                    iLGen.Emit(OpCodes.Brtrue, singleGroup.Value.Label);
                }
                else if (item.Tag is StringDifferenceGroup<CaseInfo<string>> differenceGroup)
                {
                    var charCases = new CaseInfo<int>[differenceGroup.Groups.Count];

                    for (int i = 0; i < charCases.Length; i++)
                    {
                        charCases[i] = new CaseInfo<int>(differenceGroup.Groups[i].Key, iLGen.DefineLabel()) { Tag = differenceGroup.Groups[i].Value };
                    }

                    Switch(iLGen, (ilGen2) =>
                    {
                        emitLdcValue(ilGen2);
                        ilGen2.LoadConstant(differenceGroup.Index);
                        ilGen2.Call(stringCharAtMethod);
                    }, charCases, defaultLabel);

                    DifferenceCasesProcess(iLGen, emitLdcValue, stringEqualsMethod, stringCharAtMethod, itemLdcValue,defaultLabel, charCases);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }

        private static void DifferenceSwitch(ILGenerator iLGen,
            Action<ILGenerator> emitLdcValue,
            MethodInfo stringEqualsMethod,
            MethodInfo stringCharAtMethod,
            CaseInfo<string>[] cases,
            Label defaultLabel,
            Action<ILGenerator, string> itemLdcValue)
        {

            var lengthGroup = new StringLengthGroup<CaseInfo<string>>(cases, item => item.Value);

            if (lengthGroup.GetDepth() > DifferenceSwitchMaxDepth)
            {
                throw new ArgumentException("Groups too deep.");
            }

            var lengthCases = new CaseInfo<int>[lengthGroup.Groups.Count];

            for (int i = 0; i < lengthCases.Length; i++)
            {
                lengthCases[i] = new CaseInfo<int>(lengthGroup.Groups[i].Key, iLGen.DefineLabel()) { Tag = lengthGroup.Groups[i].Value };
            }

            Switch(iLGen, (ilGen2) =>
            {
                emitLdcValue(ilGen2);
                ilGen2.Call(StringGetLengthMethod);
            }, lengthCases, defaultLabel);

            DifferenceCasesProcess(iLGen, emitLdcValue, stringEqualsMethod, stringCharAtMethod, itemLdcValue, defaultLabel, lengthCases);
        }

        /// <summary>
        /// 生成指定类型的 Switch 代码块。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="ilGen">ILGenerator IL 指令生成器</param>
        /// <param name="ldValue">生成加载 Switch 参数的指令的委托</param>
        /// <param name="getHashCodeMethod">获取 HashCode 值的方法，参数签名: int(T)</param>
        /// <param name="equalsMethod">比例两个值的方法，参数签名: bool (T, T)</param>
        /// <param name="cases">case 标签块集合</param>
        /// <param name="defaultLabel">默认标签块</param>
        /// <param name="ldCaseValue">生成加载指定 Case 块值的指定的委托</param>
        public static void Switch<T>(this ILGenerator ilGen,
            Action<ILGenerator> ldValue,
            MethodInfo getHashCodeMethod,
            MethodInfo equalsMethod,
            CaseInfo<T>[] cases,
            Label defaultLabel,
            Action<ILGenerator, T> ldCaseValue)
        {
            cases = (CaseInfo<T>[])cases.Clone();

            Array.Sort(cases);

            var groupedCases = new Dictionary<int, List<CaseInfo<T>>>();

            foreach (var item in cases)
            {
                groupedCases.TryGetValue(item.HashCode, out var items);

                if (items == null)
                {
                    items = new List<CaseInfo<T>>
                    {
                        item
                    };

                    groupedCases.Add(item.HashCode, items);
                }
                else
                {
                    items.Add(item);
                }
            }

            var hashCodeLocal = ilGen.DeclareLocal(typeof(int));

            ldValue(ilGen);
            ilGen.Emit(OpCodes.Call, getHashCodeMethod);
            ilGen.StoreLocal(hashCodeLocal);

            SwitchObject(
                ilGen,
                ldValue,
                il => il.LoadLocal(hashCodeLocal),
                il => il.Emit(OpCodes.Call, equalsMethod),
                groupedCases.ToList(),
                defaultLabel,
                ldCaseValue,
                0,
                (groupedCases.Count - 1) / 2,
                groupedCases.Count - 1);
        }


        private static void SwitchNumber<T>(this ILGenerator ilGen,
            Action<ILGenerator> ldValue,
            CaseInfo<T>[] cases,
            Label defaultLabel,
            Action<ILGenerator, T> ldCaseValue,
            int begin,
            int index,
            int end)
        {

            if (begin > end)
            {
                ilGen.Emit(OpCodes.Br, defaultLabel);

                return;
            }

            if (begin + 1 == end)
            {
                ldValue(ilGen);
                ldCaseValue(ilGen, cases[begin].Value);
                ilGen.Emit(OpCodes.Beq, cases[begin].Label);

                ldValue(ilGen);
                ldCaseValue(ilGen, cases[end].Value);
                ilGen.Emit(OpCodes.Beq, cases[end].Label);

                ilGen.Emit(OpCodes.Br, defaultLabel);

                return;
            }

            if (begin == end)
            {
                ldValue(ilGen);
                ldCaseValue(ilGen, cases[begin].Value);
                ilGen.Emit(OpCodes.Beq, cases[begin].Label);

                ilGen.Emit(OpCodes.Br, defaultLabel);

                return;
            }

            var GtLabel = ilGen.DefineLabel();

            ldValue(ilGen);
            ldCaseValue(ilGen, cases[index].Value);
            ilGen.Emit(OpCodes.Bgt, GtLabel);

            SwitchNumber(ilGen, ldValue, cases, defaultLabel, ldCaseValue, begin, (begin + index) / 2, index);

            ilGen.MarkLabel(GtLabel);

            SwitchNumber(ilGen, ldValue, cases, defaultLabel, ldCaseValue, index + 1, (index + 1 + end) / 2, end);
        }

        private static void SwitchObject<T>(this ILGenerator ilGen,
            Action<ILGenerator> ldValue,
            Action<ILGenerator> ldHashCode,
            Action<ILGenerator> callEquals,
            List<KeyValuePair<int, List<CaseInfo<T>>>> cases,
            Label defaultLabel,
            Action<ILGenerator, T> ldCaseValue,
            int begin,
            int index,
            int end)
        {
            if (begin > end)
            {
                return;
            }

            if (begin == end)
            {
                ldHashCode(ilGen);
                ilGen.LoadConstant(cases[begin].Key);
                ilGen.Emit(OpCodes.Bne_Un, defaultLabel);

                foreach (var Item in cases[begin].Value)
                {
                    ldValue(ilGen);
                    ldCaseValue(ilGen, Item.Value);
                    callEquals(ilGen);
                    ilGen.Emit(OpCodes.Brtrue, Item.Label);
                }

                ilGen.Emit(OpCodes.Br, defaultLabel);

                return;
            }

            if (begin + 1 == end)
            {

                var EndLabel = ilGen.DefineLabel();

                ldHashCode(ilGen);
                ilGen.LoadConstant(cases[begin].Key);
                ilGen.Emit(OpCodes.Bne_Un, EndLabel);

                foreach (var Item in cases[begin].Value)
                {
                    ldValue(ilGen);
                    ldCaseValue(ilGen, Item.Value);
                    callEquals(ilGen);
                    ilGen.Emit(OpCodes.Brtrue, Item.Label);
                }


                ilGen.MarkLabel(EndLabel);

                ldHashCode(ilGen);
                ilGen.LoadConstant(cases[end].Key);
                ilGen.Emit(OpCodes.Bne_Un, defaultLabel);

                foreach (var Item in cases[end].Value)
                {
                    ldValue(ilGen);
                    ldCaseValue(ilGen, Item.Value);
                    callEquals(ilGen);
                    ilGen.Emit(OpCodes.Brtrue, Item.Label);
                }

                ilGen.Emit(OpCodes.Br, defaultLabel);

                return;
            }

            var GtLabel = ilGen.DefineLabel();

            ldHashCode(ilGen);
            ilGen.LoadConstant(cases[index].Key);
            ilGen.Emit(OpCodes.Bgt, GtLabel);

            SwitchObject(
                ilGen, 
                ldValue,
                ldHashCode,
                callEquals,
                cases,
                defaultLabel,
                ldCaseValue,
                begin, 
                (begin + index) / 2,
                index);

            ilGen.MarkLabel(GtLabel);

            SwitchObject(
                ilGen,
                ldValue,
                ldHashCode,
                callEquals,
                cases,
                defaultLabel,
                ldCaseValue,
                index + 1,
                (index + 1 + end) / 2,
                end);
        }
    }

    /// <summary>
    /// 表示 Switch 的 Case 块
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class CaseInfo<T> : IComparable<CaseInfo<T>>
    {
        /// <summary>
        /// 获取 Case 块的值。
        /// </summary>
        public readonly T Value;
        /// <summary>
        /// 获取 Case 块的指令标签。
        /// </summary>
        public readonly Label Label;
        /// <summary>
        /// 获取或设置值的 HashCode 值。
        /// </summary>
        public int HashCode;

        /// <summary>
        /// 辅助变量。
        /// </summary>
        internal object Tag;

        /// <summary>
        /// 实例化 Case 块。
        /// </summary>
        /// <param name="Value">Case 块的值</param>
        /// <param name="Label">ase 块的指令标签</param>
        public CaseInfo(T Value, Label Label)
        {
            this.Value = Value;
            this.Label = Label;
        }

        /// <summary>
        /// 与另一个 Case 块信息比较 HashCode 的大小。
        /// </summary>
        /// <param name="other">Case 块信息</param>
        /// <returns>返回大于 0 则比它大，小于 0 则比它小，否则一样大</returns>
        public int CompareTo(CaseInfo<T> other)
        {
            return HashCode.CompareTo(other.HashCode);
        }
    }

}