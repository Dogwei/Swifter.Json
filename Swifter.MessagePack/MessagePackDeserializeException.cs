using System;

namespace Swifter.MessagePack
{
    /// <summary>
    /// MessagePack 反序列化出错时的异常信息。
    /// </summary>
    public class MessagePackDeserializeException : Exception
    {
        /// <summary>
        /// 反序列化出错所在索引。
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 构建实例。
        /// </summary>
        /// <param name="index">反序列化出错所在索引</param>
        public MessagePackDeserializeException(int index) : base("MessagePack deserialize failed.")
        {
            Index = index;
        }
    }
}