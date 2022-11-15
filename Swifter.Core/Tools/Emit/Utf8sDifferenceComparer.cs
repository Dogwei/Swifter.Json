using InlineIL;
using System;
using System.Reflection;
using System.Reflection.Emit;
using static Swifter.Tools.StringHelper;

namespace Swifter.Tools
{
    internal sealed unsafe class Utf8sDifferenceComparer : IDifferenceComparer<Ps<Utf8Byte>>, IHashComparer<Ps<Utf8Byte>>
    {
        public readonly bool IgnoreCase;

        public Utf8sDifferenceComparer(bool ignoreCase)
        {
            IgnoreCase = ignoreCase;
        }

        public void EmitElementAt(ILGenerator ilGen)
        {
            ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.CharAt), typeof(Ps<Utf8Byte>), typeof(int)))));

            if (IgnoreCase)
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.ToLower), typeof(Utf8Byte)))));
            }

            ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.AsByte), typeof(Utf8Byte)))));
        }

        public void EmitEquals(ILGenerator ilGen)
        {
            if (IgnoreCase)
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.EqualsWithIgnoreCase), typeof(Ps<Utf8Byte>), typeof(Ps<Utf8Byte>)))));
            }
            else
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.Equals), typeof(Ps<Utf8Byte>), typeof(Ps<Utf8Byte>)))));
            }
        }

        public void EmitGetLength(ILGenerator ilGen)
        {
            ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.GetLength), typeof(Ps<Utf8Byte>)))));
        }

        public void EmitGetHashCode(ILGenerator ilGen)
        {
            if (IgnoreCase)
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.GetHashCodeWithIgnoreCase), typeof(Ps<Utf8Byte>)))));
            }
            else
            {
                ilGen.AutoCall(TypeHelper.GetMethodFromHandle(IL.Ldtoken(MethodRef.Method(typeof(StringHelper), nameof(StringHelper.GetHashCode), typeof(Ps<Utf8Byte>)))));
            }
        }

        public int ElementAt(Ps<Utf8Byte> str, int index)
        {
            if (IgnoreCase)
            {
                return ToLower(str.Pointer[index]);
            }

            return str.Pointer[index];
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