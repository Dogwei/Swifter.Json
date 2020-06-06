using System;

namespace Swifter.Tools
{
    /// <summary>
    /// 转换器工厂接口。
    /// </summary>
    public interface IConverterFactory
    {
        /// <summary>
        /// 获取转换器实例，实例应实现 <see cref="IXConverter{TSource, TDestination}"/> 接口。
        /// </summary>
        /// <param name="sourceType">原类型</param>
        /// <param name="destinationType">目标类型</param>
        /// <returns>返回转换器实例</returns>
        object GetConverter(Type sourceType, Type destinationType);
    }
}