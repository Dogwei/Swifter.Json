﻿using Swifter.RW;
using System;
using System.ComponentModel;

namespace Swifter.Json
{
#if !NO_OPTIONS
    /// <summary>
    /// JSON 事件委托。
    /// </summary>
    /// <typeparam name="TEventArgs">事件类型</typeparam>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void JsonEventHandler<TEventArgs>(object sender, ref TEventArgs e) where TEventArgs : struct;

    /// <summary>
    /// JSON 序列化元素时事件参数。
    /// </summary>
    /// <typeparam name="TKey">元素键类型</typeparam>
    [EditorBrowsable(EditorBrowsableState.Never)] // 为了更方便在 VS 中键入 JsonFormatter
    public struct JsonFilteringEventArgs<TKey> where TKey : notnull
    {
        /// <summary>
        /// 当前 Json 写入器。
        /// </summary>
        public readonly JsonSerializer JsonWriter;

        /// <summary>
        /// 当前值信息。
        /// </summary>
        public readonly ValueFilterInfo<TKey> ValueInfo;

        /// <summary>
        /// 是否写入该值。
        /// </summary>
        public bool Result;

        internal JsonFilteringEventArgs(JsonSerializer jsonWriter, ValueFilterInfo<TKey> valueInfo, bool result)
        {
            JsonWriter = jsonWriter;
            ValueInfo = valueInfo;
            Result = result;
        }
    }

#endif
}