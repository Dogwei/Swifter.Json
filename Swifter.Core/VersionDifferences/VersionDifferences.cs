using Swifter.Tools;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Permissions;
using System.Text;

namespace Swifter
{
    /// <summary>
    /// 该文档用于解决版本差异性。
    /// </summary>
    public static partial class VersionDifferences
    {
        /// <summary>
        /// 一个 <see cref="bool"/> 值，指示是否使用 .Net 内部方法。
        /// 默认为是。
        /// 内部方法包括：
        /// <br/>1: 使用反射调用私有成员
        /// <br/>2: 试图访问 .Net Runtime 内部数据
        /// <br/>3: 试图修改类型信息
        /// </summary>
        public static bool UseInternalMethod = true;

#if NETCOREAPP
        /// <summary>
        /// .NET CORE 平台确定支持 Emit 。
        /// </summary>
        public const bool IsSupportEmit = true;
#else

        /// <summary>
        /// 获取或设置当前平台是否支持 Emit。
        /// </summary>
        public static bool IsSupportEmit
        {
            get => _IsSupportEmit ??= TestIsSupportEmit();
            set => _IsSupportEmit = value;
        }

        private static bool? _IsSupportEmit;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool TestIsSupportEmit()
        {
            try
            {
                const int value = 1218;

                var result = DynamicAssembly.DefineType($"{nameof(TestIsSupportEmit)}_{Guid.NewGuid():N}", TypeAttributes.Public, typeBuilder =>
                {
                    typeBuilder.DefineMethod(nameof(IsSupportEmit), MethodAttributes.Public | MethodAttributes.Static, typeof(int), Type.EmptyTypes, (mb, ilGen) =>
                    {
                        ilGen.LoadConstant(value);
                        ilGen.Return();

                    });

                }).GetMethod(nameof(IsSupportEmit)).Invoke(null, null);

                if (Equals(result, value))
                {
#if DEBUG
                    Console.WriteLine($"{nameof(VersionDifferences)} : {nameof(IsSupportEmit)} : {true}");
#endif

                    return true;
                }
            }
            catch
            {
            }

#if DEBUG
            Console.WriteLine($"{nameof(VersionDifferences)} : {nameof(IsSupportEmit)} : {false}");
#endif

            return false;
        }
#endif

#if NET20 || NET35 || NET40 || NET45


        /// <summary>
        /// 往 StringBuilder 后面拼接一个字符串。
        /// </summary>
        /// <param name="sb">StringBuilder</param>
        /// <param name="chars">字符串</param>
        /// <param name="count">字符串长度</param>
        /// <returns>返回当前实例</returns>
        public static unsafe StringBuilder Append(this StringBuilder sb, char* chars, int count)
        {
            for (int i = 0; i < count; i++)
            {
                sb.Append(chars[i]);
            }

            return sb;
        }

#endif

        /// <summary>
        /// 获取对象的 TypeHandle 值。
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns>返回一个 IntPtr 值。</returns>
        [MethodImpl(AggressiveInlining)]
#if NETCOREAPP
        public static IntPtr GetTypeHandle(object obj)
        {
            return Underlying.GetMethodTablePointer(obj);
        }
            
        /// <summary>
        /// 获取一个值，表示 TypeHandle 和 MethodTablePointer 是否一致。
        /// </summary>
        public const bool TypeHandleEqualMethodTablePointer = true;
#else
        public static IntPtr GetTypeHandle(object obj)
        {
            if (TypeHandleEqualMethodTablePointer)
            {
                // Faster
                return Underlying.GetMethodTablePointer(obj);
            }
            else
            {
                // Stable
                return obj.GetType().TypeHandle.Value;
            }
        }

        /// <summary>
        /// 获取一个值，表示 TypeHandle 和 MethodTablePointer 是否一致。
        /// </summary>
        public static readonly bool TypeHandleEqualMethodTablePointer = GetTypeHandleEqualMethodTablePointer();

        [MethodImpl(MethodImplOptions.NoInlining)]
        static bool GetTypeHandleEqualMethodTablePointer()
        {
            try
            {
                return
                    typeof(DBNull).TypeHandle.Value == Underlying.GetMethodTablePointer(DBNull.Value) &&
                    typeof(string).TypeHandle.Value == Underlying.GetMethodTablePointer(string.Empty);
            }
            catch (Exception)
            {
            }

            return false;
        }
#endif

#if NET20 || NET35 || NET40
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
            if (type.IsValueType  // 仅栈类型是值类型
                && !type.IsPrimitive  // 仅栈类型不是基元类型
                && !type.IsPointer // 指针不是仅栈类型
                && !type.IsByRef // 引用不是仅栈类型
                && !type.IsEnum // 枚举不是仅栈类型
                && type != typeof(void) // void 不是仅栈类型
                && type.GetInterfaces().Length == 0 // 仅栈类型不允许实现接口
                )
            {
                try
                {
                    // 仅栈类型不能当作泛型。
                    typeof(IsByRefLikeAssisted<>).MakeGenericType(type);
                }
                catch
                {
                    return true;
                }
            }

            return false;
#endif
        }

        static class IsByRefLikeAssisted<T> { }
    }
}