using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 表示 UTF8 专用字节标识。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = sizeof(byte))]
    public struct Utf8Byte
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        public static unsafe implicit operator Utf8Byte(byte value) => Underlying.As<byte, Utf8Byte>(ref value);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>

        public static unsafe implicit operator byte(Utf8Byte value) => Underlying.As<Utf8Byte, byte>(ref value);
    }
}