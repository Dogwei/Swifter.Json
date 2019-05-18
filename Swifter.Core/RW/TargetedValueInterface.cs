using Swifter.Readers;
using Swifter.Writers;
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
        
        public static void Set<T>(long id, IValueInterface<T> valueInterface)
        {
            if (!(ValueInterface<T>.Content is Impl<T>))
            {
                Impl<T>.DefaultInterface = ValueInterface<T>.Content;

                ValueInterface<T>.SetInterface(Impl<T>.Instance);
            }

            Impl<T>.Instance.Set(id, valueInterface);
        }
        
        public static void Remove(long id)
        {
            lock (Impls)
            {
                foreach (var item in Impls)
                {
                    item.Remove(id);
                }
            }
        }

        interface IImpl
        {
            void Remove(long id);
        }

        sealed class Impl<T> : TargetedValueInterface, IValueInterface<T>, IImpl
        {
            public static readonly Impl<T> Instance;

            public static IValueInterface<T> DefaultInterface;

            public static KeyValuePair<long, IValueInterface<T>>[] Interfaces;

            static Impl()
            {
                Interfaces = new KeyValuePair<long, IValueInterface<T>>[0];

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
            public void Set(long id, IValueInterface<T> valueInterface)
            {
                lock (this)
                {
                    var list = Interfaces.ToList();

                    list.RemoveAll(item => item.Key == id);

                    list.Add(new KeyValuePair<long, IValueInterface<T>>(id, valueInterface));

                    Interfaces = list.ToArray();
                }
            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public new void Remove(long id)
            {
                lock (this)
                {
                    for (int i = 0; i < Interfaces.Length; i++)
                    {
                        if (Interfaces[i].Key == id)
                        {
                            var list = Interfaces.ToList();

                            list.RemoveAt(i);

                            Interfaces = list.ToArray();

                            return;
                        }
                    }
                }
            }

            public T ReadValue(IValueReader valueReader)
            {
                if (valueReader is ITargetedBind targeted)
                {
                    var interfaces = Interfaces;

                    var id = targeted.Id;

                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        if (interfaces[i].Key == id)
                        {
                            return interfaces[i].Value.ReadValue(valueReader);
                        }
                    }
                }

                return DefaultInterface.ReadValue(valueReader);
            }

            public void WriteValue(IValueWriter valueWriter, T value)
            {
                if (valueWriter is ITargetedBind targeted)
                {
                    var interfaces = Interfaces;

                    var id = targeted.Id;

                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        if (interfaces[i].Key == id)
                        {
                            interfaces[i].Value.WriteValue(valueWriter, value);

                            return;
                        }
                    }
                }

                DefaultInterface.WriteValue(valueWriter, value);
            }
        }
    }
}