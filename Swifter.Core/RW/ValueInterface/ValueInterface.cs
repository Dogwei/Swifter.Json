using Swifter.Readers;
using Swifter.Reflection;
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

            if (VersionDifferences.IsSupportEmit)
            {
                DefaultObjectInterfaceType = typeof(FastObjectInterface<>);
            }
            else
            {
                DefaultObjectInterfaceType = typeof(XObjectInterface<>);
            }

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
}