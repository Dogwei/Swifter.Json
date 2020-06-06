using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using static Swifter.Tools.MethodHelper;
using static Swifter.Tools.StringHelper;

namespace Swifter.Tools
{
    internal sealed class StringDifferenceComparer : IDifferenceComparer<string>
    {
        public static readonly MethodInfo GetLengthMethod = typeof(string).GetProperty(nameof(string.Length)).GetGetMethod(true);
        public static readonly MethodInfo CharAtMethod = typeof(string).GetProperties(BindingFlags.Public | BindingFlags.Instance).First(
            item => item.PropertyType == typeof(char) && item.GetIndexParameters().Length == 1 && item.GetIndexParameters()[0].ParameterType == typeof(int))
            .GetGetMethod(true);

        public readonly bool IgnoreCase;

        public StringDifferenceComparer(bool ignoreCase)
        {
            IgnoreCase = ignoreCase;
        }

        public int ElementAt(string str, int index)
        {
            if (IgnoreCase)
            {
                return ToLower(str[index]);
            }

            return str[index];
        }

        public void EmitElementAt(ILGenerator ilGen)
        {
            ilGen.Call(CharAtMethod);

            if (IgnoreCase)
            {
                ilGen.Call(MethodOf<char, char>(ToLower));
            }
        }

        public void EmitEquals(ILGenerator ilGen)
        {
            if (IgnoreCase)
            {
                ilGen.Call(MethodOf<string, string, bool>(EqualsWithIgnoreCase));
            }
            else
            {
                ilGen.Call(MethodOf<string, string, bool>(StringHelper.Equals));
            }
        }

        public void EmitGetHashCode(ILGenerator ilGen)
        {
            if (IgnoreCase)
            {
                ilGen.Call(MethodOf<string, int>(GetHashCodeWithIgnoreCase));
            }
            else
            {
                ilGen.Call(MethodOf<string, int>(StringHelper.GetHashCode));
            }
        }

        public void EmitGetLength(ILGenerator ilGen)
        {
            ilGen.Call(GetLengthMethod);
        }

        public bool Equals(string x, string y)
        {
            if (IgnoreCase)
            {
                return EqualsWithIgnoreCase(x, y);
            }

            return StringHelper.Equals(x, y);
        }

        public int GetHashCode(string str)
        {
            if (IgnoreCase)
            {
                return GetHashCodeWithIgnoreCase(str);
            }

            return StringHelper.GetHashCode(str);
        }

        public int GetLength(string str)
        {
            return str.Length;
        }
    }
}