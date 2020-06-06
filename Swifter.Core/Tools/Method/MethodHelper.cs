using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;



namespace Swifter.Tools
{
    static class DelegateHelper
    {
        public static readonly int _methodPtrAux =
            typeof(Delegate).GetField(nameof(_methodPtrAux), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) is FieldInfo fieldInfo ? TypeHelper.OffsetOf(fieldInfo) : -1;

        public static readonly int _methodPtr =
            typeof(Delegate).GetField(nameof(_methodPtr), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) is FieldInfo fieldInfo ? TypeHelper.OffsetOf(fieldInfo) : -1;
    }

    /// <summary>
    /// 函数帮助工具类。
    /// </summary>
    public static unsafe class MethodHelper
    {
        private const string InvokeMethodName = nameof(Action.Invoke);
        private const string DynamicInvokeImplName = "DynamicInvokeImpl";


        /// <summary>
        /// 创建一个指定类型的委托。
        /// </summary>
        /// <typeparam name="T">委托类型</typeparam>
        /// <param name="methodInfo">需要创建委托的方法</param>
        /// <param name="throwExceptions">当参数或返回值类型不兼容时是否发生异常。</param>
        /// <returns>返回一个委托或 Null。</returns>
        public static T CreateDelegate<T>(MethodBase methodInfo, bool throwExceptions = true) where T : Delegate
        {
            return Underlying.As<T>(CreateDelegate(typeof(T), methodInfo, throwExceptions));
        }

        /// <summary>
        /// 创建一个未知类型的委托。
        /// </summary>
        /// <param name="methodInfo">需要创建委托的方法</param>
        /// <param name="throwExceptions">当参数或返回值类型不兼容时是否发生异常。</param>
        /// <returns>返回一个委托或 Null。</returns>
        public static Delegate CreateDelegate(MethodBase methodInfo, bool throwExceptions = true)
        {
            return CreateDelegate(null, methodInfo, throwExceptions);
        }

        /// <summary>
        /// 创建一个指定类型的委托。
        /// </summary>
        /// <param name="delegateType">委托类型</param>
        /// <param name="methodBase">需要创建委托的方法</param>
        /// <param name="throwExceptions">当参数或返回值类型不兼容时是否发生异常。</param>
        /// <returns>返回一个委托或 Null。</returns>
        public static Delegate CreateDelegate(Type delegateType, MethodBase methodBase, bool throwExceptions)
        {
            if (methodBase is null) 
                throw new ArgumentNullException(nameof(methodBase));

            if (methodBase.ContainsGenericParameters)
                throw new ArgumentException("Can't create a delegate for a generic method.");

            var constructorInfo = methodBase as ConstructorInfo;
            var methodInfo = methodBase as MethodInfo;

            if (constructorInfo is null && methodInfo is null)
                throw new ArgumentException("Can't create a delegate for a unknow method.");

            GetParametersTypes(methodBase, out var targetParametersTypes, out var sourceReturnType);

            if (delegateType is null)
            {
                delegateType = InternalGetDelegateType(targetParametersTypes, sourceReturnType);

                if (delegateType is null)
                    goto Failed;

                goto Create;
            }

            GetParametersTypes(delegateType, out var sourceParametersTypes, out var targetReturnType);

            if (sourceParametersTypes.Length != targetParametersTypes.Length)
                throw new ArgumentException("Parameter quantity does not match.");

            if (sourceReturnType != targetReturnType && (sourceReturnType == typeof(void) || targetReturnType == typeof(void)))
                throw new ArgumentException("Return type does not match.");

            if (throwExceptions)
            {
                for (int i = 0; i < sourceParametersTypes.Length; i++)
                    if (sourceParametersTypes[i] != targetParametersTypes[i])
                        throw new ArgumentException("Parameter type does not match.");

                if (sourceReturnType != targetReturnType)
                    throw new ArgumentException("Return type does not match.");
            }

            Create:

            if (methodInfo != null && methodInfo is DynamicMethod dynamicMethod)
                return dynamicMethod.CreateDelegate(delegateType);

            if (constructorInfo != null)
                return InternalCreateDelegateByPointer(delegateType, constructorInfo);

            if (methodInfo.IsStatic)
                return InternalCreateDelegateByPointer(delegateType, methodInfo);

            if (InternalCreateDelegateBySystem(delegateType, methodInfo) is Delegate result)
                return result;

            if (VersionDifferences.IsSupportEmit)
                return InternalCreateDelegateByProxy(delegateType, methodInfo);

            if (InternalCreateDelegateByDefault(methodInfo) is Delegate @delegate)
                return @delegate;

            Failed:

            if (throwExceptions)
                throw new NotSupportedException("Failed to create delegate.");

            return default;
        }

