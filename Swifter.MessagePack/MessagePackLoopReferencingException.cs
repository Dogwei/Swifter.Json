using System;

namespace Swifter.MessagePack
{
    /// <summary>
    /// MessagePack 序列化时出现循环引用引发的异常信息。
    /// </summary>
    public sealed class MessagePackLoopReferencingException : Exception
    {
        /// <summary>
        /// 出现循环引用的对象。
        /// </summary>
        public object LoopingObject { get; }

        /// <summary>
        /// 构建实例
        /// </summary>
        public MessagePackLoopReferencingException(object loopingObject)
        {
            LoopingObject = loopingObject;
        }
    }
}