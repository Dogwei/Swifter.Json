using System;

namespace Swifter
{
    /// <summary>
    /// 表示断言失败的异常。
    /// </summary>
    public sealed class AssertionFailedException : Exception
    {
        /// <summary>
        /// 初始化默认异常实例。
        /// </summary>
        public AssertionFailedException() : base("Assertion failed!")
        {

        }

        /// <summary>
        /// 初始化具有指定消息文本的异常实例。
        /// </summary>
        /// <param name="message">指定消息文本</param>
        public AssertionFailedException(string message) : base(message)
        {

        }
    }
}