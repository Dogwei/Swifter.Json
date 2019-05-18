using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供类型信息的一些方法。
    /// </summary>
    public static class TypeHelper
    {
        /// <summary>
        /// 获取一个字段的偏移量。如果是 Class 的字段则不包括 ObjectHandle 的大小。
        /// </summary>
        /// <param name="fieldInfo">字段信息</param>
        /// <returns>返回偏移量</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int OffsetOf(FieldInfo fieldInfo)
        {
            if (fieldInfo == null)
            {
                throw new ArgumentNullException(nameof(fieldInfo));
            }

            if (fieldInfo.IsLiteral)
            {
                throw new ArgumentException("Unable get offset of a const field.", nameof(fieldInfo));
            }

            if (OffsetHelper.OffsetOfByHandleIsAvailable)
            {
                return OffsetHelper.OffsetOfByHandle(fieldInfo);
            }

            return OffsetHelper.GetOffsetByDynamic(fieldInfo);
        }

        /// <summary>
        /// 获取一个类型占用的内存大小。如果是 Class 则不包括 ObjectHandle 的大小。
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>返回内存大小。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public unsafe static int SizeOf(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type is TypeBuilder)
            {
                throw new ArgumentException("Unable get size of a TypeBuilder.", nameof(type));
            }

            if (type.IsGenericTypeDefinition)
            {
                throw new ArgumentException("Unable get size of a generic definition type.", nameof(type));
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return sizeof(bool);
                case TypeCode.Char:
                    return sizeof(char);
                case TypeCode.SByte:
                    return sizeof(sbyte);
                case TypeCode.Byte:
                    return sizeof(byte);
                case TypeCode.Int16:
                    return sizeof(short);
                case TypeCode.UInt16:
                    return sizeof(ushort);
                case TypeCode.Int32:
                    return sizeof(int);
                case TypeCode.UInt32:
                    return sizeof(uint);
                case TypeCode.Int64:
                    return sizeof(long);
                case TypeCode.UInt64:
                    return sizeof(ulong);
                case TypeCode.Single:
                    return sizeof(float);
                case TypeCode.Double:
                    return sizeof(double);
                case TypeCode.Decimal:
                    return sizeof(decimal);
                case TypeCode.DateTime:
                    return sizeof(DateTime);
            }

            if (type.IsPointer ||
                type.IsInterface ||
                type.IsByRef ||
                typeof(object) == type ||
                typeof(IntPtr) == type)
            {
                return IntPtr.Size;
            }

            return GenericInvokerHelper.SizeOf(type);
        }

        /// <summary>
        /// 克隆一个值或对象。
        /// </summary>
        /// <typeparam name="T">值或对象的类型</typeparam>
        /// <param name="obj">值或对象</param>
        /// <returns>返回一个新的值或对象</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T Clone<T>(T obj)
        {
            if (TypeInfo<T>.IsValueType)
            {
                return obj;
            }

            if (obj == null)
            {
                return default;
            }

            if (obj is string str)
            {
                return Unsafe.As<string, T>(ref str);
            }

            if (obj is ICloneable)
            {
                return (T)((ICloneable)obj).Clone();
            }

            return (T)CloneHelper.FuncMemberwiseClone(obj);
        }

        /// <summary>
        /// 分配一个类型的实例。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object Allocate(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (type.IsArray)
            {
                var lengths = new int[type.GetArrayRank()];

                return Array.CreateInstance(type.GetElementType(), lengths);
            }

            if (type == typeof(string))
            {
                return "";
            }

            return FormatterServices.GetUninitializedObject(type);
        }

        /// <summary>
        /// 获取实例的 ObjectHandle 值。
        /// </summary>
        /// <param name="obj">实例</param>
        /// <returns>返回 ObjectHandle 值。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetObjectTypeHandle(object obj)
        {
            return Unsafe.GetObjectTypeHandle(obj);
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
        public static IntPtr GetObjectTypeHandle(Type type)
        {
            if (VersionDifferences.ObjectTypeHandleEqualsTypeHandle && !type.IsArray)
            {
                return type.TypeHandle.Value;
            }

            var obj = Allocate(type);

            if (obj == null)
            {
                VersionNotSupport(nameof(GetObjectTypeHandle));
            }

            return GetObjectTypeHandle(obj);
        }
        
        /// <summary>
        /// 获取类型的静态字段存储内存的地址。
        /// </summary>
        /// <param name="type">类型信息</param>
        /// <returns>返回内存地址。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static IntPtr GetTypeStaticMemoryAddress(Type type)
        {
            return GenericInvokerHelper.GetTypeStaticMemoryAddress(type);
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
            var size = Unsafe.SizeOf<T>();

            switch (size)
            {
                case 0:
                    return true;
                case 1:
                    return Unsafe.As<T, byte>(ref value) == 0;
                case 2:
                    return Unsafe.As<T, short>(ref value) == 0;
                case 4:
                    return Unsafe.As<T, int>(ref value) == 0;
                case 8:
                    return Unsafe.As<T, long>(ref value) == 0;
            }

            ref var first = ref Unsafe.AsByte(ref value);

            while (size >= 8)
            {
                size -= 8;

                if (Unsafe.As<byte, long>(ref Unsafe.Add(ref first, size)) != 0)
                {
                    return false;
                }
            }

            if (size >= 4)
            {
                size -= 4;

                if (Unsafe.As<byte, int>(ref Unsafe.Add(ref first, size)) != 0)
                {
                    return false;
                }
            }

            if (size >= 2)
            {
                size -= 2;

                if (Unsafe.As<byte, short>(ref Unsafe.Add(ref first, size)) != 0)
                {
                    return false;
                }
            }

            if (size >= 1)
            {
                if (Unsafe.As<byte, byte>(ref first) != 0)
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
            if (value == null)
            {
                return true;
            }

            return GenericInvokerHelper.IsEmptyValue(value);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void VersionNotSupport(string methodName)
        {
            throw new PlatformNotSupportedException($"current .net version not support '{methodName}' method.");
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
            return ref Unsafe.As<StructBox<T>>(value).Value;
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
        /// 获取指定完全名称的类型信息集合。
        /// </summary>
        /// <param name="fullName">指定类型名称</param>
        /// <returns>返回一个类型集合</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IEnumerable<Type> GetTypes(string fullName)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var item in assemblies)
            {
                var result = item.GetType(fullName);

                if (result != null)
                {
                    yield return result;
                }
            }
        }
    }
}