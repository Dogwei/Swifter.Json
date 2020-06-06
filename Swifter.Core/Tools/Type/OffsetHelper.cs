using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static Swifter.Tools.MethodHelper;
using CacheType = System.Collections.Generic.Dictionary<System.IntPtr, int>;

namespace Swifter.Tools
{
    internal static class OffsetHelper
    {
        private static readonly object CacheLock =
            new object();

        private static readonly CacheType Cache =
            new CacheType();


        public static readonly bool OffsetOfByFieldDescIsAvailable;

        static OffsetHelper()
        {
            try
            {
                OffsetOfByFieldDescIsAvailable =
                    (int)Marshal.OffsetOf(typeof(OffsetOfTest), nameof(OffsetOfTest.Field1)) == GetOffsetByFieldDesc(typeof(OffsetOfTest).GetField(nameof(OffsetOfTest.Field1))) &&
                    (int)Marshal.OffsetOf(typeof(OffsetOfTest), nameof(OffsetOfTest.Field2)) == GetOffsetByFieldDesc(typeof(OffsetOfTest).GetField(nameof(OffsetOfTest.Field2))) &&
                    (int)Marshal.OffsetOf(typeof(OffsetOfTest), nameof(OffsetOfTest.Field3)) == GetOffsetByFieldDesc(typeof(OffsetOfTest).GetField(nameof(OffsetOfTest.Field3)));
            }
            catch
            {
                OffsetOfByFieldDescIsAvailable = false;
            }
        }

        private static IntPtr Min(IntPtr ptr1, IntPtr ptr2)
        {
            return ((ulong)ptr1 < (ulong)ptr2) ? ptr1 : ptr2;
        }

        private static void Add(IntPtr add, int offset)
        {
            Cache[add] = offset;
        }

