using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;


namespace Swifter.Tools
{
    /// <summary>
    /// 提供类型信息的一些方法。
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// 引用比较器。
        /// </summary>
        public static IEqualityComparer<object> ReferenceComparer => Tools.ReferenceComparer.Instance;

        /// <summary>
        /// 获取一个字段的偏移量。如果是 Class 的字段则不包括 ObjectHandle 的大小。
        /// </summary>
        /// <param name="fieldInfo">字段信息</param>
        /// <returns>返回偏移量</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int OffsetOf(FieldInfo fieldInfo)
        {
            if (fieldInfo is null)
            {
                throw new ArgumentNullException(nameof(fieldInfo));
            }

            if (fieldInfo.IsLiteral)
            {
                throw new ArgumentException("Unable get offset of a const field.", nameof(fieldInfo));
            }

            if (VersionDifferences.UseInternalMethod && OffsetHelper.OffsetOfByFieldDescIsAvailable)
            {
                return OffsetHelper.GetOffsetByFieldDesc(fieldInfo);
            }

            if (!fieldInfo.IsStatic && OffsetHelper.GetOffsetByIndexOfInstance(fieldInfo) is int offset && offset >= 0)
            {
                return offset;
            }

            if (VersionDifferences.IsSupportEmit)
            {
                return OffsetHelper.GetOffsetByEmit(fieldInfo);
            }

            throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// 获取对象中的值的偏移量。
        /// </summary>
        /// <returns>返回一个 <see cref="int"/> 值</returns>
        public static unsafe int GetObjectValueByteOffset()
        {
            return AllocateHelper.ObjectValueByteOffset;
        }

        /// <summary>
        /// 获取一个类型值的大小。
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>返回内存大小。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int SizeOf(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type is TypeBuilder)
            {
                throw new NotSupportedException(nameof(type));
            }

            if (type.IsGenericTypeDefinition)
            {
                throw new NotSupportedException(nameof(type));
            }

            if (type.IsByRefLike())
            {
                // TODO: ref struct 的大小是所有字段大小的和。
                throw new NotSupportedException(nameof(type));
            }

            if (type.IsByRef || type.IsPointer || !type.IsValueType || type.IsClass || type.IsInterface || type.IsArray)
            {
                return IntPtr.Size;
            }

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
                _ => GenericInvokerHelper.SizeOf(type),
            };
        }

        /// <summary>
        /// 获取指定类型已对齐的实例字段 Bytes 数。
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int GetAlignedNumInstanceFieldBytes(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type is TypeBuilder)
            {
                throw new NotSupportedException(nameof(type));
            }

            if (type.IsGenericTypeDefinition)
            {
                throw new NotSupportedException(nameof(type));
            }

            if (GenericInvokerHelper.GetBaseSizeIsAvailable && VersionDifferences.UseInternalMethod)
            {
                return GenericInvokerHelper.GetBaseSize(type) - GenericInvokerHelper.GetBaseSizePadding(type);
            }

            return GenericInvokerHelper.GetAlignedNumInstanceFieldBytes(type);
        }

        /// <summary>
        /// 获取指定类型的实例字段 Bytes 数。
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns>返回一个 int 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetNumInstanceFieldBytes(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type is TypeBuilder)
            {
                throw new NotSupportedException(nameof(type));
            }

            if (type.IsGenericTypeDefinition)
            {
                throw new NotSupportedException(nameof(type));
            }

            return GenericInvokerHelper.GetNumInstanceFieldBytes(type);
        }

        /// <summary>
        /// 获取托管引用的值引用。
        /// </summary>
        /// <typeparam name="T">需要获取的引用类型</typeparam>
        /// <param name="reference">托管引用</param>
        /// <returns>返回值引用</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe ref T RefValue<T>(TypedReference reference)
        {
            return ref Underlying.AsRef<T>(*(byte**)&reference);
        }

        /// <summary>
        /// 获取指定类型的默认值。
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns>返回该类型的默认值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object GetDefaultValue(Type type)
        {
            if (type.IsValueType)
            {
                return GenericInvokerHelper.GetDefaultValue(type);
            }

            return null;
        }

        /// <summary>
        /// 获取一个指定类型的实例需要分配的堆大小。
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns>返回一个 int 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe int GetBaseSize(Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type is TypeBuilder)
            {
                throw new NotSupportedException(nameof(type));
            }

            if (type.IsGenericTypeDefinition)
            {
                throw new NotSupportedException(nameof(type));
            }

            if (GenericInvokerHelper.GetBaseSizeIsAvailable && VersionDifferences.UseInternalMethod)
            {
                return GenericInvokerHelper.GetBaseSize(type);
            }

            return Math.Max(GenericInvokerHelper.GetAlignedNumInstanceFieldBytes(type), GetObjectValueByteOffset()) + IntPtr.Size + GetObjectValueByteOffset();
        }

        /// <summary>
        /// 浅表克隆一个对象。
        /// </summary>
        /// <param name="obj">对象实例</param>
        /// <returns>返回一个新的对象实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object MemberwiseClone(object obj)
        {
            if (obj is null) return null;
            if (obj is string str) return CloneHelper.CloneString(str);
            if (obj is Array array) return CloneHelper.CloneArray(array);

            return CloneHelper.MemberwiseClone(obj);
        }

        /// <summary>
        /// 分配一个类型的实例。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object Allocate(Type type)
        {
            //if (type.IsArray)
            //{
            //    var elementType = type.GetElementType();

            //    if (type.GetArrayRank() == 1)
            //    {
            //        return Array.CreateInstance(elementType, 0);
            //    }

            //    return Array.CreateInstance(elementType, new int[type.GetArrayRank()]);
            //}

            //if (type == typeof(string))
            //{
            //    return StringHelper.MakeString(0);
            //}

            //if (VersionDifferences.TypeHandleEqualMethodTablePointer && VersionDifferences.UseInternalMethod)
            //{
            //    var m_BaseSize = GetBaseSize(type) - GenericInvokerHelper.ObjectBaseSize;
            //    var r_Obj = default(object);

            //    if (m_BaseSize == 0)
            //    {
            //        r_Obj = new object();
            //    }

            //    if (m_BaseSize > 0)
            //    {
            //        r_Obj = new byte[m_BaseSize];
            //    }

            //    if (r_Obj != null)
            //    {
            //        Underlying.GetMethodTablePointer(r_Obj) = type.TypeHandle.Value;

            //        Underlying.As<StructBox<IntPtr>>(r_Obj).Value = IntPtr.Zero;

            //        return r_Obj;
            //    }
            //}

            foreach (var allocate in AllocateHelper.AllocateMethods)
            {
                try
                {
                    return allocate(type);
                }
                catch
                {
                }
            }

            return FormatterServices.GetUninitializedObject(type);
        }

        /// <summary>
        /// 判断一个类型能否作为泛型参数。
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>返回一个 bool 值</returns>
        public static bool CanBeGenericParameter(this Type type)
        {
            return !type.IsByRef // 引用不能为泛型
                && !type.IsByRefLike()  // 仅栈类型不能为泛型
                && !type.IsPointer  // 指针类型不能为泛型
                && !(type.IsSealed && type.IsAbstract) // 静态类型不能为泛型
                && type != typeof(void) // void 不能为泛型
                ;
        }

        /// <summary>
        /// 获取实例的 ObjectHandle 值。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <returns>返回 ObjectHandle 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetMethodTablePointer(object obj)
        {
            return Underlying.GetMethodTablePointer(obj);
        }

        /// <summary>
        /// 获取实例的 TypeHandle 值。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <returns>返回 ObjectHandle 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetTypeHandle(object obj)
        {
            return VersionDifferences.GetTypeHandle(obj);
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
        /// 获取类型的 ObjectHandle 值。
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>返回 ObjectHandle 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetMethodTablePointer(Type type)
        {
            if (VersionDifferences.TypeHandleEqualMethodTablePointer && !type.IsArray)
            {
                return type.TypeHandle.Value;
            }

            return GetMethodTablePointer(Allocate(type));
        }

        /// <summary>
        /// 获取类型的托管静态字段存储内存的地址。
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>返回内存地址。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetGCStaticsBasePointer(Type type)
        {
            return GenericInvokerHelper.GetStaticsBasePointer(type, StaticsBaseBlock.GC);
        }

        /// <summary>
        /// 获取类型的非托管静态字段存储内存的地址。
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>返回内存地址。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetNonGCStaticsBasePointer(Type type)
        {
            return GenericInvokerHelper.GetStaticsBasePointer(type, StaticsBaseBlock.NonGC);
        }

        /// <summary>
        /// 获取类型的非托管静态字段存储内存的地址。
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>返回内存地址。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetThreadGCStaticsBasePointer(Type type)
        {
            return GenericInvokerHelper.GetStaticsBasePointer(type, StaticsBaseBlock.ThreadGC);
        }

        /// <summary>
        /// 获取类型的非托管静态字段存储内存的地址。
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>返回内存地址。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetThreadNonGCStaticsBasePointer(Type type)
        {
            return GenericInvokerHelper.GetStaticsBasePointer(type, StaticsBaseBlock.ThreadNonGC);
        }


        /// <summary>
        /// 获取静态字段所在堆内存的地址。
        /// </summary>
        /// <param name="fieldInfo">静态字段信息</param>
        /// <returns>返回内存地址。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetStaticsBasePointer(FieldInfo fieldInfo)
        {
            return GenericInvokerHelper.GetStaticsBasePointer(fieldInfo.DeclaringType, GetStaticBaseBlock(fieldInfo));
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static StaticsBaseBlock GetStaticBaseBlock(FieldInfo fieldInfo)
        {
            StaticsBaseBlock ret = default;

            if (!fieldInfo.FieldType.IsValueType)
            {
                ret |= (StaticsBaseBlock)0x1;
            }

            if (fieldInfo.IsThreadStatic())
            {
                ret |= (StaticsBaseBlock)0x2;
            }

            return ret;
        }

        /// <summary>
        /// 判断一个字段是否为线程静态字段。
        /// </summary>
        /// <param name="fieldInfo">静态字段信息</param>
        /// <returns>返回一个不二值</returns>
        public static bool IsThreadStatic(this FieldInfo fieldInfo)
        {
            if (VersionDifferences.IsSupportEmit && OffsetHelper.OffsetOfByFieldDescIsAvailable)
            {
                // TODO: 支持 OffsetOf 不代表也支持 IsThreadStatic。

                return OffsetHelper.IsThreadStaticOfByFieldDesc(fieldInfo);
            }

            if (fieldInfo.IsDefined(typeof(ThreadStaticAttribute), false))
            {
                return true;
            }

            // TODO: 尝试其他方案获取。

            return false;
        }

        /// <summary>
        /// 判断一个值是否为空。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="value">值</param>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IsEmptyValue<T>(T value)
        {
            var size = Underlying.SizeOf<T>();

            switch (size)
            {
                case 0:
                    return true;
                case 1:
                    return Underlying.As<T, byte>(ref value) == 0;
                case 2:
                    return Underlying.As<T, short>(ref value) == 0;
                case 4:
                    return Underlying.As<T, int>(ref value) == 0;
                case 8:
                    return Underlying.As<T, long>(ref value) == 0;
            }

            ref var first = ref Underlying.As<T, byte>(ref value);

            while (size >= 8)
            {
                size -= 8;

                if (Underlying.As<byte, long>(ref Underlying.Add(ref first, size)) != 0)
                {
                    return false;
                }
            }

            if (size >= 4)
            {
                size -= 4;

                if (Underlying.As<byte, int>(ref Underlying.Add(ref first, size)) != 0)
                {
                    return false;
                }
            }

            if (size >= 2)
            {
                size -= 2;

                if (Underlying.As<byte, short>(ref Underlying.Add(ref first, size)) != 0)
                {
                    return false;
                }
            }

            if (size >= 1)
            {
                if (Underlying.As<byte, byte>(ref first) != 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 判断一个值是否是空。
        /// </summary>
        /// <param name="value">值</param>
        /// <returns>返回一个 bool 值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool IsEmptyValue(object value)
        {
            if (value is null)
            {
                return true;
            }

            var isValueType = value.GetType().IsValueType;

            if (isValueType)
            {
                return GenericInvokerHelper.IsEmptyValue(value);
            }

            return false;
        }

        /// <summary>
        /// 获取指定 Type 的特定索引。
        /// </summary>
        /// <param name="type">指定 Type</param>
        /// <param name="types">指定索引的参数</param>
        /// <returns>返回索引信息</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static PropertyInfo GetProperty(this Type type, Type[] types)
        {
            type = type ?? throw new ArgumentNullException(nameof(type));
            types = types ?? throw new ArgumentNullException(nameof(types));

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
        /// 比较参数类型集合和参数集合是否兼容。
        /// </summary>
        /// <param name="parametersTypes">参数类型集合。</param>
        /// <param name="inputParameters">参数集合。</param>
        /// <returns>返回兼容或不兼容。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool ParametersCompares(Type[] parametersTypes, object[] inputParameters)
        {
            for (int i = 0; i < parametersTypes.Length; i++)
            {
                if (parametersTypes[i].IsInstanceOfType(inputParameters[i]) || (parametersTypes[i].IsByRef && parametersTypes[i].GetElementType().IsInstanceOfType(inputParameters[i])))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 比较两个参数类型集合是否一致。
        /// </summary>
        /// <param name="parametersTypes">参数类型集合。</param>
        /// <param name="inputParameters">参数类型集合。</param>
        /// <returns>返回一致或不一致。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static bool ParametersCompares(Type[] parametersTypes, Type[] inputParameters)
        {
            for (int i = 0; i < parametersTypes.Length; i++)
            {
                if (parametersTypes[i] != inputParameters[i])
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 使用反射获取指定静态成员的值。
        /// </summary>
        /// <typeparam name="T">成员类型</typeparam>
        /// <param name="type">定义该成员的类</param>
        /// <param name="staticMemberName">成员名称</param>
        /// <returns>返回该静态成员的值</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T SlowGetValue<T>(Type type, string staticMemberName)
        {
            return InternalSlowGetValue<T>(type.GetMember(staticMemberName,
                BindingFlags.Static |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.GetField |
                BindingFlags.GetProperty)[0], null);
        }

        /// <summary>
        /// 使用反射获取指定实例成员的值。
        /// </summary>
        /// <typeparam name="T">成员类型</typeparam>
        /// <param name="instance">定义该成员的对象</param>
        /// <param name="memberName">成员名称</param>
        /// <returns>返回该实例成员的值</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static T SlowGetValue<T>(object instance, string memberName)
        {
            return InternalSlowGetValue<T>(instance.GetType().GetMember(memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.GetField |
                BindingFlags.GetProperty)[0], instance);
        }

        private static T InternalSlowGetValue<T>(MemberInfo memberInfo, object instance)
        {
            if (memberInfo is FieldInfo fieldInfo)
            {
                if (fieldInfo.IsLiteral)
                {
                    return (T)fieldInfo.GetRawConstantValue();
                }

                return (T)fieldInfo.GetValue(instance);
            }
            else if (memberInfo is PropertyInfo propertyInfo)
            {
                return (T)propertyInfo.GetValue(instance, null);
            }
            else
            {
                throw new MissingMemberException();
            }
        }

        /// <summary>
        /// 获取已装箱值类型的引用。
        /// </summary>
        /// <typeparam name="T">引用的类型</typeparam>
        /// <param name="value">已装箱值</param>
        /// <returns>返回结构的引用</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref T Unbox<T>(object value)
        {
            return ref Underlying.As<StructBox<T>>(value).Value;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        internal static unsafe object CamouflageBox<T>(void* ptr)
        {
            return Underlying.As<IntPtr, StructBox<T>>(ref Underlying.AsRef((IntPtr)((byte*)ptr - GetObjectValueByteOffset())));
        }

        /// <summary>
        /// 获取类型已定义的指定类型的特性集合，没有则空数组。
        /// </summary>
        /// <typeparam name="TAttribute">特性类型</typeparam>
        /// <param name="type">需要查找的类型</param>
        /// <param name="inherit">是否查询类型的父级，直到 object。</param>
        /// <returns>返回特性数组</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static TAttribute[] GetDefinedAttributes<TAttribute>(this Type type, bool inherit) where TAttribute : Attribute
        {
            var list = new List<TAttribute>();

            Internal(type);

            return list.ToArray();

            void Internal(Type t)
            {
                if (inherit && t.BaseType != null)
                {
                    Internal(t.BaseType);
                }

                list.AddRange(t.GetCustomAttributes(typeof(TAttribute), false).OfType<TAttribute>());
            }
        }

        /// <summary>
        /// 判断一个属性（实例或静态）是否为自动属性（无特殊处理，直接对一个字段读写的属性）。
        /// </summary>
        /// <param name="propertyInfo">属性信息</param>
        /// <param name="fieldInfo">返回一个字段信息</param>
        /// <returns>返回一个 bool 值。</returns>
        public static bool IsAutoProperty(PropertyInfo propertyInfo, out FieldInfo fieldInfo)
        {
            fieldInfo = default;

            if (propertyInfo is null)
            {
                return false;
            }

            var getMethod = propertyInfo.GetGetMethod(true);
            var setMethod = propertyInfo.GetSetMethod(true);

            if (getMethod is null)
            {
                if (IsAutoSetMethod(setMethod, out var fieldMetadataToken))
                {
                    fieldInfo = GetMemberByMetadataToken<FieldInfo>(propertyInfo.DeclaringType, fieldMetadataToken);

                    return true;
                }

                return false;
            }

            if (setMethod is null)
            {
                if (IsAutoGetMethod(getMethod, out var fieldMetadataToken))
                {
                    fieldInfo = GetMemberByMetadataToken<FieldInfo>(propertyInfo.DeclaringType, fieldMetadataToken);

                    return true;
                }

                return false;
            }

            if (IsAutoGetMethod(getMethod, out var getFieldMetadataToken) && IsAutoSetMethod(setMethod, out var setFieldMetadataToken))
            {
                if (getFieldMetadataToken == setFieldMetadataToken)
                {
                    fieldInfo = GetMemberByMetadataToken<FieldInfo>(propertyInfo.DeclaringType, getFieldMetadataToken);

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 判断一个方法（实例或静态）是否为直接返回一个字段值的方法。
        /// </summary>
        /// <param name="methodInfo">方法信息</param>
        /// <param name="fieldMetadataToken">返回字段的元数据标识</param>
        /// <returns>返回一个 bool 值。</returns>
        public static bool IsAutoGetMethod(MethodInfo methodInfo, out int fieldMetadataToken)
        {
            fieldMetadataToken = default;

            if (methodInfo is null)
            {
                return false;
            }

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

                fieldMetadataToken = ReadMetadataToken(ref ilBytes[1]);
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

                fieldMetadataToken = ReadMetadataToken(ref ilBytes[2]);
            }

            return true;
        }

        /// <summary>
        /// 判断一个方法（实例或静态）是否为直接设置一个字段值的方法。
        /// </summary>
        /// <param name="methodInfo">方法信息</param>
        /// <param name="fieldMetadataToken">返回字段的元数据标识</param>
        /// <returns>返回一个 bool 值。</returns>
        public static bool IsAutoSetMethod(MethodInfo methodInfo, out int fieldMetadataToken)
        {
            fieldMetadataToken = default;

            if (methodInfo is null)
            {
                return false;
            }

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

                fieldMetadataToken = ReadMetadataToken(ref ilBytes[2]);
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

                fieldMetadataToken = ReadMetadataToken(ref ilBytes[3]);
            }

            return true;
        }

        /// <summary>
        /// 通过元数据根获取成员信息。
        /// </summary>
        /// <typeparam name="TMemberInfo">期望获取的成员类型</typeparam>
        /// <param name="declaringType">成员的定义类</param>
        /// <param name="metadataToken">元数据根</param>
        /// <returns>返回一个成员信息或 null。</returns>
        public static TMemberInfo GetMemberByMetadataToken<TMemberInfo>(Type declaringType, int metadataToken) where TMemberInfo : MemberInfo
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            try
            {
                if (declaringType.IsGenericType)
                {
                    if (declaringType.Module.ResolveMember(metadataToken, declaringType.GetGenericArguments(), default) is TMemberInfo ret)
                    {
                        return ret;
                    }
                }
                else
                {
                    if (declaringType.Module.ResolveMember(metadataToken) is TMemberInfo ret)
                    {
                        return ret;
                    }
                }
            }
            catch
            {
            }

            return Loop(declaringType);

            TMemberInfo Loop(Type type)
            {
                if (type is null)
                {
                    return default;
                }

                foreach (var item in type.GetMembers(flags))
                {
                    if (item.MetadataToken == metadataToken && item is TMemberInfo ret)
                    {
                        return ret;
                    }
                }

                return Loop(type.BaseType);
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        private static int ReadMetadataToken(ref byte firstByte)
        {
            var value = Underlying.As<byte, int>(ref firstByte);

            return BitConverter.IsLittleEndian
                ? value
                : unchecked((int)BinaryPrimitives.ReverseEndianness((uint)value));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TFrom"></typeparam>
        /// <typeparam name="TTo"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static TTo As<TFrom, TTo>(TFrom source) => Underlying.As<TFrom, TTo>(ref source);

        /// <summary>
        /// 尝试在所有程序集中通过类型名称获取类型。
        /// </summary>
        /// <param name="typeName">类型名称</param>
        /// <returns>返回类型信息</returns>
        public static Type GetTypeForAllAssembly(string typeName)
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

        internal static IEnumerable<Type> GetTypesForAllAssembly()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in assembly.GetTypes())
                {
                    yield return type;
                }
            }
        }

        internal static IEnumerable<FieldInfo> GetFields(Type type, BindingFlags bindingFlags)
        {
            if (type.BaseType != null)
            {
                foreach (var fieldInfo in GetFields(type.BaseType, bindingFlags))
                {
                    yield return fieldInfo;
                }
            }

            foreach (var field in type.GetFields(bindingFlags))
            {
                yield return field;
            }
        }

        /// <summary>
        /// 判断一个成员是否为共有的。
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
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
                MemberTypes.NestedType => ((Type)memberInfo).IsPublic,
                _ => /* TODO:  */false,
            };
        }

        /// <summary>
        /// 判断一个事件是否为静态的。
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <returns></returns>
        public static bool IsStatic(this EventInfo eventInfo)
        {
            return (eventInfo.GetAddMethod(true)?.IsStatic ?? eventInfo.GetRemoveMethod(true)?.IsStatic) == true;
        }

        /// <summary>
        /// 判断一个事件是否为共有的。
        /// </summary>
        /// <param name="eventInfo"></param>
        /// <returns></returns>
        public static bool IsPublic(this EventInfo eventInfo)
        {
            return (eventInfo.GetAddMethod(true)?.IsPublic ?? eventInfo.GetRemoveMethod(true)?.IsPublic) == true;
        }

        /// <summary>
        /// 判断一个属性是否为静态的。
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static bool IsStatic(this PropertyInfo propertyInfo)
        {
            return (propertyInfo.GetGetMethod(true)?.IsStatic ?? propertyInfo.GetSetMethod(true)?.IsStatic) == true;
        }

        /// <summary>
        /// 判断一个属性是否为共有的。
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static bool IsPublic(this PropertyInfo propertyInfo)
        {
            return (propertyInfo.GetGetMethod(true)?.IsPublic ?? propertyInfo.GetSetMethod(true)?.IsPublic) == true;
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
    }
}