using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using CacheType = Swifter.Tools.HashCache<System.IntPtr, int>;

namespace Swifter.Tools
{
    internal static class OffsetHelper
    {
        private static readonly object CacheLock = 
            new object();

        private static readonly CacheType Cache = 
            new CacheType();

        private static readonly MethodInfo AddMethod = 
            typeof(CacheType).GetMethod(nameof(CacheType.DirectAdd), new Type[] { typeof(IntPtr), typeof(int) });

        private static readonly MethodInfo MinMethod =
            typeof(OffsetHelper).GetMethod(nameof(OffsetHelper.Min), new Type[] { typeof(IntPtr), typeof(IntPtr) });


        public static readonly bool OffsetOfByHandleIsAvailable = OffsetOfByHandleIsAvailable =
                    (int)Marshal.OffsetOf(typeof(OffsetOfTest), nameof(OffsetOfTest.Field1)) == OffsetOfByHandle(typeof(OffsetOfTest).GetField(nameof(OffsetOfTest.Field1))) &&
                    (int)Marshal.OffsetOf(typeof(OffsetOfTest), nameof(OffsetOfTest.Field2)) == OffsetOfByHandle(typeof(OffsetOfTest).GetField(nameof(OffsetOfTest.Field2))) &&
                    (int)Marshal.OffsetOf(typeof(OffsetOfTest), nameof(OffsetOfTest.Field3)) == OffsetOfByHandle(typeof(OffsetOfTest).GetField(nameof(OffsetOfTest.Field3)));

        private static IntPtr Min(IntPtr ptr1, IntPtr ptr2)
        {
            return ((ulong)ptr1 < (ulong)ptr2) ? ptr1 : ptr2;
        }

        public static void GetAllFieldOffset(Type type)
        {
            var dynamicMethod = new DynamicMethod(
                $"{nameof(OffsetHelper)}_{type.Name}_{Guid.NewGuid().ToString("N")}",
                null, new Type[] { typeof(CacheType) },
                type.Module,
                true);

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            var ilGen = dynamicMethod.GetILGenerator();

            var instance = ilGen.DeclareLocal(typeof(IntPtr));
            var first = ilGen.DeclareLocal(typeof(IntPtr));

            /* 分配一个模拟实例 */
            ilGen.LoadConstant(8);
            ilGen.LocalAllocate();
            ilGen.StoreLocal(instance);

            /* 找到最小的静态字段地址 */
            var isFirstStatic = true;

            foreach (var item in fields)
            {
                /* 跳过常量 */
                if (item.IsLiteral)
                {
                    continue;
                }

                if (item.IsStatic)
                {
                    ilGen.LoadFieldAddress(item);

                    if (isFirstStatic)
                    {
                        isFirstStatic = false;
                    }
                    else
                    {
                        ilGen.LoadLocal(first);
                        ilGen.Call(MinMethod);
                    }

                    ilGen.StoreLocal(first);
                }
            }

            foreach (var item in fields)
            {
                /* 跳过常量 */
                if (item.IsLiteral)
                {
                    continue;
                }

                /* 加载 IdCache 实例 */
                ilGen.LoadArgument(0);
                /* 加载字段 Id */
                ilGen.LoadConstant((long)item.FieldHandle.Value);
                ilGen.ConvertPointer();

                if (item.IsStatic)
                {
                    // Offset = Address - First Address.
                    ilGen.LoadFieldAddress(item);
                    ilGen.LoadLocal(first);
                    ilGen.Subtract();
                }
                else
                {
                    /* Offset = Address - Instance. */
                    ilGen.LoadLocal(instance);
                    ilGen.LoadFieldAddress(item);
                    ilGen.LoadLocal(instance);
                    ilGen.Subtract();

                    /* if class then Offset -= sizeof TypeHandle. */
                    if (type.IsClass)
                    {
                        ilGen.LoadConstant(IntPtr.Size);
                        ilGen.Subtract();
                    }
                }

                ilGen.ConvertInt32();
                ilGen.Call(AddMethod);
            }

            ilGen.Return();

            var action = (Action<CacheType>)dynamicMethod.CreateDelegate(typeof(Action<CacheType>));

            /* Call the DynamicMethod. */
            action(Cache);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int LockGetOffset(FieldInfo fieldInfo)
        {
            lock (CacheLock)
            {
                var handle = fieldInfo.FieldHandle.Value;

                if (Cache.TryGetValue(handle, out var value))
                {
                    return value;
                }

                GetAllFieldOffset(fieldInfo.DeclaringType);

                return Cache[handle];
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetOffsetByDynamic(FieldInfo fieldInfo)
        {
            var handle = fieldInfo.FieldHandle.Value;

            if (Cache.TryGetValue(handle, out var value))
            {
                return value;
            }

            return LockGetOffset(fieldInfo);
        }
        
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe int OffsetOfByHandle(FieldInfo fieldInfo)
        {
            var pFieldHandleInfo = (FieldHandleStruct*)fieldInfo.FieldHandle.Value;

            return pFieldHandleInfo->Offset;
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct OffsetOfTest
        {
            public byte Field1;
            public int Field2;
            public long Field3;
        }
    }
}