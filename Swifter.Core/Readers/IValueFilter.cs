using Swifter.RW;
using System;

namespace Swifter.Readers
{
    /// <summary>
    /// 数据读取器的值筛选接口。
    /// </summary>
    /// <typeparam name="TKey">键类型。</typeparam>
    public interface IValueFilter<TKey>
    {
        /// <summary>
        /// 值筛选方法。
        /// </summary>
        /// <param name="valueInfo">值信息。</param>
        /// <returns>返回读取或不读取该值。</returns>
        bool Filter(ValueFilterInfo<TKey> valueInfo);
    }

    /// <summary>
    /// 值筛选时的值信息。
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    public sealed class ValueFilterInfo<TKey>
    {
        /// <summary>
        /// 初始化默认实例。
        /// </summary>
        public ValueFilterInfo()
        {
            ValueCopyer = new ValueCopyer();
        }

        /// <summary>
        /// 初始化具有指定值的实例。
        /// </summary>
        /// <param name="key">键的值</param>
        /// <param name="type">值的类型</param>
        /// <param name="valueCopyer">值的读写器</param>
        public ValueFilterInfo(TKey key, Type type, ValueCopyer valueCopyer)
        {
            Key = key;
            Type = type;
            ValueCopyer = valueCopyer;
        }

        /// <summary>
        /// 读取或设置字段名。
        /// </summary>
        public TKey Key;

        /// <summary>
        /// 读取值的定义类型。
        /// </summary>
        public Type Type;

        /// <summary>
        /// 获取值的读写器。
        /// </summary>
        public readonly ValueCopyer ValueCopyer;
    }
}