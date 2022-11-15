using InlineIL;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Swifter.Tools
{
    internal sealed class StringDifferenceComparer : IDifferenceComparer<string>, IHashComparer<string>
    {
        public readonly bool IgnoreCase;

        public StringDifferenceComparer(bool ignoreCase)
        {
            IgnoreCase = ignoreCase;
        }

        public void EmitElementAt(ILGenerator ilGen)
        {
            ilGen.AutoCall(typeof(string).GetProperty(new Type[] { typeof(int) })!.GetGetMethod(true)!);

            if (IgnoreCase)
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.ToLower), typeof(char)))));
            }
        }

        public void EmitEquals(ILGenerator ilGen)
        {
            if (IgnoreCase)
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.EqualsWithIgnoreCase), typeof(string), typeof(string)))));
            }
            else
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.Equals), typeof(string), typeof(string)))));
            }
        }

        public void EmitGetLength(ILGenerator ilGen)
        {
            ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.PropertyGet(typeof(string), nameof(string.Length)))));
        }

        public void EmitGetHashCode(ILGenerator ilGen)
        {
            if (IgnoreCase)
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.GetHashCodeWithIgnoreCase), typeof(string)))));
            }
            else
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.GetHashCode), typeof(string)))));
            }
        }

        public int ElementAt(string str, int index)
        {
            if (IgnoreCase)
            {
                return StringHelper.ToLower(str[index]);
            }

            return str[index];
        }

        public bool Equals(string x, string y)
        {
            if (IgnoreCase)
            {
                return StringHelper.EqualsWithIgnoreCase(x, y);
            }

            return StringHelper.Equals(x, y);
        }

        public int GetLength(string str)
        {
            return str.Length;
        }

        public int GetHashCode(string str)
        {
            if (IgnoreCase)
            {
                return StringHelper.GetHashCodeWithIgnoreCase(str);
            }

            return StringHelper.GetHashCode(str);
        }
    }
}