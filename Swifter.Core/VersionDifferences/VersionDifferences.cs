using Swifter.RW;
using Swifter.Tools;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;

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
        /// </summary>
        public static bool UseInternalMethod = true;

        /// <summary>
        /// 获取或设置当前平台是否支持 Emit。
        /// </summary>
        public static bool IsSupportEmit
        {
            [MethodImpl(AggressiveInlining)]
            get => isSupportEmit is RWBoolean.None ? TestIsSupportEmit() : isSupportEmit is RWBoolean.Yes;
            set => isSupportEmit = value ? RWBoolean.Yes : RWBoolean.No;
        }

        private static RWBoolean isSupportEmit;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool TestIsSupportEmit()
        {
            lock (typeof(VersionDifferences))
            {
                if (isSupportEmit is RWBoolean.None)
                {
                    isSupportEmit = RWBoolean.No;

                    try
                    {
                        const int value = 1218;

                        var result = DynamicAssembly
                            .DefineType(
                                $"{nameof(TestIsSupportEmit)}_{Guid.NewGuid():N}", 
                                TypeAttributes.Public, 
                                typeBuilder => typeBuilder.DefineMethod(
                                    nameof(IsSupportEmit), 
                                    MethodAttributes.Public | MethodAttributes.Static, 
                                    typeof(int), 
                                    Type.EmptyTypes, 
                                    (mb, ilGen) => ilGen.LoadConstant(value).Return()
                                    )
                                )
                            .GetMethod(nameof(IsSupportEmit))!
                            .Invoke(null, null);

                        if (Equals(result, value))
                        {
                            isSupportEmit = RWBoolean.Yes;
                        }
                    }
                    catch
                    {
                    }
                }

                return isSupportEmit is RWBoolean.Yes;
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
                    typeof(DBNull).TypeHandle.Value == TypeHelper.GetMethodTablePointer(DBNull.Value) &&
                    typeof(string).TypeHandle.Value == TypeHelper.GetMethodTablePointer(string.Empty);
            }
            catch
            {
            }

            return false;
        }

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
        
        /// <summary>
        /// 获取当前托管线程的唯一标识符。
        /// </summary>
        /// <returns>返回一个整数</returns>
        public static int GetCurrentManagedThreadId()
        {
            return Thread.CurrentThread.ManagedThreadId;
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

        /// <summary>
        /// 获取当前托管线程的唯一标识符。
        /// </summary>
        /// <returns>返回一个整数</returns>
        [MethodImpl(AggressiveInlining)]
        public static int GetCurrentManagedThreadId()
        {
            return Environment.CurrentManagedThreadId;
        }
#endif
        /// <summary>
        /// 判断一个类型是否为仅栈值类型。
        /// </summary>
        public static bool IsByRefLike(this Type type)
        {
#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
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

        /// <summary>
        /// 判断一个类型是否为一维零下限的数组类型。
        /// </summary>
        public static bool IsSZArray(this Type type)
        {
#if NETCOREAPP || NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
            return type.IsSZArray;
#else
            return type.IsArray && type.GetArrayRank() == 1 && type.GetElementType()!.MakeArrayType() == type;
#endif
        }

        /// <summary>
        /// 检查条件；如果条件为 false，则引发 <see cref="AssertionFailedException"/>。
        /// </summary>
        /// <param name="condition">条件</param>
        public static void Assert([DoesNotReturnIf(false)]bool condition)
        {
            if (!condition)
            {
                throw new AssertionFailedException();
            }
        }

        /// <summary>
        /// 获取要序列化对象的类型。
        /// </summary>
        /// <param name="serializationInfo">序列化信息</param>
        /// <returns>返回要序列化对象的类型</returns>
        public static Type GetObjectType(this SerializationInfo serializationInfo)
        {
#if NET20 || NET35
            return Assembly.Load(serializationInfo.AssemblyName).GetType(serializationInfo.FullTypeName);
#else
            return serializationInfo.ObjectType;
#endif
        }

        /// <summary>
        /// 跳过初始化
        /// </summary>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void SkipInit<T>(out T value)
        {
            InlineIL.IL.Emit.Ret();
            throw InlineIL.IL.Unreachable();
        }

        static class IsByRefLikeAssisted<T> { }
    }
}

namespace System.Collections.Immutable
{

}