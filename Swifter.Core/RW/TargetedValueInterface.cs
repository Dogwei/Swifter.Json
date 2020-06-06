
using Swifter.Tools;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    abstract class TargetedValueInterface
    {
        static readonly List<IImpl> Impls;

        static TargetedValueInterface()
        {
            Impls = new List<IImpl>();
        }

        public static void SetDefaultInterface<T>(IValueInterface<T> valueInterface)
        {
            Impl<T>.DefaultInterface = valueInterface;
        }

        public static void Set<T>(ITargetedBind targeted, IValueInterface<T> valueInterface)
        {
            if (valueInterface is null)
            {
                throw new ArgumentNullException(nameof(valueInterface));
            }

            lock (targeted)
            {
                targeted.MakeTargetedId();
            }

            if (!(ValueInterface<T>.Content is Impl<T>))
            {
                Impl<T>.DefaultInterface = ValueInterface<T>.Content;

                ValueInterface<T>.SetInterface(Impl<T>.Instance);
            }

            Impl<T>.Instance.Set(targeted, valueInterface);
        }

        public static IValueInterface<T> Get<T>(ITargetedBind targeted)
        {
            return Impl<T>.Instance.Get(targeted);
        }

        public static void Remove(ITargetedBind targeted)
        {
            lock (Impls)
            {
                foreach (var item in Impls)
                {
                    item.Remove(targeted);
                }
            }
        }

        interface IImpl
        {
            void Remove(ITargetedBind targeted);
        }

        sealed class Impl<T> : TargetedValueInterface, IValueInterface<T>, IImpl
        {
            public static readonly Impl<T> Instance;

            public static IValueInterface<T> DefaultInterface;

            public static Dictionary<long, IValueInterface<T>> Interfaces;

            static Impl()
            {
                Interfaces = new Dictionary<long, IValueInterface<T>>();

                Instance = new Impl<T>();

                lock (Impls)
                {
                    Impls.Add(Instance);
                }
            }

            Impl()
            {

            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public void Set(ITargetedBind targeted, IValueInterface<T> valueInterface)
            {
                var id = targeted.TargetedId;

                lock (this)
                {
                    Interfaces[id] = valueInterface;
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public new void Remove(ITargetedBind targeted)
            {
                var id = targeted.TargetedId;

                if (id != 0)
                {
                    lock (this)
                    {
                        Interfaces.Remove(id);
                    }
                }
            }

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public IValueInterface<T> Get(ITargetedBind targeted)
            {
                var interfaces = Interfaces;

                var id = targeted?.TargetedId ?? 0;

                if (id != 0 && interfaces.TryGetValue(id, out var value))
                {
                    return value;
                }

                return DefaultInterface;
            }

            public T ReadValue(IValueReader valueReader)
            {
                return Get(valueReader as ITargetedBind).ReadValue(valueReader);
            }

            public void WriteValue(IValueWriter valueWriter, T value)
            {
                Get(valueWriter as ITargetedBind).WriteValue(valueWriter, value);
            }
        }
    }
}