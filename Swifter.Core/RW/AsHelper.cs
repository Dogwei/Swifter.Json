using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    internal abstract class AsHelper
    {
        static readonly AsCache ReaderCache = new AsCache(typeof(IDataReader), typeof(IDataReader<>));
        static readonly AsCache WriterCache = new AsCache(typeof(IDataWriter), typeof(IDataWriter<>));
        static readonly AsCache RWCache = new AsCache(typeof(IDataRW), typeof(IDataRW<>));

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static AsHelper GetInstance(IDataReader dataReader)
        {
            return ReaderCache.GetOrCreate(dataReader);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static AsHelper GetInstance(IDataWriter dataWriter)
        {
            return WriterCache.GetOrCreate(dataWriter);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static AsHelper GetInstance(IDataRW dataRW)
        {
            return RWCache.GetOrCreate(dataRW);
        }

        public abstract IDataReader<TOut> As<TOut>(IDataReader dataReader);

        public abstract IDataWriter<TOut> As<TOut>(IDataWriter dataWriter);

        public abstract IDataRW<TOut> As<TOut>(IDataRW dataRW);

        public abstract Type KeyType { get; }

        public abstract void Invoke(IGenericInvoker asInvoker);

        sealed class InternalAsHelper<T> : AsHelper
        {
            public override Type KeyType => typeof(T);

            public override IDataReader<TOut> As<TOut>(IDataReader dataReader)
            {
                return new AsDataReader<T, TOut>((IDataReader<T>)dataReader);
            }

            public override IDataWriter<TOut> As<TOut>(IDataWriter dataWriter)
            {
                return new AsDataWriter<T, TOut>((IDataWriter<T>)dataWriter);
            }

            public override IDataRW<TOut> As<TOut>(IDataRW dataRW)
            {
                return new AsDataRW<T, TOut>((IDataRW<T>)dataRW);
            }

            public override void Invoke(IGenericInvoker asInvoker)
            {
                asInvoker.Invoke<T>();
            }
        }

        sealed class AsCache : BaseCache<long, AsHelper>, BaseCache<long, AsHelper>.IGetOrCreate<Type>, BaseCache<long, AsHelper>.IGetOrCreate<object>
        {
            public readonly Type InterfaceType;
            public readonly Type GenericInterfaceType;

            public AsCache(Type interfaceType, Type genericInterfaceType) : base(0)
            {
                InterfaceType = interfaceType;
                GenericInterfaceType = genericInterfaceType;
            }

            public long AsKey(Type token)
            {
                return (long)TypeHelper.GetObjectTypeHandle(token);
            }

            public long AsKey(object token)
            {
                return (long)TypeHelper.GetObjectTypeHandle(token);
            }

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            public AsHelper AsValue(Type token)
            {
                var interfaces = token.GetInterfaces();

                foreach (var item in interfaces)
                {
                    if (
                    InterfaceType.IsAssignableFrom(item) &&
                    item.IsGenericType &&
                    item.GetGenericTypeDefinition() == GenericInterfaceType)
                    {
                        var genericType = item.GetGenericArguments();

                        var asHelperType = typeof(InternalAsHelper<>).MakeGenericType(genericType);

                        return (AsHelper)Activator.CreateInstance(asHelperType);
                    }
                }

                throw new ArgumentException($"This data reader does not implement '{GenericInterfaceType}' interface.", nameof(token));
            }

            public AsHelper AsValue(object token)
            {
                return AsValue(token.GetType());
            }

            public AsHelper GetOrCreate(Type token)
            {
                return GetOrCreate(this, token);
            }

            public AsHelper GetOrCreate(object token)
            {
                return GetOrCreate(this, token);
            }

            protected override int ComputeHashCode(long key)
            {
                return ((int)key) ^ ((int)(key >> 32));
            }

            [MethodImpl(VersionDifferences.AggressiveInlining)]
            protected override bool Equals(long key1, long key2)
            {
                return key1 == key2;
            }
        }
    }
}