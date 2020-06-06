using System.ComponentModel;

namespace Swifter.Tools
{
    /// <summary>
    /// XConvert 实现接口。
    /// </summary>
    /// <typeparam name="TSource">原类型</typeparam>
    /// <typeparam name="TDestination">目标类型</typeparam>
    public interface IXConverter<TSource, TDestination>
    {
        /// <summary>
        /// 将原类型的值转换为目标类型的值。
        /// </summary>
        /// <param name="value">原类型的值</param>
        /// <returns>返回目标类型的值</returns>
        TDestination Convert(TSource value);
    }
}
