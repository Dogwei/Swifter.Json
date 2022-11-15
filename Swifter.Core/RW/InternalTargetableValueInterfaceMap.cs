using InlineIL;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Swifter.RW
{
    sealed class InternalTargetableValueInterfaceMap : Dictionary<IntPtr, object>, IGenericInvoker
    {
        ~InternalTargetableValueInterfaceMap()
        {
            Clear();
        }

        public void Set<T>(IValueInterface<T> valueInterface)
        {
            lock (this)
            {
                if (ContainsKey(TypeHelper.GetTypeHandle<T>()))
                {
                    base[TypeHelper.GetTypeHandle<T>()] = valueInterface;
                }
                else
                {
                    Add(TypeHelper.GetTypeHandle<T>(), valueInterface);

                    TargetableImplIValueInterface<T>.IncrementTargetCount();
                }
            }
        }

        public bool Remove<T>()
        {
            lock (this)
            {
                if (Remove(TypeHelper.GetTypeHandle<T>()))
                {
                    TargetableImplIValueInterface<T>.DecrementTargetCount();

                    return true;
                }

                return false;
            }
        }

        public bool TryGetValue<T>([MaybeNullWhen(false)] out IValueInterface<T> valueInterface)
        {
            IL.Push(this);
            IL.Push(TypeHelper.GetTypeHandle<T>());
            IL.PushOutRef(out valueInterface);

            IL.Emit.Call(MethodRef.Method(typeof(Dictionary<IntPtr, object>), nameof(base.TryGetValue)));

            return IL.Return<bool>();
        }

        public new void Clear()
        {
            lock (this)
            {
                // 我们认为，在已获取锁的上下文中调用 Clear 不会失败，所以此处不考虑意外情况。
                foreach (var item in this)
                {
                    foreach (var interfaceType in item.Value.GetType().GetInterfaces())
                    {
                        if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IValueInterface<>))
                        {
                            var valueType = interfaceType.GetGenericArguments()[0];

                            if (TypeHelper.GetTypeHandle(valueType) == item.Key)
                            {
                                TypeHelper.InvokeType(valueType, this);
                            }
                        }
                    }
                }

                base.Clear();
            }
        }

        public void Invoke<T>()
        {
            TargetableImplIValueInterface<T>.DecrementTargetCount();
        }
    }
}