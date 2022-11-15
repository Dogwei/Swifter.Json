using Swifter.RW;
using System;

namespace Swifter.Json
{
#if !NO_OPTIONS
    /// <summary>
    /// Json 序列化时出现循环引用引发的异常信息。
    /// </summary>
    public sealed class JsonLoopReferencingException : Exception
    {
        /// <summary>
        /// 出现循环引用的对象。
        /// </summary>
        public object LoopingObject { get; }

        /// <summary>
        /// 构建实例
        /// </summary>
        internal JsonLoopReferencingException(RWPath ref1, RWPath ref2, object loopingObject) 
            : base($"Json serializating members '{ref1}' and '{ref2}' loop referencing.")
        {
            LoopingObject = loopingObject;
        }
    }
#endif
}