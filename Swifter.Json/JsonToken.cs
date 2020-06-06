namespace Swifter.Json
{
    /// <summary>
    /// 表示 JSON 值的 Token。
    /// </summary>
    public enum JsonToken
    {
        /// <summary>
        /// 表示一个键值对值。
        /// </summary>
        Object,
        /// <summary>
        /// 表示一个数组值。
        /// </summary>
        Array,
        /// <summary>
        /// 表示一个 Boolean 值，包括 True 和 False。
        /// </summary>
        Boolean,
        /// <summary>
        /// 表示一个空值，包括 Null 和 Undefined。
        /// </summary>
        Null,
        /// <summary>
        /// 表示一个数值，包括整数和浮点数。
        /// </summary>
        Number,
        /// <summary>
        /// 表示一个字符串值，包括双引号和单引号。
        /// </summary>
        String,
        /// <summary>
        /// 这不表示一个值，而是表示一个 JSON 的结尾。
        /// </summary>
        End,
        /// <summary>
        /// 其他值，非上述的任何其他值。
        /// </summary>
        Other
    }
}
