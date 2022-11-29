using Swifter.Reflection;
using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Swifter.RW
{
    /// <summary>
    /// <see cref="ValueInterface{T}"/> 提供在 ValueReader 中读取指定类型的值或在 ValueWriter 中写入指定类型的值。
    /// 此类型提供泛型方法，效率更高。
    /// </summary>
    /// <typeparam name="T">值的类型</typeparam>
    public sealed class ValueInterface<T> : ValueInterface
    {
        private static ValueInterface<T>? instance;
        /// <summary>
        /// 此类型的具体读写方法实现。
        /// </summary>
        internal static IValueInterface<T> Content;

        /// <summary>
        /// 获取此类型是否为最终类型。
        /// </summary>
        public static readonly bool IsFinalType = TypeHelper.IsFinal(typeof(T));

        /// <summary>
        /// 获取此读写接口的实例。
        /// </summary>
        public static ValueInterface<T> Instance
        {
            [MethodImpl(VersionDifferences.AggressiveInlining)]
            get
            {
                return instance ?? InternalGetInstance();

                [MethodImpl(MethodImplOptions.NoInlining)]
                static ValueInterface<T> InternalGetInstance()
                {
                    Interlocked.CompareExchange(ref instance, new ValueInterface<T>(), null);

                    return instance;
                }
            }
        }

        /// <summary>
        /// 获取当前类型的读写接口是否为 <see cref="FastObjectInterface{T}"/>。
        /// </summary>
        public static bool IsFastObjectInterface =>
            Content is IFastObjectRWCreater<T>/* || Content is FastObjectInterface<T>*/;

        /// <summary>
        /// 表示当前值接口是否是默认行为的值接口。
        /// </summary>
        public static bool IsDefaultBehavior { get; private set; }

        /// <summary>
        /// 表示当前值接口是否是默认行为的值接口。
        /// </summary>
        public override bool IsDefaultBehaviorInternal => IsDefaultBehavior;

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
                IsDefaultBehavior = true;

                try
                {
                    RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
                }
                catch
                {
                }

                if (Content != null)
                {
                    return;
                }

                #region -- 尝试获取当前类型下的嵌套值接口类型。 --

                const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

                var NestedType = typeof(T).GetNestedType(nameof(ValueInterface), flags);

                if (NestedType != null && NestedType.GetConstructor(flags, Type.DefaultBinder, Type.EmptyTypes, null) is ConstructorInfo ConstructorInfo)
                {
                    if (NestedType.IsGenericTypeDefinition && typeof(T).IsGenericType && !typeof(T).IsGenericTypeDefinition)
                    {
                        var NestedTypeGenArgs = NestedType.GetGenericArguments();
                        var TypeGenArgs = typeof(T).GetGenericArguments();

                        if (TypeGenArgs.Length == NestedTypeGenArgs.Length)
                        {
                            NestedType = NestedType.MakeGenericType(TypeGenArgs);

                            ConstructorInfo = NestedType.GetConstructor(flags, Type.DefaultBinder, Type.EmptyTypes, null)!;
                        }
                    }

                    if (typeof(IValueInterface<T>).IsAssignableFrom(NestedType))
                    {
                        SetInterface((IValueInterface<T>)ConstructorInfo.Invoke(new object[0]));

                        return;
                    }
                }

                #endregion

                #region -- 尝试在映射器集合中匹配值接口。 -- 

                for (int i = Mapers.Count - 1; i >= 0; --i)
                {
                    var valueInterface = Mapers[i].TryMap<T>();

                    if (valueInterface != null)
                    {
                        SetInterface(valueInterface);

                        return;
                    }
                }

                #endregion

                #region DefaultObjectInterface

                var objectInterfaceType = defaultObjectInterfaceType.MakeGenericType(typeof(T));

                var objectInterfaceInstance = (IValueInterface<T>)Activator.CreateInstance(objectInterfaceType)!;

                SetInterface(objectInterfaceInstance);

                #endregion
            }
            finally
            {
                OnTypeLoaded(typeof(T));
            }
        }

        /// <summary>
        /// 往写入器中写入该类型值的方法。
        /// </summary>
        /// <param name="value">T 类型的值</param>
        /// <param name="valueWriter">值写入器</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static void WriteValue(IValueWriter valueWriter, T? value)
        {
            Content.WriteValue(valueWriter, value);
        }

        /// <summary>
        /// 在读取器中读取该类型值的方法。
        /// </summary>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回该类型的值</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static T? ReadValue(IValueReader valueReader)
        {
            return Content.ReadValue(valueReader);
        }

        /// <summary>
        /// 设置该类型的值读写接口实例。
        /// </summary>
        /// <param name="valueInterface">值读写接口实例</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        [MemberNotNull(nameof(Content))]
        public static void SetInterface(IValueInterface<T> valueInterface)
        {
            if (valueInterface == null)
            {
                throw new ArgumentNullException(nameof(valueInterface));
            }

            lock (Instance)
            {
                if (Content is ChainedValueInterfaceBase<T> chainedValueInterface)
                {
                    while (chainedValueInterface.PreviousValueInterface is ChainedValueInterfaceBase<T> previousChainedValueInterface)
                    {
                        chainedValueInterface = previousChainedValueInterface;
                    }

                    chainedValueInterface.PreviousValueInterface = valueInterface;
                }
                else
                {
                    IsDefaultBehavior = valueInterface is IDefaultBehaviorValueInterface;

                    Content = valueInterface;
                }
            }
        }

        /// <summary>
        /// 获取该类型的值读写接口实例。
        /// </summary>
        /// <returns>值读写接口实例</returns>
        public static IValueInterface<T> GetInterface()
        {
            IValueInterface<T> content = Content;

            while (content is ChainedValueInterfaceBase<T> chainedValueInterface 
                && chainedValueInterface.PreviousValueInterface is not null)
            {
                content = chainedValueInterface.PreviousValueInterface;
            }

            return content;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueInterface"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddChainedInterface(ChainedValueInterfaceBase<T> valueInterface)
        {
            if (valueInterface == null)
            {
                throw new ArgumentNullException(nameof(valueInterface));
            }

            lock (Instance)
            {
                IsDefaultBehavior = false;

                valueInterface.PreviousValueInterface = Content;

                Content = valueInterface;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueInterface"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public static void RemoveChainedInterface(ChainedValueInterfaceBase<T> valueInterface)
        {
            if (valueInterface == null)
            {
                throw new ArgumentNullException(nameof(valueInterface));
            }

            lock (Instance)
            {
                var previousValueInterface = Interlocked.Exchange(ref valueInterface.PreviousValueInterface, null);

                if (previousValueInterface == null)
                {
                    throw new NullReferenceException("Invalid ValueInterface, PreviousValueInterface cannot be null!");
                }

                ref IValueInterface<T> content = ref Content;

                while (!ReferenceEquals(content, valueInterface))
                {
                    if (content is ChainedValueInterfaceBase<T> chainedValueInterface)
                    {
                        content = ref chainedValueInterface.PreviousValueInterface!;
                    }
                    else
                    {
                        throw new InvalidOperationException("The ValueInterface is not in the collection.");
                    }
                }

                content = previousValueInterface;
            }
        }

        /// <summary>
        /// 非泛型读取值方法。
        /// </summary>
        /// <param name="valueReader">值读取器。</param>
        /// <returns>返回一个 T 类型的值。</returns>
        public override object? Read(IValueReader valueReader)
        {
            return Content.ReadValue(valueReader);
        }

        /// <summary>
        /// 非泛型写入值方法。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="value">T 类型的值</param>
        public override void Write(IValueWriter valueWriter, object? value)
        {
            Content.WriteValue(valueWriter, (T?)value);
        }
    }

    /// <summary>
    /// <see cref="ValueInterface"/> 提供在 ValueReader 中读取指定类型的值或在 ValueWriter 中写入指定类型的值。
    /// 此类提供非泛型方法。
    /// </summary>
    public abstract class ValueInterface
    {
        internal static readonly List<IValueInterfaceMaper> Mapers;
        internal static readonly object MapersLock;
        internal static readonly Dictionary<IntPtr, ValueInterface> TypedCache;

        internal static Type defaultObjectInterfaceType;

        /// <summary>
        /// 当类型被加载后触发。
        /// </summary>
        public static event Action<Type>? TypeLoaded;

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

        internal static void OnTypeLoaded(Type type)
        {
            TypeLoaded?.Invoke(type);
        }

        /// <summary>
        /// 设置指定类型在 WriteValue 方法中写入为指定格式的字符串。在 ReadValue 方法是依然使用默认方法。
        /// </summary>
        /// <typeparam name="T">指定类型</typeparam>
        /// <param name="format">指定指定格式</param>
        /// <param name="formatProvider">格式提供者, 为 <see langword="null"/> 将使用 <see cref="CultureInfo.CurrentCulture"/>.</param>
        public static void SetValueFormat<T>(string format, IFormatProvider? formatProvider = null) where T : IFormattable
        {
            ValueInterface<T>.SetInterface(new SetValueFormatInterface<T>(ValueInterface<T>.Content, format, formatProvider));
        }

        static void LoadExtensionAssembly()
        {
            try
            {
                var fileInfo = new FileInfo(typeof(ValueInterface).Assembly.Location);

                var directory = fileInfo.Directory;

                if (directory != null)
                {
                    foreach (var assemblyFile in directory.GetFiles().Where(file => file.Name.StartsWith("Swifter") && file.Name.EndsWith($"Extensions{fileInfo.Extension}")))
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
            }
            catch
            {
            }


        }

#if NET7_0_OR_GREATER
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(FastObjectInterface<>))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(XObjectInterface<>))]
#endif
        static ValueInterface()
        {
            Mapers = new List<IValueInterfaceMaper>();
            MapersLock = new object();
            TypedCache = new Dictionary<IntPtr, ValueInterface>();

            if (VersionDifferences.IsSupportEmit)
            {
                defaultObjectInterfaceType = typeof(FastObjectInterface<>);
            }
            else
            {
                defaultObjectInterfaceType = typeof(XObjectInterface<>);
            }

            Mapers.Add(new UnknowTypeInterfaceMaper());
            Mapers.Add(new CollectionInterfaceMaper());
            Mapers.Add(new GenericCollectionInterfaceMapper());
            Mapers.Add(new DataInterfaceMaper());
            Mapers.Add(new TupleInterfaceMaper());
            Mapers.Add(new ArrayInterfaceMaper());
            Mapers.Add(new BasicInterfaceMaper());

            LoadExtensionAssembly();
        }

        /// <summary>
        /// 添加一个类型与 ValueInterface 的匹配器。
        /// 此匹配器可以自定义类型的读写方法。
        /// 后加入的匹配器优先级高。
        /// </summary>
        /// <param name="maper">类型与 ValueInterface 的匹配器</param>
        public static void AddMaper(IValueInterfaceMaper maper)
        {
            lock (MapersLock)
            {
                Mapers.Add(maper);
            }
        }

        /// <summary>
        /// 非泛型方式获取指定类型的 <see cref="ValueInterface"/> ，此方式效率并不高。
        /// 如果是已知类型，请考虑使用泛型方式 <see cref="ValueInterface{T}"/> 获取。
        /// 如果是未知类型的实例，请考虑使用 <see cref="GetInterface(object)"/> 获取。
        /// </summary>
        /// <param name="type">指定类型</param>
        /// <returns>返回一个 <see cref="ValueInterface"/> 实例。</returns>
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
        /// 非泛型方式获取实例的类型的 <see cref="ValueInterface"/> ，此方式效率比 <see cref="GetInterface(Type)"/> 高，但比 <see cref="ValueInterface{T}"/> 低。
        /// </summary>
        /// <param name="obj">指定一个实例，此实例不能为 Null。</param>
        /// <returns>返回一个 <see cref="ValueInterface"/> 实例。</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static ValueInterface GetInterface(object obj)
        {
            if (TypedCache.TryGetValue(TypeHelper.GetTypeHandle(obj), out var @interface))
            {
                return @interface;
            }

            return InternalGetInterface(obj);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static ValueInterface InternalGetInterface(object obj)
        {
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

                if (type.CanBeGenericParameter())
                {
                    @interface = Unsafe.As<ValueInterface>(typeof(ValueInterface<>)
                        .MakeGenericType(type)
                        .GetProperty(nameof(ValueInterface<object>.Instance), BindingFlags.Public | BindingFlags.Static)!
                        .GetValue(null, null)!);
                }
                else
                {
                    @interface = new NonGenericValueInterface(type);
                }

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
        public static void WriteValue(IValueWriter valueWriter, object? value)
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
        public static object? ReadValue(IValueReader valueReader, Type type)
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
        public static T? ReadValue<T>(IValueReader valueReader)
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
        public static void WriteValue<T>(IValueWriter valueWriter, T? value)
        {
            ValueInterface<T>.Content.WriteValue(valueWriter, value);
        }

        /// <summary>
        /// 在 IValueReader 中读取该类型的值。
        /// </summary>
        /// <param name="valueReader">值读取器</param>
        /// <returns>返回该类型的值</returns>
        public abstract object? Read(IValueReader valueReader);

        /// <summary>
        /// 在 IValueWriter 中写入该类型的值。
        /// </summary>
        /// <param name="valueWriter">值写入器</param>
        /// <param name="value">该类型的值</param>
        public abstract void Write(IValueWriter valueWriter, object? value);

        /// <summary>
        /// 表示当前值接口是否是默认行为的值接口。
        /// </summary>
        public abstract bool IsDefaultBehaviorInternal { get; }

        /// <summary>
        /// 获取值读写器接口。
        /// </summary>
        public abstract object Interface { get; }

        /// <summary>
        /// 获取值的类型。
        /// </summary>
        public abstract Type Type { get; }
    }
}