#if !NET5_0_OR_GREATER

using System;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// 方法内变量不使用 .locals init 标识。
    /// </summary>
    [AttributeUsage(AttributeTargets.Module | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event | AttributeTargets.Interface, Inherited = false)]
    public sealed class SkipLocalsInitAttribute : Attribute
    {
    }
}
#endif