using System;
using System.Runtime.InteropServices;

#pragma warning disable 0649

namespace Swifter.Tools
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct EEClass
    {
        public IntPtr m_pGuidInfo;
        public ushort m_rpOptionalFields;
        public unsafe MethodTable* m_pMethodTable;
        public IntPtr m_pFieldDescList;
        public IntPtr m_pChunks;
        public IntPtr m_cbNativeSize;
        public IntPtr m_pccwTemplate;
        public uint m_dwAttrClass;
        public uint m_VMFlags;
        public byte m_NormType;
        public byte m_fFieldsArePacked;
        public byte m_cbFixedEEClassFields;
        public byte m_cbBaseSizePadding;
    }
}
