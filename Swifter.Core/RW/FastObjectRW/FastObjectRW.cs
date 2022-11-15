using Swifter.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using static Swifter.RW.FastObjectRW;

namespace Swifter.RW
{
    /// <summary>
    /// FastObjectRW 基于 Emit 实现的几乎完美效率的对象读写器。
    /// </summary>
    public static class FastObjectRW
    {
        internal static readonly Dictionary<Type, FastObjectRWOptions> TypeOptions;

        internal static readonly FastObjectRWOptions Initialized = (FastObjectRWOptions)0x10000000;

        static FastObjectRW()
        {
            TypeOptions = new Dictionary<Type, FastObjectRWOptions>();

            SetCurrentOptions(typeof(ValueType), DefaultOptions | FastObjectRWOptions.Field);
        }

        internal static FastObjectRWOptions GetCurrentOptions(Type type)
        {
            if (TypeOptions.TryGetValue(type, out var value))
            {
                return value;
            }

            lock (TypeOptions)
            {
                return TypeOptions.GetOrAdd(type, t =>
                {
                    if (t.IsRecordType() || t.IsTupleType())
                    {
                        return DefaultOptions | FastObjectRWOptions.Allocate | FastObjectRWOptions.AutoPropertyDirectRW;
                    }

                    if (t.BaseType is null || t.BaseType == typeof(object))
                    {
                        return DefaultOptions;
                    }

                    return GetCurrentOptions(t.BaseType) & ~Initialized;
                });
            }
        }

        internal static void SetCurrentOptions(Type type, FastObjectRWOptions options)
        {
            lock (TypeOptions)
            {
                if (TypeOptions.TryGetValue(type, out var currentOptions) && currentOptions.On(Initialized))
                {
                    throw new InvalidOperationException("Invalid modification option After type has been initialized.");
                }

                TypeOptions[type] = options;
            }
        }

        internal static FastObjectRWOptions SetToInitialized(Type type)
        {
            lock (TypeOptions)
            {
                var currentOptions = GetCurrentOptions(type);

                if (currentOptions.On(Initialized))
                {
                    throw new InvalidOperationException("Invalid modification option After type has been initialized.");
                }

                TypeOptions[type] = currentOptions | Initialized;

                return currentOptions;
            }
        }

        /// <summary>
        /// FastObjectRW 全局默认配置。
        /// </summary>
        public static FastObjectRWOptions DefaultOptions { get; set; } =
            // FastObjectRWOptions.NotFoundException |
            // FastObjectRWOptions.CannotGetException |
            // FastObjectRWOptions.CannotSetException |
            FastObjectRWOptions.BasicTypeDirectCallMethod |
            FastObjectRWOptions.Property |
            FastObjectRWOptions.Field |
            FastObjectRWOptions.IgnoreCase |
            FastObjectRWOptions.InheritedMembers;
    }

    /// <summary>
    /// FastObjectRW 基于 Emit 实现的几乎完美效率的对象读写器。
    /// </summary>
    /// <typeparam name="T">数据源对象的类型</typeparam>
    public abstract unsafe partial class FastObjectRW<T> : IObjectRW, IHasKeysDataRW<Ps<char>>, IHasKeysDataRW<Ps<Utf8Byte>>, IDataRW<int>
    {
        /// <summary>
        /// 读取或设置该类型的 FastObjectRWOptions 枚举配置项。
        /// 如果该类型已经初始化完成，则无法设置该值，且发生异常。
        /// 此属性不是高性能的，请不要多次读写。
        /// </summary>
        public static FastObjectRWOptions CurrentOptions
        {
            get => GetCurrentOptions(typeof(T)) & ~Initialized;
            set => SetCurrentOptions(typeof(T), value & ~Initialized);
        }

        /// <summary>
        /// 创建 FastObjectRW 实例。
        /// </summary>
        /// <returns>返回 FastObjectRW 实例</returns>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static FastObjectRW<T> Create()
        {
            return StaticFastObjectRW<T>.Creater.Create();
        }

        /// <summary>
        /// 获取指定索引处字段的值读写接口的实例。
        /// 此方法供内部使用。
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>返回读写接口实例</returns>
        protected internal static object? GetValueInterfaceInstance(int index)
        {
            return StaticFastObjectRW<T>.Fields[index].InterfaceInstance;
        }