        /// <summary>
        /// 使用系统绑定方式创建委托。
        /// </summary>
        private static Delegate InternalCreateDelegateBySystem(Type delegateType, MethodInfo methodInfo)
        {
            return Delegate.CreateDelegate(delegateType, methodInfo, false);
        }

        /// <summary>
        /// 使用函数指针方式创建委托。
        /// </summary>
        private static Delegate InternalCreateDelegateByPointer(Type delegateType, MethodBase methodInfo)
        {
            return (Delegate)Activator.CreateInstance(
                delegateType,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static,
                null,
                new object[] { null, methodInfo.MethodHandle.GetFunctionPointer() },
                null);
        }

        /// <summary>
        /// 使用动态方法代理模式创建委托。
        /// </summary>
        private static Delegate InternalCreateDelegateByProxy(Type delegateType, MethodInfo methodInfo)
        {
            GetParametersTypes(delegateType, out var parameterTypes, out var returnType);

            var dynamicMethod = new DynamicMethod(
                $"{nameof(MethodHelper)}_{Guid.NewGuid():N}",
                returnType,
                parameterTypes,
                methodInfo.Module,
                true);

            var ilGen = dynamicMethod.GetILGenerator();

            for (int i = 0; i < parameterTypes.Length; ++i)
            {
                ilGen.LoadArgument(i);
            }

            ilGen.Call(methodInfo);

            ilGen.Return();

            return dynamicMethod.CreateDelegate(delegateType);
        }

        /// <summary>
        /// 使用默认方式创建委托。
        /// </summary>
        private static Delegate InternalCreateDelegateByDefault(MethodInfo methodInfo)
        {
            GetParametersTypes(methodInfo, out var parametersTypes, out var returnType);

            var delegateType = InternalGetDelegateType(parametersTypes, returnType);

            if (delegateType is null)
            {
                return null;
            }

            return InternalCreateDelegateBySystem(delegateType, methodInfo);
        }

        /// <summary>
        /// 获取委托类型的参数类型集合和返回值类型。
        /// </summary>
        /// <param name="delegateType">委托类型</param>
        /// <param name="parameterTypes">返回参数类型集合</param>
        /// <param name="returnType">返回返回值类型</param>
        public static void GetParametersTypes(Type delegateType, out Type[] parameterTypes, out Type returnType)
        {
            var method = delegateType.GetMethod(InvokeMethodName);

            parameterTypes = method.GetParameters().Map(item => item.ParameterType);

            returnType = method.ReturnType;
        }

        /// <summary>
        /// 获取方法的参数类型集合和返回值类型。
        /// </summary>
        /// <param name="methodInfo">方法</param>
        /// <param name="parameterTypes">返回参数类型集合</param>
        /// <param name="returnType">返回返回值类型</param>
        public static void GetParametersTypes(MethodBase methodInfo, out Type[] parameterTypes, out Type returnType)
        {
            returnType = (methodInfo as MethodInfo)?.ReturnType ?? typeof(void);

            if (methodInfo.IsStatic)
            {
                parameterTypes = methodInfo.GetParameters().Map(element => element.ParameterType);
            }
            else
            {
                var parameters = methodInfo.GetParameters();

                parameterTypes = new Type[parameters.Length + 1];

                parameterTypes[0] = methodInfo.DeclaringType switch
                {
                    var declaringType when declaringType.IsByRefLike() => MakeByRefType(declaringType),
                    var declaringType when declaringType.IsValueType => declaringType.MakeByRefType(),
                    var declaringType => declaringType
                };

                for (int i = 0; i < parameters.Length; i++)
                {
                    parameterTypes[i + 1] = parameters[i].ParameterType;
                }
            }

            static Type MakeByRefType(Type declaringType)
            {
                try
                {
                    return declaringType.MakeByRefType();
                }
                catch
                {
                }

                try
                {
                    return declaringType.MakePointerType();
                }
                catch
                {
                }

                return typeof(IntPtr);
            }
        }

