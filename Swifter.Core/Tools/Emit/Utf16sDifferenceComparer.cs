using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using static Swifter.Tools.MethodHelper;
using static Swifter.Tools.StringHelper;

namespace Swifter.Tools
{
    internal sealed unsafe class Utf16sDifferenceComparer : IDifferenceComparer<Ps<char>>
    {
        public readonly bool IgnoreCase;

        public Utf16sDifferenceComparer(bool ignoreCase)
        {
            IgnoreCase = ignoreCase;
        }

        public int ElementAt(Ps<char> str, int index)
        {
            if (IgnoreCase)
            {
                return ToLower(str.Pointer[index]);
            }

            return str.Pointer[index];
        }

        public void EmitElementAt(ILGenerator ilGen)
        {
            ilGen.Call(MethodOf<Ps<char>, int, char>(StringHelper.CharAt));

            if (IgnoreCase)
            {
                ilGen.Call(MethodOf<char, char>(ToLower));
            }
        }

        public void EmitEquals(ILGenerator ilGen)
        {
            if (IgnoreCase)
            {
                ilGen.Call(MethodOf<Ps<char>, Ps<char>, bool>(EqualsWithIgnoreCase));
            }
            else
            {
                ilGen.Call(MethodOf<Ps<char>, Ps<char>, bool>(StringHelper.Equals));
            }
        }

        public void EmitGetHashCode(ILGenerator ilGen)
        {
            if (IgnoreCase)
            {
                ilGen.Call(MethodOf<Ps<char>, int>(GetHashCodeWithIgnoreCase));
            }
            else
            {
                ilGen.Call(MethodOf<Ps<char>, int>(StringHelper.GetHashCode));
            }
        }

        public void EmitGetLength(ILGenerator ilGen)
        {
            ilGen.Call(MethodOf<Ps<char>, int>(StringHelper.GetLength));
        }

        public bool Equals(Ps<char> x, Ps<char> y)
        {
            if (IgnoreCase)
            {
                return EqualsWithIgnoreCase(x, y);
            }

            return StringHelper.Equals(x, y);
        }

        public int GetHashCode(Ps<char> str)
        {
            if (IgnoreCase)
            {
                return GetHashCodeWithIgnoreCase(str);
            }

            return StringHelper.GetHashCode(str);
        }

        public int GetLength(Ps<char> str)
        {
            return str.Length;
        }
    }
}