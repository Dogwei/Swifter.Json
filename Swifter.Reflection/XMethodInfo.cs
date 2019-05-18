using Swifter.Tools;
using System;
using System.Reflection;

namespace Swifter.Reflection
{
    /// <summary>
    /// XMethodInfo 方法信息。
    /// 此方法提供的动态调用都比 .Net 自带的要快很多。
    /// </summary>
    public sealed class XMethodInfo
    {
        /// <summary>T
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
        readonly IntPtr declaringTypeHandle;
        readonly bool declaringTypeIsValueType;

        internal XMethodInfo(MethodInfo methodInfo, XBindingFlags flags)
        {
            MethodInfo = methodInfo;

            Delegate = MethodHelper.CreateDelegate(methodInfo);

            declaringType = methodInfo.DeclaringType;
            declaringTypeHandle = TypeHelper.GetTypeHandle(declaringType);
            declaringTypeIsValueType = declaringType.IsValueType;
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
        /// 动态执行该方法。
        /// </summary>
        /// <param name="obj">类型的实例。如果是静态方法，则指定为 Null</param>
        /// <param name="parameters">方法的参数</param>
        /// <returns>返回该方法的返回值。如果返回值类型为 Void，则返回 Null</returns>
        public object Invoke(object obj, object[] parameters)
        {
            if (Delegate is IInstanceDynamicInvoker instanceDynamicDelegate)
            {
                if (!declaringType.IsInstanceOfType(obj))
                {
                    throw new TargetException(nameof(obj));
                }

                if (declaringTypeIsValueType)
                {
                    return instanceDynamicDelegate.Invoke(ref TypeHelper.Unbox<byte>(obj), parameters);
                }

                return instanceDynamicDelegate.Invoke(ref Unsafe.AsRef<byte>(obj), parameters);
            }

            return Delegate.DynamicInvoke(parameters);
        }

        /// <summary>
        /// 动态执行该方法。
        /// </summary>
        /// <param name="typedRef">类型的实例的引用。如果是静态方法，则指定为 Null</param>
        /// <param name="parameters">方法的参数</param>
        /// <returns>返回该方法的返回值。如果返回值类型为 Void，则返回 Null</returns>
        public object Invoke(TypedReference typedRef, object[] parameters)
        {
            if (Delegate is IInstanceDynamicInvoker instanceDynamicDelegate)
            {
                if (declaringTypeHandle != TypeHelper.GetTypeHandle(__reftype(typedRef)))
                {
                    throw new TargetException(nameof(typedRef));
                }

                if (declaringTypeIsValueType)
                {
                    return instanceDynamicDelegate.Invoke(ref Unsafe.AsRef<byte>(typedRef), parameters);
                }

                return instanceDynamicDelegate.Invoke(ref Unsafe.AsRef<byte>(typedRef), parameters);
            }

            return Delegate.DynamicInvoke(parameters);
        }

        /// <summary>
        /// 动态执行该静态方法。
        /// </summary>
        /// <param name="parameters">方法的参数</param>
        /// <returns>返回该方法的返回值。如果返回值类型为 Void，则返回 Null</returns>
        public object Invoke(object[] parameters)
        {
            return Delegate.DynamicInvoke(parameters);
        }
    }
}