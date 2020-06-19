using Swifter.Reflection;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.RW
{
    /// <summary>
    /// <see cref="ValueInterface{T}"/> 提供在 ValueReader 中读取指定类型的值或在 ValueWriter 中写入指定类型的值。
    /// 此类型提供泛型方法，效率更高。
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
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
        public static bool IsNotModified { get; private set; }

        /// <summary>
        /// 表示是否使用用户自定义的读写方法，如果为 True, FastObjectRW 将不优化基础类型的读写。
        /// </summary>
        public override bool InterfaceIsNotModified => IsNotModified;

        /// <summary>
        /// 获取值读写器接口。
        /// </summary>
        public override object Interface => Content;

        /// <summary>
        /// 获取值的类型。
        /// </summary>
        public override Type Type => typeof(T);

        static ValueInterface()
        {
            try
            {
                RuntimeHelpers.RunClassConstructor(typeof(ValueInterface).TypeHandle);
            }
            catch
            {
            }

            try
            {
                RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
            }
            catch
            {
            }

            IsNotModified = true;

            if (Content != null)
            {
                return;
            }

            for (int i = Mapers.Count - 1; i >= 0; --i)
            {
                var valueInterface = Mapers[i].TryMap<T>();

                if (valueInterface != null)
                {
                    Content = valueInterface;

                    return;
                }
            }

            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

            var nestedType = typeof(T).GetNestedType(nameof(ValueInterface), flags);

            if (nestedType != null && nestedType.GetConstructor(flags, Type.DefaultBinder, Type.EmptyTypes, null) != null)
            {
                if (nestedType.IsGenericTypeDefinition && typeof(T).IsGenericType && !typeof(T).IsGenericTypeDefinition)
                {
                    var nestedTypeGenArgs = nestedType.GetGenericArguments();
                    var typeGenArgs = typeof(T).GetGenericArguments();

                    if (typeGenArgs.Length == nestedTypeGenArgs.Length)
                    {
                        nestedType = nestedType.MakeGenericType(typeGenArgs);
                    }
                }

                if (typeof(IValueInterface<T>).IsAssignableFrom(nestedType))
                {
                    Content = (IValueInterface<T>)Activator.CreateInstance(nestedType);

                    return;
                }
            }

            Content = (IValueInterface<T>)Activator.CreateInstance(defaultObjectInterfaceType.MakeGenericType(typeof(T)));
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
            if (ContentLock is null)
            {
                lock (MapersLock)
                {
                    if (ContentLock is null)
                    {
                        ContentLock = new object();
                    }
                }
            }

            lock (ContentLock)
            {
                valueInterface = valueInterface ?? throw new ArgumentNullException(nameof(valueInterface));

                IsNotModified = false;

                if (Content is TargetedValueInterface)
                {
                    TargetedValueInterface.SetDefaultInterface(valueInterface);
                }
                else
                {
                    Content = valueInterface;
                }
            }
        }

        /// <summary>
        /// 获取该类型的值读写接口实例。
        /// </summary>
        /// <returns>值读写接口实例</returns>
        public static IValueInterface<T> GetInterface() => Content;

        /// <summary>
        /// 设置针对某一目标值读写器的读写接口实例。
        /// </summary>
        /// <param name="targeted">目标</param>
        /// <param name="valueInterface">读写接口实例</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void SetTargetedInterface(ITargetedBind targeted, IValueInterface<T> valueInterface)
        {
            TargetedValueInterface.Set(targeted, valueInterface);
        }

        /// <summary>
        /// 获取针对某一目标值读写器的读写接口实例。
        /// </summary>
        /// <param name="targeted">目标</param>
        /// <returns>返回读写接口实例</returns>
        public static IValueInterface<T> GetTargetedInterface(ITargetedBind targeted)
        {
            return TargetedValueInterface.Get<T>(targeted);
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

        /// <summary>
        /// 将该类型的值通过 <see cref="XConvert"/> 转换为指定类型。
        /// </summary>
        /// <typeparam name="TSource">指定类型</typeparam>
        /// <param name="value">该类型的值</param>
        /// <returns>返回指定类型的值</returns>
        public override object XConvertFrom<TSource>(TSource value)
        {
            return XConvert.Convert<TSource, T>(value);
        }

        /// <summary>
        /// 将值类型的值通过 <see cref="XConvert"/> 转换为该类型的值。
        /// </summary>
        /// <typeparam name="TDestination">指定类型</typeparam>
        /// <param name="value">指定类型的值</param>
        /// <returns>返回该类型的值</returns>
        public override TDestination XConvertTo<TDestination>(object value)
        {
            return XConvert.Convert<T, TDestination>((T)value);
        }
    }

    /// <summary>
    /// <see cref="RW.ValueInterface"/> 提供在 ValueReader 中读取指定类型的值或在 ValueWriter 中写入指定类型的值。
    /// 此类提供非泛型方法。
    /// </summary>
    public abstract class ValueInterface
    {
        internal static readonly List<IValueInterfaceMaper> Mapers;
        internal static readonly object MapersLock;
        internal static readonly Dictionary<IntPtr, ValueInterface> TypedCache;

        internal static Type defaultObjectInterfaceType;

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
        /// 获取或设置是否对元组的支持。
        /// 如果为否，那么元组类型将被当作普通对象处理。
        /// 必须在程序初始化时设置此值才会生效。
        /// </summary>
        public static bool ValueTupleSupport = true;

        private struct TestStruct
        {

        }

        private class TestClass
        {

        }

        /// <summary>
        /// 设置指定类型在 WriteValue 方法中写入为指定格式的字符串。在 ReadValue 方法是依然使用默认方法。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="format">指定指定格式</param>
        public static void SetValueFormat<T>(string format) where T : IFormattable
        {
            ValueInterface<T>.SetInterface(new SetValueFormatInterface<T>(ValueInterface<T>.Content, format));
        }

        static void LoadExtensionAssembly()
        {
            try
            {
                var fileInfo = new FileInfo(typeof(ValueInterface).Assembly.Location);

                foreach (var assemblyFile in fileInfo.Directory.GetFiles().Where(file => file.Name.StartsWith("Swifter") && file.Name.EndsWith($"Extensions{fileInfo.Extension}")))
                {
                    try
                    {
                        Assembly.LoadFile(assemblyFile.FullName)?.GetType("Swifter.ExtensionLoader")?.GetMethod("Load", Type.EmptyTypes)?.Invoke(null, null);
                    }
                    catch
                    {
                    }
                }
            }
            catch
            {
            }


        }

        static ValueInterface()
        {
            lock (typeof(ValueInterface))
            {
                if (MapersLock !=null)
                {
                    return;
                }

                Mapers = new List<IValueInterfaceMaper>();
                MapersLock = new object();

                if (VersionDifferences.IsSupportEmit)
                {
                    defaultObjectInterfaceType = typeof(FastObjectInterface<>);
                }
                else
                {
                    defaultObjectInterfaceType = typeof(XObjectInterface<>);
                }

                TypedCache = new Dictionary<IntPtr, ValueInterface>();

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
                ValueInterface<Uri>.Content = new UriInterface();

                Mapers.Add(new BasicInterfaceMaper());
                Mapers.Add(new TaskInterfaceMaper());
                Mapers.Add(new CollectionInterfaceMaper());
                Mapers.Add(new GenericCollectionInterfaceMapper());
                Mapers.Add(new DataInterfaceMaper());
                Mapers.Add(new TupleInterfaceMaper());
                Mapers.Add(new ArrayInterfaceMaper());

                LoadExtensionAssembly();
            }
        }

        /// <summary>
        /// 添加一个类型与 ValueInterface 的匹配器。
        /// 此匹配器可以自定义类型的读写方法。
        /// 后加入的匹配器优先级高。
        /// </summary>
        /// <param name="maper">类型与 ValueInterface 的匹配器</param>
        public static void AddMaper(IValueInterfaceMaper maper)
        {
            if (maper is null)
            {
                throw new ArgumentNullException(nameof(maper));
            }

            lock (MapersLock)
            {
                Mapers.Add(maper);
            }
        }

        /// <summary>
        /// 非泛型方式获取指定类型的 <see cref="RW.ValueInterface"/> ，此方式效率并不高。
        /// 如果是已知类型，请考虑使用泛型方式 <see cref="ValueInterface{T}"/> 获取。
        /// 如果是未知类型的实例，请考虑使用 <see cref="GetInterface(object)"/> 获取。
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns>返回一个 <see cref="RW.ValueInterface"/> 实例。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ValueInterface GetInterface(Type type)
        {
            if (TypedCache.TryGetValue(TypeHelper.GetTypeHandle(type), out var @interface))
            {
                return @interface;
            }

            return InternalGetInterface(type);
        }

        /// <summary>
        /// 非泛型方式获取实例的类型的 <see cref="RW.ValueInterface"/> ，此方式效率比 <see cref="GetInterface(Type)"/> 高，但比 <see cref="ValueInterface{T}"/> 低。
        /// </summary>
        /// <param name="obj">指定一个实例，此实例不能为 Null。</param>
        /// <returns>返回一个 <see cref="RW.ValueInterface"/> 实例。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ValueInterface GetInterface(object obj)
        {
            if (TypedCache.TryGetValue(TypeHelper.GetTypeHandle(obj), out var @interface))
            {
                return @interface;
            }

            return InternalGetInterface(obj.GetType());
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ValueInterface InternalGetInterface(Type type)
        {
            lock (TypedCache)
            {
                var handle = TypeHelper.GetTypeHandle(type);

                if (TypedCache.TryGetValue(handle, out var @interface))
                {
                    return @interface;
                }

                @interface = Underlying.As<ValueInterface>(
                    Activator.CreateInstance(
                        typeof(ValueInterface<>).MakeGenericType(type)
                        )
                    );

                TypedCache.Add(handle, @interface);

                return @interface;
            }
        }

        /// <summary>
        /// 往写入器中写入一个未知类型值的方法。
        /// </summary>
        /// <param name="valueWriter">写入器</param>
        /// <param name="value">一个对象值</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void WriteValue(IValueWriter valueWriter, object value)
        {
            if (value is null)
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
        /// <param name="targeted">目标</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void RemoveTargetedInterface(ITargetedBind targeted)
        {
            TargetedValueInterface.Remove(targeted);
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

        /// <summary>
        /// 表示是否使用用户自定义的读写方法，如果为 True, FastObjectRW 将不优化基础类型的读写。
        /// </summary>
        public abstract bool InterfaceIsNotModified { get; }

        /// <summary>
        /// 获取值读写器接口。
        /// </summary>
        public abstract object Interface { get; }

        /// <summary>
        /// 获取值的类型。
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        /// 将该类型的值通过 <see cref="XConvert"/> 转换为指定类型。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">该类型的值</param>
        /// <returns>返回指定类型的值</returns>
        public abstract T XConvertTo<T>(object value);

        /// <summary>
        /// 将值类型的值通过 <see cref="XConvert"/> 转换为该类型的值。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="value">指定类型的值</param>
        /// <returns>返回该类型的值</returns>
        public abstract object XConvertFrom<T>(T value);
    }
}