using System;
using System.Runtime.InteropServices;

#pragma warning disable 0649

namespace Swifter.Tools
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct FieldDesc
    {
        public readonly IntPtr m_pMTOfEnclosingClass;
        public readonly uint m_dword1;
        public readonly uint m_dword2;

        public uint m_mb => m_dword1 & 0xffffffU;

        public bool m_isStatic => (m_dword1 & 0x1000000) != 0;

        public bool m_isThreadLocal => (m_dword1 & 0x2000000) != 0;

        public bool m_isRVA => (m_dword1 & 0x4000000) != 0;

        public byte m_prot => (byte)((m_dword1 >> 27) & 0x7);

        public bool m_requiresFullMbValue => (m_dword1 & 40000000) != 0;

        public uint m_dwOffset => m_dword2 & 0x7ffffffU;

        public uint m_type => (m_dword2 >> 27) & (0x1f);
    }
}
