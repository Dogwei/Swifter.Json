using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using static Swifter.Tools.MethodHelper;
using static Swifter.Tools.StringHelper;

namespace Swifter.Tools
{
    internal sealed unsafe class Utf8sDifferenceComparer : IDifferenceComparer<Ps<Utf8Byte>>
    {
        public readonly bool IgnoreCase;

        public Utf8sDifferenceComparer(bool ignoreCase)
        {
            IgnoreCase = ignoreCase;
        }

        public int ElementAt(Ps<Utf8Byte> str, int index)
        {
            if (IgnoreCase)
            {
                return ToLower(str.Pointer[index]);
            }

            return str.Pointer[index];
        }

        public void EmitElementAt(ILGenerator ilGen)
        {
            ilGen.Call(MethodOf<Ps<Utf8Byte>, int, Utf8Byte>(StringHelper.CharAt));

            if (IgnoreCase)
            {
                ilGen.Call(MethodOf<Utf8Byte, Utf8Byte>(ToLower));
            }

            ilGen.Call(MethodOf<Utf8Byte, byte>(AsByte));
        }

        public void EmitEquals(ILGenerator ilGen)
        {
            if (IgnoreCase)
            {
                ilGen.Call(MethodOf<Ps<Utf8Byte>, Ps<Utf8Byte>, bool>(EqualsWithIgnoreCase));
            }
            else
            {
                ilGen.Call(MethodOf<Ps<Utf8Byte>, Ps<Utf8Byte>, bool>(StringHelper.Equals));
            }
        }

        public void EmitGetHashCode(ILGenerator ilGen)
        {
            if (IgnoreCase)
            {
                ilGen.Call(MethodOf<Ps<Utf8Byte>, int>(GetHashCodeWithIgnoreCase));
            }
            else
            {
                ilGen.Call(MethodOf<Ps<Utf8Byte>, int>(StringHelper.GetHashCode));
            }
        }

        public void EmitGetLength(ILGenerator ilGen)
        {
            ilGen.Call(MethodOf<Ps<Utf8Byte>, int>(StringHelper.GetLength));
        }

        public bool Equals(Ps<Utf8Byte> x, Ps<Utf8Byte> y)
        {
            if (IgnoreCase)
            {
                return EqualsWithIgnoreCase(x, y);
            }

            return StringHelper.Equals(x, y);
        }

        public int GetHashCode(Ps<Utf8Byte> str)
        {
            if (IgnoreCase)
            {
                return GetHashCodeWithIgnoreCase(str);
            }

            return StringHelper.GetHashCode(str);
        }

        public int GetLength(Ps<Utf8Byte> str)
        {
            return str.Length;
        }
    }
}