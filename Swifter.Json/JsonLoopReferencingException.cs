using System;

namespace Swifter.Json
{
    /// <summary>
    /// Json 序列化时出现循环引用引发的异常。
    /// </summary>
    public sealed class JsonLoopReferencingException : Exception
    {
        /// <summary>
        /// 初始化实例
        /// </summary>
        internal JsonLoopReferencingException(TargetPathInfo ref1, TargetPathInfo ref2) 
            : base(string.Format("Json serializating members '{0}' and '{1}' loop referencing.", ref1, ref2))
        {
        }
    }
}