        /// <summary>
        /// 创建一个泛型委托类型。泛型定义类型是 System.Func 或 System.Action。
        /// 如果创建失败（仅当参数类型或返回值类型不能作为泛型时）则返回 Null。
        /// </summary>
        /// <param name="parameterTypes">参数类型集合</param>
        /// <param name="returnType">返回值类型</param>
        /// <returns>返回一个委托类型</returns>
        public static Type MakeDelegateType(Type[] parameterTypes, Type returnType)
        {
            foreach (var item in parameterTypes)
            {
                if (!item.CanBeGenericParameter())
                {
                    return null;
                }
            }

            if (returnType == typeof(void))
            {
                return parameterTypes.Length switch
                {
                    00 => typeof(Action),
                    01 => typeof(Action<>).MakeGenericType(parameterTypes),
                    02 => typeof(Action<,>).MakeGenericType(parameterTypes),
                    03 => typeof(Action<,,>).MakeGenericType(parameterTypes),
                    04 => typeof(Action<,,,>).MakeGenericType(parameterTypes),
                    05 => typeof(Action<,,,,>).MakeGenericType(parameterTypes),
                    06 => typeof(Action<,,,,,>).MakeGenericType(parameterTypes),
                    07 => typeof(Action<,,,,,,>).MakeGenericType(parameterTypes),
                    08 => typeof(Action<,,,,,,,>).MakeGenericType(parameterTypes),
                    09 => typeof(Action<,,,,,,,,>).MakeGenericType(parameterTypes),
                    10 => typeof(Action<,,,,,,,,,>).MakeGenericType(parameterTypes),
                    11 => typeof(Action<,,,,,,,,,,>).MakeGenericType(parameterTypes),
                    12 => typeof(Action<,,,,,,,,,,,>).MakeGenericType(parameterTypes),
                    13 => typeof(Action<,,,,,,,,,,,,>).MakeGenericType(parameterTypes),
                    14 => typeof(Action<,,,,,,,,,,,,,>).MakeGenericType(parameterTypes),
                    15 => typeof(Action<,,,,,,,,,,,,,,>).MakeGenericType(parameterTypes),
                    16 => typeof(Action<,,,,,,,,,,,,,,,>).MakeGenericType(parameterTypes),
                    _ => null
                };
            }
            else
            {
                if (!returnType.CanBeGenericParameter())
                {
                    return null;
                }

                var tParameterTypes = ArrayHelper.Merge(parameterTypes, returnType);

                return parameterTypes.Length switch
                {
                    00 => typeof(Func<>).MakeGenericType(tParameterTypes),
                    01 => typeof(Func<,>).MakeGenericType(tParameterTypes),
                    02 => typeof(Func<,,>).MakeGenericType(tParameterTypes),
                    03 => typeof(Func<,,,>).MakeGenericType(tParameterTypes),
                    04 => typeof(Func<,,,,>).MakeGenericType(tParameterTypes),
                    05 => typeof(Func<,,,,,>).MakeGenericType(tParameterTypes),
                    06 => typeof(Func<,,,,,,>).MakeGenericType(tParameterTypes),
                    07 => typeof(Func<,,,,,,,>).MakeGenericType(tParameterTypes),
                    08 => typeof(Func<,,,,,,,,>).MakeGenericType(tParameterTypes),
                    09 => typeof(Func<,,,,,,,,,>).MakeGenericType(tParameterTypes),
                    10 => typeof(Func<,,,,,,,,,,>).MakeGenericType(tParameterTypes),
                    11 => typeof(Func<,,,,,,,,,,,>).MakeGenericType(tParameterTypes),
                    12 => typeof(Func<,,,,,,,,,,,,>).MakeGenericType(tParameterTypes),
                    13 => typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(tParameterTypes),
                    14 => typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(tParameterTypes),
                    15 => typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(tParameterTypes),
                    16 => typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(tParameterTypes),
                    _ => null
                };
            }
        }

