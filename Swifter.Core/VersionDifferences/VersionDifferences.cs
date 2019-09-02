using Swifter.Tools;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Swifter
{
    /// <summary>
    /// 该文档用于解决版本差异性。
    /// </summary>
    public static partial class VersionDifferences
    {
        /// <summary>
        /// 获取当前平台是否支持 Emit。
        /// </summary>
#if NETCOREAPP
        public const bool IsSupportEmit = true;
#else
        public static readonly bool IsSupportEmit = TestIsSupportEmit();

        private static bool TestIsSupportEmit()
        {
            try
            {
                DynamicAssembly.DefineType(nameof(TestIsSupportEmit), TypeAttributes.Public).CreateTypeInfo();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
#endif
        /// <summary>
        /// 获取对象的 TypeHandle 值。
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>返回一个 IntPtr 值。</returns>

#if NETCOREAPP
        [MethodImpl(AggressiveInlining)]
        public static IntPtr GetTypeHandle(object obj)
        {
            return Unsafe.GetObjectTypeHandle(obj);
        }
#else
        [MethodImpl(AggressiveInlining)]
        public static IntPtr GetTypeHandle(object obj)
        {
            if (ObjectTypeHandleEqualsTypeHandle)
            {
                // Faster
                return Unsafe.GetObjectTypeHandle(obj);
            }
            else
            {
                // Stable
                return obj.GetType().TypeHandle.Value;
            }
        }
#endif
        /// <summary>
        /// 判断 ObjectTypeHandle 和 TypeHandle 是否一致。
        /// </summary>
        public static readonly bool ObjectTypeHandleEqualsTypeHandle =
            typeof(DBNull).TypeHandle.Value == Unsafe.GetObjectTypeHandle(DBNull.Value) &&
            typeof(string).TypeHandle.Value == Unsafe.GetObjectTypeHandle(string.Empty);

#if NET20 || NET30 || NET35 || NET40
        /// <summary>
        /// 表示该方法尽量内敛。
        /// </summary>
        public const MethodImplOptions AggressiveInlining = (MethodImplOptions)256;

        /// <summary>
        /// 定义动态程序集。
        /// </summary>
        /// <param name="assName">程序集名称</param>
        /// <param name="access">程序集的可访问性</param>
        /// <returns>返回动态程序集生成器</returns>
        [MethodImpl(AggressiveInlining)]
        public static AssemblyBuilder DefineDynamicAssembly(AssemblyName assName, AssemblyBuilderAccess access)
        {
            return AppDomain.CurrentDomain.DefineDynamicAssembly(assName, access);
        }

        /// <summary>
        /// 创建运行时程序集。
        /// </summary>
        /// <param name="typeBuilder">动态程序集生成器</param>
        /// <returns>运行时程序集</returns>
        public static Type CreateTypeInfo(this TypeBuilder typeBuilder)
        {
            return typeBuilder.CreateType();
        }
#else
        /// <summary>
        /// 表示该方法尽量内敛。
        /// </summary>
        public const MethodImplOptions AggressiveInlining = MethodImplOptions.AggressiveInlining;

        /// <summary>
        /// 定义动态程序集。
        /// </summary>
        /// <param name="assName">程序集名称</param>
        /// <param name="access">程序集的可访问性</param>
        /// <returns>返回动态程序集生成器</returns>
        [MethodImpl(AggressiveInlining)]
        public static AssemblyBuilder DefineDynamicAssembly(AssemblyName assName, AssemblyBuilderAccess access)
        {
            return AssemblyBuilder.DefineDynamicAssembly(assName, access);
        }
#endif
        /// <summary>
        /// 判断是否为引用结构。
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>返回一个 bool 值</returns>
        public static bool IsByRefLike(this Type type)
        {
#if NETCOREAPP && !NETCOREAPP2_0
            return type.IsByRefLike;
#else
            if (type.IsClass || type.IsByRef || type.IsPointer || type.IsEnum || type.IsInterface || !type.IsValueType)
            {
                return false;
            }

            if (type == typeof(void))
            {
                return false;
            }

            try
            {
                typeof(Action<>).MakeGenericType(type);

                return false;
            }
            catch (Exception)
            {
                return true;
            }
#endif
        }
    }
}