using System.Runtime.InteropServices;

namespace Swifter.Tools
{
    [StructLayout(LayoutKind.Sequential)]
    struct DecimalStruct
    {
        private const int SignMask = unchecked((int)0x80000000);
        private const int ScaleMask = 0x00FF0000;
        private const int ScaleShift = 16;

#pragma warning disable IDE0044

        private int flags;
        private int hi;
        private int lo;
        private int mid;

        public int Scale
        {
            get => (flags & ScaleMask) >> ScaleShift;
            set => flags = (flags & (~ScaleMask)) | ((value << ScaleShift) & ScaleMask);
        }

        public int Sign
        {
            get => flags & SignMask;
            set => flags = value == 0 ? flags & (~SignMask) : flags | SignMask;
        }

        public unsafe void GetBits(int* pBits)
        {
            pBits[0] = lo;
            pBits[2] = hi;
            pBits[1] = mid;
        }

        public unsafe void SetBits(int* pBits)
        {
            mid = pBits[1];
            hi = pBits[2];
            lo = pBits[0];
            flags = 0;
        }
    }
}