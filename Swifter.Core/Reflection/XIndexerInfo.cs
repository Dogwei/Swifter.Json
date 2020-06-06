using Swifter.Tools;
using System;
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
        public readonly Delegate GetValueDelegate;

        /// <summary>
        /// 获取该索引器的 set 方法的委托。
        /// </summary>
        public readonly Delegate SetValueDelegate;

        XIndexerInfo(PropertyInfo propertyInfo, XBindingFlags flags)
        {
            Flags = flags;

            PropertyInfo = propertyInfo;

            if (propertyInfo.GetGetMethod((flags & XBindingFlags.NonPublic) != 0) is var getMethod)
            {
                GetValueDelegate = MethodHelper.CreateDelegate(getMethod);
            }

            if (propertyInfo.GetSetMethod((flags & XBindingFlags.NonPublic) != 0) is var setMethod)
            {
                SetValueDelegate = MethodHelper.CreateDelegate(setMethod);
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
            if (GetValueDelegate is null)
            {
                if ((Flags & XBindingFlags.RWCannotGetException) != 0)
                {
                    ThrowMissingMethodException("Indexer", PropertyInfo.DeclaringType, PropertyInfo, "get");
                }

                return null;
            }

            if (!PropertyInfo.DeclaringType.IsInstanceOfType(obj))
            {
                ThrowTargetException(nameof(obj), PropertyInfo.DeclaringType);
            }

            return GetValueDelegate.DynamicInvoke(ArrayHelper.Merge(obj, parameters));
        }

        /// <summary>
        /// 获取静态索引器指定参数的值。
        /// </summary>
        /// <param name="parameters">索引器的参数</param>
        /// <returns>返回该值</returns>
        public object GetValue(object[] parameters)
        {
            if (GetValueDelegate is null)
            {
                if ((Flags & XBindingFlags.RWCannotGetException) != 0)
                {
                    ThrowMissingMethodException("Indexer", PropertyInfo.DeclaringType, PropertyInfo, "get");
                }

                return null;
            }

            return GetValueDelegate.DynamicInvoke(parameters);
        }

        /// <summary>
        /// 设置该索引器指定参数的值。
        /// </summary>
        /// <param name="obj">类型的实例。如果该索引器是静态的，则指定为 Null</param>
        /// <param name="parameters">索引器的参数</param>
        /// <param name="value">值</param>
        public void SetValue(object obj, object[] parameters, object value)
        {
            if (SetValueDelegate is null)
            {
                if ((Flags & XBindingFlags.RWCannotSetException) != 0)
                {
                    ThrowMissingMethodException("Indexer", PropertyInfo.DeclaringType, PropertyInfo, "set");
                }

                return;
            }

            if (!PropertyInfo.DeclaringType.IsInstanceOfType(obj))
            {
                ThrowTargetException(nameof(obj), PropertyInfo.DeclaringType);
            }

            SetValueDelegate.DynamicInvoke(ArrayHelper.Merge(obj, parameters, value));
        }

        /// <summary>
        /// 设置静态索引器指定参数的值。
        /// </summary>
        /// <param name="parameters">索引器的参数</param>
        /// <param name="value">值</param>
        public void SetValue(object[] parameters, object value)
        {
            if (SetValueDelegate is null)
            {
                if ((Flags & XBindingFlags.RWCannotSetException) != 0)
                {
                    ThrowMissingMethodException("Indexer", PropertyInfo.DeclaringType, PropertyInfo, "set");
                }

                return;
            }

            SetValueDelegate.DynamicInvoke(ArrayHelper.Merge(parameters, value));
        }
    }
}