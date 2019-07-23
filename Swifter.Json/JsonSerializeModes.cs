using System.Runtime.InteropServices;

namespace Swifter.Json
{
    static class JsonSerializeModes
    {
        public struct SimpleMode
        {
        }
        
        public struct ComplexMode
        {
            public string IndentedChars;
            public string LineBreakChars;
            public string MiddleChars;
            
            public bool IsInArray;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct ReferenceMode
        {
            [FieldOffset(0)]
            public ComplexMode DefaultMode;

            [FieldOffset(40)]
            public JsonReferenceWriter References;
        }
    }
}