using System;
using System.Runtime.InteropServices;

#pragma warning disable 0649

namespace Swifter.Tools
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct FieldHandleStruct
    {
        public readonly IntPtr Handle;
        public readonly ushort Index;
        public readonly ushort Flag;
        public readonly ushort Offset;
    }
}
