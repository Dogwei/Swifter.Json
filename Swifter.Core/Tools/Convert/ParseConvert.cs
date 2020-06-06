using System;
using System.Linq;
using System.Reflection;

namespace Swifter.Tools
{
    internal sealed class ParseConvert : BaseDynamicConvert
    {
        public const BindingFlags ParseFlags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;

        public static readonly string[] ParseNames = typeof(ParseConvert).GetMethods(ParseFlags)
            .Where(item => item.ReturnType == typeof(ParseConvert) && OneParamsAndEqual(item, typeof(string)))
            .Select(item=>item.Name)
            .ToArray();

        public static bool TryGetMathod(Type tSource, Type tDestination, out MethodInfo method)
        {
            var types = new Type[] { tSource };

            foreach (var item in ParseNames)
            {
                method = tDestination.GetMethod(item, ParseFlags, Type.DefaultBinder, types, null);

                if (method is null)
                {
                    continue;
                }

                if (method.IsGenericMethodDefinition)
                {
                    var gArgs = method.GetGenericArguments();

                    if (gArgs.Length == 1 && method.ReturnType == gArgs[0])
                    {
                        method = method.MakeGenericMethod(tDestination);
                    }
                    else
                    {
                        method = null;
                    }
                }

                if (method != null)
                {
                    return true;
                }
            }

            method = null;

            return false;
        }

        public static ParseConvert Parse(string value) => default;

        public static ParseConvert ValueOf(string value) => default;
    }
}