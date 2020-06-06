
#pragma warning disable 0649

using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    [StructLayout(LayoutKind.Sequential, Size = sizeof(char) * 3)]
    internal struct ThreeChar
    {
        public char char1;
        public char char2;
        public char char3;
    }
}