        /// <summary>
        /// 数据源。
        /// </summary>
        public T? content;

        /// <summary>
        /// 调用默认无参的构造函数初始化数据源。
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// 调用默认无参的构造函数初始化数据源。
        /// </summary>
        /// <param name="capacity">不使用此参数</param>
        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public void Initialize(int capacity) => Initialize();

        /// <summary>
        /// 将指定名称的成员的值写入到值写入器中。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <param name="valueWriter">值写入器</param>
        public abstract void OnReadValue(string key, IValueWriter valueWriter);

        /// <summary>
        /// 将值读取器中的值写入到指定名称的成员中。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <param name="valueReader">值读取器</param>
        public abstract void OnWriteValue(string key, IValueReader valueReader);

        /// <summary>
        /// 将数据源中的所有成员写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="stopToken">停止令牌</param>
        public abstract void OnReadAll(IDataWriter<string> dataWriter, RWStopToken stopToken = default);

        /// <summary>
        /// 从数据读取器中读取所有数据源字段到数据源的值
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <param name="stopToken">停止令牌</param>
        public abstract void OnWriteAll(IDataReader<string> dataReader, RWStopToken stopToken = default);

        /// <summary>
        /// 获取指定名称的成员的值读写器。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <returns>返回值读写器</returns>
        public IValueRW this[string key]
        {
            get
            {
                var ordinal = GetOrdinal(key);

                if (ordinal >= 0)
                {
                    return GetValueRW(ordinal);
                }

                if (StaticFastObjectRW<T>.Options.On(FastObjectRWOptions.NotFoundException))
                {
                    throw new MissingMemberException(typeof(T).FullName, key);
                }
                else
                {
                    return RWHelper.DefaultValueRW;
                }
            }
        }

        /// <summary>
        /// 获取指定名称的成员的值读写器。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <returns>返回值读写器</returns>
        public IValueRW this[Ps<char> key]
        {
            get
            {
                var ordinal = GetOrdinal(key);

                if (ordinal >= 0)
                {
                    return GetValueRW(ordinal);
                }

                if (StaticFastObjectRW<T>.Options.On(FastObjectRWOptions.NotFoundException))
                {
                    throw new MissingMemberException(typeof(T).FullName, key.ToStringEx());
                }
                else
                {
                    return RWHelper.DefaultValueRW;
                }
            }
        }

        /// <summary>
        /// 获取指定名称的成员的值读写器。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <returns>返回值读写器</returns>
        public IValueRW this[Ps<Utf8Byte> key]
        {
            get
            {
                var ordinal = GetOrdinal(key);

                if (ordinal >= 0)
                {
                    return GetValueRW(ordinal);
                }

                if (StaticFastObjectRW<T>.Options.On(FastObjectRWOptions.NotFoundException))
                {
                    throw new MissingMemberException(typeof(T).FullName, key.ToStringEx());
                }
                else
                {
                    return RWHelper.DefaultValueRW;
                }
            }
        }

        /// <summary>
        /// 获取指定索引处的成员的值读写器。
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <returns>返回值读写器</returns>
        public IValueRW this[int index]
        {
            get
            {
                return GetValueRW(index);
            }
        }

        /// <summary>
        /// 获取该类型所有的成员的数量。
        /// </summary>
        public int Count => StaticFastObjectRW<T>.Keys.Length;

        /// <summary>
        /// 将指定名称的成员的值写入到值写入器中。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <param name="valueWriter">值写入器</param>
        public abstract void OnReadValue(Ps<char> key, IValueWriter valueWriter);

        /// <summary>
        /// 将数据源中的所有成员写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="stopToken">停止令牌</param>
        public abstract void OnReadAll(IDataWriter<Ps<char>> dataWriter, RWStopToken stopToken = default);

        /// <summary>
        /// 将值读取器中的值写入到指定名称的成员中。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <param name="valueReader">值读取器</param>
        public abstract void OnWriteValue(Ps<char> key, IValueReader valueReader);

