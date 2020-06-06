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
                    // 对匿名类型进行 Allocate 和 AutoPropertyDirectRW
                    if (t.IsDefined(typeof(CompilerGeneratedAttribute), false))
                    {
                        return DefaultOptions | FastObjectRWOptions.Allocate | FastObjectRWOptions.AutoPropertyDirectRW;
                    }

                    // 对元组类型进行 Allocate 和 AutoPropertyDirectRW
                    if (t.IsClass && t.IsGenericType && t.Name.Contains("Tuple") && t.GetConstructor(Type.EmptyTypes) is null)
                    {
                        return DefaultOptions | FastObjectRWOptions.Allocate | FastObjectRWOptions.AutoPropertyDirectRW;
                    }

                    if ((t.BaseType ?? typeof(object)) == typeof(object))
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
                if (TypeOptions.TryGetValue(type, out var currentOptions) && (currentOptions & Initialized) != 0)
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

                if ((currentOptions & Initialized) != 0)
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
        public static
            FastObjectRWOptions DefaultOptions { get; set; } =
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
    public abstract partial class FastObjectRW<T> : IDataRW<string>, IDataRW<Ps<char>>, IDataRW<Ps<Utf8Byte>>, IDataRW<int>
    {
        /// <summary>
        /// 获取当前类型的读写接口是否为 FastObjectInterface。
        /// </summary>
        public static bool IsFastObjectInterface => 
            ValueInterface<T>.Content is IFastObjectRWCreater<T>/* || ValueInterface<T>.Content is FastObjectInterface<T>*/;

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
        protected internal static object GetValueInterfaceInstance(int index)
        {
            return StaticFastObjectRW<T>.Fields[index].InterfaceInstance;
        }

        /// <summary>
        /// 数据源。
        /// </summary>
        public T content;

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
        public abstract void OnReadAll(IDataWriter<string> dataWriter);

        /// <summary>
        /// 从数据读取器中读取所有数据源字段到数据源的值
        /// </summary>
        /// <param name="dataReader">数据读取器</param>
        public abstract void OnWriteAll(IDataReader<string> dataReader);

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

                if (ordinal == -1)
                {
                    if ((StaticFastObjectRW<T>.Options & FastObjectRWOptions.NotFoundException) != 0)
                    {
                        throw new MissingMemberException(typeof(T).FullName, key);
                    }
                    else
                    {
                        return RWHelper.DefaultValueRW;
                    }
                }

                return new ValueCopyer<int>(this, ordinal);
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

                if (ordinal == -1)
                {
                    if ((StaticFastObjectRW<T>.Options & FastObjectRWOptions.NotFoundException) != 0)
                    {
                        throw new MissingMemberException(typeof(T).FullName, key.ToStringEx());
                    }
                    else
                    {
                        return RWHelper.DefaultValueRW;
                    }
                }

                return new ValueCopyer<int>(this, ordinal);
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

                if (ordinal == -1)
                {
                    if ((StaticFastObjectRW<T>.Options & FastObjectRWOptions.NotFoundException) != 0)
                    {
                        throw new MissingMemberException(typeof(T).FullName, key.ToStringEx());
                    }
                    else
                    {
                        return RWHelper.DefaultValueRW;
                    }
                }

                return new ValueCopyer<int>(this, ordinal);
            }
        }

        /// <summary>
        /// 获取指定索引处的成员的值读写器。
        /// </summary>
        /// <param name="index">指定索引</param>
        /// <returns>返回值读写器</returns>
        public IValueRW this[int index] => new ValueCopyer<int>(this, index);

        /// <summary>
        /// 获取该类型所有的成员。
        /// </summary>
        public IEnumerable<string> Keys => StaticFastObjectRW<T>.StringKeys;

        /// <summary>
        /// 获取该类型所有的成员的数量。
        /// </summary>
        public int Count => StaticFastObjectRW<T>.StringKeys.Length;

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
        public abstract void OnReadAll(IDataWriter<Ps<char>> dataWriter);

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
        public abstract void OnWriteAll(IDataReader<Ps<char>> dataReader);

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
        public abstract void OnReadAll(IDataWriter<Ps<Utf8Byte>> dataWriter);

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
        public abstract void OnWriteAll(IDataReader<Ps<Utf8Byte>> dataReader);

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
        public abstract void OnReadAll(IDataWriter<int> dataWriter);

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
        public abstract void OnWriteAll(IDataReader<int> dataReader);

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
            name = StaticFastObjectRW<T>.StringKeys[ordinal];
        }

        /// <summary>
        /// 获取指定索引处的字段名称。
        /// </summary>
        /// <param name="ordinal">指定索引</param>
        /// <param name="name">返回字段名称</param>
        public void GetKey(int ordinal, out Ps<char> name)
        {
            name = StaticFastObjectRW<T>.UTF16Keys[ordinal];
        }

        /// <summary>
        /// 获取指定索引处的字段名称。
        /// </summary>
        /// <param name="ordinal">指定索引</param>
        /// <param name="name">返回字段名称</param>
        public void GetKey(int ordinal, out Ps<Utf8Byte> name)
        {
            name = StaticFastObjectRW<T>.UTF8Keys[ordinal];
        }

        /// <summary>
        /// 获取该读写器的名称。
        /// </summary>
        /// <returns>返回该读写器的名称</returns>
        public override string ToString() => $"{typeof(FastObjectRW).FullName}<{typeof(T).FullName}>";

        /// <summary>
        /// 读取或设置该读写器的数据源。
        /// </summary>
        public object Content
        {
            get => content;
            set => content = (T)value;
        }

        /// <summary>
        /// 获取该读写器数据源的类型。
        /// </summary>
        public Type ContentType => typeof(T);

        IEnumerable<Ps<char>> IDataRW<Ps<char>>.Keys => StaticFastObjectRW<T>.UTF16Keys;

        IEnumerable<Ps<char>> IDataReader<Ps<char>>.Keys => StaticFastObjectRW<T>.UTF16Keys;

        IEnumerable<Ps<char>> IDataWriter<Ps<char>>.Keys => StaticFastObjectRW<T>.UTF16Keys;

        IEnumerable<Ps<Utf8Byte>> IDataRW<Ps<Utf8Byte>>.Keys => StaticFastObjectRW<T>.UTF8Keys;

        IEnumerable<Ps<Utf8Byte>> IDataReader<Ps<Utf8Byte>>.Keys => StaticFastObjectRW<T>.UTF8Keys;

        IEnumerable<Ps<Utf8Byte>> IDataWriter<Ps<Utf8Byte>>.Keys => StaticFastObjectRW<T>.UTF8Keys;

        IEnumerable<int> IDataRW<int>.Keys => ArrayHelper.CreateLengthIterator(Count);

        IEnumerable<int> IDataReader<int>.Keys => ArrayHelper.CreateLengthIterator(Count);

        IEnumerable<int> IDataWriter<int>.Keys => ArrayHelper.CreateLengthIterator(Count);

        IValueWriter IDataWriter<int>.this[int key] => this[key];

        IValueReader IDataReader<int>.this[int key] => this[key];

        IValueReader IDataReader<string>.this[string key] => this[key];

        IValueWriter IDataWriter<string>.this[string key] => this[key];

        IValueReader IDataReader<Ps<char>>.this[Ps<char> key] => this[key];

        IValueWriter IDataWriter<Ps<char>>.this[Ps<char> key] => this[key];

        IValueReader IDataReader<Ps<Utf8Byte>>.this[Ps<Utf8Byte> key] => this[key];

        IValueWriter IDataWriter<Ps<Utf8Byte>>.this[Ps<Utf8Byte> key] => this[key];
    }
}