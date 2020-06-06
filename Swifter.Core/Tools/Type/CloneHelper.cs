using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Swifter.Tools
{
    internal static unsafe class CloneHelper
    {
        public static readonly IntPtr p_MemberwiseClone;

        static CloneHelper()
        {
            p_MemberwiseClone = typeof(object).GetMethod(nameof(MemberwiseClone), BindingFlags.NonPublic | BindingFlags.Instance).MethodHandle.GetFunctionPointer();
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static object MemberwiseClone(object obj)
        {
            return Underlying.Invoke<object, object>(obj, p_MemberwiseClone);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static string CloneString(string str)
        {
            fixed (char* pStr = str)
                return StringHelper.ToString(pStr, str.Length);
        }

        [MethodImpl(VersionDifferences.AggressiveInlining)]
        public static Array CloneArray(Array array)
        {
            return Underlying.As<Array>(array.Clone());
        }
    }
}