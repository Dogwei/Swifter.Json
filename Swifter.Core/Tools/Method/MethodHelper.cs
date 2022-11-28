using InlineIL;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Swifter.Tools.InternalMethodHelper;

namespace Swifter.Tools
{
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
        public static T? CreateDelegate<T>(MethodBase methodInfo, bool throwExceptions = true) where T : Delegate
        {
            return Unsafe.As<T>(CreateDelegate(typeof(T), methodInfo, throwExceptions));
        }

        /// <summary>
        /// 创建一个指定类型的委托。
        /// </summary>
        /// <param name="delegateType">委托类型</param>
        /// <param name="methodBase">需要创建委托的方法</param>
        /// <param name="throwExceptions">当参数或返回值类型不兼容时是否发生异常。</param>
        /// <returns>返回一个委托或 Null。</returns>
        public static Delegate? CreateDelegate(Type delegateType, MethodBase methodBase, bool throwExceptions)
        {
            if (methodBase.ContainsGenericParameters)
                return throwExceptions ? throw new ArgumentException("Can't create a delegate for a generic method.") : default(Delegate);

            if (methodBase is not ConstructorInfo and not MethodInfo)
                return throwExceptions ? throw new ArgumentException("Can't create a delegate for a unknow method.") : default(Delegate);

            GetSignature(methodBase, out var targetParametersTypes, out var sourceReturnType);
            GetSignature(delegateType, out var sourceParametersTypes, out var targetReturnType);

            if (sourceParametersTypes.Length != targetParametersTypes.Length)
                return throwExceptions ? throw new ArgumentException("Parameter quantity does not match.") : default(Delegate);

            if (sourceReturnType != targetReturnType && (sourceReturnType == typeof(void) || targetReturnType == typeof(void)))
                return throwExceptions ? throw new ArgumentException("Return type does not match.") : default(Delegate);

            if (throwExceptions)
            {
                for (int i = 0; i < sourceParametersTypes.Length; i++)
                    if (sourceParametersTypes[i] != targetParametersTypes[i])
                        throw new ArgumentException("Parameter type does not match.");

                if (sourceReturnType != targetReturnType)
                    throw new ArgumentException("Return type does not match.");
            }

            {
                if (methodBase is ConstructorInfo && InternalCreateDelegateByPointer(delegateType, methodBase) is Delegate result)
                    return result;
            }

            if (methodBase is DynamicMethod dynamicMethod)
            {
                try
                {
                    return dynamicMethod.CreateDelegate(delegateType);
                }
                catch (ArgumentException)
                {
                    return InternalCreateDelegateByPointer(delegateType, dynamicMethod);
                }
            }

            {
                if (InternalCreateDelegateBySystem(delegateType, Unsafe.As<MethodInfo>(methodBase)) is Delegate result)
                    return result;
            }

            if (VersionDifferences.IsSupportEmit)
                return InternalCreateDelegateByDynamicMethodProxy(delegateType, Unsafe.As<MethodInfo>(methodBase));

            if (throwExceptions)
                throw new NotSupportedException("Failed to create delegate.");

            return default;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="methodBase"></param>
        /// <returns></returns>
        public static Delegate? CreateDynamicDelegate(MethodBase methodBase)
        {
            GetSignature(methodBase, out var parameterTypes, out var returnType);

            Type? delegateType;

            if (VersionDifferences.IsSupportEmit)
            {
                delegateType = DefineDelegateType(parameterTypes, returnType);
            }
            else
            {
                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    ref var parameterType = ref parameterTypes[i];

                    if (parameterType.IsPointer || parameterType.IsByRef)
                    {
                        parameterType = typeof(IntPtr);
                    }
                }

                if (returnType.IsPointer || returnType.IsByRef)
                {
                    returnType = typeof(IntPtr);
                }

                delegateType = MakeGenericDelegateType(parameterTypes, returnType);
            }

            if (delegateType is null)
            {
                return null;
            }

            return CreateDelegate(delegateType, methodBase, false);
        }

