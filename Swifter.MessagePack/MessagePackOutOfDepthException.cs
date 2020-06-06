using System;

namespace Swifter.MessagePack
{
    /// <summary>
    /// MessagePack 序列化或反序列化时结构深度超出最大深度的异常。
    /// </summary>
    public sealed class MessagePackOutOfDepthException : Exception
    {
        /// <summary>
        /// 构建实例
        /// </summary>
        public MessagePackOutOfDepthException() : base("MessagePack struct depth out of the max depth.")
        {
        }
    }
}
