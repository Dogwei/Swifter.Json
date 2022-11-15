using InlineIL;
using System;
using System.Reflection;
using System.Reflection.Emit;
using static Swifter.Tools.StringHelper;

namespace Swifter.Tools
{
    internal sealed unsafe class Utf16sDifferenceComparer : IDifferenceComparer<Ps<char>>, IHashComparer<Ps<char>>
    {
        public readonly bool IgnoreCase;

        public Utf16sDifferenceComparer(bool ignoreCase)
        {
            IgnoreCase = ignoreCase;
        }

        public void EmitElementAt(ILGenerator ilGen)
        {
            ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.CharAt), typeof(Ps<char>), typeof(int)))));

            if (IgnoreCase)
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.ToLower), typeof(char)))));
            }
        }

        public void EmitEquals(ILGenerator ilGen)
        {
            if (IgnoreCase)
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.EqualsWithIgnoreCase), typeof(Ps<char>), typeof(Ps<char>)))));
            }
            else
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.Equals), typeof(Ps<char>), typeof(Ps<char>)))));
            }
        }

        public void EmitGetLength(ILGenerator ilGen)
        {
            ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.GetLength), typeof(Ps<char>)))));
        }

        public void EmitGetHashCode(ILGenerator ilGen)
        {
            if (IgnoreCase)
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.GetHashCodeWithIgnoreCase), typeof(Ps<char>)))));
            }
            else
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.GetHashCode), typeof(Ps<char>)))));
            }
        }


        public int ElementAt(Ps<char> str, int index)
        {
            if (IgnoreCase)
            {
                return ToLower(str.Pointer[index]);
            }

            return str.Pointer[index];
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