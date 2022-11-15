using System.Reflection;

namespace Swifter.Tools
{
    /// <summary>
    /// 类型转换器工厂接口。
    /// </summary>
    public interface IXConverterFactory
    {
        /// <summary>
        /// 获取类转换函数。
        /// </summary>
        /// <typeparam name="TSource">原类型</typeparam>
        /// <typeparam name="TDestination">目标类型</typeparam>
        /// <returns>返回转换函数。</returns>
        MethodBase? GetConverter<TSource, TDestination>();
    }
}