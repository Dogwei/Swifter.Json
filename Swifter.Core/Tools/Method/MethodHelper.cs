using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 提供创建委托的方法。
    /// 
    /// 提高委托的创建成功率和委托的动态执行效率。
    /// </summary>
    public static class MethodHelper
    {
        /// <summary>
        /// 委托的 Invoke 方法名称。
        /// </summary>
        public const string InvokeMethodName = nameof(Action.Invoke);

        private const string DelegatePointerFieldName = "_methodPtrAux";
        
        private static readonly Type[] FuncInterfaces;
        private static readonly Type[] ActionInterfaces;

        private static readonly Dictionary<MethodSign, Type> DelegateTypesCache;
        private static readonly object DelegateTypesCacheLock;

        private static IntPtr DelegatePointerOffset;

        static MethodHelper()
        {
            DelegateTypesCache = new Dictionary<MethodSign, Type>();
            DelegateTypesCacheLock = new object();

            FuncInterfaces = new Type[] {
                typeof(IFunc<>),
                typeof(IFunc<,>),
                typeof(IFunc<,,>),
                typeof(IFunc<,,,>),
                typeof(IFunc<,,,,>),
                typeof(IFunc<,,,,,>),
                typeof(IFunc<,,,,,,>),
                typeof(IFunc<,,,,,,,>),
                typeof(IFunc<,,,,,,,,>),
                typeof(IFunc<,,,,,,,,,>),
                typeof(IFunc<,,,,,,,,,,>),
                typeof(IFunc<,,,,,,,,,,,>),
                typeof(IFunc<,,,,,,,,,,,,>),
                typeof(IFunc<,,,,,,,,,,,,,>),
                typeof(IFunc<,,,,,,,,,,,,,,>),
                typeof(IFunc<,,,,,,,,,,,,,,,>),
                typeof(IFunc<,,,,,,,,,,,,,,,,>)
            };

            ActionInterfaces = new Type[] {
                typeof(IAction),
                typeof(IAction<>),
                typeof(IAction<,>),
                typeof(IAction<,,>),
                typeof(IAction<,,,>),
                typeof(IAction<,,,,>),
                typeof(IAction<,,,,,>),
                typeof(IAction<,,,,,,>),
                typeof(IAction<,,,,,,,>),
                typeof(IAction<,,,,,,,,>),
                typeof(IAction<,,,,,,,,,>),
                typeof(IAction<,,,,,,,,,,>),
                typeof(IAction<,,,,,,,,,,,>),
                typeof(IAction<,,,,,,,,,,,,>),
                typeof(IAction<,,,,,,,,,,,,,>),
                typeof(IAction<,,,,,,,,,,,,,,>)
            };

            DelegatePointerOffset = IntPtr.Zero;
        }

        /// <summary>
        /// 为方法创建一个指定类型的委托。
        /// </summary>
        /// <param name="delegateType">指定委托类型</param>
        /// <param name="methodInfo">方法</param>
        /// <param name="signatureLevel">签名等级</param>
        /// <returns>返回指定类型的委托</returns>
        public static Delegate CreateDelegate(Type delegateType, MethodBase methodInfo, SignatureLevels signatureLevel = SignatureLevels.Consistent)
        {
            return InternalCreateDelegate(delegateType, methodInfo, signatureLevel);
        }

        /// <summary>
        /// 为方法创建一个指定类型的委托。
        /// </summary>
        /// <typeparam name="T">指定委托类型</typeparam>
        /// <param name="methodInfo">方法</param>
        /// <param name="signatureLevel">签名等级</param>
        /// <returns>返回指定类型的委托</returns>
        public static T CreateDelegate<T>(MethodBase methodInfo, SignatureLevels signatureLevel = SignatureLevels.Consistent) where T : Delegate
        {
            var delegateType = typeof(T);

            var @delegate = InternalCreateDelegate(delegateType, methodInfo, signatureLevel);

            return (T)@delegate;
        }

        /// <summary>
        /// 为方法创建一个动态类型的委托。
        /// </summary>
        /// <param name="methodInfo">方法</param>
        /// <returns>返回动态类型的委托</returns>
        public static Delegate CreateDelegate(MethodBase methodInfo)
        {
            GetParametersTypes(methodInfo, out var parameterTypes, out var resultType);

            var delegateType = GetOrDefineDelegateType(parameterTypes, resultType, true);

            return InternalCreateDelegate(delegateType, methodInfo, SignatureLevels.None);
        }

        private static Delegate InternalCreateDelegate(Type delegateType, MethodBase methodInfo, SignatureLevels signatureLevel)
        {
            if (delegateType == null)
            {
                throw new ArgumentNullException(nameof(delegateType));
            }

            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            if (methodInfo.ContainsGenericParameters)
            {
                throw new ArgumentException("Cannot create a delegate for a generic method.");
            }

            GetParametersTypes(delegateType, out var ps1, out var rt1);
            GetParametersTypes(methodInfo, out var ps2, out var rt2);

            if (ps1.Length != ps2.Length)
            {
                throw new ArgumentException("Number of parameters do not match.");
            }

            if (signatureLevel == SignatureLevels.Cast)
            {
                if (!CastEquals(rt1, rt2))
                {
                    throw new ArgumentException("Return value signature does not match.");
                }

                for (int i = 0; i < ps1.Length; i++)
                {
                    if (!CastEquals(ps1[i], ps2[i]))
                    {
                        throw new ArgumentException("Parameter signatures do not match.");
                    }
                }
            }
            else if (signatureLevel == SignatureLevels.Consistent)
            {
                if (rt1 != rt2)
                {
                    throw new ArgumentException("Return value signature does not match.");
                }

                for (int i = 0; i < ps1.Length; i++)
                {
                    if (ps1[i] != ps2[i])
                    {
                        throw new ArgumentException("Parameter signatures do not match.");
                    }
                }
            }

            var createWay = ChooseCreateWays(methodInfo);

            if ((createWay & CreateWays.Pointer) != 0)
            {
                try
                {
                    return CreateDelegateByPointer(delegateType, methodInfo);
                }
                catch (Exception)
                {
                }
            }

            if ((createWay & CreateWays.Default) != 0)
            {
                try
                {
                    return Delegate.CreateDelegate(delegateType, (MethodInfo)methodInfo);
                }
                catch (Exception)
                {
                }
            }

            if ((createWay & CreateWays.Proxy) != 0)
            {
                try
                {
                    return CreateDelegateByProxy(delegateType, methodInfo);
                }
                catch (Exception)
                {
                }
            }

            try
            {
                return CreateDelegateByPointer(delegateType, methodInfo);
            }
            catch (Exception)
            {
            }

            throw new NotSupportedException("Failed to create delegate.");
        }

        private static bool CastEquals(Type type1, Type type2)
        {
            if (type1 == type2)
            {
                return true;
            }

            if (type1.IsByRef || type1.IsPointer || type1 == typeof(IntPtr) || type1 == typeof(UIntPtr))
            {
                return type2.IsByRef || type2.IsPointer || type2.IsClass || type2.IsInterface || type2 == typeof(IntPtr) || type2 == typeof(UIntPtr);
            }

            if (type1.IsClass || type1.IsInterface)
            {
                return ((type2.IsClass || type2.IsInterface) && (type1.IsAssignableFrom(type2) || type2.IsAssignableFrom(type1))) || type2.IsByRef || type2.IsPointer || type2 == typeof(IntPtr) || type2 == typeof(UIntPtr);
            }

            return false;
        }

        private static Delegate CreateDelegateByPointer(Type delegateType, MethodBase methodInfo)
        {
            return (Delegate)Activator.CreateInstance(
                delegateType,
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static,
                null,
                new object[] { null, methodInfo.MethodHandle.GetFunctionPointer() },
                null);
        }

        private static CreateWays ChooseCreateWays(MethodBase methodInfo)
        {
            if (methodInfo.IsConstructor)
            {
                return CreateWays.Pointer;
            }

            if (methodInfo is DynamicMethod)
            {
                return CreateWays.Default;
            }

            if (methodInfo.DeclaringType.IsInterface)
            {
                return CreateWays.Pointer | CreateWays.Default | CreateWays.Proxy;
            }

            if (methodInfo.IsVirtual)
            {
                return CreateWays.Default | CreateWays.Proxy;
            }

            if (methodInfo.IsStatic && methodInfo.IsSpecialName)
            {
                return CreateWays.Default | CreateWays.Proxy;
            }

            return CreateWays.Pointer | CreateWays.Default | CreateWays.Proxy;
        }

        private static Delegate CreateDelegateByProxy(Type delegateType, MethodBase methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            Type[] parameterTypes;
            Type resultType;

            if (delegateType == null)
            {
                GetParametersTypes(methodInfo, out parameterTypes, out resultType);

                delegateType = GetOrDefineDelegateType(parameterTypes, resultType, true);
            }
            else
            {
                GetParametersTypes(delegateType, out parameterTypes, out resultType);
            }

            var dynamicMethod = new DynamicMethod(
                $"{nameof(MethodHelper)}_{nameof(Delegate)}_{Guid.NewGuid().ToString("N")}",
                resultType,
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

        private static Delegate CreateDelegateByDefault(Type delegateType, MethodBase methodInfo)
        {
            try
            {
                return Delegate.CreateDelegate(delegateType, (MethodInfo)methodInfo);
            }
            catch (Exception)
            {
                return (Delegate)Activator.CreateInstance(
                    delegateType,
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static,
                    null,
                    new object[] { null, GetFunctionPointer(methodInfo) },
                    null);
            }
        }

        private static IntPtr GetFunctionPointer(MethodBase methodInfo)
        {
            var createWay = ChooseCreateWays(methodInfo);

            if ((createWay & CreateWays.Pointer) != 0)
            {
                return methodInfo.MethodHandle.GetFunctionPointer();
            }

            if ((createWay & CreateWays.Default) != 0)
            {
                GetParametersTypes(methodInfo, out var parameterTypes, out var resultType);

                var delegateType = GetOrDefineDelegateType(parameterTypes, resultType, false);

                var @delegate = Delegate.CreateDelegate(delegateType, (MethodInfo)methodInfo);

                return GetDelegatePointer(@delegate);
            }

            if ((createWay & CreateWays.Default) != 0)
            {
                var @delegate = CreateDelegateByProxy(null, methodInfo);

                return GetDelegatePointer(@delegate);
            }

            throw new NotSupportedException("Failed to get function pointer.");
        }

        private static IntPtr GetDelegatePointer(Delegate @delegate)
        {
            if (DelegatePointerOffset == IntPtr.Zero)
            {
                DelegatePointerOffset = (IntPtr)(TypeHelper.OffsetOf(typeof(Delegate).GetField(DelegatePointerFieldName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic)) + IntPtr.Size);
            }

            return Unsafe.AddByteOffset(ref Unsafe.AsRef<IntPtr>(@delegate), DelegatePointerOffset);
        }

        private static void GetParametersTypes(MethodBase methodInfo, out Type[] parameterTypes, out Type resultType, bool? isStatic = null)
        {
            var methodParameters = methodInfo.GetParameters();

            int offset = 0;

            if (isStatic == null)
            {
                isStatic = methodInfo.IsStatic;
            }

            if (isStatic.Value)
            {
                parameterTypes = new Type[methodParameters.Length];
            }
            else
            {
                parameterTypes = new Type[methodParameters.Length + 1];

                if (methodInfo.DeclaringType.IsValueType)
                {
                    parameterTypes[0] = methodInfo.DeclaringType.MakeByRefType();
                }
                else
                {
                    parameterTypes[0] = methodInfo.DeclaringType;
                }

                offset = 1;
            }

            foreach (var Item in methodParameters)
            {
                parameterTypes[offset] = Item.ParameterType;

                ++offset;
            }

            if (methodInfo is MethodInfo)
            {
                resultType = ((MethodInfo)methodInfo).ReturnType;
            }
            else
            {
                resultType = typeof(void);
            }
        }

        private static void GetParametersTypes(Type delegateType, out Type[] parameterTypes, out Type resultType)
        {
            var invokeMethod = delegateType.GetMethod(InvokeMethodName);

            var parameters = invokeMethod.GetParameters();

            parameterTypes = new Type[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                parameterTypes[i] = parameters[i].ParameterType;
            }

            resultType = invokeMethod.ReturnType;
        }

        private unsafe static Type DefineDelegateType(Type[] parameterTypes, Type resultType, bool implDynamicInterfaces)
        {
            var delegateBuilder = DynamicAssembly.DefineType(
                $"{nameof(MethodHelper)}_{nameof(Delegate)}_{Guid.NewGuid().ToString("N")}",
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
                resultType,
                parameterTypes);

            invokeBuilder.SetImplementationFlags(MethodImplAttributes.Runtime);

            if (implDynamicInterfaces)
            {
                OverrideDynamicInvokeImpl(delegateBuilder, invokeBuilder, parameterTypes, resultType);

                ImplInterface(delegateBuilder, invokeBuilder, parameterTypes, resultType);

                ImplObjectInterface(delegateBuilder, invokeBuilder, parameterTypes, resultType);

                ImplInstanceDynamicImpl(delegateBuilder, invokeBuilder, parameterTypes, resultType);
            }

            return delegateBuilder.CreateTypeInfo();
        }

        private unsafe static Type GetOrDefineDelegateType(Type[] parameterTypes, Type resultType, bool implDynamicInterfaces)
        {
            var name = "None";

            if (implDynamicInterfaces)
            {
                name = "WithInterface";
            }

            var methodSign = new MethodSign(name, parameterTypes, resultType);

            Type delegateType;

            if (DelegateTypesCache.TryGetValue(methodSign, out delegateType))
            {
                return delegateType;
            }

            lock (DelegateTypesCacheLock)
            {
                if (DelegateTypesCache.TryGetValue(methodSign, out delegateType))
                {
                    return delegateType;
                }

                delegateType = DefineDelegateType(parameterTypes, resultType, implDynamicInterfaces);

                DelegateTypesCache.Add(methodSign, delegateType);
            }

            return delegateType;
        }

        private unsafe static void ImplInterface(TypeBuilder delegateBuilder, MethodInfo invokeMethod, Type[] parameterTypes, Type resultType)
        {
            Type[] unByRefTypes = new Type[parameterTypes.Length];

            bool haveByRef = false;

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                if (parameterTypes[i].IsByRef || parameterTypes[i].IsPointer || parameterTypes[i] == typeof(TypedReference))
                {
                    unByRefTypes[i] = typeof(IntPtr);

                    haveByRef = true;

                    continue;
                }

                unByRefTypes[i] = parameterTypes[i];
            }

            if (resultType.IsPointer || resultType == typeof(TypedReference))
            {
                haveByRef = true;

                resultType = typeof(IntPtr);
            }

            Type interfaceType;

            if (resultType == typeof(void))
            {
                if (parameterTypes.Length >= ActionInterfaces.Length)
                {
                    return;
                }

                interfaceType = ActionInterfaces[parameterTypes.Length];

                if (interfaceType.IsGenericTypeDefinition)
                {
                    interfaceType = interfaceType.MakeGenericType(unByRefTypes);
                }
            }
            else
            {
                if (parameterTypes.Length >= FuncInterfaces.Length)
                {
                    return;
                }

                interfaceType = FuncInterfaces[parameterTypes.Length];

                if (interfaceType.IsGenericTypeDefinition)
                {
                    var genericTypes = unByRefTypes;

                    Array.Resize(ref genericTypes, unByRefTypes.Length + 1);

                    genericTypes[unByRefTypes.Length] = resultType;

                    interfaceType = interfaceType.MakeGenericType(genericTypes);
                }
            }

            if (!interfaceType.IsVisible)
            {
                return;
            }

            delegateBuilder.AddInterfaceImplementation(interfaceType);

            if (haveByRef)
            {
                var invokeBuilder = delegateBuilder.DefineMethod(
                    "InvokeImpl",
                    MethodAttributes.Public | MethodAttributes.Virtual,
                    CallingConventions.Standard,
                    resultType,
                    unByRefTypes);

                var iLGen = invokeBuilder.GetILGenerator();

                for (int i = 0; i <= unByRefTypes.Length; ++i)
                {
                    iLGen.LoadArgument(i);
                }

                iLGen.Call(invokeMethod);

                iLGen.Return();

                delegateBuilder.DefineMethodOverride(invokeBuilder, interfaceType.GetMethod("Invoke"));
            }
            else
            {
                delegateBuilder.DefineMethodOverride(invokeMethod, interfaceType.GetMethod("Invoke"));
            }
        }

        private unsafe static void ImplObjectInterface(TypeBuilder delegateBuilder, MethodInfo invokeMethod, Type[] parameterTypes, Type resultType)
        {
            bool isAllEqualObject = true;

            foreach (var item in parameterTypes)
            {
                if (item != typeof(object))
                {
                    isAllEqualObject = false;

                    break;
                }
            }

            if (isAllEqualObject)
            {
                return;
            }

            Type[] objectTypes = new Type[parameterTypes.Length];

            for (int i = 0; i < objectTypes.Length; i++)
            {
                objectTypes[i] = typeof(object);
            }

            Type interfaceType;

            if (resultType == typeof(void))
            {
                if (parameterTypes.Length >= ActionInterfaces.Length)
                {
                    return;
                }

                interfaceType = ActionInterfaces[parameterTypes.Length];

                if (interfaceType.IsGenericTypeDefinition)
                {
                    interfaceType = interfaceType.MakeGenericType(objectTypes);
                }
            }
            else
            {
                if (parameterTypes.Length >= FuncInterfaces.Length)
                {
                    return;
                }

                interfaceType = FuncInterfaces[parameterTypes.Length];

                if (interfaceType.IsGenericTypeDefinition)
                {
                    var genericTypes = objectTypes;

                    Array.Resize(ref genericTypes, objectTypes.Length + 1);

                    genericTypes[objectTypes.Length] = typeof(object);

                    interfaceType = interfaceType.MakeGenericType(genericTypes);
                }
            }

            delegateBuilder.AddInterfaceImplementation(interfaceType);

            var interfaceInvoke = interfaceType.GetMethod("Invoke");

            var invokeBuilder = delegateBuilder.DefineMethod(
                "InvokeImpl",
                MethodAttributes.Public | MethodAttributes.Virtual,
                CallingConventions.Standard,
                interfaceInvoke.ReturnType,
                objectTypes);

            var iLGen = invokeBuilder.GetILGenerator();

            iLGen.LoadArgument(0);

            for (int i = 0; i < parameterTypes.Length; ++i)
            {
                var item = parameterTypes[i];

                iLGen.LoadArgument(i + 1);

                if (item.IsByRef)
                {
                    item = typeof(IntPtr);
                }

                iLGen.CastClass(item);

                if (item.IsValueType)
                {
                    iLGen.UnboxAny(item);
                }
            }

            iLGen.Call(invokeMethod);

            if (resultType.IsValueType)
            {
                iLGen.Box(resultType);
            }

            iLGen.Return();

            delegateBuilder.DefineMethodOverride(invokeBuilder, interfaceInvoke);
        }

        private unsafe static void OverrideDynamicInvokeImpl(TypeBuilder delegateBuilder, MethodInfo invokeMethod, Type[] parameterTypes, Type resultType)
        {
            var dynamicInvokeMethodBuilder = delegateBuilder.DefineMethod(
                "DynamicInvokeImpl",
                MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                CallingConventions.Standard,
                typeof(object),
                new Type[] { typeof(object[]) });

            var iLGen = dynamicInvokeMethodBuilder.GetILGenerator();

            iLGen.LoadArgument(0);

            for (int i = 0; i < parameterTypes.Length; i++)
            {
                var item = parameterTypes[i];

                iLGen.LoadArgument(1);
                iLGen.LoadConstant(i);

                if (item.IsByRef)
                {
                    item = item.GetElementType();

                    iLGen.LoadReferenceElement();
                    iLGen.CastClass(item);

                    if (item.IsValueType)
                    {
                        iLGen.LoadConstant(sizeof(IntPtr));
                        iLGen.Add();

                        continue;
                    }

                    iLGen.Pop();
                    iLGen.LoadArgument(1);
                    iLGen.LoadConstant(i);
                    iLGen.LoadElementAddress(typeof(object));

                    continue;
                }

                iLGen.LoadReferenceElement();
                iLGen.CastClass(item);

                if (item.IsValueType)
                {
                    iLGen.UnboxAny(item);
                }
            }

            iLGen.Call(invokeMethod);

            if (resultType == typeof(void))
            {
                iLGen.LoadNull();
            }
            else if (resultType.IsValueType)
            {
                iLGen.Box(resultType);
            }

            iLGen.Return();
        }

        private static readonly Type Type_IInstanceDynamicInvoker = 
            typeof(IInstanceDynamicInvoker);

        private static readonly MethodInfo Method_IInstanceDynamicInvoker_Invoke = 
            Type_IInstanceDynamicInvoker.GetMethod(nameof(IInstanceDynamicInvoker.Invoke));

        private static readonly MethodInfo Method_GetStructRef = typeof(MethodHelper).GetMethod(nameof(MethodHelper.GetStructRef));

        /// <summary>
        /// 获取已装箱值类型的引用。
        /// </summary>
        /// <param name="value">已装箱值</param>
        /// <returns>返回结构的引用</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ref byte GetStructRef(object value) => ref TypeHelper.Unbox<byte>(value);

        private static void ImplInstanceDynamicImpl(TypeBuilder delegateBuilder, MethodInfo invokeMethod, Type[] parameterTypes, Type resultType)
        {
            if (!(parameterTypes.Length != 0 && // Must one or more Parameters.
                (parameterTypes[0].IsClass || parameterTypes[0].IsInterface || // First Parameter Is Class Or Interface.
                (parameterTypes[0].IsByRef && parameterTypes[0].GetElementType().IsValueType) // Or First Parameter Is ValueType Ref.
                )))
            {
                return;
            }

            GetParametersTypes(Method_IInstanceDynamicInvoker_Invoke, out var invokerParameterTypes, out var invokerReturnType, true);

            delegateBuilder.AddInterfaceImplementation(Type_IInstanceDynamicInvoker);

            var dynamicInvokeMethodBuilder = delegateBuilder.DefineMethod(
                "DynamicInvokeImpl",
                MethodAttributes.Family | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                CallingConventions.Standard,
                invokerReturnType,
                invokerParameterTypes);

            var iLGen = dynamicInvokeMethodBuilder.GetILGenerator();

            iLGen.LoadArgument(0);
            iLGen.LoadArgument(1);

            for (int i = 1; i < parameterTypes.Length; i++)
            {
                var item = parameterTypes[i];

                iLGen.LoadArgument(2);
                iLGen.LoadConstant(i - 1);

                iLGen.LoadReferenceElement();

                if (item.IsByRef)
                {
                    item = item.GetElementType();

                    iLGen.CastClass(item);

                    if (item.IsValueType)
                    {
                        iLGen.Call(Method_GetStructRef);

                        continue;
                    }

                    iLGen.Pop();
                    iLGen.LoadArgument(2);
                    iLGen.LoadConstant(i - 1);
                    iLGen.LoadElementAddress(typeof(object));

                    continue;
                }

                if (item.IsValueType)
                {
                    iLGen.UnboxAny(item);
                }
            }

            iLGen.Call(invokeMethod);

            if (resultType == typeof(void))
            {
                iLGen.LoadNull();
            }
            else if (resultType.IsValueType)
            {
                iLGen.Box(resultType);
            }

            iLGen.Return();


            delegateBuilder.DefineMethodOverride(dynamicInvokeMethodBuilder, Method_IInstanceDynamicInvoker_Invoke);
        }

        private enum CreateWays : byte
        {
            Pointer = 1,
            Default = 2,
            Proxy = 4,
        }
    }
}