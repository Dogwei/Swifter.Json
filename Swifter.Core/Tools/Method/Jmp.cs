using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Jmp
    {
        public readonly byte Command;
        public readonly int Offset;

        public unsafe Jmp(int offset)
        {
            Command = 0xe9;
            Offset = offset;
        }
    }
}