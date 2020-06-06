using System;

namespace Swifter.RW
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
}