        /// <summary>
        /// 从数据读取器中读取所有数据源字段到数据源的值
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <param name="stopToken">停止令牌</param>
        public abstract void OnWriteAll(IDataReader<Ps<char>> dataReader, RWStopToken stopToken = default);

        /// <summary>
        /// 将指定名称的成员的值写入到值写入器中。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <param name="valueWriter">值写入器</param>
        public abstract void OnReadValue(Ps<Utf8Byte> key, IValueWriter valueWriter);

        /// <summary>
        /// 将数据源中的所有成员写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="stopToken">停止令牌</param>
        public abstract void OnReadAll(IDataWriter<Ps<Utf8Byte>> dataWriter, RWStopToken stopToken = default);

        /// <summary>
        /// 将值读取器中的值写入到指定名称的成员中。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <param name="valueReader">值读取器</param>
        public abstract void OnWriteValue(Ps<Utf8Byte> key, IValueReader valueReader);

        /// <summary>
        /// 从数据读取器中读取所有数据源字段到数据源的值
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <param name="stopToken">停止令牌</param>
        public abstract void OnWriteAll(IDataReader<Ps<Utf8Byte>> dataReader, RWStopToken stopToken = default);

        /// <summary>
        /// 将指定索引处的成员的值写入到值写入器中。
        /// </summary>
        /// <param name="key">指定索引处</param>
        /// <param name="valueWriter">值写入器</param>
        public abstract void OnReadValue(int key, IValueWriter valueWriter);

        /// <summary>
        /// 将数据源中的所有成员写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="stopToken">停止令牌</param>
        public abstract void OnReadAll(IDataWriter<int> dataWriter, RWStopToken stopToken = default);

        /// <summary>
        /// 将值读取器中的值写入到指定索引处的成员中。
        /// </summary>
        /// <param name="key">指定索引处</param>
        /// <param name="valueReader">值读取器</param>
        public abstract void OnWriteValue(int key, IValueReader valueReader);

        /// <summary>
        /// 从数据读取器中读取所有数据源字段到数据源的值
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        /// <param name="stopToken">停止令牌</param>
        public abstract void OnWriteAll(IDataReader<int> dataReader, RWStopToken stopToken = default);

        /// <summary>
        /// 获取指定名称的字段的序号。
        /// </summary>
        /// <param name="name">指定名称</param>
        /// <returns>返回序号，如果没有该字段，则返回 -1</returns>
        public abstract int GetOrdinal(string name);

        /// <summary>
        /// 获取指定名称的字段的序号。
        /// </summary>
        /// <param name="name">指定名称</param>
        /// <returns>返回序号，如果没有该字段，则返回 -1</returns>
        public abstract int GetOrdinal(Ps<char> name);

        /// <summary>
        /// 获取指定名称的字段的序号。
        /// </summary>
        /// <param name="name">指定名称</param>
        /// <returns>返回序号，如果没有该字段，则返回 -1</returns>
        public abstract int GetOrdinal(Ps<Utf8Byte> name);

        /// <summary>
        /// 获取指定索引处的字段名称。
        /// </summary>
        /// <param name="ordinal">指定索引</param>
        /// <param name="name">返回字段名称</param>
        public void GetKey(int ordinal, out string name)
        {
            name = StaticFastObjectRW<T>.Keys[ordinal];
        }

        /// <summary>
        /// 获取指定索引处的字段名称。
        /// </summary>
        /// <param name="ordinal">指定索引</param>
        /// <param name="name">返回字段名称</param>
        public void GetKey(int ordinal, out Ps<char> name)
        {
            name = StaticFastObjectRW<T>._UTF16Keys[ordinal];
        }

        /// <summary>
        /// 获取指定索引处的字段名称。
        /// </summary>
        /// <param name="ordinal">指定索引</param>
        /// <param name="name">返回字段名称</param>
        public void GetKey(int ordinal, out Ps<Utf8Byte> name)
        {
            name = StaticFastObjectRW<T>._UTF8Keys[ordinal];
        }

        /// <summary>
        /// 获取指定索引处的值读取器。此函数仅为内部调用。
        /// </summary>
        internal protected abstract IValueRW GetValueRW(int ordinal);

