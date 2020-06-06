using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Swifter.Tools
{
    static class AllocateHelper
    {
        const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        public static readonly Func<Type, object>[] AllocateMethods = GetAllocateMethods().ToArray();

        public static readonly int ObjectValueByteOffset = GetObjectValueByteOffset();

        public static unsafe int GetObjectValueByteOffset()
        {
            var obj = new StructBox<byte>();

            fixed (byte* ptr = &obj.Value)
                return (int)(ptr - (byte*)Underlying.AsPointer(ref Underlying.GetMethodTablePointer(obj)));
        }

        public static IEnumerable<Func<Type, object>> GetAllocateMethods()
        {
            {
                if (TypeHelper.GetTypeForAllAssembly("System.RuntimeTypeHandle")?.GetMethod("Allocate", Flags) is MethodInfo methodInfo)
                {
                    MethodHelper.GetParametersTypes(methodInfo, out var parameterTypes, out var returnType);

                    if (returnType == typeof(object) && parameterTypes.Length == 1 && parameterTypes[0].IsInstanceOfType(typeof(object)))
                    {
                        yield return MethodHelper.CreateDelegate<Func<Type, object>>(methodInfo, false);
                    }
                }
            }

            {
                if (TypeHelper.GetTypeForAllAssembly("System.StubHelpers.StubHelpers")?.GetMethod("AllocateInternal", Flags) is MethodInfo methodInfo)
                {
                    MethodHelper.GetParametersTypes(methodInfo, out var parameterTypes, out var returnType);

                    if (returnType == typeof(object) && parameterTypes.Length == 1 && parameterTypes[0].IsInstanceOfType(typeof(object)))
                    {
                        yield return MethodHelper.CreateDelegate<Func<Type, object>>(methodInfo, false);
                    }
                }
            }

            {
                if (TypeHelper.GetTypeForAllAssembly("System.Runtime.Remoting.Activation.ActivationServices")?.GetMethod("AllocateUninitializedClassInstance", Flags) is MethodInfo methodInfo)
                {
                    MethodHelper.GetParametersTypes(methodInfo, out var parameterTypes, out var returnType);

                    if (returnType == typeof(object) && parameterTypes.Length == 1 && parameterTypes[0].IsInstanceOfType(typeof(object)))
                    {
                        yield return MethodHelper.CreateDelegate<Func<Type, object>>(methodInfo, false);
                    }

                    if (returnType == typeof(object) && parameterTypes.Length == 1 && parameterTypes[0] == typeof(IntPtr))
                    {
                        var internalFunc = MethodHelper.CreateDelegate<Func<IntPtr, object>>(methodInfo, false);

                        yield return (type) => internalFunc(type.TypeHandle.Value);
                    }
                }
            }
        }
    }
}