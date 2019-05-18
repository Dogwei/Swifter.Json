using System;
using System.Reflection;

namespace Swifter.Tools
{
    internal static class CloneHelper
    {
        public static readonly Func<object, object> FuncMemberwiseClone;

        static CloneHelper()
        {
            FuncMemberwiseClone = MethodHelper.CreateDelegate<Func<object, object>>(
                typeof(object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance),
                SignatureLevels.Cast);
        }
    }
}