        /// <summary>
        /// 获取定义委托类型的名称。此名称具有唯一性，当参数类型集合相同且返回值类型相同，那么获取的名称也相同。
        /// </summary>
        /// <param name="parameterTypes">参数类型集合</param>
        /// <param name="returnType">返回值类型</param>
        /// <returns>返回一个字符串</returns>
        private static string GetDefineDelegateName(Type[] parameterTypes, Type returnType)
        {
            const string Prefix = "Swifter.Dynamic.";
            var delegateName = returnType == typeof(void) ? "Action" : "Func";

            var text = StringHelper.MakeString((parameterTypes.Length + 1) * NumberHelper.GuidStringLength);

            fixed(char* ptr = text)
            {
                int offset = 0;

                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    offset += NumberHelper.ToString(parameterTypes[i].GUID, ptr + offset, false);
                }

                offset += NumberHelper.ToString(returnType.GUID, ptr + offset, false);

                if (offset != text.Length)
                {
                    throw new NotSupportedException();
                }
            }

            var sign = text.ComputeHash<MD5>();

            return $"{Prefix}{delegateName}_{sign}";
        }

        /// <summary>
        /// 在动态程序集中定义一个委托类型。如果动态程序集中已有相同的委托类型，则直接返回该委托类型。
        /// 如果创建失败（仅当平台不支持 Emit 时）则返回 Null。
        /// </summary>
        /// <param name="parameterTypes">参数类型集合</param>
        /// <param name="returnType">返回值类型</param>
        /// <returns>返回一个委托类型</returns>
        public static Type DefineDelegateType(Type[] parameterTypes, Type returnType)
        {
            if (!VersionDifferences.IsSupportEmit)
            {
                return null;
            }

            var delegateName = GetDefineDelegateName(parameterTypes, returnType);

            if (DynamicAssembly.GetType(delegateName) is Type definedType)
            {
                return definedType;
            }

            var delegateBuilder = DynamicAssembly.DefineType(
                delegateName,
                TypeAttributes.AutoLayout | TypeAttributes.AnsiClass | TypeAttributes.Sealed | TypeAttributes.Public,
                typeof(MulticastDelegate));

            var ctorBuilder = delegateBuilder.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName,
                CallingConventions.Standard,
                new[] { typeof(object), typeof(IntPtr) });

            ctorBuilder.SetImplementationFlags(MethodImplAttributes.Runtime);

            var invokeBuilder = delegateBuilder.DefineMethod(
                InvokeMethodName,
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual,
                CallingConventions.Standard,
                returnType,
                parameterTypes);

            invokeBuilder.SetImplementationFlags(MethodImplAttributes.Runtime);

            InternalOverrideDynamicInvokeImpl(delegateBuilder, invokeBuilder, parameterTypes, returnType);

            return delegateBuilder.CreateTypeInfo();

