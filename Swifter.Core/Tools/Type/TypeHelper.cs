using InlineIL;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;


namespace Swifter.Tools
{
    /// <summary>
    /// 提供类型信息的一些方法。
    /// </summary>
    public static class TypeHelper
    {
        private static bool OffsetOfByFieldDescIsAvailable
        {
            [MethodImpl(MethodImplOptions.NoInlining)]
            get
            {
                try
                {
                    if ((int)Marshal.OffsetOf(typeof(OffsetOfTest), nameof(OffsetOfTest.Field1)) == GetOffsetByFieldDesc(typeof(OffsetOfTest).GetField(nameof(OffsetOfTest.Field1))!) &&
                        (int)Marshal.OffsetOf(typeof(OffsetOfTest), nameof(OffsetOfTest.Field2)) == GetOffsetByFieldDesc(typeof(OffsetOfTest).GetField(nameof(OffsetOfTest.Field2))!) &&
                        (int)Marshal.OffsetOf(typeof(OffsetOfTest), nameof(OffsetOfTest.Field3)) == GetOffsetByFieldDesc(typeof(OffsetOfTest).GetField(nameof(OffsetOfTest.Field3))!))
                    {
                        return true;
                    }
                }
                catch
                {
                }

                return false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetOffsetByFieldDesc(FieldInfo fieldInfo)
        {
            IL.Push(fieldInfo.FieldHandle.Value);
            IL.Push(IntPtr.Size);
            IL.Emit.Add();
            IL.Push(sizeof(uint));
            IL.Emit.Add();
            IL.Emit.Ldind_U4();
            IL.Push(0x7ffffffU);
            IL.Emit.And();

            return IL.Return<int>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static unsafe int GetOffsetByIndexOfInstance(FieldInfo fieldInfo)
        {
            var declaringType = fieldInfo.DeclaringType ?? throw new NotSupportedException();

            if (declaringType.IsAbstract && fieldInfo.ReflectedType is Type reflectedType && !reflectedType.IsAbstract && declaringType.IsAssignableFrom(reflectedType))
            {
                declaringType = reflectedType;
            }

            try
            {
                var obj = Allocate(declaringType);

                if (obj is not null)
                {
                    var length = TypeHelper.GetAlignedNumInstanceFieldBytes(declaringType);

                    fixed (byte* ptr = &Unbox<byte>(obj))
                    {
                        Unsafe.InitBlock(ptr, 0xff, (uint)length);

                        try
                        {
                            fieldInfo.SetValue(obj, TypeHelper.GetDefaultValue(fieldInfo.FieldType));

                            for (int i = 0; i < length; i++)
                            {
                                if (ptr[i] is 0)
                                {
                                    return i;
                                }
                            }
                        }
                        finally
                        {
                            Unsafe.InitBlock(ptr, 0, (uint)length);
                        }
                    }
                }
            }
            catch
            {
            }

            return -1;
        }

        /// <summary>
        /// 获取一个字段的偏移量。如果是 Class 的字段则不包括 ObjectHandle 的大小。
        /// </summary>
        /// <param name="fieldInfo">字段信息</param>
        /// <returns>返回偏移量</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int OffsetOf(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsLiteral)
            {
                throw new ArgumentException("Unable get offset of a const field.", nameof(fieldInfo));
            }

            if (VersionDifferences.UseInternalMethod && OffsetOfByFieldDescIsAvailable)
            {
                return GetOffsetByFieldDesc(fieldInfo);
            }

            if (!fieldInfo.IsStatic && GetOffsetByIndexOfInstance(fieldInfo) is int offset && offset >= 0)
            {
                return offset;
            }

            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// 获取一个类型值的大小。
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>返回内存大小。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe int SizeOf(Type type)
        {
            return (Type.GetTypeCode(type)) switch
            {
                TypeCode.Boolean => sizeof(bool),
                TypeCode.Char => sizeof(char),
                TypeCode.SByte => sizeof(sbyte),
                TypeCode.Byte => sizeof(byte),
                TypeCode.Int16 => sizeof(short),
                TypeCode.UInt16 => sizeof(ushort),
                TypeCode.Int32 => sizeof(int),
                TypeCode.UInt32 => sizeof(uint),
                TypeCode.Int64 => sizeof(long),
                TypeCode.UInt64 => sizeof(ulong),
                TypeCode.Single => sizeof(float),
                TypeCode.Double => sizeof(double),
                TypeCode.Decimal => sizeof(decimal),
                TypeCode.DateTime => sizeof(DateTime),
                _ => GenericHelper.GetOrCreate(type).Size,
            };
        }

        /// <summary>
        /// 获取指定类型已对齐的实例字段 Bytes 数。
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetAlignedNumInstanceFieldBytes(Type type)
        {
            return GetNumInstanceFieldBytes(type) + (IntPtr.Size - 1) & (~(IntPtr.Size - 1));
        }

        /// <summary>
        /// 获取指定类型的实例字段 Bytes 数。
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetNumInstanceFieldBytes(Type type)
        {
            return type.GetAllInstanceFields().Sum(item => SizeOf(item.FieldType));
        }

        private static IEnumerable<FieldInfo> GetAllInstanceFields(this Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            if (type.BaseType is not null)
            {
                var baseFields = GetAllInstanceFields(type.BaseType);

                if (baseFields is ICollection<FieldInfo> collection && collection.Count is 0)
                {
                    return fields;
                }

                return baseFields.Concat(fields);
            }

            return fields;
        }

        /// <summary>
        /// 获取指定类型的默认值。
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object? GetDefaultValue(Type type)
        {
            return GenericHelper.GetOrCreate(type).GetDefaultValue();
        }

        /// <summary>
        /// 浅表克隆一个对象。
        /// </summary>
        /// <param name="obj">对象实例</param>
        /// <returns>返回一个新的对象实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object MemberwiseClone(object obj)
        {
            IL.Push(obj);
            IL.Emit.Call(MethodRef.Method(typeof(object), nameof(MemberwiseClone)));

            return IL.Return<object>();
        }

        /// <summary>
        /// 分配一个类型的实例。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe object Allocate(Type type)
        {
            // 我们认为 Allocate1 是完美的方法，所以当 Allocate1 不为空时直接执行即可。
            if (AllocateHelper.Allocate1 is not null)
            {
                var value = AllocateHelper.Allocate1(type);

                if (value is not null)
                {
                    return value;
                }
            }

            return InternalAllocate(type);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static object InternalAllocate(Type type)
            {
                if (AllocateHelper.Allocate2 is not null || AllocateHelper.Allocate3 is not null)
                {
                    try
                    {
                        var value
                            = AllocateHelper.Allocate2 is not null
                            ? AllocateHelper.Allocate2(type)
                            : AllocateHelper.Allocate3(type.TypeHandle.Value);

                        if (value is not null)
                        {
                            return value;
                        }
                    }
                    catch
                    {
                    }
                }

                if (AllocateHelper.Allocate4 is not null || AllocateHelper.Allocate5 is not null)
                {
                    try
                    {
                        var value
                            = AllocateHelper.Allocate4 is not null
                            ? AllocateHelper.Allocate4(type)
                            : AllocateHelper.Allocate5(type.TypeHandle.Value);

                        if (value is not null)
                        {
                            return value;
                        }
                    }
                    catch
                    {
                    }
                }

                return FormatterServices.GetUninitializedObject(type);
            }
        }

        /// <summary>
        /// 判断类型能否作为泛型参数。
        /// </summary>
        public static bool CanBeGenericParameter(this Type type)
        {
            return !type.IsByRef                        // 引用不能为泛型
                && !type.IsPointer                      // 指针类型不能为泛型
                && !type.IsGenericParameter             // 泛型参数不能为泛型
                && !type.IsGenericTypeDefinition        // 泛型类型定义不能为泛型
                && !(type.IsSealed && type.IsAbstract)  // 静态类型不能为泛型
                && type != typeof(void)                 // void 不能为泛型
                && !type.IsByRefLike()                  // 仅栈类型不能为泛型
                ;
        }

        /// <summary>
        /// 获取实例的 ObjectHandle 值。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <returns>返回 ObjectHandle 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref IntPtr GetMethodTablePointer(object obj)
        {
            IL.Emit.Ldarg_0();
            IL.Emit.Conv_I();

            return ref IL.ReturnRef<IntPtr>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="byteOffset"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T AddByteOffset<T>(ref T source, int byteOffset)
        {
            IL.Emit.Ldarg_0();
            IL.Emit.Ldarg_1();
            IL.Emit.Add();

            return ref IL.ReturnRef<T>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="byteOffset"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T SubtractByteOffset<T>(ref T source, int byteOffset)
        {
            IL.Emit.Ldarg_0();
            IL.Emit.Ldarg_1();
            IL.Emit.Sub();

            return ref IL.ReturnRef<T>();
        }

        /// <summary>
        /// 获取实例的 TypeHandle 值。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <returns>返回 ObjectHandle 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetTypeHandle(object obj)
        {
            if (VersionDifferences.TypeHandleEqualMethodTablePointer)
            {
                return GetMethodTablePointer(obj);
            }
            else
            {
                return obj.GetType().TypeHandle.Value;
            }
        }

        /// <summary>
        /// 获取类型的 TypeHandle 值。
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>返回 TypeHandle 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetTypeHandle(Type type)
        {
            return type.TypeHandle.Value;
        }

        /// <summary>
        /// 获取类型的 TypeHandle 值。
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns>返回 TypeHandle 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetTypeHandle<T>()
        {
            return InternalGetTypeHandle().Value;

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            static RuntimeTypeHandle InternalGetTypeHandle() 
                => IL.Ldtoken(typeof(T));
        }

        /// <summary>
        /// 判断一个值是否为空。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="value">值</param>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IsEmptyValue<T>([NotNullWhen(false)] T value)
        {
            if (default(T) is null) return value is null;

            return (Unsafe.SizeOf<T>()) switch
            {
                1 => Unsafe.As<T, byte>(ref value) is 0,
                2 => Unsafe.As<T, short>(ref value) is 0,
                4 => Unsafe.As<T, int>(ref value) is 0,
                8 => Unsafe.As<T, long>(ref value) is 0,
                var size => ArrayHelper.IsEmpty(ref Unsafe.As<T, byte>(ref value), size),
            };
        }

        /// <summary>
        /// 判断一个值是否是空。
        /// </summary>
        /// <param name="value">值</param>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IsEmptyValue([NotNullWhen(false)]object? value)
        {
            if (value is null)
            {
                return true;
            }

            var type = value.GetType();

            if (!type.IsValueType)
            {
                return false;
            }

            return Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean => Unbox<bool>(value) == default,
                TypeCode.Char => Unbox<char>(value) == default,
                TypeCode.SByte => Unbox<sbyte>(value) == default,
                TypeCode.Byte => Unbox<byte>(value) == default,
                TypeCode.Int16 => Unbox<short>(value) == default,
                TypeCode.UInt16 => Unbox<ushort>(value) == default,
                TypeCode.Int32 => Unbox<int>(value) == default,
                TypeCode.UInt32 => Unbox<uint>(value) == default,
                TypeCode.Int64 => Unbox<long>(value) == default,
                TypeCode.UInt64 => Unbox<ulong>(value) == default,
                TypeCode.Single => Unbox<float>(value) == default,
                TypeCode.Double => Unbox<double>(value) == default,
                TypeCode.Decimal => Unbox<decimal>(value) == default,
                TypeCode.DateTime => Unbox<DateTime>(value) == default,
                _ => GenericHelper.GetOrCreate(type).IsEmptyValue(value)
            };
        }

        /// <summary>
        /// 获取指定 Type 的特定索引。
        /// </summary>
        /// <param name="type">指定 Type</param>
        /// <param name="types">指定索引的参数</param>
        /// <returns>返回索引信息</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static PropertyInfo? GetProperty(this Type type, Type[] types)
        {
            foreach (var item in type.GetProperties())
            {
                var parameters = item.GetIndexParameters();

                if (parameters.Length == types.Length)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (types[i] != parameters[i].ParameterType)
                        {
                            goto Continue;
                        }
                    }

                    return item;
                }

                Continue:

                continue;
            }

            return null;
        }

        /// <summary>
        /// 获取已装箱值类型的引用。
        /// </summary>
        /// <typeparam name="T">引用的类型</typeparam>
        /// <param name="value">已装箱值</param>
        /// <returns>返回结构的引用</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T? Unbox<T>(object value)
        {
            return ref Unsafe.As<StructBox<T>>(value).Value;
        }

        /// <summary>
        /// 判断一个属性（实例或静态）是否为自动属性（无特殊处理，直接对一个字段读写的属性）。
        /// </summary>
        /// <param name="propertyInfo">属性信息</param>
        /// <param name="fieldInfo">返回一个字段信息</param>
        /// <returns>返回一个 bool 值。</returns>
        public static bool IsAutoProperty(this PropertyInfo propertyInfo, [NotNullWhen(true)] out FieldInfo? fieldInfo)
        {
            fieldInfo = null;

            /* getMethod 或 setMethod 不可能同时为 null */
            var getMethod = propertyInfo.GetGetMethod(true);
            var setMethod = propertyInfo.GetSetMethod(true);

            if (getMethod is null)
            {
                if (IsAutoSetMethod(setMethod!, out fieldInfo))
                {
                    return true;
                }

                return false;
            }

            if (setMethod is null)
            {
                if (IsAutoGetMethod(getMethod!, out fieldInfo))
                {
                    return true;
                }

                return false;
            }

            if (IsAutoGetMethod(getMethod, out var fieldByGet) && IsAutoSetMethod(setMethod, out var fieldBySet))
            {
                if (fieldByGet == fieldBySet)
                {
                    fieldInfo = fieldByGet;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 判断一个方法（实例或静态）是否为直接返回一个字段值的方法。
        /// </summary>
        /// <param name="methodInfo">方法信息</param>
        /// <param name="fieldInfo">返回字段信息</param>
        /// <returns>返回一个 bool 值。</returns>
        public static bool IsAutoGetMethod(this MethodInfo methodInfo, [NotNullWhen(true)] out FieldInfo? fieldInfo)
        {
            fieldInfo = null;

            if (methodInfo.GetParameters().Length != 0)
            {
                return false;
            }

            var ilBytes = methodInfo.GetMethodBody()?.GetILAsByteArray();

            if (ilBytes is null)
            {
                return false;
            }

            if (methodInfo.IsStatic)
            {
                if (!(ilBytes.Length == 6 &&
                    ilBytes[0] == OpCodes.Ldsfld.Value &&
                    ilBytes[5] == OpCodes.Ret.Value
                    ))
                {
                    return false;
                }

                fieldInfo = InternalResolveField(methodInfo.DeclaringType,InternalReadMetadataToken(ref ilBytes[1]));
            }
            else
            {
                if (!(ilBytes.Length == 7 &&
                    ilBytes[0] == OpCodes.Ldarg_0.Value &&
                    ilBytes[1] == OpCodes.Ldfld.Value &&
                    ilBytes[6] == OpCodes.Ret.Value
                    ))
                {
                    return false;
                }

                fieldInfo = InternalResolveField(methodInfo.DeclaringType, InternalReadMetadataToken(ref ilBytes[2]));
            }

            return fieldInfo is not null;
        }

        /// <summary>
        /// 判断一个方法（实例或静态）是否为直接设置一个字段值的方法。
        /// </summary>
        /// <param name="methodInfo">方法信息</param>
        /// <param name="fieldInfo">返回字段信息</param>
        /// <returns>返回一个 bool 值。</returns>
        public static bool IsAutoSetMethod(this MethodInfo methodInfo, [NotNullWhen(true)] out FieldInfo? fieldInfo)
        {
            fieldInfo = null;

            if (methodInfo.GetParameters().Length != 1)
            {
                return false;
            }

            var ilBytes = methodInfo.GetMethodBody()?.GetILAsByteArray();

            if (ilBytes is null)
            {
                return false;
            }

            if (methodInfo.IsStatic)
            {
                if (!(ilBytes.Length == 6 &&
                    ilBytes[0] == OpCodes.Ldarg_0.Value &&
                    ilBytes[1] == OpCodes.Stsfld.Value &&
                    ilBytes[6] == OpCodes.Ret.Value
                    ))
                {
                    return false;
                }

                fieldInfo = InternalResolveField(methodInfo.DeclaringType, InternalReadMetadataToken(ref ilBytes[2]));
            }
            else
            {
                if (!(ilBytes.Length == 8 &&
                    ilBytes[0] == OpCodes.Ldarg_0.Value &&
                    ilBytes[1] == OpCodes.Ldarg_1.Value &&
                    ilBytes[2] == OpCodes.Stfld.Value &&
                    ilBytes[7] == OpCodes.Ret.Value
                    ))
                {
                    return false;
                }

                fieldInfo = InternalResolveField(methodInfo.DeclaringType, InternalReadMetadataToken(ref ilBytes[3]));
            }

            return fieldInfo is not null;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static int InternalReadMetadataToken(ref byte firstByte)
        {
            var value = Unsafe.As<byte, int>(ref firstByte);

            return BitConverter.IsLittleEndian
                ? value
                : unchecked((int)BinaryPrimitives.ReverseEndianness((uint)value));
        }

        private static FieldInfo? InternalResolveField(Type? declaringType, int metadataToken)
        {
        Loop:

            if (declaringType is null)
            {
                return null;
            }

            if (declaringType.Module.ResolveField(metadataToken, declaringType.IsGenericType ? declaringType.GetGenericArguments() : null, null) is FieldInfo fieldInfo)
            {
                return fieldInfo;
            }

            declaringType = declaringType.BaseType;

            goto Loop;
        }

        /// <summary>
        /// 将原类型的值当作目标类型的值。
        /// </summary>
        /// <typeparam name="TFrom">原类型</typeparam>
        /// <typeparam name="TTo">目标类型</typeparam>
        /// <param name="source">原类型的值</param>
        /// <returns>返回目标类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TTo As<TFrom, TTo>(TFrom source) 
            => Unsafe.As<TFrom, TTo>(ref source);

        /// <summary>
        /// 尝试在所有程序集中通过类型名称获取类型。
        /// </summary>
        /// <param name="typeName">类型名称</param>
        /// <returns>返回类型信息</returns>
        public static Type? GetTypeForAllAssembly(string typeName)
        {
            if (Type.GetType(typeName) is Type result)
            {
                return result;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.GetType(typeName) is Type type)
                {
                    return type;
                }
            }

            return null;
        }

        /// <summary>
        /// 判断一个类型是否为资源类型。
        /// </summary>
        public static bool IsRecordType(this Type type)
        {
            if (type.IsClass && !type.IsAbstract)
            {
                foreach (var constructor in type.GetConstructors())
                {
                    var parameters = constructor.GetParameters();

                    if (parameters.Length is 0)
                    {
                        return false;
                    }

                    if (parameters.Length is 1 && parameters[0].ParameterType == type)
                    {
                        continue;
                    }

                    foreach (var parameter in parameters)
                    {
                        // 资源类型的构造函数参数名称由编译器设置，不为空且名称与属性名相同。
                        if (parameter.Name is not null) 
                        {
                            if (type.GetProperty(parameter.Name) is PropertyInfo propertyInfo && propertyInfo.PropertyType == parameter.ParameterType)
                            {
                                continue;
                            }

                            if (type.GetField(parameter.Name) is FieldInfo fieldInfo && fieldInfo.FieldType == parameter.ParameterType)
                            {
                                continue;
                            }
                        }

                        return false;
                    }

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 判断一个类型是否为元组类型（注意：这里区分元组和值类型元组）。
        /// </summary>
        public static bool IsTupleType(this Type type)
        {
            return type.IsClass && type.FullName is string fullName && fullName.StartsWith("System.Tuple");
        }

        /// <summary>
        /// 判断一个类型是否为值类型元组类型。
        /// </summary>
        public static bool IsValueTupleType(this Type type)
        {
            return type.IsValueType && type.FullName is string fullName && fullName.StartsWith("System.ValueTuple");
        }

        /// <summary>
        /// 判断一个成员是否为公开的。
        /// </summary>
        public static bool IsPublic(this MemberInfo memberInfo)
        {
            return memberInfo.MemberType switch
            {
                MemberTypes.Constructor => ((ConstructorInfo)memberInfo).IsPublic,
                MemberTypes.Method => ((MethodInfo)memberInfo).IsPublic,
                MemberTypes.Event => ((EventInfo)memberInfo).IsPublic(),
                MemberTypes.Field => ((FieldInfo)memberInfo).IsPublic,
                MemberTypes.Property => ((PropertyInfo)memberInfo).IsPublic(),
                MemberTypes.TypeInfo => ((Type)memberInfo).IsPublic,
                MemberTypes.NestedType => ((Type)memberInfo).IsNestedPublic,
                _ => /* TODO: 其他成员类型 */false,
            };
        }

        /// <summary>
        /// 判断一个事件是否为静态的。
        /// </summary>
        public static bool IsStatic(this EventInfo eventInfo)
        {
            return (eventInfo.GetAddMethod(true)?.IsStatic ?? eventInfo.GetRemoveMethod(true)?.IsStatic) == true;
        }

        /// <summary>
        /// 判断一个事件是否为共有的。
        /// </summary>
        public static bool IsPublic(this EventInfo eventInfo)
        {
            return (eventInfo.GetAddMethod(true)?.IsPublic ?? eventInfo.GetRemoveMethod(true)?.IsPublic) == true;
        }

        /// <summary>
        /// 判断一个属性是否为静态的。
        /// </summary>
        public static bool IsStatic(this PropertyInfo propertyInfo)
        {
            return (propertyInfo.GetGetMethod(true)?.IsStatic ?? propertyInfo.GetSetMethod(true)?.IsStatic) == true;
        }

        /// <summary>
        /// 判断一个属性是否为共有的。
        /// </summary>
        public static bool IsPublic(this PropertyInfo propertyInfo)
        {
            return (propertyInfo.GetGetMethod(true)?.IsPublic ?? propertyInfo.GetSetMethod(true)?.IsPublic) == true;
        }

        /// <summary>
        /// 判断一个类型是否为最终类。
        /// </summary>
        public static bool IsFinal(this Type type)
        {
            return type.IsValueType || type.IsSealed;
        }

        /// <summary>
        /// 判断一个类型是否为地址类型。<br/>
        /// 地址类型包括：<br/>
        /// 1. 指针类型，<br/>
        /// 2. 引用类型，<br/>
        /// 3. <see cref="IntPtr"/> 或 <see cref="UIntPtr"/><br/>
        /// </summary>
        public static bool IsAddressType(this Type type)
        {
            return type == typeof(IntPtr)
                || type == typeof(UIntPtr)
                || type.IsPointer
                || type.IsByRef;
        }

        /// <summary>
        /// 判断一个成员是否外部可见。
        /// </summary>
        /// <param name="memberInfo">成员信息</param>
        /// <returns>返回一个 <see cref="bool"/> 值</returns>
        public static bool IsExternalVisible(this MemberInfo memberInfo)
        {
            return memberInfo.IsPublic() && (memberInfo.DeclaringType?.IsExternalVisible() != false);
        }

        /// <summary>
        /// 使用指定类型调用泛型执行器。
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <param name="invoker">泛型执行器</param>
        public static void InvokeType(Type type, IGenericInvoker invoker)
        {
            GenericHelper.GetOrCreate(type).InvokeType(invoker);
        }

        /// <summary>
        /// 使用对象的类型调用泛型执行器。
        /// </summary>
        /// <param name="obj">指定对象</param>
        /// <param name="invoker">泛型执行器</param>
        public static void InvokeType(object obj, IGenericInvoker invoker)
        {
            GenericHelper.GetOrCreate(obj).InvokeType(invoker);
        }

        /// <summary>
        /// 通过方法句柄获取方法信息。
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="handle"/> 无效</exception>
        public static MethodInfo GetMethodFromHandle(RuntimeMethodHandle handle) 
            => MethodBase.GetMethodFromHandle(handle) as MethodInfo ?? throw new ArgumentException(nameof(handle));

        /// <summary>
        /// 通过方法句柄获取方法信息。
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="handle"/> 无效</exception>
        public static MethodInfo GetMethodFromHandle(RuntimeMethodHandle handle, RuntimeTypeHandle declaringType)
            => MethodBase.GetMethodFromHandle(handle, declaringType) as MethodInfo ?? throw new ArgumentException(nameof(handle));

        /// <summary>
        /// 通过方法句柄获取构造函数信息。
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="handle"/> 无效</exception>
        public static ConstructorInfo GetConstructorFromHandle(RuntimeMethodHandle handle) 
            => MethodBase.GetMethodFromHandle(handle) as ConstructorInfo ?? throw new ArgumentException(nameof(handle));

        /// <summary>
        /// 通过方法句柄获取构造函数信息。
        /// </summary>
        /// <exception cref="ArgumentException"><paramref name="handle"/> 无效</exception>
        public static ConstructorInfo GetConstructorFromHandle(RuntimeMethodHandle handle, RuntimeTypeHandle declaringType)
            => MethodBase.GetMethodFromHandle(handle, declaringType) as ConstructorInfo ?? throw new ArgumentException(nameof(handle));

        /// <summary>
        /// 判断类型能否作为指定泛型参数的类型。
        /// </summary>
        /// <param name="genericParameterType">泛型参数</param>
        /// <param name="type">类型</param>
        public static bool CanBeUsedGenericParameter(this Type genericParameterType, Type type)
        {
            if (!genericParameterType.IsGenericParameter)
            {
                return false;
            }

            if (!type.CanBeGenericParameter())
            {
                return false;
            }

            switch (genericParameterType.GenericParameterAttributes & GenericParameterAttributes.SpecialConstraintMask)
            {
                case GenericParameterAttributes.ReferenceTypeConstraint:
                    if (!type.IsClass)
                    {
                        return false;
                    }
                    break;
                case GenericParameterAttributes.NotNullableValueTypeConstraint:
                    if (type.CanBeNull())
                    {
                        return false;
                    }
                    break;
                case GenericParameterAttributes.DefaultConstructorConstraint:
                    if (type.GetConstructor(Type.EmptyTypes) is null)
                    {
                        return false;
                    }
                    break;
            }

            if (genericParameterType.BaseType is Type baseType && !type.IsSubclassOf(baseType))
            {
                return false;
            }

            foreach (var interfaceType in genericParameterType.GetInterfaces())
            {
                if (!interfaceType.IsAssignableFrom(type))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 判断类型的值是否可以为空。
        /// </summary>
        public static bool CanBeNull(this Type type)
        {
            return !type.IsValueType
                || Nullable.GetUnderlyingType(type) is not null
                || (type.IsGenericParameter && !type.GenericParameterAttributes.On(GenericParameterAttributes.NotNullableValueTypeConstraint));
        }

        static bool InternalSpeculateGenericParameters(Type parType, Type argType, Func<int, Type, bool> setGenericParameter)
        {
            while (parType.IsArray)
            {
                if (!argType.IsArray)
                {
                    return false;
                }

                parType = parType.GetElementType()!;
                argType = argType.GetElementType()!;
            }

            while (parType.IsPointer)
            {
                if (!argType.IsPointer)
                {
                    return false;
                }

                parType = parType.GetElementType()!;
                argType = argType.GetElementType()!;
            }

            if (parType.IsByRef)
            {
                if (!argType.IsByRef)
                {
                    return false;
                }

                parType = parType.GetElementType()!;
                argType = argType.GetElementType()!;
            }

            if (Nullable.GetUnderlyingType(parType) is Type parNullableUnderlyingType)
            {
                parType = parNullableUnderlyingType;
                argType = Nullable.GetUnderlyingType(argType) ?? argType;
            }

            if (parType.IsGenericParameter)
            {
                if (parType.CanBeUsedGenericParameter(argType))
                {
                    return setGenericParameter(parType.GenericParameterPosition, argType);
                }

                return false;
            }

            if (parType.IsGenericType)
            {
                var parTypeGenericTypeDefinition = parType.GetGenericTypeDefinition();

                if (parType.IsInterface)
                {
                    if (argType.IsInterface && argType.IsGenericType && argType.GetGenericTypeDefinition() == parTypeGenericTypeDefinition)
                    {
                        goto Speculates;
                    }

                    foreach (var interfaceType in argType.GetInterfaces())
                    {
                        if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == parTypeGenericTypeDefinition)
                        {
                            argType = interfaceType;

                            goto Speculates;
                        }
                    }

                    return false;
                }
                else if (parType.IsValueType)
                {
                    if (!argType.IsGenericType)
                    {
                        return false;
                    }

                    if (argType.GetGenericTypeDefinition() != parTypeGenericTypeDefinition)
                    {
                        return false;
                    }
                }
                else
                {
                    while (argType is not null)
                    {
                        if (!argType.IsGenericType)
                        {
                            return false;
                        }

                        if (argType.GetGenericTypeDefinition() == parTypeGenericTypeDefinition)
                        {
                            goto Speculates;
                        }

                        argType = argType.BaseType!;
                    }

                    return false;
                }

            Speculates:

                var parGenericTypes = parType.GetGenericArguments();
                var argGenericTypes = argType.GetGenericArguments();

                for (int i = 0; i < parGenericTypes.Length; i++)
                {
                    if (!InternalSpeculateGenericParameters(parGenericTypes[i], argGenericTypes[i], setGenericParameter))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 推测泛型参数。
        /// </summary>
        /// <param name="parType">参数定义类型</param>
        /// <param name="argType">参数传入类型</param>
        /// <param name="genericParameters">泛型参数列表</param>
        /// <returns>返回是否推测成功</returns>
        public static bool SpeculateGenericParameters(Type parType, Type argType, Type[] genericParameters)
        {
            return InternalSpeculateGenericParameters(parType, argType, (position, type) =>
            {
                ref var genericParameter = ref genericParameters[position];

                if (genericParameter.IsGenericParameter)
                {
                    genericParameter = type;

                    return true;
                }
                else if (XConvert.IsImplicitConvert(type, genericParameter))
                {
                    return true;
                }
                else if (XConvert.IsImplicitConvert(genericParameter, type))
                {
                    genericParameter = type;

                    return true;
                }

                return false;
            });
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct OffsetOfTest
        {
            public byte Field1;
            public int Field2;
            public long Field3;
        }

        private static unsafe class AllocateHelper
        {
            public static readonly delegate*<Type, object> Allocate1;

            public static readonly delegate*<Type, object> Allocate2;
            public static readonly delegate*<IntPtr, object> Allocate3;

            public static readonly delegate*<Type, object> Allocate4;
            public static readonly delegate*<IntPtr, object> Allocate5;

            static AllocateHelper()
            {
                const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

                {
                    if (typeof(RuntimeTypeHandle).GetMethod("Allocate", Flags) is MethodInfo methodInfo)
                    {
                        MethodHelper.GetSignature(methodInfo, out var parameterTypes, out var returnType);

                        if (returnType == typeof(object) && parameterTypes.Length == 1 && typeof(Type).IsAssignableFrom(parameterTypes[0]))
                        {
                            Allocate1 = (delegate*<Type, object>)methodInfo.MethodHandle.GetFunctionPointer();
                        }
                    }
                }

                {
                    if (TypeHelper.GetTypeForAllAssembly("System.StubHelpers.StubHelpers")?.GetMethod("AllocateInternal", Flags) is MethodInfo methodInfo)
                    {
                        MethodHelper.GetSignature(methodInfo, out var parameterTypes, out var returnType);

                        if (returnType == typeof(object) && parameterTypes.Length == 1 && typeof(Type).IsAssignableFrom(parameterTypes[0]))
                        {
                            Allocate2 = (delegate*<Type, object>)methodInfo.MethodHandle.GetFunctionPointer();
                        }

                        if (returnType == typeof(object) && parameterTypes.Length == 1 && parameterTypes[0] == typeof(IntPtr))
                        {
                            Allocate3 = (delegate*<IntPtr, object>)methodInfo.MethodHandle.GetFunctionPointer();
                        }
                    }
                }

                {
                    if (TypeHelper.GetTypeForAllAssembly("System.Runtime.Remoting.Activation.ActivationServices")?.GetMethod("AllocateUninitializedClassInstance", Flags) is MethodInfo methodInfo)
                    {
                        MethodHelper.GetSignature(methodInfo, out var parameterTypes, out var returnType);

                        if (returnType == typeof(object) && parameterTypes.Length == 1 && typeof(Type).IsAssignableFrom(parameterTypes[0]))
                        {
                            Allocate4 = (delegate*<Type, object>)methodInfo.MethodHandle.GetFunctionPointer();
                        }

                        if (returnType == typeof(object) && parameterTypes.Length == 1 && parameterTypes[0] == typeof(IntPtr))
                        {
                            Allocate5 = (delegate*<IntPtr, object>)methodInfo.MethodHandle.GetFunctionPointer();
                        }
                    }
                }
            }
        }

        internal abstract class GenericHelper
        {
            static readonly Dictionary<IntPtr, GenericHelper> Cache = new();

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public static GenericHelper GetOrCreate(Type type)
            {
                if (Cache.TryGetValue(GetTypeHandle(type), out var helper))
                {
                    return helper;
                }

                return InternalGetOrCreate(type);
            }

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public static GenericHelper GetOrCreate(object obj)
            {
                if (Cache.TryGetValue(GetTypeHandle(obj), out var helper))
                {
                    return helper;
                }

                return InternalGetOrCreate(obj);
            }

#if NET7_0_OR_GREATER
            [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Impl<>))]
#endif
            [MethodImpl(MethodImplOptions.NoInlining)]
            static GenericHelper InternalGetOrCreate(Type type)
            {
                lock (Cache)
                {
                    var key = GetTypeHandle(type);

                    if (!Cache.TryGetValue(key, out var helper))
                    {
                        try
                        {
                            var implType = typeof(Impl<>).MakeGenericType(type);

                            helper = (GenericHelper)Activator.CreateInstance(implType)!;
                        }
                        catch
                        {
                            helper = new NonGenericImpl(type);
                        }


                        Cache.Add(key, helper);
                    }

                    return helper;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            static GenericHelper InternalGetOrCreate(object obj)
            {
                return InternalGetOrCreate(obj.GetType());
            }

            public abstract void InvokeType(IGenericInvoker invoker);

            public abstract bool IsEmptyValue(object obj);

            public abstract int Size { get; }

            public abstract object? GetDefaultValue();

            public abstract T? XConvertTo<T>(object? value);

            public abstract object? XConvertFrom<T>(T? value);

            sealed class Impl<T> : GenericHelper
            {
                public override void InvokeType(IGenericInvoker invoker)
                {
                    invoker.Invoke<T>();
                }

                public override bool IsEmptyValue(object obj)
                {
                    return TypeHelper.IsEmptyValue((T)obj);
                }

                public override int Size => Unsafe.SizeOf<T>();

                public override object? GetDefaultValue() => default(T);

                public override object? XConvertFrom<TSource>([AllowNull] TSource value)
                {
                    return XConvert.Convert<TSource, T>(value);
                }

                [return: MaybeNull]
                public override TDestination XConvertTo<TDestination>(object? value)
                {
                    return XConvert.Convert<T, TDestination>((T)value!);
                }
            }

            sealed class NonGenericImpl : GenericHelper
            {
                public readonly Type Type;

                public NonGenericImpl(Type type)
                {
                    Type = type;
                }

                public override int Size
                {
                    get
                    {
                        if (Type.IsPointer || Type.IsByRef)
                        {
                            return IntPtr.Size;
                        }

                        if (Type.IsByRefLike())
                        {
                            return TypeHelper.GetNumInstanceFieldBytes(Type);
                        }

                        return 0;
                    }
                }

                public override object? GetDefaultValue()
                {
                    throw new NotSupportedException();
                }

                public override void InvokeType(IGenericInvoker invoker)
                {
                    throw new NotSupportedException();
                }

                public override bool IsEmptyValue(object obj)
                {
                    return false;
                }

                public override object? XConvertFrom<T>([AllowNull] T value)
                {
                    return XConvert.Convert(value, Type);
                }

                [return: MaybeNull]
                public override T XConvertTo<T>(object? value)
                {
                    return XConvert.Convert<T>(value);
                }
            }
        }
    }
}