using Swifter.RW;
using System;
using System.ComponentModel;

namespace Swifter.MessagePack
{
    /// <summary>
    /// MessagePack 事件委托。
    /// </summary>
    /// <typeparam name="TEventArgs">事件类型</typeparam>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void MessagePackEventHandler<TEventArgs>(object sender, ref TEventArgs e) where TEventArgs : struct;

    /// <summary>
    /// MessagePack 序列化元素时事件参数。
    /// </summary>
    /// <typeparam name="TKey">元素键类型</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct MessagePackFilteringEventArgs<TKey>
    {
        /// <summary>
        /// 当前 MessagePack 写入器。
        /// </summary>
        public readonly IMessagePackWriter JsonWriter;
        
        /// <summary>
        /// 当前值信息。
        /// </summary>
        public readonly ValueFilterInfo<TKey> ValueInfo;

        /// <summary>
        /// 是否写入该值。
        /// </summary>
        public bool Result;

        internal MessagePackFilteringEventArgs(IMessagePackWriter jsonWriter, ValueFilterInfo<TKey> valueInfo, bool result)
        {
            JsonWriter = jsonWriter;
            ValueInfo = valueInfo;
            Result = result;
        }
    }
}