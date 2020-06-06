
using Swifter.Tools;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    internal abstract class AsHelper
    {
        static readonly Dictionary<IntPtr, AsHelperGroup> Cache = new Dictionary<IntPtr, AsHelperGroup>();

        sealed class AsHelperGroup
        {
            public readonly AsHelper Reader;
            public readonly AsHelper Writer;
            public readonly AsHelper RW;

            public AsHelperGroup(Type type)
            {
                foreach (var @interface in type.GetInterfaces())
                {
                    if (@interface.IsGenericType)
                    {
                        // Reader
                        if (typeof(IDataReader).IsAssignableFrom(@interface) && @interface.GetGenericTypeDefinition() == typeof(IDataReader<>))
                        {
                            Store(ref Reader, @interface);
                        }

                        // Writer
                        if (typeof(IDataWriter).IsAssignableFrom(@interface) && @interface.GetGenericTypeDefinition() == typeof(IDataWriter<>))
                        {
                            Store(ref Writer, @interface);
                        }

                        // RW
                        if (typeof(IDataRW).IsAssignableFrom(@interface) && @interface.GetGenericTypeDefinition() == typeof(IDataRW<>))
                        {
                            Store(ref RW, @interface);
                        }
                    }
                }
            }

            public static void Store(ref AsHelper value, Type @interface)
            {
                var genericTypes = @interface.GetGenericArguments();

                var asHelperImplType = typeof(AsHelperImpl<>).MakeGenericType(genericTypes);

                value = (AsHelper)Activator.CreateInstance(asHelperImplType);
            }
        }

        sealed class AsHelperImpl<TKey> : AsHelper
        {
            public override Type KeyType => typeof(TKey);

            public override IDataReader<TOut> As<TOut>(IDataReader dataReader)
            {
                return new AsDataReader<TKey, TOut>((IDataReader<TKey>)dataReader);
            }

            public override IDataWriter<TOut> As<TOut>(IDataWriter dataWriter)
            {
                return new AsDataWriter<TKey, TOut>((IDataWriter<TKey>)dataWriter);
            }

            public override IDataRW<TOut> As<TOut>(IDataRW dataRW)
            {
                return new AsDataRW<TKey, TOut>((IDataRW<TKey>)dataRW);
            }

            public override void Invoke(IGenericInvoker asInvoker)
            {
                asInvoker.Invoke<TKey>();
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        static AsHelperGroup GetOrCreateGroup(object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (Cache.TryGetValue(Underlying.GetMethodTablePointer(obj), out var group))
            {
                return group;
            }

            return CreateGroup(obj);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static AsHelperGroup CreateGroup(object obj)
        {
            lock (Cache)
            {
                var mtp = Underlying.GetMethodTablePointer(obj);

                if (Cache.TryGetValue(mtp, out var group))
                {
                    return group;
                }

                group = new AsHelperGroup(obj.GetType());

                Cache.Add(mtp, group);

                return group;
            }
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static AsHelper GetInstance(IDataReader dataReader) => GetOrCreateGroup(dataReader).Reader;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static AsHelper GetInstance(IDataWriter dataWriter) => GetOrCreateGroup(dataWriter).Writer;

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static AsHelper GetInstance(IDataRW dataRW) => GetOrCreateGroup(dataRW).RW;

        public abstract IDataReader<TOut> As<TOut>(IDataReader dataReader);

        public abstract IDataWriter<TOut> As<TOut>(IDataWriter dataWriter);

        public abstract IDataRW<TOut> As<TOut>(IDataRW dataRW);

        public abstract Type KeyType { get; }

        public abstract void Invoke(IGenericInvoker asInvoker);
    }
}