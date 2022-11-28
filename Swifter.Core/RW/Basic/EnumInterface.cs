using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    abstract class EnumInterface
    {
        static readonly Dictionary<IntPtr, EnumInterface> Instances = new();

        public static EnumInterface GetInstance<TEnum>() where TEnum : struct, Enum
        {
            return EnumInterface<TEnum>.Instance;
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static EnumInterface GetInstance(Type enumType)
        {
            if (Instances.TryGetValue(TypeHelper.GetTypeHandle(enumType), out var adapter))
            {
                return adapter;
            }

            return NoInliningGetInstance(enumType);

            [MethodImpl(MethodImplOptions.NoInlining)]
            static EnumInterface NoInliningGetInstance(Type enumType)
            {
                lock (Instances)
                {
                    if (!Instances.TryGetValue(TypeHelper.GetTypeHandle(enumType), out var adapter))
                    {
                        var adapterType = typeof(EnumInterface<>).MakeGenericType(enumType);

                        adapter = (EnumInterface)Activator.CreateInstance(adapterType)!;

                        Instances.Add(TypeHelper.GetTypeHandle(enumType), adapter);
                    }

                    return adapter;
                }
            }
        }



        public abstract Type EnumType { get; }

        public abstract Enum Box(ulong value);

        public abstract ulong UnBox(Enum value);

        public abstract ulong ReadEnum(IValueReader valueReader);

        public abstract void WriteEnum(IValueWriter valueWriter, ulong value);
    }

    sealed class EnumInterface<T> : EnumInterface, IValueInterface<T>, IDefaultBehaviorValueInterface where T : struct, Enum
    {
        public static readonly EnumInterface<T> Instance = new EnumInterface<T>();

        public override Type EnumType => typeof(T);

        public override Enum Box(ulong value)
        {
            return EnumHelper.AsEnum<T>(value);
        }

        public override ulong UnBox(Enum value)
        {
            return EnumHelper.AsUInt64((T)value);
        }

        public override ulong ReadEnum(IValueReader valueReader)
        {
            return EnumHelper.AsUInt64(valueReader.ReadEnum<T>());
        }

        public override void WriteEnum(IValueWriter valueWriter, ulong value)
        {
            valueWriter.WriteEnum(EnumHelper.AsEnum<T>(value));
        }

        public T ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadEnum<T>();
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            valueWriter.WriteEnum(value);
        }
    }
}