        /// <summary>
        /// 加载指定索引处的值。此函数仅为内部调用。
        /// </summary>
        internal protected abstract void LoadValue(int ordinal, ref object value);

        /// <summary>
        /// 将值存储到指定索引处。此函数仅为内部调用。
        /// </summary>
        internal protected abstract void StoreValue(int ordinal, ref object value);

        /// <summary>
        /// 获取该读写器的名称。
        /// </summary>
        /// <returns>返回该读写器的名称</returns>
        public override string ToString() => $"{typeof(FastObjectRW).FullName}<{typeof(T).FullName}>";

        /// <summary>
        /// 读取或设置该读写器的数据源。
        /// </summary>
        public object? Content
        {
            get => content;
            set => content = (T?)value;
        }

        /// <summary>
        /// 获取该读写器数据源的类型。
        /// </summary>
        public Type ContentType => typeof(T);

        /// <summary>
        /// 此值对此读写器无效。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public Type? ValueType => null;

        /// <summary>
        /// 成员名称集合。
        /// </summary>
        public IEnumerable<string> Keys => StaticFastObjectRW<T>.ExportKeys;

        IEnumerable<Ps<char>> IHasKeysDataRW<Ps<char>>.Keys => StaticFastObjectRW<T>.ExportUTF16Keys;

        IEnumerable<Ps<char>> IHasKeysDataReader<Ps<char>>.Keys => StaticFastObjectRW<T>.ExportUTF16Keys;

        IEnumerable<Ps<char>> IHasKeysDataWriter<Ps<char>>.Keys => StaticFastObjectRW<T>.ExportUTF16Keys;

        IEnumerable<Ps<Utf8Byte>> IHasKeysDataRW<Ps<Utf8Byte>>.Keys => StaticFastObjectRW<T>.ExportUTF8Keys;

        IEnumerable<Ps<Utf8Byte>> IHasKeysDataReader<Ps<Utf8Byte>>.Keys => StaticFastObjectRW<T>.ExportUTF8Keys;

        IEnumerable<Ps<Utf8Byte>> IHasKeysDataWriter<Ps<Utf8Byte>>.Keys => StaticFastObjectRW<T>.ExportUTF8Keys;

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueReader IDataReader<string>.this[string key] => this[key];

        IValueWriter IDataWriter<string>.this[string key] => this[key];

        IValueReader IDataReader<Ps<char>>.this[Ps<char> key] => this[key];

        IValueWriter IDataWriter<Ps<char>>.this[Ps<char> key] => this[key];

        IValueReader IDataReader<Ps<Utf8Byte>>.this[Ps<Utf8Byte> key] => this[key];

        IValueWriter IDataWriter<Ps<Utf8Byte>>.this[Ps<Utf8Byte> key] => this[key];

        /// <summary>
        /// 值的读写器。此类型仅为内部使用。
        /// </summary>
        /// <typeparam name="TValue">值的类型</typeparam>
        internal protected sealed class ValueRW<TValue> : BaseGenericRW<TValue>, IValueRW<TValue>
        {
            readonly FastObjectRW<T> baseRW;
            readonly int ordinal;

            /// <summary>
            /// 创建值的读写器。
            /// </summary>
            /// <param name="baseRW">基数据读写器</param>
            /// <param name="ordinal">值的序号</param>
            public ValueRW(FastObjectRW<T> baseRW, int ordinal)
            {
                this.baseRW = baseRW;
                this.ordinal = ordinal;
            }

            /// <summary>
            /// 读取值。
            /// </summary>=
            [SkipLocalsInit]
            public override TValue? ReadValue()
            {
#if NET5_0_OR_GREATER
                Unsafe.SkipInit(out TValue? value);

                baseRW.LoadValue(ordinal, ref Unsafe.As<TValue?, object>(ref value));

                return value;
#else
                TValue? value = default;

                baseRW.LoadValue(ordinal, ref Unsafe.As<TValue?, object>(ref value));

                return value;
#endif
            }

            /// <summary>
            /// 写入值。
            /// </summary>
            public override void WriteValue(TValue? value)
            {
                baseRW.StoreValue(ordinal, ref Unsafe.As<TValue?, object>(ref value));
            }
        }
    }
}