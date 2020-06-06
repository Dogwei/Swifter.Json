using System;

namespace Swifter.RW
{
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