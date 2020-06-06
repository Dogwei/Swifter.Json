using System;
using System.Linq;
using System.Reflection;

namespace Swifter.Tools
{
    internal sealed class ImplicitConvert : BaseDynamicConvert
    {
        public const BindingFlags ImplicitFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public static readonly string ImplicitName =
            typeof(ImplicitConvert).GetMethods(ImplicitFlags)
            .First(item => item.ReturnType == typeof(ImplicitConvert) && OneParamsAndEqual(item, typeof(int)))
            .Name;

        public static bool TryGetMathod(Type tSource, Type tDestination, out MethodInfo method)
        {
            if (tDestination.GetMethod(ImplicitName, ImplicitFlags, Type.DefaultBinder, new[] { tSource }, null) is MethodInfo methodInfo 
                && methodInfo.ReturnType == tDestination 
                && OneParamsAndEqual(methodInfo, tSource))
            {
                method = methodInfo;

                return true;
            }

            foreach (var item in tSource.GetMethods(ImplicitFlags))
            {
                if (item.Name == ImplicitName &&
                    item.ReturnType == tDestination &&
                    OneParamsAndEqual(item, tSource))
                {
                    method = item;

                    return true;
                }
            }

            method = null;

            return false;
        }

        public static implicit operator ImplicitConvert(int _) => default;
    }
}