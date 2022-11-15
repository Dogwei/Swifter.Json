using InlineIL;
using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading;


namespace Swifter.Tools
{
    static unsafe class InternalMethodHelper
    {
        const long NoInit = 0;
        const long Initing = -1;

        const long ValueAdd = 0xf;

        static long OffsetOfMethodPtrAux;
        static long OffsetOfMethodPtr;
        static long OverrideIsAvailable;
        static long PtrOfGetMethodDescriptor;
        static long PtrOfGetNativeFunctionPointer;

        public static long GetOrInitValue(ref long value, delegate*<long> init)
        {
            if (value is NoInit)
            {
                if (Interlocked.CompareExchange(ref value, Initing, NoInit) is NoInit)
                {
                    value = init() + ValueAdd;
                }

                while (Thread.VolatileRead(ref value) is Initing) /* TODO: Sleep */;
            }

            return value - ValueAdd;
        }

        public static bool GetOverrideIsAvailable()
        {
            return GetOrInitValue(ref OverrideIsAvailable, &Init) is not 0;

            static long Init()
            {
                try
                {
                    MethodHelper.Override(
                        TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(InternalMethodHelper), nameof(TestSource)))),
                        TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(InternalMethodHelper), nameof(TestTarget))))
                        );

                    return TestSource(12, 18) == TestTarget(12, 18) ? 1 : 0;
                }
                catch
                {
                    return 0;
                }
            }
        }

        public static int GetOffsetOfMethodPtrAux()
        {
            return (int)GetOrInitValue(ref OffsetOfMethodPtrAux, &Init);

            static long Init()
            {
                return typeof(Delegate).GetField("_methodPtrAux", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) is FieldInfo fieldInfo
                    ? TypeHelper.OffsetOf(fieldInfo)
                    : -1;
            }
        }

        public static int GetOffsetOfMethodPtr()
        {
            return (int)GetOrInitValue(ref OffsetOfMethodPtr, &Init);

            static long Init()
            {
                return typeof(Delegate).GetField("_methodPtr", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) is FieldInfo fieldInfo
                    ? TypeHelper.OffsetOf(fieldInfo)
                    : -1;
            }
        }

        public static void* GetPtrOfGetMethodDescriptor()
        {
            return (void*)GetOrInitValue(ref PtrOfGetMethodDescriptor, &Init);

            static long Init()
            {
                return typeof(DynamicMethod).GetMethod("GetMethodDescriptor", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) is MethodInfo methodInfo
                    ? (long)methodInfo.MethodHandle.GetFunctionPointer()
                    : 0;
            }
        }

        public static void* GetPtrOfGetNativeFunctionPointer()
        {
            return (void*)GetOrInitValue(ref PtrOfGetNativeFunctionPointer, &Init);

            static long Init()
            {
                return typeof(Delegate).GetMethod("GetNativeFunctionPointer", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public) is MethodInfo methodInfo
                    ? (long)methodInfo.MethodHandle.GetFunctionPointer()
                    : 0;
            }
        }

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        static int TestSource(int x, int y) => x + y;

        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        static int TestTarget(int x, int y) => x * y;
    }
}