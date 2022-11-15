using Swifter.Tools;
using System;
using System.Reflection;

namespace Swifter.Reflection
{
    /// <summary>
    /// 方法信息。
    /// </summary>
    public sealed class XMethodInfo
    {
        /// <summary>
        /// 创建方法信息。
        /// </summary>
        /// <param name="methodInfo">.Net 自带方法信息</param>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回一个方法信息。</returns>
        public static XMethodInfo Create(MethodInfo methodInfo, XBindingFlags flags)
        {
            return new XMethodInfo(methodInfo, flags);
        }

        /// <summary>
        /// 获取 .Net 自带的方法信息。
        /// </summary>
        public readonly MethodInfo MethodInfo;

        /// <summary>
        /// 获取该方法的参数信息集合。
        /// </summary>
        public readonly XMethodParameters Parameters;

        /// <summary>
        /// 获取该方法的名称。
        /// </summary>
        public string Name => MethodInfo.Name;

        XMethodInfo(MethodInfo methodInfo, XBindingFlags _)
        {
            MethodInfo = methodInfo;
            Parameters = new(methodInfo.GetParameters());
        }

        /// <summary>
        /// 执行此方法。
        /// </summary>
        /// <param name="obj">调用实例；如果是静态方法，则置为 <see langword="null"/></param>
        /// <param name="parameters">方法的参数</param>
        /// <returns>返回返回值。如果返回值类型为 <see cref="void"/>，则返回 <see langword="null"/></returns>
        public unsafe object? Invoke(object? obj, object?[]? parameters)
        {
            return MethodInfo.Invoke(obj, parameters);
        }
    }
}