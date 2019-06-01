using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Swifter.Tools
{
    /// <summary>
    /// 函数帮助工具类。
    /// </summary>
    public static unsafe class MethodHelper
    {
        private const string InvokeMethodName = nameof(Action.Invoke);
        private const string DynamicInvokeImplName = "DynamicInvokeImpl";

        private static readonly MethodInfo XConvertMethod = typeof(XConvert).GetMethod(nameof(XConvert.Convert), BindingFlags.Public | BindingFlags.Static);

        private static readonly Dictionary<MethodSign, Type> DelegateTypesCache = new Dictionary<MethodSign, Type>();
        private static readonly object DelegateTypesCacheLock = new object();


        private static readonly Type[] FuncDelegates = {
            typeof(Func<>),
            typeof(Func<,>),
            typeof(Func<,,>),
            typeof(Func<,,,>),
            typeof(Func<,,,,>),
            typeof(Func<,,,,,>),
            typeof(Func<,,,,,,>),
            typeof(Func<,,,,,,,>),
            typeof(Func<,,,,,,,,>),
            typeof(Func<,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,,,,>)
        };
        private static readonly Type[] ActionDelegates = {
            typeof(Action),
            typeof(Action<>),
            typeof(Action<,>),
            typeof(Action<,,>),
            typeof(Action<,,,>),
            typeof(Action<,,,,>),
            typeof(Action<,,,,,>),
            typeof(Action<,,,,,,>),
            typeof(Action<,,,,,,,>),
            typeof(Action<,,,,,,,,>),
            typeof(Action<,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,,,>)
        };

        /// <summary>
        /// 创建一个指定类型的委托。
        /// </summary>
        /// <typeparam name="T">委托类型</typeparam>
        /// <param name="methodInfo">需要创建委托的方法</param>
        /// <param name="throwExceptions">当参数或返回值类型不兼容时是否发生异常。</param>
        /// <returns>返回一个委托或 Null。</returns>
        public static T CreateDelegate<T>(MethodBase methodInfo, bool throwExceptions = true) where T : Delegate
        {
            return Unsafe.As<T>(InternalCreateDelegate(typeof(T), methodInfo, throwExceptions));
        }

        /// <summary>
        /// 创建一个未知类型的委托。
        /// </summary>
        /// <param name="methodInfo">需要创建委托的方法</param>
        /// <param name="throwExceptions">当参数或返回值类型不兼容时是否发生异常。</param>
        /// <returns>返回一个委托或 Null。</returns>
        public static Delegate CreateDelegate(MethodBase methodInfo, bool throwExceptions = true)
        {
            return InternalCreateDelegate(null, methodInfo, throwExceptions);
        }

        private static Delegate InternalCreateDelegate(Type delegateType, MethodBase methodInfo, bool throwExceptions)
        {
            _ = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));

            var constructor = methodInfo as ConstructorInfo;
            var method = methodInfo as MethodInfo;

            if (methodInfo.ContainsGenericParameters)
                throw new ArgumentException("Can't create a delegate for a generic method.");
            if (constructor == null && method == null)
                throw new ArgumentException("Can't create a delegate for a unknow method.");
            if (method != null && method is DynamicMethod)
                throw new ArgumentException("Can't create a delegate for a dynamic method.");

            InternalGetParametersTypes(methodInfo, out var targetParametersTypes, out var sourceReturnType);

            if (delegateType == null)
            {
                delegateType = InternalGetDelegateType(targetParametersTypes, sourceReturnType);

                if (delegateType == null)
                    goto Failed;

                goto Create;
            }

            InternalGetParametersTypes(delegateType, out var sourceParametersTypes, out var targetReturnType);

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

            if (constructor != null)
                return InternalCreateDelegateByPointer(delegateType, constructor);

            if (method.IsFinal || method.IsStatic || !(method.IsAbstract || method.DeclaringType.IsInterface))
                return InternalCreateDelegateByPointer(delegateType, method);

            var result = InternalCreateDelegateBySystem(delegateType, method);

            if (result != null)
                return result;

            if (VersionDifferences.IsSupportEmit)
                return InternalCreateDelegateByProxy(delegateType, method);

            result = InternalCreateDelegateByDefault(method);

            if (result != null)
                return result;

            Failed:

            if (throwExceptions)
                throw new NotSupportedException("Failed to create delegate.");

            return default;
        }

        private static Delegate InternalCreateDelegateBySystem(Type delegateType, MethodInfo methodInfo)
        {
            return Delegate.CreateDelegate(delegateType, methodInfo, false);
        }

        private static Delegate InternalCreateDelegateByPointer(Type delegateType, MethodBase methodInfo)
        {
            return (Delegate)Activator.CreateInstance(
                delegateType,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static,
                null,
                new object[] { null, methodInfo.MethodHandle.GetFunctionPointer() },
                null);
        }

        private static Delegate InternalCreateDelegateByProxy(Type delegateType, MethodInfo methodInfo)
        {
            InternalGetParametersTypes(delegateType, out var sourceParametersTypes, out var targetReturnType);
            InternalGetParametersTypes(methodInfo, out var targetParametersTypes, out var sourceReturnType);

            if (sourceParametersTypes.Length != targetParametersTypes.Length)
            {
                throw new ArgumentException("Parameter quantity does not match.");
            }

            for (int i = 0; i < sourceParametersTypes.Length; i++)
            {
                InternalAssertCompatibility(sourceParametersTypes[i], targetParametersTypes[i]);
            }

            InternalAssertCompatibility(sourceReturnType, targetReturnType);

            var dynamicMethod = new DynamicMethod(
                $"{nameof(MethodHelper)}_{Guid.NewGuid().ToString("N")}",
                sourceReturnType,
                sourceParametersTypes,
                methodInfo.Module,
                true);

            var ilGen = dynamicMethod.GetILGenerator();

            for (int i = 0; i < sourceParametersTypes.Length; ++i)
            {
                ilGen.LoadArgument(i);

                if (sourceParametersTypes[i] != targetParametersTypes[i])
                {
                    var xConvertMethod = XConvertMethod.MakeGenericMethod(sourceParametersTypes[i], targetParametersTypes[i]);

                    ilGen.Call(xConvertMethod);
                }
            }

            ilGen.Call(methodInfo);

            if (sourceReturnType != targetReturnType)
            {
                var xConvertMethod = XConvertMethod.MakeGenericMethod(sourceReturnType, targetReturnType);

                ilGen.Call(xConvertMethod);
            }

            ilGen.Return();

            return dynamicMethod.CreateDelegate(delegateType);
        }

        private static Delegate InternalCreateDelegateByDefault(MethodInfo methodInfo)
        {
            InternalGetParametersTypes(methodInfo, out var parametersTypes, out var returnType);

            var delegateType = InternalGetDelegateType(parametersTypes, returnType);

            if (delegateType == null)
            {
                return null;
            }

            return InternalCreateDelegateBySystem(delegateType, methodInfo);
        }

        private static void InternalAssertCompatibility(Type left, Type right)
        {
            if (left != right && !(left.CanBeGenericParameter() && right.CanBeGenericParameter()))
            {
                throw new ArgumentException("Parameters or return values are not compatible.");
            }
        }

        private static void InternalGetParametersTypes(Type delegateType, out Type[] parametersTypes, out Type returnType)
        {
            var method = delegateType.GetMethod(InvokeMethodName);

            var parameters = method.GetParameters();

            parametersTypes = new Type[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                parametersTypes[i] = parameters[i].ParameterType;
            }

            returnType = method.ReturnType;
        }

        private static void InternalGetParametersTypes(MethodBase methodInfo, out Type[] parametersTypes, out Type returnType)
        {
            var parametersList = new List<Type>();

            if (!methodInfo.IsStatic)
            {
                if (methodInfo.DeclaringType.IsByRefLike() || methodInfo.DeclaringType.IsByRef)
                {
                    parametersList.Add(methodInfo.DeclaringType);
                }
                else if (methodInfo.DeclaringType.IsValueType)
                {
                    parametersList.Add(methodInfo.DeclaringType.MakeByRefType());
                }
                else
                {
                    parametersList.Add(methodInfo.DeclaringType);
                }
            }

            var parameters = methodInfo.GetParameters();

            foreach (var item in parameters)
            {
                parametersList.Add(item.ParameterType);
            }

            returnType = (methodInfo as MethodInfo)?.ReturnType ?? typeof(void);
            parametersTypes = parametersList.ToArray();
        }

        private static Type InternalGetDelegateType(Type[] parametersTypes, Type returnType)
        {
            var methodSign = new MethodSign(nameof(DelegateTypesCache), parametersTypes, returnType);

            if (DelegateTypesCache.TryGetValue(methodSign, out var type))
            {
                return type;
            }

            lock (DelegateTypesCacheLock)
            {
                if (DelegateTypesCache.TryGetValue(methodSign, out type))
                {
                    return type;
                }

                type = Internal();

                DelegateTypesCache[methodSign] = type;

                return type;
            }

            Type Internal()
            {
                var allCanBeGenericParameter = true;

                foreach (var item in parametersTypes)
                {
                    if (!item.CanBeGenericParameter())
                    {
                        allCanBeGenericParameter = false;
                    }
                }

                if (returnType != typeof(void) && !returnType.CanBeGenericParameter())
                {
                    allCanBeGenericParameter = false;
                }

                if (allCanBeGenericParameter && parametersTypes.Length < ActionDelegates.Length)
                {
                    if (returnType == typeof(void))
                    {
                        return ActionDelegates[parametersTypes.Length].MakeGenericType(parametersTypes);
                    }
                    else
                    {
                        var list = new List<Type>(parametersTypes)
                    {
                        returnType
                    };

                        return FuncDelegates[parametersTypes.Length].MakeGenericType(list.ToArray());
                    }
                }

                if (VersionDifferences.IsSupportEmit)
                {
                    return InternalDefineDelegateType(parametersTypes, returnType);
                }

                return null;
            }
        }

        private static Type InternalDefineDelegateType(Type[] parametersTypes, Type returnType)
        {
            var delegateBuilder = DynamicAssembly.DefineType(
                $"{nameof(MethodHelper)}_{Guid.NewGuid().ToString("N")}",
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
                parametersTypes);

            invokeBuilder.SetImplementationFlags(MethodImplAttributes.Runtime);

            InternalOverrideDynamicInvokeImpl(delegateBuilder, invokeBuilder, parametersTypes, returnType);

            return delegateBuilder.CreateTypeInfo();
        }

        private unsafe static void InternalOverrideDynamicInvokeImpl(TypeBuilder delegateBuilder, MethodInfo invokeMethod, Type[] parametersTypes, Type returnType)
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
}