        /// <summary>
        /// 使用系统绑定方式创建委托。
        /// </summary>
        private static Delegate? InternalCreateDelegateBySystem(Type delegateType, MethodInfo methodInfo)
        {
            try
            {
                return Delegate.CreateDelegate(delegateType, methodInfo, false);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 使用函数指针方式创建委托。
        /// </summary>
        private static Delegate? InternalCreateDelegateByPointer(Type delegateType, MethodBase methodInfo)
        {
            try
            {
                var constructor = delegateType
                    .GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, new Type[] { typeof(object), typeof(IntPtr) }, null);

                if (constructor is null)
                {
                    return null;
                }

                return (Delegate)constructor.Invoke(new object?[] { null, methodInfo.GetFunctionPointer() });
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 使用动态方法代理模式创建委托。
        /// </summary>
        private static Delegate InternalCreateDelegateByDynamicMethodProxy(Type delegateType, MethodInfo methodInfo)
        {
            GetSignature(delegateType, out var parameterTypes, out var returnType);

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

            ilGen.AutoCall(methodInfo);

            ilGen.Return();

            return dynamicMethod.CreateDelegate(delegateType);
        }

        /// <summary>
        /// 获取委托类型的参数类型集合和返回值类型。
        /// </summary>
        /// <param name="delegateType">委托类型</param>
        /// <param name="parameterTypes">返回参数类型集合</param>
        /// <param name="returnType">返回返回值类型</param>
        public static void GetSignature(Type delegateType, out Type[] parameterTypes, out Type returnType)
        {
            var method = delegateType.GetMethod(InvokeMethodName)!;

            parameterTypes = method.GetParameterTypes();

            returnType = method.ReturnType;
        }

        /// <summary>
        /// 获取方法的参数类型集合和返回值类型。
        /// </summary>
        /// <param name="methodInfo">方法</param>
        /// <param name="parameterTypes">返回参数类型集合</param>
        /// <param name="returnType">返回返回值类型</param>
        public static void GetSignature(MethodBase methodInfo, out Type[] parameterTypes, out Type returnType)
        {
            returnType = (methodInfo as MethodInfo)?.ReturnType 
                ?? typeof(void);

            if (methodInfo.IsStatic)
            {
                parameterTypes = methodInfo.GetParameterTypes();
            }
            else
            {
                var parameters = methodInfo.GetParameters();

                parameterTypes = new Type[parameters.Length + 1];

                parameterTypes[0] = methodInfo.DeclaringType! switch
                {
                    var declaringType when declaringType.IsValueType => declaringType.MakeByRefType(),
                    var declaringType => declaringType
                };

                for (int i = 0; i < parameters.Length; i++)
                {
                    parameterTypes[i + 1] = parameters[i].ParameterType;
                }
            }
        }

        /// <summary>
        /// 获取方法的参数类型集合。
        /// </summary>
        /// <param name="methodBase">方法</param>
        /// <returns>返回参数类型集合</returns>
        public static Type[] GetParameterTypes(this MethodBase methodBase)
        {
            var parameters = methodBase.GetParameters();
            var parameterTypes = new Type[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                parameterTypes[i] = parameters[i].ParameterType;
            }

            return parameterTypes;
        }

        /// <summary>
        /// 创建一个泛型委托类型。泛型定义类型是 System.Func 或 System.Action。
        /// 如果创建失败（仅当参数类型或返回值类型不能作为泛型时）则返回 Null。
        /// </summary>
        /// <param name="parameterTypes">参数类型集合</param>
        /// <param name="returnType">返回值类型</param>
        /// <returns>返回一个委托类型</returns>
        public static Type? MakeGenericDelegateType(Type[] parameterTypes, Type returnType)
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

                var genericTypes = parameterTypes;

                Array.Resize(ref genericTypes, genericTypes.Length + 1);

                genericTypes.Last() = returnType;

                return parameterTypes.Length switch
                {
                    00 => typeof(Func<>).MakeGenericType(genericTypes),
                    01 => typeof(Func<,>).MakeGenericType(genericTypes),
                    02 => typeof(Func<,,>).MakeGenericType(genericTypes),
                    03 => typeof(Func<,,,>).MakeGenericType(genericTypes),
                    04 => typeof(Func<,,,,>).MakeGenericType(genericTypes),
                    05 => typeof(Func<,,,,,>).MakeGenericType(genericTypes),
                    06 => typeof(Func<,,,,,,>).MakeGenericType(genericTypes),
                    07 => typeof(Func<,,,,,,,>).MakeGenericType(genericTypes),
                    08 => typeof(Func<,,,,,,,,>).MakeGenericType(genericTypes),
                    09 => typeof(Func<,,,,,,,,,>).MakeGenericType(genericTypes),
                    10 => typeof(Func<,,,,,,,,,,>).MakeGenericType(genericTypes),
                    11 => typeof(Func<,,,,,,,,,,,>).MakeGenericType(genericTypes),
                    12 => typeof(Func<,,,,,,,,,,,,>).MakeGenericType(genericTypes),
                    13 => typeof(Func<,,,,,,,,,,,,,>).MakeGenericType(genericTypes),
                    14 => typeof(Func<,,,,,,,,,,,,,,>).MakeGenericType(genericTypes),
                    15 => typeof(Func<,,,,,,,,,,,,,,,>).MakeGenericType(genericTypes),
                    16 => typeof(Func<,,,,,,,,,,,,,,,,>).MakeGenericType(genericTypes),
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
        private static string GetDelegateDefinitionName(Type[] parameterTypes, Type returnType)
        {
            var numberHelper = NumberHelper.GetOrCreateInstance(62);
            var maxHandle = parameterTypes.Max(x => (ulong)x.TypeHandle.Value);

            bool isVoidReturnType = returnType == typeof(void);

            var delegateName
                = isVoidReturnType
                ? "Swifter.Dynamic.Action"
                : "Swifter.Dynamic.Func";

            if (!isVoidReturnType)
            {
                maxHandle = Math.Max(maxHandle, (ulong)returnType.TypeHandle.Value);
            }

            var maxHandleLength = numberHelper.GetLength(maxHandle);

            var resultLength = delegateName.Length + maxHandleLength * parameterTypes.Length;

            if (!isVoidReturnType)
            {
                resultLength += maxHandleLength;
            }

            var result = StringHelper.MakeString(resultLength);

            fixed (char* chars = result)
            {
                int offset = 0;

                Unsafe.CopyBlock(
                    ref Unsafe.As<char, byte>(ref Unsafe.AsRef(StringHelper.GetRawStringData(delegateName))),
                    ref Unsafe.As<char, byte>(ref chars[offset]),
                    (uint)(delegateName.Length * sizeof(char))
                    );
                offset += delegateName.Length;

                foreach (var parameterType in parameterTypes)
                {
                    numberHelper.ToString(
                        (ulong)parameterType.TypeHandle.Value,
                        maxHandleLength,
                        chars + offset
                        );
                    offset += maxHandleLength;
                }

                if (!isVoidReturnType)
                {
                    numberHelper.ToString(
                        (ulong)returnType.TypeHandle.Value,
                        maxHandleLength,
                        chars + offset
                        );
                    offset += maxHandleLength;
                }

                VersionDifferences.Assert(offset == resultLength);
            }

            return result;
        }

        /// <summary>
        /// 在动态程序集中定义一个委托类型。如果动态程序集中已有相同的委托类型，则直接返回该委托类型。<br/>
        /// 如果创建失败则返回 Null。<br/>
        /// 通常情况下仅当平台不支持 Emit 时会创建失败。<br/>
        /// </summary>
        /// <param name="parameterTypes">参数类型集合</param>
        /// <param name="returnType">返回值类型</param>
        /// <returns>返回一个委托类型</returns>
        public static Type DefineDelegateType(Type[] parameterTypes, Type returnType)
        {
            if (!VersionDifferences.IsSupportEmit)
            {
                throw new NotSupportedException("Does not support Emit.");
            }

            var delegateName = GetDelegateDefinitionName(parameterTypes, returnType);

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

            OverrideDynamicInvokeImpl(delegateBuilder, invokeBuilder, parameterTypes, returnType);

            return delegateBuilder.CreateTypeInfo()!;

        }

        private static void OverrideDynamicInvokeImpl(TypeBuilder delegateBuilder, MethodInfo invokeMethod, Type[] parametersTypes, Type returnType)
        {
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

                if (item.IsAddressType())
                {
                    ilGen.LoadReferenceElement();
                    ilGen.Duplicate();
                    ilGen.IsInstance(typeof(IntPtr));

                    var nextLabel = ilGen.DefineLabel();

                    ilGen.BranchTrue(nextLabel);

                    if (item.IsPointer || item.IsByRef)
                    {
                        ilGen.ThrowNewException(typeof(ArgumentException));
                    }
                    else
                    {
                        ilGen.CastClass(item);
                        ilGen.UnboxAny(item);
                    }

                    ilGen.MarkLabel(nextLabel);
                    ilGen.UnboxAny(typeof(IntPtr));
                }
                else if (item.IsClass)
                {
                    ilGen.LoadReferenceElement();
                    ilGen.CastClass(item);
                }
                else
                {
                    ilGen.LoadReferenceElement();
                    ilGen.CastClass(item);
                    ilGen.UnboxAny(item);
                }
            }

            ilGen.AutoCall(invokeMethod);

            if (returnType == typeof(void))
            {
                ilGen.LoadNull();
            }
            else if (returnType.IsAddressType())
            {
                ilGen.Box(typeof(IntPtr));
            }
            else if (returnType.IsValueType)
            {
                ilGen.Box(returnType);
            }

            ilGen.Return();
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
            if (OverrideIsAvailable == false)
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

            Unsafe.CopyBlockUnaligned(source, &jmp, (uint)sizeof(Jmp));

            return true;
        }

        /// <summary>
        /// 获取 Override 方法是否可用。
        /// </summary>
        public static bool OverrideIsAvailable => GetOverrideIsAvailable();

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static RuntimeMethodHandle GetMethodDescriptor(DynamicMethod dynamicMethod)
        {
            if (GetPtrOfGetMethodDescriptor() is null)
            {
                throw new NotSupportedException();
            }

            IL.Push(dynamicMethod);
            IL.Push(GetPtrOfGetMethodDescriptor());
            IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(CallingConventions.HasThis, typeof(RuntimeMethodHandle)));

            return IL.Return<RuntimeMethodHandle>();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IntPtr GetNativeFunctionPointer(Delegate @delegate)
        {
            if (GetPtrOfGetNativeFunctionPointer() is null)
            {
                throw new NotSupportedException();
            }

            IL.Push(@delegate);
            IL.Push(GetPtrOfGetNativeFunctionPointer());
            IL.Emit.Calli(StandAloneMethodSig.ManagedMethod(CallingConventions.HasThis, typeof(IntPtr)));

            return IL.Return<IntPtr>();
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

            if (@delegate.Target is not null && GetOffsetOfMethodPtr() >= 0)
            {
                return TypeHelper.AddByteOffset(ref TypeHelper.Unbox<IntPtr>(@delegate), GetOffsetOfMethodPtr());
            }

            if (@delegate.Target is null && GetOffsetOfMethodPtrAux() >= 0)
            {
                return TypeHelper.AddByteOffset(ref TypeHelper.Unbox<IntPtr>(@delegate), GetOffsetOfMethodPtrAux());
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
                    GetSignature(methodBase, out var parameterTypes, out var returnType);

                    if (CreateDelegate(DefineDelegateType(parameterTypes, returnType), methodBase, false) is Delegate @delegate)
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
    }
}