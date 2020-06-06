using System;
using System.Runtime.InteropServices;

#pragma warning disable 0649

namespace Swifter.Tools
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct MethodTable
    {
        public uint m_dwFlags;
        public uint m_BaseSize;
        public ushort m_wFlags2;
        public ushort m_wToken;
        public ushort m_wNumVirtuals;
        public ushort m_wNumInterfaces;
        public unsafe MethodTable* m_pParentMethodTable;
        public IntPtr m_pLoaderModule;
        public IntPtr m_pWriteableData;
        public unsafe EEClass* m_pEEClass;
        public IntPtr m_pPerInstInfo;
        public IntPtr m_pInterfaceMap;
        public IntPtr m_pMultipurposeSlot2;
    }
}
