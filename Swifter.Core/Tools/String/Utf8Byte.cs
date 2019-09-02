using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    /// <summary>
    /// 表示 UTF8 专用字节标识。
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = sizeof(byte))]
    public struct Utf8Byte { }
}