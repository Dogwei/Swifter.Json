using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    internal abstract class GenericInvokerHelper
    {
        private static readonly Dictionary<IntPtr, GenericInvokerHelper> Cache = new Dictionary<IntPtr, GenericInvokerHelper>();

        public static readonly bool GetBaseSizeIsAvailable;

        public static readonly int ObjectBaseSize;

        static GenericInvokerHelper()
        {
            try
            {
                ObjectBaseSize = GetBaseSize(typeof(object));

                GetBaseSizeIsAvailable = 
                    GetBaseSize(typeof(object)) - GetBaseSizePadding(typeof(object)) == 0 &&
                    GetBaseSize(typeof(GetBaseSizeTest)) - GetBaseSizePadding(typeof(GetBaseSizeTest)) == sizeof(int);
            }
            catch
            {
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static GenericInvokerHelper GetOrCreate(Type type)
        {
            if (Cache.TryGetValue(TypeHelper.GetTypeHandle(type), out var helper))
            {
                return helper;
            }

            return InternalGetOrCreate(type);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static GenericInvokerHelper GetOrCreate(object obj)
        {
            if (Cache.TryGetValue(TypeHelper.GetTypeHandle(obj), out var helper))
            {
                return helper;
            }

            return InternalGetOrCreate(obj);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static GenericInvokerHelper InternalGetOrCreate(Type type)
        {
            lock (Cache)
            {
                var key = TypeHelper.GetTypeHandle(type);

                if (!Cache.TryGetValue(key, out var helper))
                {
                    var implType = typeof(Impl<>).MakeGenericType(type);

                    helper = (GenericInvokerHelper)Activator.CreateInstance(implType);

                    Cache.Add(key, helper);
                }

                return helper;
            }
        }
        
        [MethodImpl(MethodImplOptions.NoInlining)]
        static GenericInvokerHelper InternalGetOrCreate(object obj)
        {
            lock (Cache)
            {
                var key = TypeHelper.GetTypeHandle(obj);

                if (!Cache.TryGetValue(key, out var helper))
                {
                    var implType = typeof(Impl<>).MakeGenericType(obj.GetType());

                    helper = (GenericInvokerHelper)Activator.CreateInstance(implType);

                    Cache.Add(key, helper);
                }

                return helper;
            }
        }

        public static void Invoke(Type type, IGenericInvoker invoker)
        {
            GetOrCreate(type).InternalInvoke(invoker);
        }

        public static void Invoke(object obj, IGenericInvoker invoker)
        {
            GetOrCreate(obj).InternalInvoke(invoker);
        }

        public static TResult Invoke<TResult>(Type type, IGenericInvoker<TResult> invoker)
        {
            return GetOrCreate(type).InternalInvoke(invoker);
        }

        public static TResult Invoke<TResult>(object obj, IGenericInvoker<TResult> invoker)
        {
            return GetOrCreate(obj).InternalInvoke(invoker);
        }
        
        public static bool IsEmptyValue(object obj)
        {
            return GetOrCreate(obj).InternalIsEmptyValue(obj);
        }

        public static int GetAlignedNumInstanceFieldBytes(Type type)
        {
            return GetNumInstanceFieldBytes(type) + (IntPtr.Size - 1) & (~(IntPtr.Size - 1));
        }

        public static int GetNumInstanceFieldBytes(Type type)
        {
            return GetOrCreate(type).InternalGetNumInstanceFieldBytes();
        }

        public static IntPtr GetStaticsBasePointer(Type type, StaticsBaseBlock statics_base_block)
        {
            return GetOrCreate(type).InternalGetStaticsBasePointer(statics_base_block);
        }

        public static unsafe int GetBaseSize(Type type)
        {
            if (VersionDifferences.UseInternalMethod)
            {
                return (int)((MethodTable*)type.TypeHandle.Value)->m_BaseSize;
            }

            throw new NotSupportedException();
        }

        public static unsafe byte GetBaseSizePadding(Type type)
        {
            if (VersionDifferences.UseInternalMethod)
            {
                return ((MethodTable*)type.TypeHandle.Value)->m_pEEClass->m_cbBaseSizePadding;
            }

            throw new NotSupportedException();
        }

        public static int SizeOf(Type type)
        {
            return GetOrCreate(type).InternalSize;
        }

        public static object GetDefaultValue(Type type)
        {
            return GetOrCreate(type).GetDefaultValue();
        }

        protected abstract void InternalInvoke(IGenericInvoker invoker);
        
        protected abstract bool InternalIsEmptyValue(object obj);

        protected abstract TResult InternalInvoke<TResult>(IGenericInvoker<TResult> invoker);

        protected abstract int InternalGetNumInstanceFieldBytes();

        protected abstract IntPtr InternalGetStaticsBasePointer(StaticsBaseBlock statics_base_block);

        protected abstract int InternalSize { get; }

        protected abstract object GetDefaultValue();

        private delegate void GetStaticsBasePointerFunc(
            out IntPtr gc_statics_base_pointer,
            out IntPtr non_gc_statics_base_pointer,
            out IntPtr thread_gc_statics_base_pointer,
            out IntPtr thread_non_gc_statics_base_pointer
            );

        private sealed class Impl<T> : GenericInvokerHelper
        {
            static GetStaticsBasePointerFunc get_statics_base_pointer_func;

            protected override int InternalGetNumInstanceFieldBytes()
            {
                var fields = TypeHelper.GetFields(typeof(T), 
                    BindingFlags.NonPublic |
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly);

                return fields.Sum(item => TypeHelper.SizeOf(item.FieldType));

                //if (fields.Length == 0)
                //{
                //    return 0;
                //}
                //else
                //{
                //    int right = fields.Length - 1;
                //    var lastFieldOffset = TypeHelper.OffsetOf(fields[right]);
                //    var lastFieldType = fields[right].FieldType;

                //    while (right > 0)
                //    {
                //        --right;

                //        var offset = TypeHelper.OffsetOf(fields[right]);

                //        if (offset > lastFieldOffset)
                //        {
                //            lastFieldOffset = offset;
                //            lastFieldType = fields[right].FieldType;
                //        }
                //    }


                //    var lastFieldSize = IntPtr.Size;

                //    if (lastFieldType.IsValueType)
                //    {
                //        lastFieldSize = TypeHelper.SizeOf(lastFieldType);
                //    }

                //    // = last field offset + last field size;
                //    return lastFieldOffset + lastFieldSize;
                //}
            }

            protected override IntPtr InternalGetStaticsBasePointer(StaticsBaseBlock statics_base_block)
            {
                if (get_statics_base_pointer_func == null)
                {
                    var fields = typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                    FieldInfo gc_static_field_info = null;
                    FieldInfo non_gc_static_field_info = null;
                    FieldInfo thread_gc_static_field_info = null;
                    FieldInfo thread_non_gc_static_field_info = null;

                    foreach (var item in fields)
                    {
                        if (item.IsLiteral)
                        {
                            continue;
                        }

                        switch (TypeHelper.GetStaticBaseBlock(item))
                        {
                            case StaticsBaseBlock.GC:
                                gc_static_field_info ??= item;
                                break;
                            case StaticsBaseBlock.NonGC:
                                non_gc_static_field_info ??= item;
                                break;
                            case StaticsBaseBlock.ThreadGC:
                                thread_gc_static_field_info ??= item;
                                break;
                            case StaticsBaseBlock.ThreadNonGC:
                                thread_non_gc_static_field_info ??= item;
                                break;
                        }
                    }

                    if (gc_static_field_info == null &&
                        non_gc_static_field_info == null &&
                        thread_gc_static_field_info == null &&
                        thread_non_gc_static_field_info == null)
                    {
                        return IntPtr.Zero;
                    }

                    get_statics_base_pointer_func = DynamicAssembly.BuildDynamicMethod<GetStaticsBasePointerFunc>((dym, ilGen) =>
                    {
                        if (gc_static_field_info != null)
                        {
                            ilGen.LoadArgument(0);
                            ilGen.LoadFieldAddress(gc_static_field_info);
                            ilGen.LoadConstant(TypeHelper.OffsetOf(gc_static_field_info));
                            ilGen.Subtract();
                            ilGen.StoreValue(typeof(IntPtr));
                        }

                        if (non_gc_static_field_info != null)
                        {
                            ilGen.LoadArgument(1);
                            ilGen.LoadFieldAddress(non_gc_static_field_info);
                            ilGen.LoadConstant(TypeHelper.OffsetOf(non_gc_static_field_info));
                            ilGen.Subtract();
                            ilGen.StoreValue(typeof(IntPtr));
                        }

                        if (thread_gc_static_field_info != null)
                        {
                            ilGen.LoadArgument(2);
                            ilGen.LoadFieldAddress(thread_gc_static_field_info);
                            ilGen.LoadConstant(TypeHelper.OffsetOf(thread_gc_static_field_info));
                            ilGen.Subtract();
                            ilGen.StoreValue(typeof(IntPtr));
                        }

                        if (thread_non_gc_static_field_info != null)
                        {
                            ilGen.LoadArgument(3);
                            ilGen.LoadFieldAddress(thread_non_gc_static_field_info);
                            ilGen.LoadConstant(TypeHelper.OffsetOf(thread_non_gc_static_field_info));
                            ilGen.Subtract();
                            ilGen.StoreValue(typeof(IntPtr));
                        }

                        ilGen.Return();

                    }, typeof(T).Module, true);
                }

                get_statics_base_pointer_func(
                    out var gc_statics_base_pointer,
                    out var non_gc_statics_base_pointer,
                    out var thread_gc_statics_base_pointer,
                    out var thread_non_gc_statics_base_pointer
                    );

                return statics_base_block switch
                {
                    StaticsBaseBlock.GC => gc_statics_base_pointer,
                    StaticsBaseBlock.NonGC => non_gc_statics_base_pointer,
                    StaticsBaseBlock.ThreadGC => thread_gc_statics_base_pointer,
                    StaticsBaseBlock.ThreadNonGC => thread_non_gc_statics_base_pointer,
                    _ => throw new NotSupportedException(),
                };
            }


            protected override void InternalInvoke(IGenericInvoker invoker)
            {
                invoker.Invoke<T>();
            }

            protected override TResult InternalInvoke<TResult>(IGenericInvoker<TResult> invoker)
            {
                return invoker.Invoke<T>();
            }
            
            protected override bool InternalIsEmptyValue(object obj)
            {
                return TypeHelper.IsEmptyValue(TypeHelper.Unbox<T>(obj));
            }

            protected override int InternalSize => Underlying.SizeOf<T>();

            protected override object GetDefaultValue() => default(T);
        }

        sealed class GetBaseSizeTest
        {
            public int Id = 0;
        }
    }
}