            static void InternalOverrideDynamicInvokeImpl(TypeBuilder delegateBuilder, MethodInfo invokeMethod, Type[] parametersTypes, Type returnType)
            {
                if (returnType.IsByRef || returnType.IsByRefLike())
                {
                    return;
                }

                foreach (var item in parametersTypes)
                {
                    if (item.IsByRefLike())
                    {
                        return;
                    }
                }

                var dynamicInvokeMethodBuilder = delegateBuilder.DefineMethod(
                    DynamicInvokeImplName,
                    MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                    CallingConventions.Standard,
                    typeof(object),
                    new Type[] { typeof(object[]) });

                var ilGen = dynamicInvokeMethodBuilder.GetILGenerator();

                ilGen.LoadArgument(0);

                for (int i = 0; i < parametersTypes.Length; i++)
                {
                    var item = parametersTypes[i];

                    ilGen.LoadArgument(1);
                    ilGen.LoadConstant(i);

                    if (item.IsByRef)
                    {
                        item = item.GetElementType();

                        ilGen.LoadReferenceElement();
                        ilGen.CastClass(item);

                        if (item.IsValueType)
                        {
                            ilGen.Unbox(item);
                        }
                        else
                        {
                            ilGen.Pop();
                            ilGen.LoadArgument(1);
                            ilGen.LoadConstant(i);
                            ilGen.LoadElementAddress(typeof(object));
                        }

                        continue;
                    }
                    else
                    {
                        ilGen.LoadReferenceElement();
                        ilGen.CastClass(item);

                        if (item.IsValueType)
                        {
                            ilGen.UnboxAny(item);
                        }
                    }
                }

                ilGen.Call(invokeMethod);

                if (returnType == typeof(void))
                {
                    ilGen.LoadNull();
                }
                else if (returnType.IsPointer)
                {
                    ilGen.Box(typeof(IntPtr));
                }
                else if (returnType.IsValueType)
                {
                    ilGen.Box(returnType);
                }

                ilGen.Return();
            }
        }

        private static Type InternalGetDelegateType(Type[] parameterTypes, Type returnType)
        {
            if (DefineDelegateType(parameterTypes, returnType) is Type dynamicDelegateType)
            {
                return dynamicDelegateType;
            }

            if (MakeDelegateType(parameterTypes, returnType) is Type genericDelegateType)
            {
                return genericDelegateType;
            }

            return null;
        }

        /// <summary>
        /// 使指定原函数的调用跳转到指定目标函数。
        /// </summary>
        /// <param name="sourceMethod">原方法</param>
        /// <param name="targetMethod">目标方法</param>
        /// <returns>返回是否成功</returns>
        public static unsafe bool Override(MethodBase sourceMethod, MethodBase targetMethod)
        {
            return Override(sourceMethod, targetMethod.GetFunctionPointer());
        }

        /// <summary>
        /// 使指定原函数的调用跳转到指定目标函数。
        /// </summary>
        /// <param name="sourceMethod">原方法</param>
        /// <param name="targetMethodPointer">目标方法地址</param>
        /// <returns>返回是否成功</returns>
        public static unsafe bool Override(MethodBase sourceMethod, IntPtr targetMethodPointer)
        {
            if (override_is_available == false)
            {
                return false;
            }

            var source = (byte*)sourceMethod.GetFunctionPointer();
            var target = (byte*)targetMethodPointer;

            var offset = target - source - sizeof(Jmp);

            if ((int)offset != offset)
            {
                return false;
            }

            var jmp = new Jmp((int)offset);

            Underlying.CopyBlockUnaligned(source, &jmp, (uint)sizeof(Jmp));

            return true;
        }

        static bool? override_is_available = null;

        /// <summary>
        /// 获取 Override 方法是否可用。
        /// </summary>
        public static bool OverrideIsAvailable
        {
            get
            {
                if (override_is_available is null)
                {
                    try
                    {
                        Override(
                            typeof(MethodHelper).GetMethod(nameof(TestSource), BindingFlags.NonPublic | BindingFlags.Static),
                            typeof(MethodHelper).GetMethod(nameof(TestTarget), BindingFlags.NonPublic | BindingFlags.Static)
                            );

                        override_is_available = TestSource(12, 18) == TestTarget(12, 18);
                    }
                    catch (Exception)
                    {
                        override_is_available = false;
                    }

                }

                return override_is_available.Value;
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        static int TestSource(int x, int y) => x + y;

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        static int TestTarget(int x, int y) => x * y;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static RuntimeMethodHandle GetMethodDescriptor(DynamicMethod dynamicMethod)
        {
            MethodInfo method = null;

            try
            {
                method = typeof(DynamicMethod).GetMethod(nameof(GetMethodDescriptor), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (method != null)
                {
                    if (method.GetParameters().Length != 0 || method.ReturnType != typeof(RuntimeMethodHandle))
                    {
                        method = null;
                    }
                }

                return (RuntimeMethodHandle)method.Invoke(dynamicMethod, null);
            }
            finally
            {
                if (method != null && OverrideIsAvailable)
                {
                    Override(MethodOf<DynamicMethod, RuntimeMethodHandle>(GetMethodDescriptor), method);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IntPtr GetNativeFunctionPointer(Delegate @delegate)
        {
            MethodInfo method = null;

            try
            {
                method = typeof(Delegate).GetMethod(nameof(GetNativeFunctionPointer), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                if (method != null)
                {
                    if (method.GetParameters().Length != 0 || method.ReturnType != typeof(IntPtr))
                    {
                        method = null;
                    }
                }

                return (IntPtr)method.Invoke(@delegate, null);
            }
            finally
            {
                if (method != null && OverrideIsAvailable)
                {
                    Override(MethodOf<Delegate, IntPtr>(GetNativeFunctionPointer), method);
                }
            }
        }

        /// <summary>
        /// 获取委托的函数指针。
        /// </summary>
        /// <param name="delegate">委托</param>
        /// <returns>返回一个指针</returns>
        public static IntPtr GetFunctionPointer(this Delegate @delegate)
        {
            if (@delegate.GetInvocationList().Length != 1)
            {
                throw new NotSupportedException("MulticastDelegate are not supported.");
            }

            if (@delegate.Method is MethodInfo methodInfo)
            {
                try
                {
                    return methodInfo.GetFunctionPointer();
                }
                catch
                {
                }
            }

            if (!(@delegate.Target is null) && DelegateHelper._methodPtr >= 0)
            {
                try
                {
                    return Underlying.AddByteOffset(ref TypeHelper.Unbox<IntPtr>(@delegate), DelegateHelper._methodPtr);
                }
                catch
                {
                }
            }

            if (@delegate.Target is null && DelegateHelper._methodPtrAux >= 0)
            {
                try
                {
                    return Underlying.AddByteOffset(ref TypeHelper.Unbox<IntPtr>(@delegate), DelegateHelper._methodPtrAux);
                }
                catch
                {
                }
            }

            /* try MONO */
            try
            {
                return GetNativeFunctionPointer(@delegate);
            }
            catch
            {
            }

            return Marshal.GetFunctionPointerForDelegate(@delegate);
        }

        /// <summary>
        /// 获取方法的函数指针。
        /// </summary>
        /// <param name="methodBase">方法</param>
        /// <returns>返回一个指针</returns>
        public static IntPtr GetFunctionPointer(this MethodBase methodBase)
        {
            if (methodBase is DynamicMethod dynamicMethod)
            {
                try
                {
                    return GetMethodDescriptor(dynamicMethod).GetFunctionPointer();
                }
                catch
                {
                }

                try
                {
                    if (CreateDelegate(dynamicMethod) is Delegate @delegate)
                    {
                        return @delegate.GetFunctionPointer();
                    }
                }
                catch
                {
                }
            }

            return methodBase.MethodHandle.GetFunctionPointer();
        }

#pragma warning disable CS1591

        public static MethodInfo MethodOf(Action action) => action.Method;

        public static MethodInfo MethodOf<TIn>(Action<TIn> action) => action.Method;

        public static MethodInfo MethodOf<TIn1, TIn2>(Action<TIn1, TIn2> action) => action.Method;

        public static MethodInfo MethodOf<TIn1, TIn2, TIn3>(Action<TIn1, TIn2, TIn3> action) => action.Method;

        public static MethodInfo MethodOf<TOut>(Func<TOut> func) => func.Method;

        public static MethodInfo MethodOf<TIn, TOut>(Func<TIn, TOut> func) => func.Method;

        public static MethodInfo MethodOf<TIn1, TIn2, TOut>(Func<TIn1, TIn2, TOut> func) => func.Method;

#pragma warning restore CS1591
    }
}