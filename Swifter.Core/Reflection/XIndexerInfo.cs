using Swifter.Tools;
using System.Reflection;

namespace Swifter.Reflection
{
    /// <summary>
    /// XIndexerInfo 索引器信息。
    /// </summary>
    public sealed class XIndexerInfo
    {
        /// <summary>
        /// 创建索引器信息。
        /// </summary>
        /// <param name="propertyInfo">.Net 自带的索引器信息</param>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回 XIndexerInfo 索引器信息</returns>
        public static XIndexerInfo Create(PropertyInfo propertyInfo, XBindingFlags flags = XBindingFlags.Indexer)
        {
            return new XIndexerInfo(propertyInfo, flags);
        }

        /// <summary>
        /// 获取 .Net 自带的索引器信息。
        /// </summary>
        public readonly PropertyInfo PropertyInfo;

        /// <summary>
        /// 获取该索引器的参数信息集合。
        /// </summary>
        public readonly XMethodParameters Parameters;

        XIndexerInfo(PropertyInfo propertyInfo, XBindingFlags _)
        {
            PropertyInfo = propertyInfo;
            Parameters = new(propertyInfo.GetIndexParameters());
        }

        /// <summary>
        /// 获取该索引器指定参数的值。
        /// </summary>
        /// <param name="obj">类型的实例。如果该索引器是静态的，则指定为 Null</param>
        /// <param name="parameters">索引器的参数</param>
        /// <returns>返回该值</returns>
        public object? GetValue(object? obj, object?[]? parameters)
        {
            return PropertyInfo.GetValue(obj, parameters);
        }

        /// <summary>
        /// 设置该索引器指定参数的值。
        /// </summary>
        /// <param name="obj">类型的实例。如果该索引器是静态的，则指定为 Null</param>
        /// <param name="parameters">索引器的参数</param>
        /// <param name="value">值</param>
        public void SetValue(object? obj, object?[]? parameters, object? value)
        {
            PropertyInfo.SetValue(obj, value, parameters);
        }
    }
}