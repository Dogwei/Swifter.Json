using System;

namespace Swifter.Json
{
    /// <summary>
    /// JSON 序列化或反序列化时结构深度超出最大深度的异常。
    /// </summary>
    public sealed class JsonOutOfDepthException : Exception
    {
        /// <summary>
        /// 构建实例
        /// </summary>
        public JsonOutOfDepthException() : base("Json struct depth out of the max depth.")
        {
        }
    }
}