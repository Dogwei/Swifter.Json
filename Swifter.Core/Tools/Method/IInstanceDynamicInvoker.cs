using System;

namespace Swifter.Tools
{
    /// <summary>
    /// 第一个参数是 class 类型或是 ref 类型的动态委托将实现此接口。
    /// </summary>
    public interface IInstanceDynamicInvoker
    {
        /// <summary>
        /// 执行此委托。
        /// </summary>
        /// <param name="firstParameter">此委托的第一个参数</param>
        /// <param name="parameters">此委托后续的参数</param>
        /// <returns>执行返回值</returns>
        object Invoke(ref byte firstParameter, object[] parameters);
    }
}