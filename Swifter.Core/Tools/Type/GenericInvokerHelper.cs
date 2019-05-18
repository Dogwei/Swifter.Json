using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Swifter.Tools
{
    internal abstract class GenericInvokerHelper
    {
        private static readonly Cache cache = new Cache();
        
        public static void Invoke(Type type, IGenericInvoker invoker)
        {
            cache.GetOrCreate(type).InternalInvoke(invoker);
        }

        public static void Invoke(object obj, IGenericInvoker invoker)
        {
            cache.GetOrCreate(obj).InternalInvoke(invoker);
        }

        public static TResult Invoke<TResult>(Type type, IGenericInvoker<TResult> invoker)
        {
            return cache.GetOrCreate(type).InternalInvoke(invoker);
        }

        public static TResult Invoke<TResult>(object obj, IGenericInvoker<TResult> invoker)
        {
            return cache.GetOrCreate(obj).InternalInvoke(invoker);
        }
        
        public static bool IsEmptyValue(object obj)
        {
            return cache.GetOrCreate(obj).InternalIsEmptyValue(obj);
        }

        public static int SizeOf(Type type)
        {
            return cache.GetOrCreate(type).InternalSize;
        }

        public static IntPtr GetTypeStaticMemoryAddress(Type type)
        {
            return cache.GetOrCreate(type).InternalTypeStaticMemoryAddress;
        }


        protected abstract void InternalInvoke(IGenericInvoker invoker);
        
        protected abstract bool InternalIsEmptyValue(object obj);

        protected abstract TResult InternalInvoke<TResult>(IGenericInvoker<TResult> invoker);

        protected abstract int InternalSize { get; }

        protected abstract IntPtr InternalTypeStaticMemoryAddress { get; }

        private sealed class Impl<T> : GenericInvokerHelper
        {
            public static int GetSize()
            {
                if (typeof(T).IsValueType)
                {
                    return Unsafe.SizeOf<T>();
                }

                var fields = typeof(T).GetFields(BindingFlags.NonPublic |
                    BindingFlags.Public |
                    BindingFlags.Instance |
                    BindingFlags.DeclaredOnly);

                if (fields.Length == 0)
                {
                    return 0;
                }
                else
                {
                    int right = fields.Length - 1;
                    var lastFieldOffset = TypeHelper.OffsetOf(fields[right]);
                    var lastFieldType = fields[right].FieldType;

                    while (right > 0)
                    {
                        --right;

                        var offset = TypeHelper.OffsetOf(fields[right]);

                        if (offset > lastFieldOffset)
                        {
                            lastFieldOffset = offset;
                            lastFieldType = fields[right].FieldType;
                        }
                    }


                    var lastFieldSize = IntPtr.Size;

                    if (lastFieldType.IsValueType)
                    {
                        lastFieldSize= SizeOf(lastFieldType);
                    }

                    // = last field offset + last field size;
                    return lastFieldOffset + lastFieldSize;
                }
            }

            public static IntPtr GetTypeStaticMemoryAddress()
            {
                var fields = typeof(T).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

                FieldInfo fieldInfo = null;

                foreach (var item in fields)
                {
                    if (item.IsLiteral)
                    {
                        continue;
                    }

                    fieldInfo = item;

                    break;
                }

                if (fieldInfo == null)
                {
                    return IntPtr.Zero;
                }

                var getFieldPointerMethod = new DynamicMethod(
                    $"{nameof(GetTypeStaticMemoryAddress)}_{typeof(T).Name}_{Guid.NewGuid().ToString("N")}",
                    typeof(IntPtr),
                    Type.EmptyTypes,
                    fieldInfo.Module,
                    true);

                var ilGen = getFieldPointerMethod.GetILGenerator();

                ilGen.LoadFieldAddress(fieldInfo);
                ilGen.Return();

                var pAddress = (IntPtr)getFieldPointerMethod.Invoke(null, null);

                var pResult = (long)pAddress - TypeHelper.OffsetOf(fieldInfo);

                return (IntPtr)pResult;
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

            protected override int InternalSize { get; } = GetSize();

            protected override IntPtr InternalTypeStaticMemoryAddress { get; } = GetTypeStaticMemoryAddress();
        }

        private sealed class Cache : BaseCache<IntPtr, GenericInvokerHelper>,
            BaseCache<IntPtr, GenericInvokerHelper>.IGetOrCreate<Type>,
            BaseCache<IntPtr, GenericInvokerHelper>.IGetOrCreate<object>
        {
            public Cache() : base(0)
            {
            }

            public IntPtr AsKey(Type token) => token.TypeHandle.Value;

            public IntPtr AsKey(object token) => Unsafe.GetObjectTypeHandle(token);

            public GenericInvokerHelper AsValue(Type token) => (GenericInvokerHelper)Activator.CreateInstance(typeof(Impl<>).MakeGenericType(token));

            public GenericInvokerHelper AsValue(object token) => AsValue(token.GetType());

            public GenericInvokerHelper GetOrCreate(Type token) => GetOrCreate(this, token);

            public GenericInvokerHelper GetOrCreate(object token) => GetOrCreate(this, token);

            protected override int ComputeHashCode(IntPtr key) => key.GetHashCode();

            protected override bool Equals(IntPtr key1, IntPtr key2) => key1 == key2;
        }
    }
}