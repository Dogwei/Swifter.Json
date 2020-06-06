using Swifter.Tools;
using System;
using System.Buffers;
using System.Reflection;

namespace Swifter.Reflection
{
    /// <summary>
    /// XMethodInfo 方法信息。
    /// 此方法提供的动态调用都比 .Net 自带的要快很多。
    /// </summary>
    public sealed class XMethodInfo
    {
        /// <summary>
        /// 创建 XMethodInfo 方法信息。
        /// </summary>
        /// <param name="methodInfo">.Net 自带 MethodInfo 方法信息</param>
        /// <param name="flags">绑定标识</param>
        /// <returns>返回一个 XMethodInfo 方法信息。</returns>
        public static XMethodInfo Create(MethodInfo methodInfo, XBindingFlags flags)
        {
            return new XMethodInfo(methodInfo, flags);
        }

        readonly Type declaringType;

        internal XMethodInfo(MethodInfo methodInfo, XBindingFlags flags)
        {
            MethodInfo = methodInfo;

            Delegate = MethodHelper.CreateDelegate(methodInfo);

            declaringType = methodInfo.DeclaringType;
        }

        /// <summary>
        /// 获取 .Net 自带的 MethodInfo 方法信息。
        /// </summary>
        public MethodInfo MethodInfo { get; private set; }

        /// <summary>
        /// 获取该方法的委托。
        /// 该委托比普通的委托占用更大的内存，但动态执行的效率更高。
        /// </summary>
        public Delegate Delegate { get; private set; }

        /// <summary>
        /// 以实例方式动态执行方法。
        /// </summary>
        /// <param name="obj">调用实例</param>
        /// <param name="parameters">方法的参数</param>
        /// <returns>返回该方法的返回值。如果返回值类型为 Void，则返回 Null</returns>
        public object Invoke(object obj, object[] parameters)
        {
            if (!declaringType.IsInstanceOfType(obj))
            {
                throw new TargetException(nameof(obj));
            }

            return Delegate.DynamicInvoke(ArrayHelper.Merge(obj, parameters));
        }

        /// <summary>
        /// 以静态方式动态执行方法。
        /// </summary>
        /// <param name="parameters">方法的参数</param>
        /// <returns>返回该方法的返回值。如果返回值类型为 Void，则返回 Null</returns>
        public object Invoke(object[] parameters)
        {
            return Delegate.DynamicInvoke(parameters);
        }
    }
}