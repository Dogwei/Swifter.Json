using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// ValueInterface&lt;T&gt; 提供在 ValueReader 中读取指定类型的值或在 ValueWriter 中写入指定类型的值。
    /// 此类型提供泛型方法，效率更高。
    /// <typeparam name="T">值的类型</typeparam>
    /// </summary>
    public sealed class ValueInterface<T> : ValueInterface
    {
        private static object ContentLock;
        /// <summary>
        /// 此类型的具体读写方法实现。
        /// </summary>
        internal static IValueInterface<T> Content;

        /// <summary>
        /// 表示是否使用用户自定义的读写方法，如果为 True, FastObjectRW 将不优化基础类型的读写。
        /// </summary>
        
        internal static bool IsNoModify { get; private set; }

        static ValueInterface()
        {
            if (!StaticConstructorInvoked)
            {
                GetInterface(typeof(int));
            }

            try
            {
                RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
            }
            catch (Exception)
            {
            }

            IsNoModify = true;

            if (Content != null)
            {
                return;
            }

            if (StaticConstructorInvoking)
            {
                return;
            }

            IValueInterface<T> valueInterface;

            for (int i = Mapers.Count - 1; i >= 0; --i)
            {
                valueInterface = Mapers[i].TryMap<T>();

                if (valueInterface != null)
                {
                    Content = valueInterface;

                    return;
                }
            }

            var type = typeof(T);

            if (typeof(Type).IsAssignableFrom(type))
            {
                Content = (IValueInterface<T>)Activator.CreateInstance((typeof(TypeInfoInterface<>)).MakeGenericType(type));

                return;
            }

            if (typeof(System.Reflection.Assembly).IsAssignableFrom(type))
            {
                Content = (IValueInterface<T>)Activator.CreateInstance((typeof(AssemblyInterface<>)).MakeGenericType(type));

                return;
            }

            if (typeof(IDataReader).IsAssignableFrom(type))
            {
                foreach (var item in type.GetInterfaces())
                {
                    if (item.IsGenericType && item.GetGenericTypeDefinition() == typeof(IDataReader<>))
                    {
                        var keyType = item.GetGenericArguments()[0];

                        Content = (IValueInterface<T>)Activator.CreateInstance((typeof(DataReaderInterface<,>)).MakeGenericType(type, keyType));

                        return;
                    }
                }
            }

            if (typeof(T).IsEnum)
            {
                Content = (IValueInterface<T>)Activator.CreateInstance(typeof(EnumInterface<>).MakeGenericType(type));

                return;
            }

            if (type.IsArray)
            {
                Content = (IValueInterface<T>)Activator.CreateInstance(defaultArrayInterfaceType.MakeGenericType(type));

                return;
            }

            if (type.IsValueType && type.IsGenericType)
            {
                var underlyingType = Nullable.GetUnderlyingType(typeof(T));

                if (underlyingType != null && !ReferenceEquals(type, underlyingType))
                {
                    Content = (IValueInterface<T>)Activator.CreateInstance(nullableInterfaceType.MakeGenericType(underlyingType));

                    return;
                }
            }

            if (type.IsInterface || type.IsAbstract || typeof(IFormattable).IsAssignableFrom(type))
            {
                Content = (IValueInterface<T>)Activator.CreateInstance(unknowInterfaceType.MakeGenericType(type));

                return;
            }

            Content = (IValueInterface<T>)Activator.CreateInstance(defaultObjectInterfaceType.MakeGenericType(type));
        }

        
        /// <summary>
        /// 往写入器中写入该类型值的方法。
        /// </summary>
        /// <param name="value">T 类型的值</param>
        /// <param name="valueWriter">值写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void WriteValue(IValueWriter valueWriter, T value)
        {
            Content.WriteValue(valueWriter, value);
        }

        /// <summary>
        /// 在读取器中读取该类型值的方法。
        /// </summary>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回该类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T ReadValue(IValueReader valueReader)
        {
            return Content.ReadValue(valueReader);
        }

        /// <summary>
        /// 设置该类型的值读写接口实例。
        /// </summary>
        /// <param name="valueInterface">值读写接口实例</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetInterface(IValueInterface<T> valueInterface)
        {
            if (ContentLock == null)
            {
                lock (MapersLock)
                {
                    if (ContentLock == null)
                    {
                        ContentLock = new object();
                    }
                }
            }

            lock (ContentLock)
            {
                valueInterface = valueInterface ?? throw new ArgumentNullException(nameof(valueInterface));

                IsNoModify = false;

                if (Content is TargetedValueInterface)
                {
                    TargetedValueInterface.SetDefaultInterface(valueInterface);

                    return;
                }

                Content = valueInterface;
            }
        }

        /// <summary>
        /// 设置针对某一目标值读写器的读写接口实例。
        /// </summary>
        /// <param name="targetedId">针对目标的 Id</param>
        /// <param name="valueInterface">读写接口实例</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetTargetedInterface(long targetedId, IValueInterface<T> valueInterface)
        {
            TargetedValueInterface.Set(targetedId, valueInterface);
        }

        /// <summary>
        /// 非泛型读取值方法。
        /// </summary>
        /// <param name="valueReader">值读取器。</param>
        /// <returns>返回一个 T 类型的值。</returns>
        public override object Read(IValueReader valueReader)
        {
            return Content.ReadValue(valueReader);
        }

        /// <summary>
        /// 非泛型写入值方法。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="value">T 类型的值</param>
        public override void Write(IValueWriter valueWriter, object value)
        {
            Content.WriteValue(valueWriter, (T)value);
        }
    }

    /// <summary>
    /// ValueInterface 提供在 ValueReader 中读取指定类型的值或在 ValueWriter 中写入指定类型的值。
    /// 此类提供非泛型方法。
    /// </summary>
    public abstract class ValueInterface
    {
        internal static readonly List<IValueInterfaceMaper> Mapers;
        internal static readonly object MapersLock;
        internal static readonly bool StaticConstructorInvoked = false;
        internal static readonly bool StaticConstructorInvoking = false;
        internal static readonly ValueInterfaceCache TypedCache;

        internal static Type defaultObjectInterfaceType;
        internal static Type defaultArrayInterfaceType;
        internal static Type nullableInterfaceType;
        internal static Type unknowInterfaceType;

        /// <summary>
        /// 获取或设置默认的对象类型读写接口类型。
        /// 如果要设置此类型要满足以下条件
        /// 1: 类型必须是可以实例化并且具有公开的无参构造函数。
        /// 2: 必须继承 IValueInterface/<T/> 接口。
        /// 3: 必须是泛型类型，有且只有一个泛型参数，泛型参数与 IValueInterface/<T/> 的泛型参数对应。
        /// </summary>
        public static Type DefaultObjectInterfaceType
        {
            get
            {
                return defaultObjectInterfaceType;
            }
            set
            {
                if (value.IsInterface)
                {
                    throw new ArgumentException("Type can't be a interface.", nameof(DefaultObjectInterfaceType));
                }

                if (value.IsAbstract)
                {
                    throw new ArgumentException("Type can't be a abstract class.", nameof(DefaultObjectInterfaceType));
                }

                if (!value.ContainsGenericParameters || value.GetGenericArguments().Length != 1)
                {
                    throw new ArgumentException("Type must only one generic type.", nameof(DefaultObjectInterfaceType));
                }

                if (!typeof(IValueInterface<TestClass>).IsAssignableFrom(value.MakeGenericType(typeof(TestClass))))
                {
                    throw new ArgumentException("Type must extend IValueInterface<T>.", nameof(DefaultObjectInterfaceType));
                }

                defaultObjectInterfaceType = value;
            }
        }

        /// <summary>
        /// 获取或设置默认的数组类型读写接口类型。
        /// 如果要设置此类型要满足以下条件
        /// 1: 类型必须是可以实例化并且具有公开的无参构造函数。
        /// 2: 必须继承 IValueInterface/<T/> 接口。
        /// 3: 必须是泛型类型，有且只有一个泛型参数，泛型参数与 IValueInterface/<T/> 的泛型参数对应。
        /// </summary>
        public static Type DefaultArrayInterfaceType
        {
            get
            {
                return defaultArrayInterfaceType;
            }
            set
            {
                if (value.IsInterface)
                {
                    throw new ArgumentException("Type can't be a interface.", nameof(DefaultArrayInterfaceType));
                }

                if (value.IsAbstract)
                {
                    throw new ArgumentException("Type can't be a abstract class.", nameof(DefaultArrayInterfaceType));
                }

                if (!value.ContainsGenericParameters || value.GetGenericArguments().Length != 1)
                {
                    throw new ArgumentException("Type must only one generic type.", nameof(DefaultArrayInterfaceType));
                }

                if (!typeof(IValueInterface<TestClass[]>).IsAssignableFrom(value.MakeGenericType(typeof(TestClass[]))))
                {
                    throw new ArgumentException("Type must extend IValueInterface<T>.", nameof(DefaultArrayInterfaceType));
                }

                defaultArrayInterfaceType = value;
            }
        }

        private class TestClass
        {

        }

        internal sealed class ValueInterfaceCache : BaseCache<IntPtr, ValueInterface>, BaseCache<IntPtr, ValueInterface>.IGetOrCreate<object>, BaseCache<IntPtr, ValueInterface>.IGetOrCreate<Type>
        {
            public ValueInterfaceCache() : base(0)
            {
            }

            public IntPtr AsKey(object token) => Unsafe.GetObjectTypeHandle(token);

            public IntPtr AsKey(Type token) => token.TypeHandle.Value;

            public ValueInterface AsValue(object token) => AsValue(token.GetType());

            public ValueInterface AsValue(Type token)=> (ValueInterface)Activator.CreateInstance(typeof(ValueInterface<>).MakeGenericType(token));

            public ValueInterface GetOrCreate(object token) => GetOrCreate(this, token);

            public ValueInterface GetOrCreate(Type token) => GetOrCreate(this, token);

            protected override int ComputeHashCode(IntPtr key) => key.GetHashCode();

            protected override bool Equals(IntPtr key1, IntPtr key2) => key1 == key2;
        }


        static ValueInterface()
        {
            StaticConstructorInvoked = true;
            StaticConstructorInvoking = true;

            Mapers = new List<IValueInterfaceMaper>();
            MapersLock = new object();

            DefaultObjectInterfaceType = typeof(FastObjectInterface<>);
            DefaultArrayInterfaceType = typeof(ArrayInterface<>);
            nullableInterfaceType = typeof(NullableInterface<>);
            unknowInterfaceType = typeof(UnknowTypeInterface<>);

            TypedCache = new ValueInterfaceCache();

            ValueInterface<bool>.Content = new BooleanInterface();
            ValueInterface<sbyte>.Content = new SByteInterface();
            ValueInterface<short>.Content = new Int16Interface();
            ValueInterface<int>.Content = new Int32Interface();
            ValueInterface<long>.Content = new Int64Interface();
            ValueInterface<byte>.Content = new ByteInterface();
            ValueInterface<ushort>.Content = new UInt16Interface();
            ValueInterface<uint>.Content = new UInt32Interface();
            ValueInterface<ulong>.Content = new UInt64Interface();
            ValueInterface<char>.Content = new CharInterface();
            ValueInterface<float>.Content = new SingleInterface();
            ValueInterface<double>.Content = new DoubleInterface();
            ValueInterface<decimal>.Content = new DecimalInterface();
            ValueInterface<string>.Content = new StringInterface();
            ValueInterface<object>.Content = new ObjectInterface();
            ValueInterface<DateTime>.Content = new DateTimeInterface();
            ValueInterface<DateTimeOffset>.Content = new DateTimeOffsetInterface();
            ValueInterface<TimeSpan>.Content = new TimeSpanInterface();
            ValueInterface<IntPtr>.Content = new IntPtrInterface();
            ValueInterface<Version>.Content = new VersionInterface();
            ValueInterface<DBNull>.Content = new DbNullInterface();

            Mapers.Add(new EnumerableInterfaceMaper());
            Mapers.Add(new CollectionInterfaceMaper());
            Mapers.Add(new DictionaryInterfaceMaper());
            Mapers.Add(new ListInterfaceMaper());
            Mapers.Add(new TableRWInternalMaper());

            StaticConstructorInvoking = false;
        }

        /// <summary>
        /// 添加一个类型与 ValueInterface 的匹配器。
        /// 此匹配器可以自定义类型的读写方法。
        /// 后加入的匹配器优先级高。
        /// </summary>
        /// <param name="maper">类型与 ValueInterface 的匹配器</param>
        public static void AddMaper(IValueInterfaceMaper maper)
        {
            if (maper == null)
            {
                throw new ArgumentNullException(nameof(maper));
            }

            lock (MapersLock)
            {
                Mapers.Add(maper);
            }
        }

        /// <summary>
        /// 非泛型方式获取指定类型的 ValueInterface ，此方式效率并不高。
        /// 如果是已知类型，请考虑使用泛型方式 ValueInterface&lt;TYPE&gt; 获取。
        /// 如果是未知类型的实例，请考虑使用 ValueInterface.GetInterface(object) 获取。
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns>返回一个 ValueInterface 实例。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ValueInterface GetInterface(Type type) => TypedCache.GetOrCreate(type);

        /// <summary>
        /// 非泛型方式获取实例的类型的 ValueInterface ，此方式效率比 ValueInterface.GetInterface(Type) 高，但比 ValueInterface&lt;T&gt;.Content 低。
        /// </summary>
        /// <param name="obj">指定一个实例，此实例不能为 Null。</param>
        /// <returns>返回一个 ValueInterface 实例。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ValueInterface GetInterface(object obj) => TypedCache.GetOrCreate(obj);

        /// <summary>
        /// 往写入器中写入一个未知类型值的方法。
        /// </summary>
        /// <param name="valueWriter">写入器</param>
        /// <param name="value">一个对象值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void WriteValue(IValueWriter valueWriter, object value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);
            }
            else
            {
                GetInterface(value).Write(valueWriter, value);
            }
        }

        /// <summary>
        /// 在读取器中读取指定类型值的方法。
        /// </summary>
        /// <param name="valueReader">读取器</param>
        /// <param name="type">指定类型</param>
        /// <returns>返回一个对象值</returns>
        public static object ReadValue(IValueReader valueReader, Type type)
        {
            return GetInterface(type).Read(valueReader);
        }

        /// <summary>
        /// 在读取器中读取指定类型值的方法。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回该类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T ReadValue<T>(IValueReader valueReader)
        {
            return ValueInterface<T>.Content.ReadValue(valueReader);
        }

        /// <summary>
        /// 往写入器中写入指定类型值的方法。
        /// </summary>
        /// <typeparam name="T">值的类型</typeparam>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="value">值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void WriteValue<T>(IValueWriter valueWriter, T value)
        {
            ValueInterface<T>.Content.WriteValue(valueWriter, value);
        }

        /// <summary>
        /// 移除针对某一目标读写器的读写接口实例。
        /// </summary>
        /// <param name="targetedId">目标 Id</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RemoveTargetedInterface(long targetedId)
        {
            TargetedValueInterface.Remove(targetedId);
        }

        /// <summary>
        /// 在 IValueReader 中读取该类型的值。
        /// </summary>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回该类型的值</returns>
        public abstract object Read(IValueReader valueReader);

        /// <summary>
        /// 在 IValueWriter 中写入该类型的值。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="value">该类型的值</param>
        public abstract void Write(IValueWriter valueWriter, object value);
    }

    /// <summary>
    /// 提供某一类型在 IValueReader 中读取值和在 IValueWriter 写入值的方法。
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    public interface IValueInterface<T>
    {
        /// <summary>
        /// 在 IValueReader 中读取该类型的值。
        /// </summary>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回该类型的值</returns>
        T ReadValue(IValueReader valueReader);

        /// <summary>
        /// 在 IValueWriter 中写入该类型的值。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="value">该类型的值</param>
        void WriteValue(IValueWriter valueWriter, T value);
    }

    /// <summary>
    /// 提供类型与 IValueInterface 的匹配器。
    /// 实现它，并使用 ValueInterface.AddMaper 添加它的实例即可自定义类型的读写方法。
    /// </summary>
    public interface IValueInterfaceMaper
    {
        /// <summary>
        /// 类型与 IValueInterface 的匹配方法。
        /// 匹配成功则返回实例，不成功则返回 Null。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <returns>返回一个 IValueInterface/<T/> 实例</returns>
        IValueInterface<T> TryMap<T>();
    }

    internal sealed class BooleanInterface : IValueInterface<bool>
    {
        public bool ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadBoolean();
        }

        public void WriteValue(IValueWriter valueWriter, bool value)
        {
            valueWriter.WriteBoolean(value);
        }
    }

    internal sealed class SByteInterface : IValueInterface<sbyte>
    {
        public sbyte ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadSByte();
        }

        public void WriteValue(IValueWriter valueWriter, sbyte value)
        {
            valueWriter.WriteSByte(value);
        }
    }

    internal sealed class Int16Interface : IValueInterface<short>
    {
        public short ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadInt16();
        }

        public void WriteValue(IValueWriter valueWriter, short value)
        {
            valueWriter.WriteInt16(value);
        }
    }

    internal sealed class Int32Interface : IValueInterface<int>
    {
        public int ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadInt32();
        }

        public void WriteValue(IValueWriter valueWriter, int value)
        {
            valueWriter.WriteInt32(value);
        }
    }

    internal sealed class Int64Interface : IValueInterface<long>
    {
        public long ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadInt64();
        }

        public void WriteValue(IValueWriter valueWriter, long value)
        {
            valueWriter.WriteInt64(value);
        }
    }

    internal sealed class ByteInterface : IValueInterface<byte>
    {
        public byte ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadByte();
        }

        public void WriteValue(IValueWriter valueWriter, byte value)
        {
            valueWriter.WriteByte(value);
        }
    }

    internal sealed class UInt16Interface : IValueInterface<ushort>
    {
        public ushort ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadUInt16();
        }

        public void WriteValue(IValueWriter valueWriter, ushort value)
        {
            valueWriter.WriteUInt16(value);
        }
    }

    internal sealed class UInt32Interface : IValueInterface<uint>
    {
        public uint ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadUInt32();
        }

        public void WriteValue(IValueWriter valueWriter, uint value)
        {
            valueWriter.WriteUInt32(value);
        }
    }

    internal sealed class UInt64Interface : IValueInterface<ulong>
    {
        public ulong ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadUInt64();
        }

        public void WriteValue(IValueWriter valueWriter, ulong value)
        {
            valueWriter.WriteUInt64(value);
        }
    }

    internal sealed class CharInterface : IValueInterface<char>
    {
        public char ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadChar();
        }

        public void WriteValue(IValueWriter valueWriter, char value)
        {
            valueWriter.WriteChar(value);
        }
    }

    internal sealed class SingleInterface : IValueInterface<float>
    {
        public float ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadSingle();
        }

        public void WriteValue(IValueWriter valueWriter, float value)
        {
            valueWriter.WriteSingle(value);
        }
    }

    internal sealed class DoubleInterface : IValueInterface<double>
    {
        public double ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadDouble();
        }

        public void WriteValue(IValueWriter valueWriter, double value)
        {
            valueWriter.WriteDouble(value);
        }
    }

    internal sealed class DecimalInterface : IValueInterface<decimal>
    {
        public decimal ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadDecimal();
        }

        public void WriteValue(IValueWriter valueWriter, decimal value)
        {
            valueWriter.WriteDecimal(value);
        }
    }

    internal sealed class StringInterface : IValueInterface<string>
    {
        public string ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadString();
        }

        public void WriteValue(IValueWriter valueWriter, string value)
        {
            valueWriter.WriteString(value);
        }
    }

    internal sealed class DateTimeInterface : IValueInterface<DateTime>
    {
        public DateTime ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadDateTime();
        }

        public void WriteValue(IValueWriter valueWriter, DateTime value)
        {
            valueWriter.WriteDateTime(value);
        }
    }

    internal sealed class IntPtrInterface : IValueInterface<IntPtr>
    {
        public IntPtr ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<IntPtr> intPtrReader)
            {
                return intPtrReader.ReadValue();
            }

            var value = valueReader.ReadNullable<long>();

            if (value == null)
            {
                return IntPtr.Zero;
            }

            return (IntPtr)value.Value;
        }

        public void WriteValue(IValueWriter valueWriter, IntPtr value)
        {
            if (valueWriter is IValueWriter<IntPtr> intPtrWriter)
            {
                intPtrWriter.WriteValue(value);
            }
            else
            {
                valueWriter.WriteInt64((long)value);
            }
        }
    }

    internal sealed class VersionInterface : IValueInterface<Version>
    {
        public Version ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<Version> versionReader)
            {
                return versionReader.ReadValue();
            }

            var versionText = valueReader.ReadString();

            if (versionText == null)
            {
                return null;
            }

            return new Version(versionText);
        }

        public void WriteValue(IValueWriter valueWriter, Version value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            if (valueWriter is IValueWriter<Version> versionReader)
            {
                versionReader.WriteValue(value);
            }
            else
            {
                valueWriter.WriteString(value.ToString());
            }
        }
    }

    internal sealed class ObjectInterface : IValueInterface<object>
    {
        public object ReadValue(IValueReader valueReader)
        {
            return valueReader.DirectRead();
        }

        public void WriteValue(IValueWriter valueWriter, object value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            if (valueWriter is IValueWriter<object> writer)
            {
                writer.WriteValue(value);

                return;
            }

            /* 父类引用，子类实例时使用 Type 获取写入器。 */

            if (TypeInfo<object>.Int64TypeHandle != (long)TypeHelper.GetTypeHandle(value))
            {
                ValueInterface.GetInterface(value).Write(valueWriter, value);

                return;
            }

            valueWriter.DirectWrite(value);
        }
    }

    internal sealed class UnknowTypeInterface<T> : IValueInterface<T>
    {
        static readonly long Int64TypeHandle = TypeInfo<T>.Int64TypeHandle;
        
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T>)
            {
                return ((IValueReader<T>)valueReader).ReadValue();
            }

            var directValue = valueReader.DirectRead();

            if (directValue is T || directValue == null)
            {
                return (T)directValue;
            }

            return XConvert.FromObject<T>(directValue);
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            if (valueWriter is IValueWriter<T> writer)
            {
                writer.WriteValue(value);

                return;
            }

            if (value is IFormattable)
            {
                valueWriter.DirectWrite(value);

                return;
            }

            if (value is string)
            {
                valueWriter.WriteString((string)(object)value);

                return;
            }

            /* 父类引用，子类实例时使用 Type 获取写入器。 */
            if (Int64TypeHandle != (long)TypeHelper.GetTypeHandle(value))
            {
                ValueInterface.GetInterface(value).Write(valueWriter, value);

                return;
            }

            valueWriter.DirectWrite(value);
        }
    }

    internal sealed class DateTimeOffsetInterface : IValueInterface<DateTimeOffset>
    {
        public DateTimeOffset ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<DateTimeOffset> dateTimeOffsetReader)
            {
                return dateTimeOffsetReader.ReadValue();
            }

            object directValue = valueReader.DirectRead();

            if (directValue is DateTimeOffset)
            {
                return (DateTimeOffset)directValue;
            }

            if (directValue is string)
            {
                return DateTimeOffset.Parse((string)directValue);
            }

            return XConvert.FromObject<DateTimeOffset>(directValue);
        }

        public void WriteValue(IValueWriter valueWriter, DateTimeOffset value)
        {
            if (valueWriter is IValueWriter<DateTimeOffset> dateTimeOffsetWriter)
            {
                dateTimeOffsetWriter.WriteValue(value);

                return;
            }

            valueWriter.DirectWrite(value);
        }
    }

    internal sealed class TimeSpanInterface : IValueInterface<TimeSpan>
    {
        public TimeSpan ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<TimeSpan> timeSpanReader)
            {
                return timeSpanReader.ReadValue();
            }

            object directValue = valueReader.DirectRead();

            if (directValue is TimeSpan)
            {
                return (TimeSpan)directValue;
            }

            if (directValue is string)
            {
                return TimeSpan.Parse((string)directValue);
            }

            return XConvert.FromObject<TimeSpan>(directValue);
        }

        public void WriteValue(IValueWriter valueWriter, TimeSpan value)
        {
            if (valueWriter is IValueWriter<TimeSpan> timeSpanWriter)
            {
                timeSpanWriter.WriteValue(value);

                return;
            }

            valueWriter.DirectWrite(value);
        }
    }

    internal sealed class NullableInterface<T> : IValueInterface<T?> where T : struct
    {
        public T? ReadValue(IValueReader valueReader)
        {
            return valueReader.ReadNullable<T>();
        }

        public void WriteValue(IValueWriter valueWriter, T? value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);
            }
            else
            {
                ValueInterface<T>.WriteValue(valueWriter, value.Value);
            }
        }
    }

    internal sealed class DataReaderInterface<T, TKey> : IValueInterface<T> where T : IDataReader<TKey>
    {
        static readonly bool IsArray;

        static DataReaderInterface()
        {
            var tKey = typeof(TKey);

            switch (Type.GetTypeCode(tKey))
            {
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    IsArray = true && !tKey.IsEnum;
                    break;
            }
        }

        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }

            if (valueReader is IValueFiller<TKey> tFiller &&
                typeof(IDataWriter<TKey>).IsAssignableFrom(typeof(T)))
            {
                T instance = default;

                try
                {
                    instance = Activator.CreateInstance<T>(); ;
                }
                catch (Exception)
                {
                    goto Direct;
                }

                tFiller.FillValue((IDataWriter<TKey>)instance);

                return instance;
            }

        Direct:

            var value = valueReader.DirectRead();

            if (value is T tValue)
            {
                return tValue;
            }

            var reader = RWHelper.CreateReader(value);

            if (reader is T tResult)
            {
                return tResult;
            }

            throw new NotSupportedException($"Cannot read a '{typeof(T).Name}', It is a data {"reader"}.");
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (valueWriter is IValueWriter<T> tWriter)
            {
                tWriter.WriteValue(value);

                return;
            }

            if (valueWriter is IValueWriter<IDataReader> iWriter)
            {
                iWriter.WriteValue(value);

                return;
            }

            if (IsArray)
            {
                valueWriter.WriteArray(value.As<int>());
            }
            else
            {
                valueWriter.WriteObject(value.As<string>());
            }
        }
    }

    internal sealed class TypeInfoInterface<T> : IValueInterface<T> where T : Type
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }

            if (valueReader is IValueReader<Type> typeReader)
            {
                return (T)typeReader.ReadValue();
            }

            var value = valueReader.DirectRead();

            if (value == null)
            {
                return null;
            }

            if (value is T tValue)
            {
                return tValue;
            }

            if (value is string sValue)
            {
                return (T)Type.GetType(sValue);
            }

            throw new NotSupportedException($"Cannot Read a 'TypeInfo' by '{value}'.");
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            if (valueWriter is IValueWriter<T> tWriter)
            {
                tWriter.WriteValue(value);

                return;
            }

            if (valueWriter is IValueWriter<Type> typeWriter)
            {
                typeWriter.WriteValue(value);

                return;
            }

            valueWriter.WriteString(value.AssemblyQualifiedName);
        }
    }

    internal sealed class AssemblyInterface<T> : IValueInterface<T> where T : System.Reflection.Assembly
    {
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }

            if (valueReader is IValueReader<System.Reflection.Assembly> assemblyReader)
            {
                return (T)assemblyReader.ReadValue();
            }

            var value = valueReader.DirectRead();

            if (value == null)
            {
                return null;
            }

            if (value is T tValue)
            {
                return tValue;
            }

            if (value is string sValue)
            {
                return (T)System.Reflection.Assembly.Load(sValue);
            }

            throw new NotSupportedException($"Cannot Read a 'Assembly' by '{value}'.");
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (value == null)
            {
                valueWriter.DirectWrite(null);

                return;
            }

            if (valueWriter is IValueWriter<T> tWriter)
            {
                tWriter.WriteValue(value);

                return;
            }

            if (valueWriter is IValueWriter<System.Reflection.Assembly> assemblyWriter)
            {
                assemblyWriter.WriteValue(value);

                return;
            }

            valueWriter.WriteString(value.FullName);
        }
    }

    internal sealed class EnumInterface<T> : IValueInterface<T> where T : Enum
    {
        static readonly Type EnumType = typeof(T);
        static readonly TypeCode TypeCode = Type.GetTypeCode(Enum.GetUnderlyingType(EnumType));
        
        public T ReadValue(IValueReader valueReader)
        {
            if (valueReader is IValueReader<T> tReader)
            {
                return tReader.ReadValue();
            }

            var value = valueReader.DirectRead();

            if (value is string st)
            {
                return (T)Enum.Parse(EnumType, valueReader.ReadString());
            }

            return (T)Enum.ToObject(EnumType, value);
        }

        public void WriteValue(IValueWriter valueWriter, T value)
        {
            if (valueWriter is IValueWriter<T> tWriter)
            {
                tWriter.WriteValue(value);

                return;
            }

            var name = Enum.GetName(EnumType, value);

            if (name != null)
            {
                valueWriter.WriteString(name);
            }
            else
            {
                switch (TypeCode)
                {
                    case TypeCode.SByte:
                        valueWriter.WriteSByte((sbyte)(object)value);
                        break;
                    case TypeCode.Byte:
                        valueWriter.WriteByte((byte)(object)value);
                        break;
                    case TypeCode.Int16:
                        valueWriter.WriteInt16((short)(object)value);
                        break;
                    case TypeCode.UInt16:
                        valueWriter.WriteUInt16((ushort)(object)value);
                        break;
                    case TypeCode.UInt32:
                        valueWriter.WriteUInt32((uint)(object)value);
                        break;
                    case TypeCode.Int64:
                        valueWriter.WriteInt64((long)(object)value);
                        break;
                    case TypeCode.UInt64:
                        valueWriter.WriteUInt64((ulong)(object)value);
                        break;
                    default:
                        valueWriter.WriteInt32((int)(object)value);
                        break;
                }
            }
        }
    }

    internal sealed class DbNullInterface : IValueInterface<DBNull>
    {
        public DBNull ReadValue(IValueReader valueReader)
        {
            var value = valueReader.DirectRead();

            if (value == null || value == DBNull.Value)
            {
                return DBNull.Value;
            }

            throw new NotSupportedException("Unable convert value to DbNull.");
        }

        public void WriteValue(IValueWriter valueWriter, DBNull value)
        {
            valueWriter.DirectWrite(null);
        }
    }
}