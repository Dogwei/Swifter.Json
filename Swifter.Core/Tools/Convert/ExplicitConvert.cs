using System;
using System.Linq;
using System.Reflection;

namespace Swifter.Tools
{
    internal sealed class ExplicitConvert : BaseDynamicConvert
    {
        public const BindingFlags ExplicitFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public static readonly string ExplicitName =
            typeof(ExplicitConvert).GetMethods(ExplicitFlags)
            .First(item => item.ReturnType == typeof(ExplicitConvert) && OneParamsAndEqual(item, typeof(int)))
            .Name;

        public static bool TryGetMathod(Type tSource, Type tDestination, out MethodInfo method)
        {
            if (tDestination.GetMethod(ExplicitName, ExplicitFlags, Type.DefaultBinder, new Type[] { tSource }, null) is MethodInfo methodInfo 
                && methodInfo.ReturnType == tDestination 
                && OneParamsAndEqual(methodInfo, tSource))
            {
                method = methodInfo;

                return true;
            }

            foreach (var item in tSource.GetMethods(ExplicitFlags))
            {
                if (item.Name == ExplicitName &&
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

        public static explicit operator ExplicitConvert(int _) => default;
    }
}