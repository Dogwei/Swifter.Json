using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace Swifter.Tools
{
    static class THashAlgorithmInstances<THashAlgorithm> where THashAlgorithm : HashAlgorithm
    {
        [ThreadStatic]
        static THashAlgorithm instance;

        public static THashAlgorithm Instance => instance ?? Create();

        [MethodImpl(MethodImplOptions.NoInlining)]
        static THashAlgorithm Create()
        {
            var createMethod = typeof(THashAlgorithm).GetMethod(
                nameof(HashAlgorithm.Create),
                BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly,
                Type.DefaultBinder,
                Type.EmptyTypes,
                null);

            if (createMethod != null)
            {
                instance = (THashAlgorithm)createMethod.Invoke(null, new object[] { });
            }
            else
            {
                instance = Activator.CreateInstance<THashAlgorithm>();
            }

            return instance;
        }
    }
}