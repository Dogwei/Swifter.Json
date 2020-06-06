using System;

namespace Swifter.Json
{
    /// <summary>
    /// Json 反序列化出错时的异常信息。
    /// </summary>
    public sealed class JsonDeserializeException : Exception
    {
        /// <summary>
        /// 构建实例。
        /// </summary>
        /// <param name="index">反序列化出错所在索引</param>
        public JsonDeserializeException(int index) : base("Json deserialize failed.")
        {
            Index = index;
        }

        /// <summary>
        /// 反序列化出错所在索引。
        /// </summary>
        public int Index { get; }
    }
}