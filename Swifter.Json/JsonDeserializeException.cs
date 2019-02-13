using Swifter.Tools;
using System;
using System.ComponentModel;

namespace Swifter.Json
{
    /// <summary>
    /// Json 反序列化出错时的异常信息。
    /// </summary>
    public sealed class JsonDeserializeException : Exception
    {
        /// <summary>
        /// 反序列化出错所在行。
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// 反序列化出错所在列。
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// 反序列化出错所在索引。
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 导致反序列化出错的文本。
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 获取异常消息。
        /// </summary>
        public override string Message
        {
            get
            {
                return StringHelper.Format("Json Deserialize Failed. Index : {0}, Line: {1}, Column : {2}, Text : {3}.", Index.ToString(), Line.ToString(), Column.ToString(), Text);
            }
        }
    }
}