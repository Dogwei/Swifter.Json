using System;
using System.Data;

#pragma warning disable

namespace Swifter.RW
{
    /// <summary>
    /// 值筛选时的值信息。
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    public sealed class ValueFilterInfo<TKey> where TKey : notnull
    {
        /// <summary>
        /// 初始化默认实例。
        /// </summary>
        /// <param name="dataReader">源数据读取器</param>
        public ValueFilterInfo(IDataReader<TKey>? dataReader = null)
        {
            DataReader = dataReader;

            ValueCopyer = new ValueCopyer();
        }

        public readonly IDataReader<TKey>? DataReader;

        /// <summary>
        /// 读取或设置字段名。
        /// </summary>
        public TKey Key;

        /// <summary>
        /// 获取值的读写器。
        /// </summary>
        public readonly ValueCopyer ValueCopyer;
    }
}