using Swifter.Readers;
using Swifter.Tools;
using Swifter.Writers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using static Swifter.RW.FastObjectRW;

namespace Swifter.RW
{
    /// <summary>
    /// FastObjectRW 基于 Emit 实现的几乎完美效率的对象读写器。
    /// </summary>
    public static class FastObjectRW
    {
        internal static readonly TypeCache<FastObjectRWOptions> TypeOptions;

        internal static readonly FastObjectRWOptions Initialized = (FastObjectRWOptions)0x10000000;

        static FastObjectRW()
        {
            TypeOptions = new TypeCache<FastObjectRWOptions>();

            SetCurrentOptions(typeof(ValueType), DefaultOptions | FastObjectRWOptions.Field);
        }

        internal static FastObjectRWOptions GetCurrentOptions(Type type)
        {
            if (TypeOptions.TryGetValue(type, out var value))
            {
                return value;
            }

            return TypeOptions.LockGetOrAdd(type, t =>
            {
                if ((t.BaseType ?? typeof(object)) == typeof(object))
                {
                    return DefaultOptions;
                }

                return GetCurrentOptions(t.BaseType);
            });
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
            FastObjectRWOptions.NotFoundException |
            FastObjectRWOptions.CannotGetException |
            FastObjectRWOptions.CannotSetException |
            FastObjectRWOptions.BasicTypeDirectCallMethod |
            FastObjectRWOptions.Property |
            FastObjectRWOptions.InheritedMembers;
    }

    /// <summary>
    /// FastObjectRW 基于 Emit 实现的几乎完美效率的对象读写器。
    /// </summary>
    /// <typeparam name="T">数据源对象的类型</typeparam>
    public abstract partial class FastObjectRW<T> : IDataRW<string>, IDirectContent, IInitialize<T>, IId64DataRW<char>, ICloneable
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
        internal protected static object GetValueInterfaceInstance(int index)
        {
            return StaticFastObjectRW<T>.Fields[index].InterfaceInstance;
        }

        /// <summary>
        /// 数据源，此字段提供给 Emit 实现类使用。
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public T content;

        /// <summary>
        /// 获取数据源。
        /// </summary>
        public T Content => content;

        /// <summary>
        /// 调用默认无参的构造函数初始化数据源。
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// 设置数据源。
        /// </summary>
        /// <param name="content">数据源</param>
        public void Initialize(T content) => this.content = content;

        /// <summary>
        /// 调用默认无参的构造函数初始化数据源。
        /// </summary>
        /// <param name="capacity">不使用此参数</param>
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
        /// 对数据源中的所有成员进行筛选，并将满足筛选的结果写入到数据写入器中。
        /// </summary>
        /// <param name="dataWriter">数据写入器</param>
        /// <param name="valueFilter">值筛选器</param>
        public abstract void OnReadAll(IDataWriter<string> dataWriter, IValueFilter<string> valueFilter);

        /// <summary>
        /// 获取指定名称的成员的值读写器。
        /// </summary>
        /// <param name="key">指定名称</param>
        /// <returns>返回值读写器</returns>
        public FastFieldRW this[string key] => new FastFieldRW(this, this.GetId64Ex(key));

        /// <summary>
        /// 获取该类型所有的成员。
        /// </summary>
        public IEnumerable<string> Keys => ArrayHelper.CreateArrayIterator(StaticFastObjectRW<T>.Keys);

        /// <summary>
        /// 获取该类型所有的成员的数量。
        /// </summary>
        public int Count => StaticFastObjectRW<T>.Keys.Length;

        object IDirectContent.DirectContent { get => content; set => content = (T)value; }

        /// <summary>
        /// 获取数据源的引用根，全局唯一。如果数据源是值类型或 Null，则返回 Null。
        /// </summary>
        public object ReferenceToken => TypeInfo<T>.IsValueType ? null : (object)content;

        IValueRW IDataRW<string>.this[string key] => this[key];

        IValueReader IDataReader<string>.this[string key] => this[key];

        IValueWriter IDataWriter<string>.this[string key] => this[key];

        /// <summary>
        /// 获取该读写器的名称。
        /// </summary>
        /// <returns>返回该读写器的名称</returns>
        public override string ToString() => $"{nameof(FastObjectRW)}<{typeof(T).FullName}>";

        /// <summary>
        /// 获取字段名称的 Id64 值。
        /// </summary>
        /// <param name="firstSymbol">字段名称第一个字符的引用。</param>
        /// <param name="length">字段名称长度</param>
        /// <returns>返回 Id64 值</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract long GetId64(ref char firstSymbol, int length);

        /// <summary>
        /// 获取字段名称的 Id64 值。
        /// </summary>
        /// <param name="symbols">字段名称</param>
        /// <returns>返回 Id64 值</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public long GetId64(IEnumerable<char> symbols)
        {
            if ((StaticFastObjectRW<T>.Options & FastObjectRWOptions.IndexId64) != 0)
            {
                var chars = symbols.ToArray();

                return GetId64(ref chars[0], chars.Length);
            }

            return StaticFastObjectRW<T>.ComputeId64(symbols);
        }

        /// <summary>
        /// 获取字段 Id64 值为键的值到值写入器中。
        /// </summary>
        /// <param name="id64">Id64 值</param>
        /// <param name="valueWriter">值写入器</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract void OnReadValue(long id64, IValueWriter valueWriter);

        /// <summary>
        /// 从值读取器中读取值设置到字段 Id64 值为键的字段中。
        /// </summary>
        /// <param name="id64">Id64 值</param>
        /// <param name="valueReader">值读取器</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public abstract void OnWriteValue(long id64, IValueReader valueReader);

        object ICloneable.Clone() => Clone();

        /// <summary>
        /// 返回一个浅层克隆实例。
        /// </summary>
        /// <returns>返回一个浅层克隆实例</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public FastObjectRW<T> Clone() => (FastObjectRW<T>)MemberwiseClone();
    }
}