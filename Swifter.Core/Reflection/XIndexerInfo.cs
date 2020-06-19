using Swifter.Tools;
using System.Reflection;

using static Swifter.Reflection.ThrowHelpers;

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

        readonly XBindingFlags Flags;

        /// <summary>
        /// 获取 .Net 自带的索引器信息。
        /// </summary>
        public readonly PropertyInfo PropertyInfo;

        /// <summary>
        /// 获取该索引器的 get 方法的委托。
        /// </summary>
        public readonly XMethodInfo GetValueMethod;

        /// <summary>
        /// 获取该索引器的 set 方法的委托。
        /// </summary>
        public readonly XMethodInfo SetValueMethod;

        XIndexerInfo(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            Flags = flags;

            PropertyInfo = propertyInfo;

            if (propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0) is var getMethod)
            {
                GetValueMethod = XMethodInfo.Create(getMethod, flags);
            }

            if (propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0) is var setMethod)
            {
                SetValueMethod = XMethodInfo.Create(setMethod, flags);
            }
        }

        /// <summary>
        /// 获取该索引器指定参数的值。
        /// </summary>
        /// <param name="obj">类型的实例。如果该索引器是静态的，则指定为 Null</param>
        /// <param name="parameters">索引器的参数</param>
        /// <returns>返回该值</returns>
        public object GetValue(object obj, object[] parameters)
        {
            if (GetValueMethod is null)
            {
                ThrowMissingMethodException("Indexer", PropertyInfo.DeclaringType, PropertyInfo, "get");
            }

            return GetValueMethod.Invoke(obj, parameters);
        }

        /// <summary>
        /// 获取静态索引器指定参数的值。
        /// </summary>
        /// <param name="parameters">索引器的参数</param>
        /// <returns>返回该值</returns>
        public object GetValue(object[] parameters)
        {
            if (GetValueMethod is null)
            {
                ThrowMissingMethodException("Indexer", PropertyInfo.DeclaringType, PropertyInfo, "get");
            }

            return GetValueMethod.Invoke(parameters);
        }

        /// <summary>
        /// 设置该索引器指定参数的值。
        /// </summary>
        /// <param name="obj">类型的实例。如果该索引器是静态的，则指定为 Null</param>
        /// <param name="parameters">索引器的参数</param>
        /// <param name="value">值</param>
        public void SetValue(object obj, object[] parameters, object value)
        {
            if (GetValueMethod is null)
            {
                ThrowMissingMethodException("Indexer", PropertyInfo.DeclaringType, PropertyInfo, "set");
            }

            SetValueMethod.Invoke(ArrayHelper.Merge(obj, parameters, value));
        }

        /// <summary>
        /// 设置静态索引器指定参数的值。
        /// </summary>
        /// <param name="parameters">索引器的参数</param>
        /// <param name="value">值</param>
        public void SetValue(object[] parameters, object value)
        {
            if (GetValueMethod is null)
            {
                ThrowMissingMethodException("Indexer", PropertyInfo.DeclaringType, PropertyInfo, "set");
            }

            SetValueMethod.Invoke(ArrayHelper.Merge(parameters, value));
        }
    }
}