        public static void GetAllFieldOffsetByEmit(Type type)
        {
            var fields = TypeHelper.GetFields(type, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            DynamicAssembly.DefineDynamicMethod<Action>((_, ilGen) =>
            {
                var obj_address = ilGen.DeclareLocal(typeof(IntPtr));
                var gc_static_base_address = ilGen.DeclareLocal(typeof(IntPtr));
                var non_gc_static_base_address = ilGen.DeclareLocal(typeof(IntPtr));

                ilGen.LoadConstant((IntPtr)(-1)); // max ptr
                ilGen.StoreLocal(gc_static_base_address);

                ilGen.LoadConstant((IntPtr)(-1)); // max ptr
                ilGen.StoreLocal(non_gc_static_base_address);

                /* 分配一个模拟实例 */
                ilGen.LoadConstant(8);
                ilGen.LocalAllocate();
                ilGen.StoreLocal(obj_address);

                foreach (var item in fields)
                {
                    /* 跳过常量 */
                    if (item.IsLiteral)
                    {
                        continue;
                    }

                    if (item.IsStatic)
                    {
                        if (item.FieldType.IsValueType)
                        {
                            ilGen.LoadFieldAddress(item);
                            ilGen.LoadLocal(non_gc_static_base_address);
                            ilGen.Call(MethodOf<IntPtr, IntPtr, IntPtr>(Min));
                            ilGen.StoreLocal(non_gc_static_base_address);
                        }
                        else
                        {
                            ilGen.LoadFieldAddress(item);
                            ilGen.LoadLocal(gc_static_base_address);
                            ilGen.Call(MethodOf<IntPtr, IntPtr, IntPtr>(Min));
                            ilGen.StoreLocal(gc_static_base_address);
                        }
                    }
                }

                foreach (var item in fields)
                {
                    /* 跳过常量 */
                    if (item.IsLiteral)
                    {
                        continue;
                    }

                    /* 加载字段 Id */
                    ilGen.LoadConstant(item.FieldHandle.Value);

                    if (item.IsStatic)
                    {
                        // Offset = Address - Base Address.

                        if (item.FieldType.IsValueType)
                        {
                            ilGen.LoadFieldAddress(item);
                            ilGen.LoadLocal(non_gc_static_base_address);
                            ilGen.Subtract();
                        }
                        else
                        {
                            ilGen.LoadFieldAddress(item);
                            ilGen.LoadLocal(gc_static_base_address);
                            ilGen.Subtract();
                        }
                    }
                    else
                    {
                        /* Offset = Address - Instance. */
                        ilGen.LoadLocal(obj_address);
                        ilGen.LoadFieldAddress(item);
                        ilGen.LoadLocal(obj_address);
                        ilGen.Subtract();

                        /* if class then Offset -= sizeof TypeHandle. */
                        if (type.IsClass)
                        {
                            ilGen.LoadConstant(TypeHelper.GetObjectValueByteOffset());
                            ilGen.Subtract();
                        }
                    }

                    ilGen.ConvertInt32();
                    ilGen.Call(MethodOf<IntPtr, int>(Add));
                }

                ilGen.Return();

            }, type.Module, true)();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int LockGetOffsetByEmit(FieldInfo fieldInfo)
        {
            lock (CacheLock)
            {
                var handle = fieldInfo.FieldHandle.Value;

                if (Cache.TryGetValue(handle, out var value))
                {
                    return value;
                }

                GetAllFieldOffsetByEmit(fieldInfo.DeclaringType);

                if (Cache.TryGetValue(handle, out value))
                {
                    return value;
                }

                return -1;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int LockGetOffsetByIndexOfInstance(FieldInfo fieldInfo)
        {
            lock (CacheLock)
            {
                var handle = fieldInfo.FieldHandle.Value;

                if (Cache.TryGetValue(handle, out var value))
                {
                    return value;
                }

                try
                {
                    GetAllFieldOffsetByIndexOfInstance(fieldInfo.DeclaringType);
                }
                catch
                {
                }

                if (Cache.TryGetValue(handle, out value))
                {
                    return value;
                }

                return -1;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetOffsetByEmit(FieldInfo fieldInfo)
        {
            var handle = fieldInfo.FieldHandle.Value;

            if (Cache.TryGetValue(handle, out var value))
            {
                return value;
            }

            return LockGetOffsetByEmit(fieldInfo);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static int GetOffsetByIndexOfInstance(FieldInfo fieldInfo)
        {
            var handle = fieldInfo.FieldHandle.Value;

            if (Cache.TryGetValue(handle, out var value))
            {
                return value;
            }

            return LockGetOffsetByIndexOfInstance(fieldInfo);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe int GetOffsetByFieldDesc(FieldInfo fieldInfo)
        {
            var pFieldDesc = (FieldDesc*)fieldInfo.FieldHandle.Value;

            return (int)pFieldDesc->m_dwOffset;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static unsafe bool IsThreadStaticOfByFieldDesc(FieldInfo fieldInfo)
        {
            return ((FieldDesc*)fieldInfo.FieldHandle.Value)->m_isThreadLocal;
        }

        public static unsafe void GetAllFieldOffsetByIndexOfInstance(Type declaringType)
        {
            object obj = null;

            Loop:

            try
            {
                obj = TypeHelper.Allocate(declaringType);
            }
            catch
            {
                if (declaringType.IsAbstract && TypeHelper.GetTypesForAllAssembly().FirstOrDefault(type => declaringType.IsAssignableFrom(type) && !type.IsAbstract) is Type subClass)
                {
                    declaringType = subClass;

                    goto Loop;
                }
            }

            if (obj is null)
            {
                return;
            }

            var fields = TypeHelper.GetFields(declaringType, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            var length = TypeHelper.GetAlignedNumInstanceFieldBytes(declaringType);

            fixed (byte* ptr = &Underlying.As<StructBox<byte>>(obj).Value)
            {
                foreach (var fieldInfo in fields)
                {
                    Underlying.InitBlock(ptr, 0xff, (uint)length);

                    fieldInfo.SetValue(obj, TypeHelper.GetDefaultValue(fieldInfo.FieldType));

                    for (int i = 0; i < length; i++)
                    {
                        if (ptr[i] == 0)
                        {
                            Add(fieldInfo.FieldHandle.Value, i);

                            break;
                        }
                    }
                }

                Underlying.InitBlock(ptr, 0, (uint)